using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int BOARD_WIDTH = 25, BOARD_HEIGHT = 25, BLOCK_SIZE = 20, FPS = 5, STARTING_SNAKE_SIZE = 5;

        enum Directions
        {
            Left,
            Right,
            Up,
            Down
        }
        static readonly Dictionary<Directions, Point> _movementsCollection = new Dictionary<Directions, Point>() {
            { Directions.Left, new Point(-1, 0) },
            { Directions.Right, new Point(1, 0) },
            { Directions.Up, new Point(0, -1) },
            { Directions.Down, new Point(0, 1) },
        };

        BackgroundWorker _thread;
        bool _run = true;

        LinkedList<Point> _snake;
        Point _apple;
        Directions _direction;
        Directions _lastTailsDirection = Directions.Left;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitWindow();
            InitGame();
            Render();
            Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up :
                    SetDirection(Directions.Up);
                    break;
                case Key.Down:
                    SetDirection(Directions.Down);
                    break;
                case Key.Left:
                    SetDirection(Directions.Left);
                    break;
                case Key.Right:
                    SetDirection(Directions.Right);
                    break;
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _run = false;
            _thread.Dispose();
        }

        private void InitWindow()
        {
            canvas.Width = BOARD_WIDTH * BLOCK_SIZE;
            canvas.Height = BOARD_HEIGHT * BLOCK_SIZE;
        }
        private void Start()
        {
            _thread = new BackgroundWorker();
            _thread.DoWork += Loop;
            _thread.RunWorkerAsync();
        }
        private void InitGame()
        {
            Reset();
        }
        private void Loop(object sender, DoWorkEventArgs e)
        {
            while (_run)
            {
                Step();
                canvas.Dispatcher.Invoke(Render);
                Thread.Sleep(1000 / FPS);
            }
        }
        private void Step()
        {
            Point movement = GetMovement(_direction), head = _snake.First.Value;

            var x = head.X + movement.X;
            if (x < 0)
                x = BOARD_WIDTH - 1;
            else if (x >= BOARD_WIDTH)
                x = 0;
            var y = head.Y + movement.Y;
            if (y < 0)
                y = BOARD_WIDTH - 1;
            else if (y >= BOARD_WIDTH)
                y = 0;
            Point newPoint = new Point(x, y);
            if (IsPartOfSnake(newPoint))
            {
                Reset();
                return;
            }
            _snake.AddFirst(newPoint);
            if (_snake.First.Value.Equals(_apple))
                GenerateNewFruit();
            else _snake.RemoveLast();
        }
        private void Render()
        {
            canvas.Children.Clear();
            DrawApple(_apple);
            foreach (var part in _snake)
            {
                bool isHead = part == _snake.First.Value, isTail = part == _snake.Last.Value;
                if (isHead)
                    DrawHead(part, _direction);
                else if (isTail)
                {
                    var tailDirection = GetTailsDirection(part, _snake.Last.Previous.Value);
                    DrawTail(part, tailDirection);
                }
                else DrawBody(part, _direction);
            }
            DrawScore(_snake.Count - STARTING_SNAKE_SIZE);
        }
        private void Reset()
        {
            _snake = new LinkedList<Point>();
            for (int i = 0; i < STARTING_SNAKE_SIZE; i++) {
                _snake.AddLast(new Point(BOARD_WIDTH / 2 - i, BOARD_HEIGHT / 2));
            }
            GenerateNewFruit();
            _direction = Directions.Right;
        }
        private void GenerateNewFruit()
        {
            Random rdm = new Random();
            _apple.X = rdm.Next(BOARD_WIDTH - 1);
            _apple.Y = rdm.Next(BOARD_HEIGHT - 1);
            while (IsPartOfSnake(_apple))
            {
                _apple.X = rdm.Next(BOARD_WIDTH - 1);
                _apple.Y = rdm.Next(BOARD_HEIGHT - 1);
            }
        }
        private bool IsPartOfSnake(Point point)
        {
            foreach (var part in _snake)
                if (part.Equals(point))
                    return true;
            return false;
        }
        private void DrawApple(Point position)
        {
            Image imageControl = new Image();
            imageControl.Source = Convert(Properties.Resources.Apple);
            imageControl.Width = BLOCK_SIZE;
            imageControl.Height = BLOCK_SIZE;
            Canvas.SetLeft(imageControl, position.X * BLOCK_SIZE);
            Canvas.SetTop(imageControl, position.Y * BLOCK_SIZE);
            canvas.Children.Add(imageControl);
        }
        private void DrawHead(Point position, Directions direction)
        {
            DrawBlock(Colors.Green, position);
            var eyes = new[] { new Ellipse(), new Ellipse() };
            foreach (var eye in eyes)
            {
                eye.Fill = new SolidColorBrush(Colors.Black);
                eye.Width = BLOCK_SIZE / 4;
                eye.Height = BLOCK_SIZE / 4;
                int x = 0, y = 0, eyeStartRangeW = (int)(BLOCK_SIZE - eye.Width * 2) / 4, eyeStartRangeH = (int)(BLOCK_SIZE - eye.Width * 2) / 4;
                bool firstEye = eyes[0] == eye;
                switch (direction)
                {
                    case Directions.Left:
                        x = 0;
                        y = (int)(firstEye ? eyeStartRangeH : BLOCK_SIZE - eye.Height - eyeStartRangeH);
                        break;
                    case Directions.Right:
                        x = (int)(BLOCK_SIZE - eye.Width);
                        y = (int)(firstEye ? eyeStartRangeH : BLOCK_SIZE - eye.Height - eyeStartRangeH);
                        break;
                    case Directions.Up:
                        x = (int)(firstEye ? eyeStartRangeW : BLOCK_SIZE - eye.Width - eyeStartRangeW);
                        y = 0;
                        break;
                    case Directions.Down:
                        x = (int)(firstEye ? eyeStartRangeW : BLOCK_SIZE - eye.Width - eyeStartRangeW);
                        y = (int)(BLOCK_SIZE - eye.Height);
                        break;
                }
                Canvas.SetLeft(eye, position.X * BLOCK_SIZE + x);
                Canvas.SetTop(eye, position.Y * BLOCK_SIZE + y);
                canvas.Children.Add(eye);
            }
        }
        private void DrawBody(Point position, Directions direction)
        {
            DrawBlock(Colors.LightGreen, position);
        }
        private void DrawTail(Point position, Directions direction)
        {
            var polygon = new Polygon();
            polygon.Fill = new SolidColorBrush(Colors.LightGreen);
            polygon.Width = BLOCK_SIZE;
            polygon.Height = BLOCK_SIZE;
            switch (direction)
            {
                case Directions.Left:
                    polygon.Points = new PointCollection(new[] { new Point(0, 0), new Point(0, BLOCK_SIZE), new Point(BLOCK_SIZE, 0.5 * BLOCK_SIZE) });
                    break;
                case Directions.Right:
                    polygon.Points = new PointCollection(new[] { new Point(BLOCK_SIZE, 0), new Point(BLOCK_SIZE, BLOCK_SIZE), new Point(0, 0.5 * BLOCK_SIZE) });
                    break;
                case Directions.Up:
                    polygon.Points = new PointCollection(new[] { new Point(0, 0), new Point(BLOCK_SIZE, 0), new Point(0.5 * BLOCK_SIZE, BLOCK_SIZE) });
                    break;
                case Directions.Down:
                    polygon.Points = new PointCollection(new[] { new Point(0, BLOCK_SIZE), new Point(BLOCK_SIZE, BLOCK_SIZE), new Point(0.5 * BLOCK_SIZE, 0) });
                    break;
            }
            Canvas.SetLeft(polygon, position.X * BLOCK_SIZE);
            Canvas.SetTop(polygon, position.Y * BLOCK_SIZE);
            canvas.Children.Add(polygon);
        }
        private void DrawBlock(Color color, Point position)
        {
            var block = new Rectangle();
            block.Fill = new SolidColorBrush(color);
            block.Width = BLOCK_SIZE;
            block.Height = BLOCK_SIZE;
            Canvas.SetLeft(block, position.X * BLOCK_SIZE);
            Canvas.SetTop(block, position.Y * BLOCK_SIZE);
            canvas.Children.Add(block);
        }
        private void DrawScore(int score)
        {
            scoreboard.Content = score.ToString("00");
        }
        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
        private void SetDirection(Directions newDirection)
        {
            if (_direction != newDirection)
                switch (newDirection)
                {
                    case Directions.Left :
                    case Directions.Right when _direction != Directions.Left:
                    case Directions.Up when _direction != Directions.Down:
                    case Directions.Down when _direction != Directions.Up:
                        _direction = newDirection;
                        break;
                }
        }
        private Point GetMovement(Directions direction)
        {
            Point movement = _movementsCollection[direction];
            return movement;
        }
        private Directions GetTailsDirection(Point tail, Point previous)
        {
            var movement = (Point)(previous - tail);
            var movementNode = _movementsCollection.Where(mc => mc.Value == movement).DefaultIfEmpty(new KeyValuePair<Directions, Point>(_lastTailsDirection, new Point())).First();
            var direction = movementNode.Key;
            _lastTailsDirection = direction;
            return direction;
        }
    }
}
