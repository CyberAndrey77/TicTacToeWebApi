using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TicTacToeWebApi.Models;
using TicTacToeWebApi.Services.Interfaces;

namespace TicTacToeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IAutorizationService _autorization;
        private readonly IPlayingService _playingService;

        public GameController(IAutorizationService autorization, IPlayingService playingService)
        {
            _autorization = autorization;
            _playingService = playingService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model) 
        {
            Player player = await _autorization.Login(model);
            if (player == null) 
            { 
                return BadRequest("Не получилось войти"); 
            }
            
            try
            {
                int sessionId = await _playingService.AddPlayer(player);

                return Ok(new { player, sessionId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest("Не получилось присоедениться к игровой сессии");
            }
        }

        [HttpPost("select_symbol")]
        public async Task<IActionResult> SelectSymbol(SelectSymbolModel model)
        {
            try
            {
                var answer = await _playingService.SelectSymbol(model);

                return Ok(answer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("get_check_all_players_connect")]
        public async Task<IActionResult> GetCheckAllPlayersConnect(int sessionId)
        {
            try
            {
                bool[] playersConnect = await _playingService.CheckAllPlayers(sessionId);
                return Ok(new {Player1 = playersConnect[0], Player2 = playersConnect[1]});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get_current_turn")]
        public async Task<IActionResult> GetCurrentTurn(int sessionId)
        {
            try
            {
                return Ok(await _playingService.GetCurrentTurn(sessionId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("make_turn")]
        public async Task<IActionResult> MakeTurn(TurnModel model)
        {
            try
            {
                return Ok(await _playingService.MakeTurn(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect(RemovePlayerModel model)
        {
            try
            {
                return Ok(await _playingService.RemovePlayer(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
