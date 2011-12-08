using System;
using System.Reflection;

namespace NQuery.Runtime
{
	public class ReflectionFunctionBinding: FunctionBinding
	{
		private string _name;
		private MethodInfo _method;
		private object _instance;
		private ReflectionParameter[] _parameters;
		private bool _isDeterministic;

		public ReflectionFunctionBinding(string name, MethodInfo method, object instance, bool isDeterministic)
		{
			if (name == null)
				throw ExceptionBuilder.ArgumentNull("name");

			if (method == null)
				throw ExceptionBuilder.ArgumentNull("method");

			_name = name;
			_method = method;
			_instance = instance;
			_isDeterministic = isDeterministic;
		}

		public ReflectionFunctionBinding(MethodInfo method, object instance, bool isDeterministic)
		{
			if (method == null)
				throw ExceptionBuilder.ArgumentNull("method");

			_name = method.Name;
			_method = method;
			_instance = instance;
			_isDeterministic = isDeterministic;
		}

		public override string Name
		{
			get { return _name; }
		}

		public MethodInfo Method
		{
			get { return _method; }
		}

		public Object Instance
		{
			get { return _instance; }
		}

		public override InvokeParameter[] GetParameters()
		{
			if (_parameters == null)
			{
				ParameterInfo[] parameterInfos = _method.GetParameters();
				
				_parameters = new ReflectionParameter[parameterInfos.Length];
				for (int i = 0; i < _parameters.Length; i++)
					_parameters[i] = new ReflectionParameter(parameterInfos[i]);
			}

			return _parameters;
		}

		public override Type ReturnType
		{
			get { return _method.ReturnType; }
		}
		
		public override bool IsDeterministic
		{
			get { return _isDeterministic; }
		}

		public override object Invoke(object[] arguments)
		{
			return _method.Invoke(_instance, arguments);
		}
	}
}