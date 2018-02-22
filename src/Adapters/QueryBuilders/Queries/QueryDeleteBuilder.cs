
namespace Pistachio {
	public interface IQueryDeleteBuilder {
		
	}
	public class QueryDeleteBuilder<T, TQuery> :
		QueryFindBuilder<T, TQuery>,
		IQueryDeleteBuilder
		where TQuery : QueryDeleteBuilder<T, TQuery>, new() where T : IEntity, new() {
		public QueryDeleteBuilder() : base() {
		}
	}
	public class QueryDeleteBuilder<T> : QueryDeleteBuilder<T, QueryDeleteBuilder<T>> where T : IEntity, new() {
		
	}
}
