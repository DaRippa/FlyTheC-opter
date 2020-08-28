using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;


namespace FlyTheCopter.models
{
    public class Game
    {
        public Copter Copter { get; } = new Copter(30);

        public bool Running { get; private set; }
        public event Action<int> FpsChanged;
        public event Action<long> Step;
        public event Action<bool> GameEnded;

        public Size CanvasSize;
        private double _lastFrameEnd;
        private long _frameCounter = 0L;
        private long _scrollOffset = 0L;
        private Random _rand = new Random(123456);

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

        private double GetObstacleYPosition(double xOffset) => 180 + Math.Sin(xOffset / 79) * 40 + Math.Cos(xOffset / 43) * 52;

        private Rect GetRectangle(double xOffset)
        {
            xOffset += (_rand.NextDouble() + 0.75) * 200;

            return new Rect {Height = 60 + _rand.NextDouble() * 40, Width = 30, X = xOffset, Y = GetObstacleYPosition(xOffset)};
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
                var point = new Point(i, 100 + Terrain.GetUpperPathCoord(offset + i));
                var dist = (Copter.Position - point).Length;

                hitUpper |= dist < 10;

                point.Y = CanvasSize.Height - 120 + Terrain.GetLowerPathCoord(i + offset);
                dist = (Copter.Position - point).Length;
                hitLower |= dist < 10;
            }

            return hitUpper || hitLower;
        }
    }
}
