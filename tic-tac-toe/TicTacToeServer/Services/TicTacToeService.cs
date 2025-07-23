using Grpc.Core;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace TicTacToeServer.Services
{
    public class TicTacToeService : TicTacToe.TicTacToeBase
    {
        private static readonly ConcurrentDictionary<string, Game?>
            GamesById = new();
        private static readonly ConcurrentDictionary<string, Game?>
            GamesByCode = new();

        /// Starts a new game with the given player as Player X
        public override Task<StartGameReply> StartGame(StartGameRequest request,
            ServerCallContext context)
        {
            // Create a new game instance and add it to the dictionary
            var game = new Game(
                new Player(
                    request.Name,
                    request.Symbol
                )
            );
            GamesById[game.GameId] = game;
            GamesByCode[game.GameCode]  = game;
            return Task.FromResult(new StartGameReply
                { GameId = game.GameId, GameCode = game.GameCode });
        }
        
        /// Waits for a player to join the game
        public override async Task WaitForPlayer(
            WaitForPlayerRequest request,
            IServerStreamWriter<WaitForPlayerReply> responseStream,
            ServerCallContext context)
        {
            // Check if the game exists
            var game = GetGameById(request.GameId);
            if (game is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            
            // Stream status updates until game starts
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var gameStatus = game.Status;
                var reply = new WaitForPlayerReply 
                {
                    GameId = request.GameId,
                    Status = gameStatus,
                };
                await responseStream.WriteAsync(reply);
                
                // If the game is in progress, exit the loop
                if (gameStatus != GameStatus.WaitingForPlayer)
                    break;
                
                // Wait for a while before checking again
                await Task.Delay(1000);
            }
        }

        public override Task<JoinGameReply> JoinGame(JoinGameRequest request,
            ServerCallContext context)
        {
            // Check if the game exists
            var game = GetGameByCode(request.GameCode);
            if (game == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            
            lock (game.StatusLock)
            {
                // Check if the game is already full
                if (game.Status != GameStatus.WaitingForPlayer)
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "Game already has two players"));
                game.Status = GameStatus.InProgress;
            }
            
            // Get the symbol of the host player
            var hostPlayer = game.HostPlayer;
            
            // Check if the request symbol is valid
            var inviteePlayerSymbol = hostPlayer.Symbol == Symbol.X ? Symbol.O : Symbol.X;
            game.InviteePlayer = new Player(request.Name, inviteePlayerSymbol);
            return Task.FromResult(new JoinGameReply
                { GameId = game.GameId, Symbol = inviteePlayerSymbol });
        }

        /// Makes a move in the game
        public override async Task MakeMove(
            IAsyncStreamReader<MoveRequest> requestStream,
            IServerStreamWriter<MoveReply> responseStream,
            ServerCallContext context)
        {
            await foreach (var move in requestStream.ReadAllAsync())
            {
                // Check if the request stream is null
                var game = GetGameById(move.GameId);
                if (game is null)
                    throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));

                // Check if the game is in progress
                if (game.Status != GameStatus.InProgress) 
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, "Game is not in progress"));

                MoveReply reply;
                lock (game.TurnLock)
                {
                    // Check if the player is the one whose turn it is
                    if (game.TurnPlayerId != move.PlayerId)
                    {
                        reply = new MoveReply
                        {
                            GameId = move.GameId, 
                            Status = GameStatus.InProgress, 
                            Error = "It's not your turn"
                        };
                    } 
                    else if (game.Board.GetCell(move.Row, move.Col) != Cell.Empty)
                    {
                        reply = new MoveReply 
                        {
                            GameId = move.GameId, 
                            Status = GameStatus.InProgress, 
                            Error = "Invalid move"
                        };
                    }
                    else if (game.InviteePlayer == null)
                    {
                        reply = new MoveReply
                        {
                            GameId = move.GameId,
                            Status = GameStatus.InProgress,
                            Error = "Invitee player not found"
                        };
                    }
                    else
                    {
                        // Check if there are crossed moves
                        var playerSymbol = move.PlayerId == game.HostPlayer.PlayerId
                            ? game.HostPlayer.Symbol
                            : game.InviteePlayer.Symbol;
                        
                        // Set the cell on the board
                        game.Board.SetCell(move.Row, move.Col, playerSymbol);
                        
                        // Notify the opponent about the move
                        game.InviteePlayer.RaiseMoved(move.Row, move.Col);
                        
                        // Check for a winner
                        var winnerSymbol = game.IsWinner(playerSymbol);
                        if (winnerSymbol)
                        {
                            game.Status = playerSymbol == Symbol.X
                                ? GameStatus.XWon
                                : GameStatus.OWon;
                            game.Winner = move.PlayerId;
                            reply = new MoveReply
                            {
                                GameId = move.GameId,
                                Status = game.Status,
                                Error = "",
                            };
                        }
                        else
                        {
                            // Increment the move count
                            game.MoveCount++;
                            
                            // Switch the turn to the opponent
                            game.TurnPlayerId = game.TurnPlayerId == game.HostPlayer.PlayerId
                                ? game.InviteePlayer.PlayerId
                                : game.HostPlayer.PlayerId;

                            // Check if all cells are filled
                            if (game.MoveCount < 9)
                            {
                                reply = new MoveReply
                                {
                                    GameId = move.GameId,
                                    Status = GameStatus.InProgress,
                                    Error = ""
                                };
                            }
                            else
                            {
                                game.Status = GameStatus.Draw;
                                reply = new MoveReply
                                {
                                    GameId = move.GameId,
                                    Status = GameStatus.Draw,
                                    Error = "Game is a draw"
                                };
                            }
                        }
                    }
                }

                // Send reply to client
                await responseStream.WriteAsync(reply);
            }
        }

        /// Notify the movement of the opponent
        public override async Task NotifyOpponentMove(
            NotifyOpponentMoveRequest request,
            IServerStreamWriter<NotifyOpponentMoveReply> responseStream,
            ServerCallContext context)
        {
            // Check if the game exists
            var game = GetGameById(request.GameId);
            if (game is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));
            
            // Check if the game is in progress
            if (game.Status != GameStatus.InProgress)
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Game is not in progress"));
            
            // Get the opponent player
            var opponentPlayer = game.HostPlayer.PlayerId == request.PlayerId
                ? game.InviteePlayer
                : game.HostPlayer;
            
            // Check if the opponent player is null
            async void OnOpponentMoved(int row, int col)
            {
                var reply = new NotifyOpponentMoveReply
                {
                    GameId = request.GameId,
                    Row = row,
                    Col = col,
                    Status = game.Status
                };
                await responseStream.WriteAsync(reply);
            }

            // Check if the opponent player exists
            if (opponentPlayer == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Opponent player not found"));
            
            // Subscribe to the opponent's moved event
            opponentPlayer.moved += OnOpponentMoved;
            
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (game.Status != GameStatus.InProgress)
                {
                    // If the game is not in progress, unsubscribe and exit
                    opponentPlayer.moved -= OnOpponentMoved;
                    break;
                }
                
                // Wait for a while before checking again
                await Task.Delay(1000);
            }
        }
        
        /// Retrieves a game by its ID
        private static Game? GetGameById(string gameId)
        {
            return GamesById.GetValueOrDefault(gameId, null);
        }

        /// Retrieves a game by its game code
        private static Game? GetGameByCode(string gameCode)
        {
            return GamesByCode.GetValueOrDefault(gameCode, null);
        }
        
        /// Generates a random hexadecimal code
        private static string GenerateHexCode(int length)
        {
            // Check that the length is even
            if (length % 2 != 0)
                throw new ArgumentException("Length must be an even number", nameof(length));
            
            // Generate random bytes and convert to hexadecimal string
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length / 2];
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).Replace("-", "")
                .ToUpperInvariant();
        }
        
        /// Game status enumeration for internal state
        private enum Cell
        {
            Empty,
            X,
            O
        };
        
        /// Game board class for internal state
        private class Board
        {
            private List<Row> Rows { get; } = new()
            {
                new Row(), new Row(), new Row()
            };
            
            /// Gets the cell at the specified row and column
            public Cell GetCell(int row, int col)
            {
                if (row < 0 || row >= Rows.Count)
                    throw new ArgumentOutOfRangeException(nameof(row), "Row index out of range");
                return Rows[row].GetColumn(col);
            }
            
            /// Gets the cell as a Symbol
            public Symbol? GetCellAsSymbol(int row, int col)
            {
                var cell = GetCell(row, col);
                return cell switch
                {
                    Cell.X => Symbol.X,
                    Cell.O => Symbol.O,
                    _ => null
                };
            }
            
            /// Sets the cell at the specified row and column to the given symbol
            public void SetCell(int row, int col, Symbol symbol)
            {
                if (row < 0 || row >= Rows.Count)
                    throw new ArgumentOutOfRangeException(nameof(row), "Row index out of range");
                Rows[row].SetColumn(col, symbol);
            }
        }
        
        /// Game board row class for internal state
        private class Row
        {
            private List<Cell> Cells { get; } = new()
            {
                Cell.Empty, Cell.Empty, Cell.Empty
            };
            
            public Cell GetColumn(int col)
            {
                if (col < 0 || col >= Cells.Count)
                    throw new ArgumentOutOfRangeException(nameof(col), "Column index out of range");
                return Cells[col];
            }
            
            public void SetColumn(int col, Symbol symbol)
            {
                if (col < 0 || col >= Cells.Count)
                    throw new ArgumentOutOfRangeException(nameof(col), "Column index out of range");
                if (symbol != Symbol.X && symbol != Symbol.O)
                    throw new ArgumentException("Invalid symbol", nameof(symbol));
                Cells[col] = symbol == Symbol.X ? Cell.X : Cell.O;
            }
        }
        
        /// Game status enumeration for internal state
        private delegate void PlayerMovedHandler(int row, int col);
        
        /// Player class for internal state
        private class Player(string name, Symbol symbol)
        {
            public string Name { get; } = name;
            public Symbol Symbol { get; } = symbol;
            public string PlayerId { get; } = GeneratePlayerId();
            public event PlayerMovedHandler? moved;
            
            /// Generates a random player ID
            private static string GeneratePlayerId()
            {
                const int idLength = 16;
                return GenerateHexCode(idLength);
            }
            
            /// Raises the moved event
            public void RaiseMoved(int row, int col)
            {
                moved?.Invoke(row, col);
            }
        }
        
        /// Game class for internal state
        private class Game(Player hostPlayer)
        {
            public object StatusLock { get; } = new();
            public object TurnLock { get; } = new();
            
            public string GameId { get; } = Guid.NewGuid().ToString();
            public string GameCode { get; } = GenerateGameCode();
            public Player HostPlayer { get; } = hostPlayer;
            public Player? InviteePlayer { get; set; }
            
            public string TurnPlayerId { get; set; } = hostPlayer.PlayerId;
            public string? Winner { get; set; }
            public int MoveCount { get; set; } = 0;

            public GameStatus Status { get; set; } =
                GameStatus.WaitingForPlayer;
            public Board Board { get; } = new();
            
            /// Generates a random game code
            private static string GenerateGameCode()
            {
                const int codeLength = 6;
                return GenerateHexCode(codeLength);
            }
            
            /// Checks if the given symbol is a winner
            public bool IsWinner(Symbol symbol)
            {
                // Check rows, columns, and diagonals for a win
                for (int i = 0; i < 3; i++)
                {
                    // Check for rows crossed
                    if (Board.GetCellAsSymbol(i, 0) == symbol &&
                         Board.GetCellAsSymbol(i, 1) == symbol &&
                         Board.GetCellAsSymbol(i, 2) == symbol)
                        return true;
                    
                    // Check for columns crossed
                    if (Board.GetCellAsSymbol(0, i) == symbol &&
                        Board.GetCellAsSymbol(1, i) == symbol &&
                        Board.GetCellAsSymbol(2, i) == symbol)
                        return true;
                }
                
                // Check if the positive diagonal is crossed
                if (Board.GetCellAsSymbol(0, 0) == symbol &&
                    Board.GetCellAsSymbol(1, 1) == symbol &&
                    Board.GetCellAsSymbol(2, 2) == symbol)
                    return true;
                
                // Check if the negative diagonal is crossed
                if (Board.GetCellAsSymbol(0, 2) == symbol && 
                    Board.GetCellAsSymbol(1, 1) == symbol &&
                    Board.GetCellAsSymbol(2, 0) == symbol)
                    return true;
                    
                return false;
            }
        }
    } 
}