// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Evacuation
{
	[Register ("viewMoving")]
	partial class viewMoving
	{
		[Outlet]
		AppKit.NSButton btnPauseQueue { get; set; }

		[Outlet]
		AppKit.NSButton btnStartNewHost { get; set; }

		[Outlet]
		AppKit.NSButton btnStartNewPod { get; set; }

		[Outlet]
		AppKit.NSTextField lblSource { get; set; }

		[Outlet]
		AppKit.NSTextField lblStatus { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator prgMove { get; set; }

		[Outlet]
		AppKit.NSTableView tblDestHyps { get; set; }

		[Outlet]
		AppKit.NSTableView tblSourceVms { get; set; }

		[Action ("cmdPauseQueue:")]
		partial void cmdPauseQueue (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnStartNewHost != null) {
				btnStartNewHost.Dispose ();
				btnStartNewHost = null;
			}

			if (btnStartNewPod != null) {
				btnStartNewPod.Dispose ();
				btnStartNewPod = null;
			}

			if (lblSource != null) {
				lblSource.Dispose ();
				lblSource = null;
			}

			if (lblStatus != null) {
				lblStatus.Dispose ();
				lblStatus = null;
			}

			if (prgMove != null) {
				prgMove.Dispose ();
				prgMove = null;
			}

			if (tblDestHyps != null) {
				tblDestHyps.Dispose ();
				tblDestHyps = null;
			}

			if (tblSourceVms != null) {
				tblSourceVms.Dispose ();
				tblSourceVms = null;
			}

			if (btnPauseQueue != null) {
				btnPauseQueue.Dispose ();
				btnPauseQueue = null;
			}
		}
	}
}