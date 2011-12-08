using NQuery.Runtime;

namespace NQuery
{
	/// <remarks>
	/// For a conceptual overview of code assistance see <a href="/Overview/CodeAssistance.html">Code Assistance</a>.
	/// </remarks>
	public interface IParameterInfoAcceptor
	{
		void AcceptFunction(FunctionBinding binding);
		void AcceptMethod(MethodBinding binding);
		void AcceptAggregate(AggregateBinding binding);
	}
}