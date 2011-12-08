using System;
using System.Collections;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class ComparerTests
	{
		private Query _query;

		#region Comparer Stuff
		
		private class CompareTestDto : IComparable
		{
			public int ID;
			public string FirstName;
			public string LastName;

			public CompareTestDto(int id, string firstName, string lastName)
			{
				ID = id;
				FirstName = firstName;
				LastName = lastName;
			}

			public int CompareTo(object obj)
			{
				CompareTestDto dto = obj as CompareTestDto;
				
				if (dto == null)
					throw new ArgumentException("invalid type to compare with", "obj");
				
				return ID.CompareTo(dto.ID);
			}
		}
		
		private class NegatedComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				CompareTestDto o1 = (CompareTestDto) x;
				CompareTestDto o2 = (CompareTestDto) y;
				
				return o1.ID.CompareTo(o2.ID) * -1;
			}
		}
		
		CompareTestDto[] _compareDtos = new CompareTestDto[]
				{
					new CompareTestDto(1, "Nancy", "Davolio"),
					new CompareTestDto(2, "Andrew", "Fuller"),
					new CompareTestDto(3, "Janet", "Leverling"),
					new CompareTestDto(4, "Margaret", "Peacock"),
					new CompareTestDto(5, "Steven", "Buchanan"),
					new CompareTestDto(6, "Michael", "Suyama"),
					new CompareTestDto(7, "Robert", "King")
				};

		#endregion

		[TestInitialize]
		public void Setup()
		{
			_query = new Query();
			_query.DataContext = new DataContext();
			_query.DataContext.Tables.Add(_compareDtos, "ComparerDtos");
			_query.DataContext.Tables.Add(_compareDtos, "MyDtos");
			_query.Text = "SELECT * FROM MyDtos t GROUP BY t";
		}

		[TestMethod]
		public void WithoutCustomComparer()
		{
			DataTable dataTable = _query.ExecuteDataTable();			
			Assert.AreEqual(7, dataTable.Rows.Count);
			Assert.AreEqual(1, dataTable.Rows[0][0]);
			Assert.AreEqual(2, dataTable.Rows[1][0]);
			Assert.AreEqual(3, dataTable.Rows[2][0]);
			Assert.AreEqual(4, dataTable.Rows[3][0]);
			Assert.AreEqual(5, dataTable.Rows[4][0]);
			Assert.AreEqual(6, dataTable.Rows[5][0]);
			Assert.AreEqual(7, dataTable.Rows[6][0]);
		}

		[TestMethod]
		public void WithCustomComparer()
		{
			_query.DataContext.MetadataContext.Comparers.Register(typeof(CompareTestDto), new NegatedComparer());
			
			DataTable dataTable = _query.ExecuteDataTable();			
			Assert.AreEqual(7, dataTable.Rows.Count);
			Assert.AreEqual(7, dataTable.Rows[0][0]);
			Assert.AreEqual(6, dataTable.Rows[1][0]);
			Assert.AreEqual(5, dataTable.Rows[2][0]);
			Assert.AreEqual(4, dataTable.Rows[3][0]);
			Assert.AreEqual(3, dataTable.Rows[4][0]);
			Assert.AreEqual(2, dataTable.Rows[5][0]);
			Assert.AreEqual(1, dataTable.Rows[6][0]);
		}
	}
}
