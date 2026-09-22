using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Network.Messages;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Network
{
	internal class NetManager
	{
		public bool IsHost
		{
			get
			{
				return this.mode == NetManager.Mode.Host;
			}
		}

		public bool IsPlayer
		{
			get
			{
				return this.mode == NetManager.Mode.Player;
			}
		}

		public bool IsOnline
		{
			get
			{
				return this.mode > NetManager.Mode.None;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this.state == NetManager.State.Playing;
			}
		}

		public NetMessageHandler MessageHandler
		{
			get
			{
				return this.netMessageHandler;
			}
		}

		public ulong TicksSinceConnectionStarted
		{
			get
			{
				return (ulong)(DateTime.UtcNow - this.connectionStartedTime).Ticks;
			}
		}

		public NetManager()
		{
			this.statistics = new NetStatistics(this);
			this.netManagerCreationTime = DateTime.UtcNow;
			this.netMessageHandler = new NetMessageHandler(this);
			NetWorld instance = NetWorld.Instance;
			if (instance != null)
			{
				instance.Dispose();
			}
			this.netWorld = new NetWorld(this);
			this.p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(new Callback<P2PSessionRequest_t>.DispatchDelegate(this.OnP2PSessionRequest));
			this.p2pConnectFailCallback = Callback<P2PSessionConnectFail_t>.Create(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(this.OnP2PConnectFail));
			this.gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(this.OnGameLobbyJoinRequested));
			this.lobbyCreatedCallResult = new CallResult<LobbyCreated_t>(new CallResult<LobbyCreated_t>.APIDispatchDelegate(this.OnLobbyCreated));
			this.lobbyEnterCallResult = new CallResult<LobbyEnter_t>(new CallResult<LobbyEnter_t>.APIDispatchDelegate(this.OnLobbyEnter));
			this.m_CallResultLobbyMatchList = new CallResult<LobbyMatchList_t>(new CallResult<LobbyMatchList_t>.APIDispatchDelegate(this.OnLobbyMatchList));
			this.savePath = Path.Combine(Application.persistentDataPath, "savefile.txt");
			this.itemsPath = Path.Combine(Application.persistentDataPath, "items2.txt");
			this.carPartsPath = Path.Combine(Application.persistentDataPath, "carparts.txt");
			NetManager.Instance = this;
			this.RegisterProtocolMessagesHandlers();
			this.FindLobbies();
		}

		public void Restart()
		{
			this.players.Clear();
		}

		private void OnP2PConnectFail(P2PSessionConnectFail_t result)
		{
			Logger.Error(string.Format("P2P Connection failed, session error: {0}, remote: {1}", Utils.P2PSessionErrorToString(result.m_eP2PSessionError), result.m_steamIDRemote));
		}

		private void OnP2PSessionRequest(P2PSessionRequest_t result)
		{
			if (SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote))
			{
				Logger.Info(string.Format("Accepted p2p session with {0}", result.m_steamIDRemote));
				this.connectionStartedTime = DateTime.UtcNow;
				return;
			}
			Logger.Error(string.Format("Failed to accept P2P session with {0}", result.m_steamIDRemote));
		}

		public CSteamID getLobbyId()
		{
			return this.currentLobbyId;
		}

		public void SendSave(CSteamID id)
		{
			if (!this.IsHost)
			{
				return;
			}
			if (this.cachedSaveBytes == null || this.cachedItemsBytes == null)
			{
				this.cachedSaveBytes = File.ReadAllBytes(this.savePath);
				this.cachedItemsBytes = File.ReadAllBytes(this.itemsPath);
				this.cachedCarPartsBytes = File.ReadAllBytes(this.carPartsPath);
			}
			SaveMessage saveMessage = new SaveMessage
			{
				saveBytes = this.cachedSaveBytes,
				itemsBytes = this.cachedItemsBytes,
				carPartsBytes = this.cachedCarPartsBytes
			};
			this.BroadcastMessageToSteamID<SaveMessage>(id, saveMessage, 2, 0);
		}

		public void LoadSave(SaveMessage msg)
		{
			try
			{
				MPController.Instance.StartCoroutine(MPController.Instance.Timer());
				if (File.Exists(this.savePath))
				{
					File.Move(this.savePath, this.savePath + "_bak");
					Logger.Debug("Successfully renamed save.");
					if (File.Exists(this.itemsPath))
					{
						File.Move(this.itemsPath, this.itemsPath + "_bak");
						Logger.Debug("Successfully renamed items.");
					}
					if (File.Exists(this.carPartsPath))
					{
						File.Move(this.carPartsPath, this.carPartsPath + "_bak");
						Logger.Debug("Successfully renamed carparts.");
					}
				}
				File.WriteAllBytes(this.savePath, msg.saveBytes);
				File.WriteAllBytes(this.itemsPath, msg.itemsBytes);
				File.WriteAllBytes(this.carPartsPath, msg.carPartsBytes);
				this.isSaveLoaded = true;
				double num = (double)msg.saveBytes.Length / 1024.0;
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("Error loading received save: " + ex.Message, MessageSeverity.Error);
			}
		}

		public void RefreshSave()
		{
			try
			{
				if (File.Exists(this.savePath + "_bak"))
				{
					if (File.Exists(this.savePath))
					{
						File.Delete(this.savePath);
					}
					File.Move(this.savePath + "_bak", this.savePath);
					if (File.Exists(this.itemsPath + "_bak"))
					{
						if (File.Exists(this.itemsPath))
						{
							File.Delete(this.itemsPath);
						}
						File.Move(this.itemsPath + "_bak", this.itemsPath);
					}
					if (File.Exists(this.carPartsPath + "_bak"))
					{
						if (File.Exists(this.carPartsPath))
						{
							File.Delete(this.carPartsPath);
						}
						File.Move(this.carPartsPath + "_bak", this.carPartsPath);
					}
					Logger.Debug("Successfully got original save back.");
				}
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("Error getting backed up save back: " + ex.Message, MessageSeverity.Error);
			}
		}

		public void SleepRequest(SleepRequest sr)
		{
			Logger.Debug("Requesting sleep on " + ((sr != null) ? sr.name : null));
			this.currentSleepTrigger = sr;
			Chat.Instance.AddMessage("Waiting for other players to go to sleep", MessageSeverity.Warning);
			if (this.IsHost)
			{
				this.localPlayer.sleepRequested = true;
				this.ShouldAllSleep();
				return;
			}
			this.SendChatMsg("requestSleepxxkairavaxardzmaomodivitamashot");
		}

		public void CancelSleepRequest()
		{
			if (this.IsHost)
			{
				this.localPlayer.sleepRequested = false;
				return;
			}
			this.SendChatMsg("requestSleepxxkairavaxardzmaomodivitamashotCANCEL");
		}

		public bool SendChatMsg(string msg)
		{
			NetPlayer netPlayer = null;
			if (msg.Equals(""))
			{
				return false;
			}
			if (msg.StartsWith("/add"))
			{
				foreach (NetPlayer netPlayer2 in this.players)
				{
					try
					{
						if (((netPlayer2 != null) ? netPlayer2.GetName() : null).ToLower().Contains(msg.Split(new char[] { ' ' })[1].ToLower()))
						{
							SteamFriends.ActivateGameOverlayToUser("friendadd", netPlayer2.SteamId);
							return true;
						}
					}
					catch (Exception)
					{
					}
				}
				Chat.Instance.AddMessage("Could not find player to add!", MessageSeverity.Error);
				return true;
			}
			if (msg.StartsWith("/kick") || msg.Contains("/kick"))
			{
				if (!this.IsHost)
				{
					Chat.Instance.AddMessage("You can not kick the host!", MessageSeverity.Error);
					return true;
				}
				using (List<NetPlayer>.Enumerator enumerator = this.players.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetPlayer netPlayer3 = enumerator.Current;
						try
						{
							if (netPlayer3.GetName().ToLower().Contains(msg.Split(new char[] { ' ' })[1].ToLower()))
							{
								msg = "aoisdjasdasdadsasdiosajdoiasjdiosajdoiajnikacodeKICK";
								netPlayer = netPlayer3;
								this.kick = true;
								break;
							}
						}
						catch (Exception)
						{
						}
					}
					goto IL_3A8;
				}
			}
			if (msg.StartsWith("/tp"))
			{
				foreach (NetPlayer netPlayer4 in this.players)
				{
					try
					{
						if (((netPlayer4 != null) ? netPlayer4.GetName() : null).ToLower().Contains(msg.Split(new char[] { ' ' })[1].ToLower()))
						{
							this.GetLocalPlayer().Teleport(netPlayer4.characterGameObject.transform.position, netPlayer4.characterGameObject.transform.rotation);
							return true;
						}
					}
					catch (Exception)
					{
					}
				}
				Chat.Instance.AddMessage("Could not find player to TP to!", MessageSeverity.Error);
				return true;
			}
			if (msg.StartsWith("/givemoney"))
			{
				try
				{
					float num = float.Parse(msg.Split(new char[] { ' ' })[2]);
					if (num < FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value)
					{
						using (List<NetPlayer>.Enumerator enumerator = this.players.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								NetPlayer netPlayer5 = enumerator.Current;
								try
								{
									if (netPlayer5.GetName().ToLower().Contains(msg.Split(new char[] { ' ' })[1].ToLower()))
									{
										ChatMessage chatMessage = new ChatMessage();
										chatMessage.message = "money";
										chatMessage.FloatValue = num;
										FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value -= num;
										this.BroadcastMessageToSteamID<ChatMessage>(netPlayer5.SteamId, chatMessage, 2, 0);
										Chat.Instance.AddMessage(string.Format("You gave {0:n0}mk to {1}", num, netPlayer5.GetName()), MessageSeverity.Info);
										break;
									}
								}
								catch (Exception)
								{
								}
							}
							goto IL_36D;
						}
					}
					Chat.Instance.AddMessage(string.Format("You don't have enough money to give {0:n0}mk, you only have {1}", num, FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value), MessageSeverity.Warning);
					IL_36D:;
				}
				catch (Exception)
				{
					Chat.Instance.AddMessage("Wrong format, do: /givemoney [name] [amount]", MessageSeverity.Warning);
				}
				return true;
			}
			if (msg.Equals("sleepAcceptedbyHostdidebaufalsyoveltvisdaujereshentavs"))
			{
				this.currentSleepTrigger.Allowed = true;
				this.currentSleepTrigger.GetPositions();
			}
			IL_3A8:
			ChatMessage chatMessage2 = new ChatMessage();
			chatMessage2.message = msg;
			chatMessage2.clock = this.GetNetworkClock();
			if (!this.kick && !msg.StartsWith("aoisdjasdasdadsasdiosajdoiasjdiosajdoiajnikacodeDELUXE") && !msg.StartsWith("requestSleepxxkairavaxardzmaomodivitamashot") && !msg.Equals("sleepAcceptedbyHostdidebaufalsyoveltvisdaujereshentavs"))
			{
				string text = this.localPlayer.GetName();
				if (this.localPlayer.GetName().Length > 12)
				{
					text = this.localPlayer.GetName().Substring(0, 12);
				}
				Chat.Instance.AddMessage(((MPController.Instance.AccessLevel == 4) ? text.ToOrange() : text) + ": " + chatMessage2.message, MessageSeverity.Chat);
				this.kick = false;
			}
			if (this.kick)
			{
				this.SendMessage<ChatMessage>(netPlayer, chatMessage2, 2, 0);
				this.kick = false;
			}
			else if (msg.StartsWith("requestSleepxxkairavaxardzmaomodivitamashot"))
			{
				this.SendMessage<ChatMessage>(this.players[0], chatMessage2, 2, 0);
			}
			else
			{
				foreach (NetPlayer netPlayer6 in this.players)
				{
					this.SendMessage<ChatMessage>(netPlayer6, chatMessage2, 2, 0);
				}
			}
			return false;
		}

		private void OnLobbyCreated(LobbyCreated_t result, bool ioFailure)
		{
			if (result.m_eResult != 1 || ioFailure)
			{
				Logger.Info(string.Format("Failed to create lobby. (result: {0}, io failure: {1})", result.m_eResult, ioFailure));
				MPController.Instance.ShowMessageBox(string.Format("Failed to create lobby due to steam error.\n{0}/{1}", result.m_eResult, ioFailure));
				return;
			}
			Logger.Debug(string.Format("Lobby has been created, lobby id: {0}", result.m_ulSteamIDLobby));
			Chat.Instance.AddMessage("Session started.", MessageSeverity.Info);
			Chat.Instance.AddMessage("Press ` to open Console & type 'help' for useful commands.", MessageSeverity.Info);
			Chat.Instance.AddMessage("Attention: Do not interact with anything before others join!", MessageSeverity.Warning);
			this.localPlayer = new NetLocalPlayer(this, this.netWorld, SteamUser.GetSteamID());
			this.mode = NetManager.Mode.Host;
			this.state = NetManager.State.Playing;
			this.currentLobbyId = new CSteamID(result.m_ulSteamIDLobby);
			SteamMatchmaking.SetLobbyType(this.currentLobbyId, 2);
			SteamMatchmaking.SetLobbyJoinable(this.currentLobbyId, true);
			SteamMatchmaking.SetLobbyData(this.currentLobbyId, "ownerid", this.Public ? SteamUser.GetSteamID().ToString() : "private");
			SteamMatchmaking.SetLobbyData(this.currentLobbyId, "channel", "mwco");
			SteamMatchmaking.SetLobbyData(this.currentLobbyId, "ownername", this.localPlayer.GetName());
			SteamMatchmaking.SetLobbyData(this.currentLobbyId, "region", SteamUtils.GetIPCountry());
			SteamMatchmaking.SetLobbyData(this.currentLobbyId, "version", Client.ModVersion + (MPController.Instance.PREVIEW_BUILD ? "-prev" : ""));
			SteamMatchmaking.SetLobbyData(this.currentLobbyId, "gamebuildid", MPController.Instance.GAME_BUILD_ID.ToString());
			MPController.Instance.UpdateLobbyInfo(-1);
		}

		public bool FriendHasLobby(CSteamID friendSteamId)
		{
			int friendCount = SteamFriends.GetFriendCount(4);
			for (int i = 0; i < friendCount; i++)
			{
				CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, 4);
				FriendGameInfo_t friendGameInfo_t;
				if (SteamFriends.GetFriendGamePlayed(friendByIndex, ref friendGameInfo_t) && friendGameInfo_t.m_steamIDLobby.IsValid() && friendByIndex == friendSteamId)
				{
					return true;
				}
			}
			return false;
		}

		public void FindLobbies()
		{
			SteamMatchmaking.AddRequestLobbyListStringFilter("channel", "mwco", 0);
			SteamAPICall_t steamAPICall_t = SteamMatchmaking.RequestLobbyList();
			if (steamAPICall_t == SteamAPICall_t.Invalid)
			{
				Logger.Error("Unable to request lobby list.");
				MPController.Instance.ShowMessageBox("Failed  to request lobby list.");
				return;
			}
			this.m_CallResultLobbyMatchList.Set(steamAPICall_t, null);
		}

		private void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
		{
			uint nLobbiesMatching = result.m_nLobbiesMatching;
			this.Lobbies.Clear();
			int num = 0;
			while ((long)num < (long)((ulong)nLobbiesMatching))
			{
				try
				{
					CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(num);
					LobbyItem lobbyItem = new LobbyItem
					{
						LobbyID = lobbyByIndex,
						OwnerID = SteamMatchmaking.GetLobbyData(lobbyByIndex, "ownerid"),
						Region = SteamMatchmaking.GetLobbyData(lobbyByIndex, "region"),
						Name = SteamMatchmaking.GetLobbyData(lobbyByIndex, "ownername"),
						Version = SteamMatchmaking.GetLobbyData(lobbyByIndex, "version"),
						GameBuildID = SteamMatchmaking.GetLobbyData(lobbyByIndex, "gamebuildid")
					};
					lobbyItem.IsFriend = this.CheckFriend(ulong.Parse(lobbyItem.OwnerID));
					if (lobbyItem.IsFriend)
					{
						this.Lobbies.Insert(0, lobbyItem);
					}
					else
					{
						this.Lobbies.Add(lobbyItem);
					}
				}
				catch (Exception)
				{
				}
				num++;
			}
			MPController.Instance.UpdateLobbies();
		}

		public bool CheckFriend(ulong steamId)
		{
			return SteamFriends.GetFriendRelationship((CSteamID)steamId) == 3;
		}

		public void SavePlayersData()
		{
			new ES2Settings
			{
				saveLocation = 1
			}.filenameData.tag = "MWCO";
			foreach (NetPlayer netPlayer in this.players)
			{
				ulong num = (ulong)netPlayer.SteamId;
				ES2.Save<ulong>(num, string.Format("savefile.txt?tag={0}_PlayerID", num));
				ES2.Save<Vector3>(netPlayer.characterGameObject.transform.position, string.Format("savefile.txt?tag={0}_PlayerPosition", num));
				ES2.Save<float>(netPlayer.GetAttributes(), string.Format("savefile.txt?tag={0}_PlayerAttributes", num));
			}
		}

		private void LoadPlayerData()
		{
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			if (ES2.Exists(string.Format("savefile.txt?tag={0}_PlayerID", steamID)) && ES2.Load<ulong>(string.Format("savefile.txt?tag={0}_PlayerID", steamID)) == steamID)
			{
				DevTools.localPlayer.transform.position = ES2.Load<Vector3>(string.Format("savefile.txt?tag={0}_PlayerPosition", steamID));
				float[] array = ES2.LoadArray<float>(string.Format("savefile.txt?tag={0}_PlayerAttributes", steamID));
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerThirst").Value = array[0];
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerHunger").Value = array[1];
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerStress").Value = array[2];
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerUrine").Value = array[3];
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerFatigue").Value = array[4];
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerDirtiness").Value = array[5];
				Logger.Debug(string.Format("Tried to set attribs {0}. {1},{2},{3},{4}", new object[]
				{
					array.Length,
					array[0],
					array[1],
					array[2],
					array[3]
				}));
			}
		}

		public void GetFriendLobby(CSteamID friendSteamId)
		{
			int friendCount = SteamFriends.GetFriendCount(4);
			for (int i = 0; i < friendCount; i++)
			{
				CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, 4);
				FriendGameInfo_t friendGameInfo_t;
				if (SteamFriends.GetFriendGamePlayed(friendByIndex, ref friendGameInfo_t) && friendGameInfo_t.m_steamIDLobby.IsValid() && friendByIndex == friendSteamId)
				{
					this.JoinFriendLobby(friendGameInfo_t.m_steamIDLobby, friendSteamId);
				}
			}
		}

		public void JoinFriendLobby(CSteamID lobbyId, CSteamID friendId)
		{
			SteamAPICall_t steamAPICall_t = SteamMatchmaking.JoinLobby(lobbyId);
			if (steamAPICall_t == SteamAPICall_t.Invalid)
			{
				Logger.Error(string.Format("Unable to join lobby {0}. JoinLobby call failed.", lobbyId));
				MPController.Instance.ShowMessageBox("Failed to join lobby.\nPlease try again later.");
				return;
			}
			Logger.Debug("Setup player.");
			this.timeSinceLastHeartbeat = 0f;
			this.players.Add(new NetPlayer(this, this.netWorld, friendId));
			this.lobbyEnterCallResult.Set(steamAPICall_t, null);
		}

		private void OnLobbyEnter(LobbyEnter_t result, bool ioFailure)
		{
			if (ioFailure || result.m_EChatRoomEnterResponse != 1U)
			{
				Logger.Error("Failed to join lobby. reponse: " + result.m_EChatRoomEnterResponse.ToString() + "ioFailure: " + ioFailure.ToString());
				if (ioFailure | (result.m_EChatRoomEnterResponse == 4U))
				{
					MPController.Instance.ShowMessageBox("Session is full");
				}
				else if (ioFailure | (result.m_EChatRoomEnterResponse == 2U))
				{
					MPController.Instance.ShowMessageBox("The session you're trying to join doesn't exist.");
				}
				else
				{
					MPController.Instance.ShowMessageBox(string.Format("Failed to join lobby.\n(reponse: {0}, ioFailure: {1})", result.m_EChatRoomEnterResponse, ioFailure));
				}
				this.players.RemoveAt(0);
				return;
			}
			this.localPlayer = new NetLocalPlayer(this, this.netWorld, SteamUser.GetSteamID());
			Logger.Debug("Entered lobby: " + result.m_ulSteamIDLobby.ToString());
			Chat.Instance.AddMessage("Entered session.", MessageSeverity.Info);
			Chat.Instance.AddMessage("Press ` to open Console & type 'help' for useful commands.", MessageSeverity.Info);
			this.mode = NetManager.Mode.Player;
			this.state = NetManager.State.LoadingGameWorld;
			this.currentLobbyId = new CSteamID(result.m_ulSteamIDLobby);
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.currentLobbyId);
			Logger.Debug(string.Format("[NetManager] Players in lobby {0}", numLobbyMembers));
			for (int i = 0; i < numLobbyMembers; i++)
			{
				bool flag = false;
				CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(this.currentLobbyId, i);
				foreach (NetPlayer netPlayer in this.players)
				{
					if (netPlayer != null && netPlayer.SteamId == lobbyMemberByIndex)
					{
						flag = true;
					}
				}
				if (!flag && lobbyMemberByIndex != this.localPlayer.SteamId && lobbyMemberByIndex != this.players[0].SteamId)
				{
					this.players.Add(new NetPlayer(this, this.netWorld, lobbyMemberByIndex));
				}
			}
			this.ShowLoadingScreen(true);
			foreach (NetPlayer netPlayer2 in this.players)
			{
				if (netPlayer2 != null && netPlayer2.SteamId != this.localPlayer.SteamId)
				{
					this.SendHandshake(netPlayer2);
				}
			}
			MPController.Instance.UpdateLobbyInfo(-1);
		}

		private void RegisterProtocolMessagesHandlers()
		{
			this.netMessageHandler.BindMessageHandler<HandshakeMessage>(delegate(CSteamID sender, HandshakeMessage msg)
			{
				this.HandleHandshake(sender, msg);
			});
			this.netMessageHandler.BindMessageHandler<ChatMessage>(delegate(CSteamID sender, ChatMessage msg)
			{
				this.HandleChat(sender, msg);
			});
			this.netMessageHandler.BindMessageHandler<HeartbeatMessage>(delegate(CSteamID sender, HeartbeatMessage msg)
			{
				if (msg == null || this.GetPlayer(sender) == null)
				{
					return;
				}
				this.BroadcastMessageToSteamID<HeartbeatResponseMessage>(sender, new HeartbeatResponseMessage
				{
					clientClock = msg.clientClock,
					clock = this.GetNetworkClock()
				}, 2, 0);
			});
			this.netMessageHandler.BindMessageHandler<HeartbeatResponseMessage>(delegate(CSteamID sender, HeartbeatResponseMessage msg)
			{
				if (!this.IsHost)
				{
					NetPlayer netPlayer = this.players[0];
					if (sender != ((netPlayer != null) ? new CSteamID?(netPlayer.SteamId) : null))
					{
						return;
					}
				}
				this.timeSinceLastHeartbeat = 0f;
				this.remoteClock = msg.clock;
				if (this.IsHost)
				{
					return;
				}
				if (this.pings.Count < 5)
				{
					this.pings.Add((int)(this.GetNetworkClock() - msg.clientClock));
					return;
				}
				int num = 0;
				foreach (int num2 in this.pings)
				{
					num += num2;
				}
				this.pings.Clear();
				this.ping = num / 5;
				MPController.Instance.UpdateLobbyInfo(this.ping);
			});
			this.netMessageHandler.BindMessageHandler<DisconnectMessage>(delegate(CSteamID sender, DisconnectMessage msg)
			{
				this.HandleDisconnect(sender, false);
			});
		}

		public void ShowLoadingScreen(bool show)
		{
			if (Application.loadedLevelName == "MainMenu")
			{
				GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
				GameObject gameObject = null;
				foreach (GameObject gameObject2 in array)
				{
					if (gameObject2.transform.parent == null && gameObject2.name == "Loading")
					{
						gameObject = gameObject2;
						break;
					}
				}
				gameObject.SetActive(show);
			}
		}

		public ulong GetNetworkClock()
		{
			return (ulong)(DateTime.UtcNow - this.netManagerCreationTime).TotalMilliseconds;
		}

		private bool WriteMessage(INetMessage message, MemoryStream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(1836278637U);
			binaryWriter.Write(message.MessageId);
			if (!message.Write(binaryWriter))
			{
				ErrorHandling.Raise("Failed to write network message " + message.MessageId.ToString());
				return false;
			}
			if (DevTools.netStats)
			{
				this.statistics.RecordSendMessage((int)message.MessageId, stream.Length);
			}
			return true;
		}

		public bool BroadcastMessage<T>(T message, EP2PSend sendType, int channel = 0) where T : INetMessage
		{
			MemoryStream memoryStream = new MemoryStream();
			if (!this.WriteMessage(message, memoryStream))
			{
				return false;
			}
			foreach (NetPlayer netPlayer in this.players)
			{
				if (netPlayer != null && (netPlayer.isConnected || message is FullWorldSyncMessage || message is PlayerSyncMessage) && netPlayer != null)
				{
					netPlayer.SendPacket(memoryStream.ToArray(), sendType, channel);
				}
			}
			return true;
		}

		public bool BroadcastMessageToSteamID<T>(CSteamID sender, T message, EP2PSend sendType, int channel = 0) where T : INetMessage
		{
			MemoryStream memoryStream = new MemoryStream();
			if (!this.WriteMessage(message, memoryStream))
			{
				return false;
			}
			foreach (NetPlayer netPlayer in this.players)
			{
				if (netPlayer.SteamId == sender)
				{
					return netPlayer.SendPacket(memoryStream.ToArray(), sendType, channel);
				}
			}
			return true;
		}

		public bool SendMessage<T>(NetPlayer player, T message, EP2PSend sendType, int channel = 0) where T : INetMessage
		{
			if (player == null)
			{
				return false;
			}
			MemoryStream memoryStream = new MemoryStream();
			return this.WriteMessage(message, memoryStream) && player.SendPacket(memoryStream.ToArray(), sendType, channel);
		}

		private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t request)
		{
			SteamAPICall_t steamAPICall_t = SteamMatchmaking.JoinLobby(request.m_steamIDLobby);
			if (steamAPICall_t == SteamAPICall_t.Invalid)
			{
				Logger.Error(string.Format("Unable to join lobby {0}. JoinLobby call failed.", request.m_steamIDLobby));
				MPController.Instance.ShowMessageBox("Failed to join lobby.\nPlease try again later.");
				return;
			}
			string lobbyData = SteamMatchmaking.GetLobbyData(request.m_steamIDLobby, "version");
			if (lobbyData != Client.ModVersion + (MPController.Instance.PREVIEW_BUILD ? "-prev" : ""))
			{
				MPController.Instance.ShowMessageBox(string.Concat(new string[]
				{
					"Version mismatch!\nThe person you are trying to join is on MWCO v",
					lobbyData,
					"\nwhereas you are on MWCO v",
					Client.ModVersion,
					"!"
				}));
				SteamMatchmaking.LeaveLobby(request.m_steamIDLobby);
				return;
			}
			string lobbyData2 = SteamMatchmaking.GetLobbyData(request.m_steamIDLobby, "gamebuildid");
			if (lobbyData2 != "")
			{
				if (int.Parse(lobbyData2) > MPController.Instance.GAME_BUILD_ID)
				{
					MPController.Instance.ShowMessageBox("The person you are trying to join is on a newer version of My Winter Car.\nPlease update your game on Steam!");
					SteamMatchmaking.LeaveLobby(request.m_steamIDLobby);
					return;
				}
				if (int.Parse(lobbyData2) < MPController.Instance.GAME_BUILD_ID)
				{
					MPController.Instance.ShowMessageBox("The person you are trying to join is on an older version of My Winter Car!");
					SteamMatchmaking.LeaveLobby(request.m_steamIDLobby);
					return;
				}
			}
			MPController.Instance.skinCanvas.gameObject.SetActive(false);
			Logger.Debug("Setup player.");
			this.timeSinceLastHeartbeat = 0f;
			this.players.Add(new NetPlayer(this, this.netWorld, request.m_steamIDFriend));
			this.lobbyEnterCallResult.Set(steamAPICall_t, null);
		}

		public bool SetupLobby()
		{
			Logger.Debug("Setting up lobby.");
			this.players.Clear();
			if (this.Public)
			{
				SteamAPICall_t steamAPICall_t = SteamMatchmaking.CreateLobby(2, this.MAX_PLAYERS);
				if (steamAPICall_t == SteamAPICall_t.Invalid)
				{
					Logger.Debug("Unable to create lobby.");
					return false;
				}
				Logger.Debug("Waiting for lobby create reply..");
				this.lobbyCreatedCallResult.Set(steamAPICall_t, null);
				return true;
			}
			else
			{
				if (this.Public)
				{
					return false;
				}
				SteamAPICall_t steamAPICall_t2 = SteamMatchmaking.CreateLobby(1, this.MAX_PLAYERS);
				if (steamAPICall_t2 == SteamAPICall_t.Invalid)
				{
					Logger.Debug("Unable to create lobby.");
					return false;
				}
				Logger.Debug("Waiting for lobby create reply..");
				this.lobbyCreatedCallResult.Set(steamAPICall_t2, null);
				return true;
			}
		}

		private void LeaveLobby()
		{
			if (this.IsOnline)
			{
				SteamMatchmaking.LeaveLobby(this.currentLobbyId);
				if (!this.IsHost && this.players.Count > 0)
				{
					SteamNetworking.CloseP2PSessionWithUser(this.players[0].SteamId);
				}
				this.currentLobbyId = CSteamID.Nil;
				this.mode = NetManager.Mode.None;
				this.state = NetManager.State.Idle;
				this.localPlayer.Dispose();
				this.localPlayer = null;
				Logger.Info("Left lobby.");
				MPController.Instance.UpdateLobbyInfo(-1);
				return;
			}
		}

		public bool InviteToMyLobby(CSteamID invitee)
		{
			return this.IsHost && SteamMatchmaking.InviteUserToLobby(this.currentLobbyId, invitee);
		}

		public bool IsNetworkPlayerConnected()
		{
			return this.players.Count > 0;
		}

		public void CleanupPlayer(NetPlayer player)
		{
			SteamNetworking.CloseP2PSessionWithUser(player.SteamId);
			player.Dispose();
			this.players.Remove(player);
		}

		public void Disconnect()
		{
			this.BroadcastMessage<DisconnectMessage>(new DisconnectMessage(), 2, 0);
			this.LeaveLobby();
		}

		private void HandleDisconnect(CSteamID sender, bool timeout)
		{
			this.ShowLoadingScreen(false);
			if (this.IsHost)
			{
				try
				{
					if (this.kick)
					{
						Chat.Instance.AddMessage("Player " + this.GetPlayer(sender).GetName() + " has been kicked.", MessageSeverity.Error);
						this.CleanupPlayer(this.GetPlayer(sender));
					}
					else if (this.GetPlayer(sender) != null)
					{
						Chat.Instance.AddMessage(string.Concat(new string[]
						{
							"Player ",
							this.GetPlayer(sender).GetName(),
							" ",
							timeout ? "timed out" : "disconnected",
							"."
						}), MessageSeverity.Error);
						this.CleanupPlayer(this.GetPlayer(sender));
					}
				}
				catch (Exception ex)
				{
					string text = "Can Not Proceed with timeout! ";
					Exception ex2 = ex;
					Debug.Log(text + ((ex2 != null) ? ex2.ToString() : null));
				}
			}
			if (!this.IsPlayer || !(sender == this.players[0].SteamId))
			{
				try
				{
					Chat.Instance.AddMessage(string.Concat(new string[]
					{
						"Player ",
						this.GetPlayer(sender).GetName(),
						" ",
						timeout ? "timed out" : "disconnected",
						"."
					}), MessageSeverity.Error);
					this.CleanupPlayer(this.GetPlayer(sender));
				}
				catch (Exception ex3)
				{
					string text2 = "Can Not Proceed with timeout! ";
					Exception ex4 = ex3;
					Debug.Log(text2 + ((ex4 != null) ? ex4.ToString() : null));
				}
				return;
			}
			this.Disconnect();
			MPController.Instance.LoadLevel("MainMenu");
			if (timeout)
			{
				MPController.Instance.ShowMessageBox("Session timed out.");
				return;
			}
			if (this.kick)
			{
				MPController.Instance.ShowMessageBox("You have been kicked from the session.");
				this.kick = false;
				return;
			}
			MPController.Instance.ShowMessageBox("Host closed the session.");
		}

		private void UpdateHeartbeat()
		{
			if (!this.IsNetworkPlayerConnected())
			{
				return;
			}
			this.timeSinceLastHeartbeat += Time.deltaTime;
			if (this.timeSinceLastHeartbeat >= 90f)
			{
				if (this.players[0] != null)
				{
					this.HandleDisconnect(this.players[0].SteamId, true);
					return;
				}
			}
			else
			{
				this.timeToSendHeartbeat -= Time.deltaTime;
				if (this.timeToSendHeartbeat <= 0f)
				{
					HeartbeatMessage heartbeatMessage = new HeartbeatMessage();
					heartbeatMessage.clientClock = this.GetNetworkClock();
					if (this.IsHost)
					{
						this.BroadcastMessage<HeartbeatMessage>(heartbeatMessage, 2, 0);
					}
					else
					{
						this.BroadcastMessageToSteamID<HeartbeatMessage>(this.players[0].SteamId, heartbeatMessage, 2, 0);
					}
					this.timeToSendHeartbeat = 0.5f;
				}
			}
		}

		private void ProcessMessages()
		{
			uint num = 0U;
			while (SteamNetworking.IsP2PPacketAvailable(ref num, 0))
			{
				if (num == 0U)
				{
					Logger.Info("Received empty p2p packet");
				}
				else
				{
					byte[] array = new byte[num];
					uint num2 = 0U;
					CSteamID nil = CSteamID.Nil;
					if (!SteamNetworking.ReadP2PPacket(array, num, ref num2, ref nil, 0))
					{
						Logger.Error("Failed to read p2p packet!");
					}
					else if (num2 != num || num2 == 0U)
					{
						Logger.Error("Invalid packet size");
					}
					else
					{
						BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
						if (binaryReader.ReadUInt32() != 1836278637U)
						{
							Logger.Error("The received message was not sent by MWCO network layer.");
						}
						else
						{
							byte b = binaryReader.ReadByte();
							if (DevTools.netStats)
							{
								this.statistics.RecordReceivedMessage((int)b, (long)((ulong)num));
							}
							this.netMessageHandler.ProcessMessage(b, nil, binaryReader);
						}
					}
				}
			}
		}

		public void FixedUpdate()
		{
			this.netWorld.FixedUpdate();
		}

		public void Update()
		{
			this.framecounter += 1f;
			if (DevTools.netStats)
			{
				this.statistics.NewFrame();
			}
			this.netWorld.Update();
			if (!this.IsOnline)
			{
				return;
			}
			this.UpdateHeartbeat();
			this.ProcessMessages();
			if ((Input.GetKeyDown(289) && this.players[0] != null) || (this.isdown && this.players[0] != null))
			{
				this.GetLocalPlayer().Teleport(this.players[0].characterGameObject.transform.position, this.players[0].characterGameObject.transform.rotation);
				this.isdown = false;
			}
			foreach (NetPlayer netPlayer in this.players)
			{
				if (netPlayer != null)
				{
					netPlayer.Update();
				}
			}
			NetPlayer netPlayer2 = this.localPlayer;
			if (netPlayer2 == null)
			{
				return;
			}
			netPlayer2.Update();
		}

		public void DrawDebugGUI()
		{
			this.statistics.Draw();
			this.netWorld.UpdateIMGUI();
		}

		public void DrawNameTags()
		{
			foreach (NetPlayer netPlayer in this.players)
			{
				try
				{
					if (netPlayer != null)
					{
						netPlayer.DrawNametag();
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void RejectPlayer(string reason, NetPlayer plr)
		{
			Chat.Instance.AddMessage("Player " + plr.GetName() + " connection rejected. " + reason, MessageSeverity.Error);
			Logger.Error("Player rejected. " + reason);
			this.SendHandshake(plr);
			this.CleanupPlayer(plr);
		}

		public void AbortJoining(string reason)
		{
			this.LeaveLobby();
			string text = "Failed to join lobby.\n" + reason;
			MPController.Instance.ShowMessageBox(text);
			Logger.Error(text);
			MPController.Instance.LoadLevel("MainMenu");
		}

		private void HandleHandshake(CSteamID senderSteamId, HandshakeMessage msg)
		{
			Logger.Debug(string.Format("[NetManager] Received handshake from ({0})", senderSteamId));
			if (this.IsHost)
			{
				try
				{
					foreach (NetPlayer netPlayer in this.players)
					{
						if (netPlayer != null && senderSteamId == netPlayer.SteamId && senderSteamId != this.localPlayer.SteamId)
						{
							Logger.Debug("Received handshake from player but player is already here.");
							this.LeaveLobby();
							return;
						}
					}
					this.timeSinceLastHeartbeat = 0f;
					NetPlayer netPlayer2 = new NetPlayer(this, this.netWorld, senderSteamId);
					if (msg.modVersion != Client.ModVersion)
					{
						this.RejectPlayer("Mod version mismatch.", netPlayer2);
						return;
					}
					if (!netPlayer2.IsSpawned)
					{
						this.SendHandshake(netPlayer2);
						netPlayer2.Spawn();
						netPlayer2.SetSkin(msg.skinCode);
						Chat.Instance.AddMessage("Player " + netPlayer2.GetName() + " joined.", MessageSeverity.Player);
					}
					this.remoteClock = msg.clock;
					netPlayer2.hasHandshake = true;
					this.players.Add(netPlayer2);
					this.SendSave(senderSteamId);
					if (MPController.Instance.AccessLevel >= 4)
					{
						this.SendChatMsg("aoisdjasdasdadsasdiosajdoiasjdiosajdoiajnikacodeDELUXE");
					}
					return;
				}
				catch (Exception ex)
				{
					Logger.Error("Host handshake error: " + ex.Message);
					return;
				}
			}
			try
			{
				if (this.players.Count == 0)
				{
					Logger.Debug("Received handshake from host but host is not here.");
					this.LeaveLobby();
				}
				else if (msg.modVersion != Client.ModVersion)
				{
					this.AbortJoining(string.Concat(new string[]
					{
						"Host is using a different version (",
						msg.modVersion,
						")\nYou are on: (",
						Client.ModVersion,
						")"
					}));
				}
				else
				{
					bool flag = false;
					foreach (NetPlayer netPlayer3 in this.players)
					{
						if (netPlayer3 != null && netPlayer3.SteamId == senderSteamId)
						{
							try
							{
								if (netPlayer3.IsSpawned)
								{
									netPlayer3.SetSkin(msg.skinCode);
								}
							}
							catch (Exception ex2)
							{
								Logger.Debug(string.Format("[NetManager] Failed to set skin for {0} ({1})", netPlayer3.GetName(), ex2));
							}
							flag = true;
						}
					}
					if (!flag && senderSteamId != this.players[0].SteamId)
					{
						NetPlayer netPlayer4 = new NetPlayer(this, this.netWorld, senderSteamId);
						this.SendHandshake(netPlayer4);
						netPlayer4.Spawn();
						netPlayer4.SetSkin(msg.skinCode);
						this.players.Add(netPlayer4);
						Chat.Instance.AddMessage("Player " + netPlayer4.GetName() + " joined.", MessageSeverity.Player);
					}
					if (!this.loaded && !this.isSaveLoaded)
					{
						this.timerIsRunning = true;
						this.timeRemaining = 1f;
					}
				}
			}
			catch (Exception ex3)
			{
				Logger.Error("Player handshake error: " + ex3.Message);
			}
		}

		public void JoinLobby()
		{
			MPController.Instance.LoadLevel("GAME");
			if (MPController.Instance.AccessLevel >= 4)
			{
				this.SendChatMsg("aoisdjasdasdadsasdiosajdoiasjdiosajdoiajnikacodeDELUXE");
			}
		}

		private void HandleChat(CSteamID senderSteamId, ChatMessage msg)
		{
			NetPlayer netPlayer = null;
			foreach (NetPlayer netPlayer2 in this.players)
			{
				try
				{
					if (netPlayer2 != null && netPlayer2.SteamId == senderSteamId)
					{
						netPlayer = netPlayer2;
						break;
					}
				}
				catch (Exception)
				{
				}
			}
			if (msg.message.Equals("aoisdjasdasdadsasdiosajdoiasjdiosajdoiajnikacodeKICK"))
			{
				this.kick = true;
				this.HandleDisconnect(senderSteamId, false);
				return;
			}
			if (msg.message.Equals("aoisdjasdasdadsasdiosajdoiasjdiosajdoiajnikacodeDELUXE"))
			{
				netPlayer.accesslevel = 4;
				return;
			}
			if (msg.message.Equals("requestSleepxxkairavaxardzmaomodivitamashot"))
			{
				if (this.IsHost)
				{
					netPlayer.sleepRequested = true;
					this.ShouldAllSleep();
					Chat.Instance.AddMessage(((netPlayer.accesslevel == 4) ? netPlayer.GetName().ToOrange() : netPlayer.GetName()) + " has requested to sleep.", MessageSeverity.Warning);
					return;
				}
				Chat.Instance.AddMessage(((netPlayer.accesslevel == 4) ? netPlayer.GetName().ToOrange() : netPlayer.GetName()) + " has requested to sleep.", MessageSeverity.Warning);
				return;
			}
			else
			{
				if (msg.message.Equals("requestSleepxxkairavaxardzmaomodivitamashotCANCEL"))
				{
					if (this.IsHost)
					{
						netPlayer.sleepRequested = false;
					}
					return;
				}
				if (msg.message.Equals("sleepAcceptedbyHostdidebaufalsyoveltvisdaujereshentavs"))
				{
					this.currentSleepTrigger.Allowed = true;
					this.currentSleepTrigger.GetPositions();
					Chat.Instance.AddMessage("All players ready to sleep. Good night!", MessageSeverity.Player);
					return;
				}
				string text = netPlayer.GetName();
				if (netPlayer.GetName().Length > 12)
				{
					text = netPlayer.GetName().Substring(0, 12);
				}
				if (msg.HasFloatValue)
				{
					FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value += msg.FloatValue;
					Chat.Instance.AddMessage(((netPlayer.accesslevel == 4) ? text.ToOrange() : text) + string.Format(" gave you {0:n0}mk!", msg.FloatValue), MessageSeverity.Chat);
					return;
				}
				Chat.Instance.AddMessage(((netPlayer.accesslevel == 4) ? text.ToOrange() : text) + ": " + msg.message, MessageSeverity.Chat);
				return;
			}
		}

		private void ShouldAllSleep()
		{
			try
			{
				foreach (NetPlayer netPlayer in this.players)
				{
					if (netPlayer != null && !netPlayer.sleepRequested)
					{
						return;
					}
				}
				if (this.localPlayer.sleepRequested)
				{
					this.SendChatMsg("sleepAcceptedbyHostdidebaufalsyoveltvisdaujereshentavs");
					foreach (NetPlayer netPlayer2 in this.players)
					{
						if (netPlayer2 != null)
						{
							netPlayer2.sleepRequested = false;
						}
					}
					this.localPlayer.sleepRequested = false;
					Chat.Instance.AddMessage("All players ready to sleep. Good night!", MessageSeverity.Player);
				}
			}
			catch
			{
			}
		}

		private void SendHandshake(NetPlayer player)
		{
			HandshakeMessage handshakeMessage = new HandshakeMessage
			{
				modVersion = Client.ModVersion,
				clock = this.GetNetworkClock(),
				skinCode = MPController.Instance.skinCode
			};
			this.SendMessage<HandshakeMessage>(player, handshakeMessage, 2, 0);
			Logger.Debug(string.Format("[NetManager] Sent handshake to {0} ({1})", player.GetName(), player.SteamId));
		}

		public void OnGameWorldLoad()
		{
			this.loaded = true;
			if (!this.IsOnline)
			{
				this.SetupLobby();
				return;
			}
			if (this.IsPlayer)
			{
				this.netWorld.AskForFullWorldSync();
			}
		}

		public void DoSetup()
		{
			if (MPController.Instance.DEVELOPEROPTIONS)
			{
				foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
				{
					if ((gameObject.name.ToLower().Contains("loading") || gameObject.name.ToLower().Contains("msgbox")) && gameObject.name.ToLower().Contains("mscl"))
					{
						Logger.Debug("DELETING OBJ " + gameObject.name);
						Object.Destroy(gameObject);
					}
				}
			}
			if (UserPreferences.optimized == 0)
			{
				this.DestroyObjs(true, false);
				Logger.Info("Optimization done.");
			}
			else if (UserPreferences.optimized == 1)
			{
				this.DestroyObjs(true, true);
				Logger.Info("Optimization done.");
			}
			else if (UserPreferences.optimized == 2)
			{
				this.DestroyObjs(false, true);
				Logger.Info("Optimization done.");
			}
			else if (UserPreferences.optimized == 3)
			{
				this.DestroyObjs(false, false);
				Logger.Info("Optimization done.");
			}
			GameWeatherManager.Instance.SetupWeatherStates();
		}

		public void JoinedSetup()
		{
			try
			{
				if (!this.IsHost)
				{
					this.LoadPlayerData();
				}
				UserPreferences.Start();
				GameVehicleDatabase.Instance.vehiclesHighway.SetActive(true);
				foreach (PlayMakerFSM playMakerFSM in MPController.Instance.boat.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM.gameObject.name == "FPSCamera" && playMakerFSM.FsmName == "Death")
					{
						for (int j = 0; j < playMakerFSM.Fsm.GetState("State 1").Actions.Length - 1; j++)
						{
							playMakerFSM.Fsm.GetState("State 1").Actions[j].Enabled = false;
						}
						break;
					}
				}
				Tool.Instance.LateHookEvents();
			}
			catch (Exception ex)
			{
				string text = "Error on world load: ";
				Exception ex2 = ex;
				Logger.Error(text + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		private void DestroyObjs(bool satsuma, bool opt)
		{
			GameObject[] array = new GameObject[19];
			if (opt)
			{
				array[7] = GameObject.Find("COMBINE(350 - 400psi)");
				array[14] = GameObject.Find("ITEMS/digging bar(itemx)");
				array[17] = GameObject.Find("YARD/Building/Dynamics/KWH Meter");
			}
			GameObject[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Object.Destroy(array2[i]);
			}
		}

		public NetLocalPlayer GetLocalPlayer()
		{
			return (NetLocalPlayer)this.localPlayer;
		}

		public NetPlayer GetPlayer(CSteamID steamId)
		{
			foreach (NetPlayer netPlayer in this.players)
			{
				if (netPlayer != null && netPlayer.SteamId == steamId)
				{
					return netPlayer;
				}
			}
			return null;
		}

		public void OnNetworkWorldLoaded()
		{
			this.state = NetManager.State.Playing;
		}

		public void SpawnPlayers()
		{
			for (int i = 1; i < this.players.Count; i++)
			{
				if (this.players[i] != null)
				{
					this.players[i] = new NetPlayer(this, this.netWorld, this.players[i].SteamId);
					this.SendHandshake(this.players[i]);
					this.players[i].Spawn();
					Chat.Instance.AddMessage("Player " + this.players[i].GetName() + " spawned.", MessageSeverity.Player);
				}
			}
		}

		public void HandshakePlayers()
		{
			for (int i = 1; i < this.players.Count; i++)
			{
				if (this.players[i] != null)
				{
					this.players[i] = new NetPlayer(this, this.netWorld, this.players[i].SteamId);
					this.SendHandshake(this.players[i]);
					Logger.Debug(string.Format("[NetManager] Sent handshake to {0} ({1})", this.players[i].GetName(), this.players[i].SteamId));
				}
			}
		}

		public bool GetP2PSessionState(out P2PSessionState_t sessionState)
		{
			if (this.players.Count == 0)
			{
				sessionState = default(P2PSessionState_t);
				return false;
			}
			return SteamNetworking.GetP2PSessionState(this.players[0].SteamId, ref sessionState);
		}

		[CompilerGenerated]
		private void <RegisterProtocolMessagesHandlers>b__80_0(CSteamID sender, HandshakeMessage msg)
		{
			this.HandleHandshake(sender, msg);
		}

		[CompilerGenerated]
		private void <RegisterProtocolMessagesHandlers>b__80_1(CSteamID sender, ChatMessage msg)
		{
			this.HandleChat(sender, msg);
		}

		[CompilerGenerated]
		private void <RegisterProtocolMessagesHandlers>b__80_2(CSteamID sender, HeartbeatMessage msg)
		{
			if (msg == null || this.GetPlayer(sender) == null)
			{
				return;
			}
			this.BroadcastMessageToSteamID<HeartbeatResponseMessage>(sender, new HeartbeatResponseMessage
			{
				clientClock = msg.clientClock,
				clock = this.GetNetworkClock()
			}, 2, 0);
		}

		[CompilerGenerated]
		private void <RegisterProtocolMessagesHandlers>b__80_3(CSteamID sender, HeartbeatResponseMessage msg)
		{
			if (!this.IsHost)
			{
				NetPlayer netPlayer = this.players[0];
				if (sender != ((netPlayer != null) ? new CSteamID?(netPlayer.SteamId) : null))
				{
					return;
				}
			}
			this.timeSinceLastHeartbeat = 0f;
			this.remoteClock = msg.clock;
			if (this.IsHost)
			{
				return;
			}
			if (this.pings.Count < 5)
			{
				this.pings.Add((int)(this.GetNetworkClock() - msg.clientClock));
				return;
			}
			int num = 0;
			foreach (int num2 in this.pings)
			{
				num += num2;
			}
			this.pings.Clear();
			this.ping = num / 5;
			MPController.Instance.UpdateLobbyInfo(this.ping);
		}

		[CompilerGenerated]
		private void <RegisterProtocolMessagesHandlers>b__80_4(CSteamID sender, DisconnectMessage msg)
		{
			this.HandleDisconnect(sender, false);
		}

		public int MAX_PLAYERS = 2;

		private const uint PROTOCOL_ID = 1836278637U;

		public bool isdown;

		public List<LobbyItem> Lobbies = new List<LobbyItem>();

		public bool Public = true;

		private bool kick;

		public string skinCode = "0,0,0,0,0,0";

		public bool loaded;

		private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequestedCallback;

		private Callback<P2PSessionRequest_t> p2pSessionRequestCallback;

		private Callback<P2PSessionConnectFail_t> p2pConnectFailCallback;

		private CallResult<LobbyCreated_t> lobbyCreatedCallResult;

		private CallResult<LobbyEnter_t> lobbyEnterCallResult;

		private CallResult<LobbyMatchList_t> m_CallResultLobbyMatchList;

		private NetManager.State state;

		private NetManager.Mode mode;

		private CSteamID currentLobbyId = CSteamID.Nil;

		public List<NetPlayer> players = new List<NetPlayer>();

		public NetPlayer localPlayer;

		private const float HEARTBEAT_INTERVAL = 0.5f;

		private const float TIMEOUT_TIME = 90f;

		private float timeToSendHeartbeat;

		private float timeSinceLastHeartbeat;

		private ulong remoteClock;

		private string savePath;

		private string itemsPath;

		private string carPartsPath;

		private byte[] saveFiles;

		private byte[] itemsFiles;

		public bool isSaveLoaded;

		public int ping;

		private DateTime netManagerCreationTime;

		private NetWorld netWorld;

		private NetMessageHandler netMessageHandler;

		public float timeRemaining = 1f;

		public bool timerIsRunning;

		public static NetManager Instance;

		private NetStatistics statistics;

		public bool showNameTags = true;

		private DateTime connectionStartedTime;

		private byte[] cachedSaveBytes;

		private byte[] cachedItemsBytes;

		private byte[] cachedCarPartsBytes;

		private byte[] cachedSavefileBytes;

		private SleepRequest currentSleepTrigger;

		private List<int> pings = new List<int>();

		private float framecounter;

		public enum Mode
		{
			None,
			Host,
			Player
		}

		public enum State
		{
			Idle,
			CreatingLobby,
			LoadingGameWorld,
			Playing
		}
	}
}
