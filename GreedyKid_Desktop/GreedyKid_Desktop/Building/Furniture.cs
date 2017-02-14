using System.IO;

namespace GreedyKid
{
    public enum FurniteState
    {
        None,
        Hiding,
        Showing,

        Count
    }

    public sealed class Furniture
    {
        public const int FurnitureCount = 8;
        public const int FurnitureFrames = 14;
        public const int FurniturePerLine = 4;

        public int Type = 0;
        public int X = 0;

        private const float _frameTime = 0.1f;
        private FurniteState _state = FurniteState.None;
        public int Frame = 0;
        private float _currentFrameTime = 0.0f;

        public bool CanHide = false;

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }

        public void Update(float gameTime)
        {
            if (_state != FurniteState.None)
            {
                _currentFrameTime += gameTime;
                if (_currentFrameTime > _frameTime)
                {
                    _currentFrameTime -= _frameTime;
                    Frame++;

                    if (_state == FurniteState.Hiding && Frame == 6)
                    {
                        Frame = 5;
                    }
                    else if (_state == FurniteState.Showing && Frame == 10)
                    {
                        Frame = 0;
                        _state = FurniteState.None;
                    }
                }
            }
        }

        public void CheckCanHide()
        {
            if (_state == FurniteState.None)
            {
                CanHide = true;
            }
        }

        public bool CheckCanShow()
        {
            if (_state == FurniteState.Hiding && Frame == 5)
                return true;
            return false;
        }

        public void Hide()
        {
            if (_state == FurniteState.None)
            {
                CanHide = false;
                _state = FurniteState.Hiding;
                Frame = 0;
                _currentFrameTime = 0.0f;
            }
        }

        public void Show()
        {
            if (_state == FurniteState.Hiding)
            {
                CanHide = false;
                _state = FurniteState.Showing;
                Frame = 5;
                _currentFrameTime = 0.0f;
            }
        }
    }
}
