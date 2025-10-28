using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace EnergySavingAltTab
{
    internal class PowerLimiter : IDisposable
    {
        private bool engaged = false;
        private readonly Plugin plugin;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private ActivityDetector activityDetector;
        private readonly Stopwatch letOtherPluginWorkStopwatch = new Stopwatch();
        private const int GracePeriodForOtherPluginsMs = 30000;

        public PowerLimiter(Plugin plugin)
        {
            this.plugin = plugin;
            activityDetector = new ActivityDetector(plugin);
            Attach();
        }

        private void Attach()
        {
            Plugin.Framework.Update += OnUpdate;
        }

        public void OnUpdate(IFramework framework)
        {
            if (!plugin.Configuration.Enabled || IsGameWindowFocused())
            {
                DisengageLimiter();
                return;
            }
            if (IsAnotherPluginDoingStuff())
            {
                DisengageLimiter();
                return;
            }

            var delayInMs = GetFrameDelayFromConfigInMs();
            stopwatch.Restart();
            EngageLimiter();
            // Block the game, but check often if the window is focused to have fluent movement right after refocusing the window.
            while (stopwatch.ElapsedMilliseconds < delayInMs)
            {
                if (IsGameWindowFocused() || (plugin.Configuration.DisableWhenCrafting && IsOnFrameSensitiveActivity()))
                {
                    DisengageLimiter();
                    return;
                }
                Thread.Sleep(16);
            }
        }

        private bool IsAnotherPluginDoingStuff()
        {
            if (letOtherPluginWorkStopwatch.ElapsedMilliseconds > 0 && letOtherPluginWorkStopwatch.ElapsedMilliseconds < GracePeriodForOtherPluginsMs)
            {
                return true;
            }

            if (letOtherPluginWorkStopwatch.ElapsedMilliseconds > GracePeriodForOtherPluginsMs) { letOtherPluginWorkStopwatch.Reset(); }

            if (activityDetector.WasActivityDetected())
            {
                letOtherPluginWorkStopwatch.Restart();
                return true;
            }

            return false;
        }

        private void EngageLimiter()
        {
            if (!engaged)
            {
                Plugin.Log.Info("Unfocused window FPS limiter engaged");
            }
            engaged = true;
        }

        private void DisengageLimiter()
        {
            if (engaged)
            {
                Plugin.Log.Info("Unfocused window FPS limiter disengaged");
            }
            engaged = false;
        }

        private float GetFrameDelayFromConfigInMs()
        {
            return 10000f / plugin.Configuration.FramesPerTenSeconds;
        }

        private bool IsOnFrameSensitiveActivity()
        {
            return Plugin.Condition[ConditionFlag.Crafting];
        }

        private bool IsGameWindowFocused()
        {
            var activeHandle = GetForegroundWindow();
            if (activeHandle == IntPtr.Zero)
            {
                return false; // No window is focused
            }

            var gameProcessId = Process.GetCurrentProcess().Id;
            GetWindowThreadProcessId(activeHandle, out int activeProcessId);

            return activeProcessId == gameProcessId;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public void Dispose()
        {
            Plugin.Framework.Update -= OnUpdate;
        }
    }
}
