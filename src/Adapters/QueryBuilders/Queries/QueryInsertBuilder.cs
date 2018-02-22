namespace Pistachio {
	public interface IQueryInsertBuilder {

	}
	public class QueryInsertBuilder<T, TQuery> :
		QueryBuilder<T, TQuery>,
		IQuerySetable<T, TQuery>,
		IQueryInsertBuilder
		where TQuery : QueryInsertBuilder<T, TQuery>, new() where T : IEntity, new() {
		public QueryInsertBuilder() : base() {
		}
		public TQuery Set(T entity) {
			this.Model.Entity = entity;
			return this as TQuery;
		}
	}
	public class QueryInsertBuilder<T> : QueryInsertBuilder<T, QueryInsertBuilder<T>> where T : IEntity, new() {

	}
}
