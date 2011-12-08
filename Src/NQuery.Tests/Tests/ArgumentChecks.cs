using System;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Compilation;
using NQuery.Runtime;

namespace NQuery.Tests
{
	[TestClass]
	public class ArgumentChecks
	{
		private MethodInfo _aMethodInfo = typeof(GC).GetMethod("CollectionCount");
		private DataTableBinding _aTableBinding = new DataTableBinding(Databases.NorthwindDataSet.Tables["Employees"]);
		private Runner _aDelegate;
		private ParameterInfo _aParameterInfo;
		private FunctionBinding _aFunctionBinding;

		public ArgumentChecks()
		{
			_aDelegate = ExceptionBuilder;
			_aParameterInfo = _aMethodInfo.GetParameters()[0];
			_aFunctionBinding = new ReflectionFunctionBinding(_aMethodInfo, null, false);
		}

		#region Helpers

		private delegate void Runner();

		private static void ExpectArgumentNull(string paramName, Runner runner)
		{
			try
			{
				runner();
			}
			catch (ArgumentNullException ex)
			{
				Assert.AreEqual(paramName, ex.ParamName);
				return;
			}
			Assert.Fail("Expected ArgumentNullException for parameter '{0}'.", paramName);
		}

		private static void IgnoreValue(object value)
		{
			// Just to remove Resharper warning ("Parameter 'value' is never used")
			GC.KeepAlive(value);
		}

		private class MyEvaluatable : Evaluatable
		{
			public MyEvaluatable(string text, DataContext dataContext)
				: base(text, dataContext)
			{
			}

			public MyEvaluatable(string text)
				: base(text)
			{
			}

			public override ICodeAssistanceContextProvider GetCodeAssistanceContextProvider()
			{
				return null;
			}
		}

		#endregion

		[TestMethod]
		public void CompilationError()
		{
			ExpectArgumentNull("text", delegate { new CompilationError(SourceRange.Empty, ErrorId.AggregateCannotContainAggregate, null); });
		}

		[TestMethod]
		public void CompilationException()
		{
			ExpectArgumentNull("info", delegate { new CompilationException().GetObjectData(null, new StreamingContext()); });
		}

		[TestMethod]
		public void CompilationFailedEventArgs()
		{
			ExpectArgumentNull("errors", delegate { new CompilationFailedEventArgs(null); });
		}

		[TestMethod]
		public void ConstantCollection()
		{
			DataContext dataContext = new DataContext();
			ExpectArgumentNull("item", delegate { dataContext.Constants.Add(null); });
			ExpectArgumentNull("constantName", delegate { dataContext.Constants.Add(null, 1); });
			ExpectArgumentNull("value", delegate { dataContext.Constants.Add("Test", null); });
			ExpectArgumentNull("constantName", delegate { dataContext.Constants.Add(null, 1, new PropertyBinding[0]); });
			ExpectArgumentNull("value", delegate { dataContext.Constants.Add("Test", null, new PropertyBinding[0]); });
			ExpectArgumentNull("customProperties", delegate { dataContext.Constants.Add("Test", 1, null); });
			ExpectArgumentNull("identifier", delegate { dataContext.Constants.Find(null); });
			ExpectArgumentNull("item", delegate { dataContext.Constants.Insert(0, null); });
			ExpectArgumentNull("name", delegate { dataContext.Constants.Remove((string)null); });
			ExpectArgumentNull("bindingName", delegate { IgnoreValue(dataContext.Constants[null]); });
		}

		[TestMethod]
		public void DataContext()
		{
			DataContext dataContext = new DataContext();
			ExpectArgumentNull("dataSet", delegate { dataContext.AddTablesAndRelations(null); });
		}

		[TestMethod]
		public void Evaluatable()
		{
			ExpectArgumentNull("text", delegate { new MyEvaluatable(null); });
			ExpectArgumentNull("text", delegate { new MyEvaluatable(null, null); });
		}
		
