using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using NQuery.Demo.AddIns;
using NQuery.Runtime;

namespace NQuery.Demo.DefaultAddins
{
	public sealed class LoadMsiDatabaseAddIn : IAddIn
	{
		#region MSI

		private static class Msi
		{
			private const string MSIDLL = "MSI.DLL";
			public static readonly int MsiDbPersistMode_ReadOnly = 0;
			public static readonly int MsiDbPersistMode_PatchFile = 32;

			public static readonly int MsiColInfoType_Names = 0;
			public static readonly int MsiColInfoType_Types = 1;

			private enum MsiError : uint
			{
				NoError = 0,
				Success = 0,
				FileNotFound = 2,
				AccessDenied = 5,
				InvalidHandle = 6,
				NotEnoughMemory = 8,
				InvalidData = 13,
				OutOfMemory = 14,
				InvalidParameter = 87,
				OpenFailed = 110,
				DiskFull = 112,
				CallNotImplemented = 120,
				BadPathName = 161,
				NoData = 232,
				MoreData = 234,
				NoMoreItems = 259,
				Directory = 267,
				FileInvalid = 1006,
				AppHelpBlock = 1259,
				InstallServiceFailure = 1601,
				InstallUserExit = 1602,
				InstallFailure = 1603,
				InstallSuspend = 1604,
				UnknownProduct = 1605,
				UnknownFeature = 1606,
				UnknownComponent = 1607,
				UnknownProperty = 1608,
				InvalidHandleState = 1609,
				BadConfiguration = 1610,
				IndexAbsent = 1611,
				InstallSourceAbsent = 1612,
				InstallPackageVersion = 1613,
				ProductUninstalled = 1614,
				BadQuerySyntax = 1615,
				InvalidField = 1616,
				InstallAlreadyRunning = 1618,
				InstallPackageOpenFailed = 1619,
				InstallPackageInvalid = 1620,
				InstallUIFailure = 1621,
				InstallLogFailure = 1622,
				InstallLanguageUnsupported = 1623,
				InstallTransformFailure = 1624,
				InstallPackageRejected = 1625,
				FunctionNotCalled = 1626,
				FunctionFailed = 1627,
				InvalidTable = 1628,
				DatatypeMismatch = 1629,
				UnsupportedType = 1630,
				CreateFailed = 1631,
				InstallTempUnwritable = 1632,
				InstallPlatformUnsupported = 1633,
				InstallNotUsed = 1634,
				PatchPackageOpenFailed = 1635,
				PatchPackageInvalid = 1636,
				PatchPackageUnsupported = 1637,
				ProductVersion = 1638,
				InvalidCommandLine = 1639,
				InstallRemoteDisallowed = 1640,
				SuccessRebootInitiated = 1641,
				PatchTargetNotFound = 1642,
				InstallTransformRejected = 1643,
				InstallRemoteProhibited = 1644,
				InvalidDataType = 1804,
				BadUserName = 2202,
				SucessRebootRequired = 3010,
				E_Fail = 0x80004005,
			}

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiOpenDatabase(string path, int persist, out IntPtr handle);
			
			[DllImport(MSIDLL)]
			private extern static MsiError MsiCloseHandle(IntPtr handle);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiDatabaseOpenView(IntPtr database, string query, out IntPtr view);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiViewExecute(IntPtr view, IntPtr record);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiViewGetColumnInfo(IntPtr view, int type, out IntPtr record);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static uint MsiRecordGetFieldCount(IntPtr record);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiViewFetch(IntPtr view, ref IntPtr record);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static bool MsiRecordIsNull(IntPtr record, uint field);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static int MsiRecordGetInteger(IntPtr record, uint field);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiRecordGetString(IntPtr record, uint field, StringBuilder value, ref uint valueSize);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiRecordGetString(IntPtr record, uint field, string value, ref uint valueSize);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static uint MsiRecordDataSize(IntPtr record, uint field);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiRecordReadStream(IntPtr record, uint field, byte[] buffer, ref uint bufferSize);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static IntPtr MsiGetLastErrorRecord();

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiFormatRecord(IntPtr install, IntPtr record, string result, ref uint resultSize);

			[DllImport(MSIDLL, CharSet = CharSet.Auto)]
			private extern static MsiError MsiFormatRecord(IntPtr install, IntPtr record, StringBuilder result, ref uint resultSize);

			private static void Check(MsiError actualResult)
			{
				InnerCheck(actualResult, MsiError.Success);
			}

			private static void Check(MsiError actualResult, MsiError expectedResult)
			{
				InnerCheck(actualResult, expectedResult);
			}

