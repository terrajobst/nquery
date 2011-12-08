using System;

namespace NQuery.Runtime
{
	public sealed class SumAggregateBinding : AggregateBinding
	{
		public SumAggregateBinding(string name)
			: base(name)
		{
		}

		public override IAggregator CreateAggregator(Type inputType)
		{
			if (inputType == null)
				throw ExceptionBuilder.ArgumentNull("inputType");

			// Create an expression to determine the type of inputType + inputType

			Expression<object> addExpr = new Expression<object>();
			addExpr.Parameters.Add("@Left", inputType);
			addExpr.Parameters.Add("@Right", inputType);
			addExpr.Text = "@Left + @Right";

			Type sumType;
			try
			{
				sumType = addExpr.Resolve();
			}
			catch (CompilationException)
			{
				return null;
			}

			// Now change the type of the first argument to the result type of 'inputType + inputType'

			addExpr.Parameters.Clear();
			ParameterBinding leftParam = addExpr.Parameters.Add("@Left", sumType);
			ParameterBinding rightParam = addExpr.Parameters.Add("@Right", inputType);
			addExpr.Text = "@Left + @Right";

			try
			{
				Type newSumType = addExpr.Resolve();
				if (newSumType != sumType)
					return null;
			}
			catch (CompilationException)
			{
				return null;
			}

			// Conversion from inputType to sumType

			Expression<object> convertInputToSumExpr = new Expression<object>();
			convertInputToSumExpr.Parameters.Add("@Input", inputType);
			convertInputToSumExpr.Text = "@Input";
			convertInputToSumExpr.TargetType = sumType;

			try
			{
				convertInputToSumExpr.Resolve();
			}
			catch (CompilationException)
			{
				return null;
			}

			return new SumAggregator(addExpr, leftParam, rightParam, convertInputToSumExpr);
		}
	}
}