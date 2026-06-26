using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class Crunch : ScriptableRendererFeature
{
    class DSColor
    {
        public TextureHandle src;
        public TextureHandle dst;
    }

    static readonly int kColorID = Shader.PropertyToID("_LowRedColor");

    CrunchPass _pass;

    public override void Create()
    {
        _pass = new CrunchPass();
        _pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Debug.Log("RecordRenderGraph called");
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(_pass);
        }
    }

    class CrunchPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
        {
            var res = frameData.Get<UniversalResourceData>();

            var desc = new RenderTextureDescriptor(Screen.width / 16, Screen.height / 16);
            TextureHandle lowColorRT = UniversalRenderer.CreateRenderGraphTexture(rg, desc, "LowColorRT", false);

            using (var dsBuilder = rg.AddRasterRenderPass<DSColor>("Pixelate - Colour Downsample", out var ds))
            {
                dsBuilder.AllowPassCulling(false);

                ds.src = res.activeColorTexture;
                ds.dst = lowColorRT;

                dsBuilder.UseTexture(ds.src, AccessFlags.Read);
                dsBuilder.SetRenderAttachment(ds.dst, 0, AccessFlags.Write);

                dsBuilder.SetRenderFunc((DSColor data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1f, 1f, 0f, 0f), 0f, false);
                });

                dsBuilder.SetGlobalTextureAfterPass(lowColorRT, kColorID);
            }

            using (var usBuilder = rg.AddRasterRenderPass<DSColor>("Pixelate - Upsample", out var us))
            {
                usBuilder.AllowPassCulling(false);

                us.src = lowColorRT;
                us.dst = res.activeColorTexture;

                usBuilder.UseTexture(us.src, AccessFlags.Read);
                usBuilder.SetRenderAttachment(us.dst, 0, AccessFlags.Write);

                usBuilder.SetRenderFunc((DSColor data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1f, 1f, 0f, 0f), 0f, false);
                });
            }
        }
    }
}