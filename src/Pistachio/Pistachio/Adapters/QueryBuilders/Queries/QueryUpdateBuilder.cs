using System;
using System.Linq.Expressions;

namespace Pistachio {
	public interface IQueryUpdateBuilder {
		
	}
	public class QueryUpdateBuilder<T, TQuery> :
		QueryBuilder<T, TQuery>,
		IQuerySetable<T, TQuery>,
		IQueryWhereable<T, TQuery>,
		IQueryUpdateBuilder
		where TQuery : QueryUpdateBuilder<T, TQuery>, new() where T : IEntity, new() {

		public QueryUpdateBuilder() : base() {
		}
		public TQuery Set(T entity) {
			this.Model.Entity = entity;
			return this as TQuery;
		}
		public TQuery Where(Expression<Func<T, bool>> whereSteatment) {
			this.Model.Where.Add(whereSteatment);
			return this as TQuery;
		}
	}
	public class QueryUpdateBuilder<T> : QueryUpdateBuilder<T, QueryUpdateBuilder<T>> where T : IEntity, new() {
		
	}

}
