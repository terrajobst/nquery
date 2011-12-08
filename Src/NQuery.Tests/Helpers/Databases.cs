using System;
using System.Data;
using System.IO;
using System.Reflection;

namespace NQuery.Tests
{
	internal static class Databases
	{
		public static readonly DataSet NorthwindDataSet = LoadDataSet("Northwind.xml");
		public static readonly DataSet JoinTablesDataSet = LoadDataSet("JoinTables.xml");
		public static readonly DataSet AdventureWorksCinemaDataSet = LoadDataSet("AdventureWorks Cinema.xml");

		private static DataSet LoadDataSet(string resName)
		{
			using (Stream dataSetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Databases), resName))
			{
				DataSet result = new DataSet();
				result.ReadXml(dataSetStream);
				return result;
			}
		}
	}
}