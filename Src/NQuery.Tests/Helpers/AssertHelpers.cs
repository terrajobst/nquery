using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	internal static class AssertHelpers
	{
		private static string GetErrors(IEnumerable<CompilationError> errors)
		{
			if (errors == null)
				return "(none)";

			StringBuilder sb = new StringBuilder();
			foreach (CompilationError error in errors)
			{
				sb.Append(error.Id);
				sb.Append(": ");
				sb.AppendLine(error.Text);
			}

			return sb.ToString();
		}

		public static void AreEqual(CompilationError[] expectedCompilationErrors, CompilationErrorCollection actualCompilationErrors)
		{
			if (expectedCompilationErrors == null && actualCompilationErrors != null)
				Assert.Fail("No errors expected, but errors occured during query execution.\r\nActual errors:\r\n\r\n{0}", GetErrors(actualCompilationErrors));

			if (expectedCompilationErrors != null && actualCompilationErrors == null)
				Assert.Fail("Expected errors, but no errors occured during query execution.\r\nExpected errors:\r\n\r\n{0}", GetErrors(expectedCompilationErrors));

			try
			{
				if (expectedCompilationErrors == null)
					return;

				Assert.AreEqual(expectedCompilationErrors.Length, actualCompilationErrors.Count);

				for (int i = 0; i < expectedCompilationErrors.Length; i++)
				{
					Assert.AreEqual(expectedCompilationErrors[i].Id, actualCompilationErrors[i].Id, "Error message {0} of {1} is different", i + 1, expectedCompilationErrors.Length);
					Assert.AreEqual(expectedCompilationErrors[i].Text, actualCompilationErrors[i].Text, "Error message {0} of {1} is different", i + 1, expectedCompilationErrors.Length);
				}
			}
			catch (AssertFailedException ex)
			{
				string msg = String.Format("Expected query errors do not match actual query errors. See inner exception for first mismatch.\r\nActual errors:\r\n\r\n{0}\r\nExpected errors:\r\n\r\n{1}", GetErrors(actualCompilationErrors), GetErrors(expectedCompilationErrors));
				throw new Exception(msg, ex);
			}
		}

		public static void AreEqual(DataTable expectedResults, DataTable actualResults)
		{
			try
			{
				if (expectedResults == null && actualResults != null)
					Assert.Fail("No results expected, but query succesfully returned results.");

				if (expectedResults != null && actualResults == null)
					Assert.Fail("Expected results, but no results have been returned.");

				if (expectedResults == null)
					return;

				Assert.AreEqual(expectedResults.Rows.Count, actualResults.Rows.Count);
				Assert.AreEqual(expectedResults.Columns.Count, actualResults.Columns.Count);

				for (int rowIndex = 0; rowIndex< expectedResults.Rows.Count; rowIndex++)
				{
					DataRow expectedRow = expectedResults.Rows[rowIndex];
					DataRow actualRow = actualResults.Rows[rowIndex];

					for (int colIndex = 0; colIndex < expectedResults.Columns.Count; colIndex++)
					{
						object expectedValue = expectedRow[colIndex];
						object actualValue = actualRow[colIndex];
					
						AreEqual(expectedValue, actualValue);
					}
				}
			}
			catch (AssertFailedException ex)
			{
				throw new Exception("Expected query results do not match actual results. See inner exception for first mismatch.", ex);
			}
		}

		private static void AreEqual(object expectedValue, object actualValue)
		{
			if (expectedValue is byte[] && actualValue is byte[])
				AreEqual((byte[]) expectedValue, (byte[]) actualValue);
			else
				Assert.AreEqual(expectedValue, actualValue);
		}

		public static void AreEqual(byte[] expectedValue, byte[] actualValue)
		{
			string expectedBase64String = Convert.ToBase64String(expectedValue);
			string actualBase64String = Convert.ToBase64String(actualValue);
			Assert.AreEqual(expectedBase64String, actualBase64String);
		}

		public static void AreEqual(ShowPlan expectedPlan, ShowPlan actualPlan)
		{
			if (expectedPlan == null && actualPlan != null)
				Assert.Fail("No plan expected, but query produced a plan. Check test defintion to ensure a propery plan is included.");

			if (expectedPlan != null && actualPlan == null)
				Assert.Fail("Query did not produce an execution plan.");

			if (expectedPlan == null)
				return;

			try
			{
				AreEqual(expectedPlan.Root, actualPlan.Root);
			}
			catch (AssertFailedException ex)
			{
				throw new Exception("Expected plan did not match actual plan. See inner exception for first mismatch.", ex);
			}
		}

		private static void AreEqual(ShowPlanElement expectedPlanElement, ShowPlanElement actualPlanElement)
		{
			Assert.AreEqual(expectedPlanElement.Operator, actualPlanElement.Operator);

			Assert.AreEqual(expectedPlanElement.Properties.Count, actualPlanElement.Properties.Count, "Property count does not match.");
			for (int i = 0; i < expectedPlanElement.Properties.Count; i++)
			{
				ShowPlanProperty expectedProperty = expectedPlanElement.Properties[i];
				ShowPlanProperty actualProperty = actualPlanElement.Properties[i];

				Assert.AreEqual(expectedProperty.FullName, actualProperty.FullName, "Property full name does not match.");
				Assert.AreEqual(expectedProperty.Value, actualProperty.Value, "Value of property {0} does not match.", expectedProperty.FullName);
			}

			Assert.AreEqual(expectedPlanElement.Children.Count, actualPlanElement.Children.Count, "Count of children does not match.");
			for (int i = 0; i < expectedPlanElement.Children.Count; i++)
			{
				ShowPlanElement expectedChildElement = expectedPlanElement.Children[i];
				ShowPlanElement actualChildElement = actualPlanElement.Children[i];

				AreEqual(expectedChildElement, actualChildElement);
			}
		}
	}
}