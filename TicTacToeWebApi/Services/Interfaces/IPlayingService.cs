using TicTacToeWebApi.Models;

namespace TicTacToeWebApi.Services.Interfaces
{
    public interface IPlayingService
    {
        Task<int> AddPlayer(Player player);
        Task<bool> SelectSymbol(SelectSymbolModel symbol);
        Task<CurrentTurnModel> MakeTurn(TurnModel model);
        Task<CurrentTurnModel> GetCurrentTurn(int sessionId);
        Task<bool[]> CheckAllPlayers(int sessionId);
    }
}
