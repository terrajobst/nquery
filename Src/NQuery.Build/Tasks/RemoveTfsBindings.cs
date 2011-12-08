using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NQuery.Build
{
	public sealed class RemoveTfsBindings : Task
	{
		private ITaskItem[] _solutionFiles;

		#region Helpers

		public static void Remove(string solutionFileName)
		{
			CleanSolution(solutionFileName);
			string[] projectFileNames = GetProjectsFromSolution(solutionFileName);
			foreach (string projectFileName in projectFileNames)
				CleanProject(projectFileName);
		}

		private static string[] GetProjectsFromSolution(string solutionFilename)
		{
			List<string> projectList = new List<string>();
			string solutionDirectory = Path.GetDirectoryName(solutionFilename);

			Regex regex = new Regex(@"Project\(\""\{[A-Z|\d|\-|a-z]*\}\""\)\s*=\s*\""\S*\""\s*,\s*\""(?<FileName>[^\""]*)\""\s*,\s*",
									RegexOptions.IgnoreCase |
									RegexOptions.Multiline |
									RegexOptions.IgnorePatternWhitespace |
									RegexOptions.Compiled);

			using (StreamReader sr = new StreamReader(solutionFilename))
			{
				string line = sr.ReadLine();

				while (line != null)
				{
					if (line == "Global") //Projects definition were before this point
						break;

					Match match = regex.Match(line);
					if (match.Success)
					{
						string fileName = match.Groups["FileName"].Value;
						string fullFileName = Path.Combine(solutionDirectory, fileName);

						// Solution folder appear as projects. So we need to check whether
						// the file name is an directory. In this case it is a solution
						// folder an not a project.

						if (!Directory.Exists(fullFileName))
							projectList.Add(fullFileName);
					}

					line = sr.ReadLine();
				}
			}

			return projectList.ToArray();
		}

		private static void RemoveReadOnlyFlag(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				File.SetAttributes(path, attributes & (~FileAttributes.ReadOnly));
		}

		private static void CleanSolution(string solutionFileName)
		{
			string suoFileName = Path.ChangeExtension(solutionFileName, ".suo");

			// Delete solution user options if they exists.
			if (File.Exists(suoFileName))
			{
				RemoveReadOnlyFlag(suoFileName);
				File.Delete(suoFileName);
			}

			List<string> solutionFileContents = new List<string>();

			using (StreamReader sr = new StreamReader(solutionFileName, Encoding.Default))
			{
				string line = sr.ReadLine();
				while (line != null)
				{
					solutionFileContents.Add(line);
					line = sr.ReadLine();
				}
			}

			RemoveReadOnlyFlag(solutionFileName);
			using (StreamWriter sw = new StreamWriter(solutionFileName, false, Encoding.Default))
			{
				bool inSourceCodeSection = false;
				foreach (string line in solutionFileContents)
				{
					string trimmedLine = line.Trim();

					if (trimmedLine == "GlobalSection(TeamFoundationVersionControl) = preSolution")
					{
						inSourceCodeSection = true;
					}
					else if (trimmedLine == "EndGlobalSection" && inSourceCodeSection)
					{
						inSourceCodeSection = false;
					}
					else if (!inSourceCodeSection)
					{
						sw.WriteLine(line);
					}
				}
			}
		}

		private static void CleanProject(string projectFileName)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(projectFileName);

			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

			XmlNodeList propertyGroups = xmlDocument.SelectNodes("/msbuild:Project/msbuild:PropertyGroup", namespaceManager);
			foreach (XmlNode propertyGroup in propertyGroups)
			{
				XmlNode sccProjectNameNode = propertyGroup.SelectSingleNode("msbuild:SccProjectName", namespaceManager);
				XmlNode sccLocalPathNode = propertyGroup.SelectSingleNode("msbuild:SccLocalPath", namespaceManager);
				XmlNode sccProviderNode = propertyGroup.SelectSingleNode("msbuild:SccProvider", namespaceManager);

				if (sccProjectNameNode != null)
					propertyGroup.RemoveChild(sccProjectNameNode);

				if (sccLocalPathNode != null)
					propertyGroup.RemoveChild(sccLocalPathNode);

				if (sccProviderNode != null)
					propertyGroup.RemoveChild(sccProviderNode);
			}

			RemoveReadOnlyFlag(projectFileName);
			xmlDocument.Save(projectFileName);
		}

		#endregion

		public override bool Execute()
		{
			foreach (ITaskItem solutionFile in _solutionFiles)
			{
				try
				{
					Remove(solutionFile.ItemSpec);
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
			}

			return !Log.HasLoggedErrors;
		}

		[Required]
		public ITaskItem[] SolutionFiles
		{
			get { return _solutionFiles; }
			set { _solutionFiles = value; }
		}
	}
}