using System.Collections.Generic;

namespace Pistachio {
	public interface IAdapterService {
		string ConnectionString { get; set; }
		List<T> FindAll<T>(QueryFindAllBuilder<T> query) where T : IEntity, new();
		T FindOne<T>(QueryFindOneBuilder<T> query) where T : IEntity, new(); // implement query object
		int Count<T>(QueryCountBuilder<T> query) where T : IEntity, new();
		long Insert<T>(QueryInsertBuilder<T> query) where T : IEntity, new();
		long Update<T>(QueryUpdateBuilder<T> query) where T : IEntity, new();
		bool Delete<T>(QueryDeleteBuilder<T> query) where T : IEntity, new();
		// Raw SQL -----
		List<IDictionary<string, object>> FindAll(string sql, object parameters = null);
		List<T> FindAll<T>(string sql, object parameters = null) where T: new();
		IDictionary<string, object> FindOne(string sql, object parameters = null);
		T FindOne<T>(string sql, object parameters = null) where T : new();
	}
}

