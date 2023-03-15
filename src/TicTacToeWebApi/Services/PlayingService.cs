using Microsoft.EntityFrameworkCore;
using System.Timers;
using TicTacToeWebApi.Models;
using TicTacToeWebApi.Services.Interfaces;

namespace TicTacToeWebApi.Services
{
    public class PlayingService : IPlayingService
    {
        private ApplicationContextService _context;
        //TODO сделать кик афк игроков!!!!!!!
        private List<GameSession> _gameSessions;

        public PlayingService(ApplicationContextService context, GameSessions sessions)
        {
            _context = context;
            _gameSessions = sessions.Sessions;
        }

        #region AddPlayer
        public async Task<int> AddPlayer(Player player)
        {
            int sessionId = await Task.Run(() => RegistrationInExistSession(player));
            if (sessionId != -1)
            {
                return sessionId;
            }
            return await Task.Run(() => CreateNewSession(player));
        }

        private int RegistrationInExistSession(Player player)
        {
            int sessionId = -1;
            int index = 0;
            bool isConnectPlayer1 = false;
            for (int i = 0; i < _gameSessions.Count; i++)
            {
                //Пока не понял как избавиться от дублирования
                if (_gameSessions[i].IsClosed)
                {
                    continue;
                }
                else if (_gameSessions[i].Player1?.Id == player.Id)
                {
                    _gameSessions[i].Player1 = player;
                    sessionId = _gameSessions[i].Id; 
                    return sessionId;
                }
                else if (_gameSessions[i].Player2?.Id == player.Id)
                {
                    _gameSessions[i].Player2 = player;
                    sessionId = _gameSessions[i].Id;
                    return sessionId;
                }
                else if (_gameSessions[i].Player1 == null)
                {
                    isConnectPlayer1 = true;
                    _gameSessions[i].Player1 = player;
                    index = i;
                    sessionId = _gameSessions[i].Id; break;
                }
                else if (_gameSessions[i].Player2 == null)
                {
                    isConnectPlayer1 = false;
                    _gameSessions[i].Player2 = player;
                    index = i;
                    sessionId = _gameSessions[i].Id; break;
                }
            }

            if (sessionId == -1)
            {
                return sessionId;
            }

            GameSession sessionContext = _context.GameSessions.FirstOrDefault(x => x.Id == sessionId);
            if (sessionContext == null)
            {
                throw new ArgumentNullException("Не удалось найти игровую сессию в базе данных");
            }
            if (isConnectPlayer1)
            {
                sessionContext.Player1 = _gameSessions[index].Player1;
            }
            else
            {
                sessionContext.Player2 = _gameSessions[index].Player2;
            }
            _context.SaveChanges();
            return sessionId;
        }

        private int CreateNewSession(Player player)
        {
            var gameSession = new GameSession();
            gameSession.Player1 = player;
            _context.GameSessions.Add(gameSession);
            _context.SaveChanges();
            _gameSessions.Add(gameSession);
            return gameSession.Id;
        }
        #endregion

        #region MakeTurn
        public async Task<ConditionSessionModel> MakeTurn(TurnModel model)
        {
            var currentCondition = new ConditionSessionModel();
            var session = await Task.Run(() => FindSession(model.SessionId));
            if (!SessionIsActive(currentCondition, session))
            {
                return currentCondition;
            }
            if (session.IsClosed)
            {
                currentCondition.Error = $"Игра окончена";
                currentCondition.Code = ErrorCode.GameOver;
                return GetInfoSession(currentCondition, session);
            }
            if (session.Player1 == null || session.Player2 == null)
            {
                currentCondition.Error = $"Ожидание второго игрока";
                currentCondition.Code = ErrorCode.WaitPlayer;
                return GetInfoSession(currentCondition, session);
            }
            if (model.Player.Id != session.Player1.Id && model.Player.Id != session.Player2.Id)
            {
                currentCondition.Error = "Неверные данные игрока";
                currentCondition.Code = ErrorCode.IncorrectPlayerData;
                return GetInfoSession(currentCondition, session);
            }
            if (session.TurnPlayer.Id != model.Player.Id)
            {
                currentCondition.Error = "Ход другого игрока";
                currentCondition.Code = ErrorCode.TurnAnotherPlayer;
                return GetInfoSession(currentCondition, session);
            }
            if (model.Position > session.Field.Length - 1)
            {
                currentCondition.Error = "Выбрана позиция выходит за пределы поля";
                currentCondition.Code = ErrorCode.PlaceIsTaken;
                return GetInfoSession(currentCondition, session);
            }
            var sessionContext = await _context.GameSessions.Include(x => x.Turns)
                .FirstOrDefaultAsync(x => x.Id == model.SessionId);

            var symbol = session.TurnPlayer == session.Player1 ?
                session.Player1Symbol : session.Player2Symbol;
            if (session.Field[model.Position] == null)
            {
                session.Field[model.Position] = symbol;
            }
            else
            {
                currentCondition.Error = "Место уже занято";
                currentCondition.Code = ErrorCode.PlaceIsTaken;
                return GetInfoSession(currentCondition, session);
            }

            bool isWinner = await Task.Run(() => HasWinner(symbol, session));
            bool isGameOver = await Task.Run(() => IsGameOver(session));
            if (isWinner || isGameOver)
            {
                if (isWinner)
                {
                    session.Winner = session.TurnPlayer;
                    sessionContext.Winner = session.TurnPlayer;
                }
                session.IsClosed = true;
                sessionContext.IsClosed = true;
                //что бы игроки могли получить данные что игра закончилась и кто в ней победил
                Task.Run(() => CloseGameSession(session));
            }
            else
            {
                sessionContext.Turns.Add(
                    new Turn(session.TurnPlayer, symbol, model.Position, sessionContext.Turns.Count + 1));
                session.TurnPlayer = session.TurnPlayer != session.Player1 ?
                    session.Player1 : session.Player2;
            }
            
            await _context.SaveChangesAsync();
            return GetInfoSession(currentCondition, session);
        }

