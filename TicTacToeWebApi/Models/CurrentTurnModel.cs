namespace TicTacToeWebApi.Models
{
    public class CurrentTurnModel
    {
        public Player? TurnPlayer { get; set; }
        public string[] Field { get; set; }
        public Player? Winner { get; set; }
        public string Error { get; set; }
    }
}
