using System;

namespace NQuery.Tests
{
	internal static class QueryFactory
	{
		public static Query CreateQuery()
		{
			DataContext dataContext = new DataContext();
			dataContext.AddTablesAndRelations(Databases.NorthwindDataSet);
			dataContext.AddTablesAndRelations(Databases.JoinTablesDataSet);
			dataContext.AddTablesAndRelations(Databases.AdventureWorksCinemaDataSet);
			
			Query query = new Query(dataContext);
			return query;
		}
	}
}
