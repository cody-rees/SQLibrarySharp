using SQLibrary.System;
using SQLibrary.System.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary.ORM {

    public abstract class Model {

        private static HashSet<ModelInfo> info;
        
        public static T Find<T>(int id) where T : Model {
            List<T> models = Select<T>();
            if (models.Count < 1) {
                return null;
            }

            return models[0];
        }


        public static List<T> Select<T>() where T : Model { //Condition condition
            
            return null;
        }

        public static bool Save(Model model) {
            return false;
        }

        public static bool Delete(Model model) {
            return false;
        }
        


        public static bool BuildSchema<T>() where T : Model {
            return BuildSchema<T>(1, Database.primaryDB);
        }

        public static bool BuildSchema<T>(int level, Database db) where T : Model {
            ModelInfo info = GetInfo<T>();
            return false; // db.BuildSchema(info.table.table, info.fillables);
        }
        


        public static List<T> raw<T>(string query) where T : Model {
            return raw<T>(query, Database.primaryDB);
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
            
            foreach(ModelField field in info.fillables) {
                var value = result.Get(field.GetField());
                field.GetInfo().SetValue(model, value);
            }

            return model;
        }

        private static T NewInstance<T>() where T : Model {
            return (T) Activator.CreateInstance(typeof(T), new object[] {});
        }
        

        public static ModelInfo GetInfo<T>() where T : Model {
            if (_info == null) {
                _info = new HashSet<ModelInfo>();
            }

            ModelInfo info = _info.FirstOrDefault(f => f.mtype == typeof(T));
            if (info == null) { 
                info = new ModelInfo(typeof(T));
                _info.Add(info);
            }

            return info;
        }
    }
    
}
