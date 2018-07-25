using io.netty.channel.rxtx;
using NLog;
using System;
using System.Management;

namespace io.netty.channel.usb
{
	public class UsbChannel : RxtxChannel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public UsbChannel() {}
		public UsbChannel(String portName, bool isTryReconnect = true, int tryReconnectTime = 1000) : base(portName, isTryReconnect, tryReconnectTime) {}

		private String getPortName(String deviceName)
		{
			String SerialPortName = null;

			try
			{
				ManagementObjectSearcher pnpSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE \'%" + deviceName + "%\'");

				foreach (ManagementObject pnpObj in pnpSearcher.Get())
				{
					ManagementObjectSearcher serialSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSSerial_PortName");

					foreach (ManagementObject serialObj in serialSearcher.Get())
					{
						String deviceId = ((String)pnpObj["DeviceID"]).Trim().ToUpper();
						String instanceName = ((String)serialObj["InstanceName"]).Trim().ToUpper();

						if (instanceName.Contains(deviceId))
						{
							SerialPortName = (String)serialObj["PortName"];
						}
					}
				}
			}
			catch (ManagementException e)
			{
				logger.Error(e.Message);
			}

			return SerialPortName;
		}

		protected override void doBind(String deviceName)
		{
			_isActive = false;
			portName = getPortName(deviceName);

			if (String.IsNullOrEmpty(portName))
				return;

			isTryReconnect = true;
			tryReconnectTime = 1000;
			Init();
		}
	}
}
