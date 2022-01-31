using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogScatteringFeature : ScriptableRendererFeature
{
    public Material blitMaterial = null;
    public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
    private FogScatteringPass m_FogScatteringPass;

    public override void Create()
    {
        m_FogScatteringPass = new FogScatteringPass(name);
        m_FogScatteringPass.blitMaterial = blitMaterial;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        m_FogScatteringPass.renderPassEvent = renderPassEvent;
        renderer.EnqueuePass(m_FogScatteringPass);
    }

    private class FogScatteringPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public FilterMode filterMode { get; set; }
        RenderTargetIdentifier source;
        RenderTargetIdentifier destination;
        int temporaryRTId = Shader.PropertyToID("_TempRT");
        int sourceId;
        int destinationId;
        string m_ProfilerTag;

        private Camera m_camera;
        private Vector3[] m_frustumCorners = new Vector3[4];
        private Transform m_cameraTransform = null;
        private Rect m_rect = new Rect(0, 0, 1, 1);
        private Matrix4x4 m_frustumCornersArray;

        public FogScatteringPass(string tag)
        {
            m_ProfilerTag = tag;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            blitTargetDescriptor.depthBufferBits = 0;
            var renderer = renderingData.cameraData.renderer;

            sourceId = -1;
            source = renderer.cameraColorTarget;
            destinationId = temporaryRTId;
            cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
            destination = new RenderTargetIdentifier(destinationId);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            m_camera = renderingData.cameraData.camera;
            m_cameraTransform = m_camera.transform;

            m_camera.CalculateFrustumCorners(m_rect, m_camera.farClipPlane, m_camera.stereoActiveEye, m_frustumCorners);
            m_frustumCornersArray = Matrix4x4.identity;
            m_frustumCornersArray.SetRow(0, m_cameraTransform.TransformVector(m_frustumCorners[0]));  // bottom left
            m_frustumCornersArray.SetRow(2, m_cameraTransform.TransformVector(m_frustumCorners[1]));  // top left
            m_frustumCornersArray.SetRow(3, m_cameraTransform.TransformVector(m_frustumCorners[2]));  // top right
            m_frustumCornersArray.SetRow(1, m_cameraTransform.TransformVector(m_frustumCorners[3]));  // bottom right
            blitMaterial.SetMatrix("_FrustumCorners", m_frustumCornersArray);

            Blit(cmd, source, destination, blitMaterial, -1);
            Blit(cmd, destination, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destinationId != -1)
                cmd.ReleaseTemporaryRT(destinationId);

            if (source == destination && sourceId != -1)
                cmd.ReleaseTemporaryRT(sourceId);
        }
    }
}