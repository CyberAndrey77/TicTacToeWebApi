namespace TicTacToeWebApi.Models
{
    public class ConditionSessionModel
    {
        public Player? TurnPlayer { get; set; }
        public Player? Player1 { get; set; }
        public Player? Player2 { get; set; }
        public string Playe1Symbol { get; set; }
        public string Playe2Symbol { get; set; }
        public string[] Field { get; set; }
        public Player? Winner { get; set; }
        public bool IsGameOver { get; set; }
        public string Error { get; set; }
        public ErrorCode Code { get; set; }
    }
}
