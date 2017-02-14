using System.IO;

namespace GreedyKid
{
    public enum FloorDoorState
    {
        Closed,
        Entering,
        Exiting,

        Count
    }

    public sealed class FloorDoor
    {
        public const int DoorCount = 5;
        public const int DoorPerLine = 6;

        public const int DoorFrames = 6;

        public int Color = 0;
        public int X = 0;

        private const float _frameTime = 0.1f;
        private FloorDoorState _state = FloorDoorState.Closed;
        public int Frame = 3;
        private float _currentFrameTime = 0.0f;

        public bool CanOpen = false;

        public Room Room = null;
        public FloorDoor SisterDoor = null;

        public bool IsLocked = false;

        // transiting entities
        public Player ArrivingPlayer = null;

        public void Load(BinaryReader reader)
        {
            Color = reader.ReadInt32();
            X = reader.ReadInt32();
        }

        public void Update(float gameTime)
        {
            if (_state != FloorDoorState.Closed)
            {
                _currentFrameTime += gameTime;
                if (_currentFrameTime > _frameTime)
                {
                    _currentFrameTime -= _frameTime;
                    Frame++;

                    if (_state == FloorDoorState.Entering && Frame == 6)
                    {
                        Frame = 3;
                        _state = FloorDoorState.Closed;
                        SisterDoor.ExitOpen();
                    }
                    else if (_state == FloorDoorState.Exiting && Frame == 6)
                    {
                        Frame = 3;
                        _state = FloorDoorState.Closed;

                        IsLocked = false;
                        SisterDoor.IsLocked = false;
                    }
                }
            }
        }

        public void EnterOpen()
        {
            if (_state == FloorDoorState.Closed)
            {
                CanOpen = false;
                SisterDoor.CanOpen = false;
                _state = FloorDoorState.Entering;
                Frame = 0;
                _currentFrameTime = 0.0f;

                IsLocked = true;
                SisterDoor.IsLocked = true;
            }
        }

        public void ExitOpen()
        {
            if (_state == FloorDoorState.Closed)
            {
                _state = FloorDoorState.Exiting;
                Frame = 0;
                _currentFrameTime = 0.0f;

                if (ArrivingPlayer != null)
                {
                    ArrivingPlayer.Exit();
                    ArrivingPlayer = null;
                }
            }
        }

        public void CheckCanOpen()
        {
            if (_state == FloorDoorState.Closed && !IsLocked)
            {
                CanOpen = true;
                SisterDoor.CanOpen = true;
            }
        }        
    }
}
