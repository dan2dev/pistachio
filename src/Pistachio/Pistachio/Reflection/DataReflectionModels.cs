using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Pistachio.Reflection {
	public class EntityInfo {
		public Type EntityType { get; set; }
		public CollectionAttribute Attribute { get; set; }
		public List<EntityFieldInfo> Fields { get; set; }
		public List<EntityJoinInfo> Joins { get; set; }
	}
	public interface IEntityField {
		Type EntityFieldType { get; set; }
		PropertyInfo Property { get; set; }
	}
	public class EntityFieldInfo : IEntityField {
		public Type EntityFieldType { get; set; }
		public PropertyInfo Property { get; set; }
		public FieldAttribute Attribute { get; set; }
	}
	public class EntityJoinInfo : IEntityField {
		public Type EntityFieldType { get; set; }
		public PropertyInfo Property { get; set; }
		public JoinAttribute Attribute { get; set; }
	}
	public class EntityMemberPath {
		public List<string> Path { get; set; }
		public string GetFullPath() {
			var r = GetPathTrimRight();
			return r;
		}
		public string GetPath() {
			var r = GetPathTrimRight(1);
			return r;
		}
		private string GetPathTrimRight(int trimRight = 0) {
			StringBuilder pathStringBuilder = new StringBuilder("");
			int count = Path.Count - trimRight;
			for (int i = 0; i < count; i++) {
				pathStringBuilder.Append(Path[i]);
				if (i + 1 < count) {
					pathStringBuilder.Append("__");
				}
			}
			string path = pathStringBuilder.ToString();
			if (string.IsNullOrWhiteSpace(path)) {
				return "o";
			} else {
				return path;
			}
		}
	}
}
