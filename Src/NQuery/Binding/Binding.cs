using System;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Runtime
{
	public abstract class Binding
	{
		protected Binding()
		{
		}

		public abstract string Name { get; }

		// Since inheritors are supposed to use string construction the caller
		// is reponsible for caching the result. We indicate this by using a
		// method instead of a property.
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public virtual string GetFullName()
		{
			return Name;
		}

		public override string ToString()
		{
			return GetFullName();
		}
		
		public abstract BindingCategory Category { get; }
	}
}