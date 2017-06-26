using System;
using AppKit;
using System.Collections.Generic;

namespace Evacuation.VmTable
{
    public class DataSource : NSTableViewDataSource
    {
        public List<VirtualMachine> Vms = new List<VirtualMachine>();

		public override nint GetRowCount(NSTableView tableView)
		{
			return Vms.Count;
		}
    }
}
