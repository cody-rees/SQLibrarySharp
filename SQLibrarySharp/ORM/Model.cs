using SQLibrary.ORM;
using SQLibrary.System;
using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary.ORM {

    public abstract class Model {

        private static HashSet<ModelInfo> infoList;
        
        public static T Find<T>(int id) where T : Model {
            ModelField primary = GetInfo<T>().PrimaryField;
            if (primary == null) {
                throw new ModelFormatException("Model does not contain a PrimaryKey Attribute");
            }

            List<T> models = Select<T>().Where(primary.GetFillable().Field, id).Get();
            if (models.Count < 1) {
                return null;
            }

            return models[0];
        }

        public static ModelSelect<T> Select<T>(params string[] fields) where T : Model {
            ModelInfo info = GetInfo<T>();
            return new ModelSelect<T>(Database.PrimaryDB.Select(info.Table.Name, fields));
        }
        
        public bool Save() {
            ModelInfo info = GetInfo(this.GetType());
        
            string[] fields = info.Fillables.Select<ModelField, String>(x => x.GetField()).ToArray();
            object[] values = info.Fillables.Select<ModelField, Object>(x => x.GetValue(this)).ToArray();

            if (info.PrimaryField != null) {

                object primaryVal = info.PrimaryField.GetInfo().GetValue(this);
                if (primaryVal != null) {
                    SQLUpdate update = Database.PrimaryDB.Update(info.Table.Name)
                        .Where(info.PrimaryField.GetField(), primaryVal);

                    for (int i = 0; i < fields.Count(); i++) {
                        update.Set(fields[i], values[i]);
                    }

                    return update.Execute();
                }

            }
            
            bool result  = Database.PrimaryDB.Insert(info.Table.Name, fields)
                .Values(values)
                .Execute();

            if (!result || info.PrimaryField == null) {
                return result;
            }

            ResultMap results = Database.PrimaryDB.ExecuteQuery("SELECT LAST_INSERT_ID() as id;");
            if (results == null) {
                return false;
            }

            info.PrimaryField.SetValue(this, results.Results[0].Get("id"));
            return true;
        }


        public bool Delete() {

            ModelInfo info = GetInfo(this.GetType());
            if (info.PrimaryField == null) {
                throw new ModelFormatException("Could not delete, Model does not contain a Primary Key.");
            }
            
            object primaryVal = info.PrimaryField.GetInfo().GetValue(this);
            if (primaryVal == null) {
                throw new InvalidOperationException("Could not delete, Model Primary Key is null");
            }

            return Database.PrimaryDB.Delete(info.Table.Name)
                .Where(info.PrimaryField.GetField(), primaryVal)
                .Execute();
        }
   



        public static bool BuildSchema<T>() where T : Model {
            return BuildSchema<T>(1, Database.PrimaryDB);
        }

        public static bool BuildSchema<T>(int level, Database db) where T : Model {
            ModelInfo info = GetInfo<T>();
            return false; // db.BuildSchema(info.table.table, info.fillables);
        }
        



        public static List<T> raw<T>(string query) where T : Model {
            return raw<T>(query, Database.PrimaryDB);
        }

        public static List<T> raw<T>(string query, Database db) where T : Model {
            ResultMap results = db.ExecuteQuery(query);
            if (results == null) {
                return null;
            }

            return Construct<T>(results);
        }




        public static List<T> Construct<T>(ResultMap results) where T : Model {
            List<T> models = new List<T>();
            foreach (Result result in results.Results) {
                models.Add(Construct<T>(result));
            }

            return models;
        }

        public static T Construct<T>(Result result) where T : Model {
            ModelInfo info = GetInfo<T>();
            T model = NewInstance<T>();
            
            foreach(ModelField field in info.Fillables) {
                var value = result.Get(field.GetField());
                field.SetValue(model, value);
            }

            return model;
        }

        private static T NewInstance<T>() where T : Model {
            return (T) Activator.CreateInstance(typeof(T), new object[] {});
        }
        



        public static ModelInfo GetInfo<T>() where T : Model {
            return GetInfo(typeof(T));
        }

        public static ModelInfo GetInfo(Type type) {
            if (infoList == null) {
                infoList = new HashSet<ModelInfo>();
            }

            ModelInfo info = infoList.FirstOrDefault(f => f.MType == type);
            if (info == null) {
                info = new ModelInfo(type);
                infoList.Add(info);
            }

            return info;
        }
        
    }


    //Wrapper Class for Initializing Models from SQLSelect
    public class ModelSelect<T> : SQLConditional<ModelSelect<T>> where T : Model {

        public Dictionary<string, object> parameters = new Dictionary<string, object>();

        private SQLSelect selectInstance;
        private ModelInfo info;

        public ModelSelect(SQLSelect select) {
            this.selectInstance = select;
            this.info = Model.GetInfo<T>();
        }
        
        public List<T> Get() {
            selectInstance.Conditions.AddRange(base.Conditions);
            ResultMap results = selectInstance.Execute(parameters);
            if (results == null) {
                return null;
            }

            return Model.Construct<T>(results);
        }
    }
    
    
       
}
