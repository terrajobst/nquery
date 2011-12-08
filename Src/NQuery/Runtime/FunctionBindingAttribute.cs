using System;

namespace NQuery.Runtime
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class FunctionBindingAttribute : Attribute
	{
		private string _name;
		private bool _isDeterministic;

		public FunctionBindingAttribute(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}
		
		public bool IsDeterministic
		{
			get { return _isDeterministic; }
			set { _isDeterministic = value; }
		}
	}
}