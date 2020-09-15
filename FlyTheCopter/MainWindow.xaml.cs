using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FlyTheCopter.models;

namespace FlyTheCopter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Size _windowSize;
        private Game _game = new Game();
        private readonly StringBuilder _upperPathBuilder = new StringBuilder();
        private readonly StringBuilder _lowerPathBuilder = new StringBuilder();

        public IEnumerable<Rect> Obstacles => _game.Obstacles;
        
        public MainWindow()
        {
            InitializeComponent();

            _windowSize = new Size(Width, Height);
            Canvas.SetLeft(TheCopter, _game.Copter.Position.X - TheCopter.Width / 2);

            _game.FpsChanged += v => TbFrames.Text = $"{v:##} FPS";
            _game.Step += GameOnStep;
            _game.GameEnded += b =>
            {
                if (b)
                    DpStart.Visibility = Visibility.Visible;
            };
        }

        private void GameOnStep(long scrollOffset)
        {
            UpdateTerrain(scrollOffset);
            UpdateObstacles(scrollOffset);

            Canvas.SetTop(TheCopter, _game.Copter.Position.Y - TheCopter.Height / 2);

            RunDistance.Text = scrollOffset.ToString();
            RunScore.Text = (scrollOffset * 1.5).ToString(CultureInfo.InvariantCulture);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowSize = e.NewSize;

            var ratio = 16.0 / 9.0;

            Height = Width / ratio;

            _game.CanvasSize = _windowSize;

            if(!_game.Running)
                Canvas.SetTop(TheCopter, ActualHeight / 2 - 10);
        }

        private void MainCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_game.Running)
            {
                Scoreboard.Visibility = Visibility.Visible;
                StartGame();
            }
            else
                _game.Copter.SteerUp = true;
            
        }

        private void UpdateTerrain(long offset)
        {
            _upperPathBuilder.Clear();
            _upperPathBuilder.Append($"M {_windowSize.Width} 0 L 0 0");
            _lowerPathBuilder.Clear();
            _lowerPathBuilder.Append($"M 0 {_windowSize.Height} L {_windowSize.Width} {_windowSize.Height}");

            for (var i = 0; i <= _windowSize.Width; ++i)
            {
                _upperPathBuilder.Append(
                    $" L {i} {Terrain.GetUpperPathCoord(i + offset).ToString(CultureInfo.InvariantCulture)}");
                _lowerPathBuilder.Append(
                    $" L {_windowSize.Width - i} {(_windowSize.Height - Terrain.GetLowerPathCoord(_windowSize.Width - i + offset)).ToString(CultureInfo.InvariantCulture)}");
            }
            
            UpperPath.Data = Geometry.Parse(_upperPathBuilder.ToString());
            LowerPath.Data = Geometry.Parse(_lowerPathBuilder.ToString());
        }

        private void UpdateObstacles(double offset)
        {
            ObstacleCanvas.Children.Clear();

            foreach (var obstacle in Obstacles)
            {
                var rect = new Rectangle {Height = obstacle.Height, Width = obstacle.Width, Fill = Brushes.GreenYellow};
                ObstacleCanvas.Children.Add(rect);
                Canvas.SetTop(rect, obstacle.Y);
                Canvas.SetLeft(rect, obstacle.X - offset);
            }
        }

        private void StartGame()
        {
            _game.Start();
            DpStart.Visibility = Visibility.Collapsed;
        }

        private void MainCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_game.Running)
                return;

            _game.Copter.SteerUp = false;
        }
    }
}
