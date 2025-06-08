using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorMaskFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class ColorMaskSettings
    {
        public Shader shader;
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public ColorMaskSettings settings = new ColorMaskSettings();
    ColorMaskPass pass;

    public override void Create()
    {
        if (settings.material == null && settings.shader != null)
            settings.material = new Material(settings.shader);

        pass = new ColorMaskPass(settings.material)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public void SetRevealCenters(Vector4[] centers, float[] radii)
    {
        pass?.SetCenters(centers, radii);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material != null)
            renderer.EnqueuePass(pass);
    }

    class ColorMaskPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle cameraColorTarget;

        public ColorMaskPass(Material material)
        {
            this.material = material;
        }

        public void SetCenters(Vector4[] centers, float[] radii)
        {
            int count = Mathf.Min(centers.Length, radii.Length, 32);
            material.SetInt("_CenterCount", count);
            material.SetVectorArray("_Centers", centers);
            material.SetFloatArray("_Radii", radii);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
                return;
            if (material == null) return;
            CommandBuffer cmd = CommandBufferPool.Get("ColorMaskPass");

            RenderTargetIdentifier source = cameraColorTarget;
            int tempRTID = Shader.PropertyToID("_TempColorMaskRT");
            cmd.GetTemporaryRT(tempRTID, renderingData.cameraData.cameraTargetDescriptor);

            cmd.Blit(source, tempRTID);
            cmd.Blit(tempRTID, source, material);

            cmd.ReleaseTemporaryRT(tempRTID);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
    }
}