namespace TicTacToeWebApi.Models
{
    public class SelectSymbolModel
    {
        public int PlayerId { get; set; }
        public int SessionId { get; set; }
        public string Symbol { get; set; }
    }
}
