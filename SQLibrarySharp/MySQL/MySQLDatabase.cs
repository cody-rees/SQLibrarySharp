using MySql.Data.MySqlClient;
using SQLibrary.System;
using SQLibrary.System.Logging;
using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        public override ResultMap ExecuteQuery(string query, Dictionary<string, object> parameters) {
            if (output != null) {
                output.WriteLine(query);
            }

            MySqlCommand command = new MySqlCommand(query, Connection);
            command.CommandType = CommandType.Text;

            foreach (string key in parameters.Keys) {
                command.Parameters.AddWithValue(key, parameters[key]);
            }

            return MapCommandResult(command);
        }

        public override bool ExecuteUpdate(string update, Dictionary<string, object> parameters) {
            if (output != null) {
                output.WriteLine(update);
            }

            MySqlCommand command = new MySqlCommand(update, Connection);
            command.CommandType = CommandType.Text;

            foreach (string key in parameters.Keys) {
                command.Parameters.AddWithValue(key, parameters[key]);
            }

            try {
                command.ExecuteNonQuery();
                return true;

            }
            catch (Exception e) {
                Logger.Warning("Failed to execute Query", this.exception = e);
                return false;
            }
            
        }


        public override string BuildConditionSQL(List<Condition> conditions, ref Dictionary<string, object> parameters) {

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
                        conditionStr = ((StaticCondition)condition).SQL;
                        continue;
                    }

                    conditionStr += String.Format(" {0} {1)",
                        condition.Relation.ToString(), ((StaticCondition)condition).SQL);

                } else if (condition is SubsetCondition) {
                    if (conditionStr == null) {
                        conditionStr = String.Format("(0)",
                            BuildConditionSQL(((SubsetCondition)condition).Conditions, ref parameters));

                        continue;
                    }

                    conditionStr += String.Format(" {0} ({1})", condition.Relation.ToString(),
                        BuildConditionSQL(((SubsetCondition)condition).Conditions, ref parameters));
                }

            }

            return conditionStr;
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


        private string FormatParameterCondition(ParameterCondition paramCond, ref Dictionary<String, Object> parameters) {

            string param1 = null;
            string param2 = null;

            //Param1 Formatting Default FieldEscape
            switch (paramCond.Param1Formatting) {
                case ParameterFormat.PREPARED_STATEMENT_ARGUMENT:
                    param1 = String.Format("@val{0}", (parameters.Count + 1) );

                    parameters.Add("@val" + (parameters.Count + 1), paramCond.Param1);
                    break;

                case ParameterFormat.RAW:
                    param1 = paramCond.Param1;
                    break;

                default:
                    param1 = FieldEscape(paramCond.Param1);
                    break;

            }

            //Param2 Formatting Default Value Escape
            switch (paramCond.Param2Formatting) {
                case ParameterFormat.ESCAPE_PARAMETER_AS_FIELD:
                    param2 = FieldEscape(paramCond.Param2);
                    break;

                case ParameterFormat.RAW:
                    param2 = paramCond.Param2;
                    break;
                    
                default:
                    param2 = String.Format("@val{0}", (parameters.Count + 1));
                    parameters.Add("@val" + (parameters.Count + 1), paramCond.Param2);
                    break;
                
            }

            return String.Format("{0} {1} {2}", param1, paramCond.Operator, param2);
        }
        
        public static string EscapeValue(string value, int format, ref Dictionary<string, object> parameters) {

            string param = null;
            switch (format) {
                case ParameterFormat.RAW:
                    param = value;
                    break;

                case ParameterFormat.ESCAPE_PARAMETER_AS_FIELD:
                    param = MySQLConnection.FieldEscape(value);
                    break;

                default:
                    param = String.Format("@val{0}", (parameters.Count + 1));
                    parameters.Add("@val" + (parameters.Count + 1), value);
                    break;

            }

            return param;
        }

        public override SQLSelect Select(string table) {
            return new MySQLSelect(table, this);
        }

        public override SQLUpdate Update(string table) {
            return new MySQLUpdate(table, this);
        }

        public override SQLDelete Delete(string table) {
            return new MySQLDelete(table, this);
        }

        public override SQLInsert Insert(string table, params string[] fields) {
            return new MySQLInsert(table, this, fields);
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
            if (fieldSQL == null && Fields != null && Fields.Count() > 0) {
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

            if (base.Conditions.Count() < 1) {
                Console.WriteLine(String.Format("SELECT {0} FROM {1}", fieldSQL, Table));
                return Database.ExecuteQuery(String.Format("SELECT {0} FROM {1}", fieldSQL, Table));
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string query = String.Format("SELECT {0} FROM {1} WHERE {2}", fieldSQL, Table,
                Database.BuildConditionSQL(base.Conditions, ref parameters));

            return Database.ExecuteQuery(query, parameters);
        }

    }

    public class MySQLUpdate : SQLUpdate {

        public MySQLUpdate(string table, MySQLConnection database) : base(table, database) { }

        public override bool Execute() {

            List<string> setFields = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            foreach (FieldUpdate field in base.FieldUpdates) {
                string fieldEscaped = MySQLConnection.FieldEscape(field.Field);
                string value = MySQLConnection.EscapeValue(field.Value, field.ValueFormat, ref parameters);
                setFields.Add(String.Format("{0}={1}", fieldEscaped, value));
            }

            string fieldSQL = String.Join(", ", setFields);
            if (base.Conditions.Count() < 1) {
                return Database.ExecuteUpdate(String.Format("UPDATE {0} SET {1}", fieldSQL, Table));
            }

            string query = String.Format("UPDATE {0} SET {1} WHERE {2}", base.Table, fieldSQL,
                Database.BuildConditionSQL(base.Conditions, ref parameters));

            return Database.ExecuteUpdate(query, parameters);
        }
        
    }

    public class MySQLDelete : SQLDelete {

        public MySQLDelete(string table, MySQLConnection database) : base(table, database) { }

        public override Boolean Execute() {
            if (base.Conditions.Count() < 1) {
                return Database.ExecuteUpdate(String.Format("DELETE FROM {0}", Table));
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            String query = String.Format("DELETE FROM {0} WHERE {1}", base.Table,
                Database.BuildConditionSQL(base.Conditions, ref parameters));

            return Database.ExecuteUpdate(query, parameters);
        }
    }

    public class MySQLInsert : SQLInsert {

        public MySQLInsert(string table, MySQLConnection database) : base(table, database) { }
        public MySQLInsert(string table, MySQLConnection database, params string[] fields) 
            : base(table, database, fields) { }

        public override Boolean Execute() {

            List<string> sqlValueList = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            foreach (object[] valuesArr in base.ValuesList) {
                if (base.Fields != null && base.Fields.Count() != valuesArr.Count()) {
                    throw new ArgumentException("Provided ValueSet does not match the size of Field List");
                }

                List<string> values = new List<string>();
                foreach (object value in valuesArr) {
                    if (value is Array) {
                        object[] valuePair = (object[]) value;
                        values.Add(MySQLConnection.EscapeValue(
                            valuePair[0].ToString(), (int) valuePair[1], ref parameters)
                        );

                        continue;
                    }

                    if (value == null) {
                        values.Add("NULL");
                        continue;
                    }

                    values.Add(MySQLConnection.EscapeValue(
                        value.ToString(), ParameterFormat.PREPARED_STATEMENT_ARGUMENT, ref parameters)
                    );
                }

                sqlValueList.Add(String.Format("({0})", String.Join(", ", values)));
            }

            String query;
            if (base.Fields == null || base.Fields.Count() < 1) {
                query = String.Format("INSERT INTO {0} VALUES {1}", base.Table,
                    String.Join(",", sqlValueList));

            } else {
                List<string> fields = new List<String>();
                foreach (String field in base.Fields) {
                    fields.Add(MySQLConnection.FieldEscape(field));
                }

                query = String.Format("INSERT INTO {0} ({1}) VALUES {2}", base.Table, 
                    String.Join(", ", fields), String.Join(", ", sqlValueList));

            }
            
            return Database.ExecuteUpdate(query, parameters);
        }
    }

    

}
