using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	public class Pickupable : MonoBehaviour, ISyncedObject
	{
		public Rigidbody rigidbody
		{
			get
			{
				if (this.rb == null && this.ShouldHaveRB())
				{
					this.rb = base.gameObject.GetComponent<Rigidbody>();
				}
				return this.rb;
			}
		}

		public Pickupable()
		{
			this.rb = base.GetComponent<Rigidbody>();
			this.syncComponent = base.GetComponent<ObjectSyncComponent>();
			foreach (Collider collider in base.GetComponents<Collider>())
			{
				if (collider.enabled)
				{
					this.activeColliders.Add(collider);
				}
			}
			if (base.name == "Sausage-Potatoes(Clone)")
			{
				new PubFood(base.gameObject);
				return;
			}
			if (base.name.StartsWith("motor hoist"))
			{
				base.gameObject.AddComponent<Tool>();
				this.objectType = Pickupable.SubType.Tool;
				return;
			}
			PlayMakerFSM[] array;
			if (base.name.StartsWith("fish trap"))
			{
				foreach (PlayMakerFSM playMakerFSM in base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					string name = playMakerFSM.gameObject.name;
					if (!(name == "PickFish"))
					{
						if (!(name == "Trigger"))
						{
							if (name == "Logic")
							{
								if (playMakerFSM.FsmName == "Logic" && playMakerFSM.transform.parent.name.StartsWith("fish"))
								{
									EventHook.AddWithSync(playMakerFSM, "Trap fish", null, false);
								}
							}
						}
						else if (playMakerFSM.FsmName == "Use" && playMakerFSM.gameObject.transform.parent.name == "HatchPivot")
						{
							EventHook.AddWithSync(playMakerFSM, "Bool test", null, false);
						}
					}
					else if (playMakerFSM.FsmName == "Use")
					{
						EventHook.AddWithSync(playMakerFSM, "Bool test", null, false);
					}
				}
			}
			else if (base.name.StartsWith("rocket"))
			{
				foreach (PlayMakerFSM playMakerFSM2 in base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM2.gameObject.name == "Trigger")
					{
						EventHook.AddWithSync(playMakerFSM2, "ON 3", null, false);
						break;
					}
				}
			}
			else if (base.name.StartsWith("rocket"))
			{
				foreach (PlayMakerFSM playMakerFSM3 in base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM3.gameObject.name == "Trigger")
					{
						EventHook.AddWithSync(playMakerFSM3, "ON 3", null, false);
						break;
					}
				}
			}
			else
			{
				if (base.name.StartsWith("tv remote"))
				{
					PlayMakerFSM component = base.gameObject.GetComponent<PlayMakerFSM>();
					EventHook.AddWithSync(component, "Wait player", null, false);
					EventHook.AddWithSync(component, "Bool test", null, false);
					return;
				}
				if (base.name == "spanner set(itemx)")
				{
					EventHook.AddWithSync(base.transform.GetChild(0).GetChild(0).GetComponent<PlayMakerFSM>(), "Bool test", null, false);
					return;
				}
				if (base.name == "ratchet set(Clone)")
				{
					EventHook.AddWithSync(base.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetComponent<PlayMakerFSM>(), "Bool test", null, false);
					return;
				}
				if (base.name.StartsWith("alarm clock"))
				{
					PlayMakerFSM mainFsm = base.gameObject.GetComponent<PlayMakerFSM>();
					GameHouseDatabase.Instance.alarmClocks[(!base.gameObject.name.Contains("1")) ? 1 : 0] = mainFsm;
					Func<bool> <>9__1;
					foreach (PlayMakerFSM playMakerFSM4 in base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
					{
						if (playMakerFSM4.name == "TimeSet")
						{
							EventHook.AddWithSync(playMakerFSM4, "Wait player", null, false);
							EventHook.AddWithSync(playMakerFSM4, "Add one", null, false);
							EventHook.Add(playMakerFSM4, "Add many", delegate
							{
								this.IsHoldingAlarm = true;
								return false;
							}, false, false);
							PlayMakerFSM playMakerFSM5 = playMakerFSM4;
							string text = "Wait button";
							Func<bool> func;
							if ((func = <>9__1) == null)
							{
								func = (<>9__1 = delegate
								{
									if (this.IsHoldingAlarm)
									{
										NetLocalPlayer.Instance.WriteFSMFloatMessage(86400, mainFsm.name, (float)mainFsm.Fsm.GetFsmInt("Setting").Value);
										this.IsHoldingAlarm = false;
									}
									return false;
								});
							}
							EventHook.Add(playMakerFSM5, text, func, false, false);
						}
						else if (playMakerFSM4.name == "OffSwitch")
						{
							EventHook.AddWithSync(playMakerFSM4, "Wait player", null, false);
							EventHook.AddWithSync(playMakerFSM4, "Flip", null, false);
							EventHook.AddWithSync(playMakerFSM4, "Bool test", null, false);
						}
					}
				}
				else if (base.name.StartsWith("chargers box") || base.name.StartsWith("manuals box"))
				{
					PlayMakerFSM component2 = base.gameObject.GetComponent<PlayMakerFSM>();
					if (component2 != null)
					{
						EventHook.AddWithSync(component2, "Wait player", null, false);
						EventHook.AddWithSync(component2, "Check open", null, false);
					}
				}
				else if (base.name.StartsWith("plastic trays") || base.name.StartsWith("packaging sheets"))
				{
					PlayMakerFSM component3 = base.gameObject.GetComponent<PlayMakerFSM>();
					if (component3 != null)
					{
						EventHook.AddWithSync(component3, "Wait player", null, false);
						EventHook.AddWithSync(component3, "State 8", null, false);
					}
				}
				else if (base.name == "package(Clone)")
				{
					array = base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
					for (int i = 0; i < array.Length; i++)
					{
						PlayMakerFSM fsm2 = array[i];
						if (fsm2.gameObject == base.gameObject)
						{
							EventHook.AddWithSync(fsm2, "Wait player", null, false);
							EventHook.AddWithSync(fsm2, "State 1", null, false);
						}
						else if (fsm2.name.StartsWith("Trigger"))
						{
							this.triggerFsms.Add(fsm2);
							EventHook.Add(fsm2, "Assemble", delegate
							{
								if (this.receiver)
								{
									this.receiver = false;
								}
								else
								{
									NetLocalPlayer.Instance.WriteFSMBoolMessage(this.syncComponent.ObjectID, fsm2.name, true);
								}
								return false;
							}, false, false);
						}
					}
				}
				else if (base.name == "plastic tray(Clone)")
				{
					array = base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
					for (int i = 0; i < array.Length; i++)
					{
						PlayMakerFSM fsm3 = array[i];
						if (fsm3.name.StartsWith("Trigger"))
						{
							this.triggerFsms.Add(fsm3);
							EventHook.Add(fsm3, "Assemble", delegate
							{
								if (this.receiver)
								{
									this.receiver = false;
								}
								else
								{
									NetLocalPlayer.Instance.WriteFSMBoolMessage(this.syncComponent.ObjectID, fsm3.name, true);
								}
								return false;
							}, false, false);
						}
					}
				}
				else if (base.name == "packages box(Clone)")
				{
					array = base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
					for (int i = 0; i < array.Length; i++)
					{
						PlayMakerFSM fsm4 = array[i];
						if (fsm4.name.StartsWith("Trigger"))
						{
							this.triggerFsms.Add(fsm4);
							EventHook.Add(fsm4, "Assemble", delegate
							{
								if (this.receiver)
								{
									this.receiver = false;
								}
								else
								{
									NetLocalPlayer.Instance.WriteFSMBoolMessage(this.syncComponent.ObjectID, fsm4.name, true);
								}
								return false;
							}, false, false);
						}
					}
				}
				else if (base.name == "Post Package(Clone)")
				{
					PlayMakerFSM component4 = base.gameObject.GetComponent<PlayMakerFSM>();
					if (component4.FsmName == "Use")
					{
						EventHook.AddWithSync(component4, "1", null, false);
					}
				}
				else if (base.name == "gasoline(itemx)" || base.name == "diesel(itemx)")
				{
					array = base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
					for (int i = 0; i < array.Length; i++)
					{
						PlayMakerFSM fsm5 = array[i];
						if (fsm5.gameObject.name == "Open" && fsm5.FsmName == "Use")
						{
							EventHook.AddWithSync(fsm5, "State 2", null, false);
						}
						else if (fsm5.gameObject.name.StartsWith("CapTrigger_FuelJerry"))
						{
							if (base.gameObject.name == "diesel(itemx)")
							{
								this.canFluid = base.transform.GetChild(0).GetComponent<PlayMakerFSM>();
								EventHook.Add(fsm5, "Diesel", delegate
								{
									if (!this.receiver)
									{
										NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), fsm5.Fsm.GetFsmFloat("FuelLevel").Value);
									}
									else
									{
										this.receiver = false;
									}
									return false;
								}, false, false);
								EventHook.Add(fsm5, "State 1", delegate
								{
									if (!this.receiver)
									{
										NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), fsm5.Fsm.GetFsmFloat("FuelLevel").Value);
									}
									else
									{
										this.receiver = false;
									}
									return false;
								}, false, false);
							}
							else
							{
								this.canFluid = base.transform.GetChild(0).GetComponent<PlayMakerFSM>();
								EventHook.Add(fsm5, "Nozzle", delegate
								{
									if (!this.receiver)
									{
										NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), fsm5.Fsm.GetFsmFloat("FuelLevel").Value);
									}
									else
									{
										this.receiver = false;
									}
									return false;
								}, false, false);
								EventHook.Add(fsm5, "State 1", delegate
								{
									if (!this.receiver)
									{
										NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), fsm5.Fsm.GetFsmFloat("FuelLevel").Value);
									}
									else
									{
										this.receiver = false;
									}
									return false;
								}, false, false);
							}
						}
					}
				}
				else
				{
					if (base.name.StartsWith("log"))
					{
						Logger.Debug("Log detected");
						PlayMakerFSM fsm6 = base.gameObject.GetComponent<PlayMakerFSM>();
						if (fsm6 != null)
						{
							EventHook.Add(fsm6, "State 2", delegate
							{
								try
								{
									FixedJoint component5 = fsm6.gameObject.GetComponent<FixedJoint>();
									if (component5 != null)
									{
										Object.Destroy(component5);
									}
								}
								catch (Exception ex)
								{
									Chat.Instance.AddMessage("Error " + ex.Message, MessageSeverity.Info);
								}
								NetLocalPlayer.Instance.WriteVehicleStateMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), "WOODBREAK", false);
								return true;
							}, false, false);
						}
						return;
					}
					if (base.name.StartsWith("Tray"))
					{
						foreach (PlayMakerFSM playMakerFSM6 in base.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
						{
							EventHook.AddWithSync(playMakerFSM6, "Wait", null, false);
							EventHook.AddWithSync(playMakerFSM6, "Play anim", null, false);
							EventHook.AddWithSync(playMakerFSM6, "State 3", null, false);
						}
						return;
					}
				}
			}
			array = base.GetComponents<PlayMakerFSM>();
			for (int i = 0; i < array.Length; i++)
			{
				PlayMakerFSM fsm = array[i];
				if (fsm.gameObject.name.Contains("fireworks bag"))
				{
					return;
				}
				if (base.gameObject.name.StartsWith("shopping") && fsm.Fsm.Name == "Use")
				{
					this.SHOPPINGBAGFSM = fsm;
					if (base.gameObject.name.StartsWith("shopping"))
					{
						EventHook.Add(this.SHOPPINGBAGFSM, "Spawn one", delegate
						{
							MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), false);
							}
							else
							{
								this.receiver = false;
							}
							return false;
						}, false, false);
						EventHook.Add(this.SHOPPINGBAGFSM, "Spawn all", delegate
						{
							MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), true);
							}
							else
							{
								this.receiver = false;
							}
							return false;
						}, false, false);
					}
					this.objectType = Pickupable.SubType.ShoppingBag;
					return;
				}
				if (base.gameObject.name.StartsWith("juice"))
				{
					new Consumable(base.gameObject);
					this.objectType = Pickupable.SubType.Consumable;
				}
				else
				{
					if (base.gameObject.name.StartsWith("fuse(Clone)"))
					{
						EventHook.AddWithSync(base.GetComponent<PlayMakerFSM>(), "Destroy", null, false);
						return;
					}
					if (base.gameObject.name.StartsWith("r20"))
					{
						EventHook.Add(base.GetComponent<PlayMakerFSM>(), "Destroy", delegate
						{
							NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
							return false;
						}, false, false);
						return;
					}
					if (base.gameObject.name == "firewood(Clone)")
					{
						break;
					}
					if (base.gameObject.name == "berry box(Clone)")
					{
						EventHook.AddWithSync(fsm, "Rescale", null, false);
						return;
					}
					if (fsm.gameObject.name.StartsWith("holiday"))
					{
						EventHook.AddWithSync(fsm, "State 1", null, false);
						return;
					}
					if (fsm.Fsm.GetState("Remove bottle") != null)
					{
						this.beerCaseSubType = new BeerCase(base.gameObject);
						this.objectType = Pickupable.SubType.BeerCase;
						return;
					}
					if (fsm.Fsm.GetState("Eat") != null || fsm.Fsm.GetState("Eat 2") != null || fsm.Fsm.GetState("Use") != null)
					{
						new Consumable(base.gameObject);
						this.objectType = Pickupable.SubType.Consumable;
						return;
					}
					if (fsm.FsmName == "Use" && fsm.gameObject.transform.FindChild("_gfx") != null)
					{
						EventHook.Add(fsm, "State 1", delegate
						{
							try
							{
								bool flag = true;
								foreach (ObjectSyncComponent objectSyncComponent in fsm.gameObject.GetComponentsInChildren<ObjectSyncComponent>(true))
								{
									objectSyncComponent.gameObject.SetActive(true);
									objectSyncComponent.transform.SetParent(null);
								}
								foreach (FsmStateAction fsmStateAction in fsm.Fsm.GetState("State 1").Actions)
								{
									if (flag)
									{
										flag = false;
									}
									else
									{
										fsmStateAction.Enabled = false;
									}
								}
								fsm.gameObject.SetActive(false);
							}
							catch (Exception ex2)
							{
								Chat.Instance.AddMessage("Error " + ex2.Message, MessageSeverity.Info);
							}
							NetLocalPlayer.Instance.WriteCarPartMessage(fsm.gameObject.GetComponent<ObjectSyncComponent>(), true);
							return true;
						}, false, false);
						EventHook.Add(fsm, "State 2", delegate
						{
							try
							{
								bool flag2 = true;
								foreach (ObjectSyncComponent objectSyncComponent2 in fsm.gameObject.GetComponentsInChildren<ObjectSyncComponent>(true))
								{
									objectSyncComponent2.gameObject.SetActive(true);
									objectSyncComponent2.transform.SetParent(null);
								}
								foreach (FsmStateAction fsmStateAction2 in fsm.Fsm.GetState("State 2").Actions)
								{
									if (flag2)
									{
										flag2 = false;
									}
									else
									{
										fsmStateAction2.Enabled = false;
									}
								}
								fsm.gameObject.SetActive(false);
							}
							catch (Exception ex3)
							{
								Chat.Instance.AddMessage("Error " + ex3.Message, MessageSeverity.Info);
							}
							NetLocalPlayer.Instance.WriteCarPartMessage(fsm.gameObject.GetComponent<ObjectSyncComponent>(), true);
							return true;
						}, false, false);
						return;
					}
				}
			}
		}

		private bool ShouldHaveRB()
		{
			return base.name.StartsWith("Engine Block") || (!(base.gameObject == null) && !base.gameObject.name.StartsWith("bucket") && (base.gameObject.GetComponents<Renderer>().Length >= 1 || this.objectType != Pickupable.SubType.Consumable) && (this.objectType != Pickupable.SubType.ShoppingBag || !(base.gameObject.transform.FindChild("skinnedmesh") == null)));
		}

		private void SetupShoppingBag()
		{
			PlayMakerFSM[] components = base.gameObject.GetComponents<PlayMakerFSM>();
			for (int i = 0; i < components.Length; i++)
			{
				PlayMakerFSM fsm = components[i];
				if (fsm.FsmName == "Use")
				{
					this.SHOPPINGBAGFSM = fsm;
					if (base.gameObject.name.StartsWith("shopping"))
					{
						EventHook.Add(this.SHOPPINGBAGFSM, "Spawn one", delegate
						{
							MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), false);
							}
							else
							{
								this.receiver = false;
							}
							return false;
						}, false, false);
						EventHook.Add(this.SHOPPINGBAGFSM, "Spawn all", delegate
						{
							MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), true);
							}
							else
							{
								this.receiver = false;
							}
							return false;
						}, false, false);
					}
					else if (base.gameObject.name.StartsWith("spark") || base.gameObject.name.StartsWith("r20") || base.gameObject.name.StartsWith("bulb"))
					{
						EventHook.Add(fsm, "Create Plug", delegate
						{
							fsm.Fsm.GetFsmGameObject("CreateItemsDB").Value = MPController.Instance.createItems;
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), true);
							}
							else
							{
								this.receiver = false;
							}
							return false;
						}, false, false);
					}
					else if (base.gameObject.name.StartsWith("fuse"))
					{
						EventHook.Add(fsm, "Create Fuse", delegate
						{
							fsm.Fsm.GetFsmGameObject("CreateItemsDB").Value = MPController.Instance.createItems;
							if (!this.receiver)
							{
								NetLocalPlayer.Instance.WriteCarPartMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), true);
							}
							else
							{
								this.receiver = false;
							}
							return false;
						}, false, false);
					}
				}
			}
		}

		public void ReadTriggerAssemble(string fsmName)
		{
			foreach (PlayMakerFSM playMakerFSM in this.triggerFsms)
			{
				if (playMakerFSM.name == fsmName)
				{
					this.receiver = true;
					string childName = playMakerFSM.Fsm.GetFsmString("HandChild").Value;
					GameObject gameObject = null;
					float num = 100f;
					IEnumerable<GameObject> enumerable = (Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]).Cast<GameObject>();
					Func<GameObject, bool> func;
					Func<GameObject, bool> <>9__0;
					if ((func = <>9__0) == null)
					{
						func = (<>9__0 = (GameObject go) => go.name == childName);
					}
					foreach (GameObject gameObject2 in enumerable.Where(func))
					{
						if (Vector3.Distance(playMakerFSM.transform.position, gameObject2.transform.position) < num)
						{
							num = Vector3.Distance(playMakerFSM.transform.position, gameObject2.transform.position);
							gameObject = gameObject2;
						}
					}
					if (gameObject == null)
					{
						Logger.Debug("[Pickupable] ERROR: Null part object for " + playMakerFSM.name);
						break;
					}
					playMakerFSM.Fsm.GetFsmGameObject("Part").Value = gameObject;
					playMakerFSM.SendEvent("MP_Assemble");
				}
			}
		}

		public Transform ObjectTransform()
		{
			return base.gameObject.transform;
		}

		public bool PeriodicSyncEnabled()
		{
			return false;
		}

		public bool CanSync()
		{
			bool flag;
			try
			{
				if (this.rigidbody.velocity.sqrMagnitude >= 0.01f)
				{
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		private void RequestSync()
		{
			base.gameObject.GetComponent<ObjectSyncComponent>().RequestObjectSync();
		}

		public bool ShouldTakeOwnership()
		{
			return true;
		}

		public float[] ReturnSyncedVariables(bool sendAllVariables)
		{
			List<float> list = new List<float>();
			if (this.holdingObject)
			{
				list.Add(1f);
			}
			else
			{
				list.Add(0f);
			}
			if (this.objectType == Pickupable.SubType.BeerCase && (this.usedBottlesLast != this.beerCaseSubType.UsedBottles || sendAllVariables))
			{
				this.usedBottlesLast = this.beerCaseSubType.UsedBottles;
				list.Add((float)this.beerCaseSubType.UsedBottles);
			}
			return list.ToArray();
		}

		public void HandleSyncedVariables(float[] variables, CSteamID sender)
		{
			if (this.rigidbody != null)
			{
				if (variables[0] == 1f)
				{
					this.DisableParams();
					NetManager.Instance.GetPlayer(sender).ReadPickupMessage(true, base.gameObject);
				}
				else
				{
					this.EnableParams();
					NetManager.Instance.GetPlayer(sender).ReadPickupMessage(false, null);
				}
				if (variables.Length > 1 && this.objectType == Pickupable.SubType.BeerCase && variables[1] != (float)this.beerCaseSubType.UsedBottles)
				{
					this.beerCaseSubType.RemoveBottles((int)variables[1]);
				}
			}
		}

		public void DisableParams()
		{
			this.rigidbody.useGravity = false;
			this.rigidbody.isKinematic = true;
			if (base.gameObject.layer == 19)
			{
				base.gameObject.layer = 16;
			}
		}

		public void EnableParams()
		{
			try
			{
				if (this.rigidbody != null)
				{
					this.rigidbody.useGravity = true;
					this.rigidbody.isKinematic = false;
					this.rigidbody.detectCollisions = true;
				}
				if (base.gameObject.layer == 16)
				{
					base.gameObject.layer = 19;
				}
			}
			catch (Exception)
			{
				Chat.Instance.AddMessage("Pickupable RB error: " + base.gameObject.name, MessageSeverity.Error);
			}
		}

		public void OwnerSetToRemote()
		{
		}

		public void OwnerRemoved()
		{
			try
			{
				this.rigidbody.useGravity = true;
			}
			catch (Exception)
			{
				Logger.Debug("Avoided a rigid body/OwnerRemoved Pickupable:181 Crash, NO RIGIDBODY PRESENT FOR " + base.gameObject.name);
			}
		}

		public void SyncTakenByForce()
		{
			if (this.holdingObject)
			{
				Logger.Debug("Dropped object because remote player has taken control of it!");
				GamePlayer.Instance.DropStolenObject();
			}
		}

		public void ConstantSyncChanged(bool newValue)
		{
			this.holdingObject = newValue;
			if (!this.holdingObject && base.gameObject.GetComponent<Rigidbody>() != null)
			{
				base.gameObject.GetComponent<Rigidbody>().useGravity = true;
			}
		}

		[CompilerGenerated]
		private bool <.ctor>b__12_0()
		{
			this.IsHoldingAlarm = true;
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__12_10()
		{
			MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
			if (!this.receiver)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), false);
			}
			else
			{
				this.receiver = false;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__12_11()
		{
			MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
			if (!this.receiver)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				this.receiver = false;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <.ctor>b__12_12()
		{
			NetLocalPlayer.Instance.WriteCarPartMessage(base.GetComponent<ObjectSyncComponent>(), true);
			return false;
		}

		[CompilerGenerated]
		private bool <SetupShoppingBag>b__17_0()
		{
			MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
			if (!this.receiver)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), false);
			}
			else
			{
				this.receiver = false;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <SetupShoppingBag>b__17_1()
		{
			MPController.Instance.productList.Fsm.GetFsmGameObject("CurrentBag").Value = base.gameObject;
			if (!this.receiver)
			{
				NetLocalPlayer.Instance.WriteCarPartMessage(base.gameObject.GetComponent<ObjectSyncComponent>(), true);
			}
			else
			{
				this.receiver = false;
			}
			return false;
		}

		private Rigidbody rb;

		private ObjectSyncComponent syncComponent;

		private List<Collider> activeColliders = new List<Collider>();

		public bool isLogBreak;

		public PlayMakerFSM SHOPPINGBAGFSM;

		private bool holdingObject;

		private Pickupable.SubType objectType;

		private BeerCase beerCaseSubType;

		private int usedBottlesLast;

		public PlayMakerFSM canFluid;

		public bool receiver;

		private bool IsHoldingAlarm;

		private List<PlayMakerFSM> triggerFsms = new List<PlayMakerFSM>();

		public enum SubType
		{
			Consumable,
			ShoppingBag,
			BeerCase,
			Item,
			CarPart,
			Tool
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_0
		{
			public <>c__DisplayClass12_0()
			{
			}

			internal bool <.ctor>b__1()
			{
				if (this.<>4__this.IsHoldingAlarm)
				{
					NetLocalPlayer.Instance.WriteFSMFloatMessage(86400, this.mainFsm.name, (float)this.mainFsm.Fsm.GetFsmInt("Setting").Value);
					this.<>4__this.IsHoldingAlarm = false;
				}
				return false;
			}

			public PlayMakerFSM mainFsm;

			public Pickupable <>4__this;

			public Func<bool> <>9__1;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_1
		{
			public <>c__DisplayClass12_1()
			{
			}

			internal bool <.ctor>b__2()
			{
				if (this.<>4__this.receiver)
				{
					this.<>4__this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMBoolMessage(this.<>4__this.syncComponent.ObjectID, this.fsm.name, true);
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public Pickupable <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_2
		{
			public <>c__DisplayClass12_2()
			{
			}

			internal bool <.ctor>b__3()
			{
				if (this.<>4__this.receiver)
				{
					this.<>4__this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMBoolMessage(this.<>4__this.syncComponent.ObjectID, this.fsm.name, true);
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public Pickupable <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_3
		{
			public <>c__DisplayClass12_3()
			{
			}

			internal bool <.ctor>b__4()
			{
				if (this.<>4__this.receiver)
				{
					this.<>4__this.receiver = false;
				}
				else
				{
					NetLocalPlayer.Instance.WriteFSMBoolMessage(this.<>4__this.syncComponent.ObjectID, this.fsm.name, true);
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public Pickupable <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_4
		{
			public <>c__DisplayClass12_4()
			{
			}

			internal bool <.ctor>b__5()
			{
				if (!this.<>4__this.receiver)
				{
					NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), this.fsm.Fsm.GetFsmFloat("FuelLevel").Value);
				}
				else
				{
					this.<>4__this.receiver = false;
				}
				return false;
			}

			internal bool <.ctor>b__6()
			{
				if (!this.<>4__this.receiver)
				{
					NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), this.fsm.Fsm.GetFsmFloat("FuelLevel").Value);
				}
				else
				{
					this.<>4__this.receiver = false;
				}
				return false;
			}

			internal bool <.ctor>b__7()
			{
				if (!this.<>4__this.receiver)
				{
					NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), this.fsm.Fsm.GetFsmFloat("FuelLevel").Value);
				}
				else
				{
					this.<>4__this.receiver = false;
				}
				return false;
			}

			internal bool <.ctor>b__8()
			{
				if (!this.<>4__this.receiver)
				{
					NetLocalPlayer.Instance.WriteCarPartFloatMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), this.fsm.Fsm.GetFsmFloat("FuelLevel").Value);
				}
				else
				{
					this.<>4__this.receiver = false;
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public Pickupable <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_5
		{
			public <>c__DisplayClass12_5()
			{
			}

			internal bool <.ctor>b__9()
			{
				try
				{
					FixedJoint component = this.fsm.gameObject.GetComponent<FixedJoint>();
					if (component != null)
					{
						Object.Destroy(component);
					}
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("Error " + ex.Message, MessageSeverity.Info);
				}
				NetLocalPlayer.Instance.WriteVehicleStateMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), "WOODBREAK", false);
				return true;
			}

			public PlayMakerFSM fsm;

			public Pickupable <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass12_6
		{
			public <>c__DisplayClass12_6()
			{
			}

			internal bool <.ctor>b__13()
			{
				try
				{
					bool flag = true;
					foreach (ObjectSyncComponent objectSyncComponent in this.fsm.gameObject.GetComponentsInChildren<ObjectSyncComponent>(true))
					{
						objectSyncComponent.gameObject.SetActive(true);
						objectSyncComponent.transform.SetParent(null);
					}
					foreach (FsmStateAction fsmStateAction in this.fsm.Fsm.GetState("State 1").Actions)
					{
						if (flag)
						{
							flag = false;
						}
						else
						{
							fsmStateAction.Enabled = false;
						}
					}
					this.fsm.gameObject.SetActive(false);
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("Error " + ex.Message, MessageSeverity.Info);
				}
				NetLocalPlayer.Instance.WriteCarPartMessage(this.fsm.gameObject.GetComponent<ObjectSyncComponent>(), true);
				return true;
			}

			internal bool <.ctor>b__14()
			{
				try
				{
					bool flag = true;
					foreach (ObjectSyncComponent objectSyncComponent in this.fsm.gameObject.GetComponentsInChildren<ObjectSyncComponent>(true))
					{
						objectSyncComponent.gameObject.SetActive(true);
						objectSyncComponent.transform.SetParent(null);
					}
					foreach (FsmStateAction fsmStateAction in this.fsm.Fsm.GetState("State 2").Actions)
					{
						if (flag)
						{
							flag = false;
						}
						else
						{
							fsmStateAction.Enabled = false;
						}
					}
					this.fsm.gameObject.SetActive(false);
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("Error " + ex.Message, MessageSeverity.Info);
				}
				NetLocalPlayer.Instance.WriteCarPartMessage(this.fsm.gameObject.GetComponent<ObjectSyncComponent>(), true);
				return true;
			}

			public PlayMakerFSM fsm;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass17_0
		{
			public <>c__DisplayClass17_0()
			{
			}

			internal bool <SetupShoppingBag>b__2()
			{
				this.fsm.Fsm.GetFsmGameObject("CreateItemsDB").Value = MPController.Instance.createItems;
				if (!this.<>4__this.receiver)
				{
					NetLocalPlayer.Instance.WriteCarPartMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), true);
				}
				else
				{
					this.<>4__this.receiver = false;
				}
				return false;
			}

			internal bool <SetupShoppingBag>b__3()
			{
				this.fsm.Fsm.GetFsmGameObject("CreateItemsDB").Value = MPController.Instance.createItems;
				if (!this.<>4__this.receiver)
				{
					NetLocalPlayer.Instance.WriteCarPartMessage(this.<>4__this.gameObject.GetComponent<ObjectSyncComponent>(), true);
				}
				else
				{
					this.<>4__this.receiver = false;
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public Pickupable <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass19_0
		{
			public <>c__DisplayClass19_0()
			{
			}

			internal bool <ReadTriggerAssemble>b__0(GameObject go)
			{
				return go.name == this.childName;
			}

			public string childName;

			public Func<GameObject, bool> <>9__0;
		}
	}
}
