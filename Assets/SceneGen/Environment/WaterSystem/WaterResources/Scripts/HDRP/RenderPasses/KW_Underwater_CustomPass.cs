using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    class KW_Underwater_CustomPass : CustomPass
    {
        public WaterSystem WaterInstance;
        public bool IsInitialized;

        KWS_Underwater_CommandPass pass = new KWS_Underwater_CommandPass();

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            name = "Water.Underwater_Pass";
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var cam = ctx.hdCamera.camera;
            if (!IsUnderwaterVisible(cam, WaterInstance.WorldSpaceBounds)) return;

            var cmd = ctx.cmd;

            IsInitialized = true;
            pass.Initialize(WaterInstance);
            pass.SetColorBuffer(ctx.cameraColorBuffer);
            KWS_SPR_CoreUtils.SetRenderTarget(cmd, ctx.cameraColorBuffer);
            pass.Execute(cam, cmd, WaterInstance);
        }

        public void Release()
        {
            pass.Release();
            IsInitialized = false;
        }


        private Vector4[] nearPlane = new Vector4[4];

        bool IsUnderwaterVisible(Camera cam, Bounds waterBounds)
        {
            nearPlane[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            nearPlane[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
            nearPlane[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));
            nearPlane[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

            if (IsPointInsideAABB(nearPlane[0], waterBounds)
                || IsPointInsideAABB(nearPlane[1], waterBounds)
                || IsPointInsideAABB(nearPlane[2], waterBounds)
                || IsPointInsideAABB(nearPlane[3], waterBounds))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IsPointInsideAABB(Vector3 point, Bounds box)
        {
            return (point.x >= box.min.x && point.x <= box.max.x) &&
                   (point.y >= box.min.y && point.y <= box.max.y) &&
                   (point.z >= box.min.z && point.z <= box.max.z);
        }
    }
}