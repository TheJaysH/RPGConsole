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
        private static Stopwatch Timer { get; set; }
        private static int Frame { get; set; }
        private static int Rows { get; set; }
        private static int Cols { get; set; }

        private static VecC Player { get; set; }
        private static VecC Food { get; set; }

        private static int OrigRow { get; set; }
        private static int OrigCol { get; set; }

        private static bool Running { get; set; }

        private static int width = 80;
        private static int height = 35;

        private static int playerSpeed = 1;
        private static char playerChar = '#';
        private static char blankChar = ' ';        
        private static char foodChar = '*';
        private static bool wrapPlayer = true;
        private static bool redrawBox = true;
        private static bool debug = false;

        //private static ConsoleColor defaultCharColour = ConsoleColor.White;

        /// <summary>
        /// Main entry point 
        /// </summary>
        /// <param name="args"></param>
        static void Main()
        {
            Setup();

            Frame = 0;

            Timer = new Stopwatch();

            while (Running)
            {
                // Start the frame timer
                Timer.Start();

                // Update vectors
                Update();

                // draw each x y on the grid
                DrawGrid();

                // Stop frame timer
                Timer.Stop();

                // Incrament frames
                Frame++;

                // Display debug info (if set)
                WriteDebug(debug);

                // Reset Frame Time
                Timer.Reset();
                //Thread.Sleep(10);
            }

            Console.Clear();
            Console.WriteLine("Finished. Press Return to exit.");
            Console.ReadKey();
        }

        /// <summary>
        /// Intial setup Method
        /// Will setup console size, and initial vector positions
        /// </summary>
        private static void Setup()
        {
            Console.WindowHeight = height;
            Console.WindowWidth = width;

            Rows = Console.WindowHeight - 0;
            Cols = Console.WindowWidth - 1;

            if (Player == null) Player = new VecC(Cols / 2, Rows / 2);
            if (Food == null) Food = VecC.GetRandomVector();

           
            // Start the input thread
            InputThread = new Thread(() => GetUserInput());
            InputThread.IsBackground = true;
            InputThread.Start();

            Running = true;

            DrawGrid();
        }

        /// <summary>
        /// [THREADED] recurcive method to check for user input
        /// </summary>
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

                case ConsoleKey.R:
                    ResetConsole();
                    break;

                default:
                    break;
            }

            GetUserInput();
        }

        private static void ResetConsole()
        {
            InputThread.Abort();
            Running = false;
            Main();
        }

        /// <summary>
        /// Update Vector Info
        /// </summary>
        private static void Update()
        {
            //Player.RandomWalk();
            Player.UpdatePosition();
            Player.CheckWallCollision();
            //Player.Bounce();

            if (Player.HasColliedWith(Food))
            {                
                Food = VecC.GetRandomVector();                
            }
        }

        /// <summary>
        /// Main Draw Method
        /// Gets total rows & cols. loops through both, and draws chars at each x,y
        /// </summary>
        private static void DrawGrid()
        {
            if (redrawBox && (Console.WindowWidth - 1 != Cols || Console.WindowHeight - 0 != Rows))
            {
                Rows = Console.WindowHeight - 0;
                Cols = Console.WindowWidth - 1;
            }

            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    VecC vec = new VecC(x, y);
                    // Draw `+` if cursor in any corner.
                    if ((x == 0 && y == 0) || (x == Cols - 1 && y == 0) || (x == Cols - 1 && y == Rows - 1) || (x == 0 && y == Rows - 1))
                    {
                        WriteAt('+', x, y);
                    }
                    // Draw `|` if left or right side
                    else if (x == 0 || x == Cols - 1)
                    {
                        WriteAt('|', x, y);
                    }
                    // Draw `-` if top or bottom row
                    else if (y == 0 || y == Rows - 1)
                    {
                        WriteAt('-', x, y);

                    }
                    // Draw the player
                    else if (vec.x == Player.x && vec.y == Player.y)
                    {
                        WriteAt(playerChar, x, y, ConsoleColor.Cyan);
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

        /// <summary>
        /// Write a a specific position on the console
        /// </summary>
        /// <param name="s">Charater to write</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="colour">Charater color</param>
        protected static void WriteAt(char s, int x, int y, ConsoleColor colour = ConsoleColor.White)
        {
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
        }

        private static void WriteDebug(bool debug)
        {
            Debug.WriteLine($"---------------------------------------");
            Debug.WriteLine($"Cols\t {Cols}");
            Debug.WriteLine($"Rows\t {Rows}");
            Debug.WriteLine($"---------------------------------------");
            Debug.WriteLine($"Time:\t {Timer.ElapsedMilliseconds}ms");
            Debug.WriteLine($"Frame:\t {Frame}");
            Debug.WriteLine($"Player\t [x:{Player.x}, y:{Player.y}]");
            Debug.WriteLine($"Food\t [x:{Food.x}, y:{Food.y}]");
        }

        /// <summary>
        /// My custom Vector Class
        /// </summary>
        public class VecC
        {
            public int x { get; set; }
            public int y { get; set; }
            public Velocity velocity { get; set; }

            public enum Velocity
            {
                NONE,
                NORTH,
                NORTH_EAST,
                EAST,
                SOUTH_EAST,
                SOUTH,
                SOUTH_WEST,
                WEST,
                NORTH_WEST
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

            /// <summary>
            /// Update the player postion via Player Velocity
            /// </summary>
            public void UpdatePosition()
            {
                switch (velocity)
                {
                    case Velocity.NORTH:
                        Player.y -= playerSpeed;
                        break;
                    case Velocity.NORTH_EAST:
                        Player.y -= playerSpeed;
                        Player.x += playerSpeed;
                        break;
                    case Velocity.EAST:
                        Player.x += playerSpeed;
                        break;
                    case Velocity.SOUTH_EAST:
                        Player.y += playerSpeed;
                        Player.x += playerSpeed;
                        break;
                    case Velocity.SOUTH:
                        Player.y += playerSpeed;
                        break;
                    case Velocity.SOUTH_WEST:
                        Player.y += playerSpeed;
                        Player.x -= playerSpeed;
                        break;
                    case Velocity.WEST:
                        Player.x -= playerSpeed;
                        break;
                    case Velocity.NORTH_WEST:
                        Player.x -= playerSpeed;
                        Player.y -= playerSpeed;
                        break;
                    case Velocity.NONE:
                        Player.x += 0;
                        Player.y += 0;
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Will pick a random vector for the Player to walk
            /// </summary>
            public void RandomWalk()
            {
                Array values = Enum.GetValues(typeof(Velocity));
                Random random = new Random();
                Velocity velocity = (Velocity)values.GetValue(random.Next(values.Length));

                Player.velocity = velocity;
            }

            public void CheckWallCollision()
            {
                // Player is very left hand side
                if (Player.x <= 0) Player.x = wrapPlayer ? Cols - 2 : 1;

                // Player is very right hand side
                if (Player.x >= Cols - 1) Player.x = wrapPlayer ? 2 : Cols - 2;

                // Player is very top
                if (Player.y <= 0) Player.y = wrapPlayer ? Rows - 2 : 1;

                // Player is very bottom
                if (Player.y >= Rows - 1) Player.y = wrapPlayer ? 1 : Rows - 2;
            }

            //public void Bounce()
            //{
            //    if (Player.x <= 0) Player.velocity = Velocity.EAST;
            //    if (Player.x >= Cols - 1) Player.velocity = Velocity.WEST;

            //    if (Player.x <= 0 && Player.y < Rows / 2) Player.velocity = Velocity.NORTH_EAST;
            //    if (Player.x >= Cols - 1 && Player.y > Rows / 2) Player.velocity = Velocity.SOUTH_EAST;

            //    if (Player.y <= 0) Player.y = Player.y + 1;
            //    if (Player.y >= Rows - 1) Player.y = Player.y - 1;
            //}

            public static VecC GetRandomVector()
            {
                return new VecC(new Random().Next(1, Cols - 1), new Random().Next(1, Rows - 1));
            }
        }
    }
}