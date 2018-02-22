using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Pistachio.Reflection;

namespace Pistachio.MySql {

	public class MySqlQueryBuilderAdapter {
		public QueryBuilderModel Model { get; set; }
		public string SqlQuery { get; set; }
		public List<KeyValuePair<string, object>> ParameterStack = new List<KeyValuePair<string, object>>();
		public int LevelStack { get; set; }
		public MySqlQueryBuilderAdapter(QueryBuilderModel queryBuilderModel, int currentLevelStack = 0) {
			this.Model = queryBuilderModel;
			this.LevelStack = currentLevelStack + 1;
		}
		public void Construct() {

			if (Model.QueryType.ImplementInterface<IQueryFindOneBuilder>()) {
				FindOneConverter();
			} else
			if (Model.QueryType.ImplementInterface<IQueryFindAllBuilder>()) {
				FindAllConverter();
			} else
			if (Model.QueryType.ImplementInterface<IQueryDeleteBuilder>()) {
				DeleteConverter();
			} else
			if (Model.QueryType.ImplementInterface<IQueryUpdateBuilder>()) {
				UpdateConverter();
			} else
			if (Model.QueryType.ImplementInterface<IQueryInsertBuilder>()) {
				InsertConverter();
			} else
			if (Model.QueryType.ImplementInterface<IQueryCountBuilder>()) {
				CountConverter();
			}
		}
		private void CountConverter() {
			EntityInfo entityInfo = DataReflection.GetEntityInfo(Model.EntityType);
			if (entityInfo == null) return;
			List<KeyValuePair<string, string>> selectList = null;
			StringBuilder strBody = new StringBuilder();
			StringBuilder strCount = new StringBuilder();
			//-------------------------------------------
			CountBuilder(ref strCount);
			// ------------------------------------------
			FromBuilder(ref strBody, entityInfo);
			// Join ------------------------------------
			JoinBuilder(ref selectList, ref strBody);
			// where builder ----------------------------
			WhereBuilder(ref strBody);
			// set SqlQuery
			this.SqlQuery = $" ( {strCount.ToString()} {strBody.ToString()} )";
		}
		private void InsertConverter() {
			EntityInfo entityInfo = DataReflection.GetEntityInfo(Model.EntityType);
			StringBuilder str = new StringBuilder();
			var insertList = GetEntityKeyValueList();
			if(Model.Entity.Id > 0) {
				insertList.Add(new KeyValuePair<string, object>(entityInfo.Attribute.FieldPrimaryKey, Model.Entity.Id));
			}


			str.Append($" INSERT INTO `{entityInfo.Attribute.ColletionName}` ( ");
			for (int i = 0; i < insertList.Count; i++) {
				if (i > 0) str.Append(" , ");
				str.Append(insertList[i].Key);
			}
			str.Append(" ) VALUES ( ");
			for (int i = 0; i < insertList.Count; i++) {
				if (i > 0) str.Append(" , ");
				object value = insertList[i].Value;
				// get parameter -------------------------
				var key = this.GetStackParameterKey(insertList[i].Value);
				str.Append($" ?{key} ");

			}
			str.Append(" ) ");

			this.SqlQuery = str.ToString();
		}
		private void UpdateConverter() {
			EntityInfo entityInfo = DataReflection.GetEntityInfo(Model.EntityType);
			StringBuilder str = new StringBuilder();
			var updateList = GetEntityKeyValueList();
			str.Append($" UPDATE `{entityInfo.Attribute.ColletionName}` o SET ");
			List<string> updateSets = new List<string>();
			for (int i = 0; i < updateList.Count; i++) {
				//if (i > 0) str.Append(" , ");
				var key = updateList[i].Key;
				var value = updateList[i].Value;
				var keyParameter = GetStackParameterKey(value);
				// --------------
				if (key != "identificador" ) {
					updateSets.Add($"{updateList[i].Key} = ?{keyParameter} ");
				}
			}
			str.Append(string.Join(" , ", updateSets));
			WhereBuilder(ref str);
			this.SqlQuery = str.ToString();
		}
		private void DeleteConverter() {
			EntityInfo entityInfo = DataReflection.GetEntityInfo(Model.EntityType);
			StringBuilder str = new StringBuilder();

			str.Append($" DELETE FROM `{entityInfo.Attribute.ColletionName}` ");
			// WHERE
			// where builder ----------------------------
			WhereBuilder(ref str);
			this.SqlQuery = str.ToString().Replace("`o`.", "`" + entityInfo.Attribute.ColletionName + "`.");
		}
		private void FindOneConverter() {
			Model.Rows = 1;
			Model.Skip = 0;
			FindAllConverter();
		}
		private void FindAllConverter() {
			EntityInfo entityInfo = DataReflection.GetEntityInfo(Model.EntityType);
			if (entityInfo == null) return;
			List<KeyValuePair<string, string>> selectList = new List<KeyValuePair<string, string>>();
			List<KeyValuePair<string, string>> orderList = new List<KeyValuePair<string, string>>();

			StringBuilder strBody = new StringBuilder();
			StringBuilder strSelect = new StringBuilder();
			// ------------------------------------------
			FromBuilder(ref strBody, entityInfo);
			// Join ------------------------------------
			JoinBuilder(ref selectList, ref strBody);
			// where builder ----------------------------
			WhereBuilder(ref strBody);
			// order -------------
			OrderBuilder(ref orderList, ref strBody, entityInfo);
			// Limit builder -------------------------------
			LimitBuilder(ref strBody);
			// select ---------------------------------------
			SelectBuilder(ref selectList, ref strSelect, entityInfo);
			this.SqlQuery = $" ( {strSelect.ToString()} {strBody.ToString()} )";
		}
		/// Helpers -------------------------------------------------------------
		private List<KeyValuePair<string, object>> GetEntityKeyValueList() {
			EntityInfo entityInfo = DataReflection.GetEntityInfo(Model.EntityType);
			StringBuilder str = new StringBuilder();
			var entity = Model.Entity;
			List<KeyValuePair<string, object>> keyValueList = new List<KeyValuePair<string, object>>();

			var fieldInfoList = entityInfo.Fields;
			var joinInfoList = DataReflection.GetEntityJoinInfoList(entityInfo.EntityType);
			foreach (var fieldInfo in fieldInfoList) {
				var value = fieldInfo.Property.GetValue(entity);
				if (value != null) {
					keyValueList.Add(new KeyValuePair<string, object>(fieldInfo.Attribute.FieldName, value));
				} else {
					keyValueList.Add(new KeyValuePair<string, object>(fieldInfo.Attribute.FieldName, null));
				}
			}
			foreach (var joinInfo in joinInfoList) {
				if (joinInfo.Property.GetValue(entity) is IEntity value) {
					if(value != null && value?.Id > 0) {
						keyValueList.Add(new KeyValuePair<string, object>(joinInfo.Attribute.LeftFieldName, value.Id.ToString()));
					}
				}
			}

			return keyValueList;
		}
		private void LimitBuilder(ref StringBuilder str) {
			if (Model.Rows > -1) {
				if (Model.Skip > -1) {
					str.Append($" LIMIT {Model.Skip},{Model.Rows}   ");
				} else {
					str.Append($" LIMIT {Model.Rows} ");
				}
			}
		}
		private void CountBuilder(ref StringBuilder str) {
			str.Append(" SELECT COUNT(*) ");
		}
		private void SelectBuilder(ref List<KeyValuePair<string, string>> selectList, ref StringBuilder str, EntityInfo entityInfo) {
			SelectListBuilder(ref selectList, entityInfo);
			str.Append(" select ");
			for (int i = 0; i < selectList.Count; i++) {
				if (i > 0)
					str.Append(", ");
				str.Append($" {selectList[i].Key}.{selectList[i].Value} as '{selectList[i].Key}.{selectList[i].Value}' ");
			}
		}
		private void OrderBuilder(ref List<KeyValuePair<string, string>> orderList, ref StringBuilder str, EntityInfo entityInfo) {
			OrderListBuilder(ref orderList, entityInfo);
			if (orderList.Count > 0) {
				str.Append(" order by ");
			}
			for (int i = 0; i < orderList.Count; i++) {
				if (i > 0)
					str.Append(", ");
				str.Append($" {orderList[i].Key} {orderList[i].Value} ");
			}
		}


