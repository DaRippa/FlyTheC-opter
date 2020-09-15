using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;


namespace FlyTheCopter.models
{
    public class Game
    {
        public Copter Copter { get; } = new Copter(30, 0);
        public Size CollisionThreshold { get; set; } = new Size(8, 8);

        public bool Running { get; private set; }

        public Size CanvasSize
        {
            get => _canvasSize;
            set
            {
                _canvasSize = value;
                if (!Running)
                    Copter.Position.Y = value.Height / 2;
            }
        }

        public event Action<int> FpsChanged;
        public event Action<long> Step;
        public event Action<bool> GameEnded;


        private double _lastFrameEnd;
        private long _frameCounter = 0L;
        private long _scrollOffset = 0L;
        private Random _rand = new Random(123456);
        private Size _canvasSize;

        public Collection<Rect> Obstacles { get; } = new Collection<Rect>();

        public void Start()
        {
            _scrollOffset = 0;
            Running = true;
            _lastFrameEnd = DateTime.Now.TimeOfDay.TotalMilliseconds;

            CompositionTarget.Rendering += Update;

            InitObstacles();
        }

        public void Stop()
        {
            Running = false;
            CompositionTarget.Rendering -= Update;
            Copter.Reset(CanvasSize.Height / 2);
        }

        private void Update(object? sender, EventArgs e)
        {
            ++_frameCounter;

            var now = DateTime.Now.TimeOfDay.TotalMilliseconds;
            var diff = now - _lastFrameEnd;

            if (_frameCounter % 30 == 0)
                FpsChanged?.Invoke((int) (1000 / diff));

            _lastFrameEnd = now;

            _scrollOffset += (int) (diff / 100 * 15);
            Step?.Invoke(_scrollOffset);

            Copter.Update(diff);
            UpdateObstacles();

            if (CheckCollision(_scrollOffset))
            {
                Stop();
                GameEnded?.Invoke(true);
            }
        }

        private void UpdateObstacles()
        {
            if (Obstacles[0].X - _scrollOffset < -30)
            {
                Obstacles.RemoveAt(0);
                Obstacles.Add(GetRectangle(Obstacles.Last().X));
            }
        }

        private Rect GetRectangle(double xOffset)
        {
            xOffset += (_rand.NextDouble() + 0.75) * 200;
            var f = _rand.NextDouble() / 2.0 + 0.25;
            var space = CanvasSize.Height - Terrain.GetLowerPathCoord(xOffset) - Terrain.GetUpperPathCoord(xOffset);

            return new Rect
            {
                Height = Math.Min(60 + _rand.NextDouble() * 40, space * 0.3), 
                Width = 20, 
                X = xOffset,
                Y = (1.0 - f) * Terrain.GetUpperPathCoord(xOffset) + f * (CanvasSize.Height - Terrain.GetLowerPathCoord(xOffset))
            };
        }

        private void InitObstacles()
        {
            Obstacles.Clear();
            for (var i = 0; i < 5; ++i)
            {
                var offset = 100.0;

                if (i > 0)
                    offset = Obstacles[i - 1].Left;

                Obstacles.Add(GetRectangle(offset));
            }
        }

        private bool CheckCollision(long offset)
        {
            var hitUpper = false;
            var hitLower = false;

            for (var i = 0; i < 60 && !hitUpper && !hitLower; ++i)
            {
                var point = new Point(i, Terrain.GetUpperPathCoord(offset + i));
                var dist = (Copter.Position - point).Length;

                hitUpper |= dist < CollisionThreshold.Height;

                point.Y = CanvasSize.Height - Terrain.GetLowerPathCoord(i + offset);
                dist = (Copter.Position - point).Length;
                hitLower |= dist < CollisionThreshold.Height;
            }

            return hitUpper || hitLower || HitObstacle(offset);
        }

        private bool HitObstacle(in long offset)
        {
            var hit = false;
            
            foreach (var obstacle in Obstacles.Take(2))
            {
                if (Copter.Position.X + CollisionThreshold.Width < obstacle.X - offset || Copter.Position.X - CollisionThreshold.Width > obstacle.X + obstacle.Width - offset) //is left or right of obstacle
                    continue;

                if (Copter.Position.Y - CollisionThreshold.Height > obstacle.Y + obstacle.Height || Copter.Position.Y + CollisionThreshold.Height < obstacle.Y) //is above or below obstacle
                    continue;

                hit = true;
            }

            return hit;
        }
    }
}
