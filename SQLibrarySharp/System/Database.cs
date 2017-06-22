using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.IO;

namespace SQLibrary.System {

    public abstract class Database {

        protected TextWriter output;
        protected Exception exception;

        public abstract bool OpenConnection();
        public abstract void CloseConnection();

        public ResultMap ExecuteQuery(String query) {
            return ExecuteQuery(query, new Dictionary<string, object>());
        }

        public Boolean ExecuteUpdate(String query) {
            return ExecuteUpdate(query, new Dictionary<string, object>());
        }

        public string BuildConditionSQL(ConditionBuilder conditions, ref Dictionary<string, object> parameters) {
            return BuildConditionSQL(conditions.Conditions, ref parameters);
        }

        public abstract ResultMap ExecuteQuery(String query, Dictionary<string, object> parameters);
        public abstract Boolean ExecuteUpdate(String update, Dictionary<string, object> parameters);
        public abstract string BuildConditionSQL(List<Condition> conditions, ref Dictionary<string, object> parameters); 

        public SQLSelect Select(string table, params string[] fields) {
            SQLSelect select = Select(table);
            select.Fields = fields;
            return select;
        }

        public SQLSelect Select(string table, string fieldSQL) {
            SQLSelect select = Select(table);
            select.FieldSQL = fieldSQL;
            return select;
        }

        public abstract SQLSelect Select(string table);
        public abstract SQLUpdate Update(string table);
        public abstract SQLDelete Delete(string table);
        public abstract SQLInsert Insert(string table, params string[] fields);


        public static Database PrimaryDB { get; set; }
        

        public Database() {
            if (PrimaryDB == null) {
                PrimaryDB = this;
            }
        }

        public void SetOutput(TextWriter output) {
            this.output = output;
        }

        public Exception GetLastException() {
            return exception;
        }


    }

    public class ParameterFormat {


        /**
            Default Formatting preference, Formats parameter 2 as prepared statement argument
            int value = 1;
        */

        public const int PREPARED_STATEMENT_ARGUMENT = 1;


        /**
            Formats parameter 2 as database field reference
            int value = 2;
        */
        public const int ESCAPE_PARAMETER_AS_FIELD = 2;

        /**
            No conditional formatting, parameter 2 will be inserted directly into the prepared statement query without any formatting
            int value 3

        */
        public const int RAW = 3;


    }

    public abstract class SQLSelect : SQLConditional<SQLSelect> {

        public string Table;
        public Database Database { get; set; }

        public string[] Fields { get; set; }
        public string FieldSQL { get; set; }

        public SQLSelect(string table, Database database) {
            this.Table = table;
            this.Database = database;
        }
        
        public abstract ResultMap Execute();
        
    }

    public abstract class SQLUpdate : SQLConditional<SQLUpdate> {

        public string Table { get; set; }
        public Database Database { get; set; }
        public List<FieldUpdate> FieldUpdates;
        
        public SQLUpdate(string table, Database database) {
            this.Table = table;
            this.Database = database;
            this.FieldUpdates = new List<FieldUpdate>();
        }

        public SQLUpdate Set(string field, object value) {
            FieldUpdates.Add(new FieldUpdate(field, value));
            return this;     
        }

        public SQLUpdate Set(string field, object value, int formatting) {
            FieldUpdates.Add(new FieldUpdate(field, value, formatting));
            return this;
        }

        public class FieldUpdate {

            public string Field { get; }
            public string Value { get; }
            
            public int ValueFormat { get; }

            public FieldUpdate(string field, object value) :
                this(field, value.ToString(), ParameterFormat.PREPARED_STATEMENT_ARGUMENT) { }


            public FieldUpdate(string field, object value, int formatting) {
                this.Field = field;
                this.Value = value.ToString();
                this.ValueFormat = formatting;
            }

        }
        
        public abstract bool Execute();

    }

    public abstract class SQLDelete : SQLConditional<SQLDelete> {

        public string Table { get; set; }
        public Database Database { get; set; }

        public SQLDelete(string table, Database database) {
            this.Table = table;
            this.Database = database;
        }

        public abstract bool Execute();

    }

    public abstract class SQLInsert {

        public string Table { get; set; }
        public Database Database { get; set; }

        public string[] Fields;
        public List<Object[]> ValuesList;

        public SQLInsert(string table, Database database) : this(table, database, null) { }

        public SQLInsert(string table, Database database, params string[] fields) {
            this.Table = table;
            this.Database = database;
            this.Fields = fields;
            this.ValuesList = new List<Object[]>();
        }

        public SQLInsert Values(params object[] values) {
            this.ValuesList.Add(values);
            return this;
        }

        public abstract bool Execute();
        
    }
    

}
