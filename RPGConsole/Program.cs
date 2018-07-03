using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPGConsole
{
    class Program
    {
        private static Thread InputThread { get; set; }
        private static int Rows { get; set; }
        private static int Cols { get; set; }
        private static int Cells { get; set; }

        private static VecC Player { get; set; }
        private static VecC Food { get; set; }

        private static int OrigRow { get; set; }
        private static int OrigCol { get; set; }

        private static bool Running { get; set; }

        private static string blankChar = " ";
        private static string playerChar = "#";
        private static string foodChar = "*";
        private static bool wrapPlayer = true;

        private static ConsoleColor defaultCharColour = ConsoleColor.White;

        /// <summary>
        /// Main entry point 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Setup();

            Stopwatch stopwatch = new Stopwatch();
            int frame = 0;

            Debug.WriteLine($"Cols\t {Cols}");
                Debug.WriteLine($"Rows\t {Rows}");

            while (Running)
            {
                stopwatch.Start();

                Update();
                DrawGrid();

                stopwatch.Stop();

                frame++;

                Debug.WriteLine($"---------------------------------------");
                Debug.WriteLine($"Time:\t {stopwatch.ElapsedMilliseconds}");
                Debug.WriteLine($"Frame:\t {frame}");                
                Debug.WriteLine($"Player\t [x:{Player.x} y:{Player.y}]");
                Debug.WriteLine($"Food\t [x:{Food.x} y:{Food.y}]");

                if (frame == 60)
                {
                    frame = 0;
                }

                stopwatch.Reset();
                Thread.Sleep(10);
            }

            Console.Clear();
            Console.WriteLine("Finished. Press Return to exit.");
            Console.ReadKey();
        }

        private static void Setup()
        {

            Console.WindowHeight = 40;
            Console.WindowWidth = 40;

            Rows = Console.WindowHeight - 0;
            Cols = Console.WindowWidth - 1;

            if (Player == null) Player = new VecC(Cols / 2, Rows / 2);
            if (Food == null) Food = VecC.GetRandomVector();

            Cells = Cols * Rows;

            // Start the input thread
            InputThread = new Thread(() => GetUserInput());
            InputThread.IsBackground = true;
            InputThread.Start();

            Running = true;

            DrawGrid();
        }

        private static void GetUserInput()
        {
            var input = Console.ReadKey(true);
            //Debug.WriteLine(input.Key);

            switch (input.Key)
            {
                case ConsoleKey.UpArrow:
                    Player.velocity = VecC.Velocity.NORTH;
                    break;

                case ConsoleKey.DownArrow:
                    Player.velocity = VecC.Velocity.SOUTH;
                    break;

                case ConsoleKey.LeftArrow:
                    Player.velocity = VecC.Velocity.WEST;
                    break;

                case ConsoleKey.RightArrow:
                    Player.velocity = VecC.Velocity.EAST;
                    break;

                case ConsoleKey.Escape:
                    Running = false;
                    break;

                default:
                    break;
            }

            GetUserInput();
        }

        private static void Update()
        {

            Player.UpdatePosition();
            Player.CheckWallCollision();

            if (Player.HasColliedWith(Food))
            {
                Food = VecC.GetRandomVector();
            }

        }

        private static void DrawGrid()
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    VecC vec = new VecC(x, y);
                    // Draw `+` if cursor in any corner.
                    if ((x == 0 && y == 0) || (x == Cols - 1 && y == 0) || (x == Cols - 1 && y == Rows - 1) || (x == 0 && y == Rows - 1))
                    {
                        WriteAt("+", x, y);

                    }
                    // Draw `|` if left or right side
                    else if (x == 0 || x == Cols - 1)
                    {
                        WriteAt("|", x, y);
                    }
                    // Draw `-` if top or bottom row
                    else if (y == 0 || y == Rows - 1)
                    {
                        WriteAt("-", x, y);

                    }
                    // Draw the player
                    else if (vec.x == Player.x && vec.y == Player.y)
                    {
                        WriteAt(playerChar, x, y, ConsoleColor.Gray);
                    }
                    // Draw the food
                    else if (vec.x == Food.x && vec.y == Food.y)
                    {
                        WriteAt(foodChar, x, y, ConsoleColor.Green);
                    }
                    // Draw whitespace
                    else
                    {
                        WriteAt(blankChar, x, y);
                    }
                }
            }

        }


        protected static void WriteAt(string s, int x, int y, ConsoleColor colour = ConsoleColor.White)
        {
            OrigRow = 0;
            OrigCol = 0;

            //Console.ForegroundColor = color;

            try
            {
                Console.SetCursorPosition(OrigCol + x, OrigRow + y);

                if (colour != ConsoleColor.White) Console.ForegroundColor = colour;

                Console.Write(s);

                if (colour != ConsoleColor.White) Console.ForegroundColor = ConsoleColor.White;

            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }

            //Console.ForegroundColor = ConsoleColor.White;

        }


        class VecC
        {
            public int x { get; set; }
            public int y { get; set; }
            public Velocity velocity { get; set; }

            public enum Velocity
            {
                NONE, NORTH, SOUTH, WEST, EAST
            }

            public VecC(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public bool HasColliedWith(VecC vec)
            {
                return (x == vec.x && y == vec.y);
            }

            public void UpdatePosition()
            {
                switch (velocity)
                {
                    case Velocity.NORTH:
                        Player.y -= 1;
                        break;
                    case Velocity.SOUTH:
                        Player.y += 1;
                        break;
                    case Velocity.WEST:
                        Player.x -= 1;
                        break;
                    case Velocity.EAST:
                        Player.x += 1;
                        break;
                    case Velocity.NONE:
                        Player.x += 0;
                        Player.y += 0;
                        break;
                    default:
                        break;
                }
            }

            public void CheckWallCollision()
            {
                if (Player.x <= 0) Player.x = wrapPlayer ? Cols - 2 : 2;
                if (Player.x >= Cols - 1) Player.x = wrapPlayer ? 2 : Cols - 1;

                if (Player.y <= 0) Player.y = wrapPlayer ? Rows - 2 : 1;
                if (Player.y >= Rows - 1) Player.y = wrapPlayer ? 1 : Rows - 2;
            }

            public static VecC GetRandomVector()
            {
                return new VecC(new Random().Next(1, Cols - 1), new Random().Next(1, Rows - 1));
            }
        }
    }
}