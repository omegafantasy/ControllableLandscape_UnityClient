using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    class KW_DrawToDepth_CustomPass : CustomPass
    {
        public WaterSystem WaterInstance;
        public bool IsInitialized;
        KWS_DrawToDepth_CommandPass pass = new KWS_DrawToDepth_CommandPass();

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Water.DrawToDepth_Pass";
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var cam = ctx.hdCamera.camera;
            var cmd = ctx.cmd;

            IsInitialized = true;
            pass.Initialize(WaterInstance, ctx.cameraDepthBuffer);
            pass.Execute(cam, cmd);
        }

        public void Release()
        {
            pass.Release();
            IsInitialized = false;
        }
    }
}
