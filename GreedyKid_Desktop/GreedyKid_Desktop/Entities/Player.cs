﻿// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GreedyKid
{
    public enum DamageType
    {
        Normal, // tonfa cop / retiree
        Taser, //  taser cop
        Gun, // swat
        Magnum, // robocop
    }

    public sealed class Player : IEntity
    {
        private const float _walkSpeed = 64.0f;
        private const float _rollSpeed = 96.0f;

        public float X = 0;
        public Room Room = null;

        public EntityState State = EntityState.Idle;
        public SpriteEffects Orientation = SpriteEffects.None;

        private Rectangle[][] _frames;
        private float[] _frameDuration;

        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;

        public int Life = 3;
        public int Money = 0;

        // moving
        private int _moveDirection = 0;

        // action
        private bool _doingAction = false;

        // shouting
        private bool _shouting = false;
        private const float _maximumShoutTime = 1.0f;
        private float _currentShoutTime = 0.0f;
        private const float _shoutCooldownTime = 0.25f;
        private float _currentShoutCooldownTime = 0.0f;

        // tauting
        private bool _taunting = false;

        // entering / exiting
        private bool _isVisible = true;
        private FloorDoor _targetDoor = null;

        // hiding / showing
        private Furniture _targetFurniture = null;

        // finishing level
        public bool CanEnterElevator = false;
        public bool HasEnteredElevator = false;

        // smoke animation
        private int _smokeX = 0;
        private int _currentSmokeFrame = -1;
        private float _currentSmokeFrameTime = 0.0f;
        private const int _smokeFrameMovement = 0; // number of smoke from which prevent movements

        // closing doors
        private RoomDoor _closingDoor = null;

        // hit
        private int _XWarp = -1;
        private float _hitTime = 0.0f;
        private bool _hitShow = false;
        private float _currentHitShowTime = 0.0f;
        private const float _hitShowTime = 0.1f;
        private DamageType _lastDamageType = DamageType.Normal;

        // walking smoke
        private int _walkSmokeX = 0;
        private int _currentWalkSmokeFrame = -1;
        private float _currentWalkSmokeFrameTime = 0.0f;
        private SpriteEffects _walkSmokeOrientation = SpriteEffects.FlipHorizontally;

        // rolling smoke
        private int _rollSmokeX = 0;
        private int _currentRollSmokeFrame = -1;
        private float _currentRollSmokeFrameTime = 0.0f;
        private SpriteEffects _rollSmokeOrientation = SpriteEffects.FlipHorizontally;

        public Player()
        {
            _frames = new Rectangle[(int)EntityState.Count][];
            _frameDuration = new float[(int)EntityState.Count];

            int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);
            int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

            // idle
            _frames[(int)EntityState.Idle] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Idle].Length; f++)
            {
                _frames[(int)EntityState.Idle][f] = new Rectangle(f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Idle] = 0.1f;

            // taunting
            _frames[(int)EntityState.Taunting] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Taunting].Length; f++)
            {
                _frames[(int)EntityState.Taunting][f] = new Rectangle(f * 32 + 4 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Taunting] = 0.075f;

            // running
            _frames[(int)EntityState.Running] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Running].Length; f++)
            {
                _frames[(int)EntityState.Running][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Running] = 0.1f;

            // rolling
            _frames[(int)EntityState.Rolling] = new Rectangle[5];
            for (int f = 0; f < _frames[(int)EntityState.Rolling].Length; f++)
            {
                _frames[(int)EntityState.Rolling][f] = new Rectangle(f * 32 + 12 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Rolling] = 0.1f;

            // rolling smoke
            _frames[(int)EntityState.RollSmoke] = new Rectangle[3];
            for (int f = 0; f < _frames[(int)EntityState.RollSmoke].Length; f++)
            {
                _frames[(int)EntityState.RollSmoke][f] = new Rectangle(f * 32 + 17 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.RollSmoke] = 0.1f;

            // shouting
            _frames[(int)EntityState.Shouting] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Shouting].Length; f++)
            {
                _frames[(int)EntityState.Shouting][f] = new Rectangle(f * 32 + 20 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Shouting] = 0.05f;

            // entering
            _frames[(int)EntityState.Entering] = new Rectangle[3];
            for (int f = 0; f < _frames[(int)EntityState.Entering].Length; f++)
            {
                _frames[(int)EntityState.Entering][f] = new Rectangle(f * 32 + 33 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Entering] = 0.1f;

            // exiting
            _frames[(int)EntityState.Exiting] = new Rectangle[3];
            for (int f = 0; f < _frames[(int)EntityState.Exiting].Length; f++)
            {
                _frames[(int)EntityState.Exiting][f] = new Rectangle(f * 32 + 36 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Exiting] = 0.1f;

            // hiding
            _frames[(int)EntityState.Hiding] = new Rectangle[5];
            for (int f = 0; f < _frames[(int)EntityState.Hiding].Length; f++)
            {
                _frames[(int)EntityState.Hiding][f] = new Rectangle(f * 32 + 24 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Hiding] = 0.1f;

            // showing
            _frames[(int)EntityState.Showing] = new Rectangle[2];
            for (int f = 0; f < _frames[(int)EntityState.Showing].Length; f++)
            {
                _frames[(int)EntityState.Showing][f] = new Rectangle(f * 32 + 39 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Showing] = 0.1f;

            // crying
            _frames[(int)EntityState.Crying] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Crying].Length; f++)
            {
                _frames[(int)EntityState.Crying][f] = new Rectangle(f * 32 + 29 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Crying] = 0.1f;

            // smoke
            _frames[(int)EntityState.Smoke] = new Rectangle[5];
            for (int f = 0; f < _frames[(int)EntityState.Smoke].Length; f++)
            {
                _frames[(int)EntityState.Smoke][f] = new Rectangle(f * 32 + 41 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Smoke] = 0.1f;

            // hit
            _frames[(int)EntityState.Hit] = new Rectangle[3];
            for (int f = 0; f < _frames[(int)EntityState.Hit].Length; f++)
            {
                _frames[(int)EntityState.Hit][f] = new Rectangle(f * 32 + 46 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Hit] = 0.1f;

            // slaming door
            _frames[(int)EntityState.SlamingDoor] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.SlamingDoor].Length; f++)
            {
                _frames[(int)EntityState.SlamingDoor][f] = new Rectangle(f * 32 + 49 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.SlamingDoor] = 0.1f;

            // tased
            _frames[(int)EntityState.Tased] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Tased].Length; f++)
            {
                _frames[(int)EntityState.Tased][f] = new Rectangle(f * 32 + 53 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Tased] = 0.1f;

            // shot
            _frames[(int)EntityState.Shot] = new Rectangle[2];
            for (int f = 0; f < _frames[(int)EntityState.Shot].Length; f++)
            {
                _frames[(int)EntityState.Shot][f] = new Rectangle(f * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Shot] = 0.1f;

            // splash
            _frames[(int)EntityState.Splash] = new Rectangle[1];
            for (int f = 0; f < _frames[(int)EntityState.Splash].Length; f++)
            {
                _frames[(int)EntityState.Splash][f] = new Rectangle(f * 32 + 59 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Splash] = 0.1f;

            // walk smoke
            _frames[(int)EntityState.WalkSmoke] = new Rectangle[3];
            for (int f = 0; f < _frames[(int)EntityState.WalkSmoke].Length; f++)
            {
                _frames[(int)EntityState.WalkSmoke][f] = new Rectangle(f * 32 + 60 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.WalkSmoke] = 0.1f;


            // hiding before entering
            _isVisible = false;
        }

        public void Update(float gameTime)
        {
            // hit
            if (_hitTime > 0.0f)
            {
                _hitTime -= gameTime;

                _currentHitShowTime += gameTime;
                if (_currentHitShowTime > _hitShowTime)
                {
                    _currentHitShowTime -= _hitShowTime;
                    _hitShow = !_hitShow;
                    if (State == EntityState.Hit)
                        _hitShow = true;
                }
            }

            // update state
            _currentFrameTime += gameTime;

            if (_currentFrameTime > _frameDuration[(int)State])
            {
                _currentFrameTime -= _frameDuration[(int)State];
                _currentFrame++;

                // taunt sfx                
                if (State == EntityState.Taunting && _currentFrame == 2)
                {
                    SfxManager.Instance.Play(Sfx.Taunt2);
                }

                // walk smoke
                if (State == EntityState.Running && _currentFrame == 1)
                {
                    WalkSmoke();
                }

                // roll smoke
                if (State == EntityState.Rolling && _currentFrame == 3)
                {
                    RollSmoke();
                }

                if (_currentFrame == _frames[(int)State].Length)
                {
                    _currentFrame = 0;

                    // stop rolling
                    if (State == EntityState.Rolling)
                        State = EntityState.Idle;
                    // entering doors
                    else if (State == EntityState.Entering)
                    {
                        _isVisible = false;
                        State = EntityState.Idle;
                        if (_targetDoor != null)
                        {
                            Room = _targetDoor.Room;
                            X = _targetDoor.X;
                            _targetDoor = null;
                        }
                    }
                    // exiting doors
                    else if (State == EntityState.Exiting)
                    {
                        State = EntityState.Idle;
                    }
                    // continue hiding
                    else if (State == EntityState.Hiding)
                    {
                        _currentFrame = _frames[(int)State].Length - 1; // block on last frame
                    }
                    // showing
                    else if (State == EntityState.Showing)
                    {
                        // generate smoke
                        Smoke();
                        State = EntityState.Idle;
                        SfxManager.Instance.Play(Sfx.Show);
                    }
                    // hit
                    else if (State == EntityState.Hit)
                    {
                        State = EntityState.Idle;
                        _XWarp = -1;
                        _hitShow = false;

                        if (Life <= 0)
                            KO();
                    }
                    else if (State == EntityState.SlamingDoor)
                    {
                        State = EntityState.Idle;
                    }
                    else if (State == EntityState.Shot || State == EntityState.Splash)
                    {
                        _currentFrame = _frames[(int)State].Length - 1; // block on last frame
                    }
                    else if (State == EntityState.Taunting)
                    {
                        SfxManager.Instance.Play(Sfx.Taunt1);
                    }
                }
            }

            // smoke anim
            if (_currentSmokeFrame >= 0)
            {
                _currentSmokeFrameTime += gameTime;
                if (_currentSmokeFrameTime > _frameDuration[(int)EntityState.Smoke])
                {
                    _currentSmokeFrameTime -= _frameDuration[(int)EntityState.Smoke];
                    _currentSmokeFrame++;

                    if (_currentSmokeFrame == _frames[(int)EntityState.Smoke].Length)
                    {
                        _currentSmokeFrame = -1;
                    }
                }
            }

            // walk smoke anim
            if (_currentWalkSmokeFrame >= 0)
            {
                _currentWalkSmokeFrameTime += gameTime;
                if (_currentWalkSmokeFrameTime > _frameDuration[(int)EntityState.WalkSmoke])
                {
                    _currentWalkSmokeFrameTime -= _frameDuration[(int)EntityState.WalkSmoke];
                    _currentWalkSmokeFrame++;

                    if (_currentWalkSmokeFrame == _frames[(int)EntityState.WalkSmoke].Length)
                    {
                        _currentWalkSmokeFrame = -1;
                    }
                }
            }

            // roll smoke anim
            if (_currentRollSmokeFrame >= 0)
            {
                _currentRollSmokeFrameTime += gameTime;
                if (_currentRollSmokeFrameTime > _frameDuration[(int)EntityState.RollSmoke])
                {
                    _currentRollSmokeFrameTime -= _frameDuration[(int)EntityState.RollSmoke];
                    _currentRollSmokeFrame++;

                    if (_currentRollSmokeFrame == _frames[(int)EntityState.RollSmoke].Length)
                    {
                        _currentRollSmokeFrame = -1;
                    }
                }
            }

            // preventing any movements if invisible
            if (!_isVisible || State == EntityState.Hit || State == EntityState.KO || (_currentSmokeFrame >= 0 && _currentSmokeFrame < _smokeFrameMovement))
                _moveDirection = 0;

            // start / stop running
            if (_moveDirection != 0 && State == EntityState.Idle)
            {
                State = EntityState.Running;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }
            else if (_moveDirection == 0 && State == EntityState.Running)
            {
                State = EntityState.Idle;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }


            // limit shout time
            if (_currentShoutCooldownTime > 0.0f)
            {
                _currentShoutCooldownTime -= gameTime;
                _shouting = false;
            }
            if (_shouting)
            {               
                _currentShoutTime += gameTime;
                if (_currentShoutTime > _maximumShoutTime)
                {
                    _currentShoutTime = 0.0f;
                    _currentShoutCooldownTime = _shoutCooldownTime;
                    _shouting = false;
                }                
            }

            // start / stop shouting
            if (_shouting && (State == EntityState.Idle || State == EntityState.Running))
            {
                State = EntityState.Shouting;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
                _currentShoutTime = 0.0f;
            }
            else if (!_shouting && State == EntityState.Shouting)
            {
                State = EntityState.Idle;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
                _currentShoutCooldownTime = _shoutCooldownTime;
            }

            // start / stop taunting
            if (_taunting && (State == EntityState.Idle || State == EntityState.Running))
            {
                State = EntityState.Taunting;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }
            else if (!_taunting && State == EntityState.Taunting)
            {
                State = EntityState.Idle;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }

            // can interact with entities?

            // closing room doors
            for (int d = 0; d < Room.RoomDoors.Length; d++)
            {
                RoomDoor roomDoor = Room.RoomDoors[d];

                SpriteEffects targetOrientation = SpriteEffects.None;

                // closing
                if (X + 16 > roomDoor.X && X + 16 < roomDoor.X + 8)
                {
                    roomDoor.CheckCanCloseFromLeft();                    
                    if (roomDoor.CanClose)
                        targetOrientation = SpriteEffects.None;
                }
                else if (X + 16 > roomDoor.X + 24 && X + 16 < roomDoor.X + 32)
                {
                    roomDoor.CheckCanCloseFromRight();
                    if (roomDoor.CanClose)
                        targetOrientation = SpriteEffects.FlipHorizontally;
                }
                else
                    roomDoor.CanClose = false;

                if (_doingAction && roomDoor.CanClose)
                {
                    roomDoor.Close();
                    _closingDoor = roomDoor;
                    Orientation = targetOrientation;
                    SlamDoor();
                }
                else if (!_doingAction && State == EntityState.Rolling && _currentFrame < _frames[(int)State].Length - 1)
                {
                    // rolling through
                    roomDoor.CanClose = false;

                    if (X + 16 > roomDoor.X + 8 && X + 16 < roomDoor.X + 16)
                    {
                        roomDoor.CheckCanCloseFromLeft();
                        if (roomDoor.CanClose)
                            targetOrientation = SpriteEffects.None;
                    }
                    else if (X + 16 > roomDoor.X + 16 && X + 16 < roomDoor.X + 24)
                    {
                        roomDoor.CheckCanCloseFromRight();
                        if (roomDoor.CanClose)
                            targetOrientation = SpriteEffects.FlipHorizontally;
                    }

                    if (roomDoor.CanClose && targetOrientation == Orientation)
                    {
                        roomDoor.Close();
                        Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_17);
                    }
                }
            }

            if (_closingDoor != null)
            {
                if (_closingDoor.IsClosed)
                    _closingDoor = null;
                if (_closingDoor != null)
                    _moveDirection = 0;
            }

            // entering floor doors
            for (int d = 0; d < Room.FloorDoors.Length; d++)
            {
                FloorDoor floorDoor = Room.FloorDoors[d];

                if (X + 16 > floorDoor.X + 11 && X + 16 < floorDoor.X + 27)
                    floorDoor.CheckCanOpen();
                else
                {
                    floorDoor.CanOpen = false;
                    floorDoor.SisterDoor.CanOpen = false;
                }

                if (_doingAction && floorDoor.CanOpen)
                {
                    SfxManager.Instance.Play(Sfx.DoorOpenClose);
                    floorDoor.EnterOpen();
                    Enter(floorDoor);
                }
            }

            // hiding behind furnitures
            for (int f = 0; f < Room.Furnitures.Length; f++)
            {
                Furniture furniture = Room.Furnitures[f];

                if (X + 16 > furniture.X + 11 && X + 16 < furniture.X + 27)
                    furniture.CheckCanHide();
                else
                {
                    furniture.CanHide = false;
                }

                if (_doingAction && furniture.CanHide)
                {
                    furniture.Hide();
                    Hide(furniture);
                }
            }

            // entering elevator
            if (_doingAction && CanEnterElevator)
            {
                X = Room.ExitX + 4;
                Enter(null);
                HasEnteredElevator = true;
            }

            // unhide
            if (_doingAction && State == EntityState.Hiding && _currentFrame == _frames[(int)State].Length - 1)
            {
                if (_targetFurniture != null)
                {
                    _targetFurniture.Show();
                    _targetFurniture = null;
                    Show();
                }
            }

            // getting hit
            if (CanBeHit)
            {
                for (int r = 0; r < Room.Retirees.Count; r++)
                {
                    Retiree retiree = Room.Retirees[r];
                    
                    if (retiree.IsAngry && Math.Abs(retiree.X - X) < 11.0f)
                    {
                        Hit((retiree.Orientation == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None), DamageType.Normal);
                    }
                }

                for (int n = 0; n < Room.Nurses.Count; n++)
                {
                    Nurse nurse = Room.Nurses[n];

                    if (nurse.IsAngry && Math.Abs(nurse.X - X) < 11.0f)
                    {
                        Hit((nurse.Orientation == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None), DamageType.Normal);
                    }
                }

                for (int c = 0; c < Room.Cops.Count; c++)
                {
                    Cop cop = Room.Cops[c];

                    if (cop.IsHitting && cop.Type < Cop.NonFiringCopCount && Math.Abs(cop.X - X) <= Cop.HitRange)
                    {
                        SfxManager.Instance.Play(Sfx.HeavyHit);
                        Hit((cop.Orientation == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None), DamageType.Normal);
                    }
                }
            }

            // warp from hit
            if (_XWarp >= 0)
            {
                if (X < _XWarp)
                {
                    X = X + _walkSpeed * 0.75f * gameTime;
                    if (X > _XWarp)
                        _XWarp = -1;
                }
                else if (X > _XWarp)
                {
                    X = X - _walkSpeed * 0.75f * gameTime;
                    if (X < _XWarp)
                        _XWarp = -1;
                }
                else
                    _XWarp = -1;

                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 4)
                {
                    X = Room.LeftMargin * 8 + 4;
                    _XWarp = -1;
                }
                if (X > 304 - Room.RightMargin * 8 - 12)
                {
                    X = 304 - Room.RightMargin * 8 - 12;
                    _XWarp = -1;
                }
            }

            // moving
            if (_moveDirection != 0)
            {
                if (State == EntityState.Running)
                    X += _moveDirection * _walkSpeed * gameTime;
                else if (State == EntityState.Rolling)
                    X += _moveDirection * _rollSpeed * gameTime;

                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 4)
                    X = Room.LeftMargin * 8 + 4;
                if (X > 304 - Room.RightMargin * 8 - 12)
                    X = 304 - Room.RightMargin * 8 - 12;

                // moving toward a closed door
                for (int d = 0; d < Room.RoomDoors.Length; d++)
                {
                    RoomDoor roomDoor = Room.RoomDoors[d];
                    
                    // auto open
                    if (_moveDirection < 0 && X - 12 < roomDoor.X && X - 12 > roomDoor.X - 4)
                        roomDoor.OpenLeft();
                    else if (_moveDirection > 0 && X + 6 > roomDoor.X && X + 6 < roomDoor.X + 4)
                        roomDoor.OpenRight();

                    
                }

                if (State == EntityState.Running)
                    _moveDirection = 0;
            }

            _doingAction = false;
            _shouting = false;
            _taunting = false;
        }

        public void Draw(SpriteBatch spriteBatch, int cameraPosY)
        {
            if (!_isVisible)
                return;

            Texture2D texture = TextureManager.Gameplay;

            if (_hitTime <= 0.0f || _hitShow)
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                    _frames[(int)State][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);

            if (_currentWalkSmokeFrame >= 0)
            {
                spriteBatch.Draw(texture,
                new Rectangle(_walkSmokeX, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                _frames[(int)EntityState.WalkSmoke][_currentWalkSmokeFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                _walkSmokeOrientation,
                0.0f);
            }

            if (_currentRollSmokeFrame >= 0)
            {
                spriteBatch.Draw(texture,
                new Rectangle(_rollSmokeX, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                _frames[(int)EntityState.RollSmoke][_currentRollSmokeFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                _rollSmokeOrientation,
                0.0f);
            }

            if (_currentSmokeFrame >= 0)
            {
                spriteBatch.Draw(texture,
                new Rectangle(_smokeX, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                _frames[(int)EntityState.Smoke][_currentSmokeFrame],
                Color.White);
            }
        }

        public void MoveLeft()
        {
            if (!_isVisible)
                return;
            if (State == EntityState.Idle || State == EntityState.Running || State == EntityState.Taunting)
            {
                _moveDirection = -1;
                Orientation = SpriteEffects.FlipHorizontally;
            }
        }

        public void MoveRight()
        {
            if (!_isVisible)
                return;
            if (State == EntityState.Idle || State == EntityState.Running || State == EntityState.Taunting)
            {
                _moveDirection = 1;
                Orientation = SpriteEffects.None;
            }
        }

        public void Roll()
        {
            if (!_isVisible)
                return;
            if ((_currentSmokeFrame < 0 || _currentSmokeFrame >= _smokeFrameMovement) && (State == EntityState.Idle || State == EntityState.Running))
            {                
                if (Orientation == SpriteEffects.None)
                    _moveDirection = 1;
                else
                    _moveDirection = -1;

                SfxManager.Instance.Play(Sfx.PlayerRoll);

                State = EntityState.Rolling;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;

                RollSmoke();

                SaveManager.Instance.AddRoll();
            }
        }

        public void Hit(SpriteEffects orientation, DamageType damage)
        {
            _lastDamageType = damage;
            Orientation = orientation;
            if (Orientation == SpriteEffects.None)
                _XWarp = (int)X - 16;
            else
                _XWarp = (int)X + 16;

            State = EntityState.Hit;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;

            _hitTime = 2.0f;
            _hitShow = true;
            _currentHitShowTime = 0.0f;

            Life--;

            Helper.ScreenShakeHelper.Instance.ShakeBorders(Helper.ScreenShakeHelper.SmallForce, Helper.ScreenShakeHelper.ShortDuration);

            SfxManager.Instance.Play(Sfx.Hit);

            if (Life > 0)
                LooseMoney();
        }

        private void LooseMoney(bool all = false)
        {
            int moneyToDrop = Money / 4;
            if (moneyToDrop == 0 && Money > 0)
                moneyToDrop = 1;
            if (all)
            {
                moneyToDrop = Money;
                Money = 0;
            }
            else
            {
                Money = Money / 2;
            }

            if (moneyToDrop > 0)
                SfxManager.Instance.Play(Sfx.MoneyLoot);

            while (moneyToDrop > 0)
            {
                if (moneyToDrop > 2)
                {
                    Room.AddDrop(ObjectType.CashBig, X, 1.0f);
                    moneyToDrop -= 3;
                }
                else if (moneyToDrop > 1)
                {
                    Room.AddDrop(ObjectType.CashMedium, X, 1.0f);
                    moneyToDrop -= 2;
                }
                else
                {
                    Room.AddDrop(ObjectType.CashSmall, X, 1.0f);
                    moneyToDrop -= 1;
                }
            }
        }

        private void SlamDoor()
        {
            State = EntityState.SlamingDoor;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;

            Helper.ScreenShakeHelper.Instance.ShakeScreen(Helper.ScreenShakeHelper.SmallForce, Helper.ScreenShakeHelper.ShortDuration);
        }

        public void HitRobocop(SpriteEffects orientation)
        {
            _lastDamageType = DamageType.Magnum;
            Orientation = orientation;
            Life = 0;
            KO();
        }

        private void KO()
        {
            State = EntityState.Crying;
            if (_lastDamageType == DamageType.Taser)
            {
                SfxManager.Instance.Play(Sfx.TaserHit);
                State = EntityState.Tased;
                Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_14);
            }
            else if (_lastDamageType == DamageType.Gun)
            {
                SfxManager.Instance.Play(Sfx.SwatHit);
                State = EntityState.Shot;
            }
            else if (_lastDamageType == DamageType.Magnum)
            {
                SfxManager.Instance.Play(Sfx.RoboHit);
                State = EntityState.Splash;
                Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_15);
            }
            else
            {
                SfxManager.Instance.Play(Sfx.Cry);
            }
            _currentFrame = 0;
            _currentFrameTime = 0.0f;

            _hitTime = 0.0f;
            _currentHitShowTime = 0.0f;

            LooseMoney(true);
        }

        public void Action()
        {
            if (!_isVisible)
                return;
            if (State == EntityState.Idle || State == EntityState.Running)
            {
                _doingAction = true;
            }
            else if (State == EntityState.Hiding && _targetFurniture != null && _targetFurniture.CheckCanShow())
            {
                _doingAction = true;
            }
        }

        public void Shout(bool fromMicrophone = false)
        {
            if (!_isVisible)
                return;
            if (State == EntityState.Idle || State == EntityState.Running || State == EntityState.Shouting)
            {
                if (State != EntityState.Shouting && _currentShoutCooldownTime <= 0.0f && !fromMicrophone)
                    SfxManager.Instance.Play(Sfx.Shout1 + RandomHelper.Next(5));
                _moveDirection = 0;
                _shouting = true;

                Helper.ScreenShakeHelper.Instance.ShakeScreen(Helper.ScreenShakeHelper.SmallForce, Helper.ScreenShakeHelper.ShortDuration);
            }
        }

        public void Taunt()
        {
            if (!_isVisible)
                return;
            if (State == EntityState.Idle || State == EntityState.Running || State == EntityState.Taunting)
            {
                if (State != EntityState.Taunting)
                    SfxManager.Instance.Play(Sfx.Taunt1);
                _moveDirection = 0;
                _taunting = true;
            }
        }

        private void Enter(FloorDoor floorDoor)
        {
            if (floorDoor != null)
            {
                X = floorDoor.X;
                _targetDoor = floorDoor.SisterDoor;
                floorDoor.SisterDoor.ArrivingEntity = this;
            }

            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Entering;
        }

        public void Exit()
        {
            _isVisible = true;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Exiting;
        }

        private void Hide(Furniture furniture)
        {
            X = furniture.X;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Hiding;

            _targetFurniture = furniture;

            // generate smoke
            Smoke();
            SfxManager.Instance.Play(Sfx.Hide);

            SaveManager.Instance.AddHide();
        }

        private void Show()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Showing;
        }

        private void Smoke()
        {
            _currentSmokeFrame = 0;
            _currentSmokeFrameTime = 0.0f;
            _smokeX = (int)X;
        }

        private void WalkSmoke()
        {
            _currentWalkSmokeFrame = 0;
            _currentWalkSmokeFrameTime = 0.0f;
            _walkSmokeX = (int)X;
            _walkSmokeOrientation = Orientation;
        }

        private void RollSmoke()
        {
            _currentRollSmokeFrame = 0;
            _currentRollSmokeFrameTime = 0.0f;
            _rollSmokeX = (int)X;
            _rollSmokeOrientation = Orientation;
        }

        public bool IsShouting
        {
            get { return State == EntityState.Shouting; }
        }

        public bool IsTaunting
        {
            get { return State == EntityState.Taunting; }
        }

        public bool IsVisible
        {
            get {
                return _isVisible && Life > 0 &&
                    (State == EntityState.Shouting
                      || State == EntityState.Idle
                      || State == EntityState.Running
                      || State == EntityState.Taunting
                      || State == EntityState.Rolling);
            }
        }

        public bool CanBeHit
        {
            get {
                return _hitTime <= 0.0f && _isVisible && Life > 0 &&
                    (State == EntityState.Shouting
                      || State == EntityState.Idle
                      || State == EntityState.Running
                      || State == EntityState.Taunting);
            }
        }

        public bool CanBeHitByRobocop
        {
            get
            {
                return _hitTime <= 0.0f && _isVisible && Life > 0 &&
                    (State == EntityState.Shouting
                      || State == EntityState.Idle
                      || State == EntityState.Running
                      || State == EntityState.Taunting
                      || State == EntityState.Rolling);
            }
        }
    }
}
