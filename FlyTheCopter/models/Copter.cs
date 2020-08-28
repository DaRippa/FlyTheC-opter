using System;
using System.Windows;

namespace FlyTheCopter.models
{
    public class Copter
    {
        public bool SteerUp { get; set; }
        public double Speed { get; private set; } = 0;

        private const double Acceleration = 0.005;
        private const double MaxSpeed = 2.75;

        private Point _position;
        public Point Position => _position;

        public void Update(double dt)
        {
            Speed = Math.Min(Math.Max(Speed + (SteerUp ? -Acceleration : Acceleration) * dt, -MaxSpeed), MaxSpeed);
            _position.Y += Speed;
        }

        public void Reset(double yOffset)
        {
            Speed = 0;
            _position.Y = yOffset;
        }

        public Copter() : this(0)
        {
        }

        public Copter(double xOffset)
        {
            _position.X = xOffset;
        }
    }

}