using System;
using AppKit;
using System.Collections.Generic;

namespace Evacuation.PodTable
{
    public class DataSource : NSTableViewDataSource
    {
		public List<String> Pods = new List<String>();

		public override nint GetRowCount(NSTableView tableView)
		{
			return Pods.Count;
		}
    }
}
