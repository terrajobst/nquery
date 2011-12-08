using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

using NQuery.Runtime;

namespace NQuery
{
	public class MetadataContext
	{
		private TypeRegistry<IPropertyProvider> _propertyProviders = new TypeRegistry<IPropertyProvider>();
		private TypeRegistry<IMethodProvider> _methodProviders = new TypeRegistry<IMethodProvider>();
		private TypeRegistry<IComparer> _comparers = new TypeRegistry<IComparer>();

		public MetadataContext()
		{
			ReflectionProvider reflectionProvider = new ReflectionProvider();
			
			_propertyProviders.DefaultValue = reflectionProvider;
			_propertyProviders.Changed += TypeRegistryChanged;

			_methodProviders.DefaultValue = reflectionProvider;
			_methodProviders.Register(typeof(DataRow), NullProviders.MethodProvider);
			_methodProviders.Register(typeof(DataTable), NullProviders.MethodProvider);
			_methodProviders.Register(typeof(Hashtable), NullProviders.MethodProvider);
			_methodProviders.Changed += TypeRegistryChanged;

			_comparers.Changed += TypeRegistryChanged;
		}

		private void OnChanged()
		{
			EventHandler<EventArgs> handler = Changed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		private void TypeRegistryChanged(object sender, EventArgs e)
		{
			OnChanged();
		}

		#region Property Providers

		public TypeRegistry<IPropertyProvider> PropertyProviders
		{
			get { return _propertyProviders; }
		}

		/// <summary>
		/// Returns all properties matching the identifier.
		/// </summary>
		/// <param name="type">The type to search the properties in.</param>
		/// <param name="identifier">The identifier to match the properties.</param>
		public PropertyBinding[] FindProperty(Type type, Identifier identifier)
		{
			if (type == null)
				throw ExceptionBuilder.ArgumentNull("type");

			if (identifier == null)
				throw ExceptionBuilder.ArgumentNull("identifier");

			// Get property provider responsible for the given type.

			IPropertyProvider propertyProvider = _propertyProviders[type];

			if (propertyProvider == null)
				return new PropertyBinding[0];

			// Get properties from the provider.

			PropertyBinding[] properties;

			try
			{
				properties = propertyProvider.GetProperties(type);
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.IPropertyProviderGetPropertiesFailed(ex);
			}

			return FindProperty(properties, identifier);
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public PropertyBinding[] FindProperty(IList<PropertyBinding> properties, Identifier identifier)
		{
			if (properties == null)
				throw ExceptionBuilder.ArgumentNull("properties");

			if (identifier == null)
				throw ExceptionBuilder.ArgumentNull("identifier");

			// Return all properties that match the given name.

			List<PropertyBinding> candidateList = new List<PropertyBinding>();

			foreach (PropertyBinding propertyBinding in properties)
			{
				if (identifier.Matches(propertyBinding.Name))
					candidateList.Add(propertyBinding);
			}

			return candidateList.ToArray();
		}

		#endregion

		#region Method Providers

		public TypeRegistry<IMethodProvider> MethodProviders
		{
			get { return _methodProviders; }
		}

		/// <summary>
		/// Returns all methods matching the identifier.
		/// </summary>
		/// <param name="type">The type to search the methods in.</param>
		/// <param name="identifier">The identifier to match the methods.</param>
		public MethodBinding[] FindMethod(Type type, Identifier identifier)
		{
			if (type == null)
				throw ExceptionBuilder.ArgumentNull("type");

			if (identifier == null)
				throw ExceptionBuilder.ArgumentNull("identifier");

			// Get method provider responsible for the given type.

			IMethodProvider methodProvider = _methodProviders[type];

			if (methodProvider == null)
				return new MethodBinding[0];

			// Get properties from the provider.

			MethodBinding[] methods;

			try
			{
				methods = methodProvider.GetMethods(type);
			}
			catch (NQueryException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw ExceptionBuilder.IMethodProviderGetMethodsFailed(ex);
			}

			// Return all methods that match the given name.

			List<MethodBinding> result = new List<MethodBinding>();

			foreach (MethodBinding methodBinding in methods)
			{
				if (identifier.Matches(methodBinding.Name))
					result.Add(methodBinding);
			}

			return result.ToArray();
		}

		#endregion

		#region Comparers

		public TypeRegistry<IComparer> Comparers
		{
			get { return _comparers; }
		}

		#endregion

		public event EventHandler<EventArgs> Changed;
	}
}