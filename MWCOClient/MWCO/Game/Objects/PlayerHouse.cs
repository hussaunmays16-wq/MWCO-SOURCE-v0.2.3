using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Boo.Lang;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Game.Places;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class PlayerHouse : ISyncedObject
	{
		public PlayerHouse(GameObject go, ObjectSyncComponent osc)
		{
			try
			{
				this.gameObject = go;
				this.syncComponent = osc;
				this.ParentGameObject = go.GetTopmostParent();
				this.FindFSMs();
			}
			catch (Exception ex)
			{
				string text = "Error creating playerhouse item ";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		private void SpawnTree()
		{
		}

		public void SetRemoteSteering(bool enabled)
		{
		}

		public Transform ObjectTransform()
		{
			return this.ParentGameObject.transform;
		}

		public bool PeriodicSyncEnabled()
		{
			return false;
		}

		public bool CanSync()
		{
			return false;
		}

		public bool ShouldTakeOwnership()
		{
			return true;
		}

		public float[] ReturnSyncedVariables(bool sendAllVariables)
		{
			if (this.isSyncing)
			{
				return new float[0];
			}
			return null;
		}

		public void HandleSyncedVariables(float[] variables, CSteamID sender)
		{
			try
			{
			}
			catch (Exception)
			{
				Chat.Instance.AddMessage("Can Not Sync PlayerHouse Items! Error 0034", MessageSeverity.Error);
			}
		}

		public void SyncTakenByForce()
		{
			this.SetRemoteSteering(true);
		}

		public void OwnerSetToRemote()
		{
			this.SetRemoteSteering(true);
		}

		public void OwnerRemoved()
		{
			this.SetRemoteSteering(false);
		}

		public void ConstantSyncChanged(bool newValue)
		{
		}

		public void FindFSMs()
		{
			PlayMakerFSM[] componentsInChildren = this.ParentGameObject.GetComponentsInChildren<PlayMakerFSM>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				PlayMakerFSM fsm = componentsInChildren[i];
				if (this.ParentGameObject.name != "INSPECTION" && this.ParentGameObject.name != "COTTAGE" && this.ParentGameObject.name != "EQUIPMENTS")
				{
					if (fsm.FsmName == "Use")
					{
						if (fsm.name == "Handle")
						{
							Transform parent = fsm.transform.parent;
							string text;
							if (parent == null)
							{
								text = null;
							}
							else
							{
								Transform parent2 = parent.parent;
								text = ((parent2 != null) ? parent2.name : null);
							}
							if (text == "Fridge")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Open door", null, false);
								EventHook.AddWithSync(fsm, "Close door", null, false);
							}
							else
							{
								Transform parent3 = fsm.transform.parent;
								string text2;
								if (parent3 == null)
								{
									text2 = null;
								}
								else
								{
									Transform parent4 = parent3.parent;
									text2 = ((parent4 != null) ? parent4.name : null);
								}
								if (!(text2 == "DoorFront"))
								{
									Transform parent5 = fsm.transform.parent;
									string text3;
									if (parent5 == null)
									{
										text3 = null;
									}
									else
									{
										Transform parent6 = parent5.parent;
										text3 = ((parent6 != null) ? parent6.name : null);
									}
									if (!(text3 == "DoorRear"))
									{
										goto IL_F60;
									}
								}
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Check position", null, false);
							}
						}
						else if (fsm.name == "Trigger")
						{
							Transform parent7 = fsm.transform.parent;
							if (((parent7 != null) ? parent7.name : null) == "KitchenWaterTap")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Position", null, false);
							}
							else
							{
								Transform parent8 = fsm.transform.parent;
								if (((parent8 != null) ? parent8.name : null) == "SmokeDetector")
								{
									EventHook.AddWithSync(fsm, "Wait player", delegate
									{
										fsm.FsmStates[2].Actions[5].Enabled = true;
										fsm.FsmStates[2].Actions[6].Enabled = true;
										return false;
									}, false);
									EventHook.AddWithSync(fsm, "State 1", delegate
									{
										fsm.FsmStates[2].Actions[5].Enabled = !Utils.IsReceiver(fsm);
										fsm.FsmStates[2].Actions[6].Enabled = !Utils.IsReceiver(fsm);
										return false;
									}, false);
								}
							}
						}
						else if (fsm.name == "Switch")
						{
							Transform parent9 = fsm.transform.parent;
							string text4;
							if (parent9 == null)
							{
								text4 = null;
							}
							else
							{
								Transform parent10 = parent9.parent;
								text4 = ((parent10 != null) ? parent10.name : null);
							}
							if (!(text4 == "Shower"))
							{
								Transform parent11 = fsm.transform.parent;
								if (!(((parent11 != null) ? parent11.name : null) == "Shower"))
								{
									goto IL_F60;
								}
							}
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Check water", null, false);
						}
						else if (fsm.name == "MainSwitch")
						{
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Switch", null, false);
							EventHook.AddWithSync(fsm, "Position", null, false);
						}
						else if (fsm.name == "RadioCDSwitch")
						{
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Switch", null, false);
						}
						else if (fsm.name == "Eject")
						{
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Switch", null, false);
						}
						else if (fsm.name == "OpenMailBox")
						{
							EventHook.AddWithSync(fsm, "State 1", null, false);
							EventHook.AddWithSync(fsm, "State 3", null, false);
							EventHook.AddWithSync(fsm, "Open", null, false);
							EventHook.AddWithSync(fsm, "Close", null, false);
						}
						else if (fsm.name == "TVSwitch")
						{
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Switch", null, false);
							EventHook.AddWithSync(fsm, "Switch 2", null, false);
						}
						else if (fsm.name == "Logwall")
						{
							this.logwall = fsm;
						}
						else if (fsm.name == "SetFire")
						{
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Start fire 2", null, false);
							EventHook.AddWithSync(fsm, "Burn", null, false);
							EventHook.AddWithSync(fsm, "Off", null, false);
						}
					}
					else if (fsm.FsmName == "Switch")
					{
						if (fsm.name == "Valve")
						{
							Transform parent12 = fsm.transform.parent;
							string text5;
							if (parent12 == null)
							{
								text5 = null;
							}
							else
							{
								Transform parent13 = parent12.parent;
								text5 = ((parent13 != null) ? parent13.name : null);
							}
							if (!(text5 == "Shower"))
							{
								Transform parent14 = fsm.transform.parent;
								if (!(((parent14 != null) ? parent14.name : null) == "Shower"))
								{
									goto IL_F60;
								}
							}
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Position", null, false);
						}
					}
					else if (fsm.FsmName == "Knob")
					{
						if (fsm.name == "Thermostat")
						{
							EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
							EventHook.AddWithSync(fsm, "Increase", null, false);
							EventHook.AddWithSync(fsm, "Decrease", null, false);
							EventHook.AddWithSync(fsm, "Rotate knob", null, false);
						}
						else if (fsm.name == "Bass")
						{
							EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
							EventHook.AddWithSync(fsm, "Bass sub", null, false);
							EventHook.AddWithSync(fsm, "Bass add", null, false);
							EventHook.AddWithSync(fsm, "Set volume", null, false);
						}
						else if (fsm.name == "Volume")
						{
							EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
							EventHook.AddWithSync(fsm, "Volume dec", null, false);
							EventHook.AddWithSync(fsm, "Volume inc", null, false);
							EventHook.AddWithSync(fsm, "Set volume", null, false);
							EventHook.AddWithSync(fsm, "On", null, false);
							EventHook.AddWithSync(fsm, "Off", null, false);
						}
					}
					else if (fsm.FsmName == "ChangeChannel")
					{
						if (fsm.name == "TrackChannelSwitch")
						{
							EventHook.AddWithSync(fsm, "Wait player", null, false);
							EventHook.AddWithSync(fsm, "Check status", null, false);
							EventHook.AddWithSync(fsm, "State 1", null, false);
						}
					}
					else if (fsm.FsmName == "Screw")
					{
						if (fsm.name.StartsWith("KnobPower"))
						{
							EventHook.AddWithSync(fsm, "Screw", null, false);
							EventHook.AddWithSync(fsm, "Unscrew", null, false);
							EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
						}
					}
					else if (fsm.FsmName == "Input")
					{
						if (fsm.name.StartsWith("KeypadPhone"))
						{
						}
					}
					else if (fsm.FsmName == "Calling")
					{
						if (fsm.name.StartsWith("KeypadPhone"))
						{
						}
					}
					else if (fsm.FsmName == "Data")
					{
						if (fsm.name == "cable plug(itemx)")
						{
							EventHook.AddWithSync(fsm, "Heater on", null, false);
							EventHook.AddWithSync(fsm, "Heater off", null, false);
						}
					}
					else if (fsm.FsmName == "LOD")
					{
						fsm.gameObject.AddComponent<LODManager>().Initialize(fsm, -1f, true, "LODdistance", "LOD1");
					}
				}
				else
				{
					string name = fsm.name;
					if (name != null)
					{
						switch (name.Length)
						{
						case 5:
							if (name == "Order")
							{
								if (fsm.FsmName == "Use")
								{
									EventHook.AddWithSync(fsm, "Pay", null, false);
									Logger.Debug(fsm.name + " INSPECTION");
								}
							}
							break;
						case 6:
							if (name == "Handle")
							{
								if (fsm.FsmName == "Use")
								{
									EventHook.AddWithSync(fsm, "Open door", null, false);
									EventHook.AddWithSync(fsm, "Close door", null, false);
									Logger.Debug(componentsInChildren[i].name + " INSPECTION");
								}
								else if (fsm.transform.parent.name == "StoveDoor")
								{
									EventHook.AddWithSync(fsm, "Open door", null, false);
									EventHook.AddWithSync(fsm, "Close door", null, false);
								}
							}
							break;
						case 7:
						{
							char c = name[0];
							if (c != 'L')
							{
								if (c != 'S')
								{
									if (c == 'T')
									{
										if (name == "Trigger")
										{
											if (fsm.FsmName == "Use" && fsm.gameObject.transform.parent.name == "floor jack(itemx)")
											{
												EventHook.AddWithSync(fsm, "Wait player 2", null, false);
												fsm.transform.parent.GetChild(2).GetChild(0).gameObject.AddComponent<CollisionDetection>();
												EventHook.AddWithSync(fsm, "Up", delegate
												{
													fsm.Fsm.States[0].Actions[3].Enabled = !Utils.IsReceiver(fsm);
													return false;
												}, false);
												EventHook.AddWithSync(fsm, "Down", delegate
												{
													fsm.Fsm.States[1].Actions[2].Enabled = !Utils.IsReceiver(fsm);
													return false;
												}, false);
											}
											else if (fsm.FsmName == "Use" && fsm.transform.parent.name == "car jack(itemx)")
											{
												fsm.gameObject.SetActive(true);
												EventHook.AddWithSync(fsm, "Up", delegate
												{
													fsm.Fsm.States[0].Actions[2].Enabled = !Utils.IsReceiver(fsm);
													return false;
												}, false);
												EventHook.AddWithSync(fsm, "Up 2", delegate
												{
													fsm.Fsm.States[5].Actions[2].Enabled = !Utils.IsReceiver(fsm);
													return false;
												}, false);
												fsm.gameObject.SetActive(false);
											}
											else if (fsm.FsmName == "Use" && fsm.transform.parent.name == "garbage barrel(itemx)")
											{
												EventHook.AddWithSync(fsm, "Start fire 2", null, false);
												EventHook.AddWithSync(fsm, "Burn", null, false);
												EventHook.AddWithSync(fsm, "Destroy", null, false);
												EventHook.AddWithSync(fsm, "Wait player", null, false);
											}
										}
									}
								}
								else if (name == "SetFire")
								{
									EventHook.AddWithSync(fsm, "Start fire 2", null, false);
									EventHook.AddWithSync(fsm, "Burn", null, false);
								}
							}
							else if (name == "Logwall")
							{
								if (fsm.FsmName == "Use" && !PlayerHouse.HasBeenSynced(fsm))
								{
									EventHook.AddWithSync(fsm, "Wait player", null, false);
									EventHook.AddWithSync(fsm, "Create log", null, false);
								}
							}
							break;
						}
						case 10:
							if (name == "INSPECTION")
							{
								if (fsm.FsmName == "LOD")
								{
									fsm.gameObject.AddComponent<LODManager>().Initialize(fsm, 120f, false, "LOD", "Object");
									Logger.Debug(fsm.name + " INSPECTION");
								}
							}
							break;
						case 11:
							if (name == "WoodTrigger")
							{
								if (!PlayerHouse.HasBeenSynced(fsm))
								{
									EventHook.AddWithSync(fsm, "State 1", null, false);
								}
							}
							break;
						case 12:
							if (name == "StoveTrigger")
							{
								EventHook.AddWithSync(fsm, "Steam", null, false);
							}
							break;
						case 13:
							if (name == "PickupTrigger")
							{
								if (!PlayerHouse.HasBeenSynced(fsm))
								{
									EventHook.AddWithSync(fsm, "Wait player", null, false);
									EventHook.AddWithSync(fsm, "Bool test", null, false);
								}
							}
							break;
						case 14:
							if (name == "lantern(itemx)")
							{
								if (fsm.FsmName == "Use")
								{
									EventHook.AddWithSync(fsm, "Wait player", null, false);
									EventHook.AddWithSync(fsm, "Wait player 2", null, false);
									EventHook.AddWithSync(fsm, "ON", null, false);
									EventHook.AddWithSync(fsm, "OFF", null, false);
								}
							}
							break;
						case 15:
							if (name == "car jack(itemx)")
							{
								if (fsm.FsmName == "Fold")
								{
									this.carJack = fsm;
								}
							}
							break;
						}
					}
				}
				IL_F60:
				if (fsm.name.StartsWith("fuseholder"))
				{
					if (componentsInChildren[i].FsmName == "Use")
					{
						if (PlayerHouse.HasBeenSynced(componentsInChildren[i]))
						{
							return;
						}
						EventHook.AddWithSync(componentsInChildren[i], "Blown fuse", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "No fuse", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "No holder", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "State 6", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "State 3", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "Install", null, false);
					}
					else if (componentsInChildren[i].FsmName == "Screw")
					{
						if (PlayerHouse.HasBeenSynced(componentsInChildren[i]))
						{
							return;
						}
						EventHook.AddWithSync(componentsInChildren[i], "Mouse off 2", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "Wait 2", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "Screw", null, false);
						EventHook.AddWithSync(componentsInChildren[i], "Unscrew", null, false);
					}
				}
			}
			Logger.Debug("Hooking Events");
			this.HookEvents();
		}

		private void RefreshMagazineList()
		{
			Logger.Debug("Attempting to send array list");
			NetLocalPlayer.Instance.WriteOrderListMessage(MPController.Instance.orderList._arrayList.Cast<string>().ToArray<string>());
		}

		private void RefreshFleetarisList()
		{
			PlayMakerFSM fleetariList = MPController.Instance.fleetariList;
			for (int i = 0; i < this.fleetariOrder.Length; i++)
			{
				this.fleetariOrder[i] = fleetariList.Fsm.GetFsmBool(PlayerHouse.fleetariListItems[i]).Value;
			}
			NetLocalPlayer.Instance.WriteFleetariOrderListMessage(this.fleetariOrder);
		}

		public static void getKeyes()
		{
			FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyFerndale").Value = 1;
			FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyGifu").Value = 1;
			FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyHayosiko").Value = 1;
			FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyRuscko").Value = 1;
			FsmVariables.GlobalVariables.GetFsmInt("PlayerKeyHome").Value = 1;
			PlayerHouse.getKeys = false;
		}

		private void HookEvents()
		{
			try
			{
				if (this.fridgeFsm != null && !PlayerHouse.HasBeenSynced(this.fridgeFsm))
				{
					EventHook.AddWithSync(this.fridgeFsm, "Open door", null, false);
					EventHook.AddWithSync(this.fridgeFsm, "Close door", null, false);
				}
				if (this.woodCarrier != null && !PlayerHouse.HasBeenSynced(this.woodCarrier))
				{
					EventHook.AddWithSync(this.woodCarrier, "State 1", null, false);
					EventHook.AddWithSync(this.woodCarrier, "State 2", null, false);
					EventHook.AddWithSync(this.woodCarrier, "State 6", null, false);
				}
				if (this.woodCarrier2 != null && !PlayerHouse.HasBeenSynced(this.woodCarrier2))
				{
					EventHook.AddWithSync(this.woodCarrier, "Wait", null, false);
					EventHook.AddWithSync(this.woodCarrier, "State 1", null, false);
					EventHook.AddWithSync(this.woodCarrier, "State 6", null, false);
				}
				if (this.uncleDoor != null && !PlayerHouse.HasBeenSynced(this.uncleDoor))
				{
					EventHook.AddWithSync(this.uncleDoor, "Knock", null, false);
				}
				if (this.UNCLE != null && !PlayerHouse.HasBeenSynced(this.UNCLE))
				{
					EventHook.AddWithSync(this.UNCLE, "At uncle", null, false);
					EventHook.AddWithSync(this.UNCLE, "Away", null, false);
				}
				if (this.grandma != null && !PlayerHouse.HasBeenSynced(this.grandma))
				{
					EventHook.AddWithSync(this.grandma, "Groceries", null, false);
					EventHook.AddWithSync(this.grandma, "Call", null, false);
					EventHook.AddWithSync(this.grandma, "Call 2", null, false);
					EventHook.AddWithSync(this.grandma, "State 1", null, false);
					EventHook.AddWithSync(this.grandma, "State 2", null, false);
					EventHook.AddWithSync(this.grandma, "Fish", null, false);
					EventHook.AddWithSync(this.grandma, "Call church", null, false);
					EventHook.AddWithSync(this.grandma, "Delay", null, false);
					EventHook.AddWithSync(this.grandma, "Delay 2", null, false);
					EventHook.AddWithSync(this.grandma, "Reset", null, false);
					EventHook.AddWithSync(this.grandma, "Groceries", null, false);
					EventHook.AddWithSync(this.grandma, "Is day", null, false);
					EventHook.AddWithSync(this.grandma, "Check order", null, false);
				}
				if (this.vanKeys != null && !PlayerHouse.HasBeenSynced(this.vanKeys))
				{
					EventHook.AddWithSync(this.vanKeys, "State 1", null, false);
				}
				if (this.lantern != null && !PlayerHouse.HasBeenSynced(this.lantern))
				{
					EventHook.AddWithSync(this.lantern, "Wait player", null, false);
					EventHook.AddWithSync(this.lantern, "Wait player 2", null, false);
					EventHook.AddWithSync(this.lantern, "ON", null, false);
					EventHook.AddWithSync(this.lantern, "OFF", null, false);
				}
				if (this.TVFsm != null && !PlayerHouse.HasBeenSynced(this.TVFsm))
				{
					EventHook.Add(this.TVFsm, "Switch", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.TV, !this.TVFsm.Fsm.GetFsmBool("Open").Value, -1f);
						return false;
					}, false, false);
				}
				if (this.lidT != null && !PlayerHouse.HasBeenSynced(this.lidT))
				{
					EventHook.AddWithSync(this.lidT, "Assemble", null, false);
				}
				if (this.lidR != null && !PlayerHouse.HasBeenSynced(this.lidR))
				{
					EventHook.AddWithSync(this.lidR, "Remove part", null, false);
				}
				if (this.coffee != null)
				{
					PlayerHouse.HasBeenSynced(this.coffee);
				}
				if (this.water != null && !PlayerHouse.HasBeenSynced(this.water))
				{
					EventHook.AddWithSync(this.water, "Wait player", null, false);
					EventHook.AddWithSync(this.water, "Move lever", null, false);
				}
				if (this.cofeCap != null && !PlayerHouse.HasBeenSynced(this.cofeCap))
				{
					EventHook.AddWithSync(this.cofeCap, "Wait player", null, false);
					EventHook.AddWithSync(this.cofeCap, "Wait player 2", null, false);
					EventHook.AddWithSync(this.cofeCap, "ON", null, false);
					EventHook.AddWithSync(this.cofeCap, "OFF", null, false);
				}
				if (this.spannerFsm != null && !PlayerHouse.HasBeenSynced(this.spannerFsm))
				{
					EventHook.AddWithSync(this.spannerFsm, "Open", null, false);
					EventHook.AddWithSync(this.spannerFsm, "Close", null, false);
				}
				if (this.carJack != null && !PlayerHouse.HasBeenSynced(this.carJack))
				{
					EventHook.AddWithSync(this.carJack, "Bool test", null, false);
				}
				if (this.carJackT != null && !PlayerHouse.HasBeenSynced(this.carJackT))
				{
					this.carJackT.gameObject.SetActive(true);
					EventHook.AddWithSync(this.carJackT, "Up", null, false);
					EventHook.AddWithSync(this.carJackT, "Up 2", null, false);
					this.carJackT.gameObject.SetActive(false);
				}
				if (this.logwall != null && !PlayerHouse.HasBeenSynced(this.logwall))
				{
					EventHook.AddWithSync(this.logwall, "Wait player", null, false);
					EventHook.AddWithSync(this.logwall, "Create log", null, false);
				}
				if (this.grillFsm != null && !PlayerHouse.HasBeenSynced(this.grillFsm))
				{
					EventHook.AddWithSync(this.grillFsm, "Wait player", null, false);
					EventHook.AddWithSync(this.grillFsm, "Start fire 2", null, false);
					EventHook.AddWithSync(this.grillFsm, "Burn", null, false);
					EventHook.AddWithSync(this.grillFsm, "Off", null, false);
					EventHook.AddWithSync(this.grillFsm, "Off 2", null, false);
				}
				if (this.radioFsm != null && !PlayerHouse.HasBeenSynced(this.radioFsm))
				{
					EventHook.Add(this.radioFsm, "Switch", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Radio, !this.radioFsm.Fsm.GetFsmBool("On").Value, -1f);
						return false;
					}, false, false);
				}
				if (this.radioVFsm != null && !PlayerHouse.HasBeenSynced(this.radioVFsm))
				{
					EventHook.AddWithSync(this.radioVFsm, "Mouse off 2", null, false);
					EventHook.AddWithSync(this.radioVFsm, "Increase", delegate
					{
						this.radio1Fsm.Fsm.GetFsmFloat("Volume").Value = this.radioVFsm.Fsm.GetFsmFloat("Volume").Value + 0.1f;
						return false;
					}, false);
					EventHook.AddWithSync(this.radioVFsm, "Decrease", delegate
					{
						this.radio1Fsm.Fsm.GetFsmFloat("Volume").Value = this.radioVFsm.Fsm.GetFsmFloat("Volume").Value - 0.1f;
						return false;
					}, false);
				}
				if (this.smokeFsm != null)
				{
					PlayerHouse.HasBeenSynced(this.smokeFsm);
				}
				if (this.showerFsm != null && !PlayerHouse.HasBeenSynced(this.showerFsm))
				{
					EventHook.AddWithSync(this.showerFsm, "Wait player", null, false);
					EventHook.AddWithSync(this.showerFsm, "Set", null, false);
					EventHook.AddWithSync(this.showerFsm, "OFF", null, false);
					EventHook.AddWithSync(this.showerFsm, "ON", null, false);
					EventHook.AddWithSync(this.showerFsm, "Tap", null, false);
					EventHook.AddWithSync(this.showerFsm, "Shower", null, false);
				}
				if (this.showerSFsm != null && !PlayerHouse.HasBeenSynced(this.showerSFsm))
				{
					EventHook.AddWithSync(this.showerSFsm, "Wait player", null, false);
					EventHook.AddWithSync(this.showerSFsm, "Tap", null, false);
					EventHook.AddWithSync(this.showerSFsm, "Shower", null, false);
					EventHook.AddWithSync(this.showerSFsm, "State 1", null, false);
				}
				if (this.tapFsm != null && !PlayerHouse.HasBeenSynced(this.tapFsm))
				{
					EventHook.AddWithSync(this.tapFsm, "Wait player", null, false);
					EventHook.AddWithSync(this.tapFsm, "Position", null, false);
				}
				if (this.flashFsm != null && !PlayerHouse.HasBeenSynced(this.flashFsm))
				{
					EventHook.AddWithSync(this.flashFsm, "Wait player", null, false);
					EventHook.AddWithSync(this.flashFsm, "Flip", null, false);
					EventHook.AddWithSync(this.flashFsm, "Open", null, false);
					EventHook.AddWithSync(this.flashFsm, "Close", null, false);
				}
				if (this.saunaFsm != null && !PlayerHouse.HasBeenSynced(this.saunaFsm))
				{
					EventHook.AddWithSync(this.saunaFsm, "Mouse off 2", null, false);
					EventHook.AddWithSync(this.saunaFsm, "Screw", null, false);
					EventHook.AddWithSync(this.saunaFsm, "Unscrew", null, false);
					EventHook.AddWithSync(this.saunaFsm, "Sound", null, false);
				}
				if (this.saunaTFsm != null && !PlayerHouse.HasBeenSynced(this.saunaTFsm))
				{
					EventHook.AddWithSync(this.saunaTFsm, "Mouse off 2", null, false);
					EventHook.AddWithSync(this.saunaTFsm, "Screw", null, false);
					EventHook.AddWithSync(this.saunaTFsm, "Unscrew", null, false);
					EventHook.AddWithSync(this.saunaTFsm, "Sound", null, false);
				}
				if (this.saunaRFsm != null && !PlayerHouse.HasBeenSynced(this.saunaRFsm))
				{
					EventHook.AddWithSync(this.saunaRFsm, "Steam", null, false);
				}
				if (this.teleFsm != null && !PlayerHouse.HasBeenSynced(this.teleFsm))
				{
					EventHook.Add(this.teleFsm, "Flip", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Tele, !this.teleFsm.Fsm.GetFsmBool("Use").Value, -1f);
						return false;
					}, false, false);
					EventHook.Add(this.teleFsm, "Pick phone", delegate
					{
						if (Vector3.Distance(this.teleFsm.transform.position, GamePlayer.Instance.Object.transform.position) > 2f)
						{
							this.teleFsm.Fsm.GetState("Pick phone").Actions[6].Enabled = false;
						}
						else
						{
							this.teleFsm.Fsm.GetState("Pick phone").Actions[6].Enabled = true;
						}
						return false;
					}, false, false);
					EventHook.Add(this.teleFsm, "Close phone", delegate
					{
						if (Vector3.Distance(this.teleFsm.transform.position, GamePlayer.Instance.Object.transform.position) > 2f)
						{
							this.teleFsm.Fsm.GetState("Close phone").Actions[5].Enabled = false;
						}
						else
						{
							this.teleFsm.Fsm.GetState("Close phone").Actions[5].Enabled = true;
						}
						return false;
					}, false, false);
				}
				if (this.phoneCallsFsm != null && !PlayerHouse.HasBeenSynced(this.phoneCallsFsm))
				{
					EventHook.SyncAllEvents(this.phoneCallsFsm, () => false);
				}
				if (this.phoneCordInFsm != null && !PlayerHouse.HasBeenSynced(this.phoneCordInFsm))
				{
					EventHook.AddWithSync(this.phoneCordInFsm, "Position", null, false);
				}
				if (this.phoneCordOutFsm != null && !PlayerHouse.HasBeenSynced(this.phoneCordOutFsm))
				{
					EventHook.AddWithSync(this.phoneCordOutFsm, "Position", null, false);
				}
				if (this.knob1 != null && !PlayerHouse.HasBeenSynced(this.knob1))
				{
					EventHook.Add(this.knob1, "State 1", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob1, false, this.knob1.Fsm.GetFsmFloat("Data").Value);
						return false;
					}, false, false);
					EventHook.AddWithSync(this.knob1, "Screw", null, false);
					EventHook.AddWithSync(this.knob1, "Unscrew", null, false);
					EventHook.AddWithSync(this.knob1, "Mouse off 2", null, false);
				}
				if (this.knob2 != null && !PlayerHouse.HasBeenSynced(this.knob2))
				{
					EventHook.Add(this.knob2, "State 1", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob2, false, this.knob2.Fsm.GetFsmFloat("Data").Value);
						return false;
					}, false, false);
					EventHook.AddWithSync(this.knob2, "Screw", null, false);
					EventHook.AddWithSync(this.knob2, "Unscrew", null, false);
					EventHook.AddWithSync(this.knob2, "Mouse off 2", null, false);
				}
				if (this.knob3 != null && !PlayerHouse.HasBeenSynced(this.knob3))
				{
					EventHook.Add(this.knob3, "State 1", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob3, false, this.knob3.Fsm.GetFsmFloat("Data").Value);
						return false;
					}, false, false);
					EventHook.AddWithSync(this.knob3, "Screw", null, false);
					EventHook.AddWithSync(this.knob3, "Unscrew", null, false);
					EventHook.AddWithSync(this.knob3, "Mouse off 2", null, false);
				}
				if (this.knob4 != null && !PlayerHouse.HasBeenSynced(this.knob4))
				{
					EventHook.Add(this.knob4, "State 1", delegate
					{
						NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob4, false, this.knob4.Fsm.GetFsmFloat("Data").Value);
						return false;
					}, false, false);
					EventHook.AddWithSync(this.knob4, "Screw", null, false);
					EventHook.AddWithSync(this.knob4, "Unscrew", null, false);
					EventHook.AddWithSync(this.knob4, "Mouse off 2", null, false);
				}
				if (this.saveFsm != null)
				{
					PlayerHouse.HasBeenSynced(this.saveFsm);
				}
				foreach (PlayMakerFSM playMakerFSM in this.shitMen)
				{
					EventHook.AddWithSync(playMakerFSM, "Hello", null, false);
					EventHook.AddWithSync(playMakerFSM, "Drink", null, false);
					EventHook.AddWithSync(playMakerFSM, "Smoke", null, false);
					EventHook.AddWithSync(playMakerFSM, "Anim", null, false);
					EventHook.AddWithSync(playMakerFSM, "Wait player", null, false);
				}
				foreach (PlayMakerFSM playMakerFSM2 in this.shitNPCs)
				{
				}
				if (this.gasoline != null && !PlayerHouse.HasBeenSynced(this.gasoline))
				{
					EventHook.SyncAllEvents(this.gasoline, () => false);
				}
			}
			catch (Exception ex)
			{
				Logger.Debug("PLAYERHOUSE CRASHED WITH " + ex.Message);
			}
		}

		public static bool HasBeenSynced(PlayMakerFSM fsm)
		{
			bool flag;
			try
			{
				FsmEvent[] events = fsm.Fsm.Events;
				for (int i = 0; i < events.Length; i++)
				{
					if (events[i].Name.StartsWith("MP_"))
					{
						return true;
					}
				}
				flag = false;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public void SetHouseSwitch(PlayerHouse.SwitchIDs state, bool newValue, float newValueFloat)
		{
			switch (state)
			{
			case PlayerHouse.SwitchIDs.Radio:
				if (this.radioFsm.Fsm.GetFsmBool("On").Value != newValue)
				{
					this.radioFsm.SendEvent("MP_Switch");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.Radio1:
				if (this.radio1Fsm.Fsm.GetFsmBool("On").Value != newValue)
				{
					this.radio1Fsm.SendEvent("MP_Switch");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.RadioV:
				this.radioVFsm.Fsm.GetFsmFloat("Volume").Value = newValueFloat;
				return;
			case PlayerHouse.SwitchIDs.Tele:
				if (this.teleFsm.Fsm.GetFsmBool("Use").Value != newValue)
				{
					this.teleFsm.SendEvent("MP_Flip");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.TV:
				if (this.TVFsm.Fsm.GetFsmBool("Open").Value != newValue)
				{
					this.TVFsm.SendEvent("MP_Switch");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.Knob1:
				if (this.knob1.Fsm.GetFsmFloat("Data").Value != newValueFloat)
				{
					this.knob1.SendEvent("MP_State 1");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.Knob2:
				if (this.knob2.Fsm.GetFsmFloat("Data").Value != newValueFloat)
				{
					this.knob2.SendEvent("MP_State 1");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.Knob3:
				if (this.knob3.Fsm.GetFsmFloat("Data").Value != newValueFloat)
				{
					this.knob3.SendEvent("MP_State 1");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.Knob4:
				if (this.knob4.Fsm.GetFsmFloat("Data").Value != newValueFloat)
				{
					this.knob4.SendEvent("MP_State 1");
				}
				break;
			case PlayerHouse.SwitchIDs.Save:
			case PlayerHouse.SwitchIDs.Lantern:
			case PlayerHouse.SwitchIDs.Firewood:
			case PlayerHouse.SwitchIDs.FloorJack:
				break;
			case PlayerHouse.SwitchIDs.shitMan:
				this.shitMan.Fsm.GetFsmFloat("ShitLevel").Value = newValueFloat;
				this.shitMan.SendEvent("MP_Full");
				return;
			case PlayerHouse.SwitchIDs.shitMan2:
				this.shitMan2.Fsm.GetFsmFloat("ShitLevel").Value = newValueFloat;
				this.shitMan2.SendEvent("MP_Full");
				return;
			case PlayerHouse.SwitchIDs.shitMan3:
				this.shitMan3.Fsm.GetFsmFloat("ShitLevel").Value = newValueFloat;
				this.shitMan3.SendEvent("MP_Full");
				return;
			case PlayerHouse.SwitchIDs.shitMan4:
				this.shitMan4.Fsm.GetFsmFloat("ShitLevel").Value = newValueFloat;
				this.shitMan4.SendEvent("MP_Full");
				return;
			case PlayerHouse.SwitchIDs.shitMan5:
				this.shitMan5.Fsm.GetFsmFloat("ShitLevel").Value = newValueFloat;
				this.shitMan5.SendEvent("MP_Full");
				return;
			case PlayerHouse.SwitchIDs.lidT:
				this.lidT.SendEvent("MP_GLOBALEVENT");
				return;
			case PlayerHouse.SwitchIDs.water:
				if (this.water.Fsm.GetFsmFloat("Time").Value != newValueFloat)
				{
					this.water.SendEvent("MP_Move lever");
					return;
				}
				break;
			case PlayerHouse.SwitchIDs.Hoist:
				Logger.Debug(string.Format("Setting angle to {0}", newValueFloat));
				Tool.Instance.hoistFsm.Fsm.GetFsmFloat("Angle").Value = newValueFloat;
				return;
			default:
				return;
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PlayerHouse()
		{
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_0()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.TV, !this.TVFsm.Fsm.GetFsmBool("Open").Value, -1f);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_1()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Radio, !this.radioFsm.Fsm.GetFsmBool("On").Value, -1f);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_2()
		{
			this.radio1Fsm.Fsm.GetFsmFloat("Volume").Value = this.radioVFsm.Fsm.GetFsmFloat("Volume").Value + 0.1f;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_3()
		{
			this.radio1Fsm.Fsm.GetFsmFloat("Volume").Value = this.radioVFsm.Fsm.GetFsmFloat("Volume").Value - 0.1f;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_4()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Tele, !this.teleFsm.Fsm.GetFsmBool("Use").Value, -1f);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_5()
		{
			if (Vector3.Distance(this.teleFsm.transform.position, GamePlayer.Instance.Object.transform.position) > 2f)
			{
				this.teleFsm.Fsm.GetState("Pick phone").Actions[6].Enabled = false;
			}
			else
			{
				this.teleFsm.Fsm.GetState("Pick phone").Actions[6].Enabled = true;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_6()
		{
			if (Vector3.Distance(this.teleFsm.transform.position, GamePlayer.Instance.Object.transform.position) > 2f)
			{
				this.teleFsm.Fsm.GetState("Close phone").Actions[5].Enabled = false;
			}
			else
			{
				this.teleFsm.Fsm.GetState("Close phone").Actions[5].Enabled = true;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_8()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob1, false, this.knob1.Fsm.GetFsmFloat("Data").Value);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_9()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob2, false, this.knob2.Fsm.GetFsmFloat("Data").Value);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_10()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob3, false, this.knob3.Fsm.GetFsmFloat("Data").Value);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__86_11()
		{
			NetLocalPlayer.Instance.WriteHouseSwitchMessage(this.syncComponent, PlayerHouse.SwitchIDs.Knob4, false, this.knob4.Fsm.GetFsmFloat("Data").Value);
			return false;
		}

		private ObjectSyncComponent syncComponent;

		private bool isSyncing;

		private GameObject gameObject;

		public static bool getKeys = false;

		public GameObject ParentGameObject;

		public bool PlayerIsLocal;

		private PlayMakerFSM fridgeFsm;

		private PlayMakerFSM tapFsm;

		private PlayMakerFSM radioFsm;

		private PlayMakerFSM radio1Fsm;

		private PlayMakerFSM radioVFsm;

		private PlayMakerFSM smokeFsm;

		private PlayMakerFSM teleFsm;

		private PlayMakerFSM phoneCallsFsm;

		private PlayMakerFSM phoneCordInFsm;

		private PlayMakerFSM phoneCordOutFsm;

		private PlayMakerFSM flashFsm;

		private PlayMakerFSM TVFsm;

		private PlayMakerFSM spannerFsm;

		private PlayMakerFSM saunaFsm;

		private PlayMakerFSM saunaTFsm;

		private PlayMakerFSM saunaRFsm;

		private PlayMakerFSM showerFsm;

		private PlayMakerFSM showerSFsm;

		private PlayMakerFSM saveFsm;

		private PlayMakerFSM grillFsm;

		private PlayMakerFSM magazineFsm;

		private PlayMakerFSM carJack;

		private PlayMakerFSM carJackT;

		private PlayMakerFSM lantern;

		private PlayMakerFSM water;

		private PlayMakerFSM cofeCap;

		private PlayMakerFSM coffee;

		private PlayMakerFSM logwall;

		private PlayMakerFSM lidT;

		private PlayMakerFSM lidR;

		private PlayMakerFSM woodCarrier;

		private PlayMakerFSM woodCarrier2;

		private PlayMakerFSM logjoint;

		private PlayMakerFSM[] fireplace;

		private PlayMakerFSM gasoline;

		private PlayMakerFSM shitJob;

		private PlayMakerFSM shitJob2;

		private PlayMakerFSM shitJob3;

		private PlayMakerFSM shitJob4;

		private PlayMakerFSM shitJob5;

		private List<PlayMakerFSM> shitMen = new List<PlayMakerFSM>();

		private List<PlayMakerFSM> shitNPCs = new List<PlayMakerFSM>();

		private PlayMakerFSM shitMan;

		private PlayMakerFSM shitMan2;

		private PlayMakerFSM shitMan3;

		private PlayMakerFSM shitMan4;

		private PlayMakerFSM shitMan5;

		private PlayMakerFSM map;

		private PlayMakerFSM grandma;

		private PlayMakerFSM strawberry;

		private PlayMakerFSM knob1;

		private PlayMakerFSM knob2;

		private PlayMakerFSM knob3;

		private PlayMakerFSM knob4;

		private PlayMakerFSM uncleDoor;

		private PlayMakerFSM UNCLE;

		private PlayMakerFSM vanKeys;

		private bool[] fleetariOrder = new bool[32];

		private GameObject fleetariOrderGO;

		private float steamID = SteamUser.GetSteamID().m_SteamID;

		public static string[] fleetariListItems = new string[]
		{
			"BrakeRepair", "CarPaintArt", "CarPaintGT", "CarPaintMetallic", "CarPaintRegular", "EngineAdjustment", "EngineRepair", "EngineTune", "FinalGearChange", "FleetariCarCall",
			"MetalBody", "MetalBootlid", "MetalBumperFront", "MetalBumperRear", "MetalDoorLeft", "MetalDoorRight", "MetalFenderLeft", "MetalFenderRight", "MetalGrille", "MetalHood",
			"N2OBottleFill", "RimPaintMetallic", "RimPaintRegular", "RimPolish", "Rollcage", "RollcageRemoval", "SuspensionRepair", "TireJobEuropeiska", "TireJobStandard", "TireJobSutasiko",
			"ToeAdjustment", "WindshieldRepair"
		};

		private bool receiver;

		public enum SwitchIDs
		{
			Fridge,
			Tap,
			Smoke,
			Radio,
			Radio1,
			RadioV,
			Tele,
			TV,
			Knob1,
			Knob2,
			Knob3,
			Knob4,
			Save,
			Lantern,
			shitMan,
			shitMan2,
			shitMan3,
			shitMan4,
			shitMan5,
			lidT,
			water,
			Firewood,
			FloorJack,
			Hoist
		}

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

			internal bool <HookEvents>b__86_7()
			{
				return false;
			}

			internal bool <HookEvents>b__86_12()
			{
				return false;
			}

			public static readonly PlayerHouse.<>c <>9 = new PlayerHouse.<>c();

			public static Func<bool> <>9__86_7;

			public static Func<bool> <>9__86_12;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass80_0
		{
			public <>c__DisplayClass80_0()
			{
			}

			internal bool <FindFSMs>b__0()
			{
				this.fsm.FsmStates[2].Actions[5].Enabled = true;
				this.fsm.FsmStates[2].Actions[6].Enabled = true;
				return false;
			}

			internal bool <FindFSMs>b__1()
			{
				this.fsm.FsmStates[2].Actions[5].Enabled = !Utils.IsReceiver(this.fsm);
				this.fsm.FsmStates[2].Actions[6].Enabled = !Utils.IsReceiver(this.fsm);
				return false;
			}

			internal bool <FindFSMs>b__2()
			{
				this.fsm.Fsm.States[0].Actions[3].Enabled = !Utils.IsReceiver(this.fsm);
				return false;
			}

			internal bool <FindFSMs>b__3()
			{
				this.fsm.Fsm.States[1].Actions[2].Enabled = !Utils.IsReceiver(this.fsm);
				return false;
			}

			internal bool <FindFSMs>b__4()
			{
				this.fsm.Fsm.States[0].Actions[2].Enabled = !Utils.IsReceiver(this.fsm);
				return false;
			}

			internal bool <FindFSMs>b__5()
			{
				this.fsm.Fsm.States[5].Actions[2].Enabled = !Utils.IsReceiver(this.fsm);
				return false;
			}

			public PlayMakerFSM fsm;
		}
	}
}
