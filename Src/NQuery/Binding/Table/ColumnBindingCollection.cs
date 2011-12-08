using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class ColumnBindingCollection : ReadOnlyCollection<ColumnBinding>
	{
		public ColumnBindingCollection(IList<ColumnBinding> list)
			: base(list)
		{
		}
	}
}