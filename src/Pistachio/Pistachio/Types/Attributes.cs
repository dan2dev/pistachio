using System;

namespace Pistachio {
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class FieldAttribute : System.Attribute {
		public string FieldName { get; set; }
		public string FieldLabel { get; set; }
		public string FieldType { get; set; }
		public FieldAttribute(string fieldName, string fieldLabel = null, string fieldType = null) {
			this.FieldName = fieldName;
			this.FieldLabel = fieldLabel;
			this.FieldType = fieldType;
		}
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class CollectionAttribute : System.Attribute {
		public string ColletionName { get; set; }
		public string FieldPrimaryKey { get; set; }
		public CollectionAttribute(string colletionName = null, string fieldPrimaryKey = null) {
			this.ColletionName = colletionName;
			this.FieldPrimaryKey = fieldPrimaryKey;
		}
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class JoinAttribute : System.Attribute {
		public string LeftFieldName { get; set; }
		public string RightFieldName { get; set; }
		public JoinAttribute(string leftFieldName, string rightFieldName = null) {
			this.LeftFieldName = leftFieldName;
			this.RightFieldName = rightFieldName;
		}
	}
}
