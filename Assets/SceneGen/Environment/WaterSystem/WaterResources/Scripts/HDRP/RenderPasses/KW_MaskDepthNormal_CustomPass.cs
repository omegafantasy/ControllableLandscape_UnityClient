using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    class KW_MaskDepthNormalCustomPass : CustomPass
    {
        public WaterSystem WaterInstance;
        public bool IsInitialized;
        KWS_MaskDepthNormal_CommandPass pass = new KWS_MaskDepthNormal_CommandPass();

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Water.MaskDepthNormal_Pass";
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var cam = ctx.hdCamera.camera;
            var cmd = ctx.cmd;

            IsInitialized = true;
            pass.Initialize(WaterInstance);
            KWS_SPR_CoreUtils.SetRenderTarget(cmd, pass.GetTargetColorBuffer(), pass.GetTargetDepthBuffer());
            pass.Execute(cam, cmd);

        }

        public void Release()
        {
            pass.Release();
            IsInitialized = false;
        }
    }
}
