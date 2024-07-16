using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    public static class KW_ExtensionsHDRP
    {
        public static void SetCameraFrameSetting(this Camera cam, FrameSettingsField setting, bool enabled)
        {
            var cameraData = cam.GetComponent<HDAdditionalCameraData>();
            SetCameraFrameSetting(cameraData, setting, enabled);
        }

        public static void SetCameraFrameSetting(this HDAdditionalCameraData cameraData, FrameSettingsField setting, bool enabled)
        {
            var frameSettings = cameraData.renderingPathCustomFrameSettings;
            var frameSettingsOverrideMask = cameraData.renderingPathCustomFrameSettingsOverrideMask;

            frameSettingsOverrideMask.mask[(uint)setting] = true;
            frameSettings.SetEnabled(setting, enabled);

            cameraData.renderingPathCustomFrameSettings = frameSettings;
            cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
        }
    }
}