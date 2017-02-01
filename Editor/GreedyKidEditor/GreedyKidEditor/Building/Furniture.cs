
namespace GreedyKidEditor
{
    public sealed class Furniture
    {
        public const int FurnitureCount = 3;
        public const int FurnitureFrames = 10;
        public const int FurniturePerLine = 6;

        public int Type = 0;
        public int X = 0;

        public Furniture(int x)
        {
            X = x;
        }
    }
}
