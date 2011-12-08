using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Runtime
{
	public class PropertyBindingCollection : ReadOnlyCollection<PropertyBinding>
	{
		public PropertyBindingCollection(IList<PropertyBinding> list)
			: base(list)
		{
		}
	}
}