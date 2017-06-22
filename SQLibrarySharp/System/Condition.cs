using SQLibrary.System;
using System.Collections.Generic;
using System.Linq;

namespace SQLibrary.System  {


    /**
        SQLibrary Condition Builder, Responsible for mapping conditions.
            Also used to build SubsetConditions example "field = value OR (SQLConditional)"

    */
    public class ConditionBuilder : SQLConditional<ConditionBuilder> {

    }

    public class SQLConditional<T> where T : SQLConditional<T> {

        public List<Condition> Conditions { get; }

        public SQLConditional() {
            if (Conditions == null) {
                Conditions = new List<Condition>();
            }
        }
     

        /**
            Creates a hardcoded (AND) SQL condition
            @warning Any user input not validated or escaped using this method 
                is liable to SQL Injection and will NOT be automatically validated
            
        */
        public T Where(string sql) {
            Conditions.Add(Condition.Where(sql));
            return (T) this;
        }
        

        /**
            Creates a SQL Parameter based (AND) equal(=) Condition

        */
        public T Where(object param1, object param2) {
            Conditions.Add(Condition.Where(param1, param2));
            return (T) this;
        }

        /**
            Creates a SQL Parameter based (AND) condition with specified operator

        */
        public T Where(object param1, string opera, object param2) {
            Conditions.Add(Condition.Where(param1, opera, param2));
            return (T) this;
        }


        /**
            Creates a SQL Parameter based (AND) condition with a specified operator and formatting
            @see Condition for formatting options
            
            @default formattingOptions corrosponding to each parameter

                int[] formattingOptions = [
                    Condition.FORMAT_PARAMETER_FIELD,
                    Condition.FORMAT_PARAMETER_VALUE
                ];
        */
        public T Where(string param1, string opera, string param2, int[] formatting) {
            Conditions.Add(Condition.Where(param1, opera, param2, formatting));
            return (T) this;
        }

        /**
            Add a SQL (AND) Condition to the current conditions queue

        */
        public T Where(Condition condition) {
            Conditions.Add(condition);
            return (T) this;
        }


        /**
            Creates a enclosed (AND) condition, Accepts SQLConditional (ConditionBuilder)
            @example condition.Where( 
                new SQLConditional()
                    .Where("user_id", "!=", "5")
                    .OrWhere("admin", true) 
                
            );
        */
        public T Where(ConditionBuilder conditions) {
            Where(conditions.Conditions.ToArray<Condition>());
            return (T) this;
        }


        /**
            Creates a enclosed (AND) condition, Accepts SQLConditional (ConditionBuilder)
            @example condition.Where( 
                new SQLConditional()
                    .Where("user_id", "!=", "5")
                    .OrWhere("admin", true) 
                
            );
        */
        public T Where(Condition[] conditions) {
            Conditions.Add(Condition.Where(conditions));
            return (T) this;
        }
   



        /*
            -------------------------------------
            OR CONDITIONS
            ------------------------------------
        */




        /**
            Creates a hardcoded (OR) SQL condition
            @warning Any user input not validated or escaped using this method 
                is liable to SQL Injection and will NOT be automatically validated
            
        */
        public T OrWhere(string sql) {
            Condition condition = Condition.Where(sql);
            condition.Relation = Relation.OR;
            Conditions.Add(condition);

            return (T) this;
        }


        /**
            Creates a SQL Parameter based (OR) equal(=) Condition

        */
        public T OrWhere(object param1, object param2) {
            Condition condition = Condition.Where(param1, param2);
            condition.Relation = Relation.OR;
            Conditions.Add(condition);

            return (T) this;
        }

        /**
            Creates a SQL Parameter based (OR) condition with specified operator

        */
        public T OrWhere(object param1, string opera, object param2) {
            Condition condition = Condition.Where(param1, opera, param2);
            condition.Relation = Relation.OR;
            Conditions.Add(condition);

            return (T) this;
        }


        /**
            Creates a SQL Parameter based (OR) condition with a specified operator and formatting
            @see Condition for formatting options
            
            @default formattingOptions corrosponding to each parameter

                int[] formattingOptions = [
                    Condition.FORMAT_PARAMETER_FIELD,
                    Condition.FORMAT_PARAMETER_VALUE
                ];

        */
        public T OrWhere(string param1, string opera, string param2, int[] formatting) {
            Condition condition = Condition.Where(param1, opera, param2, formatting);
            condition.Relation = Relation.OR;
            Conditions.Add(condition);

            return (T) this;
        }

