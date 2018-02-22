
namespace Pistachio {
	public abstract class QueryBuilder: IQuery {
		public QueryBuilderModel Model = new QueryBuilderModel();
		public static implicit operator QueryBuilderModel(QueryBuilder query) {
			return query.Model;
		}
		public QueryBuilder() : base() {
			
		}
	}
	public abstract class QueryBuilder<T> : QueryBuilder, IQuery<T> where T : IEntity {
		public QueryBuilder() : base() {
			
		}
	}
	public abstract class QueryBuilder<T, TQuery> : QueryBuilder<T>, IQuery<T> where T : IEntity, new()
	where TQuery : QueryBuilder<T, TQuery>, new() {
		public QueryBuilder() : base() {
			this.Model.QueryType = this.GetType();
			this.Model.EntityType = typeof(T);
		}
	}
}
