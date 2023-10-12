using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using A2.Models;

namespace A2.Data
{
    public interface IA2Repo
    {
        IEnumerable<User> GetAllUsers();
        User AddUser(User user);
        public bool ValidLogin(string userName, string password);

        IEnumerable<GameRecord> GetAllGameRecords();

        public GameRecord UpdateGameRecord(GameRecord g, string userName);
        public GameRecord AddGameRecord(GameRecord record);
        public void UpdateGameRecordMove(GameRecord g, string move, string player);
        public void DeleteGameRecord(GameRecord g);



    }
}