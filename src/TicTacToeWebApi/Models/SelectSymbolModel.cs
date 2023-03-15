namespace TicTacToeWebApi.Models
{
    public class SelectSymbolModel
    {
        public int SessionId { get; set; }
        public Player Player { get; set; }
        public string Symbol { get; set; }
    }
}
