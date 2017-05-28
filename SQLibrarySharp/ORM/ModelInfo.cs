using SQLibrary.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SQLibrary.ORM {

    public class ModelInfo {

        public Type mtype { get; }
        public Table table { get; }
        public List<ModelField> fillables { get; }

        public ModelInfo(Type type) {
            this.mtype = type;
            this.fillables = new List<ModelField>();
            this.table = type.GetCustomAttribute<Table>();

            if (table == null) {
                throw new ModelFormatException(string.Format("Missing table attribute for Model {0}", mtype.ToString()));
            }

            foreach (FieldInfo field in type.GetFields()) {
                if (field.GetCustomAttribute(typeof(Fillable)) != null) {
                    fillables.Add(new ModelField(field));
                }
            }


        }

        [Serializable]
        private class ModelFormatException : Exception {
            
            public ModelFormatException(string message) : base(message) {}
            
        }
    }

    public class Table : Attribute {
        public string Name { get; set; }

    }

    public class PrimaryKey : Attribute {
        public String key = null;

    }

    public class ForeignKey : Attribute {
        public String key = null;
        public Model references;

    }

    public class Fillable : Attribute {
        public String field;

    }

    public class Schema : Attribute {
        /**
            Allows for multiple potential schemas and database types
        */
        public int level = 1;
        public string schema;

    }




    public class ModelField {

        private PrimaryKey primaryKey;
        private ForeignKey foreignKey;
        private Fillable fillable;

        private FieldInfo fieldInfo;
        private List<Schema> schemaList;

        public ModelField(FieldInfo field) {

            this.fillable = (Fillable) field.GetCustomAttribute(typeof(Fillable));
            this.primaryKey = (PrimaryKey) field.GetCustomAttribute(typeof(PrimaryKey));
            this.foreignKey = (ForeignKey) field.GetCustomAttribute(typeof(ForeignKey));

            this.fieldInfo = field;
            this.schemaList = new List<Schema>();
            foreach (Attribute attr in field.GetCustomAttributes()) {
                if (!typeof(Schema).IsInstanceOfType(attr)) {
                    continue;
                }

                schemaList.Add((Schema)attr);
            }
        }



        public PrimaryKey GetPrimaryKey() {
            return primaryKey;
        }

        public ForeignKey GetForeignKey() {
            return foreignKey;
        }

        public Fillable GetFillable() {
            return fillable;
        }

        public List<Schema> GetSchema() {
            return schemaList;
        }


        //Gets Highest Schema to a Specified Level
        public Schema GetSchema(int level) {
            Schema schemaBuffer = null;
            foreach (Schema schema in schemaList) {
                if (schema.level > level) {
                    continue;
                }

                if (schemaBuffer == null) {
                    schemaBuffer = schema;
                    continue;
                }

                if (schemaBuffer.level < schema.level) {
                    schemaBuffer = schema;
                }
            }

            return schemaBuffer;
        }

        public String GetField() {
            return fillable.field;
        }

        public FieldInfo GetInfo() {
            return fieldInfo;
        }
        
    }

}
