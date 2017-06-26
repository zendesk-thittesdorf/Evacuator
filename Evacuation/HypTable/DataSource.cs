using System;
using AppKit;
using System.Collections.Generic;

namespace Evacuation.HypTable
{
	public class DataSource : NSTableViewDataSource
	{
		public List<Hypervisor> Hyps = new List<Hypervisor>();

		public override nint GetRowCount(NSTableView tableView)
		{
			return Hyps.Count;
		}
    }
}
