using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Helpers
{
    
        public static class NetworkHelper
        {
            public static async Task<string> GetLocalIpAddress()
            {
                string localIP = string.Empty;
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
                return localIP;
            }

            public static async Task<string> GetLocalMacAddress()
            {
                var macAddr = (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();

                return macAddr;
            }
        public static async Task<string> GetPublicIpAddressAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Use a reliable service to get the public IP
                    string publicIp = await httpClient.GetStringAsync("https://api.ipify.org");
                    return publicIp;
                }
                catch (Exception ex)
                {
                    // Handle exception if any
                    return "Error: " + ex.Message;
                }
            }
        }
    }

   
}

