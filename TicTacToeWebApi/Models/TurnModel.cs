namespace TicTacToeWebApi.Models
{
    public class TurnModel
    {
        public int SessionId { get; set; }
        public Player Player { get; set; }
        public int Position { get; set; }
    }
}
