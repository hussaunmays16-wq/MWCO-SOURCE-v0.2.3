using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using MWCO.Utilities;
using Steamworks;

namespace MWCO.Network
{
	internal class NetMessageHandler
	{
		public NetMessageHandler(NetManager theNetManager)
		{
			this.netManager = theNetManager;
		}

		private void SetupMsgs()
		{
			for (int i = 0; i < 27; i++)
			{
				this.msgs.Add(new ValueDuple<int, int>(i, 0));
			}
		}

		public void BindMessageHandler<T>(NetMessageHandler.MessageHandler<T> Handler) where T : INetMessage, new()
		{
			try
			{
				T message = new T();
				this.messageHandlers.Add(message.MessageId, delegate(CSteamID sender, BinaryReader reader)
				{
					if (!message.Read(reader))
					{
						Logger.Info("Failed to read network message " + message.MessageId.ToString() + " received from " + sender.ToString());
						return;
					}
					Handler(sender, message);
				});
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("COMPLETELY BUGGED OUT! " + ex.Message, MessageSeverity.Error);
			}
		}

		private string MsgesSent()
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < this.msgs.Count - 1; i++)
			{
				if (this.msgs[i].second > num2)
				{
					num2 = this.msgs[i].second;
					num = i;
				}
			}
			return string.Format("ID:{0} Times:{1}", num, num2);
		}

		public void ProcessMessage(byte messageId, CSteamID senderSteamId, BinaryReader reader)
		{
			if (this.messageHandlers.ContainsKey(messageId))
			{
				this.messageHandlers[messageId](senderSteamId, reader);
			}
		}

		private Dictionary<byte, NetMessageHandler.HandleMessageLowLevel> messageHandlers = new Dictionary<byte, NetMessageHandler.HandleMessageLowLevel>();

		private List<ValueDuple<int, int>> msgs = new List<ValueDuple<int, int>>(27);

		private NetManager netManager;

		private int num;

		private delegate void HandleMessageLowLevel(CSteamID sender, BinaryReader reader);

		public delegate void MessageHandler<T>(CSteamID sender, T message);

		[CompilerGenerated]
		private sealed class <>c__DisplayClass8_0<T> where T : INetMessage, new()
		{
			public <>c__DisplayClass8_0()
			{
			}

			internal void <BindMessageHandler>b__0(CSteamID sender, BinaryReader reader)
			{
				if (!this.message.Read(reader))
				{
					Logger.Info("Failed to read network message " + this.message.MessageId.ToString() + " received from " + sender.ToString());
					return;
				}
				this.Handler(sender, this.message);
			}

			public NetMessageHandler.MessageHandler<T> Handler;

			public T message;
		}
	}
}
