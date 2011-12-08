using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery
{
	/// <summary>
	/// Represents a read-only collection of <see cref="ShowPlanProperty"/>.
	/// </summary>
	public sealed class ShowPlanPropertyCollection : ReadOnlyCollection<ShowPlanProperty>
	{
		public ShowPlanPropertyCollection(IList<ShowPlanProperty> list)
			: base(list)
		{
		}

		/// <summary>
		/// Gets the <see cref="ShowPlanProperty"/> with the given <see cref="ShowPlanProperty.FullName"/>.
		/// </summary>
		/// <param name="fullName">The <see cref="ShowPlanProperty.FullName"/> of a <see cref="ShowPlanProperty"/> to search for</param>
		/// <returns>The instance of the <see cref="ShowPlanProperty"/> with the given <see cref="ShowPlanProperty.FullName"/> or
		/// <see langword="null"/> if it is not conatained in this collection.
		/// </returns>
		public ShowPlanProperty this[string fullName]
		{
			get
			{
				foreach (ShowPlanProperty property in this)
				{
					if (String.Compare(property.FullName, fullName, StringComparison.OrdinalIgnoreCase) == 0)
						return property;
				}

				return null;
			}
		}

		/// <summary>
		/// Checks whether a <see cref="ShowPlanProperty"/> with the given <see cref="ShowPlanProperty.FullName"/> is
		/// contained in this collection.
		/// </summary>
		/// <param name="fullName">The <see cref="ShowPlanProperty.FullName"/> of a <see cref="ShowPlanProperty"/> to search for</param>
		/// <returns>If a <see cref="ShowPlanProperty"/> with the given <see cref="ShowPlanProperty.FullName"/> exists the return value 
		/// is <see langword="true"/>. Otherwise it is <see langword="false"/>.</returns>
		public bool Contains(string fullName)
		{
			return this[fullName] != null;
		}
	}
}