using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Valence.Environment
{
    public class SkyboxLutBakePass : ScriptableRenderPass
    {
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var desc = new RenderTextureDescriptor
            {
                enableRandomWrite = true,
                colorFormat = RenderTextureFormat.RGB111110Float,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D,
                volumeDepth = 1
            };
            
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    public class SkyboxRenderPass : ScriptableRenderPass
    {
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }
}