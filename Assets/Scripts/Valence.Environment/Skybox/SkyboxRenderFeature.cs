using UnityEngine.Rendering.Universal;

namespace Valence.Environment
{
    public class SkyboxRenderFeature : ScriptableRendererFeature
    {
        private SkyboxLutBakePass m_LutBakePass;
        private SkyboxRenderPass m_RenderPass;

        public override void Create()
        {
            m_LutBakePass = new SkyboxLutBakePass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques
            };

            m_RenderPass = new SkyboxRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingSkybox
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_LutBakePass);
            renderer.EnqueuePass(m_RenderPass);
        }
    }
}