			private static void InnerCheck(MsiError actualResult, MsiError expectedResult)
			{
				if (actualResult != expectedResult)
				{
					StackTrace stackTrace = new StackTrace();
					string msiMethodName = "Msi" + stackTrace.GetFrame(2).GetMethod().Name;

					IntPtr recordHandle = MsiGetLastErrorRecord();
					if (recordHandle != IntPtr.Zero)
					{
						uint resultSize = 0;
						MsiError formatRecordResult = MsiFormatRecord(IntPtr.Zero, recordHandle, String.Empty, ref resultSize);
						if (formatRecordResult == MsiError.MoreData)
						{
							resultSize++;
							StringBuilder sb = new StringBuilder((int)resultSize);
							MsiFormatRecord(IntPtr.Zero, recordHandle, sb, ref resultSize);
							throw new InvalidOperationException(String.Format("Call to Windows Installer function {0} failed with error code {1:d} (0x{1:x} = {1}): {2}", msiMethodName, actualResult, sb));
						}
					}

					throw new InvalidOperationException(String.Format("Call to Windows Installer function {0} failed with error code {1:d} (0x{1:x} = {1}).", msiMethodName, actualResult));
				}
			}

			public static void OpenDatabase(string path, int persist, out IntPtr handle)
			{
				Check(MsiOpenDatabase(path, persist, out handle));
			}

			public static void CloseHandle(IntPtr handle)
			{
				Check(MsiCloseHandle(handle));
			}

			public static void DatabaseOpenView(IntPtr database, string query, out IntPtr view)
			{
				Check(MsiDatabaseOpenView(database, query, out view));
			}

			public static void ViewExecute(IntPtr view, IntPtr record)
			{
				Check(MsiViewExecute(view, record));
			}

			public static void ViewGetColumnInfo(IntPtr view, int type, out IntPtr record)
			{
				Check(MsiViewGetColumnInfo(view, type, out record));
			}

			public static uint RecordGetFieldCount(IntPtr record)
			{
				return MsiRecordGetFieldCount(record);
			}

			public static bool ViewFetch(IntPtr view, ref IntPtr record)
			{
				return (MsiViewFetch(view, ref record) == MsiError.Success);
			}

			public static bool RecordIsNull(IntPtr record, uint field)
			{
				return MsiRecordIsNull(record, field);
			}

			public static int RecordGetInteger(IntPtr record, uint field)
			{
				return MsiRecordGetInteger(record, field);
			}

			public static string RecordGetString(IntPtr record, uint field)
			{
				uint valueSize = 0;
				Check(MsiRecordGetString(record, field, String.Empty, ref valueSize), MsiError.MoreData);
				valueSize++; // Make room for terminating \0.
				StringBuilder sb = new StringBuilder((int) valueSize);
				Check(MsiRecordGetString(record, field, sb, ref valueSize));
				return sb.ToString();
			}

			public static byte[] RecordReadStream(IntPtr record, uint field)
			{
				uint dataSize = MsiRecordDataSize(record, field);
				byte[] data = new byte[dataSize];
				Check(MsiRecordReadStream(record, field, data, ref dataSize));
				return data;
			}
		}

		private enum MsiColumnDataType
		{
			String,
			Integer,
			Binary
		}

		private class MsiColumnDefinition
		{
			private string _name;
			private MsiColumnDataType _dataType;

			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}

			public MsiColumnDataType DataType
			{
				get { return _dataType; }
				set { _dataType = value; }
			}
		}

		#endregion

		public QueryContext CreateQueryContext()
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.Filter = "Windows Installer Packages (*.msi)|*.msi|Windows Installer Patches (*.msp)|*.msp|Windows Installer Merge Modules (*.msm)|*.msm|All Files (*.*)|*.*";

				if (dlg.ShowDialog() != DialogResult.OK)
					return null;

