using TicTacToeWebApi.Models;

namespace TicTacToeWebApi.Services.Interfaces
{
    public interface IAutorizationService
    {
        Task<Player> Login(LoginModel model);
    }
}
