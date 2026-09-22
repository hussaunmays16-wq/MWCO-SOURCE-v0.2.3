using System;

namespace MWCO.Network.Messages
{
	public class MessageIdsHelpers
	{
		public static bool IsValueValid(int value)
		{
			return value <= 30;
		}

		public MessageIdsHelpers()
		{
		}
	}
}
