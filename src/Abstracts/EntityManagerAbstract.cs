using System;
using System.Collections.Generic;

namespace Pistachio {
	public interface IEntityManagerAbstract {

	}
	public class EntityManagerAbstract<T> : IEntityManagerAbstract where T : IEntity, new() {

		public static IAdapterService ContextAdapterService {
			get {
				return DataContext.GetAdapterService<T>();
			}
		}
		public static List<long> GetIdsFrom(List<T> entityList) {
			List<long> idsList = new List<long>();
			foreach (var item in entityList) {
				idsList.Add(item.Id);
			}
			return idsList;
		}

		public static int Count(IAdapterService adapterService = null) {
			return Count(query => query, adapterService);
		}

		public static int Count(Func<QueryCountBuilder<T>, QueryCountBuilder<T>> query, IAdapterService adapterService = null) {
			QueryCountBuilder<T> newQuery = new QueryCountBuilder<T>();
			var returnQuery = query(newQuery);
			return (adapterService ?? ContextAdapterService).Count(returnQuery);
		}
		public static int Count(QueryCountBuilder<T> query, IAdapterService adapterService = null) {
			return (adapterService ?? ContextAdapterService).Count(query);
		}

		public static bool Delete(T entity, IAdapterService adapterService = null) {
			//throw new NotImplementedException(); //TODO: need to be fixed. deletando tabela inteira
			long id = Convert.ToInt64(entity.Id);
			var query = Query.Delete<T>().Where(x => x.Id == (Value)id);
			return (adapterService ?? ContextAdapterService).Delete(query);
		}
		public static bool Delete(Func<QueryDeleteBuilder<T>, QueryDeleteBuilder<T>> query, IAdapterService adapterService = null) {
			QueryDeleteBuilder<T> newQuery = new QueryDeleteBuilder<T>();
			var returnQuery = query(newQuery);
			return (adapterService ?? ContextAdapterService).Delete<T>(returnQuery);
		}
		public static bool Delete(long id, IAdapterService adapterService = null) {
			var query = Query.Delete<T>().Where(x => x.Id == id);
			return (adapterService ?? ContextAdapterService).Delete(query);
		}
		public static bool Delete(List<long> ids, IAdapterService adapterService = null) {
			var query = Query.Delete<T>().Where(x => x.Id == (In)ids);
			return (adapterService ?? ContextAdapterService).Delete(query);
		}

		public static List<T> FindAll(IAdapterService adapterService = null) {
			return FindAll(query => query, adapterService);
		}
		public static List<T> FindAll(Func<QueryFindAllBuilder<T>, QueryFindAllBuilder<T>> query, IAdapterService adapterService = null) {
			QueryFindAllBuilder<T> newQuery = new QueryFindAllBuilder<T>();
			var returnQuery = query(newQuery);
			return (adapterService ?? ContextAdapterService).FindAll<T>(returnQuery);
		}
		public static List<T> FindAll(QueryFindAllBuilder<T> query, IAdapterService adapterService = null) {
			return (adapterService ?? ContextAdapterService).FindAll<T>(query);
		}

		//public static QueryFindAllBuilder<T> FindAll(IAdapterService adapterService = null) {
		//	QueryFindAllBuilder<T> newQuery = new QueryFindAllBuilder<T>();
		//	//var returnQuery = query(newQuery);
		//	return newQuery;
		//}

		public static T FindOne(IAdapterService adapterService = null) {
			return FindOne(query => query, adapterService);
		}
		public static T FindOne(long id, IAdapterService adapterService = null) {
			QueryFindOneBuilder<T> newQuery = new QueryFindOneBuilder<T>();
			var returnQuery = newQuery.Where(query => query.Id == id);
			return (adapterService ?? ContextAdapterService).FindOne<T>(returnQuery);
		}
		public static T FindOne(Func<QueryFindOneBuilder<T>, QueryFindOneBuilder<T>> query, IAdapterService adapterService = null) {
			QueryFindOneBuilder<T> newQuery = new QueryFindOneBuilder<T>();
			var returnQuery = query(newQuery);
			var adapter = (adapterService ?? ContextAdapterService);

			if (adapter == null)
				throw new InvalidOperationException("Please add/configure a adapter service.");

			return adapter.FindOne<T>(returnQuery);
		}
		public static T FindOne(QueryFindOneBuilder<T> query, IAdapterService adapterService = null) {
			return (adapterService ?? ContextAdapterService).FindOne<T>(query);
		}
		public static T FindOneOrCreate(QueryFindOneBuilder<T> query, T newEntity = default(T), IAdapterService adapterService = null) {
			T entity = FindOne(query, adapterService);
			if (entity == null) {
				if (EqualityComparer<T>.Default.Equals(newEntity, default(T))) {
					entity = new T();
				} else {
					entity = newEntity;
				}
				Insert(ref entity, adapterService);
			}
			return entity;
		}
		public static T FindOneOrCreate(long id, T newEntity = default(T), IAdapterService adapterService = null) {
			T entity = FindOne(id, adapterService);
			if (entity == null) {
				if (EqualityComparer<T>.Default.Equals(newEntity, default(T))) {
					entity = new T();
				} else {
					entity = newEntity;
				}
				entity.Id = id;
				Insert(ref entity, adapterService);
			}
			return entity;
		}
		public static T FindOneOrCreate(T newEntity, IAdapterService adapterService = null) {
			if (EqualityComparer<T>.Default.Equals(newEntity, default(T))) {
				throw new Exception("NewEntity cannot be null");
			} else {
				if (newEntity.Id <= 0) {
					throw new Exception("No Id was declared");
				}
			}
			return FindOneOrCreate(newEntity.Id, newEntity);
		}
		public static long Insert(T entity, IAdapterService adapterService = null) {
			var query = Query.Insert<T>(entity);
			return (adapterService ?? ContextAdapterService).Insert(query);
		}
		public static long Insert(ref T entity, IAdapterService adapterService = null) {
			var id = Insert(entity, adapterService);
			entity.Id = id;
			return id;
		}
		public static long Update(T entity, IAdapterService adapterService = null) {
			long id = Convert.ToInt64((long)entity.Id);
			var query = Query.Update<T>(entity).Where(x => x.Id == id);
			return (adapterService ?? ContextAdapterService).Update(query);
		}



		public static long Save(T entity, IAdapterService adapterService = null) {
			if (entity.Id <= 0) {
				return Insert(entity, adapterService);
			} else {
				return Update(entity, adapterService);
			}
		}

		public static long Save(ref T entity, IAdapterService adapterService = null) {
			if (entity.Id <= 0 || entity.Saved == false) {
				return Insert(ref entity, adapterService);
			} else {
				return Update(entity, adapterService);
			}
		}

		public static List<long> Save(List<T> entities, IAdapterService adapterServer = null) {
			List<long> returnIds = new List<long>();
			foreach (T entity in entities) {
				long returnId = Save(entity);
				returnIds.Add(returnId);
			}
			return returnIds;
		}

		// custom queries ------------------------
		public static List<TList> Raw<TList>(string sql, IAdapterService adapterService = null) {


			return new List<TList>();
		}
	}
}
