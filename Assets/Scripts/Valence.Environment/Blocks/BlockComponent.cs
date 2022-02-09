using System.Collections.Generic;
using UnityEngine;

namespace Valence.Environment
{
    public abstract class BlockAsset : ScriptableObject
    {
        [Range(0.0f, 1.0f)]
        public int hardness;
    }

    [CreateAssetMenu(fileName = nameof(BlockAsset), menuName = "Block/Create Block Cube Asset")]
    public class BlockCubeAsset : BlockAsset
    {
        /// <summary>
        /// right - left - top - bottom - front - back
        /// </summary>
        public Sprite[] atlas = new Sprite[6];
    }

    [CreateAssetMenu(fileName = nameof(BlockAsset), menuName = "Block/Create Block Foliage Asset")]
    public class BlockFoliageAsset : BlockAsset
    {
        /// <summary>
        /// right - front
        /// </summary>
        public Sprite[] atlas = new Sprite[2];
    }

    [CreateAssetMenu(fileName = nameof(BlockAsset), menuName = "Block/Create Block Mesh Asset")]
    public class BlockMeshAsset : BlockAsset
    {
        public Mesh mesh;
    }

    [CreateAssetMenu(fileName = nameof(BlockComponent), menuName = "Block/Create Block Component")]
    public class BlockComponent : ScriptableObject
    {
        public List<BlockAsset> blocks = new List<BlockAsset>();
    }
}