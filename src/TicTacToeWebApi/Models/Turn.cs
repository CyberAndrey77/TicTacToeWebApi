using System.ComponentModel.DataAnnotations;

namespace TicTacToeWebApi.Models
{
    public class Turn
    {
        [Key]
        public int NumberTurn { get; set; }
        public Player Player { get; set; }
        public string Symbol { get; set; }
        public int Position { get; set; }

        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }

        public Turn()
        {

        }
        public Turn(Player player, string symbol, int position)
        {
            Player = player;
            Symbol = symbol;
            Position = position;
        }
    }
}
