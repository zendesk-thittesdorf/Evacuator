// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Evacuation
{
	public partial class viewSelectHyp : NSViewController
	{
		public string Pod = "";
		public Hypervisor SourceHyp;
        List<Hypervisor> SrcHyps => ((HypTable.DataSource)tblSourceHyp.DataSource).Hyps;
        List<Hypervisor> DstHyps => ((HypTable.DataSource)tblDestinationHyps.DataSource).Hyps;

        HypTable.DataSource hypDS = new HypTable.DataSource();

		public viewSelectHyp (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Do any additional setup after loading the view.
			tblSourceHyp.DataSource = new HypTable.DataSource();
			tblSourceHyp.Delegate = new HypTable.Delegate((HypTable.DataSource)tblSourceHyp.DataSource);

			tblDestinationHyps.DataSource = new HypTable.DataSource();
			tblDestinationHyps.Delegate = new HypTable.Delegate((HypTable.DataSource)tblDestinationHyps.DataSource);
			new Thread(() => FindHyps()).Start();

		}

        partial void SourceHypClick(NSObject sender)
        {
            DstHyps.Clear();
            if (tblSourceHyp.SelectedRow >= 0) {
                SourceHyp = SrcHyps[(int)tblSourceHyp.SelectedRow];
                var CPU = SourceHyp.CpuVersion;
                DstHyps.Clear();
                DstHyps.AddRange(SrcHyps.Where(x => x.CpuVersion == CPU && x.HostName != SourceHyp.HostName).ToList());
            }
            tblDestinationHyps.ReloadData();
            tblDestinationHyps.SelectAll(this);
            UpdateEvacutateButton();
        }

        partial void DestinationHypClick(NSObject sender)
        {
            UpdateEvacutateButton();
        }

        private void UpdateEvacutateButton()
        {
            btnEvacuate.Enabled = (tblSourceHyp.SelectedRowCount == 1 && tblDestinationHyps.SelectedRowCount > 0);
        }

		private void FindHyps()
		{
            this.BeginInvokeOnMainThread(()=>{
				tblSourceHyp.Enabled = false;
				this.View.Window.Title = "Select Hyps - Loading Hyps";
			});

            var searchArea = Pod;
				Parallel.For(1, 30, i =>
				  {
					  var hostName = searchArea.Replace("{n}", i.ToString());
					  if (hostName.InDns())
					  {
                          var hyp = new Hypervisor() { HostName = hostName, SearchArea = searchArea };
						  this.BeginInvokeOnMainThread(() =>
						  {
							  SrcHyps.Add(hyp);
							  SrcHyps.Sort();
							  tblSourceHyp.ReloadData();
						  });
                          try
                          {
							  hyp.Load();
						  }
                          catch
                          {

                          }
                          this.BeginInvokeOnMainThread(() =>
						  {
							  tblSourceHyp.ReloadData();
						  });
					  }
				  });
			this.BeginInvokeOnMainThread(() =>
			{
                var tmpHyps = SrcHyps.Where(x => x.ActiveSession).ToList();
                SrcHyps.Clear();
                SrcHyps.AddRange(tmpHyps);
                
                tblSourceHyp.ReloadData();
				tblSourceHyp.Enabled = true;
				this.View.Window.Title = "Select Hyps";
			});
		}

        partial void cmdShowAllDest(NSObject sender)
        {
            DstHyps.Clear();
            DstHyps.AddRange(SrcHyps);
            DstHyps.Sort();
            tblDestinationHyps.ReloadData();
        }

		public override void PrepareForSegue(NSStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);

			// Take action based on Segue ID
			switch (segue.Identifier)
			{
				case "ViewEvacuate":
					List<Hypervisor> DestHyps = new List<Hypervisor>();
					foreach (var item in tblDestinationHyps.SelectedRows) DestHyps.Add(DstHyps[(int)item]);
                    ((NSWindowController)this.View.Window.WindowController).Close();
                    var dest = (viewMoving)segue.DestinationController;
                    dest.Pod = Pod;
                    dest.SourceHyp = SourceHyp;
                    dest.DestHyps.AddRange(DestHyps);
					break;
			}
		}


		partial void cmdLaunchConsole(NSObject sender)
		{
            SourceHyp.OpenIdrac();
		}

        partial void cmdCopyMoob(NSObject sender)
        {
            SourceHyp.LaunchMoobConsole();
        }
	}
}
