using System;
using AppKit;

namespace Evacuation.HypTable
{
	public class Delegate : NSTableViewDelegate
	{
		private const string CellIdentifier = "HypsCell";
		private DataSource dataSource = new DataSource();

		public Delegate(DataSource datasource)
		{
			this.dataSource = datasource;
		}

		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			NSTextField view = (NSTextField)tableView.MakeView(CellIdentifier, this);
			if (view == null)
			{
				view = new NSTextField();
				view.Identifier = CellIdentifier;
				view.BackgroundColor = NSColor.Clear;
				view.Bordered = false;
				view.Selectable = false;
				view.Editable = false;
			}
            var hyp = this.dataSource.Hyps[(int)row];
			switch (tableColumn.Title)
            {
                case "VMs":
            		view.StringValue = hyp.Vms.Count.ToString();
                    break;
				case "Source Hypervisor":
					view.StringValue = hyp.HostName;
					break;
				case "Destination Hypervisors":
					view.StringValue = hyp.HostName;
					break;
				case "CPUs Remain":
                    view.StringValue = (hyp.Cores - hyp.CoresAllocated).ToString();
					break;
				case "CPUs Used":
					view.StringValue = hyp.CoresAllocated.ToString();
					break;
				case "Disk Remain":
					view.StringValue = (hyp.DiskSize - hyp.DiskUsed).ToString();
					break;
				case "Disk Used":
					view.StringValue = hyp.DiskUsed.ToString();
					break;
				case "CPU Model":
					view.StringValue = hyp.CpuModel;
					break;
				case "Memory Remain":
					view.StringValue = hyp.MemoryFree.ToString();
					break;
				case "Patches":
					view.StringValue = hyp.PatchCount.ToString();
					break;
			}

			return view;
		}

		public override bool ShouldSelectRow(NSTableView tableView, nint row)
		{
			return true;
		}
    }
}
