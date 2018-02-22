using System;
using System.Linq.Expressions;

namespace Pistachio {
	public interface IQueryFindAllBuilder {
		
	}
	public class QueryFindAllBuilder<T, TQuery> :
		QueryFindBuilder<T, TQuery>,
		IQuerySortable<T, TQuery>,
		IQueryLimitable<T, TQuery>,
		IQueryFromable<T, TQuery>,
		IQueryFindAllBuilder
		where TQuery : QueryFindAllBuilder<T, TQuery>, new() where T : IEntity, new() {
		public QueryFindAllBuilder() : base() {
		}

		public TQuery From(QueryBuilder query) {
			this.Model.From = query;
			return this as TQuery;
		}

		public TQuery From(QueryBuilderModel query) {
			this.Model.From = query;
			return this as TQuery;
		}

		public TQuery Limit(int rows = -1, int skip = -1) {
			this.Model.Skip = skip;
			this.Model.Rows = rows;
			return this as TQuery;
		}

		public TQuery SortBy(Expression<Func<T, object>> field, Order order = Order.Asc) {
			this.Model.SortBy.Add(new QuerySortModel() {
				Field = field,
				Order = order
			});
			return this as TQuery;
		}

	}
	public class QueryFindAllBuilder<T> : QueryFindAllBuilder<T, QueryFindAllBuilder<T>> where T : IEntity, new() {
		
	}
	
}