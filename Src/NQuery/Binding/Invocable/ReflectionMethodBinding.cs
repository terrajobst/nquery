using System;
using System.Reflection;

namespace NQuery.Runtime
{
	public class ReflectionMethodBinding : MethodBinding
	{
		private string _name;
		private MethodInfo _method;
		private ReflectionParameter[] _parameters;

		public ReflectionMethodBinding(MethodInfo methodInfo)
			: this(methodInfo, methodInfo == null ? null : methodInfo.Name)
		{
		}

		public ReflectionMethodBinding(MethodInfo methodInfo, string name)
		{
			if (methodInfo == null)
				throw ExceptionBuilder.ArgumentNull("methodInfo");

			_method = methodInfo;

			if (name == null)
				_name = methodInfo.Name;
			else
				_name = name;
		}

		public override string Name
		{
			get { return _name; }
		}

		public MethodInfo Method
		{
			get { return _method; }
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
			get { return false; }
		}

		public override Type DeclaringType
		{
			get { return _method.DeclaringType; }
		}

		public override object Invoke(object target, object[] arguments)
		{
			return _method.Invoke(target, arguments);
		}
	}
}