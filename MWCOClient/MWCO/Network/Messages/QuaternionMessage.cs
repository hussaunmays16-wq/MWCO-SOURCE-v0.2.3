using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class QuaternionMessage
	{
		public QuaternionMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.w);
				writer.Write(this.x);
				writer.Write(this.y);
				writer.Write(this.z);
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
				this.w = reader.ReadSingle();
				this.x = reader.ReadSingle();
				this.y = reader.ReadSingle();
				this.z = reader.ReadSingle();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public float w;

		public float x;

		public float y;

		public float z;
	}
}
