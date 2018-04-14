using System;
using System.Reflection;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Pistachio.Reflection;
// using Tonolucro.Library.Utilities;
using Pistachio.MySql;
using Pistachio.Helpers;

namespace Pistachio {
	public class MySqlAdapterService : IAdapterService {
		private string connetionString;
		public string ConnectionString {
			get {
				return this.connetionString;
			}
			set {
				this.connetionString = value;
			}
		}
		private MySqlConnection Connection {
			get {
				return new MySqlConnection(ConnectionString);
			}
		}
		private void ExecuteReader(QueryBuilderModel query, Action<MySqlDataReader> callback) {
			var builderAdapter = new MySqlQueryBuilderAdapter(query);
			ExecuteReader(builderAdapter, callback);
		}
		private void ExecuteReader(MySqlQueryBuilderAdapter builderAdapter, Action<MySqlDataReader> callback) {
			var conn = Connection;
			builderAdapter.Construct();
			var sqlQuery = builderAdapter.SqlQuery;
			using (var cmd = new MySqlCommand(sqlQuery, conn)) {
				foreach (var parameter in builderAdapter.ParameterStack) {
					cmd.Parameters.AddWithValue(parameter.Key, ValueFormatToSql(parameter.Value));
				}
				// execute
				try {
					conn.Open();
					MySqlDataReader reader = cmd.ExecuteReader();
					while (reader.Read()) {
						callback(reader);
					}
				} catch (Exception e) {
					throw e;
				} finally {
					conn.Close();
				}
			}
		}
		private void ExecuteReader(string sql, Action<MySqlDataReader> callback) {
			ExecuteReader(sql, null, callback);
		}
		private void ExecuteReader(string sql, object parameters, Action<MySqlDataReader> callback) {

			// ------------------
			var conn = Connection;
			using (var cmd = new MySqlCommand(sql, conn)) {
				if (parameters != null) {
					var properties = parameters.GetType().GetProperties();
					foreach (var property in properties) {
						var value = ValueFormatToSql(property.GetValue(parameters));
						cmd.Parameters.AddWithValue($"@{property.Name}", value);
					}
				}
				try {
					conn.Open();
					MySqlDataReader reader = cmd.ExecuteReader();
					while (reader.Read()) {
						callback(reader);
					}
				} catch (Exception e) {
					throw e;
				} finally {
					conn.Close();
				}
			}
		}
		private List<IDictionary<string, object>> ExecuteReader(string sql, object parameters = null) {
			var table = new List<IDictionary<string, object>>();
			ExecuteReader(sql, parameters, o => {
				table.Add(GetRow(o));
			});
			return table;
		}
		private List<T> ExecuteReader<T>(string sql, object parameters = null) where T : new() {
			var table = ExecuteReader(sql, parameters);
			List<T> returnList = new List<T>();
			Type type = typeof(T);
			// ---------------------------
			foreach (var row in table) {
				T rowObj = new T();
				if (typeof(T) is IEntity) {
					(rowObj as IEntity).Saved = true;
				}
				foreach (var key in row.Keys) {
					var value = row[key];
					var pascalKey = key.ToPascalCase();
					var camelKey = key.ToCamelCase();
					var parameter = type.GetProperty(pascalKey) ?? type.GetProperty(camelKey);
					if (parameter != null) {
						try {
							parameter.SetValue(rowObj, value);
						} catch (Exception ex) {
							throw ex;
						}
					}
				}
				returnList.Add(rowObj);
			}
			return returnList;
		}
		private long ExecuteNonQuery(QueryBuilderModel query, bool returnLastInsertedId = false) {
			var builderAdapter = new MySqlQueryBuilderAdapter(query);

			return ExecuteNonQuery(builderAdapter, returnLastInsertedId);
		}
		private long ExecuteNonQuery(MySqlQueryBuilderAdapter builderAdapter, bool returnLastInsertedId = false) {
			long value = -1;
			var conn = Connection;
			builderAdapter.Construct();
			using (var cmd = new MySqlCommand(builderAdapter.SqlQuery, conn)) {
				//	addParameter
				foreach (var parameter in builderAdapter.ParameterStack) {
					cmd.Parameters.AddWithValue(parameter.Key, ValueFormatToSql(parameter.Value));
				}
				// execute
				try {
					conn.Open();
					value = cmd.ExecuteNonQuery();
					if (returnLastInsertedId) {
						value = cmd.LastInsertedId;
						if (value == 0) {
							value = builderAdapter.Model.Entity.Id;
						}
					}
				} catch (Exception e) {
					throw e;
				} finally {
					conn.Close();
				}
			}
			return value;
		}
		private object ExecuteScalar(QueryBuilderModel query) {
			var builderAdapter = new MySqlQueryBuilderAdapter(query);
			return ExecuteScalar(builderAdapter);
		}
		private object ExecuteScalar(MySqlQueryBuilderAdapter builderAdapter) {
			object value = -1;
			var conn = Connection;
			builderAdapter.Construct();
			using (var cmd = new MySqlCommand(builderAdapter.SqlQuery, conn)) {
				//	addParameter
				foreach (var parameter in builderAdapter.ParameterStack) {
					cmd.Parameters.AddWithValue(parameter.Key, ValueFormatToSql(parameter.Value));
				}
				// execute
				try {
					conn.Open();
					value = cmd.ExecuteScalar();
				} catch (Exception e) {
					throw e;
				} finally {
					conn.Close();
				}
			}
			return value;
		}

