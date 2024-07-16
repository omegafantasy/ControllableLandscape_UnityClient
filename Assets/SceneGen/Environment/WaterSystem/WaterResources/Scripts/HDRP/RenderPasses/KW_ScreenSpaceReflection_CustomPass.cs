using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    public class KW_ScreenSpaceReflection_CustomPass : CustomPass
    {
        public WaterSystem WaterInstance;
        public bool IsInitialized;
        KWS_SSR_CommandPass pass = new KWS_SSR_CommandPass();

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Water.SSR_Pass";
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var cam = ctx.hdCamera.camera;
            var cmd = ctx.cmd;

            IsInitialized = true;
            pass.Initialize(WaterInstance, ctx.cameraDepthBuffer);
            KWS_SPR_CoreUtils.SetRenderTarget(cmd, pass.GetTargetColorBuffer());
            pass.Execute(cam, cmd);
        }

        public void Release()
        {
            pass.Release();
            IsInitialized = false;
        }
    }
}