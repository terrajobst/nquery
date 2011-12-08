using System;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Runtime
{
	public static class NullProviders
	{
		private sealed class NullPropertyProvider : IPropertyProvider
		{
			private PropertyBinding[] _result = new PropertyBinding[0];
			
			public PropertyBinding[] GetProperties(Type type)
			{
				return _result;
			}
		}
		
		private sealed class NullMethodProvider : IMethodProvider
		{
			private MethodBinding[] _result = new MethodBinding[0];
			
			public MethodBinding[] GetMethods(Type type)
			{
				return _result;
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly IPropertyProvider PropertyProvider = new NullPropertyProvider();

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly IMethodProvider MethodProvider = new NullMethodProvider();
	}
}
