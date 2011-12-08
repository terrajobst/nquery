using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace NQuery
{
	/// <summary>
	/// Represents an element in an execution plan of <see cref="Query"/>.
	/// </summary>
	public sealed class ShowPlanElement : ICustomTypeDescriptor
	{
		private ShowPlanOperator _operator;
		private ShowPlanPropertyCollection _properties;
		private ShowPlanElementCollection _children;
		private PathElement _root = new PathElement();

		internal ShowPlanElement(ShowPlanOperator op, IList<ShowPlanProperty> properties, params ShowPlanElement[] children)
			: this(op, properties, (IList<ShowPlanElement>)children)
		{
		}

		internal ShowPlanElement(ShowPlanOperator op, IList<ShowPlanProperty> properties, IList<ShowPlanElement> children)
		{
			_operator = op;
			_properties = new ShowPlanPropertyCollection(properties);
			_children = new ShowPlanElementCollection(children);
			BuildPathElementRoot();
		}

		/// <summary>
		/// Gets the physical operator represented by this node.
		/// </summary>
		public ShowPlanOperator Operator
		{
			get { return _operator; }
		}

		/// <summary>
		/// Gets a collection of properties representing the configuration of this node.
		/// </summary>
		public ShowPlanPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets a collection with all children of this execution plan node.
		/// </summary>
		public ShowPlanElementCollection Children
		{
			get { return _children; }
		}

		#region Property Grid Magic

		private void BuildPathElementRoot()
		{
			Dictionary<string, PathElement> propertyDictionary = new Dictionary<string, PathElement>();

			foreach (ShowPlanProperty planProperty in _properties)
			{
				string currentPath = null;
				PathElement property = _root;

				foreach (string pathPart in planProperty.PathElements)
				{
					if (currentPath == null)
						currentPath = pathPart;
					else
						currentPath = currentPath + "." + pathPart;

					PathElement currentProperty;
					if (!propertyDictionary.TryGetValue(currentPath, out currentProperty))
					{
						currentProperty = new PathElement();
						currentProperty.Name = pathPart;
						property.Children.Add(currentProperty);
						propertyDictionary.Add(currentPath, currentProperty);
					}
					property = currentProperty;
				}

				property.Value = planProperty.Value;
			}
		}

		private class PathElementPropertyDescriptor : PropertyDescriptor
		{
			private PathElement _pathElement;

			public PathElementPropertyDescriptor(PathElement pathElement)
				: base(pathElement.Name, new Attribute[0])
			{
				_pathElement = pathElement;
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				if (_pathElement.Children.Count == 0)
					return _pathElement.Value;

				return _pathElement;
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}

			public override Type ComponentType
			{
				get { return typeof(PathElement); }
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}

			public override Type PropertyType
			{
				get
				{
					if (_pathElement.Children.Count == 0)
						return typeof (string);

					return typeof (PathElement);
				}
			}
		}

		private class PathElementTypeConverter : ExpandableObjectConverter
		{
			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
			                                 Type destinationType)
			{
				if (destinationType == typeof(string))
				{
					PathElement propertyTable = (PathElement) value;
					return propertyTable.Value;
				}

				return base.ConvertTo(context, culture, value, destinationType);
			}
		}

		[TypeConverter(typeof(PathElementTypeConverter))]
		private class PathElement : ICustomTypeDescriptor
		{
			public string Name;
			public string Value;
			public List<PathElement> Children = new List<PathElement>();

			#region ICustomTypeDescriptor Implementation

			public AttributeCollection GetAttributes()
			{
				return TypeDescriptor.GetAttributes(this, true);
			}

			public string GetClassName()
			{
				return TypeDescriptor.GetClassName(this, true);
			}

			public string GetComponentName()
			{
				return TypeDescriptor.GetComponentName(this, true);
			}

			public TypeConverter GetConverter()
			{
				return TypeDescriptor.GetConverter(this, true);
			}

			public EventDescriptor GetDefaultEvent()
			{
				return TypeDescriptor.GetDefaultEvent(this, true);
			}

			public PropertyDescriptor GetDefaultProperty()
			{
				return null;
			}

			public object GetEditor(Type editorBaseType)
			{
				return TypeDescriptor.GetEditor(this, editorBaseType, true);
			}

			public EventDescriptorCollection GetEvents()
			{
				return TypeDescriptor.GetEvents(this, true);
			}

			public EventDescriptorCollection GetEvents(Attribute[] attributes)
			{
				return TypeDescriptor.GetEvents(this, attributes, true);
			}

			public PropertyDescriptorCollection GetProperties()
			{
				return GetProperties(new Attribute[0]);
			}

			public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
			{
				List<PathElementPropertyDescriptor> customProperties = new List<PathElementPropertyDescriptor>();
				foreach (PathElement child in Children)
				{
					PathElementPropertyDescriptor showPlanPropertyDescriptor = new PathElementPropertyDescriptor(child);
					customProperties.Add(showPlanPropertyDescriptor);
				}

				return new PropertyDescriptorCollection(customProperties.ToArray());
			}

			public object GetPropertyOwner(PropertyDescriptor pd)
			{
				return this;
			}

			#endregion
		}

		#region ICustomTypeDescriptor Implementation

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return _root.GetAttributes();
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return Resources.ShowPlanElemenClassName;
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return _operator.ToString();
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return _root.GetConverter();
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return _root.GetDefaultEvent();
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return _root.GetDefaultProperty();
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return _root.GetEditor(editorBaseType);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return _root.GetEvents();
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return _root.GetEvents(attributes);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return _root.GetProperties();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return _root.GetProperties(attributes);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return _root.GetPropertyOwner(pd);
		}

		#endregion

		#endregion
	}
}
