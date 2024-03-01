using System.Text;
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
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food },
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private readonly int rows = 25, cols = 25;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new(rows, cols);
        }
        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new(rows, cols);
        }
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if(!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(gameState.GameOver)
            {
                return;
            }
            switch(e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left); 
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }
        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100);
                gameState.Move();
                Draw();
            }
        }
        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    Image image = new()
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    images[i, j] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }
        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            DrawSnakeTail();
            ScoreText.Text = $"SCORE {gameState.Score}";
        }
        private void DrawGrid()
        {
            for(int i = 0;i < rows;i++)
            {
                for(int j = 0;j < cols; j++)
                {
                    GridValue gridValue = gameState.Grid[i, j];
                    gridImages[i, j].Source = gridValToImage[gridValue];
                    gridImages[i, j].RenderTransform = Transform.Identity;
                }
            }
        }
        private void DrawSnakeHead()
        {
            Position headPosition = gameState.HeadPosition();
            Image image = gridImages[headPosition.Row, headPosition.Column];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Direction];
            image.RenderTransform = new RotateTransform(rotation);
        }
        private void DrawSnakeTail()
        {
            Position tailPosition = gameState.TailPosition();
            Image image = gridImages[tailPosition.Row, tailPosition.Column];
            image.Source = Images.Tail;

            image.RenderTransform = new RotateTransform(RotateTail(tailPosition));
        }
        private int RotateTail(Position tailPosition)
        {
            int rotation = 0;
            if(CheckBeforeTail(tailPosition.Row, tailPosition.Column + 1))
            {
                rotation = 90;               
            }
            else if(CheckBeforeTail(tailPosition.Row + 1, tailPosition.Column))
            {
                rotation = 180;
            }
            else if(CheckBeforeTail(tailPosition.Row, tailPosition.Column - 1))
            {
                rotation = 270;
            }
            return rotation;
        }
        private bool CheckBeforeTail(int row, int column)
        {
            Position beforeTail = gameState.SnakePositions().ToList()[gameState.SnakePositions().ToList().Count - 2];
            return new Position(row, column) == beforeTail;
        }
        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());
            for(int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : (i == positions.Count - 1) ? Images.DeadTail : Images.DeadBody;
                gridImages[pos.Row, pos.Column].Source = source;
                await Task.Delay(50);
            }
        }
        private async Task ShowCountDown()
        {
            for(int i = 3; i >= 0; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }
        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRES ANY KEY TO START";
        }
    }
}