using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

namespace NQuery.Tests
{
	internal sealed class TestDefinition
	{
		private string _commandText;
		private CompilationError[] _expectedCompilationErrors;
		private string _expectedRuntimeError;
		private DataTable _expectedResults;
		private ShowPlan _expectedPlan;

		public TestDefinition()
		{
		}

		public string CommandText
		{
			get { return _commandText; }
			set { _commandText = value; }
		}

		public string ExpectedRuntimeError
		{
			get { return _expectedRuntimeError; }
			set { _expectedRuntimeError = value; }
		}

		public CompilationError[] ExpectedCompilationErrors
		{
			get { return _expectedCompilationErrors; }
			set { _expectedCompilationErrors = value; }
		}

		public DataTable ExpectedResults
		{
			get { return _expectedResults; }
			set { _expectedResults = value; }
		}

		public ShowPlan ExpectedPlan
		{
			get { return _expectedPlan; }
			set { _expectedPlan = value; }
		}

		public static TestDefinition FromResource(string resName)
		{
			using (Stream testDefinitionStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
			{
				if (testDefinitionStream == null)
					return null;

				XmlDocument testDefinitionXml = new XmlDocument();
				testDefinitionXml.Load(testDefinitionStream);
				return FromXml(testDefinitionXml);
			}
		}

		public static TestDefinition FromXml(XmlDocument xmlDocument)
		{
			TestDefinition result = new TestDefinition();
			result.CommandText = xmlDocument.SelectSingleNode("/test/sql").InnerText;

			XmlNode expectedRuntimeErrorNode = xmlDocument.SelectSingleNode("/test/expectedRuntimeError");
			if (expectedRuntimeErrorNode != null)
				result.ExpectedRuntimeError = expectedRuntimeErrorNode.InnerText;

			XmlNode expectedErrorsNode = xmlDocument.SelectSingleNode("/test/expectedErrors");
			if (expectedErrorsNode != null)
			{
				List<CompilationError> errorList = new List<CompilationError>();
				foreach (XmlNode expectedErrorNode in expectedErrorsNode.SelectNodes("expectedError"))
				{
					ErrorId errorId = (ErrorId) Enum.Parse(typeof(ErrorId), expectedErrorNode.Attributes["id"].Value);
					string errorText = expectedErrorNode.Attributes["text"].Value;
					CompilationError compilationError = new CompilationError(SourceRange.Empty, errorId, errorText);
					errorList.Add(compilationError);
				}

				result.ExpectedCompilationErrors = errorList.ToArray();
			}

			XmlNode expectedResultsNode = xmlDocument.SelectSingleNode("/test/expectedResults");

			if (expectedResultsNode != null)
			{
				using (StringReader stringReader = new StringReader(expectedResultsNode.InnerXml))
				{
					DataSet dataSet = new DataSet();
					dataSet.ReadXml(stringReader);
					result.ExpectedResults = dataSet.Tables[0];
				}
			}

			XmlNode expectedPlanNode = xmlDocument.SelectSingleNode("/test/expectedPlan");
			if (expectedPlanNode != null)
			{
				result.ExpectedPlan = ShowPlan.FromXml(expectedPlanNode);
			}

			return result;
		}
	}
}
