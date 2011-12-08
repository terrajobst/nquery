using System;

namespace NQuery.Demo.AddIns
{
	public interface IAddIn
	{
		QueryContext CreateQueryContext();
		string Name { get; }
	}
}
