using System;
using System.Runtime.InteropServices;

namespace TicTacToe.User
{
    class Program
    {
        static void Main()
        {
            object game = null;

            try
            {
                // Create COM object instance
                var comType = Type.GetTypeFromProgID("LaViejaCOM.LaViejaGame");
                if (comType == null)
                {
                    Console.WriteLine("Error: Could not find the COM object.");
                    Console.WriteLine("Make sure the COM object is registered.");
                    return;
                }

                game = Activator.CreateInstance(comType);
                Console.WriteLine("=== La Vieja (Tic Tac Toe) With COM ===\n");

                Console.WriteLine("Press any key to start...");
                Console.ReadKey();

                // Game loop
                while (true)
                {
                    //Console.Clear();
                    string board = (string)comType.InvokeMember("GetBoard",
                        System.Reflection.BindingFlags.InvokeMethod, null, game, null);

                    string current = (string)comType.InvokeMember("GetCurrent",
                        System.Reflection.BindingFlags.InvokeMethod, null, game, null);

                    // Visual banner for each turn
                    Console.WriteLine("==============================");
                    Console.Write("Current turn: ");
                    if (current == "X") Console.ForegroundColor = ConsoleColor.Cyan;
                    else if (current == "O") Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(current);
                    Console.ResetColor();
                    Console.WriteLine("==============================");

                    PrintBoard(board);

                    bool gameOver = (bool)comType.InvokeMember("IsGameOver",
                        System.Reflection.BindingFlags.InvokeMethod, null, game, null);

                    if (gameOver)
                    {
                        string winner = (string)comType.InvokeMember("GetWinner",
                            System.Reflection.BindingFlags.InvokeMethod, null, game, null);

                        if (winner == "Empate" || winner == "Draw")
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("🤝 It's a draw!");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"🎉 {winner} is the winner!");
                            Console.ResetColor();
                        }

                        Console.Write("\nPlay again? (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            comType.InvokeMember("Reset",
                                System.Reflection.BindingFlags.InvokeMethod, null, game, null);
                            continue;
                        }
                        break;
                    }

                    Console.Write("Enter position (0-8) or 'q' to quit: ");

                    string input = Console.ReadLine();
                    if (input?.ToLower() == "q")
                        break;

                    if (!int.TryParse(input, out int position))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ Please enter a valid number.\n");
                        Console.ResetColor();
                        continue;
                    }

                    string result = (string)comType.InvokeMember("Play",
                        System.Reflection.BindingFlags.InvokeMethod, null, game, new object[] { position });

                    if (result != "OK")
                    {
                        if (result.StartsWith("Error:"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"❌ {result}\n");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"🎯 {result}\n");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ Move made.\n");
                        Console.ResetColor();
                    }
                }
            }
            catch (COMException comEx)
            {
                Console.WriteLine(string.Format("COM Error: {0}", comEx.Message));
                Console.WriteLine("Check that the COM object is properly registered.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}", ex.Message));
            }
            finally
            {
                // Release COM object
                if (game != null && Marshal.IsComObject(game))
                {
                    Marshal.ReleaseComObject(game);
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void PrintBoard(string boardString)
        {
            Console.WriteLine("┌───┬───┬───┐");
            for (int row = 0; row < 3; row++)
            {
                Console.Write("│");
                for (int col = 0; col < 3; col++)
                {
                    int index = row * 3 + col;
                    char cell = boardString[index];
                    string display = cell == '-' ? index.ToString() : cell.ToString();
                    // Color for X and O
                    if (cell == 'X')
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($" {display} ");
                        Console.ResetColor();
                    }
                    else if (cell == 'O')
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($" {display} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write($" {display} ");
                    }
                    Console.Write("│");
                }
                Console.WriteLine();

                if (row < 2)
                    Console.WriteLine("├───┼───┼───┤");
            }
            Console.WriteLine("└───┴───┴───┘");
            Console.WriteLine();
        }
    }
}