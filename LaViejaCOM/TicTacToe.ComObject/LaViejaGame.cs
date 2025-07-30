using System;
using System.Runtime.InteropServices;

namespace TicTacToe.ComObject
{
    // COM Interface
    [ComVisible(true)]
    [Guid("B1234567-B89C-4D5E-8F01-23456789ABCD")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ILaViejaGame // I = Interface prefix
    {
        [DispId(1)]
        string GetBoard();
        
        [DispId(2)]
        string GetCurrent();
        
        [DispId(3)]
        string Play(int position);
        
        [DispId(4)]
        void Reset();
        
        [DispId(5)]
        bool IsGameOver();
        
        [DispId(6)]
        string GetWinner();
    }

    // COM Class Implementation
    [ComVisible(true)]
    [Guid("A1234567-B89C-4D5E-8F01-23456789ABCD")]
    [ProgId("LaViejaCOM.LaViejaGame")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(ILaViejaGame))]
    public class LaViejaGame : ILaViejaGame
    {
        private char[] board = new char[9];
        private char currentPlayer = 'X';
        private bool gameOver = false;
        private string winner = string.Empty;

        public LaViejaGame()
        {
            Reset();
        }

        public string GetBoard()
        {
            return string.Join("", board);
        }

        public string GetCurrent()
        {
            return currentPlayer.ToString();
        }

        public string Play(int position)
        {
            // Validate input
            if (position < 0 || position > 8)
                return "Error: Invalid position (0-8)";

            if (gameOver)
                return "Error: The game is already over";

            if (board[position] != '-')
                return "Error: Position already taken";

            // Make the move
            board[position] = currentPlayer;

            if (CheckWin(currentPlayer))
            {
                gameOver = true;
                winner = currentPlayer.ToString();
                return string.Format("{0} wins!", currentPlayer);
            }

            // Check for tie
            if (Array.IndexOf(board, '-') == -1)
            {
                gameOver = true;
                winner = "Draw";
                return "Draw!";
            }

            // Switch players
            currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
            return "OK";
        }

        public void Reset()
        {
            for (int i = 0; i < 9; i++)
                board[i] = '-';

            currentPlayer = 'X';
            gameOver = false;
            winner = string.Empty;
        }

        public bool IsGameOver()
        {
            return gameOver;
        }

        public string GetWinner()
        {
            return winner;
        }

        private bool CheckWin(char player)
        {
            // Winning combinations
            int[,] winPatterns = {
                {0,1,2}, {3,4,5}, {6,7,8}, // Rows
                {0,3,6}, {1,4,7}, {2,5,8}, // Columns
                {0,4,8}, {2,4,6}           // Diagonals
            };

            for (int i = 0; i < 8; i++)
            {
                if (board[winPatterns[i, 0]] == player &&
                    board[winPatterns[i, 1]] == player &&
                    board[winPatterns[i, 2]] == player)
                {
                    return true;
                }
            }
            return false;
        }
    }
}