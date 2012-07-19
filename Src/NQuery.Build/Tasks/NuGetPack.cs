using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NQuery.Build
{
    public class NuGetPack : ToolTask
    {
        [Required]
        public string Package { get; set; }

        public string Version { get; set; }

        public string BasePath { get; set; }

        public string OutputDirectory { get; set; }

        public bool Symbols { get; set; }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("pack");
            builder.AppendFileNameIfNotNull(Package);
            builder.AppendSwitchIfNotNull("-Version ", Version);
            builder.AppendSwitchIfNotNull("-BasePath ", BasePath);
            builder.AppendSwitchIfNotNull("-OutputDirectory ", OutputDirectory);

            if (Symbols)
              builder.AppendSwitch("-symbols");

            return builder.ToString();
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            base.LogEventsFromTextOutput(singleLine, MessageImportance.High);
        }

        protected override bool HandleTaskExecutionErrors()
        {
            Log.LogError("NuGet pack failed.");
            return false;
        }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), ToolName);
        }

        protected override string ToolName
        {
            get { return "NuGet.exe"; }
        }
    }
}