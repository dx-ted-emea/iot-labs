using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceSender
{
    public class DeviceSendingDetails
    {
        public int NumberOfDevices { get; set; }
        public float TemperatureMin { get; set; }
        public float TemperatureMax { get; set; }
        public FailedDeviceSettings[] FailureConditions { get; set; }
        public int MessagesPerDevice { get; set; }
        public int IterationSeconds { get; set; }
        public int MillisecondDelay { get; set; }
    }

    public class FailedDeviceSettings
    {
        public FailedDeviceSettings(int deviceId, float deviceGradient)
        {
            FailedDeviceId = deviceId;
            FailedDeviceGradient = deviceGradient;
        }
        public int FailedDeviceId { get; set; }
        public float FailedDeviceGradient { get; set; }
    }
}
