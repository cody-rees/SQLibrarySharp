using SQLibrary.MySQL;
using SQLibrary.ORM;
using SQLibrary.System;
using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary_Test {

    class SQLibraryTest {

        static void Main(string[] args) {
            new SQLibraryTest();
        }

        public Database Connection;

        public SQLibraryTest() {
            Console.WriteLine("SQLibrarySharp testing enviroment.");

            Connection = new MySQLConnection("localhost", "root", "", "sqlibrary");
            Connection.OpenConnection();
            Connection.SetOutput(Console.Out);

            //MySQL Select
            //TestMySQLSelect();

            //MySQL Update
            //TestMySQLUpdate(); 

            //MySQL Delete
            //TestMySQLDelete();

            //MySQL Insert
            //TestMySQLInsert();

            // MYSQL Model Insert Test
            PlayerModel insertPlayer = new PlayerModel();
            insertPlayer.PlayerUUID = "1234-1234-51234123515-1234";
            insertPlayer.Balance = 230;
            insertPlayer.Save();
            
            // MySQL Model Select 
            PlayerModel player = Model.Find<PlayerModel>(1);
            if (player == null) {
                Console.WriteLine("Could not find player");
                Console.Read();
            }

            //MySQL Model Update Test
            player.Balance = 120;
            player.Save();

            //MySQL Delete Test
            Model.Find<PlayerModel>(9).Delete();

            Console.Read();
        }
    
        [Table(Name = "players")] 
        public class PlayerModel : Model {

            [PrimaryKey(Key = "pk_player")]
            [Fillable(Field = "player_id")]
            public Nullable<int> PlayerID;

            [Fillable(Field = "player_uuid")]
            public string PlayerUUID;

            [Fillable(Field = "balance")]
            public int Balance;

            public override string ToString() { 
                return string.Format("id:{0} uuid:{1} balance:{2}", PlayerID, PlayerUUID, Balance);
            }
        }


        public void TestMySQLInsert() {
            MySQLInsert insert = (MySQLInsert) Connection.Insert("players", "player_uuid", "balance");
            insert.Values("asdf-qwer-qwer-qwer", 200);
            insert.Values("asdf-qwer-qwer-zxcv", 100);
            insert.Values("asdf-qwer-qwer-mnvb", 120); 

            Console.WriteLine("Update Test Results: " + insert.Execute());
        }

        public void TestMySQLDelete() {
            MySQLDelete delete = (MySQLDelete) Connection.Delete("players");
            delete.Where("player_id", 2);

            Console.WriteLine("Delete Test Results: " + delete.Execute());
        }

        public void TestMySQLUpdate() {
            MySQLUpdate update = (MySQLUpdate) Connection.Update("players");
            update.Set("balance", 200);
            update.Where("player_id", 2);

            Console.WriteLine("Update Test Results: " + update.Execute());
        }

        public void TestMySQLSelect() {
            MySQLSelect select = (MySQLSelect) Connection.Select("players");
            
            // WHERE player_id > 1 AND (balance > 50 OR balance = 0)
            select.Where("player_id", ">", 1)
                .Where(new ConditionBuilder()
                    .Where("balance", ">", 50)
                    .OrWhere("balance", 0)
                );
            
            ResultMap results = select.Execute();
            PrintResult(results);
        }


        public void PrintResult(ResultMap map) {
            if (map == null) {
                Console.WriteLine(Connection.GetLastException());
                return;
            }

            Console.Write(string.Format("Table Headers ({0} Results): ", map.Results.Count));
            foreach (String header in map.Headers) {
                Console.Write(header + ", ");
            }

            Console.WriteLine();

            foreach (Result result in map.Results) {
                foreach (Object value in result.Values) {
                    Console.Write(value + ", ");
                }

                Console.WriteLine();
            }

        }

    }
}
