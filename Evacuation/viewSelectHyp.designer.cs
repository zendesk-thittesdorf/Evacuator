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
	[Register ("viewSelectHyp")]
	partial class viewSelectHyp
	{
		[Outlet]
		AppKit.NSButton btnEvacuate { get; set; }

		[Outlet]
		AppKit.NSButton btnLaunchConsole { get; set; }

		[Outlet]
		AppKit.NSTableView tblDestinationHyps { get; set; }

		[Outlet]
		AppKit.NSTableView tblSourceHyp { get; set; }

		[Action ("cmdCopyMoob:")]
		partial void cmdCopyMoob (Foundation.NSObject sender);

		[Action ("cmdLaunchConsole:")]
		partial void cmdLaunchConsole (Foundation.NSObject sender);

		[Action ("cmdShowAllDest:")]
		partial void cmdShowAllDest (Foundation.NSObject sender);

		[Action ("DestinationHypClick:")]
		partial void DestinationHypClick (Foundation.NSObject sender);

		[Action ("SourceHypClick:")]
		partial void SourceHypClick (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnEvacuate != null) {
				btnEvacuate.Dispose ();
				btnEvacuate = null;
			}

			if (btnLaunchConsole != null) {
				btnLaunchConsole.Dispose ();
				btnLaunchConsole = null;
			}

			if (tblDestinationHyps != null) {
				tblDestinationHyps.Dispose ();
				tblDestinationHyps = null;
			}

			if (tblSourceHyp != null) {
				tblSourceHyp.Dispose ();
				tblSourceHyp = null;
			}
		}
	}
}
