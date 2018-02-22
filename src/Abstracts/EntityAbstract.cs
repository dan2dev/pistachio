using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Pistachio {
	public static class EntityExtension {
		public static long Save<T>(T entity, IAdapterService adapterService = null) where T : IEntity, new() {
			var contextAdapterService = (adapterService ?? DataContext.GetAdapterService<T>());
			if (entity.Id <= 0 || entity.Saved == false) {
				QueryInsertBuilder<T> query = Query.Insert<T>(entity);
				return contextAdapterService.Insert(query);
			} else {
				var id = Convert.ToInt64((long)entity.Id);
				QueryUpdateBuilder<T> query = Query.Update<T>(entity).Where(x => x.Id == id);
				var updateLines = contextAdapterService.Update(query);
				if(updateLines > 0) {
					return id;
				} else {
					return -1;
				}
			}
		}
		public static bool Delete<T>(T entity, IAdapterService adapterService = null) where T : IEntity, new() {
			//throw new NotImplementedException(); // TODO: testar delete
			///// ----------------------
			var contextAdapterService = (adapterService ?? DataContext.GetAdapterService<T>());
			QueryDeleteBuilder<T> query = Query.Delete<T>(entity);
			return contextAdapterService.Delete(query);
		}
		public static T Load<T>(long id, IAdapterService adapterService = null) where T : IEntity, new() {
			var contextAdapterService = (adapterService ?? DataContext.GetAdapterService<T>());
			QueryFindOneBuilder<T> query = Query.FindOne<T>().Where(x => x.Id == id);
			return contextAdapterService.FindOne<T>(query);
		}
	}
	public abstract class EntityAbstract : IEntity {
		public long Id { get; set; }
		[JsonIgnore]
		public bool Saved { get; set; }
	}
	public abstract class EntityAbstract<T> : EntityAbstract, IEntity<T> where T : IEntity<T>, new() {
		//public abstract class EntityAbstract<T> : EntityAbstract, IEntity, IEntity<T> where T : IEntity<T>, new() {
		//[Field("identificador", "varchar(32)", "Identificador")]
		//public string Ide { get; set; }
		//public override long Id { get; set; }
		//public string StringId {
		//	get {
		//		return this.Id.ToString();
		//	}
		//	set {
		//		try {
		//			Id = Convert.ToInt64(value);
		//		} catch (Exception) {
		//			Id = 0;
		//		}
		//	}
		//}

		//public static implicit operator EntityAbstract<T>(long Id) {
		//	T obj = new T() {
		//		Id = Id
		//	};
		//	return obj as EntityAbstract<T>;
		//}
		interface IAbtract {

		}
		public long Save(IAdapterService adapterService = null) {
			//throw new NotImplementedException();
			///// ----------------------

			var newEntity = (T)this.MemberwiseClone();
			var SavedId = EntityExtension.Save<T>(newEntity, adapterService);
			this.Id = SavedId;
			this.Saved = true;
			return SavedId;
		}
		public bool Delete(IAdapterService adapterService = null) {
			//throw new NotImplementedException();
			///// ----------------------
			return EntityExtension.Delete<T>((T)this.MemberwiseClone(), adapterService);
		}
		public void Load(IAdapterService adapterService = null) {
			this.Load(this.Id, adapterService);
			//T entity = EntityExtension.Load<T>(this.Id, adapterService);
		}
		public void Load(long id, IAdapterService adapterService = null) {
			var adaptServ = DataContext.GetAdapterService<T>(adapterService);
			T entity = adaptServ.FindOne<T>(Query.FindOne<T>().Where(q => q.Id == id));
			//if (id > -1) {
			//	entity = adapterService.FindOne<T>(id);
			//} else {
			//	entity = EntityExtension.Load<T>(this.Id, adapterService);
			//}
			this.Saved = entity.Saved;

			if (entity != null) {
				Type type = typeof(T);
				var properties = type.GetProperties();
				foreach (var prop in properties) {
					object value = prop.GetValue(entity);
					prop.SetValue(this, value);
				}
			}
		}
		// -------------------------------------------


	}
}
