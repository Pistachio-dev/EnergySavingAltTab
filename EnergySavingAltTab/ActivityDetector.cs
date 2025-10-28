using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Numerics;

namespace EnergySavingAltTab
{
    internal unsafe class ActivityDetector
    {
        private readonly Plugin plugin;
        private IGameObject? lastTarget;
        private Vector3 lastPlayerPosition = Vector3.Zero;
        private bool[] lastAddonOpenedFlags = new bool[4];

        public ActivityDetector(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void RefreshState()
        {
            lastAddonOpenedFlags = GetAddonsOpenedFlags();
            lastPlayerPosition = GetPlayerPosition();
            lastTarget = GetPlayerTarget();
        }

        public bool WasActivityDetected()
        {
            var addonFlags = GetAddonsOpenedFlags();
            var target = GetPlayerTarget();
            var currentPos = GetPlayerPosition();

            bool changeDetected = CheckForDifferences(addonFlags, target, currentPos);

            addonFlags.CopyTo(lastAddonOpenedFlags, 0);
            lastTarget = target;

            return changeDetected;
        }

        private bool CheckForDifferences(bool[] currentAddonFlags, IGameObject? currentTarget, Vector3 currentPos)
        {
            for (int i = 0; i < lastAddonOpenedFlags.Length; i++)
            {
                if (currentAddonFlags[i] != lastAddonOpenedFlags[i])
                {
                    Plugin.Log.Info("Addon change detected");
                    return true;
                }
            }

            var targetId = currentTarget?.TargetObjectId ?? 0;
            var previousTargetId = lastTarget?.TargetObjectId ?? 0;
            if (targetId != previousTargetId)
            {
                Plugin.Log.Info("Target change detected");
                return true;
            }

            if (Math.Abs(Vector3.Distance(currentPos, lastPlayerPosition)) > 0.1)
            {
                Plugin.Log.Info("Position change detected");
                lastPlayerPosition = currentPos;
                return true;
            }

            return false;
        }

        private bool[] GetAddonsOpenedFlags()
        {
            bool[] flags = [false, false, false, false];
            flags[0] = AddonVisible<AddonSelectString>("SelectString");
            flags[1] = AddonVisible<AtkUnitBase>("SelectYesno");
            flags[2] = AddonVisible<AtkUnitBase>("RetainerList");
            flags[3] = AddonVisible<AddonTalk>("Talk");

            return flags;
        }

        private void SetFlag(bool[] flags, int digit, bool value)
        {
            flags[digit] = value;
        }

        public static bool AddonVisible<T>(string Addon) where T : unmanaged
        {
            var a = Plugin.GameGui.GetAddonByName<T>(Addon);
            if (a == default(T*))
            {
                return false;
            }

            return ((AtkUnitBasePtr)a).IsVisible;
        }

        private Vector3 GetPlayerPosition()
        {
            return Plugin.ClientState.LocalPlayer?.Position ?? Vector3.Zero;
        }
        private IGameObject? GetPlayerTarget()
        {
            return Plugin.TargetManager.Target;
        }
    }
}
