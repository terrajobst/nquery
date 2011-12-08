using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery.Samples.CustomFunctions
{
	#region MyFunctionBinding

	/// <summary>
	/// This class represents a function that uses an expression object
	/// to calculate the result. The parameters of the expression are
	/// the parameters of the function.
	/// </summary>
	public class MyFunctionBinding : FunctionBinding
	{
		private string _name;
		private Expression<object> _body;

		public MyFunctionBinding(string name, Expression<object> body)
		{
			_name = name;
			_body = body;
		}

		public override InvokeParameter[] GetParameters()
		{
			// This function is called by NQuery to get the function's parameter
			// definitions.
			//
			// In this sample, the function parameters are given by the parameters
			// of the expression object used as the function body.

			List<MyFunctionParameter> result = new List<MyFunctionParameter>();
			foreach (ParameterBinding parameter in _body.Parameters)
			{
				MyFunctionParameter myFunctionParameter = new MyFunctionParameter(parameter);
				result.Add(myFunctionParameter);
			}

			return result.ToArray();
		}

		public override object Invoke(object[] arguments)
		{
			// This function is called by NQuery to calculate the result.
			// NQuery ensures that the count and types the of arguments 
			// are correct.
			//
			// In this sample, we just assign the argument values 
			// to the expression's parameter and evaluate it.

			for (int i = 0; i < arguments.Length; i++)
				_body.Parameters[i].Value = arguments[i];

			return _body.Evaluate();
		}

		public override Type ReturnType
		{
			// Called by NQuery to get the function's return type.
			get { return _body.Resolve(); }
		}

		public override bool IsDeterministic
		{
			// Called by NQuery to detect whether the function is deterministic.
			get { return true; }
		}

		public override string Name
		{
			// Called by NQuery to get the name of the function.
			get { return _name; }
		}
	}

	#endregion

	#region MyFunctionParameter

	/// <summary>
	/// This class represents a parameter defintion for our function.
	/// In this sample, the parameter definition is bound to an
	/// expression parameter.
	/// </summary>
	public class MyFunctionParameter : InvokeParameter
	{
		private ParameterBinding _parameter;

		public MyFunctionParameter(ParameterBinding parameter)
		{
			_parameter = parameter;
		}

		public override string Name
		{
			// Called by NQuery to the name of the function parameter.
			get { return _parameter.Name; }
		}

		public override Type DataType
		{
			// Called by NQuery to the type of the function parameter.
			get { return _parameter.DataType; }
		}
	}

	#endregion
}