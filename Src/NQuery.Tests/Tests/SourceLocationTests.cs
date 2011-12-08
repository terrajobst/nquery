using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public class SourceLocationTests
	{
		[TestMethod]
		public void DefaultConstructorsYieldsEmpty()
		{
			SourceLocation location = new SourceLocation();
			Assert.AreEqual(location, SourceLocation.Empty);
		}

		[TestMethod]
		public void LimitsAreEnforced()
		{
			SourceLocation location;
			
			// Check this is legal.
			location = new SourceLocation(SourceLocation.MaxColumn, SourceLocation.MaxLine);
			Assert.AreEqual(location.Column, SourceLocation.MaxColumn);
			Assert.AreEqual(location.Line, SourceLocation.MaxLine);
			
			try
			{
				new SourceLocation(0, SourceLocation.MaxLine + 1);
				Assert.Fail("MaxLine + 1 is not legal");
			}
			catch(ArgumentOutOfRangeException)
			{
			}
			
			try
			{
				new SourceLocation(SourceLocation.MaxColumn + 1, 0);
				Assert.Fail("MaxColumn + 1 is not legal");
			}
			catch(ArgumentOutOfRangeException)
			{
			}

			// Check this is legal.
			location.Column = SourceLocation.MaxColumn;
			location.Line = SourceLocation.MaxLine;
			Assert.AreEqual(location.Column, SourceLocation.MaxColumn);
			Assert.AreEqual(location.Line, SourceLocation.MaxLine);
			
			try
			{
				location.Line = SourceLocation.MaxLine + 1;
				Assert.Fail("MaxLine + 1 is not legal");
			}
			catch(ArgumentOutOfRangeException)
			{
			}
			
			try
			{
				location.Column = SourceLocation.MaxColumn + 1;
				Assert.Fail("MaxColumn + 1 is not legal");
			}
			catch(ArgumentOutOfRangeException)
			{
			}
		}

		[TestMethod]
		public void CheckStatics()
		{
			SourceLocation min = new SourceLocation(0, 0);
			SourceLocation max = new SourceLocation(SourceLocation.MaxColumn, SourceLocation.MaxLine);
			Assert.AreEqual(min, SourceLocation.MinValue);
			Assert.AreEqual(max, SourceLocation.MaxValue);
		}

		[TestMethod]
		public void CheckOperators()
		{
			Assert.IsTrue(new SourceLocation(1, 1).IsAfter(SourceLocation.MinValue));
			Assert.IsTrue(new SourceLocation(0, 1).IsAfter(SourceLocation.MinValue));
			Assert.IsTrue(new SourceLocation(1, 0).IsAfter(SourceLocation.MinValue));

			Assert.IsTrue(SourceLocation.MinValue.IsBefore(new SourceLocation(1, 1)));
			Assert.IsTrue(SourceLocation.MinValue.IsBefore(new SourceLocation(0, 1)));
			Assert.IsTrue(SourceLocation.MinValue.IsBefore(new SourceLocation(1, 0)));

			Assert.IsTrue(SourceLocation.MinValue.IsBefore(SourceLocation.MaxValue));
			Assert.IsTrue(SourceLocation.MaxValue.IsAfter(SourceLocation.MinValue));			
			
			Assert.IsTrue(new SourceLocation(1, 1).IsAfterOrEqual(SourceLocation.MinValue));
			Assert.IsTrue(new SourceLocation(0, 1).IsAfterOrEqual(SourceLocation.MinValue));
			Assert.IsTrue(new SourceLocation(1, 0).IsAfterOrEqual(SourceLocation.MinValue));

			Assert.IsTrue(SourceLocation.MinValue.IsBeforeOrEqual(new SourceLocation(1, 1)));
			Assert.IsTrue(SourceLocation.MinValue.IsBeforeOrEqual(new SourceLocation(0, 1)));
			Assert.IsTrue(SourceLocation.MinValue.IsBeforeOrEqual(new SourceLocation(1, 0)));

			Assert.IsTrue(SourceLocation.MinValue.IsBeforeOrEqual(SourceLocation.MaxValue));
			Assert.IsTrue(SourceLocation.MaxValue.IsAfterOrEqual(SourceLocation.MinValue));			
		}
		
		[TestMethod]
		public void CheckCompareTo()
		{
			Assert.AreEqual(new SourceLocation(1, 1).CompareTo(SourceLocation.MinValue), 1);
			Assert.AreEqual(new SourceLocation(0, 1).CompareTo(SourceLocation.MinValue), 1);
			Assert.AreEqual(new SourceLocation(1, 0).CompareTo(SourceLocation.MinValue), 1);

			Assert.AreEqual(SourceLocation.MinValue.CompareTo(new SourceLocation(1, 1)), -1);
			Assert.AreEqual(SourceLocation.MinValue.CompareTo(new SourceLocation(0, 1)), -1);
			Assert.AreEqual(SourceLocation.MinValue.CompareTo(new SourceLocation(1, 0)), -1);

			Assert.AreEqual(SourceLocation.MinValue.CompareTo(SourceLocation.MaxValue), -1);
			Assert.AreEqual(SourceLocation.MaxValue.CompareTo(SourceLocation.MinValue), 1);
		}
		
		[TestMethod]
		public void CheckToString()
		{
			Assert.AreEqual(SourceLocation.MinValue.ToString(), String.Format("Ln {0}; Col {1}", 0, 0));
			Assert.AreEqual(SourceLocation.MaxValue.ToString(), String.Format("Ln {0}; Col {1}", SourceLocation.MaxLine, SourceLocation.MaxColumn));
		}
	}
}
