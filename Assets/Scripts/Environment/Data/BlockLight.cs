// ReSharper disable CompareOfFloatsByEqualityOperator
namespace Environment.Data
{
    public unsafe struct BlockLight
    {
        public fixed float ambient[24];
        public bool CompareFace(BlockLight other, int direction)
        {
            for (int i = 0; i < 4; i++)
                if (ambient[direction * 4 + i] != other.ambient[direction * 4 + i])
                    return false;

            return true;
        }
    }
}