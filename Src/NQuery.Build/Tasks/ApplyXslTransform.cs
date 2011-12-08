using System;
using System.IO;
using System.Xml.XPath;
using System.Xml.Xsl;

using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace NQuery.Build
{
	public class ApplyXslTransform : Task
	{
		private ITaskItem _xslFile;
		private ITaskItem[] _sourceFiles;
		private ITaskItem[] _destinationFiles;

		[Required]
		public ITaskItem XslFile
		{
			get { return _xslFile; }
			set { _xslFile = value; }
		}

		[Required]
		public ITaskItem[] SourceFiles
		{
			get { return _sourceFiles; }
			set { _sourceFiles = value; }
		}

		[Required]
		public ITaskItem[] DestinationFiles
		{
			get { return _destinationFiles; }
			set { _destinationFiles = value; }
		}

		public override bool Execute()
		{
			XslCompiledTransform transform = new XslCompiledTransform();
			transform.Load(_xslFile.ItemSpec);

			if (_sourceFiles.Length != _destinationFiles.Length)
			{
				Log.LogError("Source files has an item count of {0} while destination files has an item count of {1}. Both must have same count.", _sourceFiles.Length, _destinationFiles.Length);
			}
			else
			{
				string tranformName = Path.GetFileNameWithoutExtension(_xslFile.ItemSpec);

				for (int i = 0; i < _sourceFiles.Length; i++)
				{
					ITaskItem sourceFile = _sourceFiles[i];
					ITaskItem destinationFile = _destinationFiles[i];

					string directory = Path.GetDirectoryName(destinationFile.ItemSpec);
					if (!Directory.Exists(directory))
					{
						MakeDir makeDir = new MakeDir();
						makeDir.BuildEngine = BuildEngine;
						makeDir.Directories = new ITaskItem[] {new TaskItem(directory)};
						makeDir.Execute();
					}

					try
					{
						using (FileStream outputStream = new FileStream(destinationFile.ItemSpec, FileMode.Create, FileAccess.Write))
						{
							XPathDocument xPathDocument = new XPathDocument(sourceFile.ItemSpec);
							transform.Transform(xPathDocument, null, outputStream);
							Log.LogMessage(MessageImportance.Normal, "Applied transform {0} and stored result as {1}", tranformName, destinationFile.ItemSpec);
						}
					}
					catch (Exception ex)
					{
						Log.LogError("Cannot apply transformation on file {0}: {1}", sourceFile.ItemSpec, ex.Message);
					}
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
