using System;
namespace Evacuation.Models
{
    public class HostgroupDS
    {
        public string Hostgroup { get; set; } = "";
        public string HostNumber { get; set; } = "";
        public int Hyp { get; set; }
        public int Pod { get; set; }
        public string HypSort { get; set; }
        public string PodSort { get; set; }
        public string DC { get; set; }
    }
}
