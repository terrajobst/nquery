using System;

namespace NQuery.Runtime
{
	public abstract class AggregateBinding : Binding 
	{
		private string _name;

		protected AggregateBinding(string name)
		{
			if (name == null)
				throw ExceptionBuilder.ArgumentNull("name");

			_name = name;
		}

		public override string Name
		{
			get { return _name; }
		}

		public sealed override BindingCategory Category
		{
			get { return BindingCategory.Aggregate; }
		}

		public abstract IAggregator CreateAggregator(Type inputType);
	}
}