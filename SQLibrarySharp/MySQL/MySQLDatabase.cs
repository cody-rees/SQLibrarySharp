using MySql.Data.MySqlClient;
using SQLibrary.System;
using SQLibrary.System.Logging;
using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLibrary.ORM;

namespace SQLibrary.MySQL {

    public class MySQLConnection : Database { 

        public static readonly Logger Logger = new Logger("MySQLConnection");

        private MySqlConnection Connection;
        private readonly String ConnectionString;


        public MySQLConnection(String host, String user, String pass, String db) {
            ConnectionString = "server={0};uid={1};pwd={2};database={3};";
            ConnectionString = String.Format(ConnectionString, host, user, pass, db);
            
        }

        public override bool OpenConnection() {
            try {
                Connection = new MySqlConnection();
                Connection.ConnectionString = ConnectionString;
                Connection.Open();

            }
            catch (MySqlException e) {
                Logger.Severe("Failed to Open Database Connection", e);
                return false;
            }

            return true;
        }

        public override void CloseConnection() {
            Connection.Close();
        }

        public override ResultMap ExecuteQuery(string query) {
            if (output != null) {
                output.WriteLine(query);
            }

            MySqlCommand command = new MySqlCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.Connection = Connection;

            try {
                MySqlDataReader reader = command.ExecuteReader();
                List<ResultMap> results = new List<ResultMap>();

                //Ensures last result is selected
                bool nextResult = true;
                while (nextResult) {
                    String[] headers = new String[reader.FieldCount];
                    Type[] types = new Type[reader.FieldCount];

                    for (int i = 0; i < reader.FieldCount; i++) {
                        headers[i] = reader.GetName(i);
                        types[i] = reader.GetFieldType(i);
                    }

                    ResultMap map = new ResultMap(headers, types);
                    while (reader.Read()) {
                        object[] values = new object[reader.FieldCount];
                        reader.GetValues(values);

                        map.AddResult(values);
                        Console.WriteLine();
                    }

                    nextResult = reader.NextResult();
                    results.Add(map);
                }

                reader.Close();
                return results[results.Count - 1];
            }
            catch (Exception e) {
                Logger.Warning("Failed to execute Query", this.exception = e);
                return null;
            }
        }

        public override bool ExecuteUpdate(string update) {
            if (output != null) {
                output.WriteLine(update);
            }

            MySqlCommand command = new MySqlCommand();
            command.CommandText = update;
            command.CommandType = CommandType.Text;
            command.Connection = Connection;

            try {
                command.ExecuteNonQuery();
                return true;

            }
            catch (Exception e) {
                Logger.Warning("Failed to execute Query", this.exception = e);
                return false;
            }
        }
    }

}
