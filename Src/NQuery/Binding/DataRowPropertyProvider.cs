using System;
using System.Collections.Generic;
using System.Data;

namespace NQuery.Runtime
{
	/// <summary>
	/// Provides properties for an instance of <see cref="DataRow"/>.
	/// </summary>
    public static class DataRowPropertyProvider
	{
		/// <summary>
		/// Returns the list of properties for the given instance.
		/// </summary>
		/// <remarks>
		/// The query engine does not cache the result.
		/// </remarks>
		/// <param name="dataTable">The data table to get the properties for.</param>
		/// <returns>A list of <see cref="PropertyBinding"/> for the given instance.</returns>
		public static DataColumnPropertyBinding[] GetProperties(DataTable dataTable)
		{
			if (dataTable == null)
				throw ExceptionBuilder.ArgumentNull("dataTable");

			List<DataColumnPropertyBinding> dataColumnPropertyList = new List<DataColumnPropertyBinding>();

			foreach (DataColumn dataColumn in dataTable.Columns)
			{
				dataColumnPropertyList.Add(new DataColumnPropertyBinding(dataColumn));
			}

			return dataColumnPropertyList.ToArray();
		}
	}
}