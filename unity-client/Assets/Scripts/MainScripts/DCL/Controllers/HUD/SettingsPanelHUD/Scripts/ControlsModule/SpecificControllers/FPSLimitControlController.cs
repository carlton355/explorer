using DCL.SettingsPanelHUD.Common;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.fpsCap;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.fpsCap = (bool)newValue;
            ToggleFPSCap(currentQualitySetting.fpsCap);
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] public static extern void ToggleFPSCap(bool useFPSCap);
#else
        public static void ToggleFPSCap(bool useFPSCap)
        {
        }
#endif
    }
}