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
using System.Linq.Expressions;
using SQLibrary.System.Condition;

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

            return MapCommandResult(command);
        }

        public ResultMap MapCommandResult(MySqlCommand command) {
            try {
                using (MySqlDataReader reader = command.ExecuteReader()) {
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
                    
                    //Select Last in Query
                    return results[results.Count - 1];
                }
            } catch (Exception e) {
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


        public override ResultMap ExecuteConditionalQuery(string query, SQLConditional conditional, ref Dictionary<string, object> parameters) {
            string conditionalSQL = BuildConditionSQL(conditional.Conditions, ref parameters);

            MySqlCommand command = new MySqlCommand(String.Format(query, conditionalSQL), Connection);
            if (output != null) {
                output.WriteLine(command.CommandText);
            }

            foreach (string key in parameters.Keys) {
                command.Parameters.AddWithValue(key, parameters[key]);
            } 
            
            command.Prepare();
            return MapCommandResult(command);
        }


        public override Boolean ExecuteConditionalUpdate(string query, SQLConditional conditional, ref Dictionary<string, object> parameters) {
            string conditionalSQL = BuildConditionSQL(conditional.Conditions, ref parameters);

            MySqlCommand command = new MySqlCommand(String.Format(query, conditionalSQL), Connection);
            if (output != null) {
                output.WriteLine(command.CommandText);
            }

            foreach (string key in parameters.Keys) {
                command.Parameters.AddWithValue(key, parameters[key]);
            }

            command.Prepare();

            try {
                command.ExecuteNonQuery();
                return true;

            } catch (Exception e) {
                Logger.Warning("Failed to execute Query", this.exception = e);
                return false;
            }
        }



        private string BuildConditionSQL(List<Condition> conditions, ref Dictionary<string, object> parameters) {

            String conditionStr = null;
            foreach (Condition condition in conditions) {
                if (condition is ParameterCondition) {

                    string paramCondStr = FormatParameterCondition((ParameterCondition)condition, ref parameters);
                    if (conditionStr == null) {
                        conditionStr = paramCondStr;
                        continue;
                    }

                    conditionStr += String.Format(" {0} {1}",
                        condition.Relation.ToString(), paramCondStr);

                } else if (condition is StaticCondition) {
                    if (conditionStr == null) {
                        conditionStr = ((StaticCondition) condition).SQL;
                        continue;
                    }

                    conditionStr += String.Format(" {0} {1)",
                        condition.Relation.ToString(), ((StaticCondition) condition).SQL);

                } else if (condition is SubsetCondition) {
                    if (conditionStr == null) {
                        conditionStr = String.Format("(0)", 
                            BuildConditionSQL(((SubsetCondition) condition).Conditions, ref parameters));

                        continue;
                    }

                    conditionStr += String.Format(" {0} ({1})", condition.Relation.ToString(), 
                        BuildConditionSQL(((SubsetCondition) condition).Conditions, ref parameters));
                }
                
            }

            return conditionStr;
        }

        private string FormatParameterCondition(ParameterCondition paramCond, ref Dictionary<String, Object> parameters) {

            string param1 = null;
            string param2 = null;

            //Param1 Formatting Default FieldEscape
            switch (paramCond.Param1Formatting) {
                case ParameterCondition.FORMAT_PARAMETER_VALUE:
                    param1 = String.Format("@val{0}", (parameters.Count + 1) );

                    parameters.Add("@val" + (parameters.Count + 1), paramCond.Param1);
                    break;

                case ParameterCondition.FORMAT_PARAMETER_RAW:
                    param1 = paramCond.Param1;
                    break;

                default:
                    param1 = FieldEscape(paramCond.Param1);
                    break;

            }

            //Param2 Formatting Default Value Escape
            switch (paramCond.Param2Formatting) {
                case ParameterCondition.FORMAT_PARAMETER_FIELD:
                    param2 = FieldEscape(paramCond.Param2);
                    break;

                case ParameterCondition.FORMAT_PARAMETER_RAW:
                    param2 = paramCond.Param2;
                    break;
                    
                default:
                    param2 = String.Format("@val{0}", (parameters.Count + 1));
                    parameters.Add("@val" + (parameters.Count + 1), paramCond.Param2);
                    break;
                
            }

            return String.Format("{0} {1} {2}", param1, paramCond.Operator, param2);
        }

        public override SQLSelect Select(string table) {
            return new MySQLSelect(table, this);
        }

        public override SQLUpdate Update(string table) {
            return new MySQLUpdate(table, this);
        }

        public override SQLDelete Delete(string table) {
            return null;
        }

        public override SQLInsert Insert(string table, params string[] fields) {
            return null;
        }

        public static string FieldEscape(string field) {
            return String.Format("`{0}`", field.Replace(".", "`.`"));
        }
    }

    public class MySQLSelect : SQLSelect {

        public MySQLSelect(string table, MySQLConnection database) : base(table, database) { }
        
        public override ResultMap Execute() {
            //Build FieldSQL for parameters
            string fieldSQL = FieldSQL;
            if (fieldSQL == null && Fields != null) {
                foreach (string field in Fields) {
                    if (fieldSQL == null) {
                        FieldSQL = MySQLConnection.FieldEscape(field);
                        continue;
                    }

                    fieldSQL += ", " + MySQLConnection.FieldEscape(field);
                }


            } else {
                fieldSQL = "*";
            }

            string query = String.Format("SELECT {0} FROM {1} WHERE ", fieldSQL, Table) + "{0}";
            return Database.ExecuteConditionalQuery(query, this);
        }

    }

    public class MySQLUpdate : SQLUpdate {

        public MySQLUpdate(string table, MySQLConnection database) : base(table, database) { }

        public override bool Execute() {

            List<string> setFields = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            foreach (FieldUpdate field in base.FieldUpdates) {
                string fieldEscaped = MySQLConnection.FieldEscape(field.Field);
                string value = EscapeValue(field.Value, field.ValueFormat, ref parameters);
                setFields.Add(String.Format("{0}={1}", fieldEscaped, value));
            }

            string fieldSQL = String.Join(", ", setFields);
            string query = String.Format("UPDATE {0} SET {1} WHERE ", base.Table, fieldSQL) + "{0}";

            return Database.ExecuteConditionalUpdate(query, this, ref parameters);
        }

        public string EscapeValue(string value, int format, ref Dictionary<string, object> parameters) {
            
            string param = null;
            switch (format) {
                case ParameterCondition.FORMAT_PARAMETER_RAW:
                    param = value;
                    break;

                default:
                    param = String.Format("@val{0}", (parameters.Count + 1));
                    parameters.Add("@val" + (parameters.Count + 1), value);
                    break;

            }

            return param;

        }
        
    }


    

}
