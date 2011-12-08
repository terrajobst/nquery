using System;
using System.Drawing;
using System.Windows.Forms;

namespace NQuery.Demo
{
	public sealed partial class VisualStudioPropertyGrid : PropertyGrid
	{
		public VisualStudioPropertyGrid()
		{
			InitializeComponent();

			CategoryForeColor = Color.FromArgb(113, 111, 100);
			CommandsActiveLinkColor = Color.Navy;
			CommandsLinkColor = Color.Navy;
			Font = new Font("Tahoma", 11F, FontStyle.Regular, GraphicsUnit.World);
			LineColor = Color.FromArgb(241, 239, 226);
			ToolStripRenderer = ToolStripManager.Renderer;
		}
	}
}
