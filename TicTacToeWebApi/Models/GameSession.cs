using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToeWebApi.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public Player? Player1 { get; set; }
        public Player? Player2 { get; set; }
        public string? Player1Symbol { get; set; }
        public string? Player2Symbol { get; set; }
        public Player? Winner { get; set; }

        //Нужно для отслеживания очередности ходов, нет смысла записывать в бд
        [NotMapped]
        public Player TurnPlayer { get; set; }
        public List<Turn> Turns { get; set; } = new List<Turn>();

        //Для отслеживания занятых позиций в сессии, запись ходов будет в другой таблице
        [NotMapped]
        public string[] Field { get; set; } = new string[9];
    }
}
