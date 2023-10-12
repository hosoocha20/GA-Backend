using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using A2.Models;


namespace A2.Data
{
    public class A2Repo : IA2Repo
    {
        private readonly A2DBContext _dbContext;

        public A2Repo(A2DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IEnumerable<User> GetAllUsers()
        {
            IEnumerable<User> users = _dbContext.Users.ToList<User>();
            return users;
        }
        public User AddUser(User user)
        {
            EntityEntry<User> e = _dbContext.Users.Add(user);
            User u = e.Entity;
            _dbContext.SaveChanges();
            return u;
        }

        public bool ValidLogin(string userName, string password)
        {
            User u = _dbContext.Users.FirstOrDefault(e => e.UserName == userName && e.Password == password);
            if (u == null)
                return false;
            else
                return true;
        }

        public IEnumerable<GameRecord> GetAllGameRecords()
        {
            IEnumerable<GameRecord> gameRecords = _dbContext.GameRecords.ToList<GameRecord>();
            return gameRecords;
        }

        public GameRecord UpdateGameRecord(GameRecord g, string userName)
        {

            g.State = "progress";
            g.Player2 = userName;

            _dbContext.SaveChanges();
            return g;
        }

        public GameRecord AddGameRecord(GameRecord record)
        {
            EntityEntry<GameRecord> e = _dbContext.GameRecords.Add(record);
            GameRecord g = e.Entity;
            _dbContext.SaveChanges();
            return g;
        }

        public void UpdateGameRecordMove(GameRecord g, string move, string player)
        {
            if (player == "Player1")
            {
                g.LastMovePlayer1 = move;
                g.LastMovePlayer2 = null;
            }
            else
            {
                g.LastMovePlayer2 = move;
                g.LastMovePlayer1 = null;
            }
            _dbContext.SaveChanges();
        }

        public void DeleteGameRecord(GameRecord record)
        {
            EntityEntry<GameRecord> e = _dbContext.GameRecords.Remove(record);
            _dbContext.SaveChanges();
        }

    }
}