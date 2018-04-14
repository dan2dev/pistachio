using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pistachio {
	public static class DataContext {
		private static IDictionary<string, int> ContextAdapterServiceIndex = new Dictionary<string, int>();
		private static List<IAdapterService> ContextAdapterServiceList = new List<IAdapterService>();

		public static void RegisterAdapterService(AdapterServiceConfig adapterServiceConfig) {
			var adapterServiceInstance = adapterServiceConfig.GetAdapterServiceInstance();
			var entitiesAndNamespaces = adapterServiceConfig.EntityNames;
			var id = adapterServiceConfig.Id;
			// maps index keys and set default if none
			List<string> keys = new List<string>();
			if (id != null) {
				keys.Add(id);
			}
			if (entitiesAndNamespaces != null) {
				foreach (var entityNameSpace in entitiesAndNamespaces) {
					keys.Add(entityNameSpace.ToLowerInvariant());
				}
			}
			if (keys.Count == 0) {
				keys.Add("default");
			}
			// -------
			ContextAdapterServiceList.Add(adapterServiceInstance);
			var index = ContextAdapterServiceList.Count - 1;
			// set int in key
			foreach (var key in keys) {
				if (ContextAdapterServiceIndex.ContainsKey(key)) {
					ContextAdapterServiceIndex[key] = index;
				} else {
					ContextAdapterServiceIndex.Add(key, index);
				}
			}
		}

		public static void RegisterAdapterService(List<AdapterServiceConfig> adapterServiceConfigList) {
			foreach (var adapterServiceConfig in adapterServiceConfigList) {
				RegisterAdapterService(adapterServiceConfig);
			}
		}

		// GetAdapterService --------------------------------
		private static IAdapterService GetAdapterServiceByIndex(string key) {
			key = key.ToLowerInvariant();
			if (ContextAdapterServiceIndex.ContainsKey(key)) {
				int index = ContextAdapterServiceIndex[key];
				return ContextAdapterServiceList[index];
			} else {
				return null;
			}
		}
		public static IAdapterService GetAdapterService<T>(IAdapterService adapterService = null) {
			if (adapterService != null) {
				return adapterService;
			}
			Type type = typeof(T);
			var typeName = (type.GetTypeInfo().Name).ToLowerInvariant();
			var fullName = (type.GetTypeInfo().FullName).ToLowerInvariant();
			var pathNameArray = new List<string>(fullName.Split('.'));
			pathNameArray.RemoveAt(pathNameArray.Count - 1);
			var typeNamespace = string.Join(".", pathNameArray);
			var r = GetAdapterService(typeNamespace, typeName);
			return r;
		}
		public static IAdapterService GetAdapterService(string typeNamespaceOrKey = null, string typeName = null) {
			//string index;
			IAdapterService adapter = null;
			if (typeNamespaceOrKey != null) {
				if (typeName != null) {
					adapter = GetAdapterServiceByIndex($"{typeNamespaceOrKey}.{typeName}");
					if (adapter != null) {
						return adapter;
					}
				}
				adapter = GetAdapterServiceByIndex($"{typeNamespaceOrKey}.*");
				if (adapter != null) {
					return adapter;
				}
				adapter = GetAdapterServiceByIndex(typeNamespaceOrKey);
				if (adapter != null) {
					return adapter;
				}
			}
			return GetDefaultAdapterService();
		}

		public static IAdapterService GetDefaultAdapterService() {
			return GetAdapterServiceByIndex("default");
		}
	}
}
