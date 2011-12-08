using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace NQuery.Demo
{
	internal static class DataSetManager
	{
		#region Default Query Text

		private const string _defaultQueryText = @"/*
|| DEMO.SQL
||
|| Gets a list of all employee and assigned territories.
*/

SELECT	TOP 5
		e.LastName + ', ' + e.FirstName Employee,
		COUNT(*) [Territory Count],
		CONCAT(r.RegionDescription) Regions,
		CONCAT(t.TerritoryDescription) Territories

FROM	Region r,
		Employees e,
		Territories t,
		EmployeeTerritories et
	
WHERE	e.EmployeeID = et.EmployeeID
AND		t.TerritoryID = et.TerritoryID
AND     r.RegionID = t.RegionID

GROUP   BY e.LastName + ', ' + e.FirstName

ORDER	BY count(*) DESC
";
		#endregion

		private static string GetDataDirectory()
		{
			return Path.Combine(Application.StartupPath, "Data");
		}

		public static string[] GetAllDatabaseFiles()
		{
			return Directory.GetFiles(GetDataDirectory(), "*.xml", SearchOption.TopDirectoryOnly);
		}

		public static string DefaultDataSet
		{
			get { return Path.Combine(GetDataDirectory(), "Northwind.xml"); }
		}

		public static string DefaultQueryText
		{
			get { return _defaultQueryText; }
		}

		public static DataSet GetDataSet(string dataSetPath)
		{
			DataSet dataSet = new DataSet();
			dataSet.ReadXml(dataSetPath);
			return dataSet;
		}
	}
}
