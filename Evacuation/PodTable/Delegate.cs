using System;
using AppKit;

namespace Evacuation.PodTable
{
    public class Delegate : NSTableViewDelegate
    {
		private const string CellIdentifier = "PodsCell";

        private DataSource dataSource;

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

			view.StringValue = dataSource.Pods[(int)row];

			return view;
		}

		public override bool ShouldSelectRow(NSTableView tableView, nint row)
		{
			return true;
		}
    }
}
