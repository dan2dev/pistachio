namespace Pistachio {
	public interface IQueryFindOneBuilder {

	}
	public class QueryFindOneBuilder<T, TQuery> :
		QueryFindBuilder<T, TQuery>,
		IQueryFindOneBuilder
		where TQuery : QueryFindOneBuilder<T, TQuery>, new() where T : IEntity, new() {
		public QueryFindOneBuilder() : base() {
			
		}
	}
	public class QueryFindOneBuilder<T> : QueryFindOneBuilder<T, QueryFindOneBuilder<T>> where T : IEntity, new() {
		
	}

}
