using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class TableCollection : BindingCollection<TableBinding>
	{
		private DataContext _dataContext;

		internal TableCollection(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		protected override void ClearItems()
		{
			base.ClearItems();

			// Remove all relations
			_dataContext.TableRelations.Clear();
		}

		protected override void BeforeInsert(TableBinding binding)
		{
		    base.BeforeInsert(binding);
		    
			if (binding.Columns == null || binding.Columns.Count == 0)
				throw ExceptionBuilder.TableMustHaveAtLeastOneColumn("binding");
		}

		protected override void AfterRemove(TableBinding binding)
		{
			// Remove all relations referring to the removed table binding.

			foreach (TableRelation tableRelation in _dataContext.TableRelations.GetRelations(binding))
				_dataContext.TableRelations.Remove(tableRelation);
		}

		private IPropertyProvider GetRequiredPropertyProvider(Type type)
		{
			IPropertyProvider propertyProvider = _dataContext.MetadataContext.PropertyProviders[type];

			if (propertyProvider != null)
				return propertyProvider;

			if (_dataContext.MetadataContext.PropertyProviders.DefaultValue == null)
				throw ExceptionBuilder.NoPropertyProviderRegisteredAndDefaultProviderIsMissing(type);

			return _dataContext.MetadataContext.PropertyProviders.DefaultValue;
		}

		public TableBinding Add(DataTable dataTable)
		{
            if (dataTable == null)
                throw ExceptionBuilder.ArgumentNull("dataTable");

            DataTableBinding tableBinding = new DataTableBinding(dataTable);
            Add(tableBinding);
            return tableBinding;
		}

	    public TableBinding Add(IEnumerable enumerable, Type elementType, string tableName)
	    {
            if (enumerable == null)
                throw ExceptionBuilder.ArgumentNull("enumerable");

            if (elementType == null)
                throw ExceptionBuilder.ArgumentNull("elementType");
	        
            if (tableName == null)
                throw ExceptionBuilder.ArgumentNull("tableName");

			IPropertyProvider elementPropertyProvider = GetRequiredPropertyProvider(elementType);

            PropertyBinding[] properties;
            try
            {
                properties = elementPropertyProvider.GetProperties(elementType);
            }
            catch (NQueryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ExceptionBuilder.IPropertyProviderGetPropertiesFailed(ex);
            }

            EnumerableTableBinding tableBinding = new EnumerableTableBinding(enumerable, elementType, properties, tableName);
            Add(tableBinding);
            return tableBinding;
	    }

    	public TableBinding Add<T>(IEnumerable<T> enumerable, string tableName)
		{
            if (enumerable == null)
                throw ExceptionBuilder.ArgumentNull("enumerable");

            if (tableName == null)
                throw ExceptionBuilder.ArgumentNull("tableName");

            Type elementType = typeof(T);
			return Add(enumerable, elementType, tableName);
		}

	    public void AddRange(DataTableCollection dataTables)
		{
            if (dataTables == null)
                throw ExceptionBuilder.ArgumentNull("dataTables");

            foreach (DataTable dataTable in dataTables)
                Add(dataTable);
        }
	}
}