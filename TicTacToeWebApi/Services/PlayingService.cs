using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
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
        public async Task<CurrentTurnModel> MakeTurn(TurnModel model)
        {
            var session = await Task.Run(() => FindSession(model.SessionId));
            if (session.Player1 == null || session.Player2 == null)
            {
                throw new ArgumentNullException("Нет второго игрока");
            }
            var turnModel = new CurrentTurnModel();
            if (session.TurnPlayer.Id != model.Player.Id)
            {
                turnModel.TurnPlayer = session.TurnPlayer;
                turnModel.Field = session.Field;
                return turnModel;
            }
            var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == model.SessionId);

            var symbol = session.TurnPlayer == session.Player1 ?
                session.Player1Symbol : session.Player2Symbol;

            session.Field[model.Position] = symbol;

            turnModel.Field = session.Field;
            bool isWinner = await Task.Run(() => CheckWinner(symbol, session));
            if (isWinner)
            {
                session.Winner = session.TurnPlayer;
                sessionContext.Winner = session.TurnPlayer;
                turnModel.Winner = session.TurnPlayer;
                _gameSessions.Remove(session);
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

        private bool CheckWinner(string symbol, GameSession session)
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

        #endregion

        #region SelectSymbol
        public async Task<bool> SelectSymbol(SelectSymbolModel model)
        {
            //Нолик это символ О, а не цифра нуль, нолик тоненький)
            if (model.Symbol != "X" && model.Symbol != "O")
            {
                throw new ArgumentException("Выбран не верный символ"); //Смэрть неверным)))
            }
            var session = await Task.Run(() => FindSession(model.SessionId));

            if (model.PlayerId == session.Player1.Id)
            {
                if (model.Symbol == session?.Player2Symbol)
                {
                    return false;
                }
                else
                {
                    session.Player1Symbol = model.Symbol;
                    var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == session.Id);
                    sessionContext.Player1Symbol = session.Player1Symbol;
                    _context.SaveChangesAsync();
                }
            }
            else
            {
                if (model.Symbol == session?.Player1Symbol)
                {
                    return false;
                }
                else
                {
                    session.Player2Symbol = model.Symbol;
                    var sessionContext = await _context.GameSessions.FirstOrDefaultAsync(x => x.Id == session.Id);
                    sessionContext.Player2Symbol = session.Player2Symbol;
                    _context.SaveChangesAsync();
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
            return true;
        }
        #endregion

        private GameSession FindSession(int sessionId)
        {
            var session = _gameSessions.FirstOrDefault(x => x.Id == sessionId);
            if (session == null)
            {
                throw new ArgumentNullException("Сессии не существует");
            }
            return session;
        }

        public async Task<CurrentTurnModel> GetCurrentTurn(int sessionId)
        {
            var session = await Task.Run(() => FindSession(sessionId));
            return new CurrentTurnModel() 
            { 
                TurnPlayer = session.TurnPlayer, Field = session.Field, Winner = session.Winner 
            };
        }
        
        public async Task<bool[]> CheckAllPlayers(int sessionId)
        {
            var session = await Task.Run(() => FindSession(sessionId));
            return new bool[] { session.Player1 != null, session.Player2 != null };
        }
    }
}