        private bool HasWinner(string symbol, GameSession session)
        {
            for (int i = 0; i < session.Field.Length; i += 3)
            {
                if (session.Field[i] == symbol && symbol == session.Field[i + 1] 
                    && symbol == session.Field[i + 2])
                {
                    return true;
                }
            }
            //3 - длина поля
            for (int i = 0; i < 3; i++)
            {
                if (session.Field[i] == symbol && symbol == session.Field[i + 3] 
                    && symbol == session.Field[i + 6])
                {
                    return true;
                }
            }

            if (session.Field[0] == symbol && symbol == session.Field[4] && symbol == session.Field[8])
            {
                return true;
            }
            else if (session.Field[2] == symbol && symbol == session.Field[4] && symbol == session.Field[6])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsGameOver(GameSession session)
        {
            for (int i = 0; i < session.Field.Length; i++)
            {
                if (session.Field[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region SelectSymbol
        public async Task<ConditionSessionModel> SelectSymbol(SelectSymbolModel model)
        {
            var currentCondition = new ConditionSessionModel();
            //Нолик это символ О, а не цифра нуль, нолик тоненький)
            var session = await Task.Run(() => FindSession(model.SessionId));
            if (!SessionIsActive(currentCondition, session))
            {
                return currentCondition;
            }
            if (model.Symbol != "X" && model.Symbol != "O")
            {
                currentCondition.Error = "Выбран не верный символ"; //Смэрть неверным)))
                currentCondition.Code = ErrorCode.SelectedIncorrectSymbol; 
                return GetInfoSession(currentCondition, session);
            }
            if (model.Player.Id != session.Player1.Id && model.Player.Id != session.Player2.Id)
            {
                currentCondition.Error = "Неверные данные игрока";
                currentCondition.Code = ErrorCode.IncorrectPlayerData;
                return GetInfoSession(currentCondition, session);
            }
            if (model.Player.Id == session.Player1.Id)
            {
                if (model.Symbol == session.Player2Symbol)
                {
                    currentCondition.Error = "Символ уже занят";
                    currentCondition.Code = ErrorCode.SysymbolIsTaken;
                    return GetInfoSession(currentCondition, session);
                }
                else
                {
                    session.Player1Symbol = model.Symbol;
                    var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == session.Id);
                    sessionContext.Player1Symbol = session.Player1Symbol;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                if (model.Symbol == session.Player1Symbol)
                {
                    currentCondition.Error = "Символ уже занят";
                    currentCondition.Code = ErrorCode.SysymbolIsTaken;
                    return GetInfoSession(currentCondition, session);
                }
                else
                {
                    session.Player2Symbol = model.Symbol;
                    var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == session.Id);
                    sessionContext.Player2Symbol = session.Player2Symbol;
                    await _context.SaveChangesAsync();
                }
            }

            if (session.Player1Symbol == "X")
            {
                session.TurnPlayer = session.Player1;
            }
            else
            {
                session.TurnPlayer = session.Player2;
            }
            return GetInfoSession(currentCondition, session);
        }
        #endregion

        #region RemovePlayer
        public async Task<ConditionSessionModel> RemovePlayer(RemovePlayerModel model)
        {
            var currentCondition = new ConditionSessionModel();
            var session = await Task.Run(() => FindSession(model.SessionId));
            if (!SessionIsActive(currentCondition, session))
            {
                return currentCondition;
            }
            if (session.Player1 != null && session.Player2 != null)
            {
                if (session.Player1.Id == model.Player.Id)
                {
                    session.Winner = session.Player2;
                }
                else
                {
                    session.Winner = session.Player1;
                }
            }
            session.IsClosed = true;
            return GetInfoSession(currentCondition, session);
        }
        #endregion


        private GameSession FindSession(int sessionId)
        {
            var session = _gameSessions.FirstOrDefault(x => x.Id == sessionId);
            return session;
        }

        public async Task<ConditionSessionModel> GetCurrentTurn(int sessionId)
        {
            var session = await Task.Run(() => FindSession(sessionId));
            var conditionSession = new ConditionSessionModel();
            if (!SessionIsActive(conditionSession, session))
            {
                return conditionSession;
            }
            
            return GetInfoSession(conditionSession, session);
        }
        
        public async Task<bool[]> CheckAllPlayers(int sessionId)
        {
            var session = await Task.Run(() => FindSession(sessionId));
            return new bool[] { session.Player1 != null, session.Player2 != null };
        }

        private async void CloseGameSession(GameSession session)
        {
            await Task.Delay(30000);
            _gameSessions.Remove(session);
        }

        private bool SessionIsActive(ConditionSessionModel model, GameSession session)
        {
            if (session == null)
            {
                model.Error = "Сессия не найдена";
                model.Code = ErrorCode.SessionNotFound;
                return false;
            }
            return true;
        }
        private ConditionSessionModel GetInfoSession(ConditionSessionModel model, GameSession session)
        {
            model.Player1 = session.Player1;
            model.Player2 = session.Player2;
            model.Playe1Symbol = session.Player1Symbol;
            model.Playe2Symbol = session.Player2Symbol;
            model.TurnPlayer = session.TurnPlayer;
            model.Field = session.Field;
            model.Winner = session.Winner;
            model.IsGameOver = session.IsClosed;
            return model;
        }
    }
}
