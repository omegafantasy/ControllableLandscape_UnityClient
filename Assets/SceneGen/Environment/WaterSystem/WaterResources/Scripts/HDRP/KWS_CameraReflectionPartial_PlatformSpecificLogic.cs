#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    public partial class KWS_CameraReflection
    {
        private HDAdditionalCameraData hdData;

#if UNITY_EDITOR

        Camera GetSceneCamera()
        {
            if (SceneView.lastActiveSceneView != null) return SceneView.lastActiveSceneView.camera;
            else
            {
                var camCurrent = Camera.current;
                if (camCurrent != null && camCurrent.cameraType == CameraType.SceneView) return camCurrent;
            }

            return null;
        }
#endif

        Camera GetCurrentCamera(Camera[] cameras)
        {
            if (Application.isPlaying)
            {
                foreach (var cam in cameras)
                {
                    if (cam != null && cam.cameraType == CameraType.Game) return cam;
                }
            }
            else
            {
#if UNITY_EDITOR
                return GetSceneCamera();
#endif
            }
            return cameras[0];
        }

        void SubscribeBeforeCameraRendering()
        {
            //RenderPipelineManager.beginCameraRendering += RenderPipelineManagerOnbeginCameraRendering;
            RenderPipelineManager.beginFrameRendering += RenderPipelineManagerOnbeginFrameRendering;
        }



        void UnsubscribeBeforeCameraRendering()
        {
            //RenderPipelineManager.beginCameraRendering -= RenderPipelineManagerOnbeginCameraRendering;
            RenderPipelineManager.beginFrameRendering -= RenderPipelineManagerOnbeginFrameRendering;
        }

        void CameraRender(Camera cam)
        {
            cam.Render();
        }

        private void RenderPipelineManagerOnbeginFrameRendering(ScriptableRenderContext ctx, Camera[] cameras)
        {
            // _allSceneCameras = cameras;
        }

        void Update()
        {
            var currentCamera = GetCurrentCamera(Camera.allCameras);
            if (currentCamera == null) return;
            OnBeforeCameraRendering(currentCamera);
        }

        void SubscribeAfterCameraRendering()
        {
            RenderPipelineManager.endCameraRendering += RenderPipelineManagerOnendCameraRendering;
        }

        void UnsubscribeAfterCameraRendering()
        {
            RenderPipelineManager.endCameraRendering -= RenderPipelineManagerOnendCameraRendering;
        }

        private void RenderPipelineManagerOnendCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            OnAfterCameraRendering(cam);
        }

        void InitializeCameraParamsSRP()
        {
            if (hdData == null) hdData = _reflCameraGo.AddComponent<HDAdditionalCameraData>();
            hdData.defaultFrameSettings = FrameSettingsRenderType.RealtimeReflection;
            hdData.customRenderingSettings = true;

            hdData.hasPersistentHistory = true;
            hdData.invertFaceCulling = true;
            hdData.volumeLayerMask = ~0;
            hdData.SetCameraFrameSetting(FrameSettingsField.AtmosphericScattering, false);
        }


        void CopyCameraParamsSRP(Camera currentCamera, int cullingMask, bool invertFaceCulling)
        {
            if (hdData == null) hdData = _reflCameraGo.AddComponent<HDAdditionalCameraData>();
            hdData.invertFaceCulling    = invertFaceCulling;
            
        }
    }
}