using System;
using System.Windows.Forms;

namespace NQuery.Demo
{
	public partial class EditParameterForm : Form
	{
		private Type _parameterType;
		private object _parameterValue;

		public EditParameterForm(bool isCreation, string parameterName, Type parameterType, object value)
		{
			InitializeComponent();

			nameLabel.Enabled = isCreation;
			nameTextBox.Enabled = isCreation;
			nameTextBox.Text = parameterName;
			if (value != null)
				valueTextBox.Text = value.ToString();

			typeLabel.Enabled = isCreation;
			typeComboBox.Enabled = isCreation;
			foreach (PrimitiveType primitiveType in Enum.GetValues(typeof(PrimitiveType)))
				typeComboBox.Items.Add(new TypeDefinition(primitiveType));
			foreach (TypeDefinition typeDefinition in typeComboBox.Items)
			{
				if (typeDefinition.Type == parameterType)
				{
					typeComboBox.SelectedItem = typeDefinition;
					break;
				}
			}
		}

		#region Helper Types

		private enum PrimitiveType
		{
			Boolean,
			Byte,
			DateTime,
			Decimal,
			Double,
			Guid,
			Int16,
			Int32,
			Int64,
			Single,
			String
		}

		private sealed class TypeDefinition
		{
			private PrimitiveType _primitiveType;

			public TypeDefinition(PrimitiveType primitiveType)
			{
				_primitiveType = primitiveType;
			}

			public Type Type
			{
				get
				{
					switch (_primitiveType)
					{
						case PrimitiveType.Boolean:
							return typeof(Boolean);
						case PrimitiveType.Byte:
							return typeof(Byte);
						case PrimitiveType.DateTime:
							return typeof(DateTime);
						case PrimitiveType.Decimal:
							return typeof(Decimal);
						case PrimitiveType.Double:
							return typeof(Double);
						case PrimitiveType.Guid:
							return typeof(Guid);
						case PrimitiveType.Int16:
							return typeof(Int16);
						case PrimitiveType.Int32:
							return typeof(Int32);
						case PrimitiveType.Int64:
							return typeof(Int64);
						case PrimitiveType.Single:
							return typeof(Single);
						case PrimitiveType.String:
							return typeof(String);
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			public object ConvertValue(string text)
			{
				if (String.IsNullOrEmpty(text))
					return null;

				switch (_primitiveType)
				{
					case PrimitiveType.Boolean:
						return Boolean.Parse(text);
					case PrimitiveType.Byte:
						return Byte.Parse(text);
					case PrimitiveType.DateTime:
						return DateTime.Parse(text);
					case PrimitiveType.Decimal:
						return Decimal.Parse(text);
					case PrimitiveType.Double:
						return Double.Parse(text);
					case PrimitiveType.Guid:
						return new Guid(text);
					case PrimitiveType.Int16:
						return Int16.Parse(text);
					case PrimitiveType.Int32:
						return Int32.Parse(text);
					case PrimitiveType.Int64:
						return Int64.Parse(text);
					case PrimitiveType.Single:
						return Single.Parse(text);
					case PrimitiveType.String:
						return text;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public override string ToString()
			{
				return Type.Name;
			}
		}

		#endregion

		public string ParameterName
		{
			get { return nameTextBox.Text; }
		}

		public Type ParameterType
		{
			get { return _parameterType; }
		}

		public object ParameterValue
		{
			get { return _parameterValue; }
		}

		private void EditParameterForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				TypeDefinition typeDefinition = (TypeDefinition) typeComboBox.SelectedItem;
				if (typeDefinition == null)
				{
					e.Cancel = true;
					return;
				}

				_parameterType = typeDefinition.Type;

				try
				{
					_parameterValue = typeDefinition.ConvertValue(valueTextBox.Text);
				}
				catch (Exception ex)
				{
					string msg = "The parameter value is invalid: " + ex.Message;
					MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					e.Cancel = true;
				}
			}
		}
	}
}