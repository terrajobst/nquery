using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NQuery.Build
{
	public class RegexReplaceInFiles : Task
	{
		private ITaskItem[] _sourceFiles;
		private ITaskItem[] _destinationFiles;
		private string _pattern;
		private string _replacement;

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

		[Required]
		public string Pattern
		{
			get { return _pattern; }
			set { _pattern = value; }
		}

		[Required]
		public string Replacement
		{
			get { return _replacement; }
			set { _replacement = value; }
		}

		public override bool Execute()
		{
			if (_sourceFiles.Length != _destinationFiles.Length)
			{
				Log.LogError("Source files has an item count of {0} while destination files has an item count of {1}. Both must have same count.", _sourceFiles.Length, _destinationFiles.Length);
			}
			else
			{
				for (int i = 0; i < _sourceFiles.Length; i++)
				{
					ITaskItem sourceFile = _sourceFiles[i];
					ITaskItem destinationFile = _destinationFiles[i];

					try
					{
						string fileContent = File.ReadAllText(sourceFile.ItemSpec, Encoding.UTF8);
						fileContent = Regex.Replace(fileContent, _pattern, _replacement);
						File.WriteAllText(destinationFile.ItemSpec, fileContent, Encoding.UTF8);
					}
					catch (Exception ex)
					{
						Log.LogError("Cannot replace '{0}' to '{1}' in file {2}: {3}", _pattern, _replacement, sourceFile.ItemSpec, ex.Message);
					}
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
