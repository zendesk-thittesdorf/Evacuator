using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Utilities;

using XenAPI;
using System.Threading.Tasks;

namespace Evacuation
{
    public class Hypervisor : IComparable<Hypervisor>
    {
        static readonly Regex Trimmer = new Regex(@"\s\s+");

        public string SearchArea = "";

        public readonly List<VirtualMachine> Vms = new List<VirtualMachine>();
        public string HostName { get; set; } = "";
        public string Uuid { get; set; } = "";
        public string XenVersion { get; set; } = "";
        public long Cores { get; set; }
        public long CoresAllocated { get; set; }
        public long Memory { get; set; } // gb
        public long MemoryAllocated { get; set; } // gb
        public long MemoryFree { get; set; } // gb
        public long Guests { get; set; }
        public string CpuModel { get; set; } = "";
        public double DiskUsed { get; set; }
        public double DiskAllocated { get; set; }
        public double DiskSize { get; set; }
        public double DiskFree => DiskSize - DiskUsed;
        public string VolumeTypes { get; set; } = "";
        public int CurMovesToMe = 0;
		public long PatchCount { get; set; } = 0;
        public List<Pool_patch> Patches;

		public long CoresRemaining => Cores - CoresAllocated;
        public string CpuVersion {
            get{
                try
                {
                    var tmp = CpuModel.Substring(CpuModel.IndexOf(' ') + 1);
                    var app = tmp.IndexOf(' ');
                    return tmp.Substring(0, tmp.IndexOf(' '));
				}
                catch
                {
                    return "";
                }
            }
        }

        public int GetHostgroupCount(string Hostgroup)
        {
            var toReturn = 0;
            try
            {
                var vms = Vms.Where(x => x.HostGroup == Hostgroup);
                toReturn = vms.Count();
            }
            catch (Exception)
            {

            }
            return toReturn;
        }

        public List<long> Vlans { get; set; } = new List<long>();

		private Dictionary<XenRef<Network>, PIF> PifByNetwork = new Dictionary<XenRef<Network>, PIF>();
		public Dictionary<long, string> NetworkByVlan = new Dictionary<long, string>(); //Vlan,NetworkUUID
        private Dictionary<XenRef<Network>, Network> Networks = new Dictionary<XenRef<Network>, Network>();


        Session session;

		~Hypervisor()  // destructor
		{
            try
            {
                session.logout();
            }
            catch
            {
			    
			}		
        }

        public string SortPod
        {
            get
            {
                if (Pod == 0)
                    return HostName.Split('.')[1];
                else
                    return Pod.ToString("000");
            }
        }

        public string SortHost
        {
            get
            {
                return Host.ToString("000");
            }
        }

        void LoadSession()
        {
            // Establish a session
            // Trust the self-signed certs
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            session = new Session(HostName, 443);

            // Authenticate with username and password. The third parameter tells the server which API version we support.
            session.login_with_password("root", Models.Password.value, API_Version.API_2_5);
        }


        public void Load()
        {
            Clear();
            if (session == null) LoadSession();

            Parallel.Invoke(
                () => LoadHypInfo(),
                () => LoadHypDiskInfo(),
                () => LoadVMInfo(),
                () => LoadPatchInfo()
            );
        }

        private void Clear()
        {
            Vms.Clear();
            Cores = 0;
            CoresAllocated = 0;
            Memory = 0;
            MemoryAllocated = 0;
            MemoryFree = 0;
            Guests = 0;
            DiskUsed = 0;
            DiskAllocated = 0;
            DiskSize = 0;
            VolumeTypes = "";
            Vlans.Clear();
        
			PifByNetwork.Clear();
			NetworkByVlan.Clear();
			Networks.Clear();
        }

        public void LoadSourceHyp()
        {
			Clear();
			if (session == null) LoadSession();
            LoadVMInfo();
			LoadPIFByNetwork();
			LoadVIF();
        }

        public void LoadDestHyp()
        {
			Clear();
			if (session == null) LoadSession();

			Parallel.Invoke(
				() => LoadHypInfo(),
				() => LoadHypDiskInfo(),
				() => LoadVMInfo(),
				() => LoadPatchInfo()
			);
            LoadNetwork();
            LoadNetworkByVlan();
        }

        private void LoadNetwork()
        {
            Networks = Network.get_all_records(session);
        }

        private void LoadVIF()
        {
            var vifs = VIF.get_all_records(session);
            foreach (var vm in Vms)
            {
                vm.Networks.Clear();
                foreach (var vif in vm.VIFs)
                {
                    if (vifs.ContainsKey(vif))
                    {
                        var Vlan= (PifByNetwork.ContainsKey(vifs[vif].network)) ? PifByNetwork[vifs[vif].network].VLAN : -1;

                        vm.Networks.Add(new NetworkInfo { VifUUID = vifs[vif].uuid,
                        VlanID = Vlan});
                    }
                }
            }
        }

