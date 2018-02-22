
namespace Pistachio {
	public interface IQueryCountBuilder {
	}
	public class QueryCountBuilder<T, TQuery> : 
		QueryFindBuilder<T, TQuery>,
		IQueryCountable<T, TQuery>,
		IQueryCountBuilder
		where TQuery : QueryCountBuilder<T, TQuery>, new() where T : IEntity, new() {
		public QueryCountBuilder() : base() {
		}
	}
	public class QueryCountBuilder<T> : QueryCountBuilder<T, QueryCountBuilder<T>> where T : IEntity, new() {
		public QueryCountBuilder() : base() {
			
		}
	}
}
