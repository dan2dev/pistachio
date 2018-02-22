using System.Linq.Expressions;

namespace Pistachio {
	public enum Order {
		Asc, Desc
	}
	public class QuerySortModel {
		public LambdaExpression Field { get; set; }
		public Order Order { get; set; }
	}
}
