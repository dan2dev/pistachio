using System;
using System.Linq.Expressions;

namespace Pistachio {
	public interface IQuery { }
	public interface IQuery<T> : IQuery where T : IEntity {	}
	// Interfaces
	public interface IQueryWhereable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
		TQuery Where(Expression<Func<T, Boolean>> whereSteatment);
	}
	public interface IQuerySetable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
		TQuery Set(T entity);
	}
	public interface IQueryJoinable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
		TQuery Join(Expression<Func<T, Object>> property);
	}
	public interface IQuerySortable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
		TQuery SortBy(Expression<Func<T, Object>> field, Order order = Order.Asc);
	}
	public interface IQueryLimitable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
		TQuery Limit(int skip, int rows);
	}
	public interface IQueryCountable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
	}
	public interface IQueryDeletable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
	}
	public interface IQueryFromable<T, TQuery> : IQuery<T> where TQuery : IQuery<T> where T : IEntity {
		TQuery From(QueryBuilder query);
		TQuery From(QueryBuilderModel query);
	}
}
