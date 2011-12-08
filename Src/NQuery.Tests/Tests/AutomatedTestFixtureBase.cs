using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
    public abstract class AutomatedTestFixtureBase
	{
	    protected static void RunTest(string resNameOfTestDefinition)
		{
			TestDefinition testDefinition = TestDefinition.FromResource(resNameOfTestDefinition);

			if (testDefinition == null)
				Assert.Fail("Could not find test definition XML for test '{0}'.", resNameOfTestDefinition);

			Query query = QueryFactory.CreateQuery();
			query.Text = testDefinition.CommandText;

			CompilationErrorCollection actualCompilationErrors = null;
			string actualRuntimeError = null;
			ShowPlan actualPlan = null;
			DataTable actualResults = null;

			try
			{
				actualResults = query.ExecuteDataTable();
				actualPlan = query.GetShowPlan();
			}
			catch (RuntimeException ex)
			{
				actualRuntimeError = ex.Message;
			}
			catch (CompilationException ex)
			{
				actualCompilationErrors = ex.CompilationErrors;
			}

			Assert.AreEqual(testDefinition.ExpectedRuntimeError, actualRuntimeError);
            AssertHelpers.AreEqual(testDefinition.ExpectedCompilationErrors, actualCompilationErrors);
            AssertHelpers.AreEqual(testDefinition.ExpectedResults, actualResults);
            AssertHelpers.AreEqual(testDefinition.ExpectedPlan, actualPlan);
		}

		protected static void RunTestOfCallingMethod()
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame callingFrame = stackTrace.GetFrame(1);
			MethodBase callingMethod = callingFrame.GetMethod();

			string resName = String.Format("{0}.Definitions.{1}.{2}.xml", typeof(AutomatedTestFixtureBase).Namespace, callingMethod.DeclaringType.Name, callingMethod.Name);
			RunTest(resName);
		}
	}

}
