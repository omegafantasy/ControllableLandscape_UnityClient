using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace KWS
{
    public static partial class KWS_CoreUtils
    {
        static bool CanRenderWaterForCurrentCamera_PlatformSpecific(Camera cam)
        {
            return true;
        }

        public static Vector2 GetCameraRTHandleViewPortSize(Camera cam)
        {
            var rtHandleScale  = RTHandles.rtHandleProperties.rtHandleScale;
            var viewPort       = RTHandles.rtHandleProperties.currentRenderTargetSize;
            return new Vector2Int(Mathf.RoundToInt(rtHandleScale.x * viewPort.x), Mathf.RoundToInt(rtHandleScale.y * viewPort.y));
        }

    }
}