using System.Collections.Generic;
namespace Pistachio {
	public static partial class Helper {
		public static bool Exist(this IEntity entity) {
			if (entity != null) {
				if (entity.Id > 0) {
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}
		public static List<long> GetIds<T>(this List<T> entities) where T : IEntity<T> {
			List<long> ids = new List<long>();
			foreach (var entity in entities) {
				ids.Add(entity.Id);
			}
			return ids;
		}
	}
}
