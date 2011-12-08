using System;

namespace NQuery.Runtime
{
	public interface IAggregator
	{
		void Init();
		void Accumulate(object value);
		object Terminate();
		Type ReturnType { get; }
	}
}