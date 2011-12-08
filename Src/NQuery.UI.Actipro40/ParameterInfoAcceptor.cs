using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using ActiproSoftware.SyntaxEditor;

using NQuery;
using NQuery.Runtime;

namespace NQuery.UI
{
	internal sealed class ParameterInfoAcceptor : IParameterInfoAcceptor
	{
		private IntelliPromptParameterInfo _infoTip;
		private int _parameterIndex;

		public ParameterInfoAcceptor(IntelliPromptParameterInfo infoTip, int parameterIndex)
		{
			_infoTip = infoTip;
			_parameterIndex = parameterIndex;
		}
			
		public void AcceptFunction(FunctionBinding function)
		{
			AddInvocable(function);
		}

		public void AcceptMethod(MethodBinding method)
		{
            AddInvocable(method);
		}

		public void AcceptAggregate(AggregateBinding aggregate)
		{
			AddAggregate(aggregate);
		}
			
		private void AddInvocable(InvocableBinding invocableBinding)
		{
			InvokeParameter[] parameters = invocableBinding.GetParameters();

			// Note: Don't use <= otherwise we will never see an IntelliPromt for methods/functions
			//       with no parameters.
			
			if (parameters.Length < _parameterIndex)
				return;

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb, CultureInfo.CurrentCulture))
			{
				XmlWriter writer = new XmlTextWriter(sw);

				if (invocableBinding.ReturnType != null)
				{
					writer.WriteString(invocableBinding.ReturnType.Name);
					writer.WriteWhitespace(" ");
				}

				MethodBinding methodBinding = invocableBinding as MethodBinding;
				if (methodBinding != null)
				{
					writer.WriteString(methodBinding.DeclaringType.Name);
					writer.WriteString(".");
				}
				
				writer.WriteString(invocableBinding.Name);
				
				bool highlighParentheses = (_parameterIndex == 0 && parameters.Length == 0);
				
				if (highlighParentheses)
					writer.WriteStartElement("b");					
				writer.WriteString(" (");
				if (highlighParentheses)
					writer.WriteEndElement();
				
				for (int i = 0; i < parameters.Length; i++)
				{
					if (i > 0)
						writer.WriteString(", ");

					bool isSelectedParameter = (i == _parameterIndex);

					if (isSelectedParameter)
						writer.WriteStartElement("b");

					writer.WriteString(parameters[i].Name);
					writer.WriteString(": ");
					writer.WriteString(parameters[i].DataType.Name);

					if (isSelectedParameter)
						writer.WriteEndElement();
				}

				if (highlighParentheses)
					writer.WriteStartElement("b");
				writer.WriteString(")");
				if (highlighParentheses)
					writer.WriteEndElement();
			}

			_infoTip.Info.Add(sb.ToString());
		}

		private void AddAggregate(AggregateBinding aggregateBinding)
		{
			if (_parameterIndex > 1)
				return;

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb, CultureInfo.CurrentCulture))
			{
				XmlWriter writer = new XmlTextWriter(sw);

				writer.WriteString("AGGREGATE ");				
				writer.WriteString(aggregateBinding.Name);
				writer.WriteString(" (");
				writer.WriteStartElement("b");					
				writer.WriteString("value");
				writer.WriteEndElement();
				writer.WriteString(")");
			}

			_infoTip.Info.Add(sb.ToString());
		}
	}
}