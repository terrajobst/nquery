using System;

using NQuery.Runtime;

namespace NQuery.Samples.CustomFunctions
{
	#region Declaration

	/// <summary>
	/// This is a static function container. All public static methods marked 
	/// with the FunctionBinding attribute are added to the data context.
	/// </summary>
	internal static class StaticFunctionContainer
	{
		[FunctionBinding("StaticFunctionFromContainer", IsDeterministic=true)]
		public static int StaticFunctionFromContainer()
		{
			return 42;
		}
	}
	
	#endregion
}