        public void LoadPIFByNetwork()
        {
            PifByNetwork = PIF.get_all_records(session).ToDictionary(p=> p.Value.network, p=> p.Value);
        }

		public void LoadNetworkByVlan()
		{
            var pifs = PIF.get_all_records(session).Select(p => p.Value).Where(p => p.VLAN != -1).ToList();
            NetworkByVlan.Clear();
            foreach (var pif in pifs)
            {
                if (Networks.ContainsKey(pif.network))
                {
                    NetworkByVlan.Add(pif.VLAN, Networks[pif.network].uuid);
                }
            }
		}

        private void LoadHypInfo()
        {
            // Load Host Info
            Host host = XenAPI.Host.get_all_records(session).Last().Value;
            // Load Host Metrics
            Host_metrics metrics = Host_metrics.get_record(session, host.metrics);
            if (metrics != null)
            {
                Memory = metrics.memory_total.KbToGb();
                MemoryFree = metrics.memory_free.KbToGb();
            }
            if (host != null)
            {
                var cpus = host.cpu_info;
                Cores = long.Parse(cpus["cpu_count"]);
                CpuModel = Trimmer.Replace(cpus["modelname"], " ").Replace("Intel(R) Xeon(R) CPU ", "");
                XenVersion = host.software_version["product_version"];
                Uuid = host.uuid;
            }
        }

        private void LoadHypDiskInfo()
        {
            List<SR> volumes = SR.get_all_records(session).Where(srKVP => "ext,lvm".Contains(srKVP.Value.type)).Select(p => p.Value).ToList();

            DiskUsed = volumes.Sum(x => x.physical_utilisation).KbToTb();
            DiskAllocated = volumes.Sum(x => x.virtual_allocation).KbToTb();
            DiskSize = volumes.Sum(x => x.physical_size).KbToTb();
        }

        private void LoadVlanInfo()
        {
            // Load Vlans
            Vlans = (from vlan in VLAN.get_all_records(session) select vlan.Value.tag).ToList();
        }

        private void LoadVMInfo()
        {
            // Load VM Info
            List<VM> vms = (from vm in (VM.get_all_records(session)
                               .Where(v => v.Value.is_a_template == false && v.Value.is_a_snapshot == false && v.Value.power_state == vm_power_state.Running))
                            select vm.Value).ToList();
            Dictionary<XenRef<VM_guest_metrics>, VM_guest_metrics> vmMetrics = VM_guest_metrics.get_all_records(session);

            foreach (var vm in vms.Where(v => !(v.is_control_domain)))
            {
                CoresAllocated += vm.VCPUs_max;
                MemoryAllocated += vm.memory_static_max.KbToGb();
                Vms.Add(new VirtualMachine(vm));
            }
            if (vms != null) Guests = vms.Count;
        }

        public int CompareTo(Hypervisor other)
        {
            // Sort by Pod then Host
            if (other == null) return 1;

            if (SortPod == other.SortPod) return string.Compare(SortHost, other.SortHost, StringComparison.Ordinal);
            return string.Compare(SortPod, other.SortPod, StringComparison.Ordinal);

        }

        public int Host
        {
            get
            {
                var hostString = HostName.Split('.')[0];
                if (hostString.Contains("adminhyp"))
                    return int.Parse(hostString.Replace("adminhyp", ""));
                return int.Parse(hostString.Replace("hyp", ""));
            }
        }

        public int Pod
        {
            get
            {
                var podString = HostName.Split('.')[1];
                return (podString.Contains("pod")) ? int.Parse(podString.Replace("pod", "")) : 0;
            }
        }

        public string DC
        {
            get
            {
                var podString = HostName.Split('.')[1];
                return (podString.Contains("pod")) ? HostName.Split('.')[2] : podString;
            }
        }

        public double GetProgress()
        {
			double progress = 0;

			try
            {
				if (session == null) LoadSession();
				var tasks = XenAPI.Task.get_all_records(session).Where(x => x.Value.name_label == "VM.migrate_send").Select(x => x.Value).ToList();
				if (tasks.Count > 0)
				{
					progress = (tasks[0].progress);
				}
            }
            catch (Exception)
            {

            }

			return progress;
        }

		public void LoadPatchInfo()
		{
			// Load Host Patches
			//Patches = Pool_patch.get_all_records(session).Where(p => p.Value.pool_applied).Select(x => x.Value).ToList();
			Patches = Pool_patch.get_all_records(session).Select(x => x.Value).ToList();

			PatchCount = Patches.Count;
		}
    }
}
