using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary.System.Mapping
{
	public class ResultMap {

		public readonly String[] Headers;
		public readonly Type[] Types;
		public readonly List<Result> Results;

		public ResultMap(String[] headers, Type[] types) {
			this.Headers = headers;
			this.Types = types;
			this.Results = new List<Result>();
		}

		public Result AddResult(params object[] args) {
			if (args.Length != Headers.Length) {
				throw new ArgumentException("Too many or Missing Arguments in Result. " 
					+ args.Length + " != " + Headers.Length + "(Required)");
			}

			Result result = new Result(this, args);
			Results.Add(result);
			return result;
		}

	}
	
		public class Result {

			private ResultMap resultMap;
			public readonly Object[] Values;

			public Result(ResultMap resultMap, Object[] values) {
				this.resultMap = resultMap;
				this.Values = values;
			}

			public T Get<T>(String name) {
				return (T) Get(name);
			}

			public T Get<T>(int i) {
				return (T) Get(i);
			}

			public Object Get(String name) {
				return Get(IndexOf(name));
			}

			public Object Get(int i) {
				return Values[i];
			}

			

			public Type GetType(String name) {
				return GetType(IndexOf(name));
			}

			public Type GetType(int i) {
				return resultMap.Types[i];
			}

			public int IndexOf(String name) {
				for (int i = 0; i < resultMap.Headers.Length; i++) {
					if (resultMap.Headers[i].ToLower().Equals(name.ToLower())) {
						return i;
					}
				}

				throw new IndexOutOfRangeException();
			}

			public ResultMap GetSource() {
				return resultMap;
			}

		}

}
