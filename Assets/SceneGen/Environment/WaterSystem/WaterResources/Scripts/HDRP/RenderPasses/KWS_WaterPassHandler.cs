using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    [ExecuteAlways]
    public class KWS_WaterPassHandler : MonoBehaviour
    {
        public WaterSystem WaterInstance;

        CustomPassVolume volumeAfterOpaque;
        CustomPassVolume volumeBeforePreRefraction;
        CustomPassVolume volumeBeforeTransparent;
        CustomPassVolume volumeBeforePostProcess;

        GameObject volumeAfterOpaqueGO;
        GameObject volumeBeforePreRefractionGO;
        GameObject volumeBeforeTransparentGO;
        GameObject volumeBeforePostProcessGO;

        KW_MaskDepthNormalCustomPass maskDepthNormalCustomPass;
        KWS_VolumetricLightRTing_CustomPass volumetricLightingCustomPass;
        KW_CausticDecal_CustomPass causticDecalCustomPass;
        KW_ScreenSpaceReflection_CustomPass screenSpaceReflectionCustomPass;
        KW_Underwater_CustomPass underwaterCustomPass;
        KW_DrawToDepth_CustomPass drawToDepthCustomPass;


        public void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            Initialize();
        }

        public void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            KW_Extensions.SafeDestroy(volumeAfterOpaqueGO, volumeBeforePreRefractionGO, volumeBeforeTransparentGO, volumeBeforePostProcessGO);

            if (maskDepthNormalCustomPass != null && maskDepthNormalCustomPass.IsInitialized) maskDepthNormalCustomPass.Release();
            if (underwaterCustomPass != null && underwaterCustomPass.IsInitialized) underwaterCustomPass.Release();
            if (volumetricLightingCustomPass != null && volumetricLightingCustomPass.IsInitialized) volumetricLightingCustomPass.Release();
            if (causticDecalCustomPass != null && causticDecalCustomPass.IsInitialized) causticDecalCustomPass.Release();
            if (screenSpaceReflectionCustomPass != null && screenSpaceReflectionCustomPass.IsInitialized) screenSpaceReflectionCustomPass.Release();
            if (drawToDepthCustomPass != null && drawToDepthCustomPass.IsInitialized) drawToDepthCustomPass.Release();
        }

        private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            if (!WaterInstance.IsWaterVisible || !KWS_CoreUtils.CanRenderWaterForCurrentCamera(cam))
            {
                DisableAllPasses();
                return;
            }
            else UpdateWaterPasses(cam);
        }

        void DisableAllPasses()
        {
            if (maskDepthNormalCustomPass != null) maskDepthNormalCustomPass.enabled = false;
            if (volumetricLightingCustomPass != null) volumetricLightingCustomPass.enabled = false;
            if (causticDecalCustomPass != null) causticDecalCustomPass.enabled = false;
            if (screenSpaceReflectionCustomPass != null) screenSpaceReflectionCustomPass.enabled = false;
            if (underwaterCustomPass != null) underwaterCustomPass.enabled = false;
            if (drawToDepthCustomPass != null) drawToDepthCustomPass.enabled = false;
        }

        void Initialize()
        {
            if (volumeAfterOpaque == null)
            {
                volumeAfterOpaqueGO = new GameObject("HDRP_WaterVolume_AfterOpaque") { hideFlags = HideFlags.DontSave };
                volumeAfterOpaqueGO.transform.parent = transform;
                volumeAfterOpaque = volumeAfterOpaqueGO.AddComponent<CustomPassVolume>();
                volumeAfterOpaque.injectionPoint = CustomPassInjectionPoint.AfterOpaqueDepthAndNormal;
            }

            if (volumeBeforePreRefraction == null)
            {
                volumeBeforePreRefractionGO = new GameObject("HDRP_WaterVolume_BeforePreRefraction") { hideFlags = HideFlags.DontSave };
                volumeBeforePreRefractionGO.transform.parent = transform;
                volumeBeforePreRefraction = volumeBeforePreRefractionGO.AddComponent<CustomPassVolume>();
                volumeBeforePreRefraction.injectionPoint = CustomPassInjectionPoint.BeforePreRefraction;
            }

            if (volumeBeforeTransparent == null)
            {
                volumeBeforeTransparentGO = new GameObject("HDRP_WaterVolume_BeforeTransparent") { hideFlags = HideFlags.DontSave };
                volumeBeforeTransparentGO.transform.parent = transform;
                volumeBeforeTransparent = volumeBeforeTransparentGO.AddComponent<CustomPassVolume>();
                volumeBeforeTransparent.injectionPoint = CustomPassInjectionPoint.BeforeTransparent;
            }

            if (volumeBeforePostProcess == null)
            {
                volumeBeforePostProcessGO = new GameObject("HDRP_WaterVolume_BeforePostProcess") { hideFlags = HideFlags.DontSave };
                volumeBeforePostProcessGO.transform.parent = transform;
                volumeBeforePostProcess = volumeBeforePostProcessGO.AddComponent<CustomPassVolume>();
                volumeBeforePostProcess.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
            }
        }

        void UpdateWaterPasses(Camera cam)
        {
            var cameraSize = KWS_CoreUtils.GetCameraScreenSizeLimited(cam);
            KWS_RTHandles.SetReferenceSize(cameraSize.x, cameraSize.y, KWS.MSAASamples.None);

            if ((WaterInstance.UseVolumetricLight || WaterInstance.UseCausticEffect || WaterInstance.UseUnderwaterEffect) && maskDepthNormalCustomPass == null)
            {
                maskDepthNormalCustomPass = (KW_MaskDepthNormalCustomPass)volumeAfterOpaque.AddPassOfType<KW_MaskDepthNormalCustomPass>();
                maskDepthNormalCustomPass.WaterInstance = WaterInstance;
            }
            if (maskDepthNormalCustomPass != null) maskDepthNormalCustomPass.enabled = (WaterInstance.UseVolumetricLight || WaterInstance.UseCausticEffect || WaterInstance.UseUnderwaterEffect);


            if (WaterInstance.UseVolumetricLight && volumetricLightingCustomPass == null)
            {
                volumetricLightingCustomPass = (KWS_VolumetricLightRTing_CustomPass)volumeBeforeTransparent.AddPassOfType<KWS_VolumetricLightRTing_CustomPass>();
                volumetricLightingCustomPass.WaterInstance = WaterInstance;
            }
            if (volumetricLightingCustomPass != null) volumetricLightingCustomPass.enabled = WaterInstance.UseVolumetricLight;


            if (WaterInstance.UseCausticEffect && causticDecalCustomPass == null)
            {
                causticDecalCustomPass = (KW_CausticDecal_CustomPass)volumeBeforePreRefraction.AddPassOfType<KW_CausticDecal_CustomPass>();
                causticDecalCustomPass.WaterInstance = WaterInstance;
            }
            if (causticDecalCustomPass != null) causticDecalCustomPass.enabled = WaterInstance.UseCausticEffect;


            if (WaterInstance.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection && screenSpaceReflectionCustomPass == null)
            {
                screenSpaceReflectionCustomPass = (KW_ScreenSpaceReflection_CustomPass)volumeBeforeTransparent.AddPassOfType<KW_ScreenSpaceReflection_CustomPass>();
                screenSpaceReflectionCustomPass.WaterInstance = WaterInstance;
            }
            if (screenSpaceReflectionCustomPass != null) screenSpaceReflectionCustomPass.enabled = (WaterInstance.ReflectionMode == WaterSystem.ReflectionModeEnum.ScreenSpaceReflection);


            if (WaterInstance.UseUnderwaterEffect && underwaterCustomPass == null)
            {
                underwaterCustomPass = (KW_Underwater_CustomPass)volumeBeforePostProcess.AddPassOfType<KW_Underwater_CustomPass>();
                underwaterCustomPass.WaterInstance = WaterInstance;
            }
            if (underwaterCustomPass != null) underwaterCustomPass.enabled = WaterInstance.UseUnderwaterEffect;

            if (WaterInstance.DrawToPosteffectsDepth && drawToDepthCustomPass == null)
            {
                drawToDepthCustomPass = (KW_DrawToDepth_CustomPass)volumeBeforePostProcess.AddPassOfType<KW_DrawToDepth_CustomPass>();
                drawToDepthCustomPass.WaterInstance = WaterInstance;
            }
            if (drawToDepthCustomPass != null) drawToDepthCustomPass.enabled = WaterInstance.DrawToPosteffectsDepth;
        }

    }
}