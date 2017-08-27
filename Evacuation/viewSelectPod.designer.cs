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
	[Register ("viewSelectPod")]
	partial class viewSelectPod
	{
		[Outlet]
		AppKit.NSButton btnLoadPod { get; set; }

		[Outlet]
		AppKit.NSButton SetDracPassButton { get; set; }

		[Outlet]
		AppKit.NSButton SetPassButton { get; set; }

		[Outlet]
		AppKit.NSTableView tblPods { get; set; }

		[Action ("cmdLoadPod:")]
		partial void cmdLoadPod (Foundation.NSObject sender);

		[Action ("cmdSetDracPass:")]
		partial void cmdSetDracPass (Foundation.NSObject sender);

		[Action ("cmdSetPass:")]
		partial void cmdSetPass (Foundation.NSObject sender);

		[Action ("SelectPodClick:")]
		partial void SelectPodClick (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnLoadPod != null) {
				btnLoadPod.Dispose ();
				btnLoadPod = null;
			}

			if (SetPassButton != null) {
				SetPassButton.Dispose ();
				SetPassButton = null;
			}

			if (SetDracPassButton != null) {
				SetDracPassButton.Dispose ();
				SetDracPassButton = null;
			}

			if (tblPods != null) {
				tblPods.Dispose ();
				tblPods = null;
			}
		}
	}
}
