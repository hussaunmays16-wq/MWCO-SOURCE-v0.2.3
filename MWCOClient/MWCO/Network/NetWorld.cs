using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MWCO.Game;
using MWCO.Game.Components;
using MWCO.Game.Objects;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Game.Places;
using MWCO.Network.Messages;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Network
{
	internal class NetWorld
	{
		public void Dispose()
		{
			GameCallbacks.onWorldUnload = (GameCallbacks.OnWorldUnload)Delegate.Remove(GameCallbacks.onWorldUnload, new GameCallbacks.OnWorldUnload(this.OnGameWorldUnload));
			GameCallbacks.onWorldLoad = (GameCallbacks.OnWorldLoad)Delegate.Remove(GameCallbacks.onWorldLoad, new GameCallbacks.OnWorldLoad(this.OnGameWorldLoad));
			GameCallbacks.onPlayMakerObjectCreate = (GameCallbacks.OnPlayMakerObjectCreate)Delegate.Remove(GameCallbacks.onPlayMakerObjectCreate, new GameCallbacks.OnPlayMakerObjectCreate(this.OnPlayMakerObjectCreate));
			GameCallbacks.onPlayMakerObjectActivate = (GameCallbacks.OnPlayMakerObjectActivate)Delegate.Remove(GameCallbacks.onPlayMakerObjectActivate, new GameCallbacks.OnPlayMakerObjectActivate(this.onPlayMakerObjectActivate));
			GameCallbacks.onPlayMakerObjectDestroy = (GameCallbacks.OnPlayMakerObjectDestroy)Delegate.Remove(GameCallbacks.onPlayMakerObjectDestroy, new GameCallbacks.OnPlayMakerObjectDestroy(this.onPlayMakerObjectDestroy));
			GameCallbacks.onPlayMakerSetPosition = (GameCallbacks.OnPlayMakerSetPosition)Delegate.Remove(GameCallbacks.onPlayMakerSetPosition, new GameCallbacks.OnPlayMakerSetPosition(this.onPlayMakerSetPosition));
		}

		public NetWorld(NetManager netManager)
		{
			this.netManager = netManager;
			NetWorld.Instance = this;
			GameCallbacks.onWorldUnload = (GameCallbacks.OnWorldUnload)Delegate.Combine(GameCallbacks.onWorldUnload, new GameCallbacks.OnWorldUnload(this.OnGameWorldUnload));
			GameCallbacks.onWorldLoad = (GameCallbacks.OnWorldLoad)Delegate.Combine(GameCallbacks.onWorldLoad, new GameCallbacks.OnWorldLoad(this.OnGameWorldLoad));
			GameCallbacks.onPlayMakerObjectCreate = (GameCallbacks.OnPlayMakerObjectCreate)Delegate.Combine(GameCallbacks.onPlayMakerObjectCreate, new GameCallbacks.OnPlayMakerObjectCreate(this.OnPlayMakerObjectCreate));
			GameCallbacks.onPlayMakerObjectActivate = (GameCallbacks.OnPlayMakerObjectActivate)Delegate.Combine(GameCallbacks.onPlayMakerObjectActivate, new GameCallbacks.OnPlayMakerObjectActivate(this.onPlayMakerObjectActivate));
			GameCallbacks.onPlayMakerObjectDestroy = (GameCallbacks.OnPlayMakerObjectDestroy)Delegate.Combine(GameCallbacks.onPlayMakerObjectDestroy, new GameCallbacks.OnPlayMakerObjectDestroy(this.onPlayMakerObjectDestroy));
			GameCallbacks.onPlayMakerSetPosition = (GameCallbacks.OnPlayMakerSetPosition)Delegate.Combine(GameCallbacks.onPlayMakerSetPosition, new GameCallbacks.OnPlayMakerSetPosition(this.onPlayMakerSetPosition));
			this.RegisterNetworkMessagesHandlers(netManager.MessageHandler);
		}

		private void OnPlayMakerObjectCreate(GameObject instance, GameObject prefab)
		{
			instance.AddComponent<InstanceDebugger>();
		}

		private void onPlayMakerObjectActivate(GameObject instance, bool activate)
		{
			if (this.playerIsLoading)
			{
				return;
			}
			if (activate == instance.activeSelf)
			{
				return;
			}
			if (!GamePickupableDatabase.IsPickupable(instance))
			{
				return;
			}
			ObjectSyncComponent pickupableByGameObject = this.GetPickupableByGameObject(instance);
			if (pickupableByGameObject == null)
			{
				return;
			}
			if (activate)
			{
				PickupableMetaDataComponent component = pickupableByGameObject.gameObject.GetComponent<PickupableMetaDataComponent>();
				PickupableSpawnMessage pickupableSpawnMessage = new PickupableSpawnMessage();
				pickupableSpawnMessage.id = pickupableByGameObject.ObjectID;
				pickupableSpawnMessage.prefabId = component.prefabId;
				pickupableSpawnMessage.transform.position = Utils.GameVec3ToNet(instance.transform.position);
				pickupableSpawnMessage.transform.rotation = Utils.GameQuatToNet(instance.transform.rotation);
				this.netManager.BroadcastMessage<PickupableSpawnMessage>(pickupableSpawnMessage, 2, 0);
				return;
			}
			PickupableActivateMessage pickupableActivateMessage = new PickupableActivateMessage();
			pickupableActivateMessage.id = pickupableByGameObject.ObjectID;
			pickupableActivateMessage.activate = false;
			this.netManager.BroadcastMessage<PickupableActivateMessage>(pickupableActivateMessage, 2, 0);
		}

		private void onPlayMakerObjectDestroy(GameObject instance)
		{
			if (!GamePickupableDatabase.IsPickupable(instance))
			{
				return;
			}
			if (this.GetPickupableByGameObject(instance) == null)
			{
				Logger.Debug("Pickupable " + instance.name + " has been destroyed however it is not registered, skipping removal.");
				return;
			}
			this.HandlePickupableDestroy(instance);
		}

		private void onPlayMakerSetPosition(GameObject gameObject, Vector3 position, Space space)
		{
			if (!GamePickupableDatabase.IsPickupable(gameObject))
			{
				return;
			}
			ObjectSyncComponent pickupableByGameObject = this.GetPickupableByGameObject(gameObject);
			if (pickupableByGameObject == null)
			{
				return;
			}
			if (space == 1)
			{
				position += gameObject.transform.position;
			}
			PickupableSetPositionMessage pickupableSetPositionMessage = new PickupableSetPositionMessage();
			pickupableSetPositionMessage.id = pickupableByGameObject.ObjectID;
			pickupableSetPositionMessage.position = Utils.GameVec3ToNet(position);
			this.netManager.BroadcastMessage<PickupableSetPositionMessage>(pickupableSetPositionMessage, 2, 0);
		}

		private void RegisterNetworkMessagesHandlers(NetMessageHandler netMessageHandler)
		{
			netMessageHandler.BindMessageHandler<PickupableSetPositionMessage>(delegate(CSteamID sender, PickupableSetPositionMessage msg)
			{
				try
				{
					GameObject gameObject = ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject;
					if (!gameObject.name.ToLower().StartsWith("motor"))
					{
						gameObject.transform.position = Utils.NetVec3ToGame(msg.position);
					}
				}
				catch (Exception)
				{
					Chat.Instance.AddMessage("Can not transfer gameObject position! Crash Avoided But It May Become Buggy (NetWorld-188)", MessageSeverity.Error);
				}
			});
			netMessageHandler.BindMessageHandler<PickupableActivateMessage>(delegate(CSteamID sender, PickupableActivateMessage msg)
			{
				GameObject gameObject2 = null;
				if (ObjectSyncManager.Instance.ObjectIDs.ContainsKey(msg.id))
				{
					try
					{
						gameObject2 = ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject;
					}
					catch (Exception)
					{
						Chat.Instance.AddMessage("Can not grab OSM ObjectID " + msg.id.ToString() + "! Crash Avoided But It May Become Buggy (NetWorld-196)", MessageSeverity.Error);
					}
				}
				if (gameObject2 == null)
				{
					Chat.Instance.AddMessage("Tried to activate pickupable but its not spawned! Crash Avoided But It May Become Buggy (NetWorld-200)", MessageSeverity.Error);
				}
				if (msg.activate)
				{
					gameObject2.SetActive(true);
					return;
				}
				if (gameObject2 != null)
				{
					gameObject2.SetActive(false);
				}
			});
			netMessageHandler.BindMessageHandler<PickupableSpawnMessage>(delegate(CSteamID sender, PickupableSpawnMessage msg)
			{
				this.SpawnPickupable(msg);
			});
			netMessageHandler.BindMessageHandler<PickupableDestroyMessage>(delegate(CSteamID sender, PickupableDestroyMessage msg)
			{
				if (!ObjectSyncManager.Instance.ObjectIDs.ContainsKey(msg.id))
				{
					return;
				}
				try
				{
					GameObject gameObject3 = ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject;
				}
				catch
				{
					Logger.Error(string.Format("Failed to remove object: OSC found but can't get GameObject [{0}]", msg.id));
					return;
				}
				Object.Destroy(ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject);
			});
			netMessageHandler.BindMessageHandler<PlayerSyncMessage>(delegate(CSteamID sender, PlayerSyncMessage msg)
			{
				NetPlayer player = this.netManager.GetPlayer(sender);
				if (player == null)
				{
					Logger.Debug(string.Format("Received synchronization packet from {0} but there is not player registered using this id.", sender));
					return;
				}
				if (!player.isConnected)
				{
					for (int i = 0; i < this.netManager.players.Count; i++)
					{
						NetPlayer netPlayer = this.netManager.players[i];
						if (netPlayer != null && netPlayer.SteamId == sender)
						{
							this.netManager.players[i].isConnected = true;
						}
					}
				}
				player.HandleSynchronize(msg);
			});
			netMessageHandler.BindMessageHandler<AnimSyncMessage>(delegate(CSteamID sender, AnimSyncMessage msg)
			{
				NetPlayer player2 = this.netManager.GetPlayer(sender);
				if (player2 == null)
				{
					Logger.Debug(string.Format("Received animation synchronization packet from {0} but there is not player registered using this id.", sender));
					return;
				}
				player2.HandleAnimSynchronize(msg);
			});
			netMessageHandler.BindMessageHandler<OpenDoorsMessage>(delegate(CSteamID sender, OpenDoorsMessage msg)
			{
				if (this.netManager.GetPlayer(sender) == null)
				{
					Logger.Info(string.Format("Received OpenDoorsMessage however there is no matching player {0}! (open: {1}", sender, msg.open));
					return;
				}
				GameDoor gameDoor = GameDoorsManager.Instance.FindGameDoors(Utils.NetVec3ToGame(msg.position));
				if (gameDoor == null)
				{
					Logger.Info("Player tried to open door, however, the door could not be found!");
					return;
				}
				gameDoor.Open(msg.open);
			});
			netMessageHandler.BindMessageHandler<FullWorldSyncMessage>(delegate(CSteamID sender, FullWorldSyncMessage msg)
			{
				Logger.Debug("[NetWorld] Received full world sync, enqueueing sync task.");
				ErrorHandling.Assert(this.netManager.GetPlayer(sender) != null, string.Format("There is no player matching given steam id {0}.", sender));
				if (NetManager.Instance.players[0].IsSpawned)
				{
					Logger.Debug("[NetWorld] Rejected world sync message as host is not spawned");
					return;
				}
				this.FWSQueued = true;
				this.queuedFWSMessage = msg;
				this.queuedFWSsender = sender;
				this.netManager.OnNetworkWorldLoaded();
			});
			netMessageHandler.BindMessageHandler<SaveMessage>(delegate(CSteamID sender, SaveMessage msg)
			{
				if (this.netManager.IsHost)
				{
					return;
				}
				this.netManager.LoadSave(msg);
			});
			netMessageHandler.BindMessageHandler<AskForWorldStateMessage>(delegate(CSteamID sender, AskForWorldStateMessage msg)
			{
				if (this.netManager.IsPlayer)
				{
					return;
				}
				FullWorldSyncMessage fullWorldSyncMessage = new FullWorldSyncMessage();
				this.WriteFullWorldSync(fullWorldSyncMessage);
				this.netManager.BroadcastMessageToSteamID<FullWorldSyncMessage>(sender, fullWorldSyncMessage, 2, 0);
			});
			netMessageHandler.BindMessageHandler<VehicleEnterMessage>(delegate(CSteamID sender, VehicleEnterMessage msg)
			{
				NetPlayer player3 = this.netManager.GetPlayer(sender);
				if (player3 == null)
				{
					Logger.Error(string.Format("Steam user of id {0} send message however there is no active player matching this id.", sender));
					return;
				}
				ObjectSyncComponent objectSyncComponent = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
				if (objectSyncComponent == null)
				{
					Logger.Error(string.Concat(new string[]
					{
						"Player ",
						player3.SteamId.ToString(),
						" tried to enter vehicle with Object ID ",
						msg.objectID.ToString(),
						" but there is no vehicle with such id."
					}));
					return;
				}
				player3.EnterVehicle(objectSyncComponent, msg.passenger);
			});
			netMessageHandler.BindMessageHandler<VehicleLeaveMessage>(delegate(CSteamID sender, VehicleLeaveMessage msg)
			{
				NetPlayer player4 = this.netManager.GetPlayer(sender);
				if (player4 == null)
				{
					Logger.Error(string.Format("Steam user of id {0} send message however there is no active player matching this id.", sender));
					return;
				}
				player4.LeaveVehicle();
			});
			netMessageHandler.BindMessageHandler<VehicleStateMessage>(delegate(CSteamID sender, VehicleStateMessage msg)
			{
				float num = -1f;
				if (msg.state == "WOODBREAK")
				{
					ObjectSyncManager.Instance.ObjectIDs[msg.objectID].gameObject.GetComponent<PlayMakerFSM>().SendEvent("MP_State 2");
					return;
				}
				if (msg.state == "DESTROYWOOD")
				{
					PlayerVehicle playerVehicle = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					playerVehicle.isLogRelated = true;
					playerVehicle.LogTriggerFSM.SendEvent("MP_Destroy Wood");
					return;
				}
				if (msg.state == "DESTROYLOG")
				{
					PlayerVehicle playerVehicle2 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					playerVehicle2.isLogRelated = true;
					playerVehicle2.LogTriggerFSM.SendEvent("MP_Destroy Log");
					return;
				}
				ObjectSyncComponent objectSyncComponent2 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
				if (objectSyncComponent2 == null)
				{
					Logger.Info("Remote player tried to set state of vehicle " + msg.objectID.ToString() + " but there is no vehicle with such id.");
					return;
				}
				if (msg.HasStartTime)
				{
					num = msg.StartTime;
				}
				(objectSyncComponent2.GetObjectSubtype() as PlayerVehicle).SetEngineState(msg.state, num, msg.ignition);
			});
			netMessageHandler.BindMessageHandler<VehicleSwitchMessage>(delegate(CSteamID sender, VehicleSwitchMessage msg)
			{
				float num2 = -1f;
				string text = "";
				try
				{
					PlayerVehicle playerVehicle3 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					if (playerVehicle3 == null)
					{
						Logger.Info("Remote player tried to change a switch in vehicle " + msg.objectID.ToString() + " but there is no vehicle with such id.");
					}
					else
					{
						if (msg.HasSwitchValueFloat)
						{
							num2 = msg.SwitchValueFloat;
						}
						if (msg.HasSwitchValueString)
						{
							text = msg.SwitchValueString;
						}
						playerVehicle3.SetVehicleSwitch((PlayerVehicle.SwitchIDs)msg.switchID, msg.switchValue, num2, text);
					}
				}
				catch (Exception)
				{
				}
			});
			netMessageHandler.BindMessageHandler<HouseSwitchMessage>(delegate(CSteamID sender, HouseSwitchMessage msg)
			{
				float num3 = -1f;
				try
				{
					PlayerHouse playerHouse = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerHouse;
					if (playerHouse == null)
					{
						if (ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as Pickupable != null)
						{
							if (msg.HasSwitchValueFloat)
							{
								num3 = msg.SwitchValueFloat;
								Tool.Instance.SetHoistAngle(num3, msg.switchValue);
							}
							else
							{
								Tool.Instance.receiver = true;
								Tool.Instance.hoistFsm.SendEvent("MP_Down");
							}
							Logger.Debug(string.Format("Setting angle to {0}", num3));
						}
						else
						{
							Logger.Info("Remote player tried to change a switch in House " + msg.objectID.ToString() + " but there is no House with such id.");
						}
					}
					else
					{
						if (msg.HasSwitchValueFloat)
						{
							num3 = msg.SwitchValueFloat;
						}
						playerHouse.SetHouseSwitch((PlayerHouse.SwitchIDs)msg.switchID, msg.switchValue, num3);
					}
				}
				catch (Exception)
				{
				}
			});
			netMessageHandler.BindMessageHandler<FSMValueChangeMessage>(delegate(CSteamID sender, FSMValueChangeMessage msg)
			{
				try
				{
					if (msg.objectID > 50000)
					{
						if (msg.HasNewBool)
						{
							if (msg.objectID == 54500)
							{
								GameHouseDatabase.Instance.ReadPhoneFlip(int.Parse(msg.fsmValueName), msg.NewBool);
								this.netManager.GetPlayer(sender).HoldPhone(msg.NewBool);
							}
							else if (msg.objectID >= 51000 && msg.objectID <= 51100)
							{
								Peraportti.Instance.ReadCashRegisterMessage(msg.objectID - 51000);
							}
							else if (msg.objectID == 51171)
							{
								GameWorld.Instance.RefreshTime();
							}
							else if (msg.objectID == 67670)
							{
								Factory.Instance.ReadTriggerBoxMessage();
							}
						}
						else if (msg.HasNewFloat)
						{
							if (msg.objectID == 50420)
							{
								Peraportti.Instance.ReceivedPourSync(msg.fsmValueName, msg.NewFloat, false);
							}
							else if (msg.objectID == 50421)
							{
								Peraportti.Instance.ReceivedPourSync(msg.fsmValueName, msg.NewFloat, true);
							}
							else if (msg.objectID == 50100)
							{
								Peraportti.Instance.ReceivedTakeSync(msg.fsmValueName);
								this.netManager.GetPlayer(sender).HoldNozzle(true, msg.fsmValueName);
							}
							else if (msg.objectID == 50150)
							{
								Peraportti.Instance.ReceivedReserveSync(msg.NewFloat);
							}
							else if (msg.objectID == 50250)
							{
								Peraportti.Instance.ReceivedReserveSync(msg.NewFloat);
							}
							else if (msg.objectID == 53001)
							{
								GameHouseDatabase.Instance.SetChannel1Song((int)msg.NewFloat);
							}
							else if (msg.objectID == 54000)
							{
								GameHouseDatabase.Instance.ReadPhoneKey(int.Parse(msg.fsmValueName), msg.NewFloat.ToString());
							}
							else if (msg.objectID == 54100)
							{
								GameHouseDatabase.Instance.ReadRequiredRings(int.Parse(msg.fsmValueName), (int)msg.NewFloat);
							}
							else if (msg.objectID == 67420)
							{
								NetLocalPlayer.Instance.ApplyMoneyChange(msg.NewFloat, this.netManager.GetPlayer(sender).GetName());
							}
							else if (msg.objectID == 67421)
							{
								NetLocalPlayer.Instance.ApplyBankMoneyChange(msg.NewFloat, this.netManager.GetPlayer(sender).GetName());
							}
							else if (msg.objectID == 77000)
							{
								GameWorld.Instance.WorldTime = (float)int.Parse(msg.fsmValueName);
								GameWorld.Instance.WorldDay = (int)msg.NewFloat;
							}
							else if (msg.objectID == 86400)
							{
								GameHouseDatabase.Instance.alarmClocks[(!msg.fsmValueName.Contains("1")) ? 1 : 0].Fsm.GetFsmInt("Setting").Value = (int)msg.NewFloat;
							}
						}
						else if (msg.HasNewString)
						{
						}
					}
					else if (msg.fsmValueName == "Trigger")
					{
						GameVehicleDatabase.Instance.triggerGameObjects[msg.objectID].ReadFluidSyncMessage(msg.NewFloat);
					}
					else
					{
						PlayerVehicle playerVehicle4 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
						if (playerVehicle4 != null)
						{
							if (msg.HasNewFloat)
							{
								playerVehicle4.Fuel = msg.NewFloat;
							}
						}
						else
						{
							Pickupable pickupable = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as Pickupable;
							if (pickupable != null && msg.fsmValueName.StartsWith("Trigger"))
							{
								pickupable.ReadTriggerAssemble(msg.fsmValueName);
							}
						}
					}
				}
				catch (Exception)
				{
				}
			});
			netMessageHandler.BindMessageHandler<SatsumaSwitchMessage>(delegate(CSteamID sender, SatsumaSwitchMessage msg)
			{
				try
				{
					if (msg.HasTriggerID)
					{
						GameVehicleDatabase.Instance.triggerGameObjects[msg.TriggerID].ReadTriggerMessage(msg.PartObjectID, msg.objectID);
					}
					else if (msg.HasTighten)
					{
						GameVehicleDatabase.Instance.bolts[msg.objectID].ReadTightenMessage(msg.Tighten);
					}
				}
				catch (Exception ex)
				{
					Logger.Debug(string.Format("[NetWorld] Rivett message error on {0}: {1}", msg.objectID, ex));
				}
			});
			netMessageHandler.BindMessageHandler<LightSwitchMessage>(delegate(CSteamID sender, LightSwitchMessage msg)
			{
				LightSwitchManager.Instance.FindLightSwitch(Utils.NetVec3ToGame(msg.pos)).TurnOn(msg.toggle);
			});
			netMessageHandler.BindMessageHandler<WeatherUpdateMessage>(delegate(CSteamID sender, WeatherUpdateMessage msg)
			{
				GameWeatherManager.Instance.ReadWeatherSync(msg);
			});
			netMessageHandler.BindMessageHandler<ObjectSyncMessage>(delegate(CSteamID sender, ObjectSyncMessage msg)
			{
				ObjectSyncComponent objectSyncComponent3;
				try
				{
					objectSyncComponent3 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
				}
				catch
				{
					Logger.Info(string.Format("Specified object is not yet added to the ObjectID's Dictionary! (Object ID: {0})", msg.objectID));
					return;
				}
				if (objectSyncComponent3 != null)
				{
					if (msg.HasPickUp)
					{
						if (!msg.PickUp)
						{
							if (NetLocalPlayer.Instance.currentVehicle != null)
							{
								PlayerVehicle playerVehicle5 = NetLocalPlayer.Instance.currentVehicle.GetObjectSubtype() as PlayerVehicle;
								if (playerVehicle5.CurrentDrivingState == PlayerVehicle.DrivingStates.Driver)
								{
									playerVehicle5.bootBox.ClientDroppedPickupable(objectSyncComponent3.ObjectID);
									return;
								}
							}
						}
						else
						{
							objectSyncComponent3.Owner = sender.m_SteamID;
							this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent3.ObjectID, true);
							objectSyncComponent3.SyncTakenByForce();
							objectSyncComponent3.SyncEnabled = false;
						}
						return;
					}
					ObjectSyncManager.SyncTypes syncType = (ObjectSyncManager.SyncTypes)msg.SyncType;
					if (objectSyncComponent3.syncedObject == null)
					{
						objectSyncComponent3.CreateObjectSubtype();
					}
					if (syncType == ObjectSyncManager.SyncTypes.SetOwner)
					{
						objectSyncComponent3.OwnerSetToRemote(sender.m_SteamID);
						this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent3.ObjectID, true);
						if (objectSyncComponent3.Owner == ObjectSyncManager.NO_OWNER || objectSyncComponent3.Owner == sender.m_SteamID)
						{
							objectSyncComponent3.OwnerSetToRemote(sender.m_SteamID);
							this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent3.ObjectID, true);
						}
						else
						{
							Logger.Debug(string.Format("Set owner request rejected for object: {0} (Owner: {1} Sender: {2})", objectSyncComponent3.transform.name, objectSyncComponent3.Owner, sender.m_SteamID));
						}
					}
					else if (syncType == ObjectSyncManager.SyncTypes.RemoveOwner)
					{
						if (objectSyncComponent3.Owner == sender.m_SteamID)
						{
							objectSyncComponent3.OwnerRemoved();
						}
					}
					else if (syncType == ObjectSyncManager.SyncTypes.ForceSetOwner)
					{
						objectSyncComponent3.Owner = sender.m_SteamID;
						this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent3.ObjectID, true);
						objectSyncComponent3.SyncTakenByForce();
						objectSyncComponent3.SyncEnabled = false;
					}
					if (objectSyncComponent3.Owner == sender.m_SteamID || syncType == ObjectSyncManager.SyncTypes.PeriodicSync)
					{
						if (msg.HasSyncedVariables)
						{
							objectSyncComponent3.HandleSyncedVariables(msg.SyncedVariables, sender);
						}
						if (objectSyncComponent3.isAI)
						{
							objectSyncComponent3.ActivateAIVehicle();
						}
						objectSyncComponent3.SetPositionAndRotation(Utils.NetVec3ToGame(msg.position), Utils.NetQuatToGame(msg.rotation));
					}
				}
			});
			netMessageHandler.BindMessageHandler<OrderListMessage>(delegate(CSteamID sender, OrderListMessage msg)
			{
				if (msg.fleetari)
				{
					if (msg.FleetariOrderList != null)
					{
						PlayMakerFSM fleetariList = MPController.Instance.fleetariList;
						for (int j = 0; j < 32; j++)
						{
							fleetariList.Fsm.GetFsmBool(PlayerHouse.fleetariListItems[j]).Value = msg.FleetariOrderList[j];
						}
						GameObject brochure = MPController.Instance.brochure;
						if (brochure == null)
						{
							return;
						}
						brochure.SetActive(false);
						return;
					}
				}
				else if (msg.OrderList != null)
				{
					ArrayList arrayList = new ArrayList();
					foreach (string text2 in msg.OrderList)
					{
						arrayList.Add(text2);
					}
					MPController.Instance.orderList._arrayList = arrayList;
				}
			});
			netMessageHandler.BindMessageHandler<ObjectSyncResponseMessage>(delegate(CSteamID sender, ObjectSyncResponseMessage msg)
			{
				ObjectSyncComponent objectSyncComponent4 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
				if (msg.accepted)
				{
					objectSyncComponent4.SyncEnabled = true;
					objectSyncComponent4.Owner = SteamUser.GetSteamID().m_SteamID;
				}
			});
			netMessageHandler.BindMessageHandler<ObjectSyncRequestMessage>(delegate(CSteamID sender, ObjectSyncRequestMessage msg)
			{
				try
				{
					ObjectSyncManager.Instance.ObjectIDs[msg.objectID].SyncRequestAccepted();
				}
				catch
				{
					Logger.Error(string.Format("Remote client tried to request object sync of an unknown object, Object ID: {0}", msg.objectID));
				}
			});
			netMessageHandler.BindMessageHandler<EventHookSyncMessage>(delegate(CSteamID sender, EventHookSyncMessage msg)
			{
				if (msg.request && !this.netManager.IsPlayer)
				{
					EventHook.SendSync(sender, msg.fsmID);
					return;
				}
				if (msg.HasFsmEventName)
				{
					EventHook.HandleEventSync(msg.fsmID, msg.fsmEventID, msg.FsmEventName);
					return;
				}
				EventHook.HandleEventSync(msg.fsmID, msg.fsmEventID, "none");
			});
		}

		public void Update()
		{
			if (this.FWSQueued && MPController.Instance.boat != null && Application.loadedLevelName == "GAME")
			{
				this.ProcessQueuedFWSMessage();
				this.FWSQueued = false;
			}
			if (!this.netManager.IsPlayer && this.netManager.IsNetworkPlayerConnected())
			{
				bool isOnline = this.netManager.IsOnline;
			}
		}

		private void ProcessQueuedFWSMessage()
		{
			Logger.Debug("[NetWorld] Processing queued message");
			if (this.queuedFWSMessage == null)
			{
				Logger.Debug("[Exception] Expected queued FWS message is null!");
				return;
			}
			if (NetManager.Instance.players[0].IsSpawned)
			{
				Logger.Debug("[Exception] Rejected world sync message as host is not spawned");
				return;
			}
			NetPlayer player = this.netManager.GetPlayer(this.queuedFWSsender);
			this.HandleFullWorldSync(this.queuedFWSMessage);
			if (this.isHere == 0)
			{
				player.Spawn();
				player.SetSkin(this.queuedFWSMessage.hostModel);
				NetManager.Instance.SpawnPlayers();
				this.isHere++;
			}
			player.Teleport(Utils.NetVec3ToGame(this.queuedFWSMessage.spawnPosition), Utils.NetQuatToGame(this.queuedFWSMessage.spawnRotation));
			if (this.queuedFWSMessage.pickedUpObject != 65535)
			{
				player.PickupObject(this.queuedFWSMessage.pickedUpObject);
			}
			Logger.Debug("[NetWorld] Processing queued message complete.");
		}

		public void FixedUpdate()
		{
		}

		public void OnGameWorldLoad()
		{
		}

		private void OnGameWorldUnload()
		{
			ObjectSyncManager.Instance.ObjectIDs.Clear();
		}

		public void WriteFullWorldSync(FullWorldSyncMessage msg)
		{
			Logger.Debug("Writing full world synchronization message.");
			Stopwatch stopwatch = Stopwatch.StartNew();
			this.playerIsLoading = false;
			msg.dayTime = GameWorld.Instance.WorldTime;
			msg.day = GameWorld.Instance.WorldDay + (MPController.Instance.machtwagen.activeSelf ? 100 : 0);
			msg.hostModel = MPController.Instance.skinCode;
			msg.mailboxName = GameWorld.Instance.PlayerLastName;
			List<GameDoor> doors = GameDoorsManager.Instance.doors;
			int count = doors.Count;
			msg.doors = new DoorsInitMessage[count];
			Logger.Debug(string.Format("Writing state of {0} doors.", count));
			for (int i = 0; i < count; i++)
			{
				DoorsInitMessage doorsInitMessage = new DoorsInitMessage();
				GameDoor gameDoor = doors[i];
				doorsInitMessage.position = Utils.GameVec3ToNet(gameDoor.Position);
				msg.doors[i] = doorsInitMessage;
			}
			List<LightSwitch> lightSwitches = LightSwitchManager.Instance.lightSwitches;
			int count2 = lightSwitches.Count;
			msg.lights = new LightSwitchMessage[count2];
			Logger.Debug(string.Format("Writing light switches state of {0}", count2));
			for (int j = 0; j < count2; j++)
			{
				LightSwitchMessage lightSwitchMessage = new LightSwitchMessage();
				LightSwitch lightSwitch = lightSwitches[j];
				lightSwitchMessage.pos = Utils.GameVec3ToNet(lightSwitch.Position);
				lightSwitchMessage.toggle = lightSwitch.SwitchStatus;
				msg.lights[j] = lightSwitchMessage;
			}
			GameWeatherManager.Instance.WriteWeather(msg.currentWeather);
			List<PickupableSpawnMessage> list = new List<PickupableSpawnMessage>();
			List<ModOSCMessage> list2 = new List<ModOSCMessage>();
			Logger.Debug(string.Format("Writing state of {0} objects", ObjectSyncManager.Instance.ObjectIDs.Count));
			foreach (KeyValuePair<int, ObjectSyncComponent> keyValuePair in ObjectSyncManager.Instance.ObjectIDs)
			{
				ObjectSyncComponent value = keyValuePair.Value;
				if (!(value == null))
				{
					if (value.ObjectType != ObjectSyncManager.ObjectTypes.Pickupable)
					{
						if (value.gameObject.GetTopmostParent().name == "RicochetST(1010kg)")
						{
							ModOSCMessage modOSCMessage = new ModOSCMessage
							{
								ObjectName = "RicochetST(1010kg)",
								id = value.ObjectID
							};
							modOSCMessage.transform.position = Utils.GameVec3ToNet(value.transform.position);
							modOSCMessage.transform.rotation = Utils.GameQuatToNet(value.transform.rotation);
							list2.Add(modOSCMessage);
						}
						else if (value.gameObject.GetTopmostParent().name == "Panier250")
						{
							ModOSCMessage modOSCMessage2 = new ModOSCMessage
							{
								ObjectName = "Panier250",
								id = value.ObjectID
							};
							modOSCMessage2.transform.position = Utils.GameVec3ToNet(value.transform.position);
							modOSCMessage2.transform.rotation = Utils.GameQuatToNet(value.transform.rotation);
							list2.Add(modOSCMessage2);
						}
						else if (value.gameObject.GetTopmostParent().name == "SAKER(1350kg)")
						{
							ModOSCMessage modOSCMessage3 = new ModOSCMessage
							{
								ObjectName = "SAKER(1350kg)",
								id = value.ObjectID
							};
							modOSCMessage3.transform.position = Utils.GameVec3ToNet(value.transform.position);
							modOSCMessage3.transform.rotation = Utils.GameQuatToNet(value.transform.rotation);
							list2.Add(modOSCMessage3);
						}
						else if (value.name == "HouseController")
						{
							ModOSCMessage modOSCMessage4 = new ModOSCMessage
							{
								ObjectName = "HouseController",
								id = value.ObjectID
							};
							list2.Add(modOSCMessage4);
						}
					}
					else
					{
						bool flag = true;
						if (!value.gameObject.activeSelf)
						{
							flag = false;
							value.gameObject.SetActive(true);
						}
						PickupableSpawnMessage pickupableSpawnMessage = new PickupableSpawnMessage();
						PickupableMetaDataComponent component = value.gameObject.GetComponent<PickupableMetaDataComponent>();
						ErrorHandling.Assert(component != null && component.PrefabDescriptor != null, "Object with broken meta data -- " + value.gameObject.name + ".");
						pickupableSpawnMessage.prefabId = component.prefabId;
						Transform transform = value.gameObject.transform;
						pickupableSpawnMessage.transform.position = Utils.GameVec3ToNet(transform.position);
						pickupableSpawnMessage.transform.rotation = Utils.GameQuatToNet(transform.rotation);
						try
						{
							if (value.gameObject.name.StartsWith("spark") || value.gameObject.name.StartsWith("oil") || value.gameObject.name.StartsWith("alternator b") || value.gameObject.name.StartsWith("light b"))
							{
								CarPartOld component2 = value.GetComponent<CarPartOld>();
								if (component2.triggerGO.gameObject != null)
								{
									pickupableSpawnMessage.ParentTriggerName = component2.triggerGO.gameObject.name;
								}
							}
						}
						catch (Exception ex)
						{
							Logger.Debug(string.Format("Error checking parent for {0}, {1}", value.name, ex));
						}
						pickupableSpawnMessage.active = value.gameObject.activeSelf;
						pickupableSpawnMessage.id = value.gameObject.GetComponent<ObjectSyncComponent>().ObjectID;
						List<float> list3 = new List<float>();
						if (list3.Count != 0)
						{
							pickupableSpawnMessage.Data = list3.ToArray();
						}
						if (!flag)
						{
							value.gameObject.SetActive(false);
						}
						list.Add(pickupableSpawnMessage);
					}
				}
			}
			msg.pickupables = list.ToArray();
			msg.mods = list2.ToArray();
			this.netManager.GetLocalPlayer().WriteSpawnState(msg);
			stopwatch.Stop();
			Logger.Debug("[NetWorld] World state has been written. Took " + stopwatch.ElapsedMilliseconds.ToString() + "ms");
			this.sentOne++;
		}

		public void HandleFullWorldSync(FullWorldSyncMessage msg)
		{
			if (this.isDone > 0)
			{
				return;
			}
			Logger.Debug("[NetWorld] Handling full world synchronization message.");
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool flag = msg.day >= 100;
			if (flag)
			{
				msg.day -= 100;
			}
			MPController.Instance.machtwagen.SetActive(flag);
			GameWorld.Instance.WorldTime = msg.dayTime;
			GameWorld.Instance.WorldDay = msg.day;
			NetManager.Instance.skinCode = msg.hostModel;
			GameWorld.Instance.PlayerLastName = msg.mailboxName;
			DoorsInitMessage[] doors = msg.doors;
			for (int i = 0; i < doors.Length; i++)
			{
				Vector3 vector = Utils.NetVec3ToGame(doors[i].position);
				ErrorHandling.Assert(GameDoorsManager.Instance.FindGameDoors(vector) != null, string.Format("Unable to find doors at: {0}.", vector));
			}
			foreach (LightSwitchMessage lightSwitchMessage in msg.lights)
			{
				Vector3 vector2 = Utils.NetVec3ToGame(lightSwitchMessage.pos);
				LightSwitch lightSwitch = LightSwitchManager.Instance.FindLightSwitch(vector2);
				ErrorHandling.Assert(lightSwitch != null, string.Format("Unable to find light switch at: {0}.", vector2));
				if (lightSwitch.SwitchStatus != lightSwitchMessage.toggle)
				{
					lightSwitch.TurnOn(lightSwitchMessage.toggle);
				}
			}
			GameWeatherManager.Instance.SetWeather(msg.currentWeather);
			foreach (PickupableSpawnMessage pickupableSpawnMessage in msg.pickupables)
			{
				this.SpawnPickupable(pickupableSpawnMessage);
			}
			foreach (KeyValuePair<int, GameObject> keyValuePair in GamePickupableDatabase.Instance.Pickupables)
			{
				if (keyValuePair.Value.GetComponent<ObjectSyncComponent>() == null)
				{
					Object.Destroy(keyValuePair.Value);
				}
			}
			Logger.Debug("Received mods " + msg.mods.Length.ToString());
			foreach (ModOSCMessage modOSCMessage in msg.mods)
			{
				Logger.Debug("Loaded available mod object " + modOSCMessage.ObjectName);
				MPController.Instance.modsAvailable.Add(modOSCMessage);
			}
			GamePickupableDatabase.Instance.Pickupables.Clear();
			this.playerIsLoading = false;
			stopwatch.Stop();
			Logger.Debug("[NetWorld] Full world synchronization message has been handled. Took " + stopwatch.ElapsedMilliseconds.ToString() + "ms");
			this.isDone++;
		}

		public void AskForFullWorldSync()
		{
			AskForWorldStateMessage askForWorldStateMessage = new AskForWorldStateMessage();
			this.netManager.BroadcastMessageToSteamID<AskForWorldStateMessage>(NetManager.Instance.players[0].SteamId, askForWorldStateMessage, 2, 0);
		}

		public void UpdateIMGUI()
		{
		}

		public GameObject GetPickupableGameObject(int objectID)
		{
			if (ObjectSyncManager.Instance.ObjectIDs.ContainsKey(objectID))
			{
				return ObjectSyncManager.Instance.ObjectIDs[objectID].gameObject;
			}
			return null;
		}

		public ObjectSyncComponent GetPickupableByGameObject(GameObject go)
		{
			foreach (KeyValuePair<int, ObjectSyncComponent> keyValuePair in ObjectSyncManager.Instance.ObjectIDs)
			{
				try
				{
					if (keyValuePair.Value.gameObject == go)
					{
						return keyValuePair.Value;
					}
				}
				catch
				{
				}
			}
			Logger.Error("GetPickupableByGameObject: Couldn't find GameObject!");
			return null;
		}

		public int GetPickupableObjectId(GameObject go)
		{
			foreach (KeyValuePair<int, ObjectSyncComponent> keyValuePair in ObjectSyncManager.Instance.ObjectIDs)
			{
				if (keyValuePair.Value.gameObject == go)
				{
					return keyValuePair.Value.ObjectID;
				}
			}
			return 65535;
		}

		public void HandlePickupableDestroy(GameObject pickupable)
		{
			ObjectSyncComponent pickupableByGameObject = this.GetPickupableByGameObject(pickupable);
			if (pickupableByGameObject != null)
			{
				PickupableDestroyMessage pickupableDestroyMessage = new PickupableDestroyMessage();
				pickupableDestroyMessage.id = pickupableByGameObject.ObjectID;
				this.netManager.BroadcastMessage<PickupableDestroyMessage>(pickupableDestroyMessage, 2, 0);
				Logger.Debug(string.Format("Handle pickupable destroy {0}, Object ID: {1}", pickupable.name, pickupableByGameObject.ObjectID));
				return;
			}
			Logger.Debug("Unhandled pickupable has been destroyed " + pickupable.name);
			Logger.Debug(Environment.StackTrace);
		}

		public void SpawnPickupable(PickupableSpawnMessage msg)
		{
			Vector3 vector = Utils.NetVec3ToGame(msg.transform.position);
			Quaternion quaternion = Utils.NetQuatToGame(msg.transform.rotation);
			if (ObjectSyncManager.Instance.ObjectIDs.ContainsKey(msg.id))
			{
				ObjectSyncComponent objectSyncComponent = ObjectSyncManager.Instance.ObjectIDs[msg.id];
				if (objectSyncComponent.ObjectID == msg.id)
				{
					return;
				}
				GameObject gameObject = objectSyncComponent.gameObject;
				GamePickupableDatabase.Instance.GetPickupablePrefab(msg.prefabId);
				if (gameObject != null)
				{
					PickupableMetaDataComponent component = gameObject.GetComponent<PickupableMetaDataComponent>();
					if (msg.prefabId != component.prefabId)
					{
						bool flag = false;
						foreach (KeyValuePair<int, ObjectSyncComponent> keyValuePair in ObjectSyncManager.Instance.ObjectIDs)
						{
							if (keyValuePair.Value.gameObject.GetComponent<PickupableMetaDataComponent>().prefabId == msg.prefabId)
							{
								gameObject = keyValuePair.Value.gameObject;
								Logger.Debug("Prefab mismatch was resolved.");
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							ErrorHandling.Assert(true, "Prefab ID mismatch couldn't be resolved!");
						}
					}
					gameObject.SetActive(msg.active);
					gameObject.transform.position = vector;
					gameObject.transform.rotation = quaternion;
					if (gameObject.GetComponent<ObjectSyncComponent>() != null)
					{
						Object.Destroy(gameObject.GetComponent<ObjectSyncComponent>());
					}
					gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, msg.id);
					return;
				}
				this.DestroyPickupableLocal(msg.id);
			}
			GameObject gameObject2 = GameWorld.Instance.SpawnPickupable(msg.prefabId, vector, quaternion, msg.id, msg.ParentTriggerName);
			this.RegisterPickupable(gameObject2, true);
			if (gameObject2.GetComponent<ObjectSyncComponent>() != null)
			{
				Object.Destroy(gameObject2.GetComponent<ObjectSyncComponent>());
			}
			gameObject2.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, msg.id);
		}

		private void DestroyPickupableLocal(int id)
		{
			if (!ObjectSyncManager.Instance.ObjectIDs.ContainsKey(id))
			{
				return;
			}
			GameObject gameObject = ObjectSyncManager.Instance.ObjectIDs[id].gameObject;
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}

		public void RegisterPickupable(GameObject pickupable, bool remote = false)
		{
			try
			{
				if (pickupable.GetComponent<PickupableMetaDataComponent>() == null)
				{
					Logger.Debug(string.Format("Failed to register pickupable. No meta data found. {0} ({1})", pickupable.name, pickupable.GetInstanceID()));
					pickupable.AddComponent<PickupableMetaDataComponent>();
				}
				Logger.Debug(string.Format("[NetWorld] Registering {0} (instance id: {1})", pickupable.name, pickupable.GetInstanceID()));
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("Registering pickupable ERROR {0}, {1}", pickupable.name, ex));
			}
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_2(CSteamID sender, PickupableSpawnMessage msg)
		{
			this.SpawnPickupable(msg);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_4(CSteamID sender, PlayerSyncMessage msg)
		{
			NetPlayer player = this.netManager.GetPlayer(sender);
			if (player == null)
			{
				Logger.Debug(string.Format("Received synchronization packet from {0} but there is not player registered using this id.", sender));
				return;
			}
			if (!player.isConnected)
			{
				for (int i = 0; i < this.netManager.players.Count; i++)
				{
					NetPlayer netPlayer = this.netManager.players[i];
					if (netPlayer != null && netPlayer.SteamId == sender)
					{
						this.netManager.players[i].isConnected = true;
					}
				}
			}
			player.HandleSynchronize(msg);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_5(CSteamID sender, AnimSyncMessage msg)
		{
			NetPlayer player = this.netManager.GetPlayer(sender);
			if (player == null)
			{
				Logger.Debug(string.Format("Received animation synchronization packet from {0} but there is not player registered using this id.", sender));
				return;
			}
			player.HandleAnimSynchronize(msg);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_6(CSteamID sender, OpenDoorsMessage msg)
		{
			if (this.netManager.GetPlayer(sender) == null)
			{
				Logger.Info(string.Format("Received OpenDoorsMessage however there is no matching player {0}! (open: {1}", sender, msg.open));
				return;
			}
			GameDoor gameDoor = GameDoorsManager.Instance.FindGameDoors(Utils.NetVec3ToGame(msg.position));
			if (gameDoor == null)
			{
				Logger.Info("Player tried to open door, however, the door could not be found!");
				return;
			}
			gameDoor.Open(msg.open);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_7(CSteamID sender, FullWorldSyncMessage msg)
		{
			Logger.Debug("[NetWorld] Received full world sync, enqueueing sync task.");
			ErrorHandling.Assert(this.netManager.GetPlayer(sender) != null, string.Format("There is no player matching given steam id {0}.", sender));
			if (NetManager.Instance.players[0].IsSpawned)
			{
				Logger.Debug("[NetWorld] Rejected world sync message as host is not spawned");
				return;
			}
			this.FWSQueued = true;
			this.queuedFWSMessage = msg;
			this.queuedFWSsender = sender;
			this.netManager.OnNetworkWorldLoaded();
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_8(CSteamID sender, SaveMessage msg)
		{
			if (this.netManager.IsHost)
			{
				return;
			}
			this.netManager.LoadSave(msg);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_9(CSteamID sender, AskForWorldStateMessage msg)
		{
			if (this.netManager.IsPlayer)
			{
				return;
			}
			FullWorldSyncMessage fullWorldSyncMessage = new FullWorldSyncMessage();
			this.WriteFullWorldSync(fullWorldSyncMessage);
			this.netManager.BroadcastMessageToSteamID<FullWorldSyncMessage>(sender, fullWorldSyncMessage, 2, 0);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_10(CSteamID sender, VehicleEnterMessage msg)
		{
			NetPlayer player = this.netManager.GetPlayer(sender);
			if (player == null)
			{
				Logger.Error(string.Format("Steam user of id {0} send message however there is no active player matching this id.", sender));
				return;
			}
			ObjectSyncComponent objectSyncComponent = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
			if (objectSyncComponent == null)
			{
				Logger.Error(string.Concat(new string[]
				{
					"Player ",
					player.SteamId.ToString(),
					" tried to enter vehicle with Object ID ",
					msg.objectID.ToString(),
					" but there is no vehicle with such id."
				}));
				return;
			}
			player.EnterVehicle(objectSyncComponent, msg.passenger);
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_11(CSteamID sender, VehicleLeaveMessage msg)
		{
			NetPlayer player = this.netManager.GetPlayer(sender);
			if (player == null)
			{
				Logger.Error(string.Format("Steam user of id {0} send message however there is no active player matching this id.", sender));
				return;
			}
			player.LeaveVehicle();
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_15(CSteamID sender, FSMValueChangeMessage msg)
		{
			try
			{
				if (msg.objectID > 50000)
				{
					if (msg.HasNewBool)
					{
						if (msg.objectID == 54500)
						{
							GameHouseDatabase.Instance.ReadPhoneFlip(int.Parse(msg.fsmValueName), msg.NewBool);
							this.netManager.GetPlayer(sender).HoldPhone(msg.NewBool);
						}
						else if (msg.objectID >= 51000 && msg.objectID <= 51100)
						{
							Peraportti.Instance.ReadCashRegisterMessage(msg.objectID - 51000);
						}
						else if (msg.objectID == 51171)
						{
							GameWorld.Instance.RefreshTime();
						}
						else if (msg.objectID == 67670)
						{
							Factory.Instance.ReadTriggerBoxMessage();
						}
					}
					else if (msg.HasNewFloat)
					{
						if (msg.objectID == 50420)
						{
							Peraportti.Instance.ReceivedPourSync(msg.fsmValueName, msg.NewFloat, false);
						}
						else if (msg.objectID == 50421)
						{
							Peraportti.Instance.ReceivedPourSync(msg.fsmValueName, msg.NewFloat, true);
						}
						else if (msg.objectID == 50100)
						{
							Peraportti.Instance.ReceivedTakeSync(msg.fsmValueName);
							this.netManager.GetPlayer(sender).HoldNozzle(true, msg.fsmValueName);
						}
						else if (msg.objectID == 50150)
						{
							Peraportti.Instance.ReceivedReserveSync(msg.NewFloat);
						}
						else if (msg.objectID == 50250)
						{
							Peraportti.Instance.ReceivedReserveSync(msg.NewFloat);
						}
						else if (msg.objectID == 53001)
						{
							GameHouseDatabase.Instance.SetChannel1Song((int)msg.NewFloat);
						}
						else if (msg.objectID == 54000)
						{
							GameHouseDatabase.Instance.ReadPhoneKey(int.Parse(msg.fsmValueName), msg.NewFloat.ToString());
						}
						else if (msg.objectID == 54100)
						{
							GameHouseDatabase.Instance.ReadRequiredRings(int.Parse(msg.fsmValueName), (int)msg.NewFloat);
						}
						else if (msg.objectID == 67420)
						{
							NetLocalPlayer.Instance.ApplyMoneyChange(msg.NewFloat, this.netManager.GetPlayer(sender).GetName());
						}
						else if (msg.objectID == 67421)
						{
							NetLocalPlayer.Instance.ApplyBankMoneyChange(msg.NewFloat, this.netManager.GetPlayer(sender).GetName());
						}
						else if (msg.objectID == 77000)
						{
							GameWorld.Instance.WorldTime = (float)int.Parse(msg.fsmValueName);
							GameWorld.Instance.WorldDay = (int)msg.NewFloat;
						}
						else if (msg.objectID == 86400)
						{
							GameHouseDatabase.Instance.alarmClocks[(!msg.fsmValueName.Contains("1")) ? 1 : 0].Fsm.GetFsmInt("Setting").Value = (int)msg.NewFloat;
						}
					}
					else if (msg.HasNewString)
					{
					}
				}
				else if (msg.fsmValueName == "Trigger")
				{
					GameVehicleDatabase.Instance.triggerGameObjects[msg.objectID].ReadFluidSyncMessage(msg.NewFloat);
				}
				else
				{
					PlayerVehicle playerVehicle = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					if (playerVehicle != null)
					{
						if (msg.HasNewFloat)
						{
							playerVehicle.Fuel = msg.NewFloat;
						}
					}
					else
					{
						Pickupable pickupable = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as Pickupable;
						if (pickupable != null && msg.fsmValueName.StartsWith("Trigger"))
						{
							pickupable.ReadTriggerAssemble(msg.fsmValueName);
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_19(CSteamID sender, ObjectSyncMessage msg)
		{
			ObjectSyncComponent objectSyncComponent;
			try
			{
				objectSyncComponent = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
			}
			catch
			{
				Logger.Info(string.Format("Specified object is not yet added to the ObjectID's Dictionary! (Object ID: {0})", msg.objectID));
				return;
			}
			if (objectSyncComponent != null)
			{
				if (msg.HasPickUp)
				{
					if (!msg.PickUp)
					{
						if (NetLocalPlayer.Instance.currentVehicle != null)
						{
							PlayerVehicle playerVehicle = NetLocalPlayer.Instance.currentVehicle.GetObjectSubtype() as PlayerVehicle;
							if (playerVehicle.CurrentDrivingState == PlayerVehicle.DrivingStates.Driver)
							{
								playerVehicle.bootBox.ClientDroppedPickupable(objectSyncComponent.ObjectID);
								return;
							}
						}
					}
					else
					{
						objectSyncComponent.Owner = sender.m_SteamID;
						this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent.ObjectID, true);
						objectSyncComponent.SyncTakenByForce();
						objectSyncComponent.SyncEnabled = false;
					}
					return;
				}
				ObjectSyncManager.SyncTypes syncType = (ObjectSyncManager.SyncTypes)msg.SyncType;
				if (objectSyncComponent.syncedObject == null)
				{
					objectSyncComponent.CreateObjectSubtype();
				}
				if (syncType == ObjectSyncManager.SyncTypes.SetOwner)
				{
					objectSyncComponent.OwnerSetToRemote(sender.m_SteamID);
					this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent.ObjectID, true);
					if (objectSyncComponent.Owner == ObjectSyncManager.NO_OWNER || objectSyncComponent.Owner == sender.m_SteamID)
					{
						objectSyncComponent.OwnerSetToRemote(sender.m_SteamID);
						this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent.ObjectID, true);
					}
					else
					{
						Logger.Debug(string.Format("Set owner request rejected for object: {0} (Owner: {1} Sender: {2})", objectSyncComponent.transform.name, objectSyncComponent.Owner, sender.m_SteamID));
					}
				}
				else if (syncType == ObjectSyncManager.SyncTypes.RemoveOwner)
				{
					if (objectSyncComponent.Owner == sender.m_SteamID)
					{
						objectSyncComponent.OwnerRemoved();
					}
				}
				else if (syncType == ObjectSyncManager.SyncTypes.ForceSetOwner)
				{
					objectSyncComponent.Owner = sender.m_SteamID;
					this.netManager.GetLocalPlayer().SendObjectSyncResponse(sender, objectSyncComponent.ObjectID, true);
					objectSyncComponent.SyncTakenByForce();
					objectSyncComponent.SyncEnabled = false;
				}
				if (objectSyncComponent.Owner == sender.m_SteamID || syncType == ObjectSyncManager.SyncTypes.PeriodicSync)
				{
					if (msg.HasSyncedVariables)
					{
						objectSyncComponent.HandleSyncedVariables(msg.SyncedVariables, sender);
					}
					if (objectSyncComponent.isAI)
					{
						objectSyncComponent.ActivateAIVehicle();
					}
					objectSyncComponent.SetPositionAndRotation(Utils.NetVec3ToGame(msg.position), Utils.NetQuatToGame(msg.rotation));
				}
			}
		}

		[CompilerGenerated]
		private void <RegisterNetworkMessagesHandlers>b__16_23(CSteamID sender, EventHookSyncMessage msg)
		{
			if (msg.request && !this.netManager.IsPlayer)
			{
				EventHook.SendSync(sender, msg.fsmID);
				return;
			}
			if (msg.HasFsmEventName)
			{
				EventHook.HandleEventSync(msg.fsmID, msg.fsmEventID, msg.FsmEventName);
				return;
			}
			EventHook.HandleEventSync(msg.fsmID, msg.fsmEventID, "none");
		}

		public const int MAX_VEHICLES = 255;

		private int isHere;

		private int isDone;

		public const int MAX_PICKUPABLES = 65535;

		private NetManager netManager;

		private const float PERIODICAL_UPDATE_INTERVAL = 2f;

		private int sentOne;

		private float timeToSendPeriodicalUpdate = 2f;

		public bool playerIsLoading = true;

		public static NetWorld Instance;

		private bool FWSQueued;

		private FullWorldSyncMessage queuedFWSMessage;

		private CSteamID queuedFWSsender;

		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			// Note: this type is marked as 'beforefieldinit'.
			static <>c()
			{
			}

			public <>c()
			{
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_0(CSteamID sender, PickupableSetPositionMessage msg)
			{
				try
				{
					GameObject gameObject = ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject;
					if (!gameObject.name.ToLower().StartsWith("motor"))
					{
						gameObject.transform.position = Utils.NetVec3ToGame(msg.position);
					}
				}
				catch (Exception)
				{
					Chat.Instance.AddMessage("Can not transfer gameObject position! Crash Avoided But It May Become Buggy (NetWorld-188)", MessageSeverity.Error);
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_1(CSteamID sender, PickupableActivateMessage msg)
			{
				GameObject gameObject = null;
				if (ObjectSyncManager.Instance.ObjectIDs.ContainsKey(msg.id))
				{
					try
					{
						gameObject = ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject;
					}
					catch (Exception)
					{
						Chat.Instance.AddMessage("Can not grab OSM ObjectID " + msg.id.ToString() + "! Crash Avoided But It May Become Buggy (NetWorld-196)", MessageSeverity.Error);
					}
				}
				if (gameObject == null)
				{
					Chat.Instance.AddMessage("Tried to activate pickupable but its not spawned! Crash Avoided But It May Become Buggy (NetWorld-200)", MessageSeverity.Error);
				}
				if (msg.activate)
				{
					gameObject.SetActive(true);
					return;
				}
				if (gameObject != null)
				{
					gameObject.SetActive(false);
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_3(CSteamID sender, PickupableDestroyMessage msg)
			{
				if (!ObjectSyncManager.Instance.ObjectIDs.ContainsKey(msg.id))
				{
					return;
				}
				try
				{
					GameObject gameObject = ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject;
				}
				catch
				{
					Logger.Error(string.Format("Failed to remove object: OSC found but can't get GameObject [{0}]", msg.id));
					return;
				}
				Object.Destroy(ObjectSyncManager.Instance.ObjectIDs[msg.id].gameObject);
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_12(CSteamID sender, VehicleStateMessage msg)
			{
				float num = -1f;
				if (msg.state == "WOODBREAK")
				{
					ObjectSyncManager.Instance.ObjectIDs[msg.objectID].gameObject.GetComponent<PlayMakerFSM>().SendEvent("MP_State 2");
					return;
				}
				if (msg.state == "DESTROYWOOD")
				{
					PlayerVehicle playerVehicle = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					playerVehicle.isLogRelated = true;
					playerVehicle.LogTriggerFSM.SendEvent("MP_Destroy Wood");
					return;
				}
				if (msg.state == "DESTROYLOG")
				{
					PlayerVehicle playerVehicle2 = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					playerVehicle2.isLogRelated = true;
					playerVehicle2.LogTriggerFSM.SendEvent("MP_Destroy Log");
					return;
				}
				ObjectSyncComponent objectSyncComponent = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
				if (objectSyncComponent == null)
				{
					Logger.Info("Remote player tried to set state of vehicle " + msg.objectID.ToString() + " but there is no vehicle with such id.");
					return;
				}
				if (msg.HasStartTime)
				{
					num = msg.StartTime;
				}
				(objectSyncComponent.GetObjectSubtype() as PlayerVehicle).SetEngineState(msg.state, num, msg.ignition);
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_13(CSteamID sender, VehicleSwitchMessage msg)
			{
				float num = -1f;
				string text = "";
				try
				{
					PlayerVehicle playerVehicle = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerVehicle;
					if (playerVehicle == null)
					{
						Logger.Info("Remote player tried to change a switch in vehicle " + msg.objectID.ToString() + " but there is no vehicle with such id.");
					}
					else
					{
						if (msg.HasSwitchValueFloat)
						{
							num = msg.SwitchValueFloat;
						}
						if (msg.HasSwitchValueString)
						{
							text = msg.SwitchValueString;
						}
						playerVehicle.SetVehicleSwitch((PlayerVehicle.SwitchIDs)msg.switchID, msg.switchValue, num, text);
					}
				}
				catch (Exception)
				{
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_14(CSteamID sender, HouseSwitchMessage msg)
			{
				float num = -1f;
				try
				{
					PlayerHouse playerHouse = ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as PlayerHouse;
					if (playerHouse == null)
					{
						if (ObjectSyncManager.Instance.ObjectIDs[msg.objectID].GetObjectSubtype() as Pickupable != null)
						{
							if (msg.HasSwitchValueFloat)
							{
								num = msg.SwitchValueFloat;
								Tool.Instance.SetHoistAngle(num, msg.switchValue);
							}
							else
							{
								Tool.Instance.receiver = true;
								Tool.Instance.hoistFsm.SendEvent("MP_Down");
							}
							Logger.Debug(string.Format("Setting angle to {0}", num));
						}
						else
						{
							Logger.Info("Remote player tried to change a switch in House " + msg.objectID.ToString() + " but there is no House with such id.");
						}
					}
					else
					{
						if (msg.HasSwitchValueFloat)
						{
							num = msg.SwitchValueFloat;
						}
						playerHouse.SetHouseSwitch((PlayerHouse.SwitchIDs)msg.switchID, msg.switchValue, num);
					}
				}
				catch (Exception)
				{
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_16(CSteamID sender, SatsumaSwitchMessage msg)
			{
				try
				{
					if (msg.HasTriggerID)
					{
						GameVehicleDatabase.Instance.triggerGameObjects[msg.TriggerID].ReadTriggerMessage(msg.PartObjectID, msg.objectID);
					}
					else if (msg.HasTighten)
					{
						GameVehicleDatabase.Instance.bolts[msg.objectID].ReadTightenMessage(msg.Tighten);
					}
				}
				catch (Exception ex)
				{
					Logger.Debug(string.Format("[NetWorld] Rivett message error on {0}: {1}", msg.objectID, ex));
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_17(CSteamID sender, LightSwitchMessage msg)
			{
				LightSwitchManager.Instance.FindLightSwitch(Utils.NetVec3ToGame(msg.pos)).TurnOn(msg.toggle);
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_18(CSteamID sender, WeatherUpdateMessage msg)
			{
				GameWeatherManager.Instance.ReadWeatherSync(msg);
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_20(CSteamID sender, OrderListMessage msg)
			{
				if (msg.fleetari)
				{
					if (msg.FleetariOrderList != null)
					{
						PlayMakerFSM fleetariList = MPController.Instance.fleetariList;
						for (int i = 0; i < 32; i++)
						{
							fleetariList.Fsm.GetFsmBool(PlayerHouse.fleetariListItems[i]).Value = msg.FleetariOrderList[i];
						}
						GameObject brochure = MPController.Instance.brochure;
						if (brochure == null)
						{
							return;
						}
						brochure.SetActive(false);
						return;
					}
				}
				else if (msg.OrderList != null)
				{
					ArrayList arrayList = new ArrayList();
					foreach (string text in msg.OrderList)
					{
						arrayList.Add(text);
					}
					MPController.Instance.orderList._arrayList = arrayList;
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_21(CSteamID sender, ObjectSyncResponseMessage msg)
			{
				ObjectSyncComponent objectSyncComponent = ObjectSyncManager.Instance.ObjectIDs[msg.objectID];
				if (msg.accepted)
				{
					objectSyncComponent.SyncEnabled = true;
					objectSyncComponent.Owner = SteamUser.GetSteamID().m_SteamID;
				}
			}

			internal void <RegisterNetworkMessagesHandlers>b__16_22(CSteamID sender, ObjectSyncRequestMessage msg)
			{
				try
				{
					ObjectSyncManager.Instance.ObjectIDs[msg.objectID].SyncRequestAccepted();
				}
				catch
				{
					Logger.Error(string.Format("Remote client tried to request object sync of an unknown object, Object ID: {0}", msg.objectID));
				}
			}

			public static readonly NetWorld.<>c <>9 = new NetWorld.<>c();

			public static NetMessageHandler.MessageHandler<PickupableSetPositionMessage> <>9__16_0;

			public static NetMessageHandler.MessageHandler<PickupableActivateMessage> <>9__16_1;

			public static NetMessageHandler.MessageHandler<PickupableDestroyMessage> <>9__16_3;

			public static NetMessageHandler.MessageHandler<VehicleStateMessage> <>9__16_12;

			public static NetMessageHandler.MessageHandler<VehicleSwitchMessage> <>9__16_13;

			public static NetMessageHandler.MessageHandler<HouseSwitchMessage> <>9__16_14;

			public static NetMessageHandler.MessageHandler<SatsumaSwitchMessage> <>9__16_16;

			public static NetMessageHandler.MessageHandler<LightSwitchMessage> <>9__16_17;

			public static NetMessageHandler.MessageHandler<WeatherUpdateMessage> <>9__16_18;

			public static NetMessageHandler.MessageHandler<OrderListMessage> <>9__16_20;

			public static NetMessageHandler.MessageHandler<ObjectSyncResponseMessage> <>9__16_21;

			public static NetMessageHandler.MessageHandler<ObjectSyncRequestMessage> <>9__16_22;
		}
	}
}
