# uru-distributed-systems

**Note:** This repository is archived and read-only.

Projects and practices from the Distributed Systems college course (URU), in C# and Node.js.

## Projects

- **`LaViejaCOM`** — C# (.NET Framework 4.8) COM component for tic-tac-toe ("La Vieja"). `TicTacToe.ComObject` exposes `ILaViejaGame` (`GetBoard`, `GetCurrent`, `Play`, `Reset`, `IsGameOver`, `GetWinner`) with `register-com.bat` / `unregister-com.bat` (run as administrator; messages in Spanish). `TicTacToe.User` is a console client. Windows only.
- **`grpc-wrapper`** — Node.js helpers around `@grpc/grpc-js`: `loadProto`, `createInsecureServer`, `createInsecureClient`. `example/` is a book service on port `50051`.
- **`load-balancer`** — heartbeat-based load balancing demo built on the wrapper. The gRPC `LOAD_BALANCER_SERVICE` (`heartbeat`, `getNextInstance`) scores instances on CPU count, clock speed, uptime and memory (20% each), expires them after a 15 s timeout and hands out the next one round-robin. `instances/` forks 5 book services from port `50052`; `api-gateway/` is an Express server on port `8080` exposing `GET /api/book/:id`.
- **`tic-tac-toe`** — C# (.NET 9) gRPC multiplayer game. `TicTacToeServer` is an ASP.NET Core service (`Protos/tictactoe.proto`, with streaming RPCs); `TicTacToeMaui` is a .NET MAUI client.

## Running

```bash
# grpc-wrapper example
cd grpc-wrapper && npm install
npm run server   # one terminal
npm run client   # another

# load-balancer (also install grpc-wrapper dependencies; it imports ../grpc-wrapper)
cd load-balancer && npm install
npm run server && npm run instances && npm run api-gateway && npm run client

# tic-tac-toe
cd tic-tac-toe/TicTacToeServer && dotnet run
```

Start order and ports for the load balancer are set in the JSON files next to each entry point. Open `TicTacToeMaui` in an IDE with the .NET MAUI workload. For `LaViejaCOM`, build with .NET Framework 4.8, then run `register-com.bat` as administrator.

## License

GNU General Public License v3.0 (see `LICENSE`).
