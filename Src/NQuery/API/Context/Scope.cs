using System;

namespace NQuery
{
	internal sealed class Scope
	{
		private DataContext _dataContext;
		private ParameterCollection _parameters;

		public Scope()
		{
			_parameters = new ParameterCollection();
		}

		public DataContext DataContext
		{
			get { return _dataContext; }
			set { _dataContext = value; }
		}

		public ParameterCollection Parameters
		{
			get { return _parameters; }
			set { _parameters = value; }
		}
	}
}