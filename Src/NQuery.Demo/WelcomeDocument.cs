using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using NQuery.Demo.Properties;

using TD.SandDock;

namespace NQuery.Demo
{
	internal sealed partial class WelcomeDocument : UserTabbedDocument,
		IClipboardHandler
	{
		public WelcomeDocument()
		{
			InitializeComponent();
			LoadWelcomeRtf();
		}

		private void LoadWelcomeRtf()
		{
			using (MemoryStream memoryStream = new MemoryStream())
			using (StreamWriter sw = new StreamWriter(memoryStream))
			{
				sw.Write(Resources.Welcome);
				sw.Flush();

				memoryStream.Position = 0;
				richTextBox.LoadFile(memoryStream, RichTextBoxStreamType.RichText);
			}
		}

		#region IClipboardHandler Implementation

		bool IClipboardHandler.HasSelection
		{
			get { return richTextBox.SelectionLength > 0; }
		}

		bool IClipboardHandler.IsReadOnly
		{
			get { return true; }
		}

		void IClipboardHandler.Cut()
		{
			throw new NotImplementedException();
		}

		void IClipboardHandler.Copy()
		{
			richTextBox.Copy();
		}

		void IClipboardHandler.Paste()
		{
			throw new NotImplementedException();
		}

		void IClipboardHandler.Delete()
		{
			throw new NotImplementedException();
		}

		void IClipboardHandler.SelectAll()
		{
			richTextBox.SelectAll();
		}

		#endregion

		private void richTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}

		private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			copyToolStripMenuItem.Enabled = richTextBox.SelectionLength > 0;
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBox.Copy();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBox.SelectAll();
		}
	}
}