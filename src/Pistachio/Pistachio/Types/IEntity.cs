using Newtonsoft.Json;
namespace Pistachio {
	public interface IEntity {
		[JsonIgnore]
		bool Saved { get; set; }
		long Id { get; set; }
	}
	public interface IEntity<T> : IEntity {

	}
}
