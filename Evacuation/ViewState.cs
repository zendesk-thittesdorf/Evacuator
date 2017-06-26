using System.Collections.Generic;

namespace Evacuation
{
    public static class ViewState
    {
        public static string Pod = "";
        public static Hypervisor SourceHyp;
        public static List<Hypervisor> DestHyps = new List<Hypervisor>();
    }
}
