using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class CarPartOld : MonoBehaviour
	{
		private void Awake()
		{
			this.Initialize();
		}

		public void Initialize()
		{
			if (this.Name != string.Empty)
			{
				return;
			}
			this.Name = base.gameObject.name;
			Logger.Debug("INITIATING CARPART " + this.Name);
			if ((base.gameObject.GetTopmostParent().name == "CARPARTS" && base.GetComponent<PlayMakerFSM>() == null) || (base.GetComponent<PlayMakerFSM>() == null && !base.gameObject.name.StartsWith("register plate")))
			{
				Object.Destroy(base.GetComponent<CarPartOld>());
				return;
			}
			if (base.name.Contains("xxx") || base.name.StartsWith("rearlight"))
			{
				this.hasxxx = true;
			}
			this.osc = base.gameObject.GetComponent<ObjectSyncComponent>();
			try
			{
				foreach (PlayMakerFSM playMakerFSM in base.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM.FsmName == "Removal")
					{
						this.removalFsm = playMakerFSM;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("GetFSM Error " + this.Name + ": " + ex.Message, MessageSeverity.Error);
			}
			try
			{
				Logger.Debug(string.Format("ATTEMPTING TO REGISTER CARPART {0}, HAS REMOVAL? {1}", this.Name, this.removalFsm != null));
				this.GetTrigger();
				this.GetFSM();
				this.CheckAttachment();
				this.HookEvents();
				Logger.Debug(string.Format("CARPART OFFICIALY REGISTERED {0}, HAS TRIGGER? {1}", this.Name, this.triggerGO != null));
			}
			catch (Exception ex2)
			{
				Chat.Instance.AddMessage(string.Format("{0} has error in CarPart {1}", base.gameObject.name, ex2), MessageSeverity.Error);
			}
		}

		private void AttachPart(CSteamID sender)
		{
			if (this.hasxxx)
			{
				this.AssembleXXX();
				return;
			}
			this.CheckPickupable(sender);
			if (GamePlayer.Instance.PickedUpObject != null && GamePlayer.Instance.PickedUpObject == base.gameObject)
			{
				GamePlayer.Instance.DropStolenObject();
			}
			this.AssemblePart();
			this.removeOSC();
		}

		private void CheckPickupable(CSteamID sender)
		{
			(this.osc.syncedObject as Pickupable).EnableParams();
			NetManager.Instance.GetPlayer(sender).ReadPickupMessage(false, null);
		}

		private void DetachPart()
		{
			this.removalFsm.SendEvent("MP_Remove part");
			this.AddOSC();
			this.DissassemblePart();
		}

		public void AssemblePart()
		{
		}

		public void TightenBolts()
		{
			if (this.Name.StartsWith("spark") && this.Name.Contains("plug"))
			{
				this.screwFsm.Fsm.GetFsmFloat("Tightness").Value = 8f;
				this.screwFsm.Fsm.GetFsmInt("Stage").Value = 7;
				this.screwFsm.SendEvent("TIGHTEN");
				this.useFsm.Fsm.GetFsmFloat("Tightness").Value = 8f;
				return;
			}
			if (this.Name.StartsWith("oil") && this.Name.Contains("filter"))
			{
				this.screwFsm.Fsm.GetFsmFloat("Tightness").Value = 8f;
				this.screwFsm.Fsm.GetFsmInt("Stage").Value = 7;
				this.screwFsm.SendEvent("MP_Screw");
				this.dataFSM.Fsm.GetFsmFloat("Tightness").Value = 8f;
				this.dataFSM.Fsm.GetFsmBool("Installed").Value = true;
			}
		}

		private void ActivateBolts()
		{
			foreach (GameObject gameObject in this.boltsGO)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(true);
				}
			}
			foreach (PlayMakerFSM playMakerFSM in this.bolts)
			{
				try
				{
					if (!playMakerFSM.gameObject.activeSelf)
					{
						playMakerFSM.gameObject.SetActive(true);
					}
					if (!playMakerFSM.transform.GetChild(0).gameObject.activeSelf)
					{
						playMakerFSM.transform.GetChild(0).gameObject.SetActive(true);
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void DissassemblePart()
		{
			try
			{
				if (!this.Name.StartsWith("fuse"))
				{
					if (this.Name.StartsWith("spark plug"))
					{
						base.gameObject.layer = 19;
					}
					this.triggerGO.SetActive(true);
					if (this.useFsm != null)
					{
						this.useFsm.enabled = false;
					}
					base.gameObject.transform.parent = null;
				}
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("Error detaching part " + ex.Message, MessageSeverity.Info);
			}
		}

		private void AssembleXXX()
		{
			for (int i = 0; i < this.triggerFsm.Fsm.GetState("Assemble").Actions.Length; i++)
			{
				if (this.triggerFsm.Fsm.GetState("Assemble").Actions[i].Name.Contains("DestroyObject"))
				{
					this.triggerFsm.Fsm.GetState("Assemble").Actions[i].Enabled = false;
				}
			}
			GameObject closestGameObject = this.GetClosestGameObject(this.relatedGOName);
			if (this.triggerFsm.Fsm.GetFsmGameObject("Part").Value == null)
			{
				GameVehicleDatabase.Instance.carParts.Remove(closestGameObject);
				Object.Destroy(closestGameObject);
			}
			else
			{
				GameObject gameObject = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value.gameObject;
				GameVehicleDatabase.Instance.carParts.Remove(gameObject);
				Object.Destroy(gameObject);
			}
			this.triggerFsm.SendEvent("MP_Assemble");
		}

		private void DissassembleXXX()
		{
			if (this.relatedGO == null)
			{
				using (IEnumerator<GameObject> enumerator = (from GameObject go in GameVehicleDatabase.Instance.carParts
					where go.name == this.relatedGOName
					select go).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						GameObject gameObject = enumerator.Current;
						this.relatedGO = gameObject;
					}
				}
			}
		}

		private void TightenBolt(string meshName)
		{
			try
			{
				if (meshName == "SPARK")
				{
					this.screwFsm.SendEvent("TIGHTEN");
				}
				else
				{
					if (meshName.StartsWith("NOMESH"))
					{
						using (List<GameObject>.Enumerator enumerator = this.boltsGO.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								GameObject gameObject = enumerator.Current;
								if (gameObject.name == "ValveAdjust")
								{
									gameObject.gameObject.transform.GetChild(int.Parse(meshName.Substring(meshName.Length - 1))).GetComponent<PlayMakerFSM>().SendEvent("TIGHTEN");
								}
							}
							goto IL_1B4;
						}
					}
					foreach (PlayMakerFSM playMakerFSM in this.bolts)
					{
						try
						{
							Transform child = playMakerFSM.gameObject.transform.GetChild(0);
							if (((child != null) ? child.name : null) == meshName)
							{
								if (!playMakerFSM.gameObject.activeSelf)
								{
									playMakerFSM.gameObject.SetActive(true);
								}
								playMakerFSM.SendEvent("TIGHTEN");
								if (this.Name.StartsWith("block") && GamePlayer.Instance.PickedUpObject != null && GamePlayer.Instance.PickedUpObject.name == base.gameObject.name && playMakerFSM.Fsm.GetFsmInt("Stage").Value > 3)
								{
									GamePlayer.Instance.DropStolenObject();
								}
							}
						}
						catch (Exception)
						{
						}
					}
					if (!this.dataFSM.Fsm.GetFsmBool("Bolted").Value)
					{
						this.CheckBolting();
					}
				}
				IL_1B4:;
			}
			catch (Exception)
			{
			}
		}

		private void UntightenBolt(string meshName)
		{
			try
			{
				if (meshName == "SPARK")
				{
					this.screwFsm.SendEvent("UNTIGHTEN");
				}
				else
				{
					if (meshName.StartsWith("NOMESH"))
					{
						using (List<GameObject>.Enumerator enumerator = this.boltsGO.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								GameObject gameObject = enumerator.Current;
								if (gameObject.name == "ValveAdjust")
								{
									gameObject.gameObject.transform.GetChild(int.Parse(meshName.Substring(meshName.Length - 1))).GetComponent<PlayMakerFSM>().SendEvent("UNTIGHTEN");
								}
							}
							goto IL_161;
						}
					}
					foreach (PlayMakerFSM playMakerFSM in this.bolts)
					{
						try
						{
							Transform child = playMakerFSM.gameObject.transform.GetChild(0);
							if (((child != null) ? child.name : null) == meshName)
							{
								if (!playMakerFSM.gameObject.activeSelf)
								{
									playMakerFSM.gameObject.SetActive(true);
								}
								playMakerFSM.SendEvent("UNTIGHTEN");
							}
						}
						catch (Exception ex)
						{
							Chat.Instance.AddMessage("Error: " + ex.Message, MessageSeverity.Error);
						}
					}
					if (this.dataFSM.Fsm.GetFsmBool("Bolted").Value)
					{
						this.CheckBolting();
					}
				}
				IL_161:;
			}
			catch (Exception)
			{
			}
		}

		private void CheckBolting()
		{
			double num = 0.0;
			foreach (PlayMakerFSM playMakerFSM in this.bolts)
			{
				num += (double)playMakerFSM.Fsm.GetFsmInt("Stage").Value;
			}
			num /= (double)this.bolts.Count;
			if (num >= 3.0)
			{
				this.dataFSM.Fsm.GetFsmBool("Bolted").Value = true;
				using (List<PlayMakerFSM>.Enumerator enumerator = this.relatedDBs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PlayMakerFSM playMakerFSM2 = enumerator.Current;
						playMakerFSM2.Fsm.GetFsmBool("Bolted").Value = true;
					}
					return;
				}
			}
			this.dataFSM.Fsm.GetFsmBool("Bolted").Value = false;
			foreach (PlayMakerFSM playMakerFSM3 in this.relatedDBs)
			{
				playMakerFSM3.Fsm.GetFsmBool("Bolted").Value = false;
			}
		}

		public void CheckAttachment()
		{
			if (this.Name.StartsWith("block") && base.gameObject.GetTopmostParent().name == "SATSUMA(557kg, 248)")
			{
				this.removeOSC();
			}
			else
			{
				this.AddOSC();
				if (this.rigidBody == null)
				{
					this.rigidBody = base.GetComponent<Rigidbody>();
				}
			}
			if (this.removalFsm == null)
			{
				return;
			}
			if (this.removalFsm.Active)
			{
				if (this.Name.Contains("alternator belt") && base.transform.GetChild(0).gameObject.activeSelf)
				{
					base.transform.GetChild(0).gameObject.SetActive(false);
				}
				this.removeOSC();
				return;
			}
			if (this.Name.Contains("alternator belt") && !base.transform.GetChild(0).gameObject.activeSelf)
			{
				base.transform.GetChild(0).gameObject.SetActive(true);
			}
			this.AddOSC();
			if (this.rigidBody == null)
			{
				this.rigidBody = base.GetComponent<Rigidbody>();
			}
		}

		private void Update()
		{
			try
			{
				if (Input.GetKeyDown(286))
				{
					if (this.bolts.Count > 0)
					{
						foreach (PlayMakerFSM playMakerFSM in this.bolts)
						{
							Logger.Debug(string.Format("{0} in {1} has screw of {2}", playMakerFSM, this.Name, playMakerFSM.GetComponent<PlayMakerFSM>().Fsm.GetFsmInt("Stage").Value));
						}
					}
					if (base.GetComponent<BoltOld>() != null)
					{
						Logger.Debug(string.Format("{0} has BOLTCMPNT and screw of {1} and tighness {2}", this.Name, this.screwFsm.Fsm.GetFsmInt("Stage").Value, this.screwFsm.Fsm.GetFsmFloat("Tightness").Value));
					}
				}
				if (!(MPController.Instance.boat == null))
				{
					if (this.osc == null)
					{
						this.osc = base.GetComponent<ObjectSyncComponent>();
					}
					else if (this.osc != null && this.screwFsm != null && (base.name.StartsWith("spark") || base.name.Contains("filter")) && base.GetComponent<BoltOld>() == null)
					{
						base.gameObject.AddComponent<BoltOld>().belongsToObjectID = this.osc.ObjectID;
					}
					this.CheckLateBolts();
					if (this.fluidFsm != null && this.fluidFsm.transform.parent.gameObject.activeSelf)
					{
						if (this.Name.StartsWith("oilpan("))
						{
							NetLocalPlayer.Instance.WriteCarPartFloatMessage(base.GetComponent<ObjectSyncComponent>(), this.fluidFsm.Fsm.GetFsmFloat("Fluid").Value);
						}
						else if (this.Name.StartsWith("fuel tank("))
						{
							NetLocalPlayer.Instance.WriteCarPartFloatMessage(base.GetComponent<ObjectSyncComponent>(), this.fluidFsm.Fsm.GetFsmFloat("Level").Value);
						}
						else if (this.Name.StartsWith("radiator(") || this.Name.Contains("master cylinder"))
						{
							NetLocalPlayer.Instance.WriteCarPartFloatMessage(base.GetComponent<ObjectSyncComponent>(), this.fluidFsm.Fsm.GetFsmFloat("Water").Value);
						}
					}
					if (this.fluidFsm2 != null && this.fluidFsm2.transform.parent.gameObject.activeSelf)
					{
						NetLocalPlayer.Instance.WriteCarPartFloatMessage(base.GetComponent<ObjectSyncComponent>(), this.fluidFsm2.Fsm.GetFsmFloat("Water").Value);
					}
					if (this.Name != base.gameObject.name)
					{
						Logger.Debug(string.Format("{0} has wrong name {1}, atteming to fix. HasTrig {2}", base.gameObject.name, this.Name, this.triggerGO != null));
						this.Name = base.gameObject.name;
						this.GetTrigger();
						Logger.Debug(string.Format("{0} Name fixed ({1}), HasTrig {2}", base.gameObject.name, this.Name, this.triggerGO != null));
					}
					this.triggerGO == null;
					this.CheckAttachment();
				}
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("{0} is bugging out in update {1}", this.Name, ex));
			}
		}

		private void CheckLateBolts()
		{
			if (this.Name == "block(Clone)" && this.boltsGO.Count == 0)
			{
				if (GameVehicleDatabase.Instance.motorBolts != null)
				{
					this.boltsGO.Add(GameVehicleDatabase.Instance.motorBolts);
				}
				if (this.boltsGO.Count > 0)
				{
					foreach (GameObject gameObject in this.boltsGO)
					{
						foreach (PlayMakerFSM playMakerFSM in gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
						{
							if (playMakerFSM.gameObject.name == "BoltPM")
							{
								foreach (PlayMakerFSM playMakerFSM2 in playMakerFSM.gameObject.transform.parent.GetComponentsInChildren<PlayMakerFSM>(true))
								{
									if (playMakerFSM2.gameObject.name == "BoltPM")
									{
										this.bolts.Add(playMakerFSM2.GetComponent<PlayMakerFSM>());
									}
								}
								break;
							}
						}
					}
					this.boltsLength = this.bolts.Count;
				}
			}
			else if (this.Name == "battery(Clone)" && this.boltsGO.Count < 2)
			{
				if (this.Name.StartsWith("battery"))
				{
					foreach (GameObject gameObject2 in GameVehicleDatabase.Instance.extraParts)
					{
						if (gameObject2.name.StartsWith("battery"))
						{
							PlayMakerFSM[] array = gameObject2.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
							int i = 0;
							if (i < array.Length)
							{
								PlayMakerFSM playMakerFSM3 = array[i];
								if (!this.boltsGO.Contains(playMakerFSM3.gameObject))
								{
									this.boltsGO.Add(playMakerFSM3.gameObject);
									this.BoltsGOName = this.boltsGO[0].name;
								}
							}
						}
					}
				}
				foreach (GameObject gameObject3 in this.boltsGO)
				{
					foreach (PlayMakerFSM playMakerFSM4 in gameObject3.GetComponentsInChildren<PlayMakerFSM>(true))
					{
						if (playMakerFSM4.gameObject.name == "BoltPM")
						{
							foreach (PlayMakerFSM playMakerFSM5 in playMakerFSM4.gameObject.transform.parent.GetComponentsInChildren<PlayMakerFSM>(true))
							{
								if (playMakerFSM5.gameObject.name == "BoltPM")
								{
									if (playMakerFSM5.transform.childCount > 0)
									{
										playMakerFSM5.transform.GetChild(0).gameObject.name = "bolt" + this.bolts.Count.ToString() + " 0";
									}
									this.bolts.Add(playMakerFSM5.GetComponent<PlayMakerFSM>());
								}
							}
							break;
						}
					}
				}
				this.boltsLength = this.bolts.Count;
			}
			else if (this.Name.StartsWith("starter") && this.bolts.Count < 3)
			{
				foreach (GameObject gameObject4 in GameVehicleDatabase.Instance.extraParts)
				{
					if (gameObject4.name.StartsWith("wiring_starter"))
					{
						PlayMakerFSM[] array = gameObject4.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
						for (int i = 0; i < array.Length; i++)
						{
							GameObject gameObject5 = array[i].gameObject;
							if (gameObject5.name == "BoltPM" && !this.bolts.Contains(gameObject5.GetComponent<PlayMakerFSM>()))
							{
								if (gameObject5.transform.childCount > 0)
								{
									gameObject5.transform.GetChild(0).gameObject.name = "bolt" + this.bolts.Count.ToString() + "0";
								}
								this.bolts.Add(gameObject5.GetComponent<PlayMakerFSM>());
								break;
							}
						}
					}
				}
				this.boltsLength = this.bolts.Count;
			}
			if (this.bolts.Count > 0 && this.osc != null && this.bolts[0].GetComponent<BoltOld>() == null)
			{
				foreach (PlayMakerFSM playMakerFSM6 in this.bolts)
				{
					playMakerFSM6.gameObject.AddComponent<BoltOld>().belongsToObjectID = this.osc.ObjectID;
				}
			}
		}

		private void AddOSC()
		{
			if (this.osc == null)
			{
				this.osc = base.GetComponent<ObjectSyncComponent>();
				if (this.osc == null)
				{
					if (this.hasxxx)
					{
						base.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Satsuma, -1);
						return;
					}
					base.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, -1);
					return;
				}
			}
			else if (!this.osc.enabled)
			{
				this.osc.enabled = true;
			}
		}

		public void removeOSC()
		{
			if (this.osc != null && this.osc.enabled)
			{
				this.osc.enabled = false;
			}
		}

		private void GetTrigger()
		{
			try
			{
				if (this.hasxxx && this.Name != "sub frame(Clone)")
				{
					foreach (Trigger trigger in GameVehicleDatabase.Instance.triggerGameObjects)
					{
						FsmGameObject fsmGameObject = trigger.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("ActivateThis");
						GameObject gameObject = ((fsmGameObject != null) ? fsmGameObject.Value : null);
						if (gameObject == null)
						{
							FsmGameObject fsmGameObject2 = trigger.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("ActivateThis1");
							gameObject = ((fsmGameObject2 != null) ? fsmGameObject2.Value : null);
						}
						if (gameObject != null)
						{
							foreach (PlayMakerFSM playMakerFSM in gameObject.GetComponents<PlayMakerFSM>())
							{
								if (playMakerFSM.FsmName == "Removal" && playMakerFSM.gameObject.name == base.gameObject.name)
								{
									this.triggerGO = trigger.gameObject;
									this.TriggerName = trigger.gameObject.name;
								}
							}
							if (gameObject.GetComponents<PlayMakerFSM>().Length == 0)
							{
								for (int j = 0; j < gameObject.transform.childCount; j++)
								{
									if (gameObject.name == base.transform.parent.name)
									{
										this.triggerGO = trigger.gameObject;
										this.TriggerName = trigger.gameObject.name;
									}
								}
							}
						}
					}
				}
				if (this.dataFSM == null)
				{
					foreach (GameObject gameObject2 in GameVehicleDatabase.Instance.dataGameObject)
					{
						try
						{
							if (gameObject2 != null)
							{
								if (this.hasxxx && this.Name != "sub frame(xxxxx)")
								{
									GameObject value = gameObject2.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("ActivateThis").Value;
									if (((value != null) ? value.gameObject.name : null) == base.gameObject.name)
									{
										this.dataFSM = gameObject2.GetComponent<PlayMakerFSM>();
										this.DataName = gameObject2.name;
									}
									else if (base.transform.parent != null && ((value != null) ? value.gameObject.name : null) == base.transform.parent.name)
									{
										this.dataFSM = gameObject2.GetComponent<PlayMakerFSM>();
										this.DataName = gameObject2.name;
									}
								}
								else
								{
									GameObject value2 = gameObject2.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("ThisPart").Value;
									if (((value2 != null) ? value2.gameObject.name : null) == base.gameObject.name)
									{
										if (gameObject2.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("Trigger").Value != null)
										{
											GameObject value3 = gameObject2.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("Trigger").Value;
											this.triggerGO = ((value3 != null) ? value3.gameObject : null);
										}
										else if (gameObject2.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("TriggerStock").Value != null)
										{
											GameObject value4 = gameObject2.GetComponent<PlayMakerFSM>().Fsm.GetFsmGameObject("TriggerStock").Value;
											this.triggerGO = ((value4 != null) ? value4.gameObject : null);
										}
										this.TriggerName = this.triggerGO.name;
										this.dataFSM = gameObject2.GetComponent<PlayMakerFSM>();
										this.DataName = gameObject2.name;
										Logger.Debug(this.triggerGO.name + " trigger added to " + base.gameObject.name);
										break;
									}
									if (this.Name == "sub frame(xxxxx)" && gameObject2.name == "Subframe")
									{
										using (IEnumerator<GameObject> enumerator3 = (from GameObject obj in GameVehicleDatabase.Instance.triggerGameObjects
											where obj.name == "trigger_subframe"
											select obj).GetEnumerator())
										{
											if (enumerator3.MoveNext())
											{
												GameObject gameObject3 = enumerator3.Current;
												this.triggerGO = gameObject3.gameObject;
												this.TriggerName = gameObject3.gameObject.name;
											}
										}
										this.relatedGOName = "sub frame(Clone)";
										this.dataFSM = gameObject2.GetComponent<PlayMakerFSM>();
										this.DataName = gameObject2.name;
									}
								}
							}
						}
						catch (Exception ex)
						{
							Logger.Debug(base.name + " error in checking for trig " + ex.Message);
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Chat.Instance.AddMessage(string.Format("GetTrigger Error in {0}: {1}", this.Name, ex2), MessageSeverity.Error);
			}
			try
			{
				if (this.Name.StartsWith("seat"))
				{
					this.seat = true;
				}
				else if (this.Name == "hood(Clone)" || base.name == "fiberglass hood(Clone)" || this.Name.StartsWith("door left") || this.Name.StartsWith("door right") || this.Name == "bootlid(Clone)")
				{
					this.hinge = true;
				}
				else if (this.Name == "fender left(Clone)" || this.Name == "fender right(Clone)")
				{
					this.fender = true;
				}
			}
			catch (Exception ex3)
			{
				Chat.Instance.AddMessage("GetTrigger2 Error in " + this.Name + ": " + ex3.Message, MessageSeverity.Error);
			}
			try
			{
				if (!(this.Name == "block(Clone)"))
				{
					foreach (PlayMakerFSM playMakerFSM2 in base.GetComponentsInChildren<PlayMakerFSM>(true))
					{
						if (playMakerFSM2.gameObject.name == "BoltPM")
						{
							if (playMakerFSM2.gameObject.transform.parent.name.StartsWith("Masked"))
							{
								if (!this.boltsGO.Contains(playMakerFSM2.gameObject.transform.parent.parent.gameObject))
								{
									this.boltsGO.Add(playMakerFSM2.gameObject.transform.parent.parent.gameObject);
									this.BoltsGOName = this.boltsGO[0].name;
								}
							}
							else if (this.Name.StartsWith("sub frame") && playMakerFSM2.gameObject.transform.parent.gameObject.name == "_Motor")
							{
								GameVehicleDatabase.Instance.motorBolts = playMakerFSM2.gameObject.transform.parent.gameObject;
							}
							else if (!this.boltsGO.Contains(playMakerFSM2.gameObject.transform.parent.gameObject))
							{
								this.boltsGO.Add(playMakerFSM2.gameObject.transform.parent.gameObject);
								this.BoltsGOName = this.boltsGO[0].name;
							}
						}
					}
					if (this.Name.StartsWith("shock absorber"))
					{
						if (base.gameObject.transform.parent.parent.parent != null)
						{
							foreach (PlayMakerFSM playMakerFSM3 in base.gameObject.transform.parent.parent.parent.FindChild("TrailArm").GetChild(0).FindChild("ShockBottom")
								.GetChild(0)
								.GetChild(0)
								.GetComponentsInChildren<PlayMakerFSM>(true))
							{
								if (!this.boltsGO.Contains(playMakerFSM3.gameObject.transform.parent.gameObject))
								{
									this.boltsGO.Add(playMakerFSM3.gameObject.transform.parent.gameObject);
									this.BoltsGOName = this.boltsGO[0].name;
								}
							}
						}
					}
					else
					{
						if (this.Name.StartsWith("battery"))
						{
							using (List<GameObject>.Enumerator enumerator2 = GameVehicleDatabase.Instance.extraParts.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									GameObject gameObject4 = enumerator2.Current;
									if (gameObject4.name.StartsWith("battery"))
									{
										foreach (PlayMakerFSM playMakerFSM4 in gameObject4.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
										{
											if (!this.boltsGO.Contains(playMakerFSM4.gameObject.transform.parent.gameObject))
											{
												this.boltsGO.Add(playMakerFSM4.gameObject.transform.parent.gameObject);
												this.BoltsGOName = this.boltsGO[0].name;
											}
										}
									}
								}
								goto IL_AAA;
							}
						}
						if (this.Name.StartsWith("strut"))
						{
							foreach (GameObject gameObject5 in GameVehicleDatabase.Instance.extraParts)
							{
								if (gameObject5.name.StartsWith("bolts_shock") && ((this.Name.Contains("fl") && gameObject5.name.Contains("fl")) || (this.Name.Contains("fr") && gameObject5.name.Contains("fr"))))
								{
									foreach (PlayMakerFSM playMakerFSM5 in gameObject5.GetComponentsInChildren<PlayMakerFSM>(true))
									{
										if (playMakerFSM5.gameObject.name == "BoltPM" && !this.boltsGO.Contains(playMakerFSM5.gameObject.transform.parent.gameObject))
										{
											this.boltsGO.Add(playMakerFSM5.gameObject.transform.parent.gameObject);
											this.BoltsGOName = this.boltsGO[0].name;
										}
									}
								}
							}
						}
					}
					IL_AAA:
					if (this.boltsGO.Count == 0 && this.hasxxx && base.transform.parent != null)
					{
						if (this.Name.StartsWith("steering rod"))
						{
							foreach (PlayMakerFSM playMakerFSM6 in base.gameObject.GetTopmostParent().GetComponentsInChildren<PlayMakerFSM>(true))
							{
								if (playMakerFSM6.gameObject.name == "BoltPM")
								{
									if (this.Name.Contains("fl"))
									{
										try
										{
											if (playMakerFSM6.gameObject.transform.parent.name == "pivot_shock_fl")
											{
												if (!this.boltsGO.Contains(playMakerFSM6.gameObject.transform.parent.gameObject))
												{
													this.boltsGO.Add(playMakerFSM6.gameObject.transform.parent.gameObject);
													this.BoltsGOName = this.boltsGO[0].name;
												}
											}
											else if (playMakerFSM6.gameObject.transform.parent.name == "pivot_steering_arm_fl" && !this.boltsGO.Contains(playMakerFSM6.gameObject.transform.parent.gameObject))
											{
												this.boltsGO.Add(playMakerFSM6.gameObject.transform.parent.gameObject);
												this.BoltsGOName = this.boltsGO[0].name;
											}
											goto IL_D79;
										}
										catch
										{
											goto IL_D79;
										}
									}
									if (this.Name.Contains("fr") && playMakerFSM6.gameObject.name == "BoltPM")
									{
										if (playMakerFSM6.gameObject.transform.parent.name == "pivot_shock_fr")
										{
											if (!this.boltsGO.Contains(playMakerFSM6.gameObject.transform.parent.gameObject))
											{
												this.boltsGO.Add(playMakerFSM6.gameObject.transform.parent.gameObject);
												this.BoltsGOName = this.boltsGO[0].name;
											}
										}
										else if (playMakerFSM6.gameObject.transform.parent.name == "pivot_steering_arm_fr" && !this.boltsGO.Contains(playMakerFSM6.gameObject.transform.parent.gameObject))
										{
											this.boltsGO.Add(playMakerFSM6.gameObject.transform.parent.gameObject);
											this.BoltsGOName = this.boltsGO[0].name;
										}
									}
								}
								IL_D79:;
							}
						}
						else
						{
							foreach (PlayMakerFSM playMakerFSM7 in base.transform.parent.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
							{
								if (playMakerFSM7.gameObject.name == "BoltPM")
								{
									if (playMakerFSM7.gameObject.transform.parent.name.StartsWith("Masked"))
									{
										if (!this.boltsGO.Contains(playMakerFSM7.gameObject.transform.parent.parent.gameObject))
										{
											this.boltsGO.Add(playMakerFSM7.gameObject.transform.parent.gameObject);
											this.BoltsGOName = this.boltsGO[0].name;
										}
									}
									else if ((this.Name.Contains("wishbone") || this.Name.Contains("halfshaft")) && !this.boltsGO.Contains(playMakerFSM7.gameObject.transform.parent.gameObject))
									{
										this.boltsGO.Add(playMakerFSM7.gameObject.transform.parent.gameObject);
										this.BoltsGOName = this.boltsGO[0].name;
									}
								}
							}
						}
					}
					if (this.boltsGO.Count > 0)
					{
						foreach (GameObject gameObject6 in this.boltsGO)
						{
							foreach (PlayMakerFSM playMakerFSM8 in gameObject6.GetComponentsInChildren<PlayMakerFSM>(true))
							{
								if (playMakerFSM8.gameObject.name == "BoltPM")
								{
									foreach (PlayMakerFSM playMakerFSM9 in playMakerFSM8.gameObject.transform.parent.GetComponentsInChildren<PlayMakerFSM>(true))
									{
										if (playMakerFSM9.gameObject.name == "BoltPM" && !this.bolts.Contains(playMakerFSM9))
										{
											if (playMakerFSM9.transform.parent.name.StartsWith("carburator"))
											{
												EventHook.AddWithSync(playMakerFSM9, "Screw", null, false);
												EventHook.AddWithSync(playMakerFSM9, "Unscrew", null, false);
												playMakerFSM9.transform.GetChild(0).gameObject.name = "bolt4 0";
											}
											if (playMakerFSM9.transform.parent.name.StartsWith("pivot_steering_arm"))
											{
												playMakerFSM9.transform.GetChild(0).gameObject.name = "bolt5 0";
											}
											this.bolts.Add(playMakerFSM9.GetComponent<PlayMakerFSM>());
										}
									}
									break;
								}
							}
						}
						this.boltsLength = this.bolts.Count;
					}
				}
			}
			catch (Exception ex4)
			{
				Chat.Instance.AddMessage("GetTrigger3 Error in " + this.Name + ": " + ex4.Message, MessageSeverity.Error);
			}
		}

		private void GetFSM()
		{
			try
			{
				if (this.triggerGO != null)
				{
					foreach (PlayMakerFSM playMakerFSM in this.triggerGO.GetComponentsInChildren<PlayMakerFSM>(true))
					{
						if (playMakerFSM.FsmName == "Assembly")
						{
							this.triggerFsm = playMakerFSM;
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("GetFSM2 Error " + this.Name + ": " + ex.Message, MessageSeverity.Error);
			}
			try
			{
				if (this.Name.StartsWith("block"))
				{
					foreach (PlayMakerFSM playMakerFSM2 in base.GetComponents<PlayMakerFSM>())
					{
						if (playMakerFSM2.FsmName == "BoltCheck" && this.boltCheckFsm == null)
						{
							this.boltCheckFsm = playMakerFSM2;
							break;
						}
					}
				}
				else
				{
					foreach (PlayMakerFSM playMakerFSM3 in base.GetComponentsInChildren<PlayMakerFSM>(true))
					{
						if (playMakerFSM3.FsmName == "BoltCheck" && this.boltCheckFsm == null)
						{
							this.boltCheckFsm = playMakerFSM3;
							for (int j = 1; j < 4; j++)
							{
								if (this.boltCheckFsm.Fsm.GetFsmGameObject("db_BoltThis" + j.ToString()).Value != null)
								{
									this.relatedDBs.Add(this.boltCheckFsm.Fsm.GetFsmGameObject("db_BoltThis" + j.ToString()).Value.GetComponent<PlayMakerFSM>());
								}
							}
						}
						else if (playMakerFSM3.gameObject.name == "OpenCap" && playMakerFSM3.FsmName == "Screw")
						{
							if (this.capFsm == null)
							{
								this.capFsm = playMakerFSM3;
							}
							else if (this.capFsm2 == null)
							{
								this.capFsm2 = playMakerFSM3;
							}
						}
						else if (playMakerFSM3.FsmName == "HandRotate" && (playMakerFSM3.gameObject.name == "Pivot" || this.Name.StartsWith("distributor")))
						{
							this.handRotateFsm = playMakerFSM3;
						}
						else if (playMakerFSM3.gameObject.name == "Pin" && playMakerFSM3.FsmName == "Screw")
						{
							EventHook.AddWithSync(playMakerFSM3, "State 3", null, false);
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Chat.Instance.AddMessage("GetFSM3 Error " + this.Name + ": " + ex2.Message, MessageSeverity.Error);
			}
			try
			{
				foreach (PlayMakerFSM playMakerFSM4 in base.GetComponents<PlayMakerFSM>())
				{
					if (playMakerFSM4.FsmName == "Use")
					{
						this.useFsm = playMakerFSM4;
					}
					if ((this.Name.Contains("oil") || this.Name.StartsWith("spark")) && playMakerFSM4.FsmName == "Screw")
					{
						this.screwFsm = playMakerFSM4;
					}
				}
			}
			catch (Exception ex3)
			{
				Chat.Instance.AddMessage("GetFSM4 Error " + this.Name + ": " + ex3.Message, MessageSeverity.Error);
			}
			try
			{
				foreach (PlayMakerFSM playMakerFSM5 in GameVehicleDatabase.Instance.fluidFsms)
				{
					if (playMakerFSM5.transform.parent.name == "MotorOil" && this.Name.StartsWith("oilpan("))
					{
						this.fluidFsm = playMakerFSM5;
						break;
					}
					if (playMakerFSM5.transform.parent.name == "GasolineCar" && this.Name.StartsWith("fuel tank("))
					{
						this.fluidFsm = playMakerFSM5;
						break;
					}
					if (playMakerFSM5.transform.parent.name == "Radiator1" && this.Name.StartsWith("radiator("))
					{
						this.fluidFsm = playMakerFSM5;
						break;
					}
					if (playMakerFSM5.transform.parent.name == "BrakeR" && this.Name.Contains("brake master cylinder"))
					{
						this.fluidFsm = playMakerFSM5;
					}
					else if (playMakerFSM5.transform.parent.name == "BrakeF" && this.Name.Contains("brake master cylinder"))
					{
						this.fluidFsm2 = playMakerFSM5;
					}
					else if (playMakerFSM5.transform.parent.name == "Clutch" && this.Name.Contains("clutch master cylinder"))
					{
						this.fluidFsm = playMakerFSM5;
						break;
					}
				}
			}
			catch (Exception ex4)
			{
				Chat.Instance.AddMessage("GetFSM5 Error " + this.Name + ": " + ex4.Message, MessageSeverity.Error);
			}
		}

		private void HookEvents()
		{
			if (this.removalFsm != null && this.counter < 1)
			{
				try
				{
					FsmStateAction[] actions = this.removalFsm.Fsm.States[0].Actions;
				}
				catch (Exception ex)
				{
					Logger.Debug(string.Format("Fatal removal error in setting up {0}. ERROR: {1}", this.Name, ex));
					this.DetachFSM(this.removalFsm.gameObject);
					this.HookEvents();
					return;
				}
				if (this.hasxxx)
				{
					EventHook.AddWithSync(this.removalFsm, "Remove part", null, false);
				}
				else
				{
					EventHook.Add(this.removalFsm, "Remove part", delegate
					{
						NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), false);
						return false;
					}, false, false);
				}
				this.counter++;
			}
			if (this.Name.StartsWith("block") && this.boltCheckFsm != null)
			{
				EventHook.Add(this.boltCheckFsm, "Bolts OFF", delegate
				{
					if (this.osc != null)
					{
						this.osc.enabled = true;
					}
					return false;
				}, false, false);
				EventHook.Add(this.boltCheckFsm, "Bolts ON", delegate
				{
					if (this.osc != null)
					{
						this.osc.enabled = false;
					}
					return false;
				}, false, false);
			}
			if (this.handRotateFsm != null)
			{
				EventHook.AddWithSync(this.handRotateFsm, "Wait", null, false);
				EventHook.AddWithSync(this.handRotateFsm, "Clockwise", null, false);
				EventHook.AddWithSync(this.handRotateFsm, "Counterwise", null, false);
			}
			if (this.triggerFsm != null && !this.hasxxx && this.counter < 2)
			{
				try
				{
					FsmStateAction[] actions2 = this.triggerFsm.Fsm.States[0].Actions;
				}
				catch (Exception ex2)
				{
					Logger.Debug(string.Format("Fatal trioggerFSM non xxx error in setting up {0}. ERROR: {1}", this.Name, ex2));
					this.DetachFSM(this.triggerFsm.gameObject);
					return;
				}
				Logger.Debug(string.Format("TRIGGER OBJECT {0}. HAS PART? {1}", this.triggerFsm.gameObject.name, this.triggerFsm.Fsm.GetFsmGameObject("Part").Value != null));
				if (!PlayerHouse.HasBeenSynced(this.triggerFsm))
				{
					EventHook.Add(this.triggerFsm, "Wait", () => false, false, false);
					EventHook.Add(this.triggerFsm, "Wait 2", () => false, false, false);
					EventHook.Add(this.triggerFsm, "Wait 3", () => false, false, false);
					EventHook.Add(this.triggerFsm, "Assemble", delegate
					{
						GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
						if (value != null)
						{
							NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
						}
						else
						{
							NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
						}
						return false;
					}, false, false);
					EventHook.Add(this.triggerFsm, "Assemble 2", delegate
					{
						GameObject value2 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
						if (value2 != null)
						{
							NetLocalPlayer.Instance.WriteCarPartMessage(value2.GetComponent<ObjectSyncComponent>(), true);
						}
						else
						{
							NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
						}
						return false;
					}, false, false);
					if (this.triggerGO.name.Contains("steering_wheel"))
					{
						if (!PlayerHouse.HasBeenSynced(this.triggerFsm))
						{
							EventHook.Add(this.triggerFsm, "Wait", delegate
							{
								GameObject value3 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
								if (value3 != null)
								{
									NetLocalPlayer.Instance.WriteCarPartMessage(value3.GetComponent<ObjectSyncComponent>(), true);
								}
								else
								{
									NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
								}
								return false;
							}, false, false);
							EventHook.Add(this.triggerFsm, "Wait 2", delegate
							{
								GameObject value4 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
								if (value4 != null)
								{
									NetLocalPlayer.Instance.WriteCarPartMessage(value4.GetComponent<ObjectSyncComponent>(), true);
								}
								else
								{
									NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
								}
								return false;
							}, false, false);
							EventHook.Add(this.triggerFsm, "Wait 3", delegate
							{
								GameObject value5 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
								if (value5 != null)
								{
									NetLocalPlayer.Instance.WriteCarPartMessage(value5.GetComponent<ObjectSyncComponent>(), true);
								}
								else
								{
									NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
								}
								return false;
							}, false, false);
						}
						EventHook.Add(this.triggerFsm, "Wait 4", delegate
						{
							GameObject value6 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
							if (value6 != null)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(value6.GetComponent<ObjectSyncComponent>(), true);
							}
							else
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
							}
							return false;
						}, false, false);
					}
					else if (this.triggerGO.name.Contains("trigger_hood"))
					{
						EventHook.Add(this.triggerFsm, "Wait 3", delegate
						{
							GameObject value7 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
							if (value7 != null)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(value7.GetComponent<ObjectSyncComponent>(), true);
							}
							else
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
							}
							return false;
						}, false, false);
					}
					else
					{
						EventHook.Add(this.triggerFsm, "Wait 4", () => false, false, false);
					}
				}
				this.counter++;
			}
			else if (this.triggerFsm != null && this.counter < 2)
			{
				EventHook.Add(this.triggerFsm, "Wait", () => false, false, false);
				EventHook.Add(this.triggerFsm, "Wait 2", () => false, false, false);
				EventHook.Add(this.triggerFsm, "Wait 3", () => false, false, false);
				EventHook.Add(this.triggerFsm, "Wait 4", () => false, false, false);
				try
				{
					FsmStateAction[] actions3 = this.triggerFsm.Fsm.States[0].Actions;
				}
				catch (Exception ex3)
				{
					Logger.Debug(string.Format("Fatal trigger XXX error in setting up {0}. ERROR: {1}", this.Name, ex3));
					this.DetachFSM(this.triggerFsm.gameObject);
					return;
				}
				EventHook.AddWithSync(this.triggerFsm, "Assemble", delegate
				{
					try
					{
						if (this.triggerFsm.Fsm.GetFsmGameObject("Part").Value == null)
						{
							GameObject closestGameObject = this.GetClosestGameObject(this.relatedGOName);
							GameVehicleDatabase.Instance.carParts.Remove(closestGameObject);
							Object.Destroy(closestGameObject);
						}
						else
						{
							GameObject gameObject = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value.gameObject;
							GameVehicleDatabase.Instance.carParts.Remove(gameObject);
							Object.Destroy(gameObject);
						}
					}
					catch
					{
						Chat.Instance.AddMessage("Failed shit on " + this.triggerFsm.FsmName, MessageSeverity.Info);
					}
					return false;
				}, false);
				EventHook.AddWithSync(this.triggerFsm, "Assemble 2", delegate
				{
					try
					{
						if (this.triggerFsm.Fsm.GetFsmGameObject("Part").Value == null)
						{
							GameObject closestGameObject2 = this.GetClosestGameObject(this.relatedGOName);
							GameVehicleDatabase.Instance.carParts.Remove(closestGameObject2);
							Object.Destroy(closestGameObject2);
						}
						else
						{
							GameObject gameObject2 = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value.gameObject;
							GameVehicleDatabase.Instance.carParts.Remove(gameObject2);
							Object.Destroy(gameObject2);
						}
					}
					catch
					{
						Chat.Instance.AddMessage("Failed shit on " + this.triggerFsm.FsmName, MessageSeverity.Info);
					}
					return false;
				}, false);
				this.counter++;
			}
			if (this.boltCheckFsm != null && this.counter < 3)
			{
				try
				{
					FsmStateAction[] actions4 = this.boltCheckFsm.Fsm.States[0].Actions;
				}
				catch (Exception ex4)
				{
					Logger.Debug(string.Format("Fatal bolt error in setting up {0}. ERROR: {1}", this.Name, ex4));
					this.DetachFSM(this.boltCheckFsm.gameObject);
					return;
				}
				if (base.gameObject.name.Contains("camshaft gear"))
				{
					EventHook.Add(this.boltCheckFsm, "Adjust", delegate
					{
						if (!this.camIsSynced && !this.receiver)
						{
							NetLocalPlayer.Instance.WriteCarPartFloatMessage(base.GetComponent<ObjectSyncComponent>(), this.boltCheckFsm.Fsm.GetFsmFloat("Angle").Value);
						}
						return false;
					}, false, false);
				}
				this.counter++;
			}
			if (this.screwFsm != null && base.name.Contains("filter"))
			{
				EventHook.AddWithSync(this.screwFsm, "Screw", delegate
				{
					if (this.dataFSM == null)
					{
						this.dataFSM = this.triggerFsm.GetComponent<TriggerOld>().dataFSM;
					}
					this.dataFSM.Fsm.GetFsmFloat("Tightness").Value = (float)this.screwFsm.Fsm.GetFsmInt("Stage").Value + 1f;
					return false;
				}, false);
				EventHook.AddWithSync(this.screwFsm, "Unscrew", delegate
				{
					if (this.dataFSM == null)
					{
						this.dataFSM = this.triggerFsm.GetComponent<TriggerOld>().dataFSM;
					}
					this.dataFSM.Fsm.GetFsmFloat("Tightness").Value = (float)this.screwFsm.Fsm.GetFsmInt("Stage").Value - 1f;
					return false;
				}, false);
			}
			if (this.capFsm != null)
			{
				EventHook.AddWithSync(this.capFsm, "Mouse off 2", null, false);
				EventHook.AddWithSync(this.capFsm, "Wait", null, false);
				EventHook.AddWithSync(this.capFsm, "Screw", null, false);
				EventHook.AddWithSync(this.capFsm, "Unscrew", null, false);
				EventHook.AddWithSync(this.capFsm, "State 1", null, false);
				EventHook.AddWithSync(this.capFsm, "State 2", null, false);
			}
			if (this.capFsm2 != null)
			{
				EventHook.AddWithSync(this.capFsm2, "Mouse off 2", null, false);
				EventHook.AddWithSync(this.capFsm2, "Wait", null, false);
				EventHook.AddWithSync(this.capFsm2, "Screw", null, false);
				EventHook.AddWithSync(this.capFsm2, "Unscrew", null, false);
				EventHook.AddWithSync(this.capFsm2, "State 1", null, false);
				EventHook.AddWithSync(this.capFsm2, "State 2", null, false);
			}
		}

		public void DetachFSM(GameObject gameObject)
		{
			this.originalParent = gameObject.transform.parent;
			gameObject.transform.parent = null;
			if (!gameObject.activeSelf)
			{
				this.wasDisabled = true;
			}
			gameObject.SetActive(true);
			this.HookEvents();
			this.ReattachFSM(gameObject);
		}

		private void ReattachFSM(GameObject gameObject)
		{
			if (this.wasDisabled)
			{
				gameObject.SetActive(false);
			}
			gameObject.transform.parent = this.originalParent;
		}

		private GameObject GetClosestGameObjectFromArray(Transform from, List<GameObject> objects)
		{
			float num = 100f;
			GameObject gameObject = null;
			foreach (GameObject gameObject2 in objects)
			{
				float num2 = Vector3.Distance(from.position, gameObject2.transform.position);
				if (num2 < num)
				{
					num = num2;
					gameObject = gameObject2;
				}
			}
			return gameObject;
		}

		private GameObject GetClosestGameObject(string name)
		{
			GameObject gameObject3;
			try
			{
				float num = 100f;
				GameObject gameObject = null;
				foreach (GameObject gameObject2 in GameVehicleDatabase.Instance.carParts)
				{
					if (gameObject2 != null && gameObject2.name == name)
					{
						float num2 = Vector3.Distance(base.gameObject.transform.position, gameObject2.transform.position);
						if (num2 < num)
						{
							num = num2;
							gameObject = gameObject2;
						}
					}
				}
				gameObject3 = gameObject;
			}
			catch (Exception ex)
			{
				Chat instance = Chat.Instance;
				string text = "Failed no closest go: ";
				Exception ex2 = ex;
				instance.AddMessage(text + ((ex2 != null) ? ex2.ToString() : null), MessageSeverity.Error);
				gameObject3 = null;
			}
			return gameObject3;
		}

		public void ReadSwitchMessage(bool assemble, CSteamID sender)
		{
			if (assemble)
			{
				this.AttachPart(sender);
				return;
			}
			this.DetachPart();
		}

		public void ReadBoltMessage(bool tighten, string boltMesh)
		{
			this.receiver = true;
			if (tighten)
			{
				this.TightenBolt(boltMesh);
			}
			else
			{
				this.UntightenBolt(boltMesh);
			}
			this.receiver = false;
		}

		public void ReadPaintMessage(Color color)
		{
			GamePlayer.Instance.PaintObject(base.gameObject, color);
		}

		public void ReadFloatMessage(float value)
		{
			if (!this.Name.Contains("camshaft gear") || !(this.boltCheckFsm != null))
			{
				if (this.Name.StartsWith("radiator("))
				{
					if (value > this.dataFSM.Fsm.GetFsmFloat("Water").Value)
					{
						this.dataFSM.Fsm.GetFsmFloat("Water").Value = value;
						return;
					}
				}
				else if (this.Name.StartsWith("oilpan("))
				{
					if (value > this.dataFSM.Fsm.GetFsmFloat("Oil").Value)
					{
						this.dataFSM.Fsm.GetFsmFloat("Oil").Value = value;
						return;
					}
				}
				else
				{
					if (this.Name.Contains("brake master cyl"))
					{
						this.dataFSM.Fsm.GetFsmFloat("BrakeFluidR").Value = value;
						this.dataFSM.Fsm.GetFsmFloat("BrakeFluidF").Value = value;
						return;
					}
					if (this.Name.StartsWith("fuel tank("))
					{
						if (value > this.dataFSM.Fsm.GetFsmFloat("FuelLevel").Value)
						{
							this.dataFSM.Fsm.GetFsmFloat("FuelLevel").Value = value;
							return;
						}
					}
					else if (this.Name.Contains("clutch master cyl") && value > this.dataFSM.Fsm.GetFsmFloat("ClutchFluid").Value)
					{
						this.dataFSM.Fsm.GetFsmFloat("ClutchFluid").Value = value;
					}
				}
				return;
			}
			if (base.gameObject.transform.GetChild(1).localEulerAngles.x - value <= 5.1f && base.gameObject.transform.GetChild(1).localEulerAngles.x - value >= -5.1f)
			{
				this.camIsSynced = true;
				return;
			}
			base.gameObject.transform.GetChild(1).localEulerAngles = new Vector3(value - 5f, 180f, 180f);
		}

		public CarPartOld()
		{
		}

		[CompilerGenerated]
		private bool <DissassembleXXX>b__40_0(GameObject go)
		{
			return go.name == this.relatedGOName;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_0()
		{
			NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), false);
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_1()
		{
			if (this.osc != null)
			{
				this.osc.enabled = true;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_2()
		{
			if (this.osc != null)
			{
				this.osc.enabled = false;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_6()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_7()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_8()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_9()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_10()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_11()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_12()
		{
			GameObject value = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value;
			if (value != null)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(value.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_18()
		{
			try
			{
				if (this.triggerFsm.Fsm.GetFsmGameObject("Part").Value == null)
				{
					GameObject closestGameObject = this.GetClosestGameObject(this.relatedGOName);
					GameVehicleDatabase.Instance.carParts.Remove(closestGameObject);
					Object.Destroy(closestGameObject);
				}
				else
				{
					GameObject gameObject = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value.gameObject;
					GameVehicleDatabase.Instance.carParts.Remove(gameObject);
					Object.Destroy(gameObject);
				}
			}
			catch
			{
				Chat.Instance.AddMessage("Failed shit on " + this.triggerFsm.FsmName, MessageSeverity.Info);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_19()
		{
			try
			{
				if (this.triggerFsm.Fsm.GetFsmGameObject("Part").Value == null)
				{
					GameObject closestGameObject = this.GetClosestGameObject(this.relatedGOName);
					GameVehicleDatabase.Instance.carParts.Remove(closestGameObject);
					Object.Destroy(closestGameObject);
				}
				else
				{
					GameObject gameObject = this.triggerFsm.Fsm.GetFsmGameObject("Part").Value.gameObject;
					GameVehicleDatabase.Instance.carParts.Remove(gameObject);
					Object.Destroy(gameObject);
				}
			}
			catch
			{
				Chat.Instance.AddMessage("Failed shit on " + this.triggerFsm.FsmName, MessageSeverity.Info);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_20()
		{
			if (!this.camIsSynced && !this.receiver)
			{
				NetLocalPlayer.Instance.WriteCarPartFloatMessage(base.GetComponent<ObjectSyncComponent>(), this.boltCheckFsm.Fsm.GetFsmFloat("Angle").Value);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_21()
		{
			if (this.dataFSM == null)
			{
				this.dataFSM = this.triggerFsm.GetComponent<TriggerOld>().dataFSM;
			}
			this.dataFSM.Fsm.GetFsmFloat("Tightness").Value = (float)this.screwFsm.Fsm.GetFsmInt("Stage").Value + 1f;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__52_22()
		{
			if (this.dataFSM == null)
			{
				this.dataFSM = this.triggerFsm.GetComponent<TriggerOld>().dataFSM;
			}
			this.dataFSM.Fsm.GetFsmFloat("Tightness").Value = (float)this.screwFsm.Fsm.GetFsmInt("Stage").Value - 1f;
			return false;
		}

		public GameObject triggerGO;

		private List<GameObject> boltsGO = new List<GameObject>();

		private GameObject relatedGO;

		private Rigidbody rigidBody;

		private List<PlayMakerFSM> bolts = new List<PlayMakerFSM>();

		private List<PlayMakerFSM> relatedDBs = new List<PlayMakerFSM>();

		public PlayMakerFSM dataFSM;

		public PlayMakerFSM removalFsm;

		public PlayMakerFSM triggerFsm;

		private PlayMakerFSM useFsm;

		private PlayMakerFSM screwFsm;

		private PlayMakerFSM boltCheckFsm;

		private PlayMakerFSM handRotateFsm;

		private PlayMakerFSM capFsm;

		private PlayMakerFSM capFsm2;

		private PlayMakerFSM fluidFsm;

		private PlayMakerFSM fluidFsm2;

		private ObjectSyncComponent osc;

		public string Name = string.Empty;

		public string TriggerName = string.Empty;

		public string relatedGOName = string.Empty;

		public string DataName = string.Empty;

		public string BoltsGOName = string.Empty;

		public int boltsLength;

		public bool seat;

		public bool hinge;

		public bool fender;

		public bool receiver;

		private bool camIsSynced;

		public bool hasxxx;

		private int counter;

		private Transform originalParent;

		private bool wasDisabled;

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

			internal bool <GetTrigger>b__49_0(GameObject obj)
			{
				return obj.name == "trigger_subframe";
			}

			internal bool <HookEvents>b__52_3()
			{
				return false;
			}

			internal bool <HookEvents>b__52_4()
			{
				return false;
			}

			internal bool <HookEvents>b__52_5()
			{
				return false;
			}

			internal bool <HookEvents>b__52_13()
			{
				return false;
			}

			internal bool <HookEvents>b__52_14()
			{
				return false;
			}

			internal bool <HookEvents>b__52_15()
			{
				return false;
			}

			internal bool <HookEvents>b__52_16()
			{
				return false;
			}

			internal bool <HookEvents>b__52_17()
			{
				return false;
			}

			public static readonly CarPartOld.<>c <>9 = new CarPartOld.<>c();

			public static Func<GameObject, bool> <>9__49_0;

			public static Func<bool> <>9__52_3;

			public static Func<bool> <>9__52_4;

			public static Func<bool> <>9__52_5;

			public static Func<bool> <>9__52_13;

			public static Func<bool> <>9__52_14;

			public static Func<bool> <>9__52_15;

			public static Func<bool> <>9__52_16;

			public static Func<bool> <>9__52_17;
		}
	}
}
