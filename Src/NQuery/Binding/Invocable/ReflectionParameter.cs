using System;
using System.Reflection;

namespace NQuery.Runtime
{
	public class ReflectionParameter : InvokeParameter
	{
		private ParameterInfo _parameterInfo;

		public ReflectionParameter(ParameterInfo parameterInfo)
		{
			_parameterInfo = parameterInfo;
		}

		public override string Name
		{
			get { return _parameterInfo.Name; }
		}

		public override Type DataType
		{
			get { return _parameterInfo.ParameterType; }
		}
	}
}