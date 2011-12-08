using System;
using System.Collections;
using System.Text;

namespace NQuery.Runtime
{
	public sealed class ConcatAggregator : IAggregator
	{
		private SortedList _valueList = new SortedList();

		public void Init()
		{
			_valueList.Clear();
		}

		public void Accumulate(object value)
		{
			if (value == null)
				return;

			string strValue = value.ToString().Trim();

			if (_valueList.ContainsKey(strValue))
				return;

			_valueList.Add(strValue, null);
		}

		public object Terminate()
		{
			StringBuilder sb = new StringBuilder(_valueList.Count * 8);

			foreach (string value in _valueList.Keys)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append(value);
			}

			return sb.ToString();
		}

		public Type ReturnType
		{
			get { return typeof(string); }
		}
	}
}