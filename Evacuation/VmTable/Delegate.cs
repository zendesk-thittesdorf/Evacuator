using System;
using AppKit;

namespace Evacuation.VmTable
{
    public class Delegate : NSTableViewDelegate
    {
		private const string CellIdentifier = "VmsCell";
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
			var vm = this.dataSource.Vms[(int)row];
			switch (tableColumn.Title)
			{
				case "Name":
					view.StringValue = vm.Name;
					break;
				case "Cores":
					view.StringValue = vm.Cores.ToString();
					break;
				case "Memory":
                    view.StringValue = vm.Memory.ToString();
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
