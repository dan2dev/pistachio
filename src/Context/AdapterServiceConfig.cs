using System;
namespace Pistachio {
	public class AdapterServiceConfig {
		public string Id { get; set; }
		public string[] EntityNames { get; set; }
		public string AdapterServiceType { get; set; }
		public string ConnectionString { get; set; }
		public IAdapterService GetAdapterServiceInstance() {
			IAdapterService adapterServiceInstance = null;
			try {
				var PossibleAdapterServiceInstance = Activator.CreateInstance(Type.GetType(this.AdapterServiceType));
				if (PossibleAdapterServiceInstance is IAdapterService) {
					adapterServiceInstance = (IAdapterService)PossibleAdapterServiceInstance;
				} else {
					throw new System.Exception("The declared AdapterServiceType isn't an IAdapterService");
				}
			} catch (Exception ex) {
				throw ex;
				throw new System.Exception("The declared AdapterServiceType doesn't exist.");
			}
			adapterServiceInstance.ConnectionString = ConnectionString;
			if(adapterServiceInstance == null) {
				throw new System.Exception("Adapter service was not registred");
			}
			return adapterServiceInstance;
		}
	}
}
