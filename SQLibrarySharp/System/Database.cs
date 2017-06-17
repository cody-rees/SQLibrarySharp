using SQLibrary.ORM;
using SQLibrary.System.Condition;
using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary.System {

    public abstract class Database {

        protected TextWriter output;
        protected Exception exception;

        public abstract bool OpenConnection();
        public abstract void CloseConnection();

        public abstract ResultMap ExecuteQuery(String query);
        public abstract Boolean ExecuteUpdate(String update);

        public ResultMap ExecuteConditionalQuery(string query, SQLConditional conditional) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            return ExecuteConditionalQuery(query, conditional, ref parameters);
        }

        public Boolean ExecuteConditionalUpdate(string query, SQLConditional conditional) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            return ExecuteConditionalUpdate(query, conditional, ref parameters);
        }

        public abstract ResultMap ExecuteConditionalQuery(string query, SQLConditional conditional, ref Dictionary<string, object> parameters);
        public abstract Boolean ExecuteConditionalUpdate(string query, SQLConditional conditional, ref Dictionary<string, object> parameters);





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

    public abstract class SQLSelect : SQLConditional {

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

    public abstract class SQLUpdate : SQLConditional {

        public const int FORMAT_ESCAPE_VALUE = 0;
        public const int FORMAT_RAW = 1;

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
                this(field, value.ToString(), SQLUpdate.FORMAT_ESCAPE_VALUE) { }


            public FieldUpdate(string field, object value, int formatting) {
                this.Field = field;
                this.Value = value.ToString();
                this.ValueFormat = formatting;
            }

        }
        
        public abstract bool Execute();

    }

    public abstract class SQLDelete : SQLConditional {

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
        }

        public SQLInsert Values(params string[] values) {
            this.ValuesList.Add(values);
            return this;
        }

        public abstract bool Execute();
        
    }
    

}