		private void FromBuilder(ref StringBuilder str, EntityInfo entityInfo) {
			str.Append(" from ");
			if (Model.From != null) {
				var fromAdapter = new MySqlQueryBuilderAdapter(Model.From, this.LevelStack);
				fromAdapter.Construct();
				// get all values from the paramterStack ---------------------------
				foreach (var parameter in fromAdapter.ParameterStack) {
					ParameterStack.Add(parameter);
				}
				// ----------------------------------------
				str.Append($" { fromAdapter.SqlQuery } as o ");
			} else {
				str.Append($" `{entityInfo.Attribute.ColletionName}` o");
			}
		}
		private void JoinBuilder(ref List<KeyValuePair<string, string>> selectList, ref StringBuilder str) {
			List<string> joineds = new List<string>();
			if (Model.JoinAll.Count > 0) {
				foreach (var joinInfo in Model.JoinAll) {
					EntityInfo leftEntityInfo = DataReflection.GetEntityInfo(joinInfo.Property.DeclaringType);
					EntityInfo rightEntityInfo = DataReflection.GetEntityInfo(joinInfo.EntityFieldType);
					var entityMemberPath = DataReflection.GetEntityMemberPath(joinInfo);
					string fullPath = entityMemberPath.GetFullPath();
					string path = entityMemberPath.GetPath();

					str.Append($" left join `{rightEntityInfo.Attribute.ColletionName}` {fullPath} on `{path}`.`{joinInfo.Attribute.LeftFieldName}` = {fullPath}.`{joinInfo.Attribute.RightFieldName}` ");

					joineds.Add(fullPath);
					// --------------------------
					if (selectList != null) {
						SelectListBuilder(ref selectList, rightEntityInfo, fullPath);
					}
				}
			}

			if (Model.Join.Count > 0) {
				foreach (var join in Model.Join) {
					MemberExpression joinMemberExp = DataReflection.GetMemberExpression(join);
					PropertyInfo property = (PropertyInfo)joinMemberExp.Member;
					EntityJoinInfo joinInfo = DataReflection.GetEntityJoinInfo(property);
					// ----------------------------
					EntityInfo leftEntityInfo = DataReflection.GetEntityInfo(property.DeclaringType);
					EntityInfo rightEntityInfo = DataReflection.GetEntityInfo(joinInfo.EntityFieldType);
					// ----------------------------
					var entityMemberPath = DataReflection.GetEntityMemberPath(joinMemberExp);
					string fullPath = entityMemberPath.GetFullPath();
					string path = entityMemberPath.GetPath();
					if (!joineds.Contains(fullPath)) {
						// ----------------------------

						str.Append($" left join `{rightEntityInfo.Attribute.ColletionName}` {fullPath} on `{path}`.`{joinInfo.Attribute.LeftFieldName}` = {fullPath}.`{joinInfo.Attribute.RightFieldName}` ");
						// --------------------------
						if (selectList != null) {
							SelectListBuilder(ref selectList, rightEntityInfo, fullPath);
						}
					}
				}
			}
		}

