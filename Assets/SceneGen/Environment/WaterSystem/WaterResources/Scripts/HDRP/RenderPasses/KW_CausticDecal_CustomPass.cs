using KWS;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{

    public class KW_CausticDecal_CustomPass : CustomPass
    {
        public WaterSystem WaterInstance;
        public bool IsInitialized;
        KWS_Caustic_CommandPass pass = new KWS_Caustic_CommandPass();

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Water.Caustic_Pass";
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var cam = ctx.hdCamera.camera;
            var cmd = ctx.cmd;

            IsInitialized = true;
            pass.Initialize(WaterInstance, ctx.cameraColorBuffer);
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
