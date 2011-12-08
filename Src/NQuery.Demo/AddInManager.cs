using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using NQuery.Demo.AddIns;

namespace NQuery.Demo
{
	internal sealed class AddInDefinition
	{
		private Type _addInType;
		private IAddIn _instance;
		private string _error;

		public AddInDefinition(Type addInType, IAddIn instance, string error)
		{
			_addInType = addInType;
			_instance = instance;
			_error = error;
		}

		public Type AddInType
		{
			get { return _addInType; }
		}

		public IAddIn Instance
		{
			get { return _instance; }
		}

		public string Error
		{
			get { return _error; }
		}

		public bool HasErrors
		{
			get { return _addInType == null || _error != null; }
		}
	}

	internal sealed class AddInDefinitionCollection : ReadOnlyCollection<AddInDefinition>
	{
		public AddInDefinitionCollection(IList<AddInDefinition> list) : base(list)
		{
		}
	}

	internal static class AddInManager
	{
		private static AddInDefinitionCollection _addInDefinitionCollection;

		private static string GetAddInDirectory()
		{
			return Path.Combine(Application.StartupPath, "AddIns");
		}

		private static List<AddInDefinition> LoadAddInDefinitions()
		{
			string addInPath = GetAddInDirectory();
			if (!Directory.Exists(addInPath))
				return new List<AddInDefinition>();

			string[] addinAssemblyFilenames = Directory.GetFiles(addInPath, "*.addin", SearchOption.AllDirectories);

			List<AddInDefinition> result = new List<AddInDefinition>();

			List<Type> addInTypes = new List<Type>();
			foreach (string addinAssemblyFilename in addinAssemblyFilenames)
			{
				Assembly addInAssembly = Assembly.LoadFrom(addinAssemblyFilename);
				Type[] exportedTypes = addInAssembly.GetExportedTypes();
				foreach (Type exportedType in exportedTypes)
				{
					if (!exportedType.IsAbstract && !exportedType.IsInterface)
					{
						Type[] interfaces = exportedType.GetInterfaces();
						bool implementsIAddIn = Array.IndexOf(interfaces, typeof (IAddIn)) >= 0;
						if (implementsIAddIn)
						{
							ConstructorInfo defaultConstructor = exportedType.GetConstructor(new Type[0]);
							if (defaultConstructor != null)
							{
								addInTypes.Add(exportedType);
							}
							else
							{
								string error = String.Format("Add-in type '{0}' does not provide a public default constructor.", exportedType.FullName);
								AddInDefinition addInDefinition = new AddInDefinition(exportedType, null, error);
								result.Add(addInDefinition);
							}
						}
					}
				}
			}

			foreach (Type addInType in addInTypes)
			{
				AddInDefinition addInDefinition;
				try
				{
					IAddIn addIn = (IAddIn)Activator.CreateInstance(addInType);
					addInDefinition = new AddInDefinition(addInType, addIn, null);
				}
				catch (Exception ex)
				{
					string error = String.Format("Cannot create instance of add-in type '{0}': {1}", addInType.FullName, ex);
					addInDefinition = new AddInDefinition(addInType, null, error);
				}

				result.Add(addInDefinition);
			}

			return result;
		}


		public static AddInDefinitionCollection AddInDefinitions
		{
			get
			{
				if (_addInDefinitionCollection == null)
				{
					List<AddInDefinition> addInDefinitions = LoadAddInDefinitions();
					_addInDefinitionCollection = new AddInDefinitionCollection(addInDefinitions);
				}

				return _addInDefinitionCollection;
			}
		}
	}
}
