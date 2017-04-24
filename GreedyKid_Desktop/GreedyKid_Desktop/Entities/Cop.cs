using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Cop : IEntity
    {
        public const int CopCount = 3;
        public const int NonFiringCopCount = 1;
        public const int NormalCopCount = 2;
        public const int SwatCopCount = 1;

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

                // type
                for (int t = 0; t < CopCount; t++)
                {
                    // idle (ok)
                    _frames[(int)EntityState.Idle][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Idle][t].Length; f++)
                    {
                        _frames[(int)EntityState.Idle][t][f] = new Rectangle(f * 32 + 35 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
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
                        _frames[(int)EntityState.Turning][t][f] = new Rectangle(f * 32 + 13 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Turning] = 0.1f;

                    // walking (ok)
                    _frames[(int)EntityState.Walking][t] = new Rectangle[8];
                    for (int f = 0; f < _frames[(int)EntityState.Walking][t].Length; f++)
                    {
                        _frames[(int)EntityState.Walking][t][f] = new Rectangle(f * 32 + 17 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
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
                        _frames[(int)EntityState.Entering][t][f] = new Rectangle(f * 32 + 29 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Entering] = 0.1f;

                    // exiting (ok)
                    _frames[(int)EntityState.Exiting][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Exiting][t].Length; f++)
                    {
                        _frames[(int)EntityState.Exiting][t][f] = new Rectangle(f * 32 + 32 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Exiting] = 0.1f;

                    // boo (ok)
                    _frames[(int)EntityState.Boo][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Boo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Boo][t][f] = new Rectangle(f * 32 + 46 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Boo] = 0.1f;

                    // unboo (ok)
                    _frames[(int)EntityState.Unboo][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Unboo][t].Length; f++)
                    {
                        _frames[(int)EntityState.Unboo][t][f] = new Rectangle(f * 32 + 49 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Unboo] = 0.1f;

                    // HitCooldown (ok)
                    _frames[(int)EntityState.HitCooldown][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.HitCooldown][t].Length; f++)
                    {
                        _frames[(int)EntityState.HitCooldown][t][f] = new Rectangle(f * 32 + 53 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.HitCooldown] = 0.1f;

                    // slam (ok)
                    _frames[(int)EntityState.Slam][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Slam][t].Length; f++)
                    {
                        _frames[(int)EntityState.Slam][t][f] = new Rectangle(f * 32 + 43 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Slam] = 0.1f;

                    // hit (ok)
                    _frames[(int)EntityState.Hit][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Hit][t].Length; f++)
                    {
                        _frames[(int)EntityState.Hit][t][f] = new Rectangle(f * 32 + 25 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Hit] = 0.1f;

                    // rolling (ok)
                    _frames[(int)EntityState.Rolling][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.Rolling][t].Length; f++)
                    {
                        _frames[(int)EntityState.Rolling][t][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Rolling] = 0.1f;

                    // WindowBreak (ok)
                    _frames[(int)EntityState.WindowBreak][t] = new Rectangle[7];
                    for (int f = 0; f < _frames[(int)EntityState.WindowBreak][t].Length; f++)
                    {
                        _frames[(int)EntityState.WindowBreak][t][f] = new Rectangle(f * 32 + BuildingManager.TextureWidth - 657, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 16, 32, 32);
                    }
                    _frameDuration[(int)EntityState.WindowBreak] = 0.1f;

                    // HookingUp (ok)
                    _frames[(int)EntityState.HookingUp][t] = new Rectangle[15];                    
                    _frames[(int)EntityState.HookingUp][t][0] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][1] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][2] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][3] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][4] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][5] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][6] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][7] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][8] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][9] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][10] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][11] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][12] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][13] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingUp][t][14] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);                    
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
                    _frames[(int)EntityState.HookingDown][t][0] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][1] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][2] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][3] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][4] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][5] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][6] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][7] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][8] = new Rectangle(3 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][9] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][10] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][11] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][12] = new Rectangle(1 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][13] = new Rectangle(2 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);
                    _frames[(int)EntityState.HookingDown][t][14] = new Rectangle(0 * 32 + 57 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32 + Retired.RetiredCount * 32 + Nurse.NurseCount * 32, 32, 32);                    
                    _frameDuration[(int)EntityState.HookingDown] = 0.1f;

                    _hookingDownOffset = new int[]
                    {
                        0, 0, 0, -2, -3, -1, 5, 19, 36, 40, 40, 40, 40, 40, 40
                    };

                    _frames[(int)EntityState.RopeDown][t] = new Rectangle[15];
                    for (int f = 0; f < _frames[(int)EntityState.RopeDown][t].Length; f++)
                    {
                        _frames[(int)EntityState.RopeDown][t][f] = new Rectangle(904 + f * 32, 1870, 32, 49);
                    }
                    _frameDuration[(int)EntityState.RopeDown] = 0.1f;
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

            Roll();
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
                    else if (State == EntityState.HookingUp || State == EntityState.HookingDown)
                    {
                        Room.Cops.Remove(this);
                        Room = _hookedRoom;
                        _hookedRoom = null;
                        Room.Cops.Add(this);

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

                if (Type >= NormalCopCount && Type < NormalCopCount + SwatCopCount)
                    LookForRoomToHook();
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
            else if (_isVisible && !_isAngry && canSeePlayer && _currentHitCooldown <= 0.0f && State != EntityState.Rolling && State != EntityState.Exiting && State != EntityState.HookingDown && State != EntityState.HookingUp)
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

                // hooking to rooms
                if (_wantsToHookRoom && (_upperRoom != null || _lowerRoom != null))
                {
                    if (_upperRoom != null && _lowerRoom != null)
                    {
                        if (RandomHelper.Next() > 0.5f)
                        {
                            _hookedRoom = _upperRoom;
                            HookingUp();
                        }
                        else
                        {
                            _hookedRoom = _lowerRoom;
                            HookingDown();
                        }
                    }
                    else if (_upperRoom != null)
                    {
                        _hookedRoom = _upperRoom;
                        HookingUp();
                    }
                    else
                    {
                        _hookedRoom = _lowerRoom;
                        HookingDown();
                    }

                    _wantsToHookRoom = false;
                    _upperRoom = null;
                    _lowerRoom = null;
                }
            }
        }

        private void HookingUp()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;

            _isAngry = false;

            State = EntityState.HookingUp;
        }

        private void HookingDown()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;

            _isAngry = false;

            State = EntityState.HookingDown;
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
            }
           
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

            if (Type >= NonFiringCopCount)
                HasFired = true;
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
            _XWarp = warp;
            State = EntityState.Slam;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _actionTime = 0.0f;
            _hasJustTurned = false;
            _wantsToOpenDoor = false;
            _wantsToHookRoom = false;
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
            if (!_isAngry && RandomHelper.Next() <= 0.25f)
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
            if (RandomHelper.Next() <= 0.25f)
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
            _actionTime = _currentHitCooldown;
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

            _shouldRoll = false;
        }

        public void Draw(SpriteBatch spriteBatch, int cameraPosY)
        {
            if (!_isVisible)
                return;

            Texture2D texture = TextureManager.Building;

            if (State != EntityState.HookingDown && State != EntityState.HookingUp)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)X, 128 - 40 * Room.Y + 9 + cameraPosY, 32, 32),
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
