using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NQuery.UI
{
	internal static class NativeMethods
	{
		private const UInt32 TVIF_HANDLE = 16;
		private const UInt32 TVIF_STATE = 8;
		private const UInt32 TVIS_OVERLAYMASK = 0x0F00;
		private const UInt32 TV_FIRST = 4352;
		private const UInt32 TVM_SETITEM = TV_FIRST + 13;

		[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
		private struct TVITEM
		{
			public uint mask;
			public IntPtr hItem;
			public uint state;
			public uint stateMask;
			public IntPtr pszText;
			public int cchTextMax;
			public int iImage;
			public int iSelectedImage;
			public int cChildren;
			public IntPtr lParam;
		}

		[DllImport("User32", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref TVITEM lParam);

		[DllImport("comctl32", EntryPoint = "ImageList_SetOverlayImage")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetOverlayImage(IntPtr hImageList, Int32 imageIndex, Int32 overlayIndex);

		/// <summary>
		/// Associates an overlay image with a tree node. The overlay image is taken from the normal
		/// image list of the tree view the node is contained in. Also the overlay image must have been
		/// made available by <see cref="SetOverlayImage(ImageList,int,int)"/>.
		/// </summary>
		/// <param name="node">node to associate the overlay with</param>
		/// <param name="overlayIndex">index between 1 and 5 indicating the overlay image to use or 0 to indicate no image</param>
		public static void SetOverlayIndex(TreeNode node, int overlayIndex)
		{
			if (overlayIndex < 0 || overlayIndex > 5)
				throw new ArgumentOutOfRangeException("overlayIndex", overlayIndex, "overlay image must be between 0 and 5");

			TVITEM tvi = new TVITEM();

			// Define the item we want to set the State in.
			tvi.hItem = node.Handle;

			tvi.mask = TVIF_HANDLE | TVIF_STATE;

			// Left shift 8 to put info in bits 8 to 11
			tvi.state = (uint)overlayIndex << 8;

			// activate the overlay information
			tvi.stateMask = TVIS_OVERLAYMASK;

			// Send the TVM_SETITEM message.
			SendMessage(node.TreeView.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
		}

		/// <summary>
		/// Makes an image also availabe as overlay image. One image list may have up to 4 overlay images.
		/// </summary>
		/// <param name="imageList">image list</param>
		/// <param name="imageIndex">index of image to use as overlay image</param>
		/// <param name="overlayIndex">index between 1 and 5 under which the overlay index should be availabe</param>
		public static void SetOverlayImage(ImageList imageList, int imageIndex, int overlayIndex)
		{
			if (overlayIndex < 1 || overlayIndex > 5)
				throw new ArgumentOutOfRangeException("overlayIndex", overlayIndex, "overlay index must be between 1 and 5");

			SetOverlayImage(imageList.Handle, imageIndex, overlayIndex);
		}

		private const int SB_HORZ = 0;
		private const int SB_VERT = 1;

		[DllImport("user32.dll")]
		private static extern int GetScrollPos(IntPtr hWnd, int nBar);

		[DllImport("user32.dll")]
		private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

		public static Point GetScrollPos(Control control)
		{
			int hScroll = GetScrollPos(control.Handle, SB_HORZ);
			int vScroll = GetScrollPos(control.Handle, SB_VERT);
			return new Point(hScroll, vScroll);
		}

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "NQuery.UI.NativeMethods.SetScrollPos(System.IntPtr,System.Int32,System.Int32,System.Boolean)")]
        public static void SetScrollPos(Control control, Point scrollPos, bool redraw)
		{
			SetScrollPos(control.Handle, SB_HORZ, scrollPos.X, redraw);
			SetScrollPos(control.Handle, SB_VERT, scrollPos.Y, redraw);
		}
	}
}