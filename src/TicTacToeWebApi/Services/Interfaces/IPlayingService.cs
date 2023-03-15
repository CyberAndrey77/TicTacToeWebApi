using TicTacToeWebApi.Models;

namespace TicTacToeWebApi.Services.Interfaces
{
    public interface IPlayingService
    {
        Task<int> AddPlayer(Player player);
        Task<ConditionSessionModel> SelectSymbol(SelectSymbolModel symbol);
        Task<ConditionSessionModel> MakeTurn(TurnModel model);
        Task<ConditionSessionModel> GetCurrentTurn(int sessionId);
        Task<bool[]> CheckAllPlayers(int sessionId);
        Task<ConditionSessionModel> RemovePlayer(RemovePlayerModel model);
    }
}
