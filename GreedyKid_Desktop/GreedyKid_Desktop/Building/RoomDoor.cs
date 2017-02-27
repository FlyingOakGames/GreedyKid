using System.IO;

namespace GreedyKid
{
    public enum RoomDoorState
    {
        Closed,
        OpeningToLeft,
        OpenLeft,
        OpeningToRight,
        OpenRight,
        ClosingFromLeft,
        ClosingFromRight,

        Count
    }

    public sealed class RoomDoor
    {
        public const int DoorFrames = 18;
        public const int FramePerLine = 18;

        public int X = 0;

        private const float _frameTime = 0.1f;
        private RoomDoorState _state = RoomDoorState.Closed;
        public int Frame = 0;
        private float _currentFrameTime = 0.0f;

        public bool CanClose = false;

        public bool IsOpenLeft
        {
            get { return _state == RoomDoorState.OpenLeft; }
        }

        public bool IsOpenRight
        {
            get { return _state == RoomDoorState.OpenRight; }
        }

        public bool IsClosed
        {
            get { return _state == RoomDoorState.Closed; }
        }            

        public bool IsClosingFromLeft
        {
            get { System.Console.WriteLine(_state);  return _state == RoomDoorState.ClosingFromLeft; }
        }

        public bool IsClosingFromRight
        {
            get { return _state == RoomDoorState.ClosingFromRight; }
        }

        public void Load(BinaryReader reader)
        {
            X = reader.ReadInt32();
        }

        public void Update(float gameTime)
        {
            if (_state != RoomDoorState.Closed && _state != RoomDoorState.OpenLeft && _state != RoomDoorState.OpenRight)
            {
                _currentFrameTime += gameTime;
                if (_currentFrameTime > _frameTime)
                {
                    _currentFrameTime -= _frameTime;
                    Frame++;

                    if (_state == RoomDoorState.OpeningToLeft && Frame == 13)
                    {
                        _state = RoomDoorState.OpenLeft;
                    }
                    else if (_state == RoomDoorState.OpeningToRight && Frame == 4)
                    {
                        _state = RoomDoorState.OpenRight;
                    }
                    else if (_state == RoomDoorState.ClosingFromLeft && Frame == 17)
                    {
                        _state = RoomDoorState.Closed;
                    }
                    else if (_state == RoomDoorState.ClosingFromRight && Frame == 8)
                    {
                        _state = RoomDoorState.Closed;
                    }
                }
            }
        }

        public void Reset()
        {
            Frame = 0;
            _state = RoomDoorState.Closed;
        }

        public void OpenLeft()
        {

            if (_state == RoomDoorState.Closed)
            {
                _state = RoomDoorState.OpeningToLeft;
                Frame = 9;
                _currentFrameTime = 0.0f;
            }
        }

        public void OpenRight()
        {
            if (_state == RoomDoorState.Closed)
            {
                _state = RoomDoorState.OpeningToRight;
                Frame = 0;
                _currentFrameTime = 0.0f;
            }
        }

        public void CheckCanCloseFromLeft()
        {
            if (_state == RoomDoorState.OpenLeft)
            {
                CanClose = true;
            }
        }

        public void CheckCanCloseFromRight()
        {
            if (_state == RoomDoorState.OpenRight)
            {
                CanClose = true;
            }
        }

        public void Close()
        {
            if (_state == RoomDoorState.OpenRight)
            {
                _state = RoomDoorState.ClosingFromRight;
                Frame = 4;
                _currentFrameTime = 0.0f;
                CanClose = false;
            }
            else if (_state == RoomDoorState.OpenLeft)
            {
                _state = RoomDoorState.ClosingFromLeft;
                Frame = 13;
                _currentFrameTime = 0.0f;
                CanClose = false;
            }
        }
    }
}
