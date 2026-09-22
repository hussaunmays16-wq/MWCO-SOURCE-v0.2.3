using System;
using System.IO;

namespace MWCO.Network.Messages
{
	public class PickedUpSync
	{
		public PickedUpSync()
		{
			this.position = new Vector3Message();
			this.rotation = new QuaternionMessage();
		}

		public bool Write(BinaryWriter writer)
		{
			bool flag;
			try
			{
				if (!this.position.Write(writer))
				{
					flag = false;
				}
				else if (!this.rotation.Write(writer))
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
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
				if (!this.position.Read(reader))
				{
					flag = false;
				}
				else if (!this.rotation.Read(reader))
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public Vector3Message position;

		public QuaternionMessage rotation;
	}
}
