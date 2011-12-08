using NQuery.Runtime;

namespace NQuery
{
	/// <remarks>
	/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
	/// </remarks>
	public interface IMemberCompletionAcceptor
	{
		void AcceptKeyword(string keyword);
		void AcceptAggregate(AggregateBinding binding);
		void AcceptFunction(FunctionBinding binding);
		void AcceptMethod(MethodBinding binding);
		void AcceptTable(TableBinding binding);
		void AcceptTableRef(TableRefBinding binding);
		void AcceptColumnRef(ColumnRefBinding binding);
		void AcceptProperty(PropertyBinding binding);
		void AcceptParameter(ParameterBinding binding);
		void AcceptConstant(ConstantBinding binding);
		void AcceptRelation(TableRefBinding parentBinding, TableRefBinding childBinding, TableRelation relation);
	}
}