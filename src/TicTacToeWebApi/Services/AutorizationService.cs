using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TicTacToeWebApi.Models;
using TicTacToeWebApi.Services.Interfaces;

namespace TicTacToeWebApi.Services
{
    public class AutorizationService : IAutorizationService
    {
        private ApplicationContextService _context;

        public AutorizationService(ApplicationContextService applicationContext)
        {
            _context = applicationContext;
        }
        public async Task<Player> Login(LoginModel model)
        {
            var player = await _context.Players.FirstOrDefaultAsync(x => model.Name == x.Name);

            if (player == null)
            {
                player = new Player { Name = model.Name };
                await _context.Players.AddAsync(player);
                await _context.SaveChangesAsync();
            }

            return player;
        }
    }
}
