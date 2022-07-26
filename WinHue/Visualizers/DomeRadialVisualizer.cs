using System;
using WinHue.Framework;
using WinHue.Input;
using WinHue.LED;
using WinHue.Utilities;

namespace WinHue.Visualizers
{
    internal class DomeRadialVisualizer : IVisualizer
    {
        public enum Mode { Radar, Pulse, Spiral, Bubble };

        [Configurable]
        public Mode Animation { get; set; } = Mode.Radar;

        [Configurable]
        public double RotationSpeed { get; set; } = 1;

        [Configurable]
        public double GradientSpeed { get; set; } = 1;

        [Configurable]
        public double CenterSpeed { get; set; } = 1;

        [Configurable]
        public double RadialFrequency { get; set; } = 4;

        public int Priority => 1;

        public DomeRadialVisualizer(ClockInput clock, AudioInput audioInput, DomeOutput dome)
        {
            mClock = clock;
            mAudioInput = audioInput;
            mDome = dome;
        }

        public void Update()
        {
            mCurrentAngle = Wrap(mCurrentAngle + mClock.Delta * RotationSpeed * 0.25, 0, 1);
            mCurrentGradient = Wrap(mCurrentGradient + mClock.Delta * GradientSpeed, 0, 1);
            mCurrentCenterAngle = Wrap(mCurrentCenterAngle + mClock.Delta * CenterSpeed * 0.25, 0, 1);

            foreach (var pixel in mDome.Pixels)
            {
                var (r, azimuth) = Projection.GetAzimuthalEquidistant(pixel.Position);
                double v = Wrap(Normalize(azimuth, -Math.PI, Math.PI));
                var (u, du) = GetAnimationParameters(v, r);
            }
        }

        private static double Wrap(double x) { return Wrap(x, 0, 1); }

        private static double Wrap(double x, double min, double max)
        {
            double t = (x - min) % (max - min);
            return t < 0 ? max + t : min + t;
        }

        private static double Normalize(double x, double min, double max)
        {
            return (x - min) / (max - min);
        }

        private (double Value, double Gradient) GetAnimationParameters(double angle, double dist)
        {
            double u, du;
            switch (Animation)
            {
                case Mode.Radar:
                    u = Wrap(angle - mCurrentAngle);
                    du = dist;
                    break;

                case Mode.Pulse:
                    u = Wrap(dist - mCurrentAngle);
                    du = Math.Abs(2 * angle - 1);
                    break;

                case Mode.Spiral:
                    u = Wrap(angle + dist / RadialFrequency - mCurrentAngle);
                    du = dist;
                    break;

                case Mode.Bubble:
                    u = Wrap(angle - mCurrentAngle);
                    du = dist;
                    break;

                default:
                    u = 0;
                    du = 0;
                    break;
            }

            return (Math.Abs(2 * Wrap(u * RadialFrequency) - 1), du);
        }

        private readonly ClockInput mClock;
        private readonly AudioInput mAudioInput;
        private readonly DomeOutput mDome;

        private double mCurrentAngle;
        private double mCurrentGradient;
        private double mCurrentCenterAngle;
    }
}
