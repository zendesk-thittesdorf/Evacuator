using XenAPI;
using Utilities;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Evacuation
{
	public class VirtualMachine
	{
        string _name = "";
		public long Cores { get; set; }
		public long Memory { get; set; }
		public string State { get; set; } = "";
		public string HostGroup { get; set; } = "";
        public string HostNumber { get; set; } = "";
        public List<XenRef<VIF>> VIFs;
        public string UUID { get; set; } = "";
        public List<NetworkInfo> Networks = new List<NetworkInfo>();

		public VirtualMachine(VM vm)
		{
			Name = vm.name_label;
			Cores = vm.VCPUs_max;
			Memory = vm.memory_static_max.KbToGb();
			State = vm.power_state.ToString();
            UUID = vm.uuid;
            VIFs = vm.VIFs;
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
				Regex rgx = new Regex(@"\d+$");
				HostGroup = rgx.Replace(value.Split('.')[0], "");
                HostNumber = value.Split('.')[0].Replace(HostGroup, "");
			}
		}
	}

    public class NetworkInfo
    {
        public long VlanID { get; set; } = -1;
        public string VifUUID { get; set; } = "";
    }
}
