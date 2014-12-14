using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoTLabs.DeviceSender
{
    public class ConnectionDetails
    {
        public ConnectionDetails(string serviceBusNamespace, string eventHubName, string sasPolicyName, string sasPolicyKey)
        {
            ServiceBusNamespace = serviceBusNamespace;
            EventHubName = eventHubName;
            SasPolicyName = sasPolicyName;
            SasPolicyKey = sasPolicyKey;
        }

        public string ServiceBusNamespace { get; set; }
        public string EventHubName { get; set; }
        public string SasPolicyName { get; set; }
        public string SasPolicyKey { get; set; }
    }
}
