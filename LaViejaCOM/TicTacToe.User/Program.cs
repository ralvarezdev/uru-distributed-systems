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
                    Console.WriteLine("Error: No se pudo encontrar el objeto COM.");
                    Console.WriteLine("Asegurate de que el objeto COM este registrado.");
                    return;
                }

                game = Activator.CreateInstance(comType);
                Console.WriteLine("=== La Vieja (Tic Tac Toe) COM ===\n");

                // Game loop
                while (true)
                {
                    string board = (string)comType.InvokeMember("GetBoard", 
                        System.Reflection.BindingFlags.InvokeMethod, null, game, null);
                    PrintBoard(board);
                    
                    bool gameOver = (bool)comType.InvokeMember("IsGameOver", 
                        System.Reflection.BindingFlags.InvokeMethod, null, game, null);
                    
                    if (gameOver)
                    {
                        string winner = (string)comType.InvokeMember("GetWinner", 
                            System.Reflection.BindingFlags.InvokeMethod, null, game, null);
                        
                        if (winner == "Empate")
                            Console.WriteLine("🤝 ¡Es un empate!");
                        else
                            Console.WriteLine(string.Format("🎉 ¡{0} es el ganador!", winner));
                        
                        Console.Write("\n¿Jugar otra vez? (s/n): ");
                        if (Console.ReadLine()?.ToLower() == "s")
                        {
                            comType.InvokeMember("Reset", 
                                System.Reflection.BindingFlags.InvokeMethod, null, game, null);
                            continue;
                        }
                        break;
                    }

                    string current = (string)comType.InvokeMember("GetCurrent", 
                        System.Reflection.BindingFlags.InvokeMethod, null, game, null);
                    
                    Console.WriteLine(string.Format("Turno de: {0}", current));
                    Console.Write("Ingresa posicion (0-8) o 'q' para salir: ");
                    
                    string input = Console.ReadLine();
                    if (input?.ToLower() == "q")
                        break;

                    if (!int.TryParse(input, out int position))
                    {
                        Console.WriteLine("❌ Ingresa un numero valido.\n");
                        continue;
                    }

                    string result = (string)comType.InvokeMember("Play", 
                        System.Reflection.BindingFlags.InvokeMethod, null, game, new object[] { position });
                    
                    if (result != "OK")
                    {
                        if (result.StartsWith("Error:"))
                            Console.WriteLine(string.Format("❌ {0}\n", result));
                        else
                            Console.WriteLine(string.Format("🎯 {0}\n", result));
                    }
                    else
                    {
                        Console.WriteLine("✅ Movimiento realizado.\n");
                    }
                }
            }
            catch (COMException comEx)
            {
                Console.WriteLine(string.Format("Error COM: {0}", comEx.Message));
                Console.WriteLine("Verifica que el objeto COM este correctamente registrado.");
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

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
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
                    Console.Write(string.Format(" {0} │", display));
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