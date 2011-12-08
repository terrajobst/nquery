using System;
using System.ComponentModel;
using System.Data;

namespace NQuery
{
	public class DataContext : Component
	{
		private MetadataContext _metadataContext;
		private TableCollection _tables;
		private TableRelationCollection _tableRelations;
		private ConstantCollection _constants;
		private AggregateCollection _aggregates;
		private FunctionCollection _functions;
			
		public DataContext()
			: this(new MetadataContext())
		{
		}

		public DataContext(MetadataContext metadataContext)
		{
			_metadataContext = metadataContext;

			_tables = new TableCollection(this);
			_tables.Changed += member_Changed;

			_tableRelations = new TableRelationCollection(this);
			_tableRelations.Changed += member_Changed;

			_constants = new ConstantCollection();
			_constants.Changed += member_Changed;

			_aggregates = new AggregateCollection();
			_aggregates.AddDefaults();
			_aggregates.Changed += member_Changed;

			_functions = new FunctionCollection();
			_functions.AddDefaults();
			_functions.Changed += member_Changed;			
		}

		protected virtual void OnChange(EventArgs args)
		{
			EventHandler<EventArgs> changedHandler = Changed;

			if (changedHandler != null)
				changedHandler.Invoke(this, args);
		}

		protected virtual void OnMetadataContextChanged(EventArgs args)
		{
			EventHandler<EventArgs> changedHandler = MetadataContextChanged;

			if (changedHandler != null)
				changedHandler.Invoke(this, args);
		}

		private void member_Changed(object sender, EventArgs e)
		{
			OnChange(EventArgs.Empty);
		}

		public MetadataContext MetadataContext
		{
			get { return _metadataContext; }
			set
			{
				_metadataContext = value;
				OnMetadataContextChanged(EventArgs.Empty);
			}
		}

		public void AddTablesAndRelations(DataSet dataSet)
		{
			if (dataSet == null)
				throw ExceptionBuilder.ArgumentNull("dataSet");
			
			_tables.AddRange(dataSet.Tables);
			_tableRelations.AddRange(dataSet.Relations);
		}

		public TableCollection Tables
		{
			get { return _tables; }
		}

		public TableRelationCollection TableRelations
		{
			get { return _tableRelations; }
		}

		public ConstantCollection Constants
		{
			get { return _constants; }
		}

		public AggregateCollection Aggregates
		{
			get { return _aggregates; }
		}

		public FunctionCollection Functions
		{
			get { return _functions; }
		}

		public event EventHandler<EventArgs> Changed;
		public event EventHandler<EventArgs> MetadataContextChanged;
	}
}