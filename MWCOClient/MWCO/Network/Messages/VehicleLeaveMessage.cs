using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class VehicleLeaveMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 10;
			}
		}

		public VehicleLeaveMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public bool Read(BinaryReader reader)
		{
			bool flag;
			try
			{
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}
	}
}
