using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using XenAPI;
using System.Threading.Tasks;

namespace Evacuation
{
    public class Hypervisor : IComparable<Hypervisor>
    {
        static readonly Regex Trimmer = new Regex(@"\s\s+");

        public string SearchArea = "";

        public List<VirtualMachine> Vms = new List<VirtualMachine>();
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
		public string SortPod => (Pod == 0) ? HostName.Split('.')[1] : Pod.ToString("000");
		public string SortHost => Host.ToString("000");
        public List<Pool_patch> Patches;
        public int Host => int.Parse(HostNameAt(0).Replace("adminhyp", "").Replace("hyp", ""));
        public int Pod => (HostNameAt(1).Contains("pod")) ? int.Parse(HostNameAt(1).Replace("pod", "")) : 0;
        public string DC => (HostNameAt(1).Contains("pod")) ? HostNameAt(2) : HostNameAt(1);
        public bool ActiveSession => session != null;
        public long CoresRemaining => Cores - CoresAllocated;

        public string CpuVersion => Utilities.TryOrDefault<string, string>(() => {
			var tmp = CpuModel.Substring(CpuModel.IndexOf(' ') + 1);
			return tmp.Substring(0, tmp.IndexOf(' '));
		}, "");

        public int GetHostgroupCount(string HostGroup) => Utilities.TryOrDefault<int, int>(() => Vms.Where(x => x.HostGroup == HostGroup).Count(), 0);


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
			    //Already logged out
			}		
        }

        void LoadSession()
        {
            if (session == null)
            {
				try
				{
					// Establish a session
					// Trust the self-signed certs
					ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
					session = new Session(HostName, 443);
					// Authenticate with username and password. The third parameter tells the server which API version we support.
					session.login_with_password("root", Models.Password.ToString(), API_Version.API_2_5);
				}
				catch (Exception)
				{
					session = null;
				}
            }
        }


        public void Load()
        {
            Clear();
            LoadSession();
            if (session != null)
            {
                Parallel.Invoke(
                    () => LoadHypInfo(),
                    () => LoadHypDiskInfo(),
                    () => LoadVMInfo(),
                    () => LoadPatchInfo()
                );
            }
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
			LoadSession();
			if (session != null)
			{
				LoadVMInfo();
				LoadPIFByNetwork();
				LoadVIF();
			}
        }

        public void LoadDestHyp()
        {
			Clear();
			LoadSession();
			if (session != null)
			{
				Parallel.Invoke(
					() => LoadHypInfo(),
					() => LoadHypDiskInfo(),
					() => LoadVMInfo(),
					() => LoadPatchInfo()
				);
				LoadNetwork();
				LoadNetworkByVlan();
			}
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
            var vms = (from vm in (VM.get_all_records(session)
                               .Where(v => v.Value.is_a_template == false && v.Value.is_a_snapshot == false && v.Value.power_state == vm_power_state.Running && !v.Value.is_control_domain))
                            select vm.Value).ToList();
            Dictionary<XenRef<VM_guest_metrics>, VM_guest_metrics> vmMetrics = VM_guest_metrics.get_all_records(session);

            foreach (VM vm in vms)
            {
                var tmpVm = new VirtualMachine() {Vm = vm};
                Vms.Add(tmpVm);
                CoresAllocated += tmpVm.Cores;
                MemoryAllocated += tmpVm.Memory;
            }
            if (Vms != null) Guests = Vms.Count;
        }

        public int CompareTo(Hypervisor other)
        {
            // Sort by Pod then Host
            if (other == null) return 1;

            if (SortPod == other.SortPod) return string.Compare(SortHost, other.SortHost, StringComparison.Ordinal);
            return string.Compare(SortPod, other.SortPod, StringComparison.Ordinal);

        }

		public double GetProgress(out int TaskCount)
		{
            try
			{
				if (session == null) LoadSession();
				var tasks = XenAPI.Task.get_all_records(session).Where(x => x.Value.name_label == "VM.migrate_send").Select(x => x.Value).ToList();
				TaskCount = tasks.Count;
                return tasks.Sum(x => x.progress) / TaskCount;
			}
			catch
			{
                TaskCount = 0;
                return 0;
			}
		}

        public List<double> GetProgress()
        {
            List<double> taskStatus = new List<double>();
			try
			{
				if (session == null) LoadSession();
				var tasks = XenAPI.Task.get_all_records(session).Where(x => x.Value.name_label == "VM.migrate_send").OrderBy(x => x.Value.created).Select(x => x.Value).ToList();
                return tasks.Select(x => x.progress).ToList();
			}
			catch
			{
                return new List<double>();
			}
        }

		public void LoadPatchInfo()
		{
			// Load Host Patches
			Patches = Pool_patch.get_all_records(session).Select(x => x.Value).ToList();
			PatchCount = Patches.Count;
		}

		private string HostNameAt(int i)
		{
			try
			{
				return HostName.Split('.')[i];
			}
			catch
			{
				return "";
			}
		}
    }
}
