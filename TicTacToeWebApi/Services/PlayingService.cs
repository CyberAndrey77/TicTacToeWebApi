using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
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
        private float _turnTime;

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
            bool IsConnectPlayer1 = false;
            for (int i = 0; i < _gameSessions.Count; i++)
            {
                //Пока не понял как избавиться от этого дублирования
                if (_gameSessions[i].Player1 == null)
                {
                    IsConnectPlayer1 = true;
                    _gameSessions[i].Player1 = player;
                    index = i;
                    sessionId = _gameSessions[i].Id; break;
                }
                if (_gameSessions[i].Player2 == null)
                {
                    IsConnectPlayer1 = false;
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
            if (IsConnectPlayer1)
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
            var turnModel = new ConditionSessionModel();
            var session = await Task.Run(() => FindSession(model.SessionId));
            if (!SessionIsActive(turnModel, session))
            {
                return turnModel;
            }
            if (session.IsClosed)
            {
                turnModel.Error = $"Игра окончена";
                turnModel.Code = ErrorCode.GameOver;
                return turnModel;
            }
            if (session.Player1 == null || session.Player2 == null)
            {
                turnModel.Error = $"Ожидание второго игрока";
                turnModel.Code = ErrorCode.WaitPlayer;
                return turnModel;
            }
            if (session.TurnPlayer.Id != model.Player.Id)
            {
                turnModel.TurnPlayer = session.TurnPlayer;
                turnModel.Field = session.Field;
                return turnModel;
            }
            var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == model.SessionId);

            var symbol = session.TurnPlayer == session.Player1 ?
                session.Player1Symbol : session.Player2Symbol;
            if (session.Field[model.Position] == null)
            {
                session.Field[model.Position] = symbol;
            }
            else
            {
                turnModel.TurnPlayer = session.TurnPlayer;
                turnModel.Field = session.Field;
                turnModel.Error = "Место уже занято";
                return turnModel;
            }
            

            turnModel.Field = session.Field;
            bool isWinner = await Task.Run(() => HasWinner(symbol, session));
            bool isGameOver = await Task.Run(() => IsGameOver(session));
            if (isWinner || isGameOver)
            {
                if (isWinner)
                {
                    session.Winner = session.TurnPlayer;
                    sessionContext.Winner = session.TurnPlayer;
                    turnModel.Winner = session.TurnPlayer;
                }
                session.IsClosed = true;
                sessionContext.IsClosed = true;
                Task.Run(() => CloseGameSession(session));
            }
            else
            {
                session.TurnPlayer = session.TurnPlayer != session.Player1 ?
                session.Player1 : session.Player2;
                turnModel.TurnPlayer = session.TurnPlayer;
            }
            sessionContext.Turns.Add(new Turn(model.Player, symbol, model.Position));
            _context.SaveChangesAsync();
            return turnModel;
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
            if (model.Symbol != "X" && model.Symbol != "O")
            {
                currentCondition.Error = "Выбран не верный символ"; //Смэрть неверным)))
                currentCondition.Code = ErrorCode.SelectedIncorrectSymbol; 
                return currentCondition;
            }
            var session = await Task.Run(() => FindSession(model.SessionId));
            if (!SessionIsActive(currentCondition, session))
            {
                return currentCondition;
            }
            if (model.PlayerId == session.Player1.Id)
            {
                if (model.Symbol == session.Player2Symbol)
                {
                    currentCondition.Error = "Символ уже занят";
                    currentCondition.Code = ErrorCode.SysymbolIsTaken;
                    return currentCondition;
                }
                else
                {
                    session.Player1Symbol = model.Symbol;
                    currentCondition.Playe1Symbol = model.Symbol;
                    var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == session.Id);
                    sessionContext.Player1Symbol = session.Player1Symbol;
                    _context.SaveChangesAsync();
                }
            }
            else
            {
                if (model.Symbol == session.Player1Symbol)
                {
                    currentCondition.Error = "Символ уже занят";
                    currentCondition.Code = ErrorCode.SysymbolIsTaken;
                    return currentCondition;
                }
                else
                {
                    session.Player2Symbol = model.Symbol;
                    currentCondition.Playe2Symbol = model.Symbol;
                    var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == session.Id);
                    sessionContext.Player2Symbol = session.Player2Symbol;
                    _context.SaveChangesAsync();
                }
            }
            if (session.Player1Symbol == "X")
            {
                session.TurnPlayer = session.Player1;
                currentCondition.TurnPlayer = session.Player1;
            }
            else
            {
                session.TurnPlayer = session.Player2;
                currentCondition.TurnPlayer = session.Player2;
            }
            return currentCondition;
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
            return new ConditionSessionModel() 
            { 
                TurnPlayer = session.TurnPlayer, Field = session.Field, Winner = session.Winner 
            };
        }
        
        public async Task<bool[]> CheckAllPlayers(int sessionId)
        {
            var session = await Task.Run(() => FindSession(sessionId));
            return new bool[] { session.Player1 != null, session.Player2 != null };
        }

        private async void CloseGameSession(GameSession session)
        {
            await Task.Delay(60000);
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
    }
}
