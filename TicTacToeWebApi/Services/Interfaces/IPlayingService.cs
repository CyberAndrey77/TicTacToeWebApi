using TicTacToeWebApi.Models;

namespace TicTacToeWebApi.Services.Interfaces
{
    public interface IPlayingService
    {
        Task<int> AddPlayer(Player player);
        Task<bool> SelectSymbol(SelectSymbolModel symbol);
        Task<ConditionSessionModel> MakeTurn(TurnModel model);
        Task<ConditionSessionModel> GetCurrentTurn(int sessionId);
        Task<bool[]> CheckAllPlayers(int sessionId);
    }
}
