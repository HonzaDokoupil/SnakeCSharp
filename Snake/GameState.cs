
namespace Snake
{
    public class GameState
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public GridValue[,] Grid { get; set; }
        public Direction Direction { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        private readonly LinkedList<Direction> directionChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols)
        {
            Rows = rows; 
            Columns = cols;
            Grid = new GridValue[Rows, Columns];
            Direction = Direction.Right;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            for (int i = 1; i <= 3; i++)
            {
                Grid[r, i] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, i));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for(int i = 0;i < Rows; i++)
            {
                for(int j = 0;j < Columns; j++)
                {
                    if (Grid[i, j] == GridValue.Empty)
                    {
                        yield return new Position(i, j);
                    }
                }
            }
        }
        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if(empty.Count == 0)
            {
                return;
            }

            Position positionOne = empty[random.Next(empty.Count)];
            Grid[positionOne.Row, positionOne.Column] = GridValue.Food;
            Position positionTwo = empty[random.Next(empty.Count)];
            while(positionOne == positionTwo)
            {
                positionTwo = empty[random.Next(empty.Count)];
            }
            Grid[positionTwo.Row, positionTwo.Column] = GridValue.Food;
        }
        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }
        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }
        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position position)
        {
            snakePositions.AddFirst(position);
            Grid[position.Row, position.Column] = GridValue.Snake;
        }
        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if(directionChanges.Count == 0)
            {
                return Direction;
            }

            return directionChanges.Last.Value;
        }
        private bool CanChangeDirection(Direction newDirection)
        {
            if(directionChanges.Count == 2)
            {
                return false;
            }

            Direction lastDirection = GetLastDirection();
            return newDirection != lastDirection && newDirection != lastDirection.Opposite();
        }
        public void ChangeDirection(Direction direction)
        {
            if(CanChangeDirection(direction))
            {
                directionChanges.AddLast(direction);
            }
        }

        private bool OutsideGrid(Position position)
        {
            return position.Row < 0 || position.Column < 0 || position.Row >= Rows || position.Column >= Columns;
        }
        private GridValue Colide(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }
            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }
            return Grid[newHeadPos.Row, newHeadPos.Column];
        }
        private bool FoodRemains()
        {
            bool foodRemains = false;
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    if (Grid[i, j] == GridValue.Food)
                    {
                        foodRemains = true;
                    }
                }
            }
            return foodRemains;
        }
        public void Move()
        {
            if (directionChanges.Count > 0)
            {
                Direction = directionChanges.First.Value;
                directionChanges.RemoveFirst();
            }
            Position newHeadPos = HeadPosition().Translate(Direction);
            GridValue colide = Colide(newHeadPos);

            if(colide == GridValue.Snake || colide == GridValue.Outside)
            {
                GameOver = true;
            }
            else if(colide == GridValue.Empty)
            { 
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if(colide == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                if (!FoodRemains())
                {
                    AddFood();
                }
            }
        }
    }
}
