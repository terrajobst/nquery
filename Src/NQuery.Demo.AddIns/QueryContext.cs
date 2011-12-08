using System;

namespace NQuery.Demo.AddIns
{
	public class QueryContext
	{
		private Query _query;
		private string _queryName;

		public QueryContext()
		{
		}

		public QueryContext(Query query, string queryName)
		{
			_query = query;
			_queryName = queryName;
		}

		public Query Query
		{
			get { return _query; }
			set { _query = value; }
		}

		public string QueryName
		{
			get { return _queryName; }
			set { _queryName = value; }
		}
	}
}