        /**
            Add a SQL (OR) Condition to the current conditions queue
            
        */
        public T OrWhere(Condition condition) {
            condition.Relation = Relation.OR;
            Conditions.Add(condition);
            return (T) this;
        }


        /**
            Creates a enclosed (OR) condition, Accepts SQLConditional (ConditionBuilder)
            @example condition.Where( 
                new SQLConditional()
                    .Where("user_id", "!=", "5")
                    .OrWhere("admin", true) 
                 
            );
        */
        public T OrWhere(ConditionBuilder conditions) {
            Where(conditions.Conditions.ToArray<Condition>());
            return (T) this;
        }


        /**
            Creates a enclosed (OR) condition, Accepts SQLConditional (ConditionBuilder)
            @example condition.Where( 
                new SQLConditional()
                    .Where("user_id", "!=", "5")
                    .OrWhere("admin", true) 
                 
            );
        */
        public T OrWhere(Condition[] conditions) {
            Condition condition = Condition.Where(conditions);
            condition.Relation = Relation.OR;
            Conditions.Add(condition);

            return (T) this;
        }




    }

    public class Condition {


        public Relation Relation { get; set; } = Relation.AND;

        /**
            Creates a Static SQL Condition

        */
        public static Condition Where(string sql) {
            return new StaticCondition(sql);
        }
        

        /**
            Parameter Based Condition
        */
        public static Condition Where(object param1, object param2) {
            return Where(param1, "=", param2);
        }
       

        /**
            Parameter Based Condition
        */
        public static Condition Where(object param1, string opera, object param2) {
            return new ParameterCondition(param1.ToString(), opera, param2.ToString());
        }
        
        /**
            Parameter Based Condition
            @see Condition for more formatting options and descriptions

            @default formattingOptions corrosponding to each parameter

                int[] formattingOptions = [
                    Condition.FORMAT_PARAMETER_FIELD,
                    Condition.FORMAT_PARAMETER_VALUE
                ];

        */

        public static Condition Where(object param1, string opera, object param2, int[] formattingOptions) {
            ParameterCondition condition = new ParameterCondition(param1.ToString(), opera, param2.ToString());

            if (formattingOptions.Length > 0) {
                condition.Param1Formatting = formattingOptions[0];
            }

            if (formattingOptions.Length > 1) {
                condition.Param2Formatting = formattingOptions[1];
            }

            return condition;
        }


        public static Condition Where(params Condition[] conditions) {
            return new SubsetCondition(conditions);
        }

    }

    public enum Relation {
        AND, OR
    }

    public class ParameterCondition : Condition {

        public string Param1 { get; }
        public string Operator { get; }
        public string Param2 { get; }

        /**
            Specify Formatting Algorythm for Formatting Preference for Parameter
            @default Condition.ESCAPE_PARAMETER_AS_FIELD
        */
        public int Param1Formatting { get; set; } = ParameterFormat.ESCAPE_PARAMETER_AS_FIELD;

        /**
            Specify Formatting Algorythm for Formatting Preference for Parameter
            @default Condition.ESCAPE_PARAMETER_AS_VALUE
        */
        public int Param2Formatting { get; set; } = ParameterFormat.PREPARED_STATEMENT_ARGUMENT;
        

        public ParameterCondition(string param1, string opera, string param2) {
            this.Param1 = param1;
            this.Operator = opera;
            this.Param2 = param2;
        }

    }

    /**
        Static SQL Condition

    */
    public class StaticCondition : Condition {

        public string SQL { get; }

        public StaticCondition(string sql) {
            this.SQL = sql;
        }

    }

    /**
        Wrapper class for a List of Conditions that is itself a Condition

    */
    public class SubsetCondition : Condition {

        public List<Condition> Conditions { get; }

        public SubsetCondition() : this(new Condition[0]) { }

        public SubsetCondition(params Condition[] conditions) {
            this.Conditions = new List<Condition>();
            this.Conditions.AddRange(conditions);
        }

    }

}