				QueryContext queryContext = new QueryContext();
				queryContext.Query = new Query();
				queryContext.QueryName = Path.GetFileName(dlg.FileName);
				ReadAllTables(queryContext.Query.DataContext, dlg.FileName);
				return queryContext;
			}
		}

		private static void ReadAllTables(DataContext dataContext, string pathToMsiFile)
		{
			IntPtr databaseHandle;
			int openFlags = Msi.MsiDbPersistMode_ReadOnly;
			if (Path.GetExtension(pathToMsiFile).Equals(".msp", StringComparison.InvariantCultureIgnoreCase))
				openFlags += Msi.MsiDbPersistMode_PatchFile;

			Msi.OpenDatabase(pathToMsiFile, openFlags, out databaseHandle);
			try
			{
				// Read table metadata table.
				DataTable tableDataTable = ReadTable(databaseHandle, "_Tables");
				// NOTE: The metadata table itself is not listed in _Tables.
				dataContext.Tables.Add(tableDataTable);

				foreach (DataRow row in tableDataTable.Rows)
				{
					string tableName = (string) row["Name"];
					DataTable table = ReadTable(databaseHandle, tableName);
					dataContext.Tables.Add(table);
				}

				// Add relations as described in the _Validation metadata table.

				DataTableBinding validationTableBinding = (DataTableBinding) dataContext.Tables["_Validation"];
				if (validationTableBinding != null)
				{
					foreach (DataRow dataRow in validationTableBinding.DataTable.Rows)
					{
						if (dataRow.IsNull("KeyTable"))
							continue;

						string tableName = (string) dataRow["Table"];
						string columnName = (string) dataRow["Column"];
						string[] keyTablesNames = ((string) dataRow["KeyTable"]).Split(';');
						int keyColumnNumber = (int) dataRow["KeyColumn"];

						foreach (string keyTableName in keyTablesNames)
						{
							if (dataContext.Tables[tableName] != null)
							{
								TableBinding keyTable = dataContext.Tables[keyTableName];
								if (keyTable != null && keyColumnNumber >= 1 && keyColumnNumber <= keyTable.Columns.Count)
								{
									string keyColumnName = keyTable.Columns[keyColumnNumber - 1].Name;
									dataContext.TableRelations.Add(tableName, new string[] {columnName}, keyTableName, new string[] {keyColumnName});
								}
							}
						}
					}
				}
			}
			finally
			{
				Msi.CloseHandle(databaseHandle);
			}
		}

		private static DataTable ReadTable(IntPtr databaseHandle, string tableName)
		{
			string query = string.Format("SELECT * FROM {0}", tableName);

			IntPtr viewHandle;
			Msi.DatabaseOpenView(databaseHandle, query, out viewHandle);
			try
			{
				List<MsiColumnDefinition> columnDefinitions = ReadColumnDefinitions(viewHandle);

				// Write columns to data table

				DataTable dataTable = new DataTable(tableName);
				for (int i = 0; i < columnDefinitions.Count; i++)
				{
					Type columnType;
					switch (columnDefinitions[i].DataType)
					{
						case MsiColumnDataType.String:
							columnType = typeof (string);
							break;
						case MsiColumnDataType.Integer:
							columnType = typeof (int);
							break;
						case MsiColumnDataType.Binary:
							columnType = typeof(byte[]);
							break;
						default:
							throw new NotImplementedException(String.Format("Unexpected column type: '{0}'", columnDefinitions[i].DataType));
					}
					dataTable.Columns.Add(columnDefinitions[i].Name, columnType);
				}

				// Read rows

				IntPtr recordHande = IntPtr.Zero;
				try
				{
					while (Msi.ViewFetch(viewHandle, ref recordHande))
					{
						DataRow dataRow = dataTable.NewRow();
						for (int i = 0; i < columnDefinitions.Count; i++)
						{
							uint column = (uint)(i + 1);
							if (Msi.RecordIsNull(recordHande, column))
							{
								dataRow[i] = DBNull.Value;
							}
							else if (columnDefinitions[i].DataType == MsiColumnDataType.Integer)
							{
								int value = Msi.RecordGetInteger(recordHande, column);
								dataRow[i] = value;
							}
							else if (columnDefinitions[i].DataType == MsiColumnDataType.String)
							{
								dataRow[i] = Msi.RecordGetString(recordHande, column);
							}
							else if (columnDefinitions[i].DataType == MsiColumnDataType.Binary)
							{
								dataRow[i] = Msi.RecordReadStream(recordHande, column);
							}
						}
						dataTable.Rows.Add(dataRow);
					}
				}
				finally
				{
					if (recordHande != IntPtr.Zero)
						Msi.CloseHandle(recordHande);
				}

				return dataTable;
			}
			finally
			{
				Msi.CloseHandle(viewHandle);
			}
		}

		private static List<MsiColumnDefinition> ReadColumnDefinitions(IntPtr viewHandle)
		{
			List<MsiColumnDefinition> columnDefinitions = new List<MsiColumnDefinition>();

			Msi.ViewExecute(viewHandle, IntPtr.Zero);
			IntPtr recordHande;

			// Get column names

			Msi.ViewGetColumnInfo(viewHandle, Msi.MsiColInfoType_Names, out recordHande);
			try
			{
				uint columnCount = Msi.RecordGetFieldCount(recordHande);

				for (int i = 0; i < columnCount; i++)
				{
					MsiColumnDefinition columnDefinition = new MsiColumnDefinition();
					columnDefinition.Name = Msi.RecordGetString(recordHande, (uint)(i + 1));
					columnDefinitions.Add(columnDefinition);
				}
			}
			finally
			{
				Msi.CloseHandle(recordHande);
			}

			// Get column types

			Msi.ViewGetColumnInfo(viewHandle, Msi.MsiColInfoType_Types, out recordHande);
			try
			{
				uint columnCount = Msi.RecordGetFieldCount(recordHande);

				for (int i = 0; i < columnCount; i++)
				{
					string columnTypeDescriptor = Msi.RecordGetString(recordHande, (uint)(i + 1));
					char columnTypeIndicator = columnTypeDescriptor[0];
					switch (Char.ToLower(columnTypeIndicator))
					{
						case 'i': // Integer
						case 'j': // Temporary integer
							columnDefinitions[i].DataType = MsiColumnDataType.Integer;
							break;

						case 's': // String
						case 'g': // Temporary string
						case 'l': 
							columnDefinitions[i].DataType = MsiColumnDataType.String;
							break;

						case 'v': // Binary Stream 
						case 'o': // Undocumented.
							columnDefinitions[i].DataType = MsiColumnDataType.Binary;
							break;

						default:
							throw new NotImplementedException(string.Format("Unexpected column type: '{0}'.", columnTypeDescriptor));
					}
				}
			}
			finally
			{
				Msi.CloseHandle(recordHande);
			}
			return columnDefinitions;
		}

		public string Name
		{
			get { return "Load MSI Database"; }
		}
	}
}
