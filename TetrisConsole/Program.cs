using System;
using System.Collections.Generic;
using System.Linq;

namespace TetrisConsole
{
    class Program
    {
        // aktualnie wybrany bloczek
        private static BlockType Btype;

        // do przechowania warinatów można użyć słownika
        static Dictionary<BlockType, int[,]> _blocks;
        private static bool isPaused;
        private static bool isRunning;
        private static int score = 0;
        private const int BLOCK_SIZE = 4;

        private const int FIELD_COLUMNS = 10;
        private const int FIELD_ROWS = 21;
        private static bool[,] _field = new bool[FIELD_ROWS, FIELD_COLUMNS];

        private const int CANVAS_COLUMNS = 40;
        private const int CANVAS_ROWS = 25;

        private static BlockType _currentBlockType;
        private static int _currentBlockRow;
        private static int _currentBlockCol;


        public static int GameTime { get; private set; }

        static void Main(string[] args)
        {
            Initialize();       // inicjalizjuj grę (zmienne)
            GenerateNewBlock(); // wyznacz początkową pozycję nowego losowego bloku
            DrawCanvas();       // wyrysuj planszę
            DrawBlock();        // wyrysuj aktualny blok

            while (isRunning)
            {
                

                if (GameTime > 500 && !isPaused)
                {
                    GameTime = 0;

                    // BlockCanMove - czy doszło do "kolizji" zależnie od aktualnego kierunku
                    if (BlockCanMove(MoveDir.Down))
                    {
                        DrawCanvas();
                        MoveBlock(MoveDir.Down); // zmodyfikuj pozycję aktualnego bloczka
                        DrawBlock(); // wyrysuj aktualny blok
                    }
                    else
                    {
                        score += 10;
                        EmbedBlock(); // doszło do koliji, osadź aktualny blok 
                        var fullLines = CheckForFullLine();
                        
                        while(fullLines.Any()) // pełna linia znika
                        {
                            DeleteFullRows(fullLines);
                            fullLines = CheckForFullLine();
                        }
                        GenerateNewBlock(); // wyznacz początkową pozycję nowego losowego bloku
                        if (!BlockCanMove(MoveDir.Down))
                        {
                            break; // End of the game - new block cannot move
                        }

                        DrawCanvas(); // wyrysuj planszę
                        DrawBlock(); 
                    }
                }

                while (Console.KeyAvailable) 
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.DownArrow:
                            GameTime = 1000;
                            break;

                        case ConsoleKey.LeftArrow:
                            if (BlockCanMove(MoveDir.Left))
                            {
                                MoveBlock(MoveDir.Left);
                                DrawCanvas();
                                DrawBlock();
                            }
                            break;

                        case ConsoleKey.RightArrow:
                            if (BlockCanMove(MoveDir.Right))
                            {
                                MoveBlock(MoveDir.Right);
                                DrawCanvas();
                                DrawBlock();
                            }
                            break;

                        case ConsoleKey.UpArrow:
                            TransformBlock();
                            DrawCanvas();
                            DrawBlock();
                            break;

                        case ConsoleKey.Escape:
                            isRunning = false;
                            break;

                        case ConsoleKey.Spacebar:
                            isPaused = !isPaused;
                            break;
                    }
                }

                System.Threading.Thread.Sleep(16);
                GameTime += 16;
            }

