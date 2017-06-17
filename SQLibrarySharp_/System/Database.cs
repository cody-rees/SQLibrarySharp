using SQLibrary.ORM;
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
        
        
        public static Database primaryDB { get; set; }

        public Database() {
            if (primaryDB == null) {
                primaryDB = this;
            }
        }

        public void SetOutput(TextWriter output) {
            this.output = output;
        }

        public Exception GetLastException() {
            return exception;
        }


    }
    
}
