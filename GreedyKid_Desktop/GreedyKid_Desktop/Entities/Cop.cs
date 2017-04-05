using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Cop : IEntity
    {
        public const int CopCount = 1;

        private const float _walkSpeed = 24.0f;
        private const float _runSpeed = 80.0f;
        private const float _rollSpeed = 96.0f;

        public int Type = 0;
        public float X = 0;
        public Room Room = null;

        public EntityState State = EntityState.Idle;
        public SpriteEffects Orientation = SpriteEffects.None;

        // shared statics
        private static Rectangle[][][] _frames;
        private static float[] _frameDuration;

        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;

        // entering / exiting
        private bool _isVisible = true;
        private FloorDoor _targetDoor = null;
        private bool _wantsToOpenDoor = false;

        // walking
        private float _actionTime = 0.0f;
        private bool _hasJustTurned = false;

        // angry
        public int LastKnownPlayerPosition = -1;
        private bool _isAngry = false;
        private float _currentHitCooldown = 0.0f;
        private const float _hitCooldown = 3.0f;
        public const float HitRange = 16.0f;

        // stun
        private int _XWarp = -1;

        // roll arriving
        private bool _shouldRoll = false;

        public Cop()
        {
            // init once
            if (_frames == null)
            {
                _frames = new Rectangle[(int)EntityState.Count][][];
                _frameDuration = new float[(int)EntityState.Count];

                int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);
                int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

                // idle
                _frames[(int)EntityState.Idle] = new Rectangle[CopCount][];
                // idle special
                _frames[(int)EntityState.IdleSpecial] = new Rectangle[CopCount][];
                // turning
                _frames[(int)EntityState.Turning] = new Rectangle[CopCount][];
                // walking
                _frames[(int)EntityState.Walking] = new Rectangle[CopCount][];
                // running
                _frames[(int)EntityState.Running] = new Rectangle[CopCount][];
                // entering
                _frames[(int)EntityState.Entering] = new Rectangle[CopCount][];
                // exiting
                _frames[(int)EntityState.Exiting] = new Rectangle[CopCount][];
                // boo
                _frames[(int)EntityState.Boo] = new Rectangle[CopCount][];
                // unboo
                _frames[(int)EntityState.Unboo] = new Rectangle[CopCount][];
                // Slam
                _frames[(int)EntityState.Slam] = new Rectangle[CopCount][];
                // Hit
                _frames[(int)EntityState.Hit] = new Rectangle[CopCount][];
                // Rolling
                _frames[(int)EntityState.Rolling] = new Rectangle[CopCount][];
                // HitCooldown
                _frames[(int)EntityState.HitCooldown] = new Rectangle[CopCount][];

                // type
                for (int t = 0; t < CopCount; t++)
                {
                    // idle (ok)
                    _frames[(int)EntityState.Idle][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Idle][t].Length; f++)
                    {
                        _frames[(int)EntityState.Idle][t][f] = new Rectangle(f * 32 + 38 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Idle] = 0.1f;

                    // idle special (missing)
                    _frames[(int)EntityState.IdleSpecial][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.IdleSpecial][t].Length; f++)
                    {
                        _frames[(int)EntityState.IdleSpecial][t][f] = new Rectangle(f * 32 + 4 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.IdleSpecial] = 0.1f;

                    // turning (ok)
                    _frames[(int)EntityState.Turning][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Turning][t].Length; f++)
                    {
                        _frames[(int)EntityState.Turning][t][f] = new Rectangle(f * 32 + 14 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Turning] = 0.1f;

                    // walking (ok)
                    _frames[(int)EntityState.Walking][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Walking][t].Length; f++)
                    {
                        _frames[(int)EntityState.Walking][t][f] = new Rectangle(f * 32 + 18 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Walking] = 0.1f;

                    // running (ok)
                    _frames[(int)EntityState.Running][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Running][t].Length; f++)
                    {
                        _frames[(int)EntityState.Running][t][f] = new Rectangle(f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Running] = 0.1f;

                    // entering (ok)
                    _frames[(int)EntityState.Entering][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Entering][t].Length; f++)
                    {
                        _frames[(int)EntityState.Entering][t][f] = new Rectangle(f * 32 + 32 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Entering] = 0.1f;

                    // exiting (ok)
                    _frames[(int)EntityState.Exiting][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Exiting][t].Length; f++)
                    {
                        _frames[(int)EntityState.Exiting][t][f] = new Rectangle(f * 32 + 35 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Exiting] = 0.1f;

                    // boo (ok)
                    _frames[(int)EntityState.Boo][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Boo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Boo][t][f] = new Rectangle(f * 32 + 49 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Boo] = 0.1f;

                    // unboo (ok)
                    _frames[(int)EntityState.Unboo][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Unboo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Unboo][t][f] = new Rectangle(f * 32 + 52 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Unboo] = 0.1f;

                    // HitCooldown (ok)
                    _frames[(int)EntityState.HitCooldown][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.HitCooldown][t].Length; f++)
                    {
                        _frames[(int)EntityState.HitCooldown][t][f] = new Rectangle(f * 32 + 56 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.HitCooldown] = 0.1f;

                    // slam (ok)
                    _frames[(int)EntityState.Slam][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Slam][t].Length; f++)
                    {
                        _frames[(int)EntityState.Slam][t][f] = new Rectangle(f * 32 + 46 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Slam] = 0.1f;

                    // hit (ok)
                    _frames[(int)EntityState.Hit][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Hit][t].Length; f++)
                    {
                        _frames[(int)EntityState.Hit][t][f] = new Rectangle(f * 32 + 28 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Hit] = 0.1f;

                    // rolling (ok)
                    _frames[(int)EntityState.Rolling][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.Rolling][t].Length; f++)
                    {
                        _frames[(int)EntityState.Rolling][t][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Rolling] = 0.1f;
                }
            }

            NextAction();
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }

        public void Spawn(float x)
        {
            X = x;
            _isVisible = false;
            State = EntityState.Idle;
            _currentFrame = 0;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _isAngry = false;
            _shouldRoll = true;
            if (RandomHelper.Next() < 0.5f)
                Orientation = SpriteEffects.FlipHorizontally;
        }

        public void Update(float gameTime, bool boo, bool taunted)
        {
            // boo
            if (boo && _isVisible
                && State != EntityState.Boo
                && State != EntityState.Entering
                && State != EntityState.Exiting)
            {
                Boo();
            }

            // update state
            _currentFrameTime += gameTime;

            if (_currentFrameTime > _frameDuration[(int)State])
            {
                _currentFrameTime -= _frameDuration[(int)State];
                _currentFrame++;

                if (_currentFrame == _frames[(int)State][Type].Length)
                {
                    _currentFrame = 0;

                    // AI states

                    if (State == EntityState.Turning)
                    {
                        if (Orientation == SpriteEffects.None)
                            Orientation = SpriteEffects.FlipHorizontally;
                        else
                            Orientation = SpriteEffects.None;

                        NextAction();
                    }
                    else if (State == EntityState.IdleSpecial)
                    {
                        NextAction();
                    }
                    else if (State == EntityState.Boo)
                    {
                        NextAction();
                    }
                    else if (State == EntityState.Unboo)
                    {
                        NextAction();
                    }
                    else if (State == EntityState.Slam)
                    {
                        NextAction();
                    }
                    else if (State == EntityState.Hit)
                    {
                        Cooldown();
                    }
                    else if (State == EntityState.Rolling)
                    {
                        NextAction();
                    }
                    // entering doors
                    else if (State == EntityState.Entering)
                    {
                        _isVisible = false;
                        State = EntityState.Idle;
                        if (_targetDoor != null)
                        {
                            Room.Cops.Remove(this);
                            Room = _targetDoor.Room;
                            Room.Cops.Add(this);
                            X = _targetDoor.X;
                            _targetDoor = null;
                        }                        
                    }
                    // exiting doors
                    else if (State == EntityState.Exiting)
                    {
                        if (_shouldRoll)
                            Roll();
                        else
                            NextAction();
                    }
                }
            }

            // action
            if (_isVisible && _actionTime > 0.0f)
            {
                _actionTime -= gameTime;

                if (_actionTime <= 0.0f)
                {
                    NextAction();
                }
            }

            if (_currentHitCooldown > 0.0f)
            {
                _currentHitCooldown -= gameTime;
            }

            // warp from slam
            if (_XWarp >= 0)
            {
                if (X < _XWarp)
                {
                    X = X + _runSpeed * 2.0f * gameTime;
                    if (X > _XWarp)
                        _XWarp = -1;
                }
                else if (X > _XWarp)
                {
                    X = X - _runSpeed * 2.0f * gameTime;
                    if (X < _XWarp)
                        _XWarp = -1;
                }
                else
                    _XWarp = -1;
            }

            if (State == EntityState.Walking || State == EntityState.Running || State == EntityState.Rolling)
            {
                float speed = _walkSpeed;
                if (State == EntityState.Running)
                    speed = _runSpeed;
                else if (State == EntityState.Rolling)
                    speed = _rollSpeed;

                if (Orientation == SpriteEffects.None)
                    X = X + speed * gameTime;
                else
                    X = X - speed * gameTime;

                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 8)
                {
                    Turn();
                    X = Room.LeftMargin * 8 + 8;
                }
                if (X > 304 - Room.RightMargin * 8 - 16)
                {
                    Turn();
                    X = 304 - Room.RightMargin * 8 - 16;
                }
            }


            // handle room door collisions

            bool canSeePlayer = true;

            for (int d = 0; d < Room.RoomDoors.Length; d++)
            {
                RoomDoor roomDoor = Room.RoomDoors[d];

                // check if player in sight
                if (LastKnownPlayerPosition >= 0)
                {
                    if (LastKnownPlayerPosition > X + 16
                        && Orientation == SpriteEffects.None
                        && roomDoor.IsClosed
                        && roomDoor.X + 16 > X + 16 && LastKnownPlayerPosition > roomDoor.X + 16)
                        canSeePlayer = false;
                    else if (LastKnownPlayerPosition < X + 16
                        && Orientation == SpriteEffects.FlipHorizontally
                        && roomDoor.IsClosed
                        && roomDoor.X + 16 < X + 16 && LastKnownPlayerPosition < roomDoor.X + 16)
                        canSeePlayer = false;
                    else if (LastKnownPlayerPosition < X + 16
                        && Orientation == SpriteEffects.None)
                        canSeePlayer = false;
                    else if (LastKnownPlayerPosition > X + 16
                        && Orientation == SpriteEffects.FlipHorizontally)
                        canSeePlayer = false;
                }
                else
                    canSeePlayer = false;

                if (roomDoor.IsClosed && (State == EntityState.Walking || State == EntityState.Running || State == EntityState.Rolling))
                {
                    if (Orientation == SpriteEffects.FlipHorizontally && X - 12 < roomDoor.X && X - 12 > roomDoor.X - 4)
                    {
                        if (State == EntityState.Rolling)
                            roomDoor.OpenLeft();
                        else
                            Turn();
                    }
                    else if (Orientation == SpriteEffects.None && X + 12 > roomDoor.X && X + 12 < roomDoor.X + 4)
                    {
                        if (State == EntityState.Rolling)
                            roomDoor.OpenRight();
                        else
                            Turn();
                    }
                }
                else if (roomDoor.IsClosingFromLeft && State != EntityState.KO)
                {
                    if (X < roomDoor.X + 14 && X > roomDoor.X - 14)
                    {
                        Slam(roomDoor.X + 14);
                    }
                }
                else if (roomDoor.IsClosingFromRight && State != EntityState.KO)
                {
                    if (X < roomDoor.X + 14 && X > roomDoor.X - 14)
                    {
                        Slam(roomDoor.X - 14);
                    }
                }
            }

            if (LastKnownPlayerPosition < 0)
                canSeePlayer = false;

            // player in sight action
            if (_isAngry && !canSeePlayer)
            {
                // lost player             
                Unboo();                
            }
            else if (_isVisible && !_isAngry && canSeePlayer && _currentHitCooldown <= 0.0f && State != EntityState.Rolling && State != EntityState.Exiting)
            {
                // caught player
                Boo(false);
            }

            if (_isAngry && canSeePlayer && State != EntityState.Boo && State != EntityState.Slam && Math.Abs(X + 16.0f - LastKnownPlayerPosition) <=  HitRange - 1.0f)
            {
                Hit();
            }

            if (!_isAngry && (State == EntityState.Walking || State == EntityState.Running))
            {
                // entering floor doors
                if (_wantsToOpenDoor)
                {
                    for (int d = 0; d < Room.FloorDoors.Length; d++)
                    {
                        FloorDoor floorDoor = Room.FloorDoors[d];

                        if (X + 16 > floorDoor.X + 14 && X + 16 < floorDoor.X + 24)
                        {
                            if (floorDoor.CanAIOpen)
                            {
                                floorDoor.EnterOpen();
                                Enter(floorDoor);
                                _wantsToOpenDoor = false;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void Boo(bool turn = true)
        {
            _currentFrame = 0;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            if (turn)
            {
                if (Orientation == SpriteEffects.None)
                    Orientation = SpriteEffects.FlipHorizontally;
                else
                    Orientation = SpriteEffects.None;
            }
           
            State = EntityState.Boo;
            _isAngry = true;            
        }

        private void Hit()
        {
            _currentFrame = 0;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _isAngry = false;

            State = EntityState.Hit;

            _currentHitCooldown = _hitCooldown;
        }

        private void NextAction()
        {
            _currentFrame = 0;
            _wantsToOpenDoor = false;

            if (_isAngry)
                Walk();
            else
            {
                // should walk or turn
                if (!_hasJustTurned && RandomHelper.Next() < 0.5f)
                    Turn();
                else if (RandomHelper.Next() < 0.5f)
                    WaitSpecial();
                else if (RandomHelper.Next() < 0.75f)
                    Walk();
                else
                    Wait();
            }
        }

        private void Turn()
        {
            State = EntityState.Turning;
            _hasJustTurned = true;
            _actionTime = 0.0f;
            _currentFrame = 0;

            if (_isAngry)
            {
                if (Orientation == SpriteEffects.None)
                    Orientation = SpriteEffects.FlipHorizontally;
                else
                    Orientation = SpriteEffects.None;

                NextAction();
            }
        }

        private void Slam(int warp)
        {
            _XWarp = warp;
            State = EntityState.Slam;
            _currentFrame = 0;
            _actionTime = 0.0f;
            _hasJustTurned = false;
            _wantsToOpenDoor = false;
        }        

        private void WaitSpecial()
        {
            State = EntityState.IdleSpecial;
            _actionTime = 0.0f;
            //_hasJustTurned = false;
        }

        private void Unboo()
        {
            _isAngry = false;
            _actionTime = 0.0f;
            _currentFrame = 0;
            State = EntityState.Unboo;
        }

        private void Walk()
        {
            State = EntityState.Walking;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            if (!_isAngry && RandomHelper.Next() <= 0.25f)
            {
                _wantsToOpenDoor = true;
            }

            if (_isAngry)
            {
                State = EntityState.Running;
            }
        }

        private void Wait()
        {
            State = EntityState.Idle;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            if (RandomHelper.Next() <= 0.25f)
            {
                _wantsToOpenDoor = true;
            }
        }

        private void Cooldown()
        {
            _currentFrame = 0;
            State = EntityState.HitCooldown;
            _actionTime = _currentHitCooldown;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
        }


        private void Enter(FloorDoor floorDoor)
        {
            X = floorDoor.X;
            _targetDoor = floorDoor.SisterDoor;
            floorDoor.SisterDoor.ArrivingEntity = this;

            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Entering;

            _actionTime = 0.0f;
        }

        public void Exit()
        {
            _isVisible = true;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Exiting;

            _actionTime = 0.0f;
        }

        private void Roll()
        {            
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Rolling;

            _actionTime = 0.0f;

            _shouldRoll = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible)
                return;

            Texture2D texture = TextureManager.Building;

            spriteBatch.Draw(texture,
                new Rectangle((int)X, 128 - 40 * Room.Y + 9, 32, 32),
                _frames[(int)State][Type][_currentFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                Orientation,
                0.0f);
        }

        public bool NotFacing(int playerMiddle)
        {
            int copMiddle = (int)X + 16;

            if (Orientation == SpriteEffects.None && copMiddle >= playerMiddle)
                return true;
            else if (Orientation == SpriteEffects.FlipHorizontally && copMiddle <= playerMiddle)
                return true;

            return false;
        }

        public bool IsHitting
        {
            get { return State == EntityState.Hit; }
        }
    }
}
