using System;
using System.Collections;

namespace NQuery.Samples.CustomComparer
{
	#region ExternalPersonDataType

	public class ExternalPersonDataType
	{
		private int _id;
		private string _firstName;
		private string _lastName;

		public ExternalPersonDataType(int id, string firstName, string lastName)
		{
			_id = id;
			_firstName = firstName;
			_lastName = lastName;
		}

		public int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		public string FirstName
		{
			get { return _firstName; }
			set { _firstName = value; }
		}

		public string LastName
		{
			get { return _lastName; }
			set { _lastName = value; }
		}
	}

	#endregion

	#region MyComparer

	public class MyComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			ExternalPersonDataType left = x as ExternalPersonDataType;
			ExternalPersonDataType right = y as ExternalPersonDataType;

			if (left == null && right == null)
				return 0;

			if (left == null)
				return -1;

			if (right == null)
				return +1;

			return left.Id.CompareTo(right.Id);
		}
	}

	#endregion
}
