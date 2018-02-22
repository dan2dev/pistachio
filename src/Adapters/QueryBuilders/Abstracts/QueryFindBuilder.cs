using System;
using System.Linq.Expressions;

namespace Pistachio {
	public abstract class QueryFindBuilder<T, TQuery> :
		QueryBuilder<T, TQuery>,
		IQueryWhereable<T, TQuery>,
		IQueryJoinable<T, TQuery>
		where TQuery : QueryFindBuilder<T, TQuery>, new() where T : IEntity, new() {
		public QueryFindBuilder() : base() {
		}
		public TQuery Join(Expression<Func<T, Object>> property) {
			this.Model.Join.Add(property);
			return this as TQuery;
		}
		public TQuery Join() {
			var type = typeof(T);
			var joinInfoList = Reflection.DataReflection.GetEntityJoinInfoList(type);
			foreach (var joinInfo in joinInfoList) {
				this.Model.JoinAll.Add(joinInfo);
			}
			return this as TQuery;
		}
		public TQuery Where(Expression<Func<T, bool>> whereSteatment) {
			this.Model.Where.Add(whereSteatment);
			return this as TQuery;
		}
	}
}
