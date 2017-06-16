
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary_Test {
    /**
        Potential concept for expression based conditions, Unsure of implementation of MySQL functions like "LIKE". 
        TODO: Come back to this
    */

    class SQLExpression {
        

    }


    class Expressions {
        static void Run(string[] args) {
           
            Test test = new Test();
            

            // Target Output SQL "num NOT NULL AND num < 20 AND num > 0 AND (NUM != 3 OR num > 0)"
            Expression<Func<Nullable<int>, bool>> expr = num => num > 5 && num < 20 && num > 0 && (num != 2 || num != 5) || num == 3;
            Expression<Func<Nullable<int>, bool>> expr2 = num => num > 1 && num < 1;
            
            Console.WriteLine(((BinaryExpression)((BinaryExpression) expr.Body).Right).Right);
            
            //Console.WriteLine(ExprToSQL((BinaryExpression) expr.Body));
            Console.Read();


        }

        public class Test {
            public int Num = 5;
        }

        public static string ExprToSQL(BinaryExpression expr) {
            string sql = string.Empty;

            while (expr != null) {

                string exprSQL;
                if (expr.Right.NodeType == ExpressionType.AndAlso || expr.Right.NodeType == ExpressionType.OrElse) {
                    exprSQL = String.Format("({0})", ExprToSQL((BinaryExpression)expr.Right));
                }
                else if (expr.Right is BinaryExpression) {
                    exprSQL = ExprCondToSQL((BinaryExpression) expr.Right);
                }
                else {
                    exprSQL = ExprCondToSQL(expr);
                }

                if (expr.NodeType == ExpressionType.AndAlso) {
                    sql = " AND " + exprSQL + sql;
                    expr = (BinaryExpression) expr.Left;
                }
                else if (expr.NodeType == ExpressionType.OrElse) {
                    sql = " OR " + exprSQL + sql;
                    expr = (BinaryExpression) expr.Left;
                }
                else {
                    sql = exprSQL + sql;
                    expr = null;
                }

            }


            return sql;
        }


        private static string ExprCondToSQL(BinaryExpression expr) {

            string left = NonBinaryExprToSQL(expr.Left);
            string right = NonBinaryExprToSQL(expr.Right);


            return null;

        }

        private static string NonBinaryExprToSQL(Expression expr) {
            if (expr is ParameterExpression) {
                ParameterExpression param = (ParameterExpression) expr;
                return "param";
            }
            else {
                return "const";
            }
        }

    }

}
