using System;
using System.Windows.Forms;

namespace NQuery.Demo
{
	public class VisualStudioToolBarRenderer : ToolStripProfessionalRenderer
	{
		public VisualStudioToolBarRenderer()
			: base(new TanColorTable())
		{
			RoundedEdges = false;
		}
	}
}
