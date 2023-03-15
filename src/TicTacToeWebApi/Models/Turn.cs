using System.ComponentModel.DataAnnotations;

namespace TicTacToeWebApi.Models
{
    public class Turn
    {
        public int Id { get; set; }
        public int TurnNumber { get; set; }
        public Player Player { get; set; }
        public string Symbol { get; set; }
        public int Position { get; set; }

        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }

        public Turn()
        {

        }

        public Turn(Player player, string symbol, int position, int turnNumber)
        {
            Player = player;
            Symbol = symbol;
            Position = position;
            TurnNumber = turnNumber;
        }
    }
}
