using SQLibrary.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary_Test {

    class Program {
        static void Main(string[] args) {

        }
    }

    [Table(Name = "users")]
    class User {

        [PrimaryKey(key = "pk_user")]
        [Fillable(field = "user_id")]
        [Schema(schema = "INT AUTO INCREMENT NOT NULL")]
        public Nullable<int> userID;




    }

}
