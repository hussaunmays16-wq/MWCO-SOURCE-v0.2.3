using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class EventHookSyncMessage : INetMessage
	{
		public byte MessageId
		{
			get
			{
				return 22;
			}
		}

		public EventHookSyncMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.optionalsMask);
				writer.Write(this.fsmID);
				writer.Write(this.fsmEventID);
				writer.Write(this.request);
				if ((this.optionalsMask & 1) != 0)
				{
					writer.Write(this.fsmEventName);
				}
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
				this.optionalsMask = reader.ReadByte();
				this.fsmID = reader.ReadInt32();
				this.fsmEventID = reader.ReadInt32();
				this.request = reader.ReadBoolean();
				if ((this.optionalsMask & 1) != 0)
				{
					this.fsmEventName = reader.ReadString();
				}
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public string FsmEventName
		{
			get
			{
				return this.fsmEventName;
			}
			set
			{
				this.fsmEventName = value;
				this.optionalsMask |= 1;
			}
		}

		public bool HasFsmEventName
		{
			get
			{
				return (this.optionalsMask & 1) > 0;
			}
		}

		private byte optionalsMask;

		public int fsmID;

		public int fsmEventID;

		public bool request;

		private string fsmEventName;
	}
}
