using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using A2.Models;
using A2.Data;
using A2.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace A2.Controllers
{
    [Route("api")]
    [ApiController]
    public class A2Controller : Controller
    {

        private readonly IA2Repo _repository;

        public A2Controller(IA2Repo repository)
        {
            _repository = repository;
        }
        //Endpoint 1
        [HttpPost("Register")]
        public ActionResult Register(User register)
        {
            User u;
            string output;
            IEnumerable<User> users = _repository.GetAllUsers();
            var res = users.Any(p => p.UserName == register.UserName);
            if (res)
            {
                output = "Username not available.";
            }
            else
            {
                u = new User { UserName = register.UserName, Password = register.Password, Address = register.Address };
                User user = _repository.AddUser(u);
                output = "User successfully registered.";
            }
            return Ok(output);
        }

        //Endpoint 2
        [Authorize(AuthenticationSchemes = "MyAuthentication")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet("GetVersionA")]
        public ActionResult GetVersionA()
        {
            ClaimsIdentity ci = HttpContext.User.Identities.FirstOrDefault();
            Claim c = ci.FindFirst("userName");

            string s = "1.0.0 (auth)";



            return Ok(s);
        }

        //Endpoint 3
        [Authorize(AuthenticationSchemes = "MyAuthentication")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet("PurchaseItem/{id}")]
        public ActionResult<IEnumerable<Order>> PurchaseItem(int id)
        {
            ClaimsIdentity ci = HttpContext.User.Identities.FirstOrDefault();
            Claim c = ci.FindFirst("userName");

            Order o = new Order { UserName = c.Value, ProductId = id };



            return Ok(o);
        }

        //Endpoint 4
        [Authorize(AuthenticationSchemes = "MyAuthentication")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet("PairMe")]
        public ActionResult<IEnumerable<GameRecordOut>> PairMe()
        {
            ClaimsIdentity ci = HttpContext.User.Identities.FirstOrDefault();
            Claim c = ci.FindFirst("userName");

            IEnumerable<GameRecord> gameRecords = _repository.GetAllGameRecords();
            GameRecord g;
            GameRecordOut gOut;
            var res = gameRecords.Any(p => p.State == "wait" && p.Player1 != c.Value);
            if (res)
            {
                GameRecord gWait = gameRecords.FirstOrDefault(e => e.State == "wait");
                GameRecord update = _repository.UpdateGameRecord(gWait, c.Value);
                gOut = new GameRecordOut { GameId = gWait.GameId, State = "progress", Player1 = gWait.Player1, Player2 = c.Value, LastMovePlayer1 = null, LastMovePlayer2 = null };

            }
            //Ensure only has at least one "wait" - i.e. if all the game records only contains state=progress
            else if (gameRecords.All(p => p.State == "progress"))
            {
                g = new GameRecord { GameId = System.Guid.NewGuid().ToString(), State = "wait", Player1 = c.Value, Player2 = null, LastMovePlayer1 = null, LastMovePlayer2 = null };
                GameRecord addedRecord = _repository.AddGameRecord(g);
                gOut = new GameRecordOut { GameId = g.GameId, State = "wait", Player1 = c.Value, Player2 = null, LastMovePlayer1 = null, LastMovePlayer2 = null };
            }
            else
            {
                GameRecord gWait = gameRecords.FirstOrDefault(e => e.State == "wait");
                gOut = new GameRecordOut { GameId = gWait.GameId, State = "wait", Player1 = c.Value, Player2 = null, LastMovePlayer1 = null, LastMovePlayer2 = null };
            }

            return Ok(gOut);
        }


        //EndPoint 5
        [Authorize(AuthenticationSchemes = "MyAuthentication")]
        [Authorize(Policy = "UserOnly")]
        [HttpGet("TheirMove/{gameId}")]
        public ActionResult TheirMove(string gameId)
        {
            ClaimsIdentity ci = HttpContext.User.Identities.FirstOrDefault();
            Claim c = ci.FindFirst("userName");

            string output;
            IEnumerable<GameRecord> gameRecords = _repository.GetAllGameRecords();
            var res = gameRecords.Any(p => p.GameId == gameId && (p.Player1 == c.Value || p.Player2 == c.Value) && p.State != "wait");
            if (res)
            {
                GameRecord userGame = gameRecords.FirstOrDefault(e => e.GameId == gameId);
                if (userGame.Player1 == c.Value && userGame.LastMovePlayer2 != null)
                {
                    output = userGame.LastMovePlayer2;
                }
                else if (userGame.Player2 == c.Value && userGame.LastMovePlayer1 != null)
                    output = userGame.LastMovePlayer1;
                else
                    output = "Your opponent has not moved yet.";

            }
            else if (gameRecords.Any(p => p.GameId == gameId && (p.Player1 == c.Value || p.Player2 == c.Value) && p.State == "wait"))
            {
                output = "You do not have an opponent yet.";
            }
            else if (gameRecords.Any(p => p.GameId == gameId && (p.Player1 != c.Value && p.Player2 != c.Value)))
            {
                output = "not your game id";
            }
            else
                output = "no such gameId";

            return Ok(output);

        }

        //EndPoint 6
        [Authorize(AuthenticationSchemes = "MyAuthentication")]
        [Authorize(Policy = "UserOnly")]
        [HttpPost("MyMove")]
        public ActionResult MyMove(GameMove gMove)
        {
            ClaimsIdentity ci = HttpContext.User.Identities.FirstOrDefault();
            Claim c = ci.FindFirst("userName");

            string output;
            string player;
            IEnumerable<GameRecord> gameRecords = _repository.GetAllGameRecords();
            var res = gameRecords.Any(p => p.GameId == gMove.GameId && p.State != "wait");
            if (res)
            {
                GameRecord userGame = gameRecords.FirstOrDefault(e => e.GameId == gMove.GameId);
                if (userGame.Player1 == c.Value && userGame.LastMovePlayer1 == null)
                {
                    player = "Player1";
                    _repository.UpdateGameRecordMove(userGame, gMove.Move, player);
                    output = "move registered";
                }
                else if (userGame.Player2 == c.Value && userGame.LastMovePlayer2 == null)
                {
                    player = "Player2";
                    _repository.UpdateGameRecordMove(userGame, gMove.Move, player);
                    output = "move registered";
                }
                else
                    output = "It is not your turn.";

            }
            else if (gameRecords.Any(p => p.GameId == gMove.GameId && p.State == "wait"))
            {
                output = "You do not have an opponent yet.";
            }
            else if (gameRecords.Any(p => p.GameId == gMove.GameId && (p.Player1 != c.Value && p.Player2 != c.Value)))
            {
                output = "not your game id";
            }
            else
                output = "no such game id";

            return Ok(output);
        }

        //EndPoint 7
        [Authorize(AuthenticationSchemes = "MyAuthentication")]
        [Authorize(Policy = "UserOnly")]
        [HttpPost("QuitGame/{gameId}")]
        public ActionResult QuitGame(string gameId)
        {
            ClaimsIdentity ci = HttpContext.User.Identities.FirstOrDefault();
            Claim c = ci.FindFirst("userName");

            string output;
            string player;
            IEnumerable<GameRecord> gameRecords = _repository.GetAllGameRecords();
            var res = gameRecords.Any(p => p.GameId == gameId);
            if (res)
            {
                GameRecord userGame = gameRecords.FirstOrDefault(e => e.GameId == gameId);
                if (userGame.Player1 == c.Value || userGame.Player2 == c.Value)
                {
                    _repository.DeleteGameRecord(userGame);
                    output = "game over";
                }
                else if (gameRecords.Any(p => p.Player1 != c.Value && p.Player2 != c.Value))
                {
                    output = "You have not started a game.";
                }
                else
                    output = "not your game id";

            }

            else
                output = "no such gameId";

            return Ok(output);
        }

    }
}