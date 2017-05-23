using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;

namespace GreedyKid
{
    public sealed class MusicManager
    {
        private static MusicManager _instance;

        public static MusicManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MusicManager();
                return _instance;
            }
        }

        private const string _musicPath = "Musics/";

        private Song _currentSong;

        private bool IsPlaying = false;

        private MusicManager()
        {
            SetVolume(1.0f);
        }

        public static ContentManager Content
        {
            get;
            set;
        }

        public void LoadSong(int id)
        {
            _currentSong = Content.Load<Song>(_musicPath + id);
        }

        public void SetVolume(float volume)
        {
            volume = Math.Min(1.0f, volume);
            volume = Math.Max(0.0f, volume);
            _shouldApplyVolume = true;
            _lastRequestedVolume = volume;
        }

        private object _volumeLock = new object();
        private float _lastRequestedVolume = 1.0f;
        private bool _shouldApplyVolume = false;
        private const float _volumeTime = 0.1f;
        private float _currentVolumeTime = 0.0f;

        public void Update(float gameTime)
        {
            if (_currentVolumeTime <= _volumeTime)
                _currentVolumeTime += gameTime;

            if (_shouldApplyVolume)
            {
                if (_currentVolumeTime > _volumeTime)
                {
                    _currentVolumeTime = 0.0f;

                    ApplyVolume();
                }
            }
        }

        private void ApplyVolume()
        {
            if (IsPlaying)
            {
                _shouldApplyVolume = false;
#if !NO_THREADING
                lock (_volumeLock)
#endif
                {
                    try
                    {
                        MediaPlayer.Volume = _lastRequestedVolume * SettingsManager.Instance.MusicVolume;
                    }
                    catch (Exception)
                    {
                        // workaround to silently catch NullReferenceException & SharpDXException
                        // issue: https://github.com/mono/MonoGame/issues/4298
                    }
                }
            }
        }

        public void Play(float volume = -1.0f)
        {
            if (!IsPlaying)
            {
                if (volume >= 0.0f)
                    SetVolume(volume);
#if !NO_THREADING
                lock (_volumeLock)
#endif
                {
                    MediaPlayer.Play(_currentSong);
                    MediaPlayer.IsRepeating = true;
                }
                IsPlaying = true;
            }
        }

        public void Stop()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                MediaPlayer.Stop();
            }
        }

        public void Unload()
        {
            Stop();
            Content.Unload();
            _currentSong = null;
        }
    }
}
