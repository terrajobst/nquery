using System;
using System.Collections;
using System.Text;

namespace NQuery.UI
{
	internal class ScopeInfo
	{
		private SortedList _reservedWords = new SortedList();
		private SortedList _aggregateNames = new SortedList();
		private SortedList _functionNames = new SortedList();

		public SortedList ReservedWords
		{
			get { return _reservedWords; }
		}

		public SortedList AggregateNames
		{
			get { return _aggregateNames; }
		}

		public SortedList FunctionNames
		{
			get { return _functionNames; }
		}

		public string GetKeywords()
		{
			return GetKeys(_reservedWords);
		}

	    public string GetFunctionsAndAggregates()
		{
			SortedList sortedList = new SortedList(_aggregateNames.Count + _functionNames.Count);
			
			foreach (string key in _aggregateNames.Keys)
			{
				if (!sortedList.ContainsKey(key))
					sortedList.Add(key, null);
			}

			foreach (string key in _functionNames.Keys)
			{
				if (!sortedList.ContainsKey(key))
					sortedList.Add(key, null);
			}
			
			return GetKeys(sortedList);
		}
		
		private static string GetKeys(SortedList sortedList)
		{
			StringBuilder sb = new StringBuilder();
				
			foreach (string key in sortedList.Keys)
			{
				if (sb.Length > 0)
					sb.Append(" ");
					
				sb.Append(key);
			}
				
			return sb.ToString();
		}
	}
}