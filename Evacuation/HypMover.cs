using System;
using System.Collections.Generic;
using Renci.SshNet;

namespace Evacuation
{
    public class HypMover
    {
        public string SourceHyp = "";
        public string DestHyp = "";
        public string Username = "root";
        public string VMUUID;
        public Dictionary<string, string> VIFtoDstNetwork = new Dictionary<string, string>();


        public void MoveVM()
        {
            using (var client = new SshClient(SourceHyp, Username, Models.Password.value))
            {
                client.Connect();
                client.RunCommand("xe vm-migrate --live host=" + DestHyp + " remote-master=" + 
                                  DestHyp + " remote-username=" + Username + " remote-password=" + 
                                  Models.Password.value + " vm=" + VMUUID + GetNetworkMap());
                client.Disconnect();
            }
        }

        private string GetNetworkMap()
        {
            var toReturn = "";

            foreach (var item in VIFtoDstNetwork)
            {
                toReturn += " vif:" + item.Key + "=" + item.Value;
            }

            return toReturn;
        }


    }
}
