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

        public static readonly Logger _Logger = new Logger("MySQLConnection");

        private MySqlConnection connection;
        private readonly String connectionString;


        public MySQLConnection(String host, String user, String pass, String db) {
            connectionString = "server={0};uid={1};pwd={2};database={3};";
            connectionString = String.Format(connectionString, host, user, pass, db);
            
        }

        public override bool OpenConnection() {
            try {
                connection = new MySqlConnection();
                connection.ConnectionString = connectionString;
                connection.Open();

            }
            catch (MySqlException e) {
                _Logger.Severe("Failed to Open Database Connection", e);
                return false;
            }

            return true;
        }

        public override void CloseConnection() {
            connection.Close();
        }

        public override ResultMap ExecuteQuery(string query) {
            if (output != null) {
                output.WriteLine(query);
            }

            MySqlCommand command = new MySqlCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.Connection = connection;

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
                _Logger.Warning("Failed to execute Query", this.exception = e);
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
            command.Connection = connection;

            try {
                command.ExecuteNonQuery();
                return true;

            }
            catch (Exception e) {
                _Logger.Warning("Failed to execute Query", this.exception = e);
                return false;
            }
        }
    }

}
