using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Pistachio.Reflection {
	public static class DataReflection {
		public static TAttribute GetAttribute<TAttribute>(Type entityType) where TAttribute : Attribute {
			return entityType.GetTypeInfo().GetCustomAttribute<TAttribute>();
		}
		public static bool ImplementInterface<T>(this Type type)  {
			var interfaceType = typeof(T);
			var QueryInterfaces = new List<Type>(type.GetInterfaces());
			return QueryInterfaces.IndexOf(interfaceType) > -1;
		}
		public static EntityInfo GetEntityInfo(Type type) {
			if(type == null) {
				return null;
			} else 
			if(type.GetTypeInfo().GetCustomAttributes<CollectionAttribute>() == null) {
				return null;
			} else {
				List<CollectionAttribute> collectionAttributes = new List<CollectionAttribute>(type.GetTypeInfo().GetCustomAttributes<CollectionAttribute>());
				if (collectionAttributes.Count > 0) {
					var collectionAttribute = collectionAttributes[0];
					EntityInfo entityInfo = new EntityInfo() {
						EntityType = type,
						Attribute = collectionAttribute,
						Fields = GetEntityFieldInfoList(type),
						Joins = GetEntityJoinInfoList(type)
					};
					return entityInfo;
				} else {
					return null;
				}
			}
		}

		public static List<EntityFieldInfo> GetEntityFieldInfoList(Type type)  {
			List<EntityFieldInfo> entityFields = new List<EntityFieldInfo>();
			PropertyInfo[] props = type.GetProperties();
			foreach (PropertyInfo prop in props) {
				List<FieldAttribute> fieldAttributes = new List<FieldAttribute>(prop.GetCustomAttributes<FieldAttribute>());
				if (fieldAttributes.Count > 0) {
					var fieldAttribute = fieldAttributes[0];
					entityFields.Add(GetEntityFieldInfo(prop));
				}
			}
			return entityFields;
		}

		public static List<EntityJoinInfo> GetEntityJoinInfoList(Type type) {
			List<EntityJoinInfo> entityJoins = new List<EntityJoinInfo>();
			PropertyInfo[] props = type.GetProperties();
			foreach (PropertyInfo prop in props) {
				List<JoinAttribute> fieldAttributes = new List<JoinAttribute>(prop.GetCustomAttributes<JoinAttribute>());
				if (fieldAttributes.Count > 0) {
					var fieldAttribute = fieldAttributes[0];
					entityJoins.Add(GetEntityJoinInfo(prop));
				}
			}
			return entityJoins;
		}

		public static EntityJoinInfo GetEntityJoinInfo(PropertyInfo property){
			List<JoinAttribute> joinAttributes = new List<JoinAttribute>(property.GetCustomAttributes<JoinAttribute>());
			if (joinAttributes.Count > 0) {
				var joinAttribute = joinAttributes[0];
				return new EntityJoinInfo() {				
					EntityFieldType = property.PropertyType,
					Attribute = joinAttribute,
					Property = property,
				};
			} else {
				return null;
			}
		}


		public static EntityFieldInfo GetEntityFieldInfo(PropertyInfo property) {
			List<FieldAttribute> fieldAttributes = new List<FieldAttribute>(property.GetCustomAttributes<FieldAttribute>());
			if (fieldAttributes.Count > 0) {
				var fieldAttribute = fieldAttributes[0];
				return new EntityFieldInfo() {
					EntityFieldType = property.PropertyType,
					Attribute = fieldAttribute,
					Property = property
				};
			} else {
				return null;
			}
		}
		public static EntityMemberPath GetEntityMemberPath(MemberExpression expression) {
			List<string> pathList = new List<string>();
			MemberExpression exp = expression;
			do {
				pathList.Add(exp.Member.Name);
				exp = exp.Expression as MemberExpression;
			} while (exp != null);

			pathList.Reverse();
			var entityMemberPath = new EntityMemberPath() {
				Path = pathList
			};
			return entityMemberPath;
		}

		public static EntityMemberPath GetEntityMemberPath(EntityJoinInfo joinInfo) {
			List<string> pathList = new List<string> {
				joinInfo.Property.Name
			};
			var entityMemberPath = new EntityMemberPath() {
				Path = pathList
			};
			return entityMemberPath;
		}

		public static MemberExpression GetMemberExpression(LambdaExpression exp) {
			var member = exp.Body as MemberExpression;
			var unary = exp.Body as UnaryExpression;
			return member ?? (unary != null ? unary.Operand as MemberExpression : null);
		}
		public static object ExecuteExpression(Expression expression) {
			return Expression.Lambda(expression).Compile().DynamicInvoke(); // working
		}

		public static List<long> GetIds<T>(this List<T> entityList) where T : IEntity {
			List<long> idsList = new List<long>();
			foreach (var item in entityList) {
				idsList.Add(item.Id);
			}
			return idsList;
		}

		// --------------
		public static string CreateMD5(string input) {
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++) {
					sb.Append(hashBytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
		}
	}
}
