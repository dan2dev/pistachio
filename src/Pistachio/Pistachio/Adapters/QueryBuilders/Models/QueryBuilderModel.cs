using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Pistachio.Reflection;

namespace Pistachio {
	public class QueryBuilderModel {
		// ----------------------
		public IEntity Entity { get; set; }
		public Type QueryType { get; set; }
		public Type EntityType { get; set; }
		//public List<LambdaExpression> Fields = new List<LambdaExpression>();
		public List<LambdaExpression> Where = new List<LambdaExpression>();
		public List<LambdaExpression> Join = new List<LambdaExpression>();
		public List<EntityJoinInfo> JoinAll = new List<EntityJoinInfo>();
		public List<QuerySortModel> SortBy = new List<QuerySortModel>();
		public QueryBuilderModel From = null;
		public int Skip = -1;
		public int Rows = -1;
		public int Page {
			get {
				if(Skip < 0 || Rows < 0) {
					return 1;
				} else {
					return (Skip / Rows) + 1;
				}
			}
			set {
				this.Skip = (value - 1) * Rows;
			}
		}
		public int RowsByPage {
			get {
				return this.Rows;
			}
			set {
				this.Rows = value;
			}
		}
	}
}
