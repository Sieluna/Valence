using System.Runtime.InteropServices;

namespace Environment
{
    [StructLayout(LayoutKind.Sequential, Size=1)]
    public struct Block
    {
        public BlockType type;
        
        public Block(BlockType type) => this.type = type;

        public static Block Empty => new Block(BlockType.Air);
    }

    public enum BlockType : byte
    {
        Air, Dirt, GrassDirt, Stone, Sand, SandStone, Bedrock,
        CoalOre, IronOre, GoldOre, DiamondOre, Water,
        Glowstone, OakLog, OakPlanks, OakLeaves, Foliage, Grass
    }

    public enum BlockShape : byte
    {
        Empty, Block, Transparent, Foliage, Liquid
    }

}