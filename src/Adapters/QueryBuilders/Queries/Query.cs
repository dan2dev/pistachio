using System;
using System.Collections.Generic;

namespace Pistachio {
	public static class Query {
		public static QueryFindAllBuilder<T> FindAll<T>() where T : IEntity, new() {
			return new QueryFindAllBuilder<T>();
		}
		public static QueryFindOneBuilder<T> FindOne<T>() where T : IEntity, new() {
			return new QueryFindOneBuilder<T>();
		}
		public static QueryInsertBuilder<T> Insert<T>(T entity) where T : IEntity, new() {
			return new QueryInsertBuilder<T>().Set(entity);
		}
		public static QueryDeleteBuilder<T> Delete<T>() where T : IEntity, new() {

			return new QueryDeleteBuilder<T>();
		}
		public static QueryDeleteBuilder<T> Delete<T>(this T entity) where T : IEntity, new() {

			return Delete<T>(entity.Id);
		}
		public static QueryDeleteBuilder<T> Delete<T>(long id) where T : IEntity, new() {
			
			return new QueryDeleteBuilder<T>().Where(x => x.Id == (Value)id);
		}
		public static QueryUpdateBuilder<T> Update<T>(T entity) where T : IEntity, new() {
			long id = (long)entity.Id;
			return new QueryUpdateBuilder<T>().Set(entity).Where(x=>x.Id == id);
		}
		public static QueryCountBuilder<T> Count<T>() where T : IEntity, new() {

			return new QueryCountBuilder<T>();
		}
	}
	// LIKE ----------------
	public class In {
		public string[] Value { get; set; }
		public static implicit operator In(string[] value) {
			return new In() {
				Value = value
			};
		}
		public static implicit operator In(long[] value) {
			List<string> newlist = new List<string>();
			foreach (var el in value) {
				newlist.Add(el.ToString());
			}
			return new In() {
				Value = newlist.ToArray()
			};
		}
		public static implicit operator In(List<string> value) {
			return new In() {
				Value = value.ToArray()
			};
		}
		public static implicit operator In(List<int> value) {
			List<string> newlist = new List<string>();
			foreach (var el in value) {
				newlist.Add(el.ToString());
			}
			return new In() {
				Value = newlist.ToArray()
			};
		}
		public static implicit operator In(List<long> value) {
			List<string> newlist = new List<string>();
			foreach (var el in value) {
				newlist.Add(el.ToString());
			}
			return new In() {
				Value = newlist.ToArray()
			};
		}
		public static implicit operator In(List<IEntity> entityList) {
			List<string> newlist = new List<string>();
			foreach (var el in entityList) {
				newlist.Add(el.Id.ToString());
			}
			return new In() {
				Value = newlist.ToArray()
			};
		}
		public static implicit operator List<string>(In value) {
			return new List<string>(value.Value);
		}
		public static implicit operator string(In value) {
			return null;
		}
		public static implicit operator long(In value) {
			return 0;
		}
	}

	public class Value {
		public object Obj { get; set; }
		// to Value
		public static implicit operator Value(string value) {	return new Value() { Obj = value }; }
		public static implicit operator Value(long value) { return new Value() {Obj = value};}
		public static implicit operator Value(int value) { return new Value() { Obj = value }; }
		public static implicit operator Value(DateTime value) { return new Value() { Obj = value };	}
		public static implicit operator Value(Ide value) { return new Value() { Obj = value };	}
		// to Object
		public static implicit operator string(Value value) { return (value.Obj as string); }
		public static implicit operator long(Value value) { return Convert.ToInt64(value.Obj); }
		public static implicit operator int(Value value) { return Convert.ToInt32(value.Obj); }
		public static implicit operator DateTime(Value value) { return Convert.ToDateTime(value.Obj); }
		public static implicit operator Ide(Value value) { return (value.Obj as Ide); }
	}

	public class Contains {
		public string Value { get; set; }
		public static implicit operator Contains(string value) {
			return new Contains() {
				Value = value
			};
		}
		public static implicit operator string(Contains obj) {
			return obj.Value as string;
		}
	}
	public class StartsWith {
		public string Value { get; set; }
		public static implicit operator StartsWith(string value) {
			return new StartsWith() {
				Value = value
			};
		}
		public static implicit operator string(StartsWith obj) {
			return obj.Value;
		}
	}
	public class EndsWith {
		public string Value { get; set; }
		public static implicit operator EndsWith(string value) {
			return new EndsWith() {
				Value = value
			};
		}
		public static implicit operator string(EndsWith obj) {
			return obj.Value;
		}
	}
}
