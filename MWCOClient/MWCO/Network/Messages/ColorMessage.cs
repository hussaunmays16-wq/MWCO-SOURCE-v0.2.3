using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class ColorMessage
	{
		public ColorMessage()
		{
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				writer.Write(this.r);
				writer.Write(this.g);
				writer.Write(this.b);
				writer.Write(this.a);
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
				this.r = reader.ReadSingle();
				this.g = reader.ReadSingle();
				this.b = reader.ReadSingle();
				this.a = reader.ReadSingle();
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public float r;

		public float g;

		public float b;

		public float a;
	}
}
