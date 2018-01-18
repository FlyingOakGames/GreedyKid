﻿using Microsoft.Xna.Framework;

namespace GreedyKid.Helper
{
    class ScreenShakeHelper
    {
        private static ScreenShakeHelper _instance;

        // screen shake
        private float _screenShakeTime = 0.5f;
        private float _currentScreenShakeTime = 0.0f;
        private float _screenShakeForce = 0.0f;
        private Vector2 _currentScreenShake = Vector2.Zero;

        // borders shake
        private float _bordersShakeTime = 0.5f;
        private float _currentBordersShakeTime = 0.0f;
        private float _bordersShakeForce = 0.0f;
        private Vector2 _currentBordersShake = Vector2.Zero;

        // consts
        public const float SmallForce = 5.0f;
        public const float MediumForce = 10.0f;
        public const float ShortDuration = 0.25f;
        public const float MediumDuration = 0.5f;

        private ScreenShakeHelper()
        {

        }

        public static ScreenShakeHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ScreenShakeHelper();
                return _instance;
            }
        }

        public void Reset()
        {
            // screen shake
            _screenShakeTime = 0.5f;
            _currentScreenShakeTime = 0.0f;
            _screenShakeForce = 0.0f;
            _currentScreenShake = Vector2.Zero;

            // borders shake
            _bordersShakeTime = 0.5f;
            _currentBordersShakeTime = 0.0f;
            _bordersShakeForce = 0.0f;
            _currentBordersShake = Vector2.Zero;
        }

        public void Update(float gameTime)
        {
            if (_currentScreenShakeTime < _screenShakeTime)
            {
                _currentScreenShakeTime += gameTime;
                float progress = _currentScreenShakeTime / _screenShakeTime;
                float magnitude = _screenShakeForce * (1f - (progress * progress));
                _currentScreenShake = new Vector2(RandomHelper.Next(), RandomHelper.Next()) * magnitude;
            }
            else
                _screenShakeForce = 0.0f;

            if (_currentBordersShakeTime < _bordersShakeTime)
            {
                _currentBordersShakeTime += gameTime;
                float progress = _currentBordersShakeTime / _bordersShakeTime;
                float magnitude = _bordersShakeForce * (1f - (progress * progress));
                _currentBordersShake = new Vector2(RandomHelper.Next(), RandomHelper.Next()) * magnitude;
            }
            else
                _bordersShakeForce = 0.0f;
        }

        public void ShakeScreen(float force, float duration)
        {
            if (force >= _screenShakeForce)
            {
                _screenShakeForce = force;
                _screenShakeTime = duration;
                _currentScreenShakeTime = 0.0f;
            }
        }

        public void ShakeBorders(float force, float duration)
        {
            if (force >= _bordersShakeForce)
            {
                _bordersShakeForce = force;
                _bordersShakeTime = duration;
                _currentBordersShakeTime = 0.0f;
            }
        }

        public Vector2 BorderShake
        {
            get
            {
                if (_screenShakeForce > 0.0f)
                    return Vector2.Zero;
                return _currentBordersShake;
            }
        }

        public Vector2 ScreenShake
        {
            get { return _currentScreenShake; }
        }
    }
}
