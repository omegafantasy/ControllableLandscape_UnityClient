using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    public class KWS_VolumetricLightRTing_CustomPass : CustomPass
    {
        public WaterSystem WaterInstance;
        public bool IsInitialized;

        KWS_VolumetricLighting_CommandPass pass = new KWS_VolumetricLighting_CommandPass();

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Water.VolumetricLighting_Pass";
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var cam = ctx.hdCamera.camera;

            var cmd = ctx.cmd;

            IsInitialized = true;
            pass.Initialize(WaterInstance);
            CoreUtils.SetRenderTarget(cmd, pass.GetTargetColorBuffer());
            pass.Execute(cam, cmd);
        }

        public void Release()
        {
            pass.Release();
            IsInitialized = false;
        }
    }
}