		private object ValueFormatToSql(object value) {
			object safeValue;
			if (value is Ide) {
				safeValue = ((Ide)value).Hash.ToString();
			} else {
				safeValue = value;
			}
			return safeValue;
		}
		private object ValueFormatFromSql(string field, object value) {
			object formated = null;
			if (field.EndsWith(".identificador") || field.ToLowerInvariant().Equals("identificador")) {
				Ide ide = new Ide();
				if (value != null) {
					ide.Hash = value.ToString();
				}
				return ide;
			} else {
				formated = value;
			}
			return formated;
		}
		private IDictionary<string, object> GetRow(MySqlDataReader reader) {
			IDictionary<string, object> row = new Dictionary<string, object>();
			int fieldCount = reader.FieldCount;
			for (int i = 0; i < fieldCount; i++) {
				var fieldName = reader.GetName(i);
				Type fieldType = reader.GetFieldType(i);
				if (fieldType == typeof(DateTime)) {
					DateTime? fieldValue = null;
					try {
						var fieldValueDateTime = (reader.IsDBNull(i) ? null : reader.GetValue(i));
						fieldValue = fieldValueDateTime as DateTime?;
						if (fieldValue == null) {
							fieldValue = DateTime.MinValue;
						}
					} catch (Exception ex) {
					}
					if (fieldName != null) {
						if (!row.ContainsKey(fieldName)) {
							row.Add(fieldName, fieldValue);
						}
					}
				} else {
					var fieldValue = (reader.IsDBNull(i) ? null : reader.GetValue(i));
					if (fieldName != null) {
						if (!row.ContainsKey(fieldName)) {
							row.Add(fieldName, ValueFormatFromSql(fieldName, fieldValue));
						}
					}
				}
			}
			return row;
		}