		private void WhereBuilder(ref StringBuilder str) {

			if (Model.Where.Count > 0) {
				str.Append($" where ( ");
				for (int i = 0; i < Model.Where.Count; i++) {
					var where = Model.Where[i];
					if(i > 0) {
						str.Append(" ) AND ( ");
					}
					var whereString = GetWhere(where);
					str.Append($" {whereString} ");
				}
				str.Append($" ) ");
			}
		}
		private string GetWhere(Expression expression) {
			if (expression == null)
				throw new Exception("The GetWhere(expression) method received null as parameter");
			var nodeType = expression.NodeType;
			var type = expression.Type;

			if (nodeType == ExpressionType.Lambda)
				return $" ( {GetWhere((expression as LambdaExpression).Body)} )";

			if (expression as MemberExpression != null) {
				var exp = expression as MemberExpression;
				if (!(exp.Expression is ConstantExpression)) {
					return $" {GetWhereField(exp)} ";
				}
			}
			if (expression as BinaryExpression != null || nodeType == ExpressionType.Equal) {
				var exp = expression as BinaryExpression;
				var left = GetWhere(exp.Left);
				var right = GetWhere(exp.Right);
				return $" ( {left} {GetOperator(exp)} {right} ) ";
			}

			return GetWhereValue(expression);
		}
		private string GetOperator(BinaryExpression expression) {
			var nt = expression.NodeType;
			string op = null;
			var rightExpression = expression.Right;
			if (rightExpression is UnaryExpression) {
				var method = (rightExpression as UnaryExpression).Method;
				var dt = method.DeclaringType;
				// detect like ------------------
				if (dt == typeof(Contains) || dt == typeof(StartsWith) || dt == typeof(EndsWith)) {
					if (nt == ExpressionType.Equal) {
						op = " LIKE ";
					} else {
						op = " NOT LIKE ";
					}
					return op;
				}
				// detect in
				if (dt == typeof(In)) {
					if (nt == ExpressionType.Equal) {
						op = " IN ";
					}
					if (nt == ExpressionType.NotEqual) {
						op = " NOT IN ";
					}
					return op;
				}
			}
			if (nt == ExpressionType.And || nt == ExpressionType.AndAlso) {
				return " AND ";
			} else if (nt == ExpressionType.Or || nt == ExpressionType.OrElse) {
				return " OR ";
			} else if (nt == ExpressionType.Equal) {
				return " = ";
			} else if (nt == ExpressionType.GreaterThan) {
				return " > ";
			} else if (nt == ExpressionType.GreaterThanOrEqual) {
				return " >= ";
			} else if (nt == ExpressionType.LessThan) {
				return " < ";
			} else if (nt == ExpressionType.LessThanOrEqual) {
				return " <= ";
			} else if (nt == ExpressionType.NotEqual) {
				return " != ";
			}
			return op;
		}
		private string GetWhereValue(Expression expression) {
			string sqlValue = null;
			// ---------------
			if (expression is UnaryExpression) {
				// -----------------
				var exp = (expression as UnaryExpression);
				var method = exp.Method;
				if (exp != null) {
					if (method == null) {
						object value = DataReflection.ExecuteExpression(expression);
						var key = this.GetStackParameterKey(value);
						sqlValue = $" ?{key} ";
					} else {
						var dt = method.DeclaringType;
						if (dt == typeof(Contains)) {
							object value = DataReflection.ExecuteExpression(expression);
							var key = this.GetStackParameterKey($"%{value}%");
							sqlValue = $" ?{key} ";
						} else if (dt == typeof(StartsWith)) {
							object value = DataReflection.ExecuteExpression(expression);
							var key = this.GetStackParameterKey($"{value}%");
							sqlValue = $" ?{key} ";
						} else if (dt == typeof(EndsWith)) {
							object value = DataReflection.ExecuteExpression(expression);
							var key = this.GetStackParameterKey($"%{value}");
							sqlValue = $" ?{key} ";
						} else if (dt == typeof(In)) {
							// array of values here -----------
							List<string> listArray = new List<string>();
							var inExpression = Expression.Convert(exp.Operand, typeof(In));
							var inLambdaExpression = Expression.Lambda<Func<object>>(inExpression);
							var inLambdaExpressionCompiled = inLambdaExpression.Compile();
							var inObj = (inLambdaExpressionCompiled.DynamicInvoke()) as In;
							if (inObj != null) {
								List<string> InValues = inObj;
								StringBuilder strInBuilder = new StringBuilder();
								strInBuilder.Append(" ( ");
								if (InValues.Count == 0) {
									// Atenção Danilo do futuro. Vc tem que corrigir isso aqui.
									// Em busca de arrays ele deve cancelar o where
									strInBuilder.Append(" '-598724806.349209005' ");
								} else {
									for (int i = 0; i < InValues.Count; i++) {
										var v = InValues[i];
										if (i > 0) {
											strInBuilder.Append($", ?{this.GetStackParameterKey(v)} ");
										} else {
											strInBuilder.Append($" ?{this.GetStackParameterKey(v)} ");
										}
									}
								}
								strInBuilder.Append(" ) ");
								sqlValue = strInBuilder.ToString();
							}
						}
					}
				}
			}
			if (sqlValue == null) {
				object value = DataReflection.ExecuteExpression(expression);
				var key = this.GetStackParameterKey(value);
				sqlValue = $" ?{key} ";
			}
			// add to stack ----------------------
			return sqlValue;
		}
		private string GetStackParameterKey(object value) {
			var key = $"p_{this.LevelStack}_{this.ParameterStack.Count}";
			this.ParameterStack.Add(new KeyValuePair<string, object>(key, value));
			return key;
		}
		private string GetWhereField(MemberExpression expression) {
			EntityMemberPath entityMemberPath = DataReflection.GetEntityMemberPath(expression);

			//object property3;
			if (expression.Member is FieldInfo) {
				var fieldInfo = (FieldInfo)(expression.Member);
				return GetWhere(expression.Expression);
			} else {
				var property = (PropertyInfo)(expression.Member);

				var field = DataReflection.GetEntityFieldInfo(property);
				if (field != null) {
					return $" `{entityMemberPath.GetPath()}`.`{field.Attribute.FieldName}` ";
				} else {
					bool isMemeberExpression = expression.Expression is MemberExpression;
					bool isParameterExpression = expression.Expression is ParameterExpression;
					bool isUnaryExpression = expression.Expression is UnaryExpression;

					if (expression.Expression is MemberExpression) {
						var memberExpression = expression.Expression as MemberExpression;
						return GetWhereField(memberExpression);
					} else
					if (expression.Expression is ParameterExpression || expression.Expression is UnaryExpression) {
						if (property.Name == "Id") {
							var parameterExpression = expression.Expression as ParameterExpression;
							if(parameterExpression != null) {
								Type entityType = parameterExpression.Type;
								var entityInfo = DataReflection.GetEntityInfo(entityType);
								return $" `{entityMemberPath.GetPath()}`.{entityInfo.Attribute.FieldPrimaryKey} ";
							} else {
								var entityType = Model.EntityType;
								var entityInfo = DataReflection.GetEntityInfo(entityType);

								var path = entityMemberPath.GetPath();


								return $" `{entityMemberPath.GetPath()}`.{entityInfo.Attribute.FieldPrimaryKey} ";
							}
						} else {
							var joinEntityInfo = DataReflection.GetEntityInfo(property.PropertyType);
							var propertyInfo = DataReflection.GetEntityJoinInfo(property);
							return $"  `{entityMemberPath.GetPath()}`.`{propertyInfo.Attribute.LeftFieldName}` ";
						}
					} else
					if (expression.Expression is UnaryExpression) {
						return GetWhereValue(expression.Expression as UnaryExpression);
					} else {
						return "  ";
					}
				}
			}
		}
		private string GetOrderField(MemberExpression expression) {
			EntityMemberPath entityMemberPath = DataReflection.GetEntityMemberPath(expression);

			//object property3;
			if (expression.Member is FieldInfo) {
				var fieldInfo = (FieldInfo)(expression.Member);
				return GetWhere(expression.Expression);
			} else {
				var property = (PropertyInfo)(expression.Member);

				var field = DataReflection.GetEntityFieldInfo(property);
				if (field != null) {
					return $" `{entityMemberPath.GetPath()}`.`{field.Attribute.FieldName}` ";
				} else {
					bool isMemeberExpression = expression.Expression is MemberExpression;
					bool isParameterExpression = expression.Expression is ParameterExpression;
					bool isUnaryExpression = expression.Expression is UnaryExpression;

					if (expression.Expression is MemberExpression) {
						var memberExpression = expression.Expression as MemberExpression;
						return GetWhereField(memberExpression);
					} else
					if (expression.Expression is ParameterExpression) {
						if (property.Name == "Id") {
							var parameterExpression = expression.Expression as ParameterExpression;
							Type entityType = parameterExpression.Type;
							var entityInfo = DataReflection.GetEntityInfo(entityType);
							return $" `{entityMemberPath.GetPath()}`.{entityInfo.Attribute.FieldPrimaryKey} ";
						} else {
							var joinEntityInfo = DataReflection.GetEntityInfo(property.PropertyType);
							return $"  `{entityMemberPath.GetPath()}`.{joinEntityInfo.Attribute.FieldPrimaryKey} ";
						}
					} else
					if (expression.Expression is UnaryExpression) {
						return GetWhereValue(expression.Expression as UnaryExpression);
					} else {
						return "  ";
					}
				}
			}
		}

		private void SelectListBuilder(ref List<KeyValuePair<string, string>> selectList, EntityInfo entityInfo, string path = "o") {
			selectList.Add(new KeyValuePair<string, string>(path, entityInfo.Attribute.FieldPrimaryKey));
			foreach (var field in entityInfo.Fields) {
				selectList.Add(new KeyValuePair<string, string>(path, field.Attribute.FieldName));
			}
			foreach (var field in entityInfo.Joins) {
				selectList.Add(new KeyValuePair<string, string>(path, field.Attribute.LeftFieldName));
			}
		}
		private void OrderListBuilder(ref List<KeyValuePair<string, string>> orderList, EntityInfo entityInfo, string path = "o") {
			for (int i = 0; i < Model.SortBy.Count; i++) {
				QuerySortModel sort = Model.SortBy[i];
				MemberExpression member = DataReflection.GetMemberExpression(sort.Field);
				// ----------------
				var field = GetOrderField(member); 
				string fieldOrder = "ASC";

				if (sort.Order == Order.Desc) {
					fieldOrder = "DESC";
				}

				orderList.Add(new KeyValuePair<string, string>(field, fieldOrder));
			}
		}
	}
}
