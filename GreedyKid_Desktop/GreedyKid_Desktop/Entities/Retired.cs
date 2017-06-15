using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Retired : IEntity
    {
        public const int RetiredCount = 7;
        public const int MaxLife = 3;

        private const float _walkSpeed = 16.0f;
        private const float _runSpeed = 48.0f;

        public int Type = 0;
        public float X = 0;
        public Room Room = null;

        public int Life = 1;

        private bool _isFemale = false;

        public int Money = 0;

        public EntityState State = EntityState.Idle;
        public SpriteEffects Orientation = SpriteEffects.None;

        // shared statics
        private static Rectangle[][][] _frames;
        private static float[] _frameDuration;
        private static Rectangle[][] _lifeRectangles;

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
        private float _angryTime = 0.0f;
        public int LastKnownPlayerPosition = -1;

        // stun
        private int _XWarp = -1;

        // heart
        private int _currentHeartFrame = 0;
        private float _currentHeartFrameTime = 0.0f;
        private const float _heartFrameTime = 0.1f;

        public Retired()
        {
            // init once
            if (_frames == null)
            {
                _frames = new Rectangle[(int)EntityState.Count][][];
                _frameDuration = new float[(int)EntityState.Count];

                int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);
                int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

                // idle
                _frames[(int)EntityState.Idle] = new Rectangle[RetiredCount][];
                // idle special
                _frames[(int)EntityState.IdleSpecial] = new Rectangle[RetiredCount][];
                // idle angry
                _frames[(int)EntityState.IdleAngry] = new Rectangle[RetiredCount][];
                // turning
                _frames[(int)EntityState.Turning] = new Rectangle[RetiredCount][];
                // walking
                _frames[(int)EntityState.Walking] = new Rectangle[RetiredCount][];
                // running
                _frames[(int)EntityState.Running] = new Rectangle[RetiredCount][];
                // entering
                _frames[(int)EntityState.Entering] = new Rectangle[RetiredCount][];
                // exiting
                _frames[(int)EntityState.Exiting] = new Rectangle[RetiredCount][];
                // boo
                _frames[(int)EntityState.Boo] = new Rectangle[RetiredCount][];
                // KO
                _frames[(int)EntityState.KO] = new Rectangle[RetiredCount][];
                // Slam
                _frames[(int)EntityState.Slam] = new Rectangle[RetiredCount][];
                // Stun
                _frames[(int)EntityState.Stun] = new Rectangle[RetiredCount][];
                // BeingRessurected
                _frames[(int)EntityState.BeingRessurected] = new Rectangle[RetiredCount][];

                // type
                for (int t = 0; t < RetiredCount; t++)
                {
                    // idle
                    _frames[(int)EntityState.Idle][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Idle][t].Length; f++)
                    {
                        _frames[(int)EntityState.Idle][t][f] = new Rectangle(f * 32 + 38 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Idle] = 0.1f;

                    // idle special
                    _frames[(int)EntityState.IdleSpecial][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.IdleSpecial][t].Length; f++)
                    {
                        _frames[(int)EntityState.IdleSpecial][t][f] = new Rectangle(f * 32 + 4 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.IdleSpecial] = 0.1f;

                    // idle angry
                    _frames[(int)EntityState.IdleAngry][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.IdleAngry][t].Length; f++)
                    {
                        _frames[(int)EntityState.IdleAngry][t][f] = new Rectangle(f * 32 + 18 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.IdleAngry] = 0.1f;

                    // turning
                    _frames[(int)EntityState.Turning][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Turning][t].Length; f++)
                    {
                        _frames[(int)EntityState.Turning][t][f] = new Rectangle(f * 32 + 14 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Turning] = 0.1f;

                    // walking
                    _frames[(int)EntityState.Walking][t] = new Rectangle[6];
                    for (int f = 0; f < _frames[(int)EntityState.Walking][t].Length; f++)
                    {
                        _frames[(int)EntityState.Walking][t][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Walking] = 0.1f;

                    // running
                    _frames[(int)EntityState.Running][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Running][t].Length; f++)
                    {
                        _frames[(int)EntityState.Running][t][f] = new Rectangle(f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Running] = 0.1f;

                    // entering
                    _frames[(int)EntityState.Entering][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Entering][t].Length; f++)
                    {
                        _frames[(int)EntityState.Entering][t][f] = new Rectangle(f * 32 + 32 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Entering] = 0.1f;

                    // exiting
                    _frames[(int)EntityState.Exiting][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Exiting][t].Length; f++)
                    {
                        _frames[(int)EntityState.Exiting][t][f] = new Rectangle(f * 32 + 35 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Exiting] = 0.1f;

                    // boo
                    _frames[(int)EntityState.Boo][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.Boo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Boo][t][f] = new Rectangle(f * 32 + 22 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Boo] = 0.1f;

                    // KO
                    _frames[(int)EntityState.KO][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.KO][t].Length; f++)
                    {
                        _frames[(int)EntityState.KO][t][f] = new Rectangle(f * 32 + 27 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.KO] = 0.1f;

                    // Slam
                    _frames[(int)EntityState.Slam][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Slam][t].Length; f++)
                    {
                        _frames[(int)EntityState.Slam][t][f] = new Rectangle(f * 32 + 46 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Slam] = 0.1f;

                    // Stun
                    _frames[(int)EntityState.Stun][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Stun][t].Length; f++)
                    {
                        _frames[(int)EntityState.Stun][t][f] = new Rectangle(f * 32 + 49 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Stun] = 0.1f;

                    // BeingRessurected
                    _frames[(int)EntityState.BeingRessurected][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.BeingRessurected][t].Length; f++)
                    {
                        _frames[(int)EntityState.BeingRessurected][t][f] = new Rectangle(f * 32 + 53 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.BeingRessurected] = 0.1f;
                }

                // life rectangles
                _lifeRectangles = new Rectangle[MaxLife][];
                for (int l = 0; l < MaxLife; l++)
                {
                    int nbFrames = 4 + l * 4;
                    _lifeRectangles[l] = new Rectangle[nbFrames];

                    for (int f = 0; f < nbFrames; f++)
                    {
                        _lifeRectangles[l][f] = new Rectangle(TextureManager.GameplayWidth - (4 + (MaxLife - 1) * 4) * 16 + f * 16, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 16, 16, 16);
                    }
                }
            }

            NextAction();
            _currentHeartFrame = RandomHelper.Next(4 + (Life - 1) * 4);

            _isFemale = false;
            if (Type == 1 || Type == 3)
                _isFemale = true;
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
            Life = reader.ReadInt32();
            Money = reader.ReadInt32();
        }

        public void Update(float gameTime, bool boo, bool taunted)
        {
            // boo
            if (boo && _isVisible && _angryTime <= 0.0f 
                && State != EntityState.Boo 
                && State != EntityState.Entering 
                && State != EntityState.Exiting 
                && State != EntityState.KO
                && State != EntityState.BeingRessurected)
            {
                Boo();
            }

            // update heart
            if (Life > 0)
            {
                _currentHeartFrameTime += gameTime;

                if (_currentHeartFrameTime > _heartFrameTime)
                {
                    _currentHeartFrameTime -= _heartFrameTime;
                    _currentHeartFrame++;
                    _currentHeartFrame %= _lifeRectangles[Life - 1].Length;
                }
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
                    else if (State == EntityState.Slam)
                    {
                        if (_angryTime > 0.0f)
                            Stun();
                        else
                            NextAction();
                    }

                    else if (State == EntityState.KO)
                    {
                        _currentFrame = _frames[(int)State][Type].Length - 1;
                        if (RandomHelper.Next() < 0.1f)
                            _currentFrame = _frames[(int)State][Type].Length - 2;
                    }

                    // entering doors
                    else if (State == EntityState.Entering)
                    {
                        _isVisible = false;
                        State = EntityState.Idle;
                        if (_targetDoor != null)
                        {
                            Room.Retireds.Remove(this);
                            Room = _targetDoor.Room;
                            Room.Retireds.Add(this);
                            X = _targetDoor.X;
                            _targetDoor = null;
                        }
                    }
                    // exiting doors
                    else if (State == EntityState.Exiting)
                    {
                        NextAction();
                    }                    
                }
            }

            // AI states

            if (_angryTime > 0.0f)
            {
                _angryTime -= gameTime;

                if (_angryTime <= 0.0f)
                {
                    _currentFrame = 0;
                    _currentFrameTime = 0.0f;

                    if (State == EntityState.Running)
                        State = EntityState.Walking;
                    else if (State == EntityState.IdleAngry)
                        State = EntityState.Idle;
                }
            }

            if (_isVisible && _actionTime > 0.0f)
            {
                _actionTime -= gameTime;

                if (_actionTime <= 0.0f)
                {
                    /*
                    if (State == EntityState.Walking)
                    {
                        NextAction();
                    }
                    else if (State == EntityState.Idle)
                    {
                        NextAction();
                    }*/
                    NextAction();
                }
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

            if (State == EntityState.Walking || State == EntityState.Running)
            {
                float speed = _walkSpeed;
                if (State == EntityState.Running)
                    speed = _runSpeed;

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

                if (Life <= 0 && X < roomDoor.X + 16 && X > roomDoor.X - 16)
                {
                    roomDoor.CanClose = false;
                    roomDoor.IsKOBlocked = true;
                    canSeePlayer = false;
                    continue;
                }

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

                if (roomDoor.IsClosed && (State == EntityState.Walking || State == EntityState.Running))
                {
                    if (Orientation == SpriteEffects.FlipHorizontally && X - 12 < roomDoor.X && X - 12 > roomDoor.X - 4)
                    {
                        Turn();
                    }
                    else if (Orientation == SpriteEffects.None && X + 12 > roomDoor.X && X + 12 < roomDoor.X + 4)
                    {
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
            else if (
                (LastKnownPlayerPosition < X + 16.0f && Orientation == SpriteEffects.None) ||
                (LastKnownPlayerPosition > X + 16.0f && Orientation != SpriteEffects.None)
                )
                canSeePlayer = false;

            // player in sight action
            if (LastKnownPlayerPosition >= 0 && canSeePlayer && taunted)
            {
                Taunt();
            }

            if (State == EntityState.Walking || State == EntityState.Running)
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
                                SfxManager.Instance.Play(Sfx.DoorOpenClose);
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

        private void Boo()
        {
            Life--;

            _currentHeartFrame = 0;

            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            if (Orientation == SpriteEffects.None)
                Orientation = SpriteEffects.FlipHorizontally;
            else
                Orientation = SpriteEffects.None;
            
            if (Life <= 0)
            {
                State = EntityState.KO;

                if (Money > 0)
                    SfxManager.Instance.Play(Sfx.MoneyLoot);

                while (Money > 0)
                {
                    if (Money > 2)
                    {
                        Room.AddDrop(ObjectType.CashBig, X);
                        Money -= 3;
                    }
                    else if (Money > 1)
                    {
                        Room.AddDrop(ObjectType.CashMedium, X);
                        Money -= 2;
                    }
                    else
                    {
                        Room.AddDrop(ObjectType.CashSmall, X);
                        Money -= 1;
                    }
                }

                SfxManager.Instance.Play(Sfx.RKOH + (_isFemale ? 1 : 0));
            }
            else
            {
                SfxManager.Instance.Play(Sfx.RBooH + (_isFemale ? 1 : 0));

                SfxManager.Instance.Play(Sfx.RAngryH + (_isFemale ? 1 : 0));

                _angryTime = RandomHelper.Next() * 5.0f + 3.0f;
                
                State = EntityState.Boo;                
            }
        }

        private void Taunt()
        {
            if (_angryTime > 0.0f || Life <= 0)
                return;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;
            _angryTime = RandomHelper.Next() * 5.0f + 3.0f;

            SfxManager.Instance.Play(Sfx.RAngryH + (_isFemale ? 1 : 0));

            Walk();
        }

        private void NextAction()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _wantsToOpenDoor = false;

            // should walk or turn
            if (!_hasJustTurned && RandomHelper.Next() < 0.5f)
                Turn();
            else if (_angryTime <= 0.0f && RandomHelper.Next() < 0.5f)
                WaitSpecial();
            else if (RandomHelper.Next() < 0.75f)
                Walk();
            else
                Wait();
        }

        private void Turn()
        {
            State = EntityState.Turning;
            _hasJustTurned = true;
            _actionTime = 0.0f;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;

            if (_angryTime > 0.0f)
            {
                if (Orientation == SpriteEffects.None)
                    Orientation = SpriteEffects.FlipHorizontally;
                else
                    Orientation = SpriteEffects.None;

                NextAction();
            }
        }

        private void Stun()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Stun;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;
        }

        private void Slam(int warp)
        {
            _XWarp = warp;
            State = EntityState.Slam;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;
            _wantsToOpenDoor = false;

            SfxManager.Instance.Play(Sfx.RBooH + (_isFemale ? 1 : 0));
        }

        public void Ressurect()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;
            _wantsToOpenDoor = false;

            State = EntityState.BeingRessurected;
        }

        public void Revive()
        {
            Life = 1;
            State = EntityState.Idle;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _hasJustTurned = true;
            _wantsToOpenDoor = false;
            _actionTime = 0.0f;

            SfxManager.Instance.Play(Sfx.Revive + (_isFemale ? 1 : 0));

            NextAction();

            if (RandomHelper.Next() > 0.5f)
            {
                _angryTime = RandomHelper.Next() * 5.0f + 3.0f;                
            }
            else
                Stun();            
        }

        private void WaitSpecial()
        {
            State = EntityState.IdleSpecial;
            _actionTime = 0.0f;
            //_hasJustTurned = false;
        }

        private void Walk()
        {
            State = EntityState.Walking;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            if (RandomHelper.Next() <= 0.25f)
            {
                _wantsToOpenDoor = true;
            }

            if (_angryTime > 0.0f)
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

            if (_angryTime > 0.0f)
            {
                State = EntityState.IdleAngry;
            }
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

        public void Draw(SpriteBatch spriteBatch, int cameraPosY)
        {
            if (!_isVisible)
                return;

            Texture2D texture = TextureManager.Gameplay;

            spriteBatch.Draw(texture,
                new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                _frames[(int)State][Type][_currentFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                Orientation,
                0.0f);

            if (Life > 0)
            {
                spriteBatch.Draw(texture,
                new Rectangle((int)X + 8, 128 - 40 * Room.Y + 4 + cameraPosY, 16, 16),
                _lifeRectangles[Life - 1][_currentHeartFrame],
                Color.White);
            }
        }

        public bool NotFacing(int playerMiddle)
        {
            int retiredMiddle = (int)X + 16;

            if (Orientation == SpriteEffects.None && retiredMiddle >= playerMiddle)
                return true;
            else if (Orientation == SpriteEffects.FlipHorizontally && retiredMiddle <= playerMiddle)
                return true;

            return false;
        }

        public bool IsAngry
        {
            get { return _angryTime > 0.0f && State != EntityState.Entering && State != EntityState.Exiting; }
        }
    }
}