		private List<T> QueryToList<T>(QueryBuilderModel query) where T : IEntity, new() {
			List<IEntity> entityList = new List<IEntity>();
			// Reader Adapter ------------------------------------
			IEntity ReaderAdapter(Type type, IDictionary<string, object> row, string parent = null) {
				var entityInfo = DataReflection.GetEntityInfo(type);
				//object newEntity = Activator.CreateInstance(type);
				IEntity newEntity = (IEntity)Activator.CreateInstance(type);
				object id;
				row.TryGetValue($"{(parent == null ? "o" : parent)}.{entityInfo.Attribute.FieldPrimaryKey}", out id);
				newEntity.Id = Convert.ToInt64(id);
				// fields -----------------------------------
				foreach (var field in entityInfo.Fields) {
					string key = $"{(parent == null ? "o" : parent)}.{field.Attribute.FieldName}";
					object valueNative;
					row.TryGetValue(key, out valueNative);
					object valueFormated = this.ValueFormatFromSql(key, valueNative);
					try {
						field.Property.SetValue(newEntity, valueFormated);
					} catch (Exception ex1) {
						try {
							field.Property.SetValue(newEntity, valueFormated.ToString());
						} catch (Exception ex) {
							throw ex;
						}
					}
				}
				// emptyJoins -----------------------------------
				foreach (var join in entityInfo.Joins) {
					string key = $"{(parent == null ? "o" : parent)}.{join.Attribute.LeftFieldName}";
					// value is a entity
					object joinValue = (IEntity)Activator.CreateInstance(join.EntityFieldType);
					object joinValueId = 0;
					row.TryGetValue(key, out joinValueId);
					(joinValue as IEntity).Id = Convert.ToInt64(joinValueId);
					join.Property.SetValue(newEntity, joinValue);
				}
				// joins -----------------------------------
				foreach (var join in entityInfo.Joins) {
					string fieldKey = $"{(parent == null ? "" : $"{parent}__")}{join.Property.Name}";
					bool joinIncluded = false;
					foreach (var c in row) {
						if (c.Key.StartsWith($"{fieldKey}.")) {
							joinIncluded = true;
							break;
						}
					}
					if (joinIncluded) {
						IEntity joinValue = ReaderAdapter(join.EntityFieldType, row, fieldKey);
						join.Property.SetValue(newEntity, joinValue);
					}
				}
				return newEntity;
			}
			// Execute Reader -------------------------------------
			ExecuteReader(query, o => {
				var row = GetRow(o);
				entityList.Add(ReaderAdapter(typeof(T), row));
			});
			List<T> returnList = new List<T>();
			foreach (var item in entityList) {
				item.Saved = true;
				returnList.Add((T)item);
			}
			return returnList;
		}

		// ------------------------------------------------
		public int Count<T>(QueryCountBuilder<T> query) where T : IEntity, new() {
			object scalarReturn = ExecuteScalar(query);
			int count = 0;
			int.TryParse(scalarReturn?.ToString(), out count);
			return count;
		}

		public bool Delete<T>(QueryDeleteBuilder<T> query) where T : IEntity, new() {
			var returnValue = ExecuteNonQuery(query);
			if (returnValue >= 0) {
				return true;
			} else {
				return false;
			}
		}

		public List<T> FindAll<T>(QueryFindAllBuilder<T> query) where T : IEntity, new() {
			var listReturn = QueryToList<T>(query) as List<T>;
			return listReturn;
		}
		public PaginatedList<T> FindAllPaginated<T>(QueryFindAllBuilder<T> query) where T : IEntity, new() {
			var listReturn = QueryToList<T>(query) as List<T>;

			var countQuery = new QueryCountBuilder<T>();
			countQuery.Model.Join = query.Model.Join;
			countQuery.Model.JoinAll = query.Model.JoinAll;
			countQuery.Model.Where = query.Model.Where;
			var count = Count<T>(countQuery);
			return new PaginatedList<T>() {
				Items = listReturn,
				Page = query.Model.Page,
				RowsByPage = query.Model.RowsByPage,
				Skip = query.Model.Skip,
				Count = count,
			};
		}
		public T FindOne<T>(QueryFindOneBuilder<T> query) where T : IEntity, new() {
			var listReturn = QueryToList<T>(query);
			T entity = default(T);
			if (listReturn.Count > 0) {
				entity = listReturn[0];
			}
			return entity;
		}
		public long Insert<T>(QueryInsertBuilder<T> query) where T : IEntity, new() {
			return ExecuteNonQuery(query, true);
		}
		public long Update<T>(QueryUpdateBuilder<T> query) where T : IEntity, new() {
			return ExecuteNonQuery(query);
		}
		public List<IDictionary<string, object>> FindAll(string sql, object parameters = null) {
			return ExecuteReader(sql, parameters);
		}
		public List<T> FindAll<T>(string sql, object parameters = null) where T : new() {
			return ExecuteReader<T>(sql, parameters);
		}
		public IDictionary<string, object> FindOne(string sql, object parameters = null) {
			var list = FindAll(sql, parameters);
			if (list.Count > 0) {
				return list[0];
			} else {
				return null;
			}
		}
		public T FindOne<T>(string sql, object parameters = null) where T : new() {
			var list = FindAll<T>(sql, parameters);
			if (list.Count > 0) {

				return list[0];
			} else {
				return default(T);
			}
		}
	}
}
