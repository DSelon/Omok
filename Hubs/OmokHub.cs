using Microsoft.AspNetCore.SignalR;

public class OmokHub : Hub
{
    private static Dictionary<string, int[,]> rooms = new Dictionary<string, int[,]>();
    private static Dictionary<string, string[]> roomPlayers = new Dictionary<string, string[]>();
    private static Dictionary<string, int> roomTurn = new Dictionary<string, int>();

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

        if (!rooms.ContainsKey(roomName))
        {
            rooms[roomName] = new int[15, 15];
            roomPlayers[roomName] = new string[2];
            roomTurn[roomName] = 1;
        }

        if (roomPlayers[roomName][0] == null)
        {
            roomPlayers[roomName][0] = Context.ConnectionId;
            await Clients.Caller.SendAsync("AssignRole", 1);
        }
        else if (roomPlayers[roomName][1] == null)
        {
            roomPlayers[roomName][1] = Context.ConnectionId;
            await Clients.Caller.SendAsync("AssignRole", 2);
            await Clients.Group(roomName).SendAsync("GameStart", "두 플레이어가 입장했습니다. 게임을 시작합니다!");
        }
        else
        {
            await Clients.Caller.SendAsync("Notice", "방이 가득 찼습니다.");
        }
    }

    public async Task MakeMove(string roomName, int x, int y, int player)
    {
        if (!rooms.ContainsKey(roomName)) return;

        var board = rooms[roomName];
        
        if (roomTurn[roomName] == player && board[x, y] == 0)
        {
            board[x, y] = player;
            roomTurn[roomName] = (player == 1) ? 2 : 1;

            await Clients.Group(roomName).SendAsync("ReceiveMove", x, y, player, roomTurn[roomName]);

            if (CheckWin(board, x, y, player))
            {
                await Clients.Group(roomName).SendAsync("GameOver", player);
                rooms[roomName] = new int[15, 15];
                roomTurn[roomName] = 1;
            }
        }
    }

    private bool CheckWin(int[,] board, int x, int y, int player)
    {
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int count = 1;
            for (int step = 1; step < 5; step++)
            {
                int nx = x + dx[i] * step;
                int ny = y + dy[i] * step;
                if (nx < 0 || nx >= 15 || ny < 0 || ny >= 15 || board[nx, ny] != player) break;
                count++;
            }
            for (int step = 1; step < 5; step++)
            {
                int nx = x - dx[i] * step;
                int ny = y - dy[i] * step;
                if (nx < 0 || nx >= 15 || ny < 0 || ny >= 15 || board[nx, ny] != player) break;
                count++;
            }

            if (count >= 5) return true;
        }
        return false;
    }
}