            // koniec gry        }
        }

        private static void DeleteFullRows(List<int> fullLines)
        {
            foreach (var row in fullLines)
            {
                for (int j = 0; j < FIELD_COLUMNS; j++)
                {
                    _field[row, j] = false;
                }

                for (int i = row; i > 0; i--)
                {
                    for (int j = 0; j < FIELD_COLUMNS; j++)
                    {
                        _field[i, j] = _field[i - 1, j];
                    }
                }
            }
        }

        private static List<int> CheckForFullLine()
        {
            List<int> rowsToClean = new List<int>();
            for (int i = 0; i < FIELD_ROWS; i++)
            {
                var rowSum = 0;
                for (int j = 0; j < FIELD_COLUMNS; j++)
                {
                    if (_field[i, j] == true)
                        rowSum++;
                }
                if (rowSum >= FIELD_COLUMNS)
                    rowsToClean.Add(i);                
            }
            return rowsToClean;
        }

        private static void TransformBlock()
        {
            int currentBlockTypeInt = (int)_currentBlockType;

            if (currentBlockTypeInt + 1 > 4)
                currentBlockTypeInt = 1;
            else
                currentBlockTypeInt++;

            _currentBlockType = (BlockType)currentBlockTypeInt;
        }

        private static void EmbedBlock()
        {
            var blockArray = _blocks[_currentBlockType];
            for (int i = 0; i < BLOCK_SIZE; i++)
            {
                for (int j = 0; j < BLOCK_SIZE; j++)
                {
                    if (blockArray[i, j] == 1)
                    {
                        if (_currentBlockRow + i > FIELD_ROWS - 1 || _currentBlockCol + j > FIELD_COLUMNS - 1)
                            continue;

                        _field[_currentBlockRow + i, _currentBlockCol + j] = true;
                    }
                }
            }
        }

        private static void MoveBlock(MoveDir direction)
        {
            switch (direction)
            {
                case MoveDir.Down:
                    _currentBlockRow++;
                    break;

                case MoveDir.Left:
                    _currentBlockCol--;
                    break;

                case MoveDir.Right:
                    _currentBlockCol++;
                    break;

                default:
                    break;
            }
        }

        private static bool BlockCanMove(MoveDir direction)
        {
            switch (direction)
            {
                case MoveDir.Left:
                    if (_currentBlockCol - 1 < 0)
                        return false;

                    var blockArrayLeft = _blocks[_currentBlockType];
                    for (int i = 0; i < BLOCK_SIZE; i++)
                    {
                        for (int j = 0; j < BLOCK_SIZE; j++)
                        {
                            if(blockArrayLeft[i,j] == 1)
                            {
                                if (_field[_currentBlockRow + i, _currentBlockCol + j - 1])
                                    return false;
                            }
                        }
                    }

                    return true;

                case MoveDir.Right:

                    var farRightColumn = 0;
                    var blockArrayRight = _blocks[_currentBlockType];
                    for (int i = 0; i < BLOCK_SIZE; i++)
                    {
                        for (int j = 0; j < BLOCK_SIZE; j++)
                        {
                            if (blockArrayRight[i, j] == 1)
                            {
                                if (j > farRightColumn)
                                    farRightColumn = j;                                
                            }
                        }
                    }

                    if (_currentBlockCol + 1 + farRightColumn > FIELD_COLUMNS - 1)
                        return false;

                    for (int i = 0; i < BLOCK_SIZE; i++)
                    {
                        for (int j = 0; j < BLOCK_SIZE; j++)
                        {
                            if (blockArrayRight[i, j] == 1)
                            {
                                if (j > farRightColumn)
                                    farRightColumn = j;

                                if (_currentBlockCol + j + 1 > FIELD_COLUMNS - 1)
                                    continue;

                                if (_field[_currentBlockRow + i, _currentBlockCol + j + 1])
                                    return false;
                            }
                        }
                    }

                    

                    return true;

                case MoveDir.Down:
                    if (_currentBlockRow + BLOCK_SIZE + 1 > FIELD_ROWS)
                        return false;

                    var blockArrayDown = _blocks[_currentBlockType];
                    for (int i = 0; i < BLOCK_SIZE; i++)
                    {
                        for (int j = 0; j < BLOCK_SIZE; j++)
                        {
                            if (blockArrayDown[i, j] == 1)
                            {
                                if (_field[_currentBlockRow + i + 1, _currentBlockCol + j])
                                    return false;
                            }
                        }
                    }

                    return true;
                default:
                    return true;                    
            }
        }

        private static void DrawBlock()
        {
            Console.SetCursorPosition(_currentBlockCol, _currentBlockRow);
            var blockArray = _blocks[_currentBlockType];
            for (int i = 0; i < BLOCK_SIZE; i++)
            {
                for (int j = 0; j < BLOCK_SIZE; j++)
                {
                    if(blockArray[i,j] == 1)
                    {
                        Console.SetCursorPosition(_currentBlockCol + j, _currentBlockRow + i);
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.Write(" ");
                    }
                }
            }
        }

        private static void GenerateNewBlock()
        {
            var random = new Random();
            _currentBlockType = (BlockType) random.Next(1, 5);
            _currentBlockRow = 0;
            _currentBlockCol = 0;
        }

        private static void DrawCanvas()
        {
            DrawField();
            DrawScore();
        }

        private static void DrawScore()
        {
            Console.SetCursorPosition(13, 0);
            Console.Write($"Score : {score}");
        }

        private static void DrawField()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < FIELD_ROWS; i++)
            {
                for (int j = 0; j < FIELD_COLUMNS; j++)
                {
                    if (_field[i,j] == true)                    
                        Console.BackgroundColor = ConsoleColor.DarkBlue;                    
                    else
                        Console.BackgroundColor = ConsoleColor.Gray;

                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }

        // w metodzie intialize
        private static void Initialize()
        {
            Console.CursorVisible = false;
            Console.WindowHeight = CANVAS_ROWS;
            Console.WindowWidth = CANVAS_COLUMNS;
            Console.BufferHeight = CANVAS_ROWS;
            Console.BufferWidth = CANVAS_COLUMNS;

            isRunning = true;            

            _blocks = new Dictionary<BlockType, int[,]>();

            // T1
            _blocks.Add(BlockType.T1, new int[BLOCK_SIZE, BLOCK_SIZE]
                       {
                   {0, 0, 0, 0},
                   {0, 0, 0, 0},
                   {0, 1, 0, 0},
                   {1, 1, 1, 0}
                       });

            // T2
            _blocks.Add(BlockType.T2, new int[BLOCK_SIZE, BLOCK_SIZE]
                       {
                   {0, 0, 0, 0},
                   {1, 0, 0, 0},
                   {1, 1, 0, 0},
                   {1, 0, 0, 0}
                       });

            // T3
            _blocks.Add(BlockType.T3, new int[BLOCK_SIZE, BLOCK_SIZE]
                       {
                   {0, 0, 0, 0},
                   {0, 0, 0, 0},
                   {1, 1, 1, 0},
                   {0, 1, 0, 0}
                       });

            // T4
            _blocks.Add(BlockType.T4, new int[BLOCK_SIZE, BLOCK_SIZE]
                       {
                   {0, 0, 0, 0},
                   {0, 1, 0, 0},
                   {1, 1, 0, 0},
                   {0, 1, 0, 0}
                       });

        }
    }

    enum MoveDir
    {
        Left,
        Right,
        Down
    }

    // warianty bloków T (każdy obrót)
    enum BlockType
    {
        T1 = 1, 
        T2 = 2, 
        T3 = 3, 
        T4 = 4,
    }
}


    







