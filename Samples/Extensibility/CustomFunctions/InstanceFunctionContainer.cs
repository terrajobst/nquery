using System;

using NQuery.Runtime;

namespace NQuery.Samples.CustomFunctions
{
	#region Declaration

	/// <summary>
	/// This is an instance function container. All public static and instance
	/// methods marked with the FunctionBinding attribute are added to the data 
	/// context.
	/// </summary>
	internal class InstanceFunctionContainer
	{
		private int _value;

		public InstanceFunctionContainer(int value)
		{
			_value = value;
		}

		[FunctionBinding("InstanceFunctionFromContainer", IsDeterministic=true)]
		public int InstanceFunctionFromContainer()
		{
			return _value;
		}
	}
	
	#endregion
}
