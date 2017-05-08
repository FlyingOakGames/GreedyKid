using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Audio;

namespace GreedyKid
{
    public sealed class MicrophoneManager : IDisposable
    {
        private static MicrophoneManager _instance;

        private Microphone _microphone = null;
        private TimeSpan _bufferDuration = TimeSpan.FromMilliseconds(100.0);
        private byte[] _buffer;

        private float _measuredVolume = 0.0f;

        // noise reduction
        private const float _noiseGate = 20.0f; // unused ?
        private int _meanSamples = 0;
        private int _currentMeanSamples = 0;
        private int _volumeMeasures = 0;
        private float _accumulatedVolume = 0.0f;
        private float _currentMean = 0.0f;

        private float _adjustedVolume = 0.0f;

        private bool _started = false;

        public static MicrophoneManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MicrophoneManager();
                return _instance;
            }
        }

        private MicrophoneManager()
        {
            
        }

        public bool Working
        {
            get { return _started; }
        }

        public void SetMicrophone(int selected)
        {
            try
            {
                if (_microphone != null)
                {
                    StopCapture();
                    _microphone.BufferReady -= BufferReady;
                }

                _started = false;

                if (selected >= 0)
                {
                    _microphone = Microphone.All[selected];
                    _microphone.BufferDuration = _bufferDuration;
                    _buffer = new byte[_microphone.GetSampleSizeInBytes(_bufferDuration)];
                    _meanSamples = _microphone.GetSampleSizeInBytes(TimeSpan.FromSeconds(2.0));
                    _microphone.BufferReady += BufferReady;
                    StartCapture();
                }
            }
            catch (Exception)
            {
                _started = false;
            }
        }

        public float RawVolume
        {
            get { return _measuredVolume; }
        }

        public float AdjustedVolume
        {
            get { return _adjustedVolume; }
        }

        public int LeveledVolume
        {
            get { return (int)(_adjustedVolume / 10); }
        }

        public bool HasMicrophone
        {
            get { return _microphone != null && _microphone.State == MicrophoneState.Started; }
        }

        private void StartCapture()
        {
            _measuredVolume = 0.0f;
            _adjustedVolume = 0.0f;

            try
            {
                if (_microphone != null)
                    _microphone.Start();
                _started = true;
            }
            catch (Exception)
            {
                _started = false;
            }
        }

        private void StopCapture()
        {
            _measuredVolume = 0.0f;
            _adjustedVolume = 0.0f;

            try
            {
                if (_microphone != null)
                    _microphone.Stop();
                _started = false;
            }
            catch (Exception)
            {
                _started = false;
            }
        }

        public void Update(float gameTime)
        {            
            _adjustedVolume = (_measuredVolume - _currentMean) / (100.0f - _currentMean);
            _adjustedVolume = Math.Max(0, _adjustedVolume * 100.0f);
        }

        private void BufferReady(object sender, EventArgs e)
        {
            int sampleCount = _microphone.GetData(_buffer);

            // RMS Method
            double rms = 0;
            ushort byte1 = 0;
            ushort byte2 = 0;
            short sample = 0;

            rms = (short)(byte1 | (byte2 << 8));

            for (int i = 0; i < sampleCount - 1; i += 2)
            {
                byte1 = _buffer[i];
                byte2 = _buffer[i + 1];

                sample = (short)(byte1 | (byte2 << 8));
                rms += Math.Pow(sample, 2);
            }

            rms /= sampleCount / 2.0;
            _measuredVolume = (float)Math.Floor(Math.Sqrt(rms));

            if (_measuredVolume > 10000.0f)
            {
                _measuredVolume = 10000.0f;
            }
            _measuredVolume = _measuredVolume / 100.0f;

            // mean
            _accumulatedVolume += _measuredVolume;
            _currentMeanSamples += sampleCount;
            _volumeMeasures++;
            if (_currentMeanSamples >= _meanSamples)
            {
                _currentMean = _accumulatedVolume / _volumeMeasures;

                _currentMeanSamples = 0;
                _volumeMeasures = 0;
                _accumulatedVolume = 0.0f;
            }
        }

        public void Dispose()
        {
            if (_instance != null)
                _instance.StopCapture();
            _instance = null;
        }
    }
}
