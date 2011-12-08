using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NQuery
{
	/// <summary>
	/// Represents a property of a <see cref="ShowPlanElement"/>.
	/// </summary>
	public sealed class ShowPlanProperty
	{
		private string _fullName;
		private StringCollection _pathElements;
		private string _value;

		private class StringCollection : ReadOnlyCollection<string>
		{
			public StringCollection(IList<string> list)
				: base(list)
			{
			}
		}

		internal ShowPlanProperty(string fullPath, string value)
		{
			_fullName = fullPath;
			_pathElements = SplitFullPath(fullPath);
			_value = value;
		}

		private static StringCollection SplitFullPath(string fullPath)
		{
			// We cannot use String.Split('.') here. The problem is that fullPath might look like this:
			//
			//		ijkl.mno..pqr.stuv
			// 
			// Two consecutive dots should be treated a single dot inside a property name, i.e. it does
			// not cause to split (so a double dot is in fact a masked single dot).

			List<string> pathElements = new List<string>();
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < fullPath.Length; i++)
			{
				if (fullPath[i] == '.')
				{
					int j = i + 1;

					if (j >= fullPath.Length)
					{
						// We found a dot, but it is the last character.
						// Just ignore it.
						break;
					}

					if (fullPath[j] == '.')
					{
						// We found to consecutive dots, i.e. as maked dot.
						// Eat the first one and fall through so that the
						// second one added to the string builder.
					}
					else
					{
						// We found a single dot. The string builder now contains
						// the unescaped version of a path element. Add it to the 
						// list of path elements and clear the string builder.
						string currentElement = sb.ToString();
						pathElements.Add(currentElement);
						sb.Length = 0;
					}

					// Skip the dot.
					i++;
				}

				sb.Append(fullPath[i]);
			}

			// The content of the string builder contains a remaining part (it cannot be empty 
			// since this would require a string like 'abc.xyz.' but the trailing dot is ignored in 
			// the logic above).
			pathElements.Add(sb.ToString());

			return new StringCollection(pathElements);
		}

		/// <summary>
		/// Gets the full name of this property. Properties have a hierarchical structure. The hierarchy is given
		/// by <c>"."</c> characters. Double dots (<c>".."</c>) indicate a masked dot; in this case the dot does not
		/// indicate the start of a new level.
		/// </summary>
		/// <remarks>
		/// For easier access you might want to use <see cref="PathElements"/> which contains a correctly splitted
		/// collection of all levels.
		/// </remarks>
		public string FullName
		{
			get { return _fullName; }
		}

		/// <summary>
		/// Gets the simple name of this property i.e. the last element in <see cref="PathElements"/> and right most
		/// level in <see cref="FullName"/>.
		/// </summary>
		public string Name
		{
			get { return _pathElements[_pathElements.Count - 1]; }
		}

		/// <summary>
		/// Gets a collection of all levels of the <see cref="FullName"/> of this <see cref="ShowPlanProperty"/>.
		/// </summary>
		public ICollection<string> PathElements
		{
			get { return _pathElements; }
		}

		/// <summary>
		/// Gets the value of this <see cref="ShowPlanProperty"/>.
		/// </summary>
		public string Value
		{
			get { return _value; }
		}
	}
}