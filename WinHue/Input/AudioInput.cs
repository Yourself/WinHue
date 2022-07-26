using System;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using WinHue.Framework;

namespace WinHue.Input
{
    internal sealed class AudioInput : IInput, IDisposable
    {
        [Configurable]
        public string? AudioDeviceID { get; set; }

        [Configurable]
        public int BufferLengthMS { get; set; } = 15;

        public float Volume { get; private set; }

        public float[] AudioData { get => mFft.Resullt; }

        public void Dispose()
        {
            ((IInput)this).Deactivate();
        }

        private class AudioHandler : IDisposable
        {
            public delegate void UseBufferDelegate(short[] samples, int sampleOffset);

            public AudioHandler(AudioInput parent, MMDevice device)
            {
                Device = device;
                Stream = new WasapiCapture(Device, false, parent.BufferLengthMS)
                {
                    WaveFormat = Device.AudioClient.MixFormat,
                };
                Stream.DataAvailable += OnAudioDataAvailable;
                Stream.StartRecording();
            }

            /// <summary>
            /// Called to use the internal audio buffer for computations in a thread safe way.
            /// </summary>
            /// 
            /// <param name="handler">A handler delegate that will be provided with the internal buffer and offset.</param>
            public void UseBuffer(UseBufferDelegate handler)
            {
                lock (mBuffer)
                {
                    handler(mBuffer, mBufferOffset);
                }
            }

            public void Dispose()
            {
                Stream.StopRecording();
                Stream.Dispose();
                Device.Dispose();
            }

            public MMDevice Device { get; }
            public WasapiCapture Stream { get; }

            private void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
            {
                // Generally this handler will be called on a separate thread. For concurrency reasons the only work
                // that should be done here is updating the ring buffer. Any other computation should be handled in
                // the input Update method.
                lock (mBuffer)
                {
                    // If, for some reason, we've read in more than the total size of the buffer, initialize things so
                    // we only read the last mBuffer.Length samples
                    int newSamples = Math.Min(mBuffer.Length, e.BytesRecorded / sizeof(short));
                    int samplesRead = e.BytesRecorded / sizeof(short) - newSamples;
                    if (newSamples == mBuffer.Length)
                    {
                        // If we're reading the whole buffer in one go, just reset the buffer offset to 0 so we can do
                        // this all in one copy instead of 2
                        mBufferOffset = 0;
                    }

                    if (newSamples + mBufferOffset >= mBuffer.Length)
                    {
                        int samplesToRead = mBuffer.Length - mBufferOffset;
                        Buffer.BlockCopy(e.Buffer, samplesRead * sizeof(short), mBuffer, mBufferOffset * sizeof(short), samplesToRead * sizeof(short));
                        newSamples -= samplesToRead;
                        samplesRead += samplesToRead;
                        mBufferOffset = 0;
                    }

                    Buffer.BlockCopy(e.Buffer, samplesRead * sizeof(short), mBuffer, mBufferOffset * sizeof(short), newSamples * sizeof(short));
                    mBufferOffset += newSamples;
                }
            }

            private readonly short[] mBuffer = new short[FFTSize];
            private int mBufferOffset = 0;
        }

        private class FFT
        {
            public void Compute(short[] samples, int offset = 0)
            {
                for (int i = 0; i < FFTSize; ++i)
                {
                    mData[i].X = (float)(samples[(i + offset) % FFTSize] * FastFourierTransform.BlackmannHarrisWindow(i, FFTSize));
                    mData[i].Y = 0;
                }
                FastFourierTransform.FFT(true, FFTLogSize, mData);

                float maxBinValue = 0;
                for (int i = 0; i < FFTSize; ++i)
                {
                    Resullt[i] = (float)Math.Sqrt(mData[i].X * mData[i].X + mData[i].Y * mData[i].Y);
                    maxBinValue = Math.Max(maxBinValue, Resullt[i]);
                }

                for (int i = 0; i < FFTSize; ++i)
                {
                    Resullt[i] /= maxBinValue;
                }
            }

            public float[] Resullt { get; } = new float[FFTSize];

            private readonly Complex[] mData = new Complex[FFTSize];
        }

        private const int FFTLogSize = 16;
        private const int FFTSize = 1 << FFTLogSize;

        private readonly FFT mFft = new();

        private bool mIsActive;
        private AudioHandler? mAudioHandler;
        void IInput.Activate()
        {
            if (mIsActive) return;
            mIsActive = true;

            if (AudioDeviceID == null)
            {
                throw new InvalidOperationException("AudioDeviceID was not set.");
            }

            var device = new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                .FirstOrDefault((d) => d.ID == AudioDeviceID);

            if (device == null)
            {
                throw new InvalidOperationException($"Could not find audio device with ID: `{AudioDeviceID}`");
            }

            mAudioHandler = new AudioHandler(this, device);
        }

        void IInput.Deactivate()
        {
            if (!mIsActive) return;
            mIsActive = false;
            mAudioHandler?.Dispose();
            mAudioHandler = null;
        }

        void IInput.Update()
        {
            if (mAudioHandler == null) return;

            Volume = mAudioHandler.Device.AudioMeterInformation.MasterPeakValue;

            mAudioHandler.UseBuffer(mFft.Compute);
        }

        bool IInput.AlwaysActive => true;
    }
}
