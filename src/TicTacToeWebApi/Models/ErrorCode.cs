namespace TicTacToeWebApi.Models
{
    public enum ErrorCode
    {
        NotError = 0,
        SysymbolIsTaken = 6000,
        PlaceIsTaken = 6001,
        GameOver = 6002,
        SessionNotFull = 6003,
        PlayerNotTakenSybol = 6004,
        WaitPlayer = 6005,
        SessionNotFound = 6006,
        SelectedIncorrectSymbol = 6007,
        IncorrectPlayerData = 6008,
        TurnAnotherPlayer = 6009,
        PlaceIsOutside = 6010
    }
}
