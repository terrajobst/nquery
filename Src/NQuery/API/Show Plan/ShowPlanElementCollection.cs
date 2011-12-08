using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery
{
	/// <summary>
	/// Represents a read-only collection of <see cref="ShowPlanElement"/>.
	/// </summary>
	public class ShowPlanElementCollection : ReadOnlyCollection<ShowPlanElement>
	{
		public ShowPlanElementCollection(IList<ShowPlanElement> list) : base(list)
		{
		}
	}
}