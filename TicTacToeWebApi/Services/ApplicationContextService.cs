using Microsoft.EntityFrameworkCore;
using TicTacToeWebApi.Models;

namespace TicTacToeWebApi.Services
{
    public class ApplicationContextService : DbContext
    {
        public DbSet<GameSession> GameSessions {get; set;}
        public DbSet<Player> Players { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public ApplicationContextService(DbContextOptions<ApplicationContextService> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
