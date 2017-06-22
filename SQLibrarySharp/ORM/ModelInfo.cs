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

        public Type MType { get; }
        public Table Table { get; }

        public ModelField PrimaryField { get; }
        public List<ModelField> Fillables { get; }

        public ModelInfo(Type _type) {
            this.MType = _type;
            this.Fillables = new List<ModelField>();
            this.Table = _type.GetCustomAttribute<Table>();

            if (Table == null) {
                throw new ModelFormatException(string.Format("Missing table attribute for Model {0}", MType.ToString()));
            }

            foreach (FieldInfo field in _type.GetFields()) {

                if (field.GetCustomAttribute(typeof(Fillable)) != null) {
                    ModelField modelField = new ModelField(field);
                    if (PrimaryField == null && modelField.GetPrimaryKey() != null) {
                        this.PrimaryField = modelField;
                    }

                    Fillables.Add(modelField);
                }
            }


        }

    }

    [Serializable]
    public class ModelFormatException : Exception {

        public ModelFormatException(string message) : base(message) { }

    }

    public class Table : Attribute {
        public string Name { get; set; }

    }

    public class PrimaryKey : Attribute {
        public String Key = null;

    }

    public class ForeignKey : Attribute {
        public String Key = null;
        public Model References;

    }

    public class Fillable : Attribute {
        public String Field;

    }

    public class Schema : Attribute {
        /**
            Allows for multiple potential schemas and database types
        */
        public int Level = 1;
        public string SQL;

    }




    public class ModelField {

        private PrimaryKey PrimaryKey;
        private ForeignKey ForeignKey;
        private Fillable Fillable;

        private FieldInfo FieldInfo;
        private List<Schema> SchemaList;
        public Type FieldType { get; }

        public ModelField(FieldInfo field) {

            this.Fillable = (Fillable) field.GetCustomAttribute(typeof(Fillable));
            this.PrimaryKey = (PrimaryKey) field.GetCustomAttribute(typeof(PrimaryKey));
            this.ForeignKey = (ForeignKey) field.GetCustomAttribute(typeof(ForeignKey));
            
            this.FieldInfo = field;
            this.SchemaList = new List<Schema>();
            this.FieldType = field.FieldType;

            foreach (Attribute attr in field.GetCustomAttributes()) {
                if (!typeof(Schema).IsInstanceOfType(attr)) {
                    continue;
                }

                SchemaList.Add((Schema)attr);
            }
        }
        


        public PrimaryKey GetPrimaryKey() {
            return PrimaryKey;
        }

        public ForeignKey GetForeignKey() {
            return ForeignKey;
        }

        public Fillable GetFillable() {
            return Fillable;
        }

        public List<Schema> GetSchema() {
            return SchemaList;
        }



        public void SetValue(Model instance, object value) {
            //If type is nullable
            Type nullableType = Nullable.GetUnderlyingType(FieldInfo.FieldType);
            if (nullableType != null) {
                //Convert to UnderlyingType
                this.FieldInfo.SetValue(instance, Convert.ChangeType(value, nullableType));
                return;
            }

            this.FieldInfo.SetValue(instance, Convert.ChangeType(value, FieldInfo.FieldType));
        }

        public object GetValue(Model instance) {
            return FieldInfo.GetValue(instance);
        }


        //Gets Highest Schema to a Specified Level
        public Schema GetSchema(int level) {
            Schema schemaBuffer = null;
            foreach (Schema schema in SchemaList) {
                if (schema.Level > level) {
                    continue;
                }

                if (schemaBuffer == null) {
                    schemaBuffer = schema;
                    continue;
                }

                if (schemaBuffer.Level < schema.Level) {
                    schemaBuffer = schema;
                }
            }

            return schemaBuffer;
        }

        public String GetField() {
            return Fillable.Field;
        }

        public FieldInfo GetInfo() {
            return FieldInfo;
        }
        
    }

}
