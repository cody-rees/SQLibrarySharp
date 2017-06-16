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

        public abstract ResultMap ExecuteConditionalQuery(string query, SQLConditional conditional);
        public abstract Boolean ExecuteConditionalUpdate(string query, SQLConditional conditional);



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
    
}
