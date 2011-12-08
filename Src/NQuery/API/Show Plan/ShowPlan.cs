using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using NQuery.Compilation;

namespace NQuery
{
	/// <summary>
	/// Represents an execution plan of <see cref="Query"/>. Provides methods for writing to plain text and XML.
	/// </summary>
	public sealed class ShowPlan
	{
		private ShowPlanElement _root;
		
		private ShowPlan(ShowPlanElement root)
		{
			_root = root;
		}

        internal static ShowPlan Build(AlgebraNode root)
        {
            ShowPlanElement rootElement = ShowPlanBuilder.Convert(root);
            return new ShowPlan(rootElement);
        }
	    
        /// <summary>
		/// Gets the root element of this execution plan.
		/// </summary>
		public ShowPlanElement Root
		{
			get { return _root; }
		}
		
		/// <summary>
		/// Creates an XML document containing this execution plan.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		public XmlDocument ToXml()
		{
			XmlDocument document = new XmlDocument();
			XmlNode root = document.CreateElement("executionPlan");
			document.AppendChild(root);
			WriteTo(root, _root);
			return document;
		}

		/// <summary>
		/// Loads an execution plan from an XML document.
		/// </summary>
		/// <param name="navigable">An XPath navigable object to load the execution plan from</param>
		public static ShowPlan FromXml(IXPathNavigable navigable)
		{
			if (navigable == null)
				throw ExceptionBuilder.ArgumentNull("navigable");

			XPathNavigator navigator = navigable.CreateNavigator();
			ShowPlanElement root = ReadPlanElement(navigator.SelectSingleNode("executionPlan/element"));
			return new ShowPlan(root);
		}

		private static ShowPlanElement ReadPlanElement(XPathNavigator elementNavigator)
		{
			ShowPlanOperator op = (ShowPlanOperator) Enum.Parse(typeof(ShowPlanOperator), elementNavigator.GetAttribute("operator", String.Empty));

			List<ShowPlanProperty> properties = new List<ShowPlanProperty>();
			foreach (XPathNavigator propertyNavigator in elementNavigator.Select("properties/property"))
			{
				string propertyName = propertyNavigator.GetAttribute("name", String.Empty);
				string propertyValue = propertyNavigator.GetAttribute("value", String.Empty);
				ShowPlanProperty property = new ShowPlanProperty(propertyName, propertyValue);
				properties.Add(property);
			}

			List<ShowPlanElement> children = new List<ShowPlanElement>();
			foreach (XPathNavigator childNode in elementNavigator.Select("input/*"))
			{
				ShowPlanElement childElement = ReadPlanElement(childNode);
				children.Add(childElement);
			}

			ShowPlanElement element = new ShowPlanElement(op, properties, children);
			return element;
		}

		private static void WriteTo(XmlNode parent, ShowPlanElement element)
		{
			XmlNode elementNode = parent.OwnerDocument.CreateElement("element");
			parent.AppendChild(elementNode);

			XmlAttribute operatorAtt = parent.OwnerDocument.CreateAttribute("operator");
			operatorAtt.Value = element.Operator.ToString();
			elementNode.Attributes.Append(operatorAtt);

			XmlNode propertiesNode = parent.OwnerDocument.CreateElement("properties");
			elementNode.AppendChild(propertiesNode);

			foreach (ShowPlanProperty property in element.Properties)
			{
				XmlNode propertyNode = parent.OwnerDocument.CreateElement("property");
				propertiesNode.AppendChild(propertyNode);

				XmlAttribute propertyNameAtt = parent.OwnerDocument.CreateAttribute("name");
				propertyNameAtt.Value = property.FullName;
				propertyNode.Attributes.Append(propertyNameAtt);

				XmlAttribute propertyValueAtt = parent.OwnerDocument.CreateAttribute("value");
				propertyValueAtt.Value = property.Value;
				propertyNode.Attributes.Append(propertyValueAtt);
			}

			XmlNode inputNode = parent.OwnerDocument.CreateElement("input");
			elementNode.AppendChild(inputNode);

			foreach (ShowPlanElement child in element.Children)
			{
				WriteTo(inputNode, child);
			}
		}

		/// <summary>
		/// Writes this execution plan to the given text writer.
		/// </summary>
		/// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
		/// <param name="indent">The number of spaces used to indent the node levels.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="textWriter"/> is <see langword="null"/>.</exception>
		public void WriteTo(TextWriter textWriter, int indent)
		{
			if (textWriter == null)
				throw ExceptionBuilder.ArgumentNull("textWriter");

			IndentedTextWriter indentedTextWriter = new IndentedTextWriter(textWriter, new string(' ', indent));
			WriteTo(indentedTextWriter, _root);
		}

		private static void WriteTo(IndentedTextWriter textWriter, ShowPlanElement element)
		{
			textWriter.Write("[");
			textWriter.Write(element.Operator);
			textWriter.Write("]");
			textWriter.WriteLine();

			// Write properties
			
			foreach (ShowPlanProperty property in element.Properties)
			{
				textWriter.Write(property.FullName);
				textWriter.Write(" = ");
				textWriter.Write(property.Value);
				textWriter.WriteLine();
			}

			// Write Children

			textWriter.Indent++;
			foreach (ShowPlanElement child in element.Children)
				WriteTo(textWriter, child);
			textWriter.Indent--;
		}

		/// <summary>
		/// Creates a textual representation of this execution plan.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			{
				WriteTo(stringWriter, 2);
				return stringWriter.ToString();
			}
		}
	}
}