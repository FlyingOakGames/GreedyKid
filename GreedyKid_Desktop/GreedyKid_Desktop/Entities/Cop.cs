﻿// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Cop : IEntity
    {
        public const int CopCount = 6;
        public const int NonFiringCopCount = 1;
        public const int NormalCopCount = 3;
        public const int SwatCopCount = 2;

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
        private bool _isBreakingWindow = false;
        private int _currentWindowFrame = -1;
        private float _currentWindowFrameTime = 0.0f;
        private int _windowX = 0;

        // bullet
        public bool HasFired = false;

        // SWAT hook
        private Room _upperRoom = null;
        private Room _lowerRoom = null;
        private bool _wantsToHookRoom = false;
        private Room _hookedRoom = null;
        private static int[] _hookingUpOffset = null;
        private static int[] _hookingDownOffset = null;        

        private int _currentSmokeUpFrame = -1;
        private float _currentSmokeUpFrameTime = 0.0f;
        private int _currentSmokeDownFrame = -1;
        private float _currentSmokeDownFrameTime = 0.0f;
        private int _smokeX = 0;
        private int _smokeY = 0;
        private SpriteEffects _smokeOrientation = SpriteEffects.None;

        // Robocop hook
        private static int[] _rocketUpOffset = null;
        private static int[] _rocketDownOffset = null;

        private int _currentRocketSmokeUpFrame = -1;
        private float _currentRocketSmokeUpFrameTime = 0.0f;

        private int _currentLandingSmokeFrame = -1;
        private float _currentLandingSmokeFrameTime = 0.0f;

        private int _currentStartingSmokeFrame = -1;
        private float _currentStartingSmokeFrameTime = 0.0f;

        private static Rectangle _landingLightRectangle;

        public Cop()
        {
            // init once
            if (_frames == null)
            {
                _frames = new Rectangle[(int)EntityState.Count][][];
                _frameDuration = new float[(int)EntityState.Count];

                int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);
                int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

                _landingLightRectangle = new Rectangle(1 * 32 + 28 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + (NormalCopCount + SwatCopCount) * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32 + 16, 32, 1);

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
                // WindowBreak
                _frames[(int)EntityState.WindowBreak] = new Rectangle[CopCount][];
                // HookingDown
                _frames[(int)EntityState.HookingDown] = new Rectangle[CopCount][];
                // HookingUp
                _frames[(int)EntityState.HookingUp] = new Rectangle[CopCount][];
                // RopeUp
                _frames[(int)EntityState.RopeUp] = new Rectangle[CopCount][];
                // RopeDown
                _frames[(int)EntityState.RopeDown] = new Rectangle[CopCount][];
                // SmokeUp
                _frames[(int)EntityState.SmokeUp] = new Rectangle[CopCount][];
                // SmokeDown
                _frames[(int)EntityState.SmokeDown] = new Rectangle[CopCount][];
                // RocketUp
                _frames[(int)EntityState.RocketUp] = new Rectangle[CopCount][];
                // RocketDown
                _frames[(int)EntityState.RocketDown] = new Rectangle[CopCount][];
                // RocketSmokeUp
                _frames[(int)EntityState.RocketSmokeUp] = new Rectangle[CopCount][];
                // LandingSmoke
                _frames[(int)EntityState.LandingSmoke] = new Rectangle[CopCount][];
                // StartingSmoke
                _frames[(int)EntityState.StartingSmoke] = new Rectangle[CopCount][];
                // Landing
                _frames[(int)EntityState.Landing] = new Rectangle[CopCount][];

                // type
                for (int t = 0; t < CopCount; t++)
                {
                    // idle (ok)
                    _frames[(int)EntityState.Idle][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Idle][t].Length; f++)
                    {
                        _frames[(int)EntityState.Idle][t][f] = new Rectangle(f * 32 + 35 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Idle] = 0.1f;

                    // idle special (missing)
                    _frames[(int)EntityState.IdleSpecial][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.IdleSpecial][t].Length; f++)
                    {
                        _frames[(int)EntityState.IdleSpecial][t][f] = new Rectangle(f * 32 + 4 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.IdleSpecial] = 0.1f;

                    // turning (ok)
                    _frames[(int)EntityState.Turning][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Turning][t].Length; f++)
                    {
                        _frames[(int)EntityState.Turning][t][f] = new Rectangle(f * 32 + 13 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Turning] = 0.1f;

                    // walking (ok)
                    _frames[(int)EntityState.Walking][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Walking][t].Length; f++)
                    {
                        _frames[(int)EntityState.Walking][t][f] = new Rectangle(f * 32 + 17 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Walking] = 0.1f;

                    // running (ok)
                    _frames[(int)EntityState.Running][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Running][t].Length; f++)
                    {
                        _frames[(int)EntityState.Running][t][f] = new Rectangle(f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Running] = 0.1f;

                    // entering (ok)
                    _frames[(int)EntityState.Entering][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Entering][t].Length; f++)
                    {
                        _frames[(int)EntityState.Entering][t][f] = new Rectangle(f * 32 + 29 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Entering] = 0.1f;

                    // exiting (ok)
                    _frames[(int)EntityState.Exiting][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Exiting][t].Length; f++)
                    {
                        _frames[(int)EntityState.Exiting][t][f] = new Rectangle(f * 32 + 32 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Exiting] = 0.1f;

                    // boo (ok)
                    _frames[(int)EntityState.Boo][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Boo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Boo][t][f] = new Rectangle(f * 32 + 46 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Boo] = 0.1f;

                    // unboo (ok)
                    _frames[(int)EntityState.Unboo][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Unboo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Unboo][t][f] = new Rectangle(f * 32 + 49 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Unboo] = 0.1f;

                    // HitCooldown (ok)
                    _frames[(int)EntityState.HitCooldown][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.HitCooldown][t].Length; f++)
                    {
                        _frames[(int)EntityState.HitCooldown][t][f] = new Rectangle(f * 32 + 53 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.HitCooldown] = 0.1f;

                    // slam (ok)
                    _frames[(int)EntityState.Slam][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Slam][t].Length; f++)
                    {
                        _frames[(int)EntityState.Slam][t][f] = new Rectangle(f * 32 + 43 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Slam] = 0.1f;

                    // hit (ok)
                    _frames[(int)EntityState.Hit][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Hit][t].Length; f++)
                    {
                        _frames[(int)EntityState.Hit][t][f] = new Rectangle(f * 32 + 25 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Hit] = 0.1f;

                    // rolling (ok)
                    _frames[(int)EntityState.Rolling][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.Rolling][t].Length; f++)
                    {
                        _frames[(int)EntityState.Rolling][t][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Rolling] = 0.1f;

                    // WindowBreak (ok)
                    _frames[(int)EntityState.WindowBreak][t] = new Rectangle[7];
                    for (int f = 0; f < _frames[(int)EntityState.WindowBreak][t].Length; f++)
                    {
                        _frames[(int)EntityState.WindowBreak][t][f] = new Rectangle(f * 32 + TextureManager.GameplayWidth - 657, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 16, 32, 32);
                    }
                    _frameDuration[(int)EntityState.WindowBreak] = 0.1f;

                    // LandingSmoke (ok)
                    _frames[(int)EntityState.LandingSmoke][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.LandingSmoke][t].Length; f++)
                    {
                        _frames[(int)EntityState.LandingSmoke][t][f] = new Rectangle(f * 32 + TextureManager.GameplayWidth - 672 - 7 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 32, 32, 16);
                    }
                    _frameDuration[(int)EntityState.LandingSmoke] = 0.1f;

                    // StartingSmoke (ok)
                    _frames[(int)EntityState.StartingSmoke][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.StartingSmoke][t].Length; f++)
                    {
                        _frames[(int)EntityState.StartingSmoke][t][f] = new Rectangle(f * 32 + TextureManager.GameplayWidth - 672 - 3 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 32, 32, 16);
                    }
                    _frameDuration[(int)EntityState.StartingSmoke] = 0.1f;

                    // SmokeUp (ok)
                    _frames[(int)EntityState.SmokeUp][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.SmokeUp][t].Length; f++)
                    {
                        _frames[(int)EntityState.SmokeUp][t][f] = new Rectangle(f * 32 + TextureManager.GameplayWidth - 657 + 7 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 32, 32, 16);
                    }
                    _frameDuration[(int)EntityState.SmokeUp] = 0.1f;

                    // SmokeDown (ok)
                    _frames[(int)EntityState.SmokeDown][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.SmokeDown][t].Length; f++)
                    {
                        _frames[(int)EntityState.SmokeDown][t][f] = new Rectangle(f * 32 + TextureManager.GameplayWidth - 657 + 11 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 32, 32, 16);
                    }
                    _frameDuration[(int)EntityState.SmokeDown] = 0.1f;

                    // RocketSmokeUp (ok)
                    _frames[(int)EntityState.RocketSmokeUp][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.RocketSmokeUp][t].Length; f++)
                    {
                        _frames[(int)EntityState.RocketSmokeUp][t][f] = new Rectangle(f * 32 + TextureManager.GameplayWidth - 5 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 32, 32, 16);
                    }
                    _frameDuration[(int)EntityState.RocketSmokeUp] = 0.1f;

                    // RocketUp (ok)
                    _frames[(int)EntityState.RocketUp][t] = new Rectangle[9];
                    for (int f = 0; f < _frames[(int)EntityState.RocketUp][t].Length; f++)
                    {
                        _frames[(int)EntityState.RocketUp][t][f] = new Rectangle(f * 32 + 43 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.RocketUp] = 0.1f;

                    // RocketDown (ok)
                    _frames[(int)EntityState.RocketDown][t] = new Rectangle[9];
                    for (int f = 0; f < _frames[(int)EntityState.RocketDown][t].Length; f++)
                    {
                        if (f < 2)
                            _frames[(int)EntityState.RocketDown][t][f] = new Rectangle(f * 32 + 43 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                        else
                            _frames[(int)EntityState.RocketDown][t][f] = new Rectangle(f * 32 + 55 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.RocketDown] = 0.1f;

                    // Landing (ok)
                    _frames[(int)EntityState.Landing][t] = new Rectangle[7];
                    for (int f = 0; f < _frames[(int)EntityState.Landing][t].Length; f++)
                    {
                        if (f > 0)
                            _frames[(int)EntityState.Landing][t][f] = new Rectangle(f * 32 + 28 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                        else
                            _frames[(int)EntityState.Landing][t][f] = new Rectangle(0, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Landing] = 0.1f;

                    // HookingUp (ok)
                    _frames[(int)EntityState.HookingUp][t] = new Rectangle[15];                    
                    _frames[(int)EntityState.HookingUp][t][0] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][1] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][2] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][3] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][4] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][5] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][6] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][7] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][8] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][9] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][10] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][11] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][12] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][13] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][14] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);                    
                    _frameDuration[(int)EntityState.HookingUp] = 0.1f;

                    _hookingUpOffset = new int[]
                    {
                        0, 0, 0, 0, 0, 0, 0, -4, -15, -33, -42, -43, -42, -40, -40
                    };

                    _frames[(int)EntityState.RopeUp][t] = new Rectangle[15];
                    for (int f = 0; f < _frames[(int)EntityState.RopeUp][t].Length; f++)
                    {
                        _frames[(int)EntityState.RopeUp][t][f] = new Rectangle(904 + f * 32, 1814, 32, 49);
                    }
                    _frameDuration[(int)EntityState.RopeUp] = 0.1f;

                    // HookingDown (ok)
                    _frames[(int)EntityState.HookingDown][t] = new Rectangle[15];
                    _frames[(int)EntityState.HookingDown][t][0] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][1] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][2] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][3] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][4] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][5] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][6] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][7] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][8] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][9] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][10] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][11] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][12] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][13] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][14] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retiree.RetireeCount * 32 + Nurse.NurseCount * 32, 32, 32);                    
                    _frameDuration[(int)EntityState.HookingDown] = 0.1f;

                    _hookingDownOffset = new int[]
                    {
                        0, 0, 0, -2, -3, -1, 5, 19, 36, 40, 40, 40, 40, 40, 40
                    };

                    _frames[(int)EntityState.RopeDown][t] = new Rectangle[15];
                    for (int f = 0; f < _frames[(int)EntityState.RopeDown][t].Length; f++)
                    {
                        _frames[(int)EntityState.RopeDown][t][f] = new Rectangle(904 + f * 32 + 15 * 32, 1814, 32, 49);
                    }
                    _frameDuration[(int)EntityState.RopeDown] = 0.1f;

                    _rocketUpOffset = new int[]
                    {
                        0, 0, -4, -11, -22, -31, -40, -40, -40
                    };

                    _rocketDownOffset = new int[]
                    {
                        0, 0, 0, 0, 5, 21, 40, 40, 40
                    };
                }
            }
            
            NextAction();
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }

        public void SpawnElevator(float x)
        {
            X = x;
            _isVisible = false;
            State = EntityState.Idle;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _isAngry = false;
            _shouldRoll = true;
            _isBreakingWindow = false;

            if (RandomHelper.Next() < 0.5f)
                Orientation = SpriteEffects.FlipHorizontally;
        }

        public void SpawnWindow(float x, SpriteEffects orientation)
        {
            X = x;
            _isVisible = true;
            State = EntityState.Idle;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _isAngry = false;
            _shouldRoll = false;
            _isBreakingWindow = true;

            Orientation = orientation;

            _windowX = (int)X;
            _currentWindowFrame = 0;
            _currentWindowFrameTime = 0.0f;

            SfxManager.Instance.Play(Sfx.WidowsBreak);
            Helper.ScreenShakeHelper.Instance.ShakeScreen(Helper.ScreenShakeHelper.MediumForce, Helper.ScreenShakeHelper.ShortDuration);

            Roll();
        }

        public void SpawnLand(float x, SpriteEffects orientation)
        {
            X = x;
            _isVisible = true;
            State = EntityState.Idle;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _isAngry = false;
            _shouldRoll = false;
            _isBreakingWindow = false;

            Orientation = orientation;

            SfxManager.Instance.Play(Sfx.RoboLanding);

            Land();
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

            // window frame
            if (_currentWindowFrame >= 0)
            {
                _currentWindowFrameTime += gameTime;

                if (_currentWindowFrameTime > _frameDuration[(int)EntityState.WindowBreak])
                {
                    _currentWindowFrameTime -= _frameDuration[(int)EntityState.WindowBreak];
                    _currentWindowFrame++;

                    if (_currentWindowFrame == _frames[(int)EntityState.WindowBreak][Type].Length)
                    {
                        _currentWindowFrame = -1;
                    }
                }
            }

            // smoke frame
            if (_currentSmokeUpFrame >= 0)
            {
                _currentSmokeUpFrameTime += gameTime;

                if (_currentSmokeUpFrameTime > _frameDuration[(int)EntityState.SmokeUp])
                {
                    _currentSmokeUpFrameTime -= _frameDuration[(int)EntityState.SmokeUp];
                    _currentSmokeUpFrame++;

                    if (_currentSmokeUpFrame == _frames[(int)EntityState.SmokeUp][Type].Length)
                    {
                        _currentSmokeUpFrame = -1;
                    }
                }
            }

            if (_currentSmokeDownFrame >= 0)
            {
                _currentSmokeDownFrameTime += gameTime;

                if (_currentSmokeDownFrameTime > _frameDuration[(int)EntityState.SmokeDown])
                {
                    _currentSmokeDownFrameTime -= _frameDuration[(int)EntityState.SmokeDown];
                    _currentSmokeDownFrame++;

                    if (_currentSmokeDownFrame == _frames[(int)EntityState.SmokeDown][Type].Length)
                    {
                        _currentSmokeDownFrame = -1;
                    }
                }
            }

            // rocket smoke
            if (_currentRocketSmokeUpFrame >= 0)
            {
                _currentRocketSmokeUpFrameTime += gameTime;

                if (_currentRocketSmokeUpFrameTime > _frameDuration[(int)EntityState.RocketSmokeUp])
                {
                    _currentRocketSmokeUpFrameTime -= _frameDuration[(int)EntityState.RocketSmokeUp];
                    _currentRocketSmokeUpFrame++;

                    if (_currentRocketSmokeUpFrame == _frames[(int)EntityState.RocketSmokeUp][Type].Length)
                    {
                        _currentRocketSmokeUpFrame = -1;
                    }
                }
            }

            if (_currentLandingSmokeFrame >= 0)
            {
                _currentLandingSmokeFrameTime += gameTime;

                if (_currentLandingSmokeFrameTime > _frameDuration[(int)EntityState.LandingSmoke])
                {
                    _currentLandingSmokeFrameTime -= _frameDuration[(int)EntityState.LandingSmoke];
                    _currentLandingSmokeFrame++;

                    if (_currentLandingSmokeFrame == _frames[(int)EntityState.LandingSmoke][Type].Length)
                    {
                        _currentLandingSmokeFrame = -1;
                    }
                }
            }

            if (_currentStartingSmokeFrame >= 0)
            {
                _currentStartingSmokeFrameTime += gameTime;

                if (_currentStartingSmokeFrameTime > _frameDuration[(int)EntityState.StartingSmoke])
                {
                    _currentStartingSmokeFrameTime -= _frameDuration[(int)EntityState.StartingSmoke];
                    _currentStartingSmokeFrame++;

                    if (_currentStartingSmokeFrame == _frames[(int)EntityState.StartingSmoke][Type].Length)
                    {
                        _currentStartingSmokeFrame = -1;
                    }
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
                    else if (State == EntityState.Landing)
                    {
                        NextAction();
                    }
                    else if (State == EntityState.HookingUp ||
                        State == EntityState.HookingDown ||
                        State == EntityState.RocketUp ||
                        State == EntityState.RocketDown)
                    {
                        Room.Cops.Remove(this);
                        Room = _hookedRoom;
                        _hookedRoom = null;
                        Room.Cops.Add(this);

                        if (State == EntityState.RocketUp)
                        {
                            _currentLandingSmokeFrame = 0;
                            _currentLandingSmokeFrameTime = 0.0f;

                            _smokeX = (int)X;
                            _smokeY = 128 - 40 * Room.Y + 9 + 16;
                            _smokeOrientation = Orientation;
                        }

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
                else if (State == EntityState.Rolling && _isBreakingWindow)
                {
                    _isBreakingWindow = false;
                }
                else if (State == EntityState.RocketUp && _currentFrame == 2)
                {
                    _currentRocketSmokeUpFrame = 0;
                    _currentRocketSmokeUpFrameTime = 0.0f;
                }
                else if (State == EntityState.RocketDown && _currentFrame == 2)
                {
                    _currentStartingSmokeFrame = 0;
                    _currentStartingSmokeFrameTime = 0.0f;
                }
                else if (State == EntityState.RocketDown && _currentFrame == 7)
                {
                    _currentLandingSmokeFrame = 0;
                    _currentLandingSmokeFrameTime = 0.0f;

                    _smokeX = (int)X;
                    _smokeY = 128 - 40 * Room.Y + 9 + 16 + 40;
                    _smokeOrientation = Orientation;
                }
                else if (State == EntityState.HookingUp && _currentFrame == 7)
                {
                    _currentSmokeUpFrame = 0;
                    _currentSmokeUpFrameTime = 0.0f;
                }
                else if (State == EntityState.HookingUp && _currentFrame == 13)
                {
                    _currentSmokeDownFrame = 0;
                    _currentSmokeDownFrameTime = 0.0f;

                    _smokeX = (int)X;
                    _smokeY = 128 - 40 * Room.Y + 9 + 16 - 40;
                    _smokeOrientation = Orientation;
                }
                else if (State == EntityState.Landing && _currentFrame == 5)
                {
                    _currentSmokeDownFrame = 0;
                    _currentSmokeDownFrameTime = 0.0f;

                    _smokeX = (int)X;
                    _smokeY = 128 - 40 * Room.Y + 9 + 16;
                    _smokeOrientation = Orientation;

                    Helper.ScreenShakeHelper.Instance.ShakeScreen(Helper.ScreenShakeHelper.MediumForce, Helper.ScreenShakeHelper.MediumDuration);
                }
                else if (State == EntityState.HookingDown && _currentFrame == 3)
                {
                    _currentSmokeUpFrame = 0;
                    _currentSmokeUpFrameTime = 0.0f;
                }
                else if (State == EntityState.HookingDown && _currentFrame == 9)
                {
                    _currentSmokeDownFrame = 0;
                    _currentSmokeDownFrameTime = 0.0f;

                    _smokeX = (int)X;
                    _smokeY = 128 - 40 * Room.Y + 9 + 16 + 40;
                    _smokeOrientation = Orientation;
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

                _upperRoom = null;
                _lowerRoom = null;
            }

            if (State == EntityState.Walking || State == EntityState.Running || State == EntityState.Rolling)
            {
                float speed = _walkSpeed;
                if (State == EntityState.Running)
                    speed = _runSpeed;
                else if (State == EntityState.Rolling)
                    speed = _rollSpeed;

                if (_isBreakingWindow && _currentFrame == 0)
                    speed = 0;

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

                if (Type >= NormalCopCount)
                    LookForRoomToHook();
            }


            // handle room door collisions

            bool canSeePlayer = true;

            for (int d = 0; d < Room.RoomDoors.Length; d++)
            {
                RoomDoor roomDoor = Room.RoomDoors[d];

                if (Type >= NormalCopCount + SwatCopCount && X < roomDoor.X + 16 && X > roomDoor.X - 16 && (roomDoor.IsOpenLeft || roomDoor.IsOpenRight))
                {
                    roomDoor.CanClose = false;
                    roomDoor.IsRobocopBlocked = true;
                    //continue;
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
            else if (
                (LastKnownPlayerPosition < X + 16.0f && Orientation == SpriteEffects.None) ||
                (LastKnownPlayerPosition > X + 16.0f && Orientation != SpriteEffects.None)
                )
                canSeePlayer = false;

            // player in sight action
            if (_isAngry && !canSeePlayer)
            {
                // lost player             
                Unboo();                
            }
            else if (_isVisible && !_isAngry && canSeePlayer && _currentHitCooldown <= 0.0f && State != EntityState.Rolling && State != EntityState.Exiting && State != EntityState.HookingDown && State != EntityState.HookingUp && State != EntityState.Landing && State != EntityState.RocketUp && State != EntityState.RocketDown)
            {
                // caught player
                if (Type < NormalCopCount + SwatCopCount)
                    Boo(false);
                else
                    Hit();
            }

            if (_isAngry && canSeePlayer && State != EntityState.Boo && State != EntityState.Slam && Math.Abs(X + 16.0f - LastKnownPlayerPosition) <=  HitRange - 1.0f)
            {
                SfxManager.Instance.Play(Sfx.CopSwing);
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
                                SfxManager.Instance.Play(Sfx.DoorOpenClose);
                                floorDoor.EnterOpen();
                                Enter(floorDoor);
                                _wantsToOpenDoor = false;
                                _wantsToHookRoom = false;
                                break;
                            }
                        }
                    }
                }

                // hooking to rooms
                if (_wantsToHookRoom && (_upperRoom != null || _lowerRoom != null))
                {                   
                    if (_upperRoom != null && _lowerRoom != null)
                    {
                        if (RandomHelper.Next() > 0.5f)
                        {
                            _hookedRoom = _upperRoom;
                            GoUp();
                        }
                        else
                        {
                            _hookedRoom = _lowerRoom;
                            GoDown();
                        }
                    }
                    else if (_upperRoom != null)
                    {
                        _hookedRoom = _upperRoom;
                        GoUp();
                    }
                    else
                    {
                        _hookedRoom = _lowerRoom;
                        GoDown();
                    }

                    _wantsToHookRoom = false;
                    _wantsToOpenDoor = false;
                    _upperRoom = null;
                    _lowerRoom = null;
                }
            }
        }

        private void GoUp()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;

            _isAngry = false;

            if (Type < NormalCopCount + SwatCopCount)
            {
                State = EntityState.HookingUp;
                SfxManager.Instance.Play(Sfx.GrapplingUp);
            }
            else
            {
                State = EntityState.RocketUp;
                SfxManager.Instance.Play(Sfx.RoboUp);
            }
        }

        private void GoDown()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;

            _isAngry = false;

            if (Type < NormalCopCount + SwatCopCount)
            {
                State = EntityState.HookingDown;
                SfxManager.Instance.Play(Sfx.GrapplingDown);
            }
            else
            {
                State = EntityState.RocketDown;
                SfxManager.Instance.Play(Sfx.RoboDown);
            }
        }

        private void Boo(bool turn = true)
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            if (turn)
            {
                if (Orientation == SpriteEffects.None)
                    Orientation = SpriteEffects.FlipHorizontally;
                else
                    Orientation = SpriteEffects.None;

                SaveManager.Instance.AddBoo();

                if (Type >= NormalCopCount)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_20);
            }

            SfxManager.Instance.Play(Sfx.CopSurprise);

            State = EntityState.Boo;
            _isAngry = true;            
        }

        private void Hit()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _isAngry = false;

            State = EntityState.Hit;

            _currentHitCooldown = _hitCooldown;

            if (Type >= NonFiringCopCount && Type < NormalCopCount + SwatCopCount)
            {
                if (Type >= NormalCopCount)
                    SfxManager.Instance.Play(Sfx.SwatFire);
                else
                    SfxManager.Instance.Play(Sfx.CopTaser);
                HasFired = true;
            }
        }

        private void NextAction()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;

            if (_isAngry && Type == 0)
                Walk();
            else if (_isAngry)
                Hit();
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
            _currentFrameTime = 0.0f;

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
            if (State == EntityState.Slam)
                return;

            _XWarp = warp;
            State = EntityState.Slam;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;
            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;

            Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_16);
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
            _currentFrameTime = 0.0f;
            State = EntityState.Unboo;
        }

        private void Walk()
        {
            State = EntityState.Walking;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            if (!_isAngry && RandomHelper.Next() <= 0.25f && Type < NormalCopCount + SwatCopCount)
            {
                _wantsToOpenDoor = true;
            }

            _wantsToHookRoom = false;
            if (RandomHelper.Next() <= 0.25f)
            {
                _wantsToHookRoom = true;
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
            if (RandomHelper.Next() <= 0.25f && Type < NormalCopCount + SwatCopCount)
            {
                _wantsToOpenDoor = true;
            }

            _wantsToHookRoom = false;
            if (RandomHelper.Next() <= 0.25f)
            {
                _wantsToHookRoom = true;
            }
        }

        private void Cooldown()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.HitCooldown;
            if (Type < NormalCopCount + SwatCopCount)
                _actionTime = _currentHitCooldown;
            else
            {
                SfxManager.Instance.Play(Sfx.RoboFire);
                HasFired = true;
                _actionTime = _frameDuration[(int)EntityState.HitCooldown] * _frames[(int)EntityState.HitCooldown][Type].Length;
            }
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;
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

            SfxManager.Instance.Play(Sfx.CopRoll);

            _shouldRoll = false;
        }

        private void Land()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Landing;

            _actionTime = 0.0f;

            _shouldRoll = false;
        }

        public void Draw(SpriteBatch spriteBatch, int cameraPosY)
        {
            if (!_isVisible)
                return;

            Texture2D texture = TextureManager.Gameplay;

            int robocopOffset = 0;
            if (Type >= NormalCopCount + SwatCopCount)
            {
                if (Orientation == SpriteEffects.None)
                    robocopOffset = 4;
                else
                    robocopOffset = -4;
            }

            // landing light
            if (State == EntityState.Landing && _currentFrame == 1)
            {
                int height = 128 - 40 * Room.Y + 9 + cameraPosY + 2;
                if (height > 0)
                    spriteBatch.Draw(texture,
                        new Rectangle((int)X + robocopOffset,0, 32, height),
                        _landingLightRectangle,
                        Color.White,
                        0.0f,
                        Vector2.Zero,
                        Orientation,
                        0.0f);
            }

            if (State != EntityState.HookingDown && State != EntityState.HookingUp && State != EntityState.RocketUp && State != EntityState.RocketDown)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X + robocopOffset, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                    _frames[(int)State][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }
            else if (State == EntityState.HookingUp)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY - 42, 32, 49),
                    _frames[(int)EntityState.RopeUp][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY + _hookingUpOffset[_currentFrame], 32, 32),
                    _frames[(int)State][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }
            else if (State == EntityState.HookingDown)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY - 2, 32, 49),
                    _frames[(int)EntityState.RopeDown][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY + _hookingDownOffset[_currentFrame], 32, 32),
                    _frames[(int)State][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }
            else if (State == EntityState.RocketUp)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X + robocopOffset, 128 - 40 * Room.Y + 9 + cameraPosY + _rocketUpOffset[_currentFrame], 32, 32),
                    _frames[(int)State][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }
            else if (State == EntityState.RocketDown)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X + robocopOffset, 128 - 40 * Room.Y + 9 + cameraPosY + _rocketDownOffset[_currentFrame], 32, 32),
                    _frames[(int)State][Type][_currentFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }

            if (_currentWindowFrame >= 0)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(_windowX, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
                    _frames[(int)EntityState.WindowBreak][Type][_currentWindowFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }

            if (_currentSmokeUpFrame >= 0)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY + 16, 32, 16),
                    _frames[(int)EntityState.SmokeUp][Type][_currentSmokeUpFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }

            if (_currentSmokeDownFrame >= 0)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(_smokeX, _smokeY + cameraPosY, 32, 16),
                    _frames[(int)EntityState.SmokeDown][Type][_currentSmokeDownFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    _smokeOrientation,
                    0.0f);
            }

            if (_currentRocketSmokeUpFrame >= 0)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY + 16, 32, 16),
                    _frames[(int)EntityState.RocketSmokeUp][Type][_currentRocketSmokeUpFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }

            if (_currentStartingSmokeFrame >= 0)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY + 16, 32, 16),
                    _frames[(int)EntityState.StartingSmoke][Type][_currentStartingSmokeFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Orientation,
                    0.0f);
            }

            if (_currentLandingSmokeFrame >= 0)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(_smokeX, _smokeY + cameraPosY, 32, 16),
                    _frames[(int)EntityState.LandingSmoke][Type][_currentLandingSmokeFrame],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    _smokeOrientation,
                    0.0f);
            }
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

        public int RobocopBeamPosition
        {
            get
            {
                if (State == EntityState.Landing && _currentFrame == 0)
                    return (int)X;
                else
                    return -1;
            }
        }

        public bool RobocopFlash
        {
            get
            {
                return (State == EntityState.Landing && _currentFrame == 3) || (Type >= NormalCopCount + SwatCopCount && State == EntityState.HitCooldown && _currentFrame == 0);
            }
        }

        private Room LookForRoomToHook()
        {
            _upperRoom = null;
            _lowerRoom = null;

            // look if room available up
            if (Room.UpperFloor != null)
            {
                for (int r = 0; r < Room.UpperFloor.Rooms.Length; r++)
                {
                    Room room = Room.UpperFloor.Rooms[r];

                    // handle wall collisions
                    if (X >= room.LeftMargin * 8 + 8 && X <= 304 - room.RightMargin * 8 - 16)
                    {
                        bool doorInTheWay = false;
                        // check on doors
                        for (int d = 0; d < room.RoomDoors.Length; d++)
                        {
                            RoomDoor door = room.RoomDoors[d];
                            if ((X >= door.X && X <= door.X + 32) ||
                                (X + 32 >= door.X && X + 32 <= door.X + 32))
                                doorInTheWay = true;
                        }

                        if (!doorInTheWay)
                            _upperRoom = room;
                    }
                }
            }

            // look if room available down
            if (Room.LowerFloor != null)
            {
                for (int r = 0; r < Room.LowerFloor.Rooms.Length; r++)
                {
                    Room room = Room.LowerFloor.Rooms[r];

                    // handle wall collisions
                    if (X >= room.LeftMargin * 8 + 8 && X <= 304 - room.RightMargin * 8 - 16)
                    {
                        bool doorInTheWay = false;
                        // check on doors
                        for (int d = 0; d < room.RoomDoors.Length; d++)
                        {
                            RoomDoor door = room.RoomDoors[d];
                            if ((X >= door.X && X <= door.X + 32) ||
                                (X + 32 >= door.X && X + 32 <= door.X + 32))
                                doorInTheWay = true;
                        }

                        if (!doorInTheWay)
                            _lowerRoom = room;
                    }
                }
            }

            return null;
        }
    }
}
