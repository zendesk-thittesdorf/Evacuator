// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;
using System.Threading.Tasks;
using System.Threading;
using Utilities;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace Evacuation
{
	public partial class viewSelectHyp : NSViewController
	{
        private List<Hypervisor> srcHyps
        {
            get{
                return ((HypTable.DataSource)tblSourceHyp.DataSource).Hyps;
            }
            set{
                ((HypTable.DataSource)tblSourceHyp.DataSource).Hyps = value;
            }
        }

		private List<Hypervisor> dstHyps
		{
			get
			{
				return ((HypTable.DataSource)tblDestinationHyps.DataSource).Hyps;
			}
			set
			{
				((HypTable.DataSource)tblDestinationHyps.DataSource).Hyps = value;
			}
		}

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
        }

        public override void ViewDidAppear(){
            new Thread(() => FindHyps()).Start();
		}

        partial void SourceHypClick(NSObject sender)
        {
            dstHyps.Clear();
            if (tblSourceHyp.SelectedRow >= 0) {
                ViewState.SourceHyp = srcHyps[(int)tblSourceHyp.SelectedRow];
                var CPU = ViewState.SourceHyp.CpuVersion;
                dstHyps = srcHyps.Where(x => x.CpuVersion == CPU && x.HostName != ViewState.SourceHyp.HostName).ToList();
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

            var searchArea = ViewState.Pod;
				Parallel.For(1, 30, i =>
				  {
					  var hostName = searchArea.Replace("{n}", i.ToString());
					  if (hostName.InDns())
					  {
                          var hyp = new Hypervisor() { HostName = hostName, SearchArea = searchArea };
						  this.BeginInvokeOnMainThread(() =>
						  {
							  srcHyps.Add(hyp);
							  srcHyps.Sort();
							  tblSourceHyp.ReloadData();
						  });
                          hyp.Load();
                          this.BeginInvokeOnMainThread(() =>
						  {
							  tblSourceHyp.ReloadData();
						  });
					  }
				  });
			this.BeginInvokeOnMainThread(() =>
			{
				tblSourceHyp.Enabled = true;
				this.View.Window.Title = "Select Hyps";
			});
		}

        partial void cmdShowAllDest(NSObject sender)
        {
            var src = ((HypTable.DataSource)tblSourceHyp.DataSource).Hyps;
            var dst = ((HypTable.DataSource)tblDestinationHyps.DataSource).Hyps;
            dst.Clear();
            dst.AddRange(src);
            dst.Sort();
            tblDestinationHyps.ReloadData();
        }

		public override void PrepareForSegue(NSStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);

			// Take action based on Segue ID
			switch (segue.Identifier)
			{
				case "ViewEvacuate":
					ViewState.DestHyps.Clear();
					var hypsDs = (HypTable.DataSource)tblDestinationHyps.DataSource;
					foreach (var item in tblDestinationHyps.SelectedRows) ViewState.DestHyps.Add(hypsDs.Hyps[(int)item]);
					NSWindowController wind = (NSWindowController)this.View.Window.WindowController;
					wind.Close();
					break;
			}
		}

		private List<Patch> LoadPatches(string ServerVersion)
		{
			var allPatches =
				(Patchdata)(new XmlSerializer(typeof(Patchdata)))
					.Deserialize(new MemoryStream(
						Encoding.UTF8.GetBytes(
							new System.Net.WebClient()
								 .DownloadString("http://updates.xensource.com/XenServer/updates.xml"))));
			List<Patch> minimalPatches = new List<Patch>();
			foreach (var patch in allPatches.Serverversions.Version.First(x => x.Value == ServerVersion).Minimalpatches.Patch)
			{
				minimalPatches.Add(allPatches.Patches.Patch.First(y => y.Uuid == patch.Uuid));
			}
			return minimalPatches.OrderBy(y => y.Namelabel).ToList();
		}
	}
}