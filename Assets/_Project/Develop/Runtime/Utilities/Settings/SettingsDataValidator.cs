using System;
using _Project.Develop.Runtime.Utilities.DataManagement;
using UnityEngine;

namespace _Project.Develop.Runtime.Utilities.Settings
{
    public static class SettingsDataValidator
    {
        public static bool IsValid(SettingsData settings)
        {
            return settings != null
                   && settings.ResolutionWidth > 0
                   && settings.ResolutionHeight > 0
                   && Enum.IsDefined(
                       typeof(FullScreenMode), settings.ScreenMode)
                   && Enum.IsDefined(
                       typeof(FrameRateLimit), settings.FrameRateLimit)
                   && IsFinite(settings.MasterVolume)
                   && IsFinite(settings.MusicVolume)
                   && IsFinite(settings.SfxVolume)
                   && IsFinite(settings.MouseSensitivity);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value)
                   && !float.IsInfinity(value);
        }
    }
}