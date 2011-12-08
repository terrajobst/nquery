using System;
using System.Reflection;

using NQuery.Runtime;

namespace NQuery.Samples.CustomMethodProvider
{
	#region MyDataType

	/// <summary>
	/// This is a custom data type which has marker attributes placed on all methods 
	/// that should be exposed. Methods that don't have this marker attribute - even
	/// if they are public - should not be exposed.
	/// </summary>
	public class MyDataType
	{
		private int _intValue;
		private string _stringValue;

		public MyDataType(int intValue, string stringValue)
		{
			_intValue = intValue;
			_stringValue = stringValue;
		}

		public int IntValue
		{
			get { return _intValue; }
			set { _intValue = value; }
		}

		public string StringValue
		{
			get { return _stringValue; }
			set { _stringValue = value; }
		}

		[MyMarker]
		public int GetIntValue()
		{
			return _intValue;			
		}

		[MyMarker]
		public int GetStringValue()
		{
			return _intValue;
		}

		[MyMarker(Name="DO_IT")]
		public string MultiplyAndConcat(int factor)
		{
			return (_intValue * factor) + " - " + _stringValue;
		}

		/// <summary>
		/// This method is not exposed.
		/// </summary>
		public override string ToString()
		{
			return MultiplyAndConcat(1);
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
		protected override MethodBinding CreateMethod(MethodInfo methodInfo)
		{
			// Get custom attributes of the method.
			object[] customAttributes = methodInfo.GetCustomAttributes(typeof(MyMarkerAttribute), false);

			if (customAttributes.Length != 1)
			{
				// Does not have MyMarkerAttribute attribute. Return null
				// to indicate we want to skip this method.
				return null;
			}

			MyMarkerAttribute myMarkerAttribute = (MyMarkerAttribute)customAttributes[0];

			// Generate a name for this method. If no name was specified in the marker
			// attribute we want to use the source code name.
			string name;
			if (myMarkerAttribute.Name == null)
				name = methodInfo.Name;
			else
				name = myMarkerAttribute.Name;

			// Create a new ReflectionMethodBinding. This is a built-in method
			// binding that simply uses reflection to evaluate its value.
			return new ReflectionMethodBinding(methodInfo, name);
		}
	}

	#endregion
}
