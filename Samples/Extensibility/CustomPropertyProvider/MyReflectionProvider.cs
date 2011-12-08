using System;
using System.Reflection;

using NQuery.Runtime;

namespace NQuery.Samples.CustomPropertyProvider
{
	#region MyDataType1

	/// <summary>
	/// This is a custom data type which has marker attributes placed on all properties 
	/// that should be exposed. Properties that don't have this marker attribute - even
	/// if they are public - should not be exposed.
	/// </summary>
	public class MyDataType1
	{
		private int _intValue;
		private string _stringValue;
		private DateTime _dataTimeValue;

		public MyDataType1(int intValue, string stringValue, DateTime dataTimeValue)
		{
			_intValue = intValue;
			_stringValue = stringValue;
			_dataTimeValue = dataTimeValue;
		}

		[MyMarker(Name = "FirstProperty")]
		public int IntValue
		{
			get { return _intValue; }
			set { _intValue = value; }
		}

		[MyMarker(Name = "SecondProperty")]
		public string StringValue
		{
			get { return _stringValue; }
			set { _stringValue = value; }
		}

		[MyMarker(Name = "ThirdProperty")]
		public DateTime DataTimeValue
		{
			get { return _dataTimeValue; }
			set { _dataTimeValue = value; }
		}

		/// <summary>
		/// This attribute is not exposed.
		/// </summary>
		public object NonMarkedProperty
		{
			get { return 42; }
		}
	}

	#endregion

	#region MyMarkerAttribute
	
	/// <summary>
	/// This is a marker attribute used to mark all fields, properties, and 
	/// methods that should be accessible inside of queries and expressions.
	/// In addition it allows to specify the exposed name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | 
	                AttributeTargets.Property | 
	                AttributeTargets.Field)]
	public sealed class MyMarkerAttribute : Attribute
	{
		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
	}

	#endregion

	#region MyReflectionProvider

	public class MyReflectionProvider : ReflectionProvider
	{
		protected override PropertyBinding CreateProperty(PropertyInfo propertyInfo)
		{
			// Get custom attributes of the property.
			object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(MyMarkerAttribute), false);

			if (customAttributes.Length != 1)
			{
				// Does not have MyMarkerAttribute attribute. Return null
				// to indicate we want to skip this property.
				return null;
			}

			MyMarkerAttribute myMarkerAttribute = (MyMarkerAttribute)customAttributes[0];

			// Generate a name for this property. If no name was specified in the marker
			// attribute we want to use the source code name.
			string name;
			if (myMarkerAttribute.Name == null)
				name = propertyInfo.Name;
			else
				name = myMarkerAttribute.Name;

			// Create a new ReflectionPropertyBinding. This is a built-in property
			// binding that simply uses reflection to evaluate its value.
			return new ReflectionPropertyBinding(propertyInfo, name);
		}
	}

	#endregion
}