		[TestMethod]
		public void ExceptionBuilder()
		{
			// Although the exception builder is not part of the public interface I want to
			// make sure that it throws exceptions when it is wrongly called. Otherwise
			// users could have a hard time to figure out why NQuery crashes...

			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.AllColumnsMustBelongToSameTable(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ArgumentArrayMustHaveSameSize(null, "test"); });
			ExpectArgumentNull("paramNameOfReferenceArray", delegate { NQuery.ExceptionBuilder.ArgumentArrayMustHaveSameSize("test", null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ArgumentArrayMustNotBeEmpty(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ArgumentInvalidIdentifier(null, "test"); });
			ExpectArgumentNull("message", delegate { NQuery.ExceptionBuilder.ArgumentInvalidIdentifier("test", null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ArgumentMustBeOfType(null, typeof(int)); });
			ExpectArgumentNull("type", delegate { NQuery.ExceptionBuilder.ArgumentMustBeOfType("test", null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ArgumentNull(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ArgumentOutOfRange(null, 0, 1, 2); });
			ExpectArgumentNull("actualValue", delegate { NQuery.ExceptionBuilder.ArgumentOutOfRange("test", null, 1, 2); });
			ExpectArgumentNull("minValue", delegate { NQuery.ExceptionBuilder.ArgumentOutOfRange("test", 0, null, 2); });
			ExpectArgumentNull("maxValue", delegate { NQuery.ExceptionBuilder.ArgumentOutOfRange("test", 0, 1, null); });
			ExpectArgumentNull("message", delegate { NQuery.ExceptionBuilder.AssertionFailed(null); });
			ExpectArgumentNull("binaryOperator", delegate { NQuery.ExceptionBuilder.BinaryOperatorFailed(null, _aMethodInfo, typeof(int), typeof(int), 1, 2, new ArgumentException()); });
			ExpectArgumentNull("operatorMethod", delegate { NQuery.ExceptionBuilder.BinaryOperatorFailed(BinaryOperator.Add, null, typeof(int), typeof(int), 1, 2, new ArgumentException()); });
			ExpectArgumentNull("leftOperandType", delegate { NQuery.ExceptionBuilder.BinaryOperatorFailed(BinaryOperator.Add, _aMethodInfo, null, typeof(int), 1, 2, new ArgumentException()); });
			ExpectArgumentNull("rightOperandType", delegate { NQuery.ExceptionBuilder.BinaryOperatorFailed(BinaryOperator.Add, _aMethodInfo, typeof(int), null, 1, 2, new ArgumentException()); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.BinaryOperatorFailed(BinaryOperator.Add, _aMethodInfo, typeof(int), typeof(int), 1, 2, null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.BindingWithSameNameAlreadyInCollection(null, new ParameterBinding("test", typeof(int))); });
			ExpectArgumentNull("binding", delegate { NQuery.ExceptionBuilder.BindingWithSameNameAlreadyInCollection("test", null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.CastingOperatorFailed(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ChildColumnNotFound(null, _aTableBinding, "EmployeeId"); });
			ExpectArgumentNull("childTable", delegate { NQuery.ExceptionBuilder.ChildColumnNotFound("test", null, "EmployeeId"); });
			ExpectArgumentNull("childColumn", delegate { NQuery.ExceptionBuilder.ChildColumnNotFound("test", _aTableBinding, null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ChildTableMustExistInDataContext(null); });
			ExpectArgumentNull("errors", delegate { NQuery.ExceptionBuilder.CodeAssistanceFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.ColumnBindingGetValueFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.ConstantBindingGetValueFailed(null); });
			ExpectArgumentNull("targetType", delegate { NQuery.ExceptionBuilder.ConversionFailed(null, 1, new Exception()); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.ConversionFailed(typeof(int), 1, null); });
			ExpectArgumentNull("errors", delegate { NQuery.ExceptionBuilder.ExpressionCompilationFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.FunctionBindingInvokeFailed(null); });
			ExpectArgumentNull("function", delegate { NQuery.ExceptionBuilder.FunctionMustNotBeVoid(null); });
			ExpectArgumentNull("function", delegate { NQuery.ExceptionBuilder.FunctionMustNotHaveArrayParams(null, _aParameterInfo); });
			ExpectArgumentNull("parameter", delegate { NQuery.ExceptionBuilder.FunctionMustNotHaveArrayParams(_aDelegate, null); });
			ExpectArgumentNull("function", delegate { NQuery.ExceptionBuilder.FunctionMustNotHaveOptionalParams(null, _aParameterInfo); });
			ExpectArgumentNull("parameter", delegate { NQuery.ExceptionBuilder.FunctionMustNotHaveOptionalParams(_aDelegate, null); });
			ExpectArgumentNull("function", delegate { NQuery.ExceptionBuilder.FunctionMustNotHaveRefOrOutParams(null, _aParameterInfo); });
			ExpectArgumentNull("parameter", delegate { NQuery.ExceptionBuilder.FunctionMustNotHaveRefOrOutParams(_aDelegate, null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.FunctionWithSameNameAndSignatureAlreadyInCollection(null, _aFunctionBinding); });
			ExpectArgumentNull("functionBinding", delegate { NQuery.ExceptionBuilder.FunctionWithSameNameAndSignatureAlreadyInCollection("test", null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.IAggregatorAccumulateFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.IAggregatorInitFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.IAggregatorTerminateFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.IMethodProviderGetMethodsFailed(null); });
			ExpectArgumentNull("format", delegate { NQuery.ExceptionBuilder.InternalError(null); });
			ExpectArgumentNull("type", delegate { NQuery.ExceptionBuilder.InternalErrorGetValueNotSupported(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.IPropertyProviderGetPropertiesFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.MethodBindingInvokeFailed(null); });
			ExpectArgumentNull("type", delegate { NQuery.ExceptionBuilder.NoPropertyProviderRegisteredAndDefaultProviderIsMissing(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.ParameterBindingGetValueFailed(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ParameterValueTypeMismatch(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ParentColumnNotFound(null, _aTableBinding, "test"); });
			ExpectArgumentNull("parentTable", delegate { NQuery.ExceptionBuilder.ParentColumnNotFound("test", null, "test"); });
			ExpectArgumentNull("parentColumn", delegate { NQuery.ExceptionBuilder.ParentColumnNotFound("test", _aTableBinding, null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ParentColumnNotFound(null, _aTableBinding, "test"); });
			ExpectArgumentNull("parentTable", delegate { NQuery.ExceptionBuilder.ParentColumnNotFound("test", null, "test"); });
			ExpectArgumentNull("parentColumn", delegate { NQuery.ExceptionBuilder.ParentColumnNotFound("test", _aTableBinding, null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.ParentTableMustExistInDataContext(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.PropertyBindingGetValueFailed(null); });
			ExpectArgumentNull("propertyName", delegate { NQuery.ExceptionBuilder.PropertyNotInitialized(null); });
			ExpectArgumentNull("errors", delegate { NQuery.ExceptionBuilder.QueryCompilationFailed(null); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.RuntimeError(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.TableMustHaveAtLeastOneColumn(null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.TargetTypeMismatch(null, typeof(int), typeof(int)); });
			ExpectArgumentNull("genericType", delegate { NQuery.ExceptionBuilder.TargetTypeMismatch("test", null, typeof(int)); });
			ExpectArgumentNull("targetType", delegate { NQuery.ExceptionBuilder.TargetTypeMismatch("test", typeof(int), null); });
			ExpectArgumentNull("paramName", delegate { NQuery.ExceptionBuilder.TypeAlreadyRegistered(null, typeof(int)); });
			ExpectArgumentNull("type", delegate { NQuery.ExceptionBuilder.TypeAlreadyRegistered("test", null); });
			ExpectArgumentNull("unaryOperator", delegate { NQuery.ExceptionBuilder.UnaryOperatorFailed(null, _aMethodInfo, typeof(int), 1, new Exception()); });
			ExpectArgumentNull("operatorMethod", delegate { NQuery.ExceptionBuilder.UnaryOperatorFailed(UnaryOperator.Negation, null, typeof(int), 1, new Exception()); });
			ExpectArgumentNull("expressionType", delegate { NQuery.ExceptionBuilder.UnaryOperatorFailed(UnaryOperator.Negation, _aMethodInfo, null, 1, new Exception()); });
			ExpectArgumentNull("exception", delegate { NQuery.ExceptionBuilder.UnaryOperatorFailed(UnaryOperator.Negation, _aMethodInfo, typeof(int), 1, null); });
		}

		[TestMethod]
		public void Expression()
		{
			ExpectArgumentNull("text", delegate { new Expression<bool>((string) null); });
			ExpectArgumentNull("text", delegate { new Expression<bool>(null, null); });
			ExpectArgumentNull("value", delegate { new Expression<bool>().TargetType = null; });
		}

		[TestMethod]
		public void FunctionCollection()
		{
			DataContext dataContext = new DataContext();
			ExpectArgumentNull("functionName", delegate { dataContext.Functions.Add(null, new Runner(FunctionCollection)); });
			ExpectArgumentNull("functionDelegate", delegate { dataContext.Functions.Add("Test", null); });
			ExpectArgumentNull("containerType", delegate { dataContext.Functions.AddFromContainer((Type)null); });
			ExpectArgumentNull("container", delegate { dataContext.Functions.AddFromContainer((object)null); });
			ExpectArgumentNull("containerType", delegate { dataContext.Functions.RemoveFromContainer((Type)null); });
			ExpectArgumentNull("container", delegate { dataContext.Functions.RemoveFromContainer((object)null); });
			ExpectArgumentNull("identifier", delegate { dataContext.Functions.Find(null); });
			ExpectArgumentNull("item", delegate { dataContext.Functions.Insert(0, null); });
			ExpectArgumentNull("name", delegate { dataContext.Functions.Remove((string)null); });
			ExpectArgumentNull("bindingName", delegate { IgnoreValue(dataContext.Functions[null]); });
		}

		[TestMethod]
		public void Identifier()
		{
			ExpectArgumentNull("text", delegate { NQuery.Identifier.CreateNonVerbatim(null); });
			ExpectArgumentNull("text", delegate { NQuery.Identifier.CreateVerbatim(null); });
			ExpectArgumentNull("text", delegate { NQuery.Identifier.FromSource(null); });
			ExpectArgumentNull("text", delegate { NQuery.Identifier.MustBeParenthesized(null); });
		}

		[TestMethod]
		public void MetadataContext()
		{
			MetadataContext metadataContext = new MetadataContext();
			ExpectArgumentNull("type", delegate { metadataContext.FindMethod(null, NQuery.Identifier.CreateNonVerbatim("Test")); });
			ExpectArgumentNull("identifier", delegate { metadataContext.FindMethod(typeof(int), null); });
			ExpectArgumentNull("type", delegate { metadataContext.FindProperty((Type) null, NQuery.Identifier.CreateNonVerbatim("Test")); });
			ExpectArgumentNull("identifier", delegate { metadataContext.FindProperty(typeof(int), null); });
			ExpectArgumentNull("properties", delegate { metadataContext.FindProperty((PropertyBinding[])null, NQuery.Identifier.CreateNonVerbatim("Test")); });
		}

		[TestMethod]
		public void ParameterCollection()
		{
			Query query = new Query();
			ExpectArgumentNull("item", delegate { query.Parameters.Add(null); });
			ExpectArgumentNull("parameterName", delegate { query.Parameters.Add(null, typeof(int)); });
			ExpectArgumentNull("parameterType", delegate { query.Parameters.Add("Test", null); });
			ExpectArgumentNull("parameterName", delegate { query.Parameters.Add(null, typeof(int), 1); });
			ExpectArgumentNull("parameterType", delegate { query.Parameters.Add("Test", null, 1); });
			ExpectArgumentNull("parameterName", delegate { query.Parameters.Add(null, typeof(int), 1, new PropertyBinding[0]); });
			ExpectArgumentNull("parameterType", delegate { query.Parameters.Add("Test", null, 1, new PropertyBinding[0]); });
			ExpectArgumentNull("customProperties", delegate { query.Parameters.Add("Test", typeof(int), 1, null); });
			ExpectArgumentNull("identifier", delegate { query.Parameters.Find(null); });
			ExpectArgumentNull("item", delegate { query.Parameters.Insert(0, null); });
			ExpectArgumentNull("name", delegate { query.Parameters.Remove((string)null); });
			ExpectArgumentNull("bindingName", delegate { IgnoreValue(query.Parameters[null]); });
		}

		[TestMethod]
		public void QueryDataReader()
		{
			Query query = QueryFactory.CreateQuery();
			query.Text = "SELECT * FROM Employees";
			using (QueryDataReader queryDataReader = query.ExecuteDataReader())
			{
				ExpectArgumentNull("values", delegate { queryDataReader.GetValues(null); });
				ExpectArgumentNull("name", delegate { queryDataReader.GetOrdinal(null); });
				ExpectArgumentNull("name", delegate { IgnoreValue(queryDataReader[null]); });
			}
		}

		[TestMethod]
		public void ShowPlan()
		{
			ExpectArgumentNull("navigable", delegate { NQuery.ShowPlan.FromXml(null); });

			Query query = QueryFactory.CreateQuery();
			query.Text = "SELECT * FROM Employees";
			ShowPlan showPlan = query.GetShowPlan();
			ExpectArgumentNull("textWriter", delegate { showPlan.WriteTo(null, 1); });
		}

		[TestMethod]
		public void TableCollection()
		{
			DataContext dataContext = new DataContext();
			ExpectArgumentNull("item", delegate { dataContext.Tables.Add((TableBinding)null); });
			ExpectArgumentNull("dataTable", delegate { dataContext.Tables.Add((DataTable)null); });
			ExpectArgumentNull("enumerable", delegate { dataContext.Tables.Add(null, typeof(int), "test"); });
			ExpectArgumentNull("elementType", delegate { dataContext.Tables.Add(new int[0], null, "test"); });
			ExpectArgumentNull("tableName", delegate { dataContext.Tables.Add(new int[0], typeof(int), null); });
			ExpectArgumentNull("enumerable", delegate { dataContext.Tables.Add<int>(null, "test"); });
			ExpectArgumentNull("tableName", delegate { dataContext.Tables.Add(new int[0], null); });
			ExpectArgumentNull("dataTables", delegate { dataContext.Tables.AddRange(null); });
			ExpectArgumentNull("identifier", delegate { dataContext.Tables.Find(null); });
			ExpectArgumentNull("item", delegate { dataContext.Tables.Insert(0, null); });
			ExpectArgumentNull("name", delegate { dataContext.Tables.Remove((string)null); });
			ExpectArgumentNull("bindingName", delegate { IgnoreValue(dataContext.Tables[null]); });
		}

		[TestMethod]
		public void TableRelation()
		{
			ExpectArgumentNull("parentColumns", delegate { new TableRelation(null, new ColumnBinding[0]); });
			ExpectArgumentNull("childColumns", delegate { new TableRelation(new ColumnBinding[0], null); });
		}

		[TestMethod]
		public void TableRelationCollection()
		{
			DataContext dataContext = new DataContext();
			ExpectArgumentNull("item", delegate { dataContext.TableRelations.Add((TableRelation)null); });
			ExpectArgumentNull("dataRelation", delegate { dataContext.TableRelations.Add((DataRelation)null); });
			ExpectArgumentNull("parentColumns", delegate { dataContext.TableRelations.Add(null, new ColumnBinding[1]); });
			ExpectArgumentNull("childColumns", delegate { dataContext.TableRelations.Add(new ColumnBinding[1], null); });
			ExpectArgumentNull("parentTable", delegate { dataContext.TableRelations.Add(null, new string[1], "test", new string[1]); });
			ExpectArgumentNull("parentColumns", delegate { dataContext.TableRelations.Add("test", null, "test", new string[1]); });
			ExpectArgumentNull("childTable", delegate { dataContext.TableRelations.Add("test", new string[1], null, new string[1]); });
			ExpectArgumentNull("childColumns", delegate { dataContext.TableRelations.Add("test", new string[1], "test", null); });
			ExpectArgumentNull("parentTable", delegate { dataContext.TableRelations.Add(null, new string[1], _aTableBinding, new string[1]); });
			ExpectArgumentNull("parentColumns", delegate { dataContext.TableRelations.Add(_aTableBinding, null, _aTableBinding, new string[1]); });
			ExpectArgumentNull("childTable", delegate { dataContext.TableRelations.Add(_aTableBinding, new string[1], null, new string[1]); });
			ExpectArgumentNull("childColumns", delegate { dataContext.TableRelations.Add(_aTableBinding, new string[1], _aTableBinding, null); });
			ExpectArgumentNull("dataRelations", delegate { dataContext.TableRelations.AddRange(null); });
			ExpectArgumentNull("table", delegate { dataContext.TableRelations.GetRelations((TableBinding) null); });
			ExpectArgumentNull("table", delegate { dataContext.TableRelations.GetChildRelations((TableBinding)null); });
			ExpectArgumentNull("table", delegate { dataContext.TableRelations.GetParentRelations((TableBinding)null); });
		}

		[TestMethod]
		public void TypeRegistry()
		{
			TypeRegistry<object> myTypeRegistry = new TypeRegistry<object>();
			ExpectArgumentNull("key", delegate { myTypeRegistry.Register(null, 1); });
			ExpectArgumentNull("value", delegate { myTypeRegistry.Register(typeof(int), null); });
			ExpectArgumentNull("key", delegate { myTypeRegistry.Unregister((Type)null); });
			ExpectArgumentNull("value", delegate { myTypeRegistry.Unregister((object)null); });
			ExpectArgumentNull("key", delegate { myTypeRegistry.GetValue(null); });
			ExpectArgumentNull("key", delegate { myTypeRegistry.IsRegistered(null); });
			ExpectArgumentNull("key", delegate { myTypeRegistry[null] = 1; });
			ExpectArgumentNull("value", delegate { myTypeRegistry[typeof(int)] = null; });
		}
	}
}
