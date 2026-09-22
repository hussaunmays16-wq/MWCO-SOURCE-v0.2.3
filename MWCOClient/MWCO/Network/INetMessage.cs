using System;
using System.IO;

namespace MWCO.Network
{
	public interface INetMessage
	{
		byte MessageId { get; }

		bool Read(BinaryReader reader);

		bool Write(BinaryWriter writer);
	}
}
