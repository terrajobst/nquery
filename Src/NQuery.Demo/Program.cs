using System;
using System.Windows.Forms;

namespace NQuery.Demo
{
	internal static class Program
	{
		[STAThread]
		internal static void Main() 
		{
			ToolStripManager.Renderer = new VisualStudioToolBarRenderer();

			Application.EnableVisualStyles();
			Application.DoEvents();
			
			Application.Run(new MainForm());
		}
	}
}
