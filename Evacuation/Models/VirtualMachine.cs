using XenAPI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Evacuation
{
	public class VirtualMachine
    {
		Regex rgx = new Regex(@"\d+$");

		public VM Vm;
        public long Cores => (Vm == null) ? 0 : Vm.VCPUs_max;     
        public long Memory => (Vm == null) ? 0 : Vm.memory_static_max.KbToGb();
        public string State => (Vm == null) ? "" : Vm.power_state.ToString();
        public  List<XenRef<VIF>> VIFs => (Vm == null) ? new List<XenRef<VIF>>() : Vm.VIFs;
        public string UUID => (Vm == null) ? "" : Vm.uuid;
		public List<NetworkInfo> Networks = new List<NetworkInfo>();
        public string Name => (Vm == null) ? "" : Vm.name_label;
		public string HostGroup => (Name == "") ? "" : rgx.Replace(Name.Split('.')[0], "");
		public string HostNumber => (Name == "") ? "" : Name.Split('.')[0].Replace(HostGroup, "");
	}

    public class NetworkInfo
    {
        public long VlanID { get; set; } = -1;
        public string VifUUID { get; set; } = "";
    }

	public class VirtualMachineDisk
	{
		public string Name = "";
		public int PhysicalSize = 0;
		public int VirtualSize = 0;
	}
}
