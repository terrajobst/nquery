using System;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class JoinTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void FilterNotMergedWithJoinHavingPassthruPredicate()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void SimpleExplicitInnerJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void NoJoinCondition()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void LeftHashJoin()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void RightHashJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void LeftLoopJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightLoopJoin()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterHashJoin1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterHashJoin2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterHashJoin3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterHashJoin4()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void LeftOuterHashJoin1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void LeftOuterHashJoin2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void LeftOuterHashJoin3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightOuterHashJoin1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightOuterHashJoin2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightOuterHashJoin3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightOuterHashJoin4()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterLoopJoin1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterLoopJoin2()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterLoopJoin3()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void FullOuterLoopJoin4()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void FullOuterLoopJoinWithEmptyInner()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void LeftOuterLoopJoin1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void LeftOuterLoopJoin2()
		{
			RunTestOfCallingMethod();
		}

        [TestMethod]
        public void LeftOuterLoopJoinWithEmptyInner()
        {
            RunTestOfCallingMethod();
        }
	    
        [TestMethod]
		public void LeftOuterLoopJoinWithEmptyInnerAndCalculation()
        {
            RunTestOfCallingMethod();
        }

		[TestMethod]
		public void RightOuterLoopJoin1()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightOuterLoopJoin2()
		{
			RunTestOfCallingMethod();
		}

		private class Rec1
		{
			public Rec1(int key, int secKey)
			{
				Key = key;
				SecKey = secKey;
			}

			public int Key;
			public int SecKey;
		}
		
		[TestMethod]
		public void HashMatchWithMultikeysAndProbeResidual()
		{
			Rec1[] records1 = new Rec1[]
				{
					new Rec1(1, 1),
					new Rec1(1, 2),
					new Rec1(2, 1),
					new Rec1(2, 2),
			};

			Rec1[] records2 = new Rec1[]
				{
					new Rec1(1, 2),
				};
			
			Query query = new Query();
			query.DataContext.Tables.Add(records1, "Records1");
			query.DataContext.Tables.Add(records2, "Records2");
			
			query.Text = @"
SELECT	*
FROM	Records2 r2, Records1 r1
WHERE	r1.Key = r2.Key
AND		r1.SecKey = r2.SecKey
";
			DataTable result = query.ExecuteDataTable();
			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(1, result.Rows[0][0]);
			Assert.AreEqual(2, result.Rows[0][1]);
			Assert.AreEqual(1, result.Rows[0][2]);
			Assert.AreEqual(2, result.Rows[0][3]);
		}

		[TestMethod]
		public void LeftOuterJoinWithExists()
		{
			RunTestOfCallingMethod();
		}

		[TestMethod]
		public void RightOuterJoinWithExists()
		{
			RunTestOfCallingMethod();
		}
		
		[TestMethod]
		public void InnerJoinWithExists()
		{
			RunTestOfCallingMethod();
		}
	}
}
