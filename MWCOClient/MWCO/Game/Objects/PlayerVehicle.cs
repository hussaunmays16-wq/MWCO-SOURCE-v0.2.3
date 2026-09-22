using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Components;
using MWCO.Game.Places;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	public class PlayerVehicle : ISyncedObject
	{
		public float Steering
		{
			get
			{
				float num;
				try
				{
					num = this.axisCarController.steering;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR IN STEERING " + ex.Message, MessageSeverity.Error);
					num = 0f;
				}
				return num;
			}
			set
			{
				try
				{
					this.mpCarController.remoteSteerInput = value;
					float num = Mathf.Lerp(-450f, 450f, (value + 1f) / 2f);
					if (this.ParentGameObject.name == "JONNEZ ES(Clone)")
					{
						num = Mathf.Lerp(-45f, 45f, (value + 1f) / 2f);
					}
					if (this.ParentGameObject.name.Contains("GIFU") || this.ParentGameObject.name.Contains("Rico"))
					{
						this.steeringPivot.localRotation = Quaternion.Euler(0f, num, 0f);
					}
					else if (this.ParentGameObject.name.Contains("SAKER"))
					{
						this.steeringPivot.localRotation = Quaternion.Euler(344f, 180f, num + 90f);
					}
					else
					{
						this.steeringPivot.localRotation = Quaternion.Euler(0f, 0f, num);
					}
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR SETTING STEERING " + ex.Message, MessageSeverity.Error);
				}
			}
		}

		public float Throttle
		{
			get
			{
				float num;
				try
				{
					num = this.axisCarController.throttleInput;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR IN THROTTLE " + ex.Message, MessageSeverity.Error);
					num = 0f;
				}
				return num;
			}
			set
			{
				try
				{
					this.mpCarController.remoteThrottleInput = value;
					this.axisCarController.throttleInput = value;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR SETTING THROTTLE " + ex.Message, MessageSeverity.Error);
				}
			}
		}

		public float Brake
		{
			get
			{
				float num;
				try
				{
					num = this.axisCarController.brakeInput;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR IN BRAKE " + ex.Message, MessageSeverity.Error);
					num = 0f;
				}
				return num;
			}
			set
			{
				try
				{
					this.mpCarController.remoteBrakeInput = value;
					this.axisCarController.brakeInput = value;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR SETTING BRAKE " + ex.Message, MessageSeverity.Error);
				}
			}
		}

		public float ClutchInput
		{
			get
			{
				float num;
				try
				{
					num = this.driveTrain.clutch.GetClutchPosition();
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR IN CLUTCHINPUT " + ex.Message, MessageSeverity.Error);
					num = 0f;
				}
				return num;
			}
			set
			{
				try
				{
					this.driveTrain.clutch.SetClutchPosition(value);
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR SETTING CLUTCHINPUT " + ex.Message, MessageSeverity.Error);
				}
			}
		}

		public int Gear
		{
			get
			{
				int num;
				try
				{
					num = this.driveTrain.gear;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR IN GEAR " + ex.Message, MessageSeverity.Error);
					num = 0;
				}
				return num;
			}
			set
			{
				try
				{
					this.mpCarController.remoteTargetGear = value;
				}
				catch (Exception ex)
				{
					Chat.Instance.AddMessage("ERROR SETTING GEAR " + ex.Message, MessageSeverity.Error);
				}
			}
		}

		public float Fuel
		{
			get
			{
				float num;
				try
				{
					num = this.fuelTankFsm.Fsm.GetFsmFloat("FuelLevel").Value;
				}
				catch (Exception)
				{
					num = 40f;
				}
				return num;
			}
			set
			{
				try
				{
					this.fuelTankFsm.Fsm.GetFsmFloat("FuelLevel").Value = value;
				}
				catch (Exception)
				{
				}
			}
		}

		public PlayerVehicle(GameObject go, ObjectSyncComponent osc)
		{
			this.gameObject = go;
			this.syncComponent = osc;
			if (go.GetTopmostParent().name == "JOBS")
			{
				this.wasDisabled = !go.activeSelf;
				go.SetActive(true);
			}
			this.ParentGameObject = go;
			string name = this.ParentGameObject.name;
			Object.Destroy(this.ParentGameObject.transform.GetComponent<LODGroup>());
			this.rigidbody = this.ParentGameObject.GetComponent<Rigidbody>();
			if (go.name != "FLATBED")
			{
				this.dynamics = this.ParentGameObject.GetComponent<CarDynamics>();
				this.driveTrain = this.ParentGameObject.GetComponent<Drivetrain>();
				this.axisCarController = this.ParentGameObject.GetComponent<AxisCarController>();
				this.mpCarController = this.ParentGameObject.AddComponent<PlayerVehicle.MPCarController>();
				this.mpCarController.osc = this.syncComponent;
				this.SetupPivots();
			}
			this.FindFSMs();
			if (this.wasDisabled)
			{
				this.ParentGameObject.SetActive(false);
			}
			if (go.name == "CORRIS")
			{
				this.LoadWiring();
			}
		}

		public void SetupPivots()
		{
			GameObject gameObject = null;
			this.steeringPivot = this.ParentGameObject.GetComponentsInChildren<SteeringWheel>(true)[0].transform;
			foreach (PlayMakerFSM playMakerFSM in this.ParentGameObject.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.name == "Pivot")
				{
					if (playMakerFSM.FsmName == "Movemement" || playMakerFSM.FsmName == "Pos")
					{
						gameObject = playMakerFSM.gameObject;
					}
				}
				else if (playMakerFSM.FsmName == "Movemement" && playMakerFSM.transform.parent.parent.parent.name.StartsWith("gear stick"))
				{
					gameObject = playMakerFSM.gameObject;
				}
				else if (playMakerFSM.name == "Lever" && (playMakerFSM.FsmName == "Move" || playMakerFSM.FsmName == "Pos"))
				{
					gameObject = playMakerFSM.gameObject;
				}
			}
			GameObject gameObject2 = GameObject.CreatePrimitive(0);
			gameObject2.transform.SetParent(this.steeringPivot.transform);
			gameObject2.transform.position = this.steeringPivot.transform.position;
			gameObject2.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
			gameObject2.name = "steerIK" + this.ParentGameObject.name;
			if (this.ParentGameObject.name == "SORBET(190-200psi)")
			{
				gameObject2.transform.localPosition = new Vector3(-0.08f, -0.2f, 0.03f);
			}
			else if (this.ParentGameObject.name == "CORRIS")
			{
				gameObject2.transform.localPosition = new Vector3(0.21f, 0.03f, 0.81f);
			}
			else if (this.ParentGameObject.name == "MACHTWAGEN")
			{
				gameObject2.transform.localPosition = new Vector3(-0.04f, -0.21f, 0.03f);
			}
			else if (this.ParentGameObject.name == "BACHGLOTZ(1905kg)")
			{
				gameObject2.transform.localPosition = new Vector3(-0.03f, -0.17f, 0.14f);
			}
			else if (this.ParentGameObject.name == "GIFU(750/450psi)")
			{
				gameObject2.transform.localPosition = new Vector3(-0.22f, 0.05f, 0f);
			}
			else if (this.ParentGameObject.name == "KEKMET(350-400psi)")
			{
				gameObject2.transform.localPosition = new Vector3(-0.2f, 0.1f, 0.02f);
			}
			else if (this.ParentGameObject.name.StartsWith("JONNEZ ES"))
			{
				gameObject2.transform.localPosition = new Vector3(-0.25f, -0.03f, 0.37f);
			}
			else if (this.ParentGameObject.name == "Panier250")
			{
				gameObject2.transform.localPosition = new Vector3(0.18f, 0.06f, 0.07f);
			}
			else if (this.ParentGameObject.name == "RicochetST(1010kg)")
			{
				gameObject2.transform.localPosition = new Vector3(0.21f, -0.03f, 0f);
			}
			else if (this.ParentGameObject.name == "SAKER(1350kg)")
			{
				gameObject2.transform.localPosition = new Vector3(0.18f, 0.06f, 0.07f);
			}
			gameObject2.transform.GetComponent<SphereCollider>().isTrigger = true;
			gameObject2.GetComponent<MeshRenderer>().enabled = false;
			this.steeringIK = gameObject2.transform;
			if (gameObject != null && this.ParentGameObject.name != "BACHGLOTZ(1905kg)")
			{
				GameObject gameObject3 = GameObject.CreatePrimitive(0);
				gameObject3.transform.SetParent(gameObject.transform);
				gameObject3.transform.position = gameObject.transform.position;
				gameObject3.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
				gameObject3.name = "gearstickIK" + this.ParentGameObject.name;
				if (this.ParentGameObject.name == "SORBET(190-200psi)")
				{
					gameObject3.transform.localPosition = new Vector3(0.01f, -0.06f, 0.3f);
				}
				else if (this.ParentGameObject.name == "CORRIS")
				{
					gameObject3.transform.localPosition = new Vector3(0f, -0.16f, 0.37f);
				}
				else if (this.ParentGameObject.name == "MACHTWAGEN")
				{
					gameObject3.transform.localPosition = new Vector3(0.01f, -0.05f, 0.19f);
				}
				else if (this.ParentGameObject.name == "GIFU(750/450psi)")
				{
					gameObject3.transform.localPosition = new Vector3(-0.01f, -0.06f, 0.3f);
				}
				else if (this.ParentGameObject.name == "KEKMET(350-400psi)")
				{
					gameObject3.transform.localPosition = new Vector3(0f, -0.04f, 0.54f);
				}
				gameObject3.transform.GetComponent<SphereCollider>().isTrigger = true;
				gameObject3.GetComponent<MeshRenderer>().enabled = false;
				this.gearstickIK = gameObject3.transform;
				return;
			}
			if (this.ParentGameObject.name.StartsWith("JONNEZ ES"))
			{
				GameObject gameObject4 = GameObject.CreatePrimitive(0);
				gameObject4.transform.SetParent(this.steeringPivot.transform);
				gameObject4.transform.position = this.steeringPivot.transform.position;
				gameObject4.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
				gameObject4.transform.localPosition = new Vector3(0.22f, -0.01f, 0.41f);
				gameObject4.transform.GetComponent<SphereCollider>().isTrigger = true;
				gameObject4.GetComponent<MeshRenderer>().enabled = false;
				this.gearstickIK = gameObject4.transform;
			}
		}

		private void AddPassengerSeat(PlayMakerFSM fsm = null)
		{
			GameObject gameObject = GameObject.CreatePrimitive(3);
			this.PassengerSeatTransform = gameObject.transform;
			gameObject.transform.parent = (fsm ? fsm.gameObject.transform.parent : this.SeatTransform.parent);
			gameObject.transform.position = gameObject.transform.parent.position;
			if (fsm)
			{
				if (fsm.gameObject.transform.parent.transform.parent.transform.name == "JONNEZ ES(Clone)")
				{
					gameObject.transform.localScale = new Vector3(0.04f, 0.1f, 0.04f);
				}
				else
				{
					gameObject.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
				}
			}
			else
			{
				gameObject.transform.localScale = new Vector3(0.25f, 0.2f, 0.25f);
			}
			gameObject.transform.GetComponent<BoxCollider>().isTrigger = true;
			PassengerSeat passengerSeat = gameObject.AddComponent(typeof(PassengerSeat)) as PassengerSeat;
			passengerSeat.VehicleType = this.ParentGameObject.name;
			passengerSeat.DriversSeat = (fsm ? fsm.gameObject : this.SeatTransform.gameObject);
			passengerSeat.syncComponent = this.syncComponent;
			passengerSeat.rear = false;
		}

		private void AddRearSeat(PlayMakerFSM fsm = null)
		{
			GameObject gameObject = GameObject.CreatePrimitive(3);
			this.RearSeatTransform = gameObject.transform;
			gameObject.transform.parent = (fsm ? fsm.gameObject.transform.parent : this.SeatTransform.parent);
			gameObject.transform.position = gameObject.transform.parent.position;
			if (!fsm)
			{
				if (this.ParentGameObject.name == "RicochetST(1010kg)")
				{
					gameObject.transform.localEulerAngles = Vector3.zero;
					gameObject.transform.localScale = new Vector3(1f, 0.25f, 0.25f);
				}
				else if (this.ParentGameObject.name == "Panier250")
				{
					gameObject.transform.localEulerAngles = Vector3.zero;
					gameObject.transform.localScale = new Vector3(1.15f, 0.25f, 0.25f);
				}
				else if (this.ParentGameObject.name == "SAKER(1350kg)")
				{
					gameObject.transform.localEulerAngles = Vector3.zero;
					gameObject.transform.localScale = new Vector3(1.15f, 0.25f, 0.25f);
				}
			}
			gameObject.transform.GetComponent<BoxCollider>().isTrigger = true;
			PassengerSeat passengerSeat = gameObject.AddComponent(typeof(PassengerSeat)) as PassengerSeat;
			passengerSeat.VehicleType = this.ParentGameObject.name;
			passengerSeat.DriversSeat = (fsm ? fsm.gameObject : this.SeatTransform.gameObject);
			passengerSeat.syncComponent = this.syncComponent;
			passengerSeat.rear = true;
		}

		private void AddBootBox(PlayMakerFSM fsm = null)
		{
			GameObject gameObject = GameObject.CreatePrimitive(3);
			gameObject.transform.SetParent(fsm.gameObject.transform.parent);
			if (this.ParentGameObject.name != "SORBET(190-200psi)" && this.ParentGameObject.name != "BACHGLOTZ(1905kg)" && this.ParentGameObject.name != "MACHTWAGEN" && this.ParentGameObject.name != "KEKMET(350-400psi)" && this.ParentGameObject.name != "CORRIS" && this.ParentGameObject.name != "GIFU(750/450psi)")
			{
				return;
			}
			gameObject.transform.GetComponent<BoxCollider>().isTrigger = true;
			this.bootBox = gameObject.AddComponent<BootBox>();
			this.bootBox.VehicleType = this.ParentGameObject.name;
			this.bootBox.DriversSeat = fsm.gameObject;
			this.bootBox.syncComponent = this.syncComponent;
			this.bootBox.Initialize();
		}

		public void SetRemoteSteering(bool enabled)
		{
			try
			{
				this.axisCarController.enabled = !enabled;
				this.mpCarController.enabled = enabled;
				this.dynamics.enabled = false;
				this.steeringPivot.GetComponent<SteeringWheel>().enabled = !enabled;
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("PlayerVehicle.cs 360 Crash avoided why? {0}", ex));
				Chat.Instance.AddMessage(string.Format("PlayerVehicle.cs 360 Crash avoided. May become buggy! {0} ", enabled), MessageSeverity.Error);
			}
		}

		public Transform ObjectTransform()
		{
			return this.ParentGameObject.transform;
		}

		public bool PeriodicSyncEnabled()
		{
			return true;
		}

		public bool CanSync()
		{
			if (this.DriverIsLocal)
			{
				this.isSyncing = true;
				return true;
			}
			if (this.rigidbody.velocity.sqrMagnitude >= 0.01f)
			{
				this.isSyncing = true;
				return true;
			}
			this.isSyncing = false;
			return false;
		}

		public bool ShouldTakeOwnership()
		{
			return true;
		}

		public void ResetRemoteInputs()
		{
			// v0.3.2 backport: when the driver exits, observers must not keep stale throttle/brake
			// (engine revving with nobody in the driver's seat).
			try
			{
				this.Throttle = 0f;
				this.Brake = 0f;
			}
			catch (Exception ex)
			{
				Logger.Debug("ResetRemoteInputs failed: " + ex);
			}
		}

		public float[] GetWheelRpms()
		{
			try
			{
				if (!this.wheelCollidersSearched)
				{
					this.wheelCollidersSearched = true;
					if (this.ParentGameObject != null)
					{
						this.wheelCollidersCache = this.ParentGameObject.GetComponentsInChildren<WheelCollider>(true);
					}
				}
				if (this.wheelCollidersCache == null || this.wheelCollidersCache.Length == 0)
				{
					return null;
				}
				float[] array = new float[this.wheelCollidersCache.Length];
				for (int i = 0; i < this.wheelCollidersCache.Length; i++)
				{
					array[i] = ((this.wheelCollidersCache[i] != null) ? this.wheelCollidersCache[i].rpm : 0f);
				}
				return array;
			}
			catch (Exception ex)
			{
				Logger.Debug("GetWheelRpms failed: " + ex);
				return null;
			}
		}

		public float[] ReturnSyncedVariables(bool sendAllVariables)
		{
			if (this.isSyncing)
			{
				return new float[]
				{
					this.Steering,
					this.Throttle,
					this.Brake,
					this.ClutchInput,
					(float)this.Gear,
					this.Fuel
				};
			}
			return null;
		}

		public void HandleSyncedVariables(float[] variables, CSteamID sender)
		{
			try
			{
				if (variables != null)
				{
					this.Steering = variables[0];
					this.Throttle = variables[1];
					this.Brake = variables[2];
					this.ClutchInput = variables[3];
					this.Gear = (int)variables[4];
					this.Fuel = variables[5];
					if (this.Throttle > 0.5f)
					{
						NetManager.Instance.GetPlayer(sender).FootDown();
					}
					else
					{
						NetManager.Instance.GetPlayer(sender).FootUp();
					}
					if (this.axisCarController.enabled)
					{
						this.SetRemoteSteering(true);
					}
				}
			}
			catch (Exception)
			{
				Chat.Instance.AddMessage("Can Not Sync Car Variables! Crash Avoided But It May Become Buggy", MessageSeverity.Error);
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
				if (this.ParentGameObject.name == "FLATBED")
				{
					Logger.Debug("FLATBED FSM " + fsm.FsmName + " IN OBJECT " + fsm.gameObject.name);
					if (fsm.gameObject.name == "Bed")
					{
						this.LogTriggerFSM = fsm.transform.GetChild(3).GetComponent<PlayMakerFSM>();
						EventHook.Add(this.LogTriggerFSM, "Destroy Wood", delegate
						{
							if (this.isLogRelated)
							{
								this.isLogRelated = false;
							}
							else
							{
								NetLocalPlayer.Instance.WriteVehicleStateMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), "DESTROYWOOD", false);
							}
							return false;
						}, false, false);
						EventHook.Add(this.LogTriggerFSM, "Destroy Log", delegate
						{
							if (this.isLogRelated)
							{
								this.isLogRelated = false;
							}
							else
							{
								NetLocalPlayer.Instance.WriteVehicleStateMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), "DESTROYLOG", false);
							}
							return false;
						}, false, false);
					}
				}
				if (fsm.FsmName == "Death" && fsm.gameObject.name == "DriverHeadPivot")
				{
					this.driverHeadTrigger = fsm;
					this.driverHeadTrigger.enabled = false;
					this.driverHeadTrigger.GetComponent<Rigidbody>().isKinematic = true;
					Object.Destroy(this.driverHeadTrigger.GetComponent<ConfigurableJoint>());
				}
				else if (fsm.FsmName == "HeadForce")
				{
					this.headForceFSM = fsm;
					FsmState[] states = fsm.Fsm.States;
					for (int j = 0; j < states.Length; j++)
					{
						FsmStateAction[] actions = states[j].Actions;
						for (int k = 0; k < actions.Length; k++)
						{
							actions[k].Enabled = false;
						}
					}
				}
				else if (fsm.FsmName == "PlayerTrigger")
				{
					if (fsm.gameObject.name.StartsWith("DriveTrigger"))
					{
						this.playerTrigger = fsm;
						this.PlayerEventHooks(fsm, null);
						int num = 0;
						PlayMakerFSM[] fsms = DevTools.fsms;
						for (int j = 0; j < fsms.Length; j++)
						{
							if (fsms[j] == null)
							{
								DevTools.fsms[num] = fsm;
								break;
							}
							num++;
						}
					}
				}
				else if (fsm.FsmName == "Logic" && fsm.gameObject.name == "LogTrigger")
				{
					EventHook.SyncAllEvents(fsm, null);
				}
				else if (fsm.FsmName == "LOD" && fsm.gameObject == this.ParentGameObject)
				{
					fsm.gameObject.AddComponent<LODManager>().Initialize(fsm, 50f, false, "LOD", "Object");
				}
				else
				{
					if (fsm.FsmName == "Use" && fsm.name == "Remove")
					{
						Transform parent = fsm.transform.parent;
						if (((parent != null) ? parent.name : null) == "Trailer")
						{
							this.trailerRemoveFSM = fsm;
							EventHook.AddWithSync(this.trailerRemoveFSM, "Wait player", null, false);
							EventHook.AddWithSync(this.trailerRemoveFSM, "Close door", null, false);
							goto IL_1F11;
						}
					}
					if (fsm.FsmName == "Trigger" && fsm.name.StartsWith("CapTrigger_Fuel"))
					{
						this.fuelCapTriggerFSM = fsm;
						EventHook.Add(fsm, "Nozzle", delegate
						{
							Peraportti.Instance.activeVehicle = this;
							return false;
						}, false, false);
					}
					else if (fsm.FsmName == "Distance" && fsm.name == "Hook")
					{
						EventHook.AddWithSync(fsm, "Attach trailer", delegate
						{
							try
							{
								this.trailerRemoveFSM.gameObject.SetActive(true);
							}
							catch
							{
							}
							return false;
						}, false);
					}
					else
					{
						if (fsm.FsmName == "Use" && fsm.name == "Trigger")
						{
							Transform parent2 = fsm.transform.parent;
							if (((parent2 != null) ? parent2.name : null) == "body_fuel_hatch")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Check position", null, false);
								goto IL_1F11;
							}
						}
						if (fsm.name == "FuelUsage")
						{
							fsm.gameObject.SetActive(false);
							fsm.enabled = false;
						}
						else
						{
							if (fsm.FsmName == "Use" && fsm.name == "flatbed_hatch")
							{
								EventHook.AddWithSync(fsm, "Check position", null, false);
								return;
							}
							if (fsm.name == "ButtonHazard")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Flip+sound", null, false);
								EventHook.AddWithSync(fsm, "On/off", null, false);
							}
							else if (fsm.name == "ButtonWindowHeater" || fsm.name == "ButtonDome" || fsm.name == "ParkBrk")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Flip+sound", null, false);
								EventHook.AddWithSync(fsm, "On/off", null, false);
							}
							else if (fsm.name == "ButtonHeaterTemp")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Increase", null, false);
								EventHook.AddWithSync(fsm, "Decrease", null, false);
							}
							else if (fsm.name == "ButtonHeaterDirection")
							{
								if (this.ParentGameObject.name == "MACHTWAGEN")
								{
									EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
								}
								else
								{
									EventHook.AddWithSync(fsm, "Wait player", null, false);
								}
								EventHook.AddWithSync(fsm, "Increase", null, false);
								EventHook.AddWithSync(fsm, "Decrease", null, false);
							}
							else if (fsm.name == "ButtonHeaterBlower")
							{
								if (this.ParentGameObject.name == "MACHTWAGEN")
								{
									EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
									EventHook.AddWithSync(fsm, "Increase", null, false);
									EventHook.AddWithSync(fsm, "Decrease", null, false);
								}
								else
								{
									EventHook.AddWithSync(fsm, "Wait player", null, false);
									EventHook.AddWithSync(fsm, "State 1", null, false);
								}
							}
							else if (fsm.name == "ButtonWipers")
							{
								EventHook.AddWithSync(fsm, "Wait player", null, false);
								EventHook.AddWithSync(fsm, "Test", null, false);
								EventHook.AddWithSync(fsm, "Wait player 2", null, false);
								EventHook.AddWithSync(fsm, "Test 2", null, false);
							}
							else if (fsm.FsmName == "Scrape")
							{
								EventHook.AddWithSync(fsm, "Scrape 2", null, false);
							}
							else if (fsm.name == "ButtonChoke")
							{
								EventHook.SyncAllEvents(fsm, null);
								if (fsm != null && !this.HasBeenSynced(fsm))
								{
									EventHook.Add(fsm, "Off", delegate
									{
										NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Choke").Value, this.chokeFsm.Fsm.GetFsmFloat("Pos").Value));
										return false;
									}, true, false);
									EventHook.Add(fsm, "On", delegate
									{
										NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Choke").Value, this.chokeFsm.Fsm.GetFsmFloat("Pos").Value));
										return false;
									}, true, false);
								}
							}
							else
							{
								Transform parent3 = fsm.transform.parent;
								string text;
								if (parent3 == null)
								{
									text = null;
								}
								else
								{
									Transform parent4 = parent3.parent;
									text = ((parent4 != null) ? parent4.name : null);
								}
								if (text == "FuelFiller")
								{
									EventHook.AddWithSync(fsm, "Wait player", null, false);
									EventHook.AddWithSync(fsm, "Check position", null, false);
								}
								else if (fsm.name == "opener" && fsm.FsmName == "Knob")
								{
									EventHook.AddWithSync(fsm, "Get scroll", null, false);
									EventHook.AddWithSync(fsm, "State 1", null, false);
									EventHook.AddWithSync(fsm, "State 2", null, false);
								}
								else
								{
									Transform parent5 = fsm.transform.parent;
									if (((parent5 != null) ? parent5.name : null) == "InteriorLight" && fsm.name == "Use")
									{
										EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
										EventHook.AddWithSync(fsm, "Flip 2", null, false);
										EventHook.AddWithSync(fsm, "Light", null, false);
									}
									else if (fsm.name == "ButtonLightModes" && fsm.FsmName == "Use")
									{
										EventHook.AddWithSync(fsm, "Send event", null, false);
										EventHook.AddWithSync(fsm, "Mode add", null, false);
										EventHook.AddWithSync(fsm, "Test", null, false);
										EventHook.AddWithSync(fsm, "Test 2", null, false);
										EventHook.AddWithSync(fsm, "Off 2", null, false);
										EventHook.AddWithSync(fsm, "Park", null, false);
										EventHook.AddWithSync(fsm, "Drive", null, false);
									}
									else if (fsm.name == "RearHitch")
									{
										EventHook.AddWithSync(fsm, "Wait player 2", null, false);
										EventHook.AddWithSync(fsm, "State 2", null, false);
										EventHook.AddWithSync(fsm, "State 3", null, false);
										EventHook.AddWithSync(fsm, "State 4", null, false);
										EventHook.AddWithSync(fsm, "Set position 2", null, false);
									}
									else if (fsm.name == "PTO" && fsm.FsmName == "Use")
									{
										EventHook.AddWithSync(fsm, "Wait player", null, false);
										EventHook.AddWithSync(fsm, "Flip", null, false);
										EventHook.AddWithSync(fsm, "Flip 2", null, false);
									}
									else if (fsm.name == "Pivot")
									{
										Transform parent6 = fsm.transform.parent;
										if (((parent6 != null) ? parent6.name : null) == "GearLever" && fsm.FsmName == "Pos")
										{
											EventHook.AddWithSync(fsm, "Wait player", null, false);
											EventHook.AddWithSync(fsm, "Shift", null, false);
											EventHook.AddWithSync(fsm, "Park", null, false);
											EventHook.AddWithSync(fsm, "Drive", null, false);
											EventHook.AddWithSync(fsm, "Reverse", null, false);
											EventHook.AddWithSync(fsm, "Neutral", null, false);
											EventHook.AddWithSync(fsm, "1", null, false);
											EventHook.AddWithSync(fsm, "2", null, false);
										}
									}
									else if (fsm.name == "Colliders" && this.ParentGameObject.name == "BACHGLOTZ(1905kg)")
									{
										fsm.transform.GetChild(19).gameObject.SetActive(false);
									}
									else if (fsm.name == "ButtonMode")
									{
										EventHook.AddWithSync(fsm, "Wait player", null, false);
										EventHook.AddWithSync(fsm, "State 1", null, false);
										EventHook.AddWithSync(fsm, "Flip Light", null, false);
										EventHook.AddWithSync(fsm, "Light On?", null, false);
									}
									else if (fsm.name == "KnobMode")
									{
										EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
										EventHook.AddWithSync(fsm, "Volume dec", null, false);
										EventHook.AddWithSync(fsm, "Volume inc", null, false);
										EventHook.AddWithSync(fsm, "State 1", null, false);
									}
									else if (fsm.name == "SunflapLeft")
									{
										this.sunflapFsm = fsm;
										EventHook.SyncAllEvents(fsm, null);
										EventHook.Add(fsm, "Wait player", delegate
										{
											NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Sunflap, false, fsm.Fsm.GetFsmFloat("Angle").Value, "");
											return false;
										}, false, false);
									}
									else if (fsm.name == "TrackChannelSwitch")
									{
										EventHook.AddWithSync(fsm, "Wait player", null, false);
										EventHook.AddWithSync(fsm, "State 1", null, false);
									}
									else if (fsm.name == "CDplrVolume" && fsm.FsmName == "Knob")
									{
										EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
										EventHook.AddWithSync(fsm, "Volume dec", null, false);
										EventHook.AddWithSync(fsm, "Volume inc", null, false);
										EventHook.AddWithSync(fsm, "Set volume", null, false);
									}
									else if (fsm.name == "Payment")
									{
										EventHook.AddWithSync(fsm, "Wait player", null, false);
										EventHook.AddWithSync(fsm, "Make payment", null, false);
										EventHook.AddWithSync(fsm, "Wait player 2", null, false);
										EventHook.AddWithSync(fsm, "State 1", null, false);
										EventHook.AddWithSync(fsm, "State 6", null, false);
										EventHook.AddWithSync(fsm, "State 3", null, false);
										EventHook.AddWithSync(fsm, "Wait player 3", null, false);
										EventHook.AddWithSync(fsm, "Drop ticket", null, false);
										EventHook.AddWithSync(fsm, "Check duty", null, false);
									}
									else if (fsm.name == "Function" && fsm.FsmName == "Latch")
									{
										EventHook.AddWithSync(fsm, "State 1", null, false);
									}
									else if (fsm.FsmName == "Starter" && fsm.gameObject.GetTopmostParent().name == "GIFU(750/450psi)")
									{
										this.gifuStall = fsm;
									}
									else if (fsm.FsmName == "Starter")
									{
										this.starterFsm = fsm;
									}
									else if (fsm.gameObject.name == "Kickstart" && fsm.FsmName == "Use")
									{
										this.kickstarterFsm = fsm;
									}
									else if (fsm.gameObject.name == "Windshield")
									{
										EventHook.AddWithSync(fsm, "Break glass", null, false);
									}
									else if ((fsm.FsmName == "Use" && (fsm.gameObject.name == "Ignition" || fsm.gameObject.name.StartsWith("IGNITIONx"))) || (fsm.FsmName == "UseNew" && fsm.gameObject.name == "IgnitionSatsuma"))
									{
										this.ignitionFsm = fsm;
									}
									else if (fsm.FsmName == "Use" && fsm.gameObject.name == "ButtonHorn")
									{
										this.horn = fsm;
										Logger.Debug("Found horn for " + this.horn.transform.parent.transform.parent.name);
									}
									else if (fsm.gameObject.name == "ParkingBrake" && fsm.FsmName == "Use" && this.ParentGameObject.name == "KEKMET(350-400psi)")
									{
										this.handbrakeFsm = fsm;
										this.hasPushParkingBrake = false;
									}
									else if (((fsm.gameObject.name == "ParkingBrake" || fsm.gameObject.name == "LeverPivot") && fsm.FsmName == "Use") || (fsm.gameObject.name == "handbrake lever" && fsm.FsmName == "Use"))
									{
										this.handbrakeFsm = fsm;
										this.hasPushParkingBrake = true;
									}
									else if (fsm.name == "Parking Brake" && fsm.FsmName == "Use")
									{
										this.handbrakeFsm = fsm;
										this.hasLeverParkingBrake = true;
									}
									else if (fsm.name == "Range" && fsm.FsmName == "Use")
									{
										this.rangeFsm = fsm;
										this.hasRange = true;
									}
									else if (fsm.name.StartsWith("FuelTank") && fsm.FsmName == "Data")
									{
										this.fuelTankFsm = fsm;
										Logger.Debug("Got Fuel Tank FSM For " + fsm.name);
									}
									else if (fsm.gameObject.name == "FuelTap" && fsm.FsmName == "Use")
									{
										this.fuelTapFsm = fsm;
									}
									else if (fsm.gameObject.name == "OpenCap" && fsm.FsmName == "Screw")
									{
										this.fuelCapFsm = fsm;
									}
									else if (fsm.gameObject.name == "Trigger" && fsm.FsmName == "Use " && fsm.gameObject.transform.parent.name == "body_fuel_hatch")
									{
										this.fuelHatchFsm = fsm;
									}
									else if ((fsm.gameObject.name == "Lights" && fsm.FsmName == "Use") || (fsm.gameObject.name == "knob" && fsm.FsmName == "Use"))
									{
										this.lightsFsm = fsm;
									}
									else if ((fsm.gameObject.name == "Wipers" && fsm.FsmName == "Use") || (fsm.gameObject.name == "ButtonWipers" && fsm.FsmName == "Use"))
									{
										this.wipersFsm = fsm;
									}
									else if (fsm.gameObject.name == "RadioTuner" && fsm.FsmName == "Knob")
									{
										this.radioFsm = fsm;
									}
									else if (fsm.gameObject.name == "RadioVolume" && fsm.FsmName == "Knob")
									{
										this.radioVFsm = fsm;
									}
									else if (fsm.gameObject.name == "ButtonInteriorLight" && fsm.FsmName == "Use")
									{
										this.interiorLightFsm = fsm;
									}
									else if (fsm.gameObject.name == "Use" && fsm.FsmName == "Use" && fsm.Fsm.GetState("Flip 2") != null)
									{
										this.interiorLightFsm = fsm;
									}
									else if (fsm.FsmName == "GearIndicator")
									{
										this.gearIndicatorFsm = fsm;
									}
									else if (fsm.gameObject.name == "FrontHydArm" && fsm.FsmName == "Use")
									{
										this.frontHydraulicFsm = fsm;
										this.isTractor = true;
									}
									else if (fsm.gameObject.name == "FrontHydLoader" && fsm.FsmName == "Use")
									{
										this.frontHydraulicFsm2 = fsm;
										this.isTractor = true;
									}
									else if (fsm.gameObject.name == "Throttle" && fsm.gameObject.transform.parent.parent.parent.gameObject.name.Contains("KEKMET"))
									{
										this.isTractor = true;
										if (fsm.FsmName == "Use")
										{
											this.chokeFsm = fsm;
											EventHook.Add(fsm, "Wait player", delegate
											{
												if (this.isHolding)
												{
													this.isHolding = false;
													NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												}
												return false;
											}, false, false);
											EventHook.Add(fsm, "Wait button", delegate
											{
												if (this.isHolding)
												{
													this.isHolding = false;
													NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												}
												return false;
											}, false, false);
											EventHook.Add(fsm, "INCREASE", delegate
											{
												this.isHolding = true;
												NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												return false;
											}, false, false);
											EventHook.Add(fsm, "DECREASE", delegate
											{
												this.isHolding = true;
												NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												return false;
											}, false, false);
										}
										else if (fsm.FsmName == "Throttle")
										{
											this.chokeFsm2 = fsm;
										}
									}
									else if (fsm.gameObject.name == "WaspHive" && fsm.FsmName == "Data")
									{
										this.waspNestFsm = fsm;
									}
									else if (fsm.gameObject.name == "TurnSignalsX" && fsm.FsmName == "Usage")
									{
										this.indicatorsFsm = fsm;
									}
									else if (fsm.gameObject.name == "Electricity" && fsm.FsmName == "Power" && this.ParentGameObject.name.StartsWith("JONNEZ"))
									{
										this.electricityFsm = fsm;
									}
									else if (fsm.gameObject.name == "Hydraulics" && fsm.FsmName == "Use")
									{
										this.hydraulicPumpFsm = fsm;
										this.isTruck = true;
									}
									else if (fsm.gameObject.name == "ButtonHandThrottle")
									{
										this.isTruck = true;
										if (fsm.FsmName == "Use")
										{
											EventHook.Add(fsm, "INCREASE", delegate
											{
												this.isHolding = true;
												NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												return false;
											}, false, false);
											EventHook.Add(fsm, "DECREASE", delegate
											{
												this.isHolding = true;
												NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												return false;
											}, false, false);
											EventHook.Add(fsm, "Wait button", delegate
											{
												if (this.isHolding)
												{
													this.isHolding = false;
													NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", fsm.Fsm.GetFsmFloat("LeverPos").Value, fsm.Fsm.GetFsmFloat("Throttle").Value));
												}
												return false;
											}, false, false);
											this.chokeFsm = fsm;
										}
										else if (fsm.FsmName == "Throttle")
										{
											this.chokeFsm2 = fsm;
										}
									}
									else if (fsm.gameObject.name == "Differential lock" && fsm.FsmName == "Use")
									{
										this.diffLockFsm = fsm;
									}
									else if (fsm.gameObject.name == "Liftaxle" && fsm.FsmName == "Use")
									{
										this.axleLiftFsm = fsm;
									}
									else if (fsm.gameObject.name == "OpenSpill" && fsm.FsmName == "Use")
									{
										this.spillValveFsm = fsm;
									}
									else if (fsm.gameObject.name == "KnobBeacon" && fsm.FsmName == "Use")
									{
										EventHook.AddWithSync(fsm, "Wait player", null, false);
										EventHook.AddWithSync(fsm, "Test", null, false);
									}
									else if (fsm.gameObject.name == "KnobWorkLights" && fsm.FsmName == "Use")
									{
										EventHook.AddWithSync(fsm, "Wait player", null, false);
										EventHook.AddWithSync(fsm, "Test", null, false);
									}
									else if (fsm.gameObject.name == "ActivateHose" && fsm.FsmName == "Use")
									{
										this.hoseFsm = fsm;
									}
									else if (fsm.gameObject.name == "Collider" && fsm.FsmName == "Use" && fsm.gameObject.transform.parent.name == "door")
									{
										this.sideFsm = fsm;
									}
									else if (fsm.gameObject.name == "Kickstand" && fsm.FsmName == "Use")
									{
										this.kickstandFsm = fsm;
									}
									else if (fsm.gameObject.name == "HookFront" && fsm.FsmName == "Logic")
									{
										if (!this.ParentGameObject.name.StartsWith("SAKER"))
										{
											this.frontHookFsm = fsm;
										}
									}
									else if (fsm.gameObject.name == "HookRear" && fsm.FsmName == "Logic")
									{
										if (!this.ParentGameObject.name.StartsWith("SAKER"))
										{
											this.rearHookFsm = fsm;
										}
									}
									else if (fsm.gameObject.name == "hose coupler(xxxxx)" && fsm.FsmName == "Attach")
									{
										this.hose = fsm;
									}
									else if (!(fsm.gameObject.name == "Tipping") || !(fsm.FsmName == "Tipping1"))
									{
										if (fsm.gameObject.name == "SeatbeltLock" || fsm.gameObject.name == "BuckleUp")
										{
											this.seatbeltFsm = fsm;
											EventHook.Add(this.seatbeltFsm, "State 2", () => false, false, false);
											EventHook.Add(this.seatbeltFsm, "Open", () => false, false, false);
										}
										else if (fsm.FsmName == "Installer")
										{
											Transform parent7 = fsm.transform.parent;
											if (((parent7 != null) ? parent7.name : null) == "CORRIS")
											{
												GameVehicleDatabase.Instance.RivettAssembly = fsm;
												EventHook.Add(fsm, "State 1", () => false, false, false);
											}
										}
									}
								}
							}
						}
					}
				}
				IL_1F11:;
			}
			this.HookEvents();
		}

		private void HookEvents()
		{
			if (this.fuelTankFsm == null && this.ParentGameObject.name == "SATSUMA(557kg, 248)")
			{
				this.fuelTankFsm = GameVehicleDatabase.Instance.satsumaFuelTankFsm;
			}
			if (this.ignitionFsm != null)
			{
				foreach (string text in new string[] { "Wait button", "Motor starting", "Motor OFF", "Test", "Shut off", "ACC on", "ACC on 2", "Wait1", "Wait2" })
				{
					EventHook.AddWithSync(this.ignitionFsm, text, null, false);
				}
			}
			if (this.dashboardFsm != null)
			{
				foreach (string text2 in new string[] { "ACC on", "Test", "ACC on 2", "Motor starting", "Shut off", "Motor OFF", "Wait button", "State 1" })
				{
					EventHook.AddWithSync(this.dashboardFsm, text2, null, false);
				}
			}
			if (this.hasRange)
			{
				EventHook.AddWithSync(this.rangeFsm, "Wait player", null, false);
				if (this.isTruck)
				{
					EventHook.AddWithSync(this.rangeFsm, "Switch", null, false);
				}
				else if (this.isTractor)
				{
					EventHook.AddWithSync(this.rangeFsm, "Flip", null, false);
					EventHook.AddWithSync(this.rangeFsm, "Flip 2", null, false);
				}
			}
			if (this.hasPushParkingBrake && this.handbrakeFsm != null)
			{
				EventHook.Add(this.handbrakeFsm, "DECREASE", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.hasPushParkingBrake, false, this.handbrakeFsm.Fsm.GetFsmFloat("KnobPos").Value, "");
					return false;
				}, true, false);
				EventHook.Add(this.handbrakeFsm, "INCREASE", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.hasPushParkingBrake, false, this.handbrakeFsm.Fsm.GetFsmFloat("KnobPos").Value, "");
					return false;
				}, true, false);
			}
			else if (!this.hasPushParkingBrake && this.handbrakeFsm != null && !this.hasLeverParkingBrake)
			{
				EventHook.AddWithSync(this.handbrakeFsm, "Wait player", null, false);
				EventHook.AddWithSync(this.handbrakeFsm, "Flip", null, false);
			}
			if (this.kickstandFsm != null)
			{
				EventHook.AddWithSync(this.kickstandFsm, "State 4", null, false);
			}
			if (this.horn != null)
			{
				EventHook.AddWithSync(this.horn, "Mouse off", null, false);
				EventHook.AddWithSync(this.horn, "State 1", null, false);
			}
			if (this.hasLeverParkingBrake)
			{
				EventHook.AddWithSync(this.handbrakeFsm, "Wait player", null, false);
				EventHook.AddWithSync(this.handbrakeFsm, "Flip", null, false);
			}
			if (this.kickstarterFsm != null)
			{
				EventHook.AddWithSync(this.kickstarterFsm, "Test", null, false);
				EventHook.AddWithSync(this.kickstarterFsm, "Motor starting", null, false);
				EventHook.AddWithSync(this.kickstarterFsm, "Motor OFF", null, false);
			}
			if (this.frontHydraulicFsm != null)
			{
				EventHook.AddWithSync(this.frontHydraulicFsm, "Wait player 2", null, false);
				EventHook.AddWithSync(this.frontHydraulicFsm, "INCREASE 2", null, false);
				EventHook.AddWithSync(this.frontHydraulicFsm, "DECREASE 2", null, false);
			}
			if (this.frontHydraulicFsm2 != null)
			{
				EventHook.AddWithSync(this.frontHydraulicFsm2, "Wait player", null, false);
				EventHook.AddWithSync(this.frontHydraulicFsm2, "INCREASE", null, false);
				EventHook.AddWithSync(this.frontHydraulicFsm2, "DECREASE", null, false);
			}
			if (this.gifuStall != null)
			{
				EventHook.Add(this.gifuStall, "Starting engine", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.GifuStall, !this.gifuStall.Fsm.GetFsmBool("ShutOff").Value, -1f, "");
					return false;
				}, false, false);
				EventHook.AddWithSync(this.gifuStall, "Start engine", null, false);
				EventHook.AddWithSync(this.gifuStall, "Starting engine", null, false);
				EventHook.AddWithSync(this.gifuStall, "Motor running", null, false);
				EventHook.Add(this.gifuStall, "Motor running", () => false, false, false);
			}
			if (this.fuelTapFsm != null)
			{
				EventHook.AddWithSync(this.fuelTapFsm, "Wait player 2", null, false);
				EventHook.AddWithSync(this.fuelTapFsm, "On", delegate
				{
					if (!NetManager.Instance.IsNetworkPlayerConnected())
					{
						this.fuelTapFsm.SendEvent("MP_Wait player 2");
					}
					return false;
				}, false);
				EventHook.AddWithSync(this.fuelTapFsm, "Off", delegate
				{
					if (!NetManager.Instance.IsNetworkPlayerConnected())
					{
						this.fuelTapFsm.SendEvent("MP_Wait player 2");
					}
					return false;
				}, false);
				EventHook.AddWithSync(this.fuelTapFsm, "Res", delegate
				{
					if (!NetManager.Instance.IsNetworkPlayerConnected())
					{
						this.fuelTapFsm.SendEvent("MP_Wait player 2");
					}
					return false;
				}, false);
			}
			if (this.fuelCapFsm != null)
			{
				EventHook.AddWithSync(this.fuelCapFsm, "Mouse off 2", null, false);
				EventHook.AddWithSync(this.fuelCapFsm, "Wait", null, false);
				EventHook.AddWithSync(this.fuelCapFsm, "Screw", null, false);
				EventHook.AddWithSync(this.fuelCapFsm, "Unscrew", null, false);
				EventHook.AddWithSync(this.fuelCapFsm, "State 1", null, false);
				EventHook.AddWithSync(this.fuelCapFsm, "State 2", null, false);
			}
			if (this.fuelHatchFsm != null)
			{
				EventHook.Add(this.fuelHatchFsm, "Open door", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FuelHatch, !this.fuelHatchFsm.Fsm.GetFsmBool("DoorOpen").Value, -1f, "");
					return false;
				}, false, false);
				EventHook.Add(this.fuelHatchFsm, "Close door", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FuelHatch, !this.fuelHatchFsm.Fsm.GetFsmBool("DoorOpen").Value, -1f, "");
					return false;
				}, false, false);
			}
			if (this.lightsFsm != null)
			{
				if (this.ParentGameObject.name == "SATSUMA(557kg, 248)")
				{
					EventHook.AddWithSync(this.lightsFsm, "Sound 2", null, false);
				}
				else if (!this.ParentGameObject.name.StartsWith("JONNEZ ES"))
				{
					EventHook.AddWithSync(this.lightsFsm, "Sound", null, false);
				}
				if (this.ParentGameObject.name.StartsWith("JONNEZ ES"))
				{
					EventHook.AddWithSync(this.lightsFsm, "Test", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Wait player 2", delegate
					{
						if (this.electricityFsm != null)
						{
							this.electricityFsm.Fsm.GetFsmBool("ACC").Value = true;
						}
						return false;
					}, false);
					EventHook.AddWithSync(this.lightsFsm, "Off", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Shorts", null, false);
				}
				else
				{
					EventHook.AddWithSync(this.lightsFsm, "State 1", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Wait button", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Wait player 2", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Check down", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Off", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Shorts", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Longs", null, false);
				}
			}
			string[] array2 = new string[] { "Wait player 2", "Activate cable", "Wait player 3", "Activate cable 2", "Remove rope", "State 3", "Get car" };
			if (this.frontHookFsm != null)
			{
				foreach (string text3 in array2)
				{
					EventHook.AddWithSync(this.frontHookFsm, text3, null, false);
				}
			}
			if (this.rearHookFsm != null)
			{
				foreach (string text4 in array2)
				{
					EventHook.AddWithSync(this.rearHookFsm, text4, null, false);
				}
			}
			if (this.sideFsm != null)
			{
				EventHook.AddWithSync(this.sideFsm, "Open door", null, false);
				EventHook.AddWithSync(this.sideFsm, "Close door", null, false);
				EventHook.AddWithSync(this.sideFsm, "Mouse off", null, false);
			}
			if (this.indicatorsFsm != null)
			{
				EventHook.AddWithSync(this.indicatorsFsm, "Activate dash", null, false);
				EventHook.AddWithSync(this.indicatorsFsm, "Activate dash 2", null, false);
				EventHook.AddWithSync(this.indicatorsFsm, "On", () => !this.DriverIsLocal, false);
				EventHook.AddWithSync(this.indicatorsFsm, "On 2", () => !this.DriverIsLocal, false);
				EventHook.AddWithSync(this.indicatorsFsm, "Off", () => !this.DriverIsLocal, false);
				EventHook.AddWithSync(this.indicatorsFsm, "Off 2", () => !this.DriverIsLocal, false);
				EventHook.AddWithSync(this.indicatorsFsm, "State 3", delegate
				{
					if (!this.DriverIsLocal)
					{
						Logger.Info("Turning off indicators!");
						GameObject gameObject = this.gameObject.transform.FindChild("LOD/Electricity/PowerON/Blinkers/Left").gameObject;
						GameObject gameObject2 = this.gameObject.transform.FindChild("LOD/Electricity/PowerON/Blinkers/Right").gameObject;
						if (gameObject == null)
						{
							gameObject = this.gameObject.transform.FindChild("LOD/Electricity 1/PowerON/Blinkers/Left").gameObject;
							gameObject2 = this.gameObject.transform.FindChild("LOD/Electricity 1/PowerON/Blinkers/Right").gameObject;
						}
						gameObject.SetActive(false);
						gameObject2.SetActive(false);
						if (gameObject == null)
						{
							Logger.Info("Left indicator not found!");
						}
					}
					return false;
				}, false);
			}
			if (this.radioFsm != null)
			{
				EventHook.AddWithSync(this.radioFsm, "Mouse off 2", null, false);
				EventHook.Add(this.radioFsm, "Increase", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT, true, this.radioFsm.Fsm.GetFsmFloat("Rot").Value + 10f, "");
					return false;
				}, false, false);
				EventHook.Add(this.radioFsm, "Decrease", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT, false, this.radioFsm.Fsm.GetFsmFloat("Rot").Value - 10f, "");
					return false;
				}, false, false);
			}
			if (this.radioVFsm != null)
			{
				EventHook.AddWithSync(this.radioVFsm, "Mouse off 2", null, false);
				EventHook.Add(this.radioVFsm, "Increase", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV, true, this.radioVFsm.Fsm.GetFsmFloat("Rot").Value + 27f, "");
					return false;
				}, false, false);
				EventHook.Add(this.radioVFsm, "Decrease", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV, false, this.radioVFsm.Fsm.GetFsmFloat("Rot").Value - 27f, "");
					return false;
				}, false, false);
			}
			if (this.tipping != null)
			{
				EventHook.AddWithSync(this.tipping, "Get up", null, false);
				EventHook.AddWithSync(this.tipping, "Tip over", null, false);
			}
			if (this.starterFsm != null)
			{
				foreach (string text5 in new string[]
				{
					"Wait", "ACC", "Starting engine", "Stall engine", "Fuel Mixture", "Fuel Mixture 2", "ACC / Glowplug", "Turn Key", "Start engine", "Wait for start",
					"State 1"
				})
				{
					try
					{
						EventHook.Add(this.starterFsm, text5, delegate
						{
							if (!Utils.IsReceiver(this.starterFsm))
							{
								NetLocalPlayer.Instance.WriteVehicleStateMessage(this.syncComponent, this.starterFsm.Fsm.ActiveStateName, false);
							}
							return false;
						}, false, false);
					}
					catch
					{
					}
				}
				EventHook.Add(this.starterFsm, "Running", delegate
				{
					this.driveTrain.canStall = this.DriverIsLocal;
					return false;
				}, false, true);
			}
			if (this.wipersFsm != null)
			{
				if (this.ParentGameObject.name == "SATSUMA(557kg, 248)")
				{
					EventHook.AddWithSync(this.wipersFsm, "Wait button", null, false);
				}
				else
				{
					EventHook.AddWithSync(this.wipersFsm, "Wait button 2", null, false);
				}
				EventHook.AddWithSync(this.wipersFsm, "Check car", null, false);
				EventHook.AddWithSync(this.wipersFsm, "Off", null, false);
				EventHook.AddWithSync(this.wipersFsm, "Slow", null, false);
				EventHook.AddWithSync(this.wipersFsm, "Fast", null, false);
			}
			if (this.lightsFsm != null)
			{
				if (this.isTruck)
				{
					EventHook.AddWithSync(this.interiorLightFsm, "Wait player", null, false);
					EventHook.AddWithSync(this.interiorLightFsm, "Switch", null, false);
				}
				else
				{
					EventHook.Add(this.interiorLightFsm, "Flip 2", delegate
					{
						NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.InteriorLight, !this.interiorLightFsm.Fsm.GetFsmBool("LightON").Value, -1f, "");
						return false;
					}, false, false);
				}
			}
			if (this.isTruck)
			{
				EventHook.AddWithSync(this.hydraulicPumpFsm, "Wait player", null, false);
				EventHook.AddWithSync(this.hydraulicPumpFsm, "Test", null, false);
				if (this.frontHydraulicFsm != null)
				{
					EventHook.Add(this.frontHydraulicFsm, "State 1", delegate
					{
						NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FrontHyd, false, this.frontHydraulicFsm.Fsm.GetFsmFloat("ArmRot").Value, "");
						return false;
					}, false, false);
				}
				if (this.frontHydraulicFsm2 != null)
				{
					EventHook.Add(this.frontHydraulicFsm2, "State 1", delegate
					{
						NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FrontHyd2, false, this.frontHydraulicFsm2.Fsm.GetFsmFloat("ArmRot").Value, "");
						return false;
					}, false, false);
				}
				EventHook.Add(this.spillValveFsm, "Switch", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.SpillValve, !this.spillValveFsm.Fsm.GetFsmBool("Open").Value, -1f, "");
					return false;
				}, false, false);
				EventHook.Add(this.axleLiftFsm, "Test", delegate
				{
					if (!this.axleLiftFirstRun)
					{
						NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.AxleLift, !this.axleLiftFsm.Fsm.GetFsmBool("Up").Value, -1f, "");
					}
					else
					{
						this.axleLiftFirstRun = false;
					}
					return false;
				}, false, false);
				EventHook.Add(this.diffLockFsm, "Test", delegate
				{
					if (!this.diffLockFirstRun)
					{
						NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.DiffLock, !this.diffLockFsm.Fsm.GetFsmBool("Lock").Value, -1f, "");
					}
					else
					{
						this.diffLockFirstRun = false;
					}
					return false;
				}, false, false);
				EventHook.AddWithSync(this.hoseFsm, "Mouse off", null, false);
				EventHook.AddWithSync(this.hoseFsm, "Mouse off 2", null, false);
				EventHook.AddWithSync(this.hoseFsm, "Remove part", null, false);
				EventHook.AddWithSync(this.hoseFsm, "Put hose back", null, false);
			}
			if (this.waspNestFsm != null)
			{
				EventHook.AddWithSync(this.waspNestFsm, "State 2", null, false);
			}
			this.hose != null;
			if (NetManager.Instance.IsOnline && !NetManager.Instance.IsHost)
			{
				this.syncComponent.RequestObjectSync();
			}
			if (this.rigidbody != null)
			{
				this.rigidbody.isKinematic = true;
				this.rigidbody.isKinematic = false;
			}
		}

		public void SyncRadioTForSatsuma()
		{
			this.satsumaRadioT = GameVehicleDatabase.Instance.radioT;
			if (this.satsumaRadioT != null && !this.HasBeenSynced(this.satsumaRadioT))
			{
				EventHook.AddWithSync(this.satsumaRadioT, "Mouse off 2", null, false);
				EventHook.Add(this.satsumaRadioT, "Increase", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT2, true, this.satsumaRadioT.Fsm.GetFsmFloat("Rot").Value + 10f, "");
					return false;
				}, false, false);
				EventHook.Add(this.satsumaRadioT, "Decrease", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT2, false, this.satsumaRadioT.Fsm.GetFsmFloat("Rot").Value - 10f, "");
					return false;
				}, false, false);
			}
		}

		public void SyncRadioVForSatsuma()
		{
			this.satsumaRadioV = GameVehicleDatabase.Instance.radioV;
			if (this.satsumaRadioV != null && !this.HasBeenSynced(this.satsumaRadioV))
			{
				EventHook.AddWithSync(this.satsumaRadioV, "Mouse off 2", null, false);
				EventHook.Add(this.satsumaRadioV, "Increase", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV2, true, this.satsumaRadioV.Fsm.GetFsmFloat("Rot").Value + 27f, "");
					return false;
				}, false, false);
				EventHook.Add(this.satsumaRadioV, "Decrease", delegate
				{
					if (this.isReceiver)
					{
						this.isReceiver = false;
						return false;
					}
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV2, false, this.satsumaRadioV.Fsm.GetFsmFloat("Rot").Value - 27f, "");
					return false;
				}, false, false);
			}
		}

		public void SyncChoke()
		{
			if (this.chokeFsm != null && !this.HasBeenSynced(this.chokeFsm))
			{
				EventHook.AddWithSync(this.chokeFsm, "Wait player", null, false);
				EventHook.Add(this.chokeFsm, "DECREASE", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
					return false;
				}, true, false);
				EventHook.Add(this.chokeFsm, "INCREASE", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
					return false;
				}, true, false);
			}
		}

		private bool HasBeenSynced(PlayMakerFSM fsm)
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

		private void RemoveDeadBody()
		{
			for (int i = 0; i < this.ParentGameObject.transform.childCount; i++)
			{
				try
				{
					Logger.Debug("Remove body " + this.ParentGameObject.transform.GetChild(i).gameObject.name);
					if (this.ParentGameObject.transform.GetChild(i).gameObject.name == "DeadBody")
					{
						Object.Destroy(this.ParentGameObject.transform.GetChild(i).gameObject);
						Logger.Debug("Destroyed  abd breaking");
						break;
					}
				}
				catch
				{
				}
			}
		}

		private void UnlockRico()
		{
			foreach (Component component in this.ParentGameObject.GetComponents<Component>())
			{
				if (component.GetType().Name == "InteractionRaycast")
				{
					component.GetType().GetField("rayDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component, 1.5f);
					return;
				}
			}
		}

		private void UnlockSaker()
		{
			foreach (Component component in this.ParentGameObject.GetComponents<Component>())
			{
				if (component.GetType().Name == "KeyHandler")
				{
					component.GetType().GetField("PlayerHasKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component, true);
				}
			}
			this.ParentGameObject.transform.GetChild(10).GetComponents<MonoBehaviour>()[1].enabled = false;
		}

		private void UnlockPanier()
		{
			Transform transform = this.ParentGameObject.transform;
			foreach (Component component in transform.GetChild(17).GetChild(0).GetComponents<Component>())
			{
				if (component.GetType().Name == "InteractionRaycast")
				{
					Logger.Debug("Interaction raycast found, tryna set value " + component.GetType().Name);
					component.GetType().GetField("rayDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component, 1.5f);
					break;
				}
			}
			foreach (Component component2 in transform.GetChild(17).GetChild(5).GetComponents<Component>())
			{
				if (component2.GetType().Name == "InteractionRaycast")
				{
					component2.GetType().GetField("rayDistance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component2, 1.5f);
					break;
				}
			}
			foreach (Component component3 in transform.GetChild(8).GetChild(1).GetChild(0)
				.GetChild(1)
				.GetComponents<Component>())
			{
				if (component3.GetType().Name == "DoorLock")
				{
					component3.GetType().GetField("Locked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component3, false);
					break;
				}
			}
			foreach (Component component4 in transform.GetChild(8).GetChild(1).GetChild(1)
				.GetChild(1)
				.GetComponents<Component>())
			{
				if (component4.GetType().Name == "DoorLock")
				{
					component4.GetType().GetField("Locked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component4, false);
					break;
				}
			}
			foreach (Component component5 in transform.GetChild(8).GetChild(1).GetChild(2)
				.GetChild(11)
				.GetComponents<Component>())
			{
				if (component5.GetType().Name == "BootLock")
				{
					component5.GetType().GetField("Locked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(component5, false);
					return;
				}
			}
		}

		public void LateFind()
		{
			List<PlayMakerFSM> satsumaSyncables = GameVehicleDatabase.Instance.satsumaSyncables;
			Logger.Debug("Commencing latefind");
			foreach (PlayMakerFSM playMakerFSM in satsumaSyncables)
			{
				if (playMakerFSM.gameObject.name == "Handles" && playMakerFSM.FsmName == "Use" && this.ParentGameObject.name == "SATSUMA(557kg, 248)")
				{
					this.bootFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "LightModes" && playMakerFSM.FsmName == "Use")
				{
					this.lightsFsm = playMakerFSM;
				}
				else if ((playMakerFSM.gameObject.name == "Hazard" || playMakerFSM.gameObject.name == "ButtonHazard") && playMakerFSM.FsmName == "Use")
				{
					this.hazardsFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "ButtonWipers" && playMakerFSM.FsmName == "Use")
				{
					this.wipersFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "glovbox" && playMakerFSM.FsmName == "Use")
				{
					this.gloveboxFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "Choke" && playMakerFSM.FsmName == "Use")
				{
					this.chokeFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "Trigger" && playMakerFSM.gameObject.transform.parent.name == "HoodLocking" && playMakerFSM.FsmName == "Use")
				{
					this.hoodlockFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "hood(Clone)" && playMakerFSM.FsmName == "Use")
				{
					this.bootlidFsm = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "opener" && playMakerFSM.FsmName == "Knob" && playMakerFSM.gameObject.transform.parent.name == "door left(Clone)")
				{
					this.knobLeft = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "opener" && playMakerFSM.FsmName == "Knob" && playMakerFSM.gameObject.transform.parent.name == "door right(Clone)")
				{
					this.knobRight = playMakerFSM;
				}
				else if (playMakerFSM.gameObject.name == "seattop" && playMakerFSM.FsmName == "Use")
				{
					this.seatFsm = playMakerFSM;
				}
			}
			if (this.seatFsm && this.bootFsm && this.lightsFsm && this.hazardsFsm && this.wipersFsm && this.gloveboxFsm && this.chokeFsm && this.bootlidFsm && this.knobLeft && this.knobRight)
			{
				Logger.Debug("Late Finding done. Now hooking late.");
				this.LateHookEvents();
				return;
			}
			Logger.Debug(string.Format("Now hooking {0}{1}{2}{3}{4}{5}{6}{7}{8} {9}", new object[]
			{
				this.wipersFsm != null,
				this.gloveboxFsm != null,
				this.chokeFsm != null,
				this.bootlidFsm != null,
				this.knobLeft != null,
				this.knobRight != null,
				this.hazardsFsm != null,
				this.lightsFsm != null,
				this.seatFsm != null,
				this.bootFsm != null
			}));
		}

		private void LateHookEvents()
		{
			try
			{
				bool flag = false;
				bool flag2 = false;
				if (this.gloveboxFsm != null)
				{
					if (!this.gloveboxFsm.gameObject.activeSelf)
					{
						flag = true;
						this.gloveboxFsm.gameObject.SetActive(true);
					}
					if (this.gloveboxFsm.gameObject.transform.parent != null && !this.gloveboxFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.gloveboxFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.gloveboxFsm, "Mouse over 1", null, false);
					EventHook.AddWithSync(this.gloveboxFsm, "Mouse over 2", null, false);
					EventHook.AddWithSync(this.gloveboxFsm, "Open door", null, false);
					EventHook.AddWithSync(this.gloveboxFsm, "Close door", null, false);
					if (flag)
					{
						flag = false;
						this.gloveboxFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent = this.gloveboxFsm.gameObject.transform.parent;
						if (parent != null)
						{
							parent.gameObject.SetActive(false);
						}
					}
				}
				if (this.hoodlockFsm != null)
				{
					if (!this.hoodlockFsm.gameObject.activeSelf)
					{
						flag = true;
						this.hoodlockFsm.gameObject.SetActive(true);
					}
					if (this.hoodlockFsm.gameObject.transform.parent != null && !this.hoodlockFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.hoodlockFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.hoodlockFsm, "Wait button", null, false);
					EventHook.AddWithSync(this.hoodlockFsm, "State 1", null, false);
					if (flag)
					{
						flag = false;
						this.hoodlockFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent2 = this.hoodlockFsm.gameObject.transform.parent;
						if (parent2 != null)
						{
							parent2.gameObject.SetActive(false);
						}
					}
				}
				if (this.bootlidFsm != null)
				{
					if (!this.bootlidFsm.gameObject.activeSelf)
					{
						flag = true;
						this.bootlidFsm.gameObject.SetActive(true);
					}
					if (this.bootlidFsm.gameObject.transform.parent != null && !this.bootlidFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.bootlidFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.bootlidFsm, "Mouse over 3", null, false);
					EventHook.AddWithSync(this.bootlidFsm, "Mouse over 4", null, false);
					EventHook.AddWithSync(this.bootlidFsm, "State 2", null, false);
					EventHook.AddWithSync(this.bootlidFsm, "Open hood 2", null, false);
					if (flag)
					{
						flag = false;
						this.bootlidFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent3 = this.bootlidFsm.gameObject.transform.parent;
						if (parent3 != null)
						{
							parent3.gameObject.SetActive(false);
						}
					}
				}
				if (this.hazardsFsm != null)
				{
					if (!this.hazardsFsm.gameObject.activeSelf)
					{
						flag = true;
						this.hazardsFsm.gameObject.SetActive(true);
					}
					if (this.hazardsFsm.gameObject.transform.parent != null && !this.hazardsFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.hazardsFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.hazardsFsm, "Wait buttons", null, false);
					EventHook.AddWithSync(this.hazardsFsm, "On", null, false);
					EventHook.AddWithSync(this.hazardsFsm, "Off", null, false);
					if (flag)
					{
						flag = false;
						this.hazardsFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent4 = this.hazardsFsm.gameObject.transform.parent;
						if (parent4 != null)
						{
							parent4.gameObject.SetActive(false);
						}
					}
				}
				if (this.bootFsm != null)
				{
					if (!this.bootFsm.gameObject.activeSelf)
					{
						flag = true;
						this.bootFsm.gameObject.SetActive(true);
					}
					if (this.bootFsm.gameObject.transform.parent != null && !this.bootFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.bootFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.bootFsm, "Open hood", null, false);
					EventHook.AddWithSync(this.bootFsm, "Drop", null, false);
					EventHook.AddWithSync(this.bootFsm, "Mouse over", null, false);
					if (flag)
					{
						flag = false;
						this.bootFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent5 = this.bootFsm.gameObject.transform.parent;
						if (parent5 != null)
						{
							parent5.gameObject.SetActive(false);
						}
					}
				}
				if (this.lightsFsm != null)
				{
					if (!this.lightsFsm.gameObject.activeSelf)
					{
						flag = true;
						this.lightsFsm.gameObject.SetActive(true);
					}
					if (this.lightsFsm.gameObject.transform.parent != null && !this.lightsFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.lightsFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.lightsFsm, "Sound 2", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Test 2", null, false);
					EventHook.AddWithSync(this.lightsFsm, "State 1", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Wait button", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Check down", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Off", null, false);
					EventHook.AddWithSync(this.lightsFsm, "On or Off", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Shorts", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Longs", null, false);
					EventHook.AddWithSync(this.lightsFsm, "Mode add", null, false);
					if (flag)
					{
						flag = false;
						this.lightsFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent6 = this.lightsFsm.gameObject.transform.parent;
						if (parent6 != null)
						{
							parent6.gameObject.SetActive(false);
						}
					}
				}
				if (this.wipersFsm != null)
				{
					if (!this.wipersFsm.gameObject.activeSelf)
					{
						flag = true;
						this.wipersFsm.gameObject.SetActive(true);
					}
					if (this.wipersFsm.gameObject.transform.parent != null && !this.wipersFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.wipersFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.wipersFsm, "Wait button", null, false);
					EventHook.AddWithSync(this.wipersFsm, "Check car", null, false);
					EventHook.AddWithSync(this.wipersFsm, "Off", null, false);
					EventHook.AddWithSync(this.wipersFsm, "Slow", null, false);
					EventHook.AddWithSync(this.wipersFsm, "Fast", null, false);
					if (flag)
					{
						flag = false;
						this.wipersFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent7 = this.wipersFsm.gameObject.transform.parent;
						if (parent7 != null)
						{
							parent7.gameObject.SetActive(false);
						}
					}
				}
				if (this.seatFsm != null)
				{
					if (!this.seatFsm.gameObject.activeSelf)
					{
						flag = true;
						this.seatFsm.gameObject.SetActive(true);
					}
					if (this.seatFsm.gameObject.transform.parent != null && !this.seatFsm.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.seatFsm.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.seatFsm, "Mouse over 1", null, false);
					EventHook.AddWithSync(this.seatFsm, "Mouse over 2", null, false);
					EventHook.AddWithSync(this.seatFsm, "Open door", null, false);
					EventHook.AddWithSync(this.seatFsm, "Close door", null, false);
					if (flag)
					{
						flag = false;
						this.seatFsm.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent8 = this.seatFsm.gameObject.transform.parent;
						if (parent8 != null)
						{
							parent8.gameObject.SetActive(false);
						}
					}
				}
				if (this.knobLeft != null)
				{
					if (!this.knobLeft.gameObject.activeSelf)
					{
						flag = true;
						this.knobLeft.gameObject.SetActive(true);
					}
					if (this.knobLeft.gameObject.transform.parent != null && !this.knobLeft.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.knobLeft.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.knobLeft, "Get scroll", null, false);
					EventHook.AddWithSync(this.knobLeft, "State 1", null, false);
					EventHook.AddWithSync(this.knobLeft, "State 2", null, false);
					if (flag)
					{
						flag = false;
						this.knobLeft.gameObject.SetActive(false);
					}
					if (flag2)
					{
						flag2 = false;
						Transform parent9 = this.knobLeft.gameObject.transform.parent;
						if (parent9 != null)
						{
							parent9.gameObject.SetActive(false);
						}
					}
				}
				if (this.knobRight != null)
				{
					if (!this.knobRight.gameObject.activeSelf)
					{
						flag = true;
						this.knobRight.gameObject.SetActive(true);
					}
					if (this.knobRight.gameObject.transform.parent != null && !this.knobRight.gameObject.transform.parent.gameObject.activeSelf)
					{
						flag2 = true;
						this.knobRight.gameObject.transform.parent.gameObject.SetActive(true);
					}
					EventHook.AddWithSync(this.knobRight, "Get scroll", null, false);
					EventHook.AddWithSync(this.knobRight, "State 1", null, false);
					EventHook.AddWithSync(this.knobRight, "State 2", null, false);
					if (flag)
					{
						this.knobRight.gameObject.SetActive(false);
					}
					if (flag2)
					{
						Transform parent10 = this.knobRight.gameObject.transform.parent;
						if (parent10 != null)
						{
							parent10.gameObject.SetActive(false);
						}
					}
				}
				EventHook.AddWithSync(this.chokeFsm, "Wait player", null, false);
				EventHook.Add(this.chokeFsm, "DECREASE", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
					return false;
				}, true, false);
				EventHook.Add(this.chokeFsm, "INCREASE", delegate
				{
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
					return false;
				}, true, false);
			}
			catch (Exception ex)
			{
				string text = "LateHook Crashed with ";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		private void PlayerEventHooks(PlayMakerFSM fsm = null, GameObject go = null)
		{
			if (fsm != null)
			{
				EventHook.Add(fsm, "Player in car", delegate
				{
					this.CurrentDrivingState = PlayerVehicle.DrivingStates.Driver;
					this.DriverIsLocal = true;
					this.SetRemoteSteering(false);
					BootBox bootBox = this.bootBox;
					if (bootBox != null)
					{
						bootBox.ClaimOwnershipOfItems();
					}
					NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 0);
					this.syncComponent.SyncEnabled = true;
					this.dynamics.enabled = true;
					if (this.driveTrain.rpm > 100f)
					{
						this.driveTrain.canStall = true;
					}
					return false;
				}, false, false);
				EventHook.Add(fsm, "Wait for player", delegate
				{
					if (this.CurrentDrivingState == PlayerVehicle.DrivingStates.Driver && this.DriverIsLocal)
					{
						this.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
						this.DriverIsLocal = false;
						if (this.driveTrain.rpm > 100f)
						{
							this.driveTrain.canStall = false;
						}
						NetLocalPlayer.Instance.LeaveVehicle();
					}
					return false;
				}, false, false);
				Logger.Debug("Creating player now");
				EventHook.Add(fsm, "Create player", delegate
				{
					BootBox bootBox2 = this.bootBox;
					if (bootBox2 != null)
					{
						bootBox2.UnclaimOwnership();
					}
					return false;
				}, false, false);
				this.SeatTransform = fsm.gameObject.transform;
			}
			else if (go != null)
			{
				this.SeatTransform = go.transform;
			}
			if (!this.ParentGameObject.name.StartsWith("Panier") && this.SeatTransform.gameObject.name.StartsWith("DriveTrigger") && !this.ParentGameObject.name.StartsWith("KEKMET") && !this.ParentGameObject.name.StartsWith("JONNEZ ES"))
			{
				this.AddPassengerSeat(fsm);
				this.AddRearSeat(fsm);
				this.AddBootBox(fsm);
				return;
			}
			if (this.ParentGameObject.name.StartsWith("JONNEZ ES"))
			{
				this.AddPassengerSeat(fsm);
				return;
			}
			if (this.ParentGameObject.name.StartsWith("KEKMET"))
			{
				this.AddPassengerSeat(fsm);
				this.AddBootBox(fsm);
			}
		}

		public void EnterDrivingMode()
		{
			this.CurrentDrivingState = PlayerVehicle.DrivingStates.Driver;
			this.DriverIsLocal = true;
			this.SetRemoteSteering(false);
			NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 0);
			this.syncComponent.SyncEnabled = true;
		}

		public void ExitDrivingMode()
		{
			if (this.CurrentDrivingState == PlayerVehicle.DrivingStates.Driver && this.DriverIsLocal)
			{
				this.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
				this.DriverIsLocal = false;
				NetLocalPlayer.Instance.LeaveVehicle();
			}
		}

		public void ForceLeaveCar()
		{
			if (this.CurrentDrivingState == PlayerVehicle.DrivingStates.Driver && this.DriverIsLocal)
			{
				this.seatbeltFsm.SendEvent("MP_State 2");
				this.seatbeltFsm.SendEvent("MP_Open");
				this.playerTrigger.SendEvent("MP_Create player");
				this.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
				this.DriverIsLocal = false;
				NetLocalPlayer.Instance.LeaveVehicle();
				return;
			}
			if (this.CurrentDrivingState == PlayerVehicle.DrivingStates.Passenger)
			{
				this.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
				this.PassengerSeatTransform.GetComponent<PassengerSeat>().ForceLeaveSeat();
				return;
			}
			if (this.CurrentDrivingState == PlayerVehicle.DrivingStates.Backseater)
			{
				this.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
				this.RearSeatTransform.GetComponent<PassengerSeat>().ForceLeaveSeat();
			}
		}

		public void SetEngineState(string state, float startTime, bool ignition)
		{
			if (ignition)
			{
				this.ignitionFsm.SendEvent("MP_" + state);
				return;
			}
			this.starterFsm.SendEvent("MP_" + state);
		}

		public void LoadWiring()
		{
			foreach (PlayMakerFSM playMakerFSM in this.gameObject.GetComponentsInChildren<PlayMakerFSM>())
			{
				if (playMakerFSM.name.StartsWith("Wiring"))
				{
					if (playMakerFSM.FsmName == "Status")
					{
						playMakerFSM.enabled = false;
					}
					else if (playMakerFSM.FsmName == "Data")
					{
						playMakerFSM.Fsm.GetFsmBool("Installed").Value = true;
						playMakerFSM.Fsm.GetFsmBool("Bolted").Value = true;
						FsmGameObject fsmGameObject = playMakerFSM.Fsm.GetFsmGameObject("WireMesh");
						if (fsmGameObject != null)
						{
							fsmGameObject.Value.SetActive(true);
						}
						FsmGameObject fsmGameObject2 = playMakerFSM.Fsm.GetFsmGameObject("WireTriggers");
						if (fsmGameObject2 != null)
						{
							fsmGameObject2.Value.SetActive(false);
						}
					}
				}
			}
		}

		public void SetVehicleSwitch(PlayerVehicle.SwitchIDs state, bool newValue, float newValueFloat, string newValueString)
		{
			if (state == PlayerVehicle.SwitchIDs.hasPushParkingBrake)
			{
				this.handbrakeFsm.Fsm.GetFsmFloat("KnobPos").Value = newValueFloat;
				return;
			}
			if (state == PlayerVehicle.SwitchIDs.Choke)
			{
				string[] array = newValueString.Split(new char[] { 'K' });
				float num;
				float num2;
				if (array.Length == 2 && float.TryParse(array[0], out num) && float.TryParse(array[1], out num2))
				{
					if (this.isTruck)
					{
						this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value = num2;
						this.chokeFsm2.Fsm.GetFsmFloat("Throttle").Value = num2;
						this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value = num;
						this.chokeFsm.Fsm.GetFsmGameObject("ButtonMesh").Value.transform.localPosition = new Vector3(0f, num, 0f);
						return;
					}
					if (this.isTractor)
					{
						this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value = num2;
						this.chokeFsm2.Fsm.GetFsmFloat("Throttle").Value = num2;
						this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value = num;
						this.chokeFsm.Fsm.GetFsmGameObject("Pivot").Value.transform.localEulerAngles = new Vector3(num, 0f, 0f);
						return;
					}
					this.chokeFsm.Fsm.GetFsmFloat("Choke").Value = num;
					this.chokeFsm.Fsm.GetFsmFloat("Pos").Value = num2;
					return;
				}
			}
			else
			{
				if (state == PlayerVehicle.SwitchIDs.Sunflap)
				{
					this.sunflapFsm.Fsm.GetFsmFloat("Angle").Value = newValueFloat;
					this.sunflapFsm.transform.GetChild(0).localEulerAngles = new Vector3(newValueFloat, 0f, 0f);
					return;
				}
				if (state == PlayerVehicle.SwitchIDs.RadioT)
				{
					if (newValue)
					{
						if (newValueFloat > this.radioFsm.Fsm.GetFsmFloat("Rot").Value)
						{
							this.isReceiver = true;
							this.radioFsm.SendEvent("MP_Increase");
							return;
						}
					}
					else if (newValueFloat < this.radioFsm.Fsm.GetFsmFloat("Rot").Value)
					{
						this.isReceiver = true;
						this.radioFsm.SendEvent("MP_Decrease");
						return;
					}
				}
				else if (state == PlayerVehicle.SwitchIDs.RadioV)
				{
					if (newValue)
					{
						if (newValueFloat > this.radioVFsm.Fsm.GetFsmFloat("Rot").Value)
						{
							this.isReceiver = true;
							this.radioVFsm.SendEvent("MP_Increase");
							return;
						}
					}
					else if (newValueFloat < this.radioVFsm.Fsm.GetFsmFloat("Rot").Value)
					{
						this.isReceiver = true;
						this.radioVFsm.SendEvent("MP_Decrease");
						return;
					}
				}
				else if (state == PlayerVehicle.SwitchIDs.HandbrakeLever)
				{
					if (this.handbrakeFsm.Fsm.GetFsmBool("Brake").Value != newValue)
					{
						this.handbrakeFsm.SendEvent("MP_Flip");
						return;
					}
				}
				else if (state == PlayerVehicle.SwitchIDs.GifuStall)
				{
					if (this.gifuStall.Fsm.GetFsmBool("Starting").Value == this.gifuStall.Fsm.GetFsmBool("ShutOff").Value)
					{
						this.gifuStall.Fsm.GetFsmBool("ShutOff").Value = false;
						this.gifuStall.SendEvent("MP_Motor running");
						return;
					}
				}
				else if (state == PlayerVehicle.SwitchIDs.FuelHatch)
				{
					if (this.fuelCapFsm.Fsm.GetFsmBool("DoorOpen").Value != newValue && newValue)
					{
						this.fuelCapFsm.SendEvent("MP_Open door");
						return;
					}
					if (this.fuelCapFsm.Fsm.GetFsmBool("DoorOpen").Value != newValue && !newValue)
					{
						this.fuelCapFsm.SendEvent("MP_Open door");
						return;
					}
				}
				else if (state == PlayerVehicle.SwitchIDs.FuelTap)
				{
					if (this.fuelTapFsm.Fsm.GetFsmFloat("FuelTapState").Value != newValueFloat)
					{
						this.fuelTapFsm.SendEvent("MP_Test");
						return;
					}
				}
				else
				{
					if (state == PlayerVehicle.SwitchIDs.Starter)
					{
						this.tipping.Fsm.GetFsmFloat("Rot").Value = newValueFloat;
						return;
					}
					if (state != PlayerVehicle.SwitchIDs.Steer)
					{
						if (state == PlayerVehicle.SwitchIDs.Lights)
						{
							while ((float)this.lightsFsm.Fsm.GetFsmInt("Selection").Value != newValueFloat)
							{
								this.lightsFsm.SendEvent("MP_Test");
							}
							return;
						}
						if (state == PlayerVehicle.SwitchIDs.Wipers)
						{
							if ((float)this.wipersFsm.Fsm.GetFsmInt("Selection").Value != newValueFloat)
							{
								this.wipersFsm.SendEvent("MP_Test 2");
								return;
							}
						}
						else
						{
							if (state == PlayerVehicle.SwitchIDs.Starter)
							{
								this.starterFsm.Fsm.GetFsmBool("ShutOff").Value = false;
								return;
							}
							if (state == PlayerVehicle.SwitchIDs.InteriorLight)
							{
								if (this.isTruck)
								{
									if (this.interiorLightFsm.Fsm.GetFsmBool("On").Value != newValue)
									{
										this.interiorLightFsm.SendEvent("MP_Switch");
										return;
									}
								}
								else if (this.interiorLightFsm.Fsm.GetFsmBool("LightON").Value != newValue)
								{
									this.interiorLightFsm.SendEvent("MP_Flip 2");
									return;
								}
							}
							else if (state == PlayerVehicle.SwitchIDs.HydraulicPump)
							{
								if (this.hydraulicPumpFsm.Fsm.GetFsmBool("On").Value != newValue)
								{
									this.hydraulicPumpFsm.SendEvent("MP_Test");
									return;
								}
							}
							else
							{
								if (state == PlayerVehicle.SwitchIDs.FrontHyd)
								{
									this.frontHydraulicFsm.Fsm.GetFsmFloat("ArmRot").Value = newValueFloat;
									return;
								}
								if (state == PlayerVehicle.SwitchIDs.FrontHyd2)
								{
									this.frontHydraulicFsm2.Fsm.GetFsmFloat("ArmRot").Value = newValueFloat;
									return;
								}
								if (state == PlayerVehicle.SwitchIDs.SpillValve)
								{
									if (this.spillValveFsm.Fsm.GetFsmBool("Open").Value != newValue)
									{
										this.spillValveFsm.SendEvent("MP_Switch");
										return;
									}
								}
								else if (state == PlayerVehicle.SwitchIDs.AxleLift)
								{
									if (this.axleLiftFsm.Fsm.GetFsmBool("Up").Value != newValue)
									{
										this.axleLiftFsm.SendEvent("MP_Test");
										return;
									}
								}
								else if (state == PlayerVehicle.SwitchIDs.DiffLock && this.diffLockFsm.Fsm.GetFsmBool("Lock").Value != newValue)
								{
									this.diffLockFsm.SendEvent("MP_Test");
								}
							}
						}
					}
				}
			}
		}

		[CompilerGenerated]
		private bool <FindFSMs>b__122_0()
		{
			if (this.isLogRelated)
			{
				this.isLogRelated = false;
			}
			else
			{
				NetLocalPlayer.Instance.WriteVehicleStateMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), "DESTROYWOOD", false);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <FindFSMs>b__122_1()
		{
			if (this.isLogRelated)
			{
				this.isLogRelated = false;
			}
			else
			{
				NetLocalPlayer.Instance.WriteVehicleStateMessage(this.gameObject.GetComponent<ObjectSyncComponent>(), "DESTROYLOG", false);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <FindFSMs>b__122_2()
		{
			Peraportti.Instance.activeVehicle = this;
			return false;
		}

		[CompilerGenerated]
		private bool <FindFSMs>b__122_3()
		{
			try
			{
				this.trailerRemoveFSM.gameObject.SetActive(true);
			}
			catch
			{
			}
			return false;
		}

		[CompilerGenerated]
		private bool <FindFSMs>b__122_4()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Choke").Value, this.chokeFsm.Fsm.GetFsmFloat("Pos").Value));
			return false;
		}

		[CompilerGenerated]
		private bool <FindFSMs>b__122_5()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Choke").Value, this.chokeFsm.Fsm.GetFsmFloat("Pos").Value));
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_0()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.hasPushParkingBrake, false, this.handbrakeFsm.Fsm.GetFsmFloat("KnobPos").Value, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_1()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.hasPushParkingBrake, false, this.handbrakeFsm.Fsm.GetFsmFloat("KnobPos").Value, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_2()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.GifuStall, !this.gifuStall.Fsm.GetFsmBool("ShutOff").Value, -1f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_4()
		{
			if (!NetManager.Instance.IsNetworkPlayerConnected())
			{
				this.fuelTapFsm.SendEvent("MP_Wait player 2");
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_5()
		{
			if (!NetManager.Instance.IsNetworkPlayerConnected())
			{
				this.fuelTapFsm.SendEvent("MP_Wait player 2");
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_6()
		{
			if (!NetManager.Instance.IsNetworkPlayerConnected())
			{
				this.fuelTapFsm.SendEvent("MP_Wait player 2");
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_7()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FuelHatch, !this.fuelHatchFsm.Fsm.GetFsmBool("DoorOpen").Value, -1f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_8()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FuelHatch, !this.fuelHatchFsm.Fsm.GetFsmBool("DoorOpen").Value, -1f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_9()
		{
			if (this.electricityFsm != null)
			{
				this.electricityFsm.Fsm.GetFsmBool("ACC").Value = true;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_10()
		{
			return !this.DriverIsLocal;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_11()
		{
			return !this.DriverIsLocal;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_12()
		{
			return !this.DriverIsLocal;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_13()
		{
			return !this.DriverIsLocal;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_14()
		{
			if (!this.DriverIsLocal)
			{
				Logger.Info("Turning off indicators!");
				GameObject gameObject = this.gameObject.transform.FindChild("LOD/Electricity/PowerON/Blinkers/Left").gameObject;
				GameObject gameObject2 = this.gameObject.transform.FindChild("LOD/Electricity/PowerON/Blinkers/Right").gameObject;
				if (gameObject == null)
				{
					gameObject = this.gameObject.transform.FindChild("LOD/Electricity 1/PowerON/Blinkers/Left").gameObject;
					gameObject2 = this.gameObject.transform.FindChild("LOD/Electricity 1/PowerON/Blinkers/Right").gameObject;
				}
				gameObject.SetActive(false);
				gameObject2.SetActive(false);
				if (gameObject == null)
				{
					Logger.Info("Left indicator not found!");
				}
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_15()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT, true, this.radioFsm.Fsm.GetFsmFloat("Rot").Value + 10f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_16()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT, false, this.radioFsm.Fsm.GetFsmFloat("Rot").Value - 10f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_17()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV, true, this.radioVFsm.Fsm.GetFsmFloat("Rot").Value + 27f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_18()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV, false, this.radioVFsm.Fsm.GetFsmFloat("Rot").Value - 27f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_26()
		{
			if (!Utils.IsReceiver(this.starterFsm))
			{
				NetLocalPlayer.Instance.WriteVehicleStateMessage(this.syncComponent, this.starterFsm.Fsm.ActiveStateName, false);
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_25()
		{
			this.driveTrain.canStall = this.DriverIsLocal;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_19()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.InteriorLight, !this.interiorLightFsm.Fsm.GetFsmBool("LightON").Value, -1f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_20()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FrontHyd, false, this.frontHydraulicFsm.Fsm.GetFsmFloat("ArmRot").Value, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_21()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.FrontHyd2, false, this.frontHydraulicFsm2.Fsm.GetFsmFloat("ArmRot").Value, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_22()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.SpillValve, !this.spillValveFsm.Fsm.GetFsmBool("Open").Value, -1f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_23()
		{
			if (!this.axleLiftFirstRun)
			{
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.AxleLift, !this.axleLiftFsm.Fsm.GetFsmBool("Up").Value, -1f, "");
			}
			else
			{
				this.axleLiftFirstRun = false;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__125_24()
		{
			if (!this.diffLockFirstRun)
			{
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.DiffLock, !this.diffLockFsm.Fsm.GetFsmBool("Lock").Value, -1f, "");
			}
			else
			{
				this.diffLockFirstRun = false;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <SyncRadioTForSatsuma>b__128_0()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT2, true, this.satsumaRadioT.Fsm.GetFsmFloat("Rot").Value + 10f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <SyncRadioTForSatsuma>b__128_1()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioT2, false, this.satsumaRadioT.Fsm.GetFsmFloat("Rot").Value - 10f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <SyncRadioVForSatsuma>b__129_0()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV2, true, this.satsumaRadioV.Fsm.GetFsmFloat("Rot").Value + 27f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <SyncRadioVForSatsuma>b__129_1()
		{
			if (this.isReceiver)
			{
				this.isReceiver = false;
				return false;
			}
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.RadioV2, false, this.satsumaRadioV.Fsm.GetFsmFloat("Rot").Value - 27f, "");
			return false;
		}

		[CompilerGenerated]
		private bool <SyncChoke>b__130_0()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
			return false;
		}

		[CompilerGenerated]
		private bool <SyncChoke>b__130_1()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
			return false;
		}

		[CompilerGenerated]
		private bool <LateHookEvents>b__142_0()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
			return false;
		}

		[CompilerGenerated]
		private bool <LateHookEvents>b__142_1()
		{
			NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.chokeFsm.Fsm.GetFsmFloat("Throttle").Value, this.chokeFsm.Fsm.GetFsmFloat("LeverPos").Value));
			return false;
		}

		[CompilerGenerated]
		private bool <PlayerEventHooks>b__143_0()
		{
			this.CurrentDrivingState = PlayerVehicle.DrivingStates.Driver;
			this.DriverIsLocal = true;
			this.SetRemoteSteering(false);
			BootBox bootBox = this.bootBox;
			if (bootBox != null)
			{
				bootBox.ClaimOwnershipOfItems();
			}
			NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 0);
			this.syncComponent.SyncEnabled = true;
			this.dynamics.enabled = true;
			if (this.driveTrain.rpm > 100f)
			{
				this.driveTrain.canStall = true;
			}
			return false;
		}

		[CompilerGenerated]
		private bool <PlayerEventHooks>b__143_1()
		{
			if (this.CurrentDrivingState == PlayerVehicle.DrivingStates.Driver && this.DriverIsLocal)
			{
				this.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
				this.DriverIsLocal = false;
				if (this.driveTrain.rpm > 100f)
				{
					this.driveTrain.canStall = false;
				}
				NetLocalPlayer.Instance.LeaveVehicle();
			}
			return false;
		}

		[CompilerGenerated]
		private bool <PlayerEventHooks>b__143_2()
		{
			BootBox bootBox = this.bootBox;
			if (bootBox != null)
			{
				bootBox.UnclaimOwnership();
			}
			return false;
		}

		public ObjectSyncComponent syncComponent;

		private bool isSyncing;

		private GameObject gameObject;

		public Rigidbody rigidbody;

		private Transform steeringPivot;

		public Transform steeringIK;

		public Transform gearstickIK;

		public GameObject ParentGameObject;

		public Transform SeatTransform;

		public Transform PassengerSeatTransform;

		public Transform RearSeatTransform;

		public bool DriverIsLocal;

		public PlayerVehicle.DrivingStates CurrentDrivingState = PlayerVehicle.DrivingStates.None;

		private CarDynamics dynamics;

		private WheelCollider[] wheelCollidersCache;

		private bool wheelCollidersSearched;

		public Drivetrain driveTrain;

		private PlayMakerFSM playerTrigger;

		public PlayMakerFSM starterFsm;

		private PlayMakerFSM kickstarterFsm;

		public PlayMakerFSM ignitionFsm;

		private PlayMakerFSM handbrakeFsm;

		private PlayMakerFSM fuelTankFsm;

		private PlayMakerFSM rangeFsm;

		private PlayMakerFSM gearIndicatorFsm;

		private PlayMakerFSM dashboardFsm;

		private PlayMakerFSM fuelTapFsm;

		private PlayMakerFSM lightsFsm;

		private PlayMakerFSM wipersFsm;

		private PlayMakerFSM interiorLightFsm;

		private PlayMakerFSM frontHydraulicFsm;

		private PlayMakerFSM frontHydraulicFsm2;

		private PlayMakerFSM indicatorsFsm;

		public PlayMakerFSM hazardsFsm;

		private PlayMakerFSM radioFsm;

		private PlayMakerFSM radioVFsm;

		private PlayMakerFSM sideFsm;

		private PlayMakerFSM kickstandFsm;

		private PlayMakerFSM frontHookFsm;

		private PlayMakerFSM rearHookFsm;

		private PlayMakerFSM fuelCapFsm;

		private PlayMakerFSM fuelHatchFsm;

		private PlayMakerFSM hose;

		private PlayMakerFSM tipping;

		private PlayMakerFSM horn;

		private PlayMakerFSM tractorThrottle;

		private PlayMakerFSM LOD;

		private PlayMakerFSM gloveboxFsm;

		private PlayMakerFSM bootFsm;

		private PlayMakerFSM chokeFsm;

		private PlayMakerFSM chokeFsm2;

		private PlayMakerFSM hoodlockFsm;

		private PlayMakerFSM bootlidFsm;

		private PlayMakerFSM doorlFsm;

		private PlayMakerFSM doorrFsm;

		public PlayMakerFSM fuelCapTriggerFSM;

		private PlayMakerFSM electricityFsm;

		private PlayMakerFSM hydraulicPumpFsm;

		private PlayMakerFSM diffLockFsm;

		private PlayMakerFSM axleLiftFsm;

		private PlayMakerFSM spillValveFsm;

		private PlayMakerFSM beaconFsm;

		private PlayMakerFSM workLightsFsm;

		private PlayMakerFSM hoseFsm;

		private PlayMakerFSM hoseCFsm;

		private PlayMakerFSM sunflapFsm;

		private PlayMakerFSM gifuStall;

		public PlayMakerFSM seatbeltFsm;

		private PlayMakerFSM waspNestFsm;

		public bool hasRange;

		private bool hasLeverParkingBrake;

		private bool hasPushParkingBrake;

		private bool isTruck;

		private bool isTractor;

		private bool hydraulicPumpFirstRun = true;

		private bool axleLiftFirstRun = true;

		private bool diffLockFirstRun = true;

		private AxisCarController axisCarController;

		private PlayerVehicle.MPCarController mpCarController;

		private float steamID = SteamUser.GetSteamID().m_SteamID;

		private bool wasDisabled;

		public BootBox bootBox;

		private PlayMakerFSM trailerRemoveFSM;

		public PlayMakerFSM LogTriggerFSM;

		public bool isLogRelated;

		private bool isHolding;

		private bool NeedToSendSync;

		private PlayMakerFSM satsumaRadioV;

		private PlayMakerFSM satsumaRadioT;

		public PlayMakerFSM driverHeadTrigger;

		public PlayMakerFSM headForceFSM;

		private PlayMakerFSM knobLeft;

		private PlayMakerFSM knobRight;

		private PlayMakerFSM seatFsm;

		private string LastStarterFsm;

		private bool isReceiver;

		public enum DrivingStates
		{
			Driver,
			Passenger,
			Backseater,
			None
		}

		public enum EngineStates
		{
			WaitForStart,
			ACC,
			Glowplug,
			TurnKey,
			CheckClutch,
			StartingEngine,
			StartEngine,
			StartOrNot,
			MotorRunning,
			Wait,
			BrokenStarter,
			State1,
			Null
		}

		public enum DashboardStates
		{
			ACCon,
			Test,
			ACCon2,
			MotorStarting,
			ShutOff,
			MotorOff,
			WaitButton,
			WaitPlayer,
			Null
		}

		public enum SwitchIDs
		{
			FrontHyd,
			FrontHyd2,
			hasPushParkingBrake,
			HandbrakeLever,
			Lights,
			Wipers,
			HydraulicPump,
			DiffLock,
			AxleLift,
			InteriorLight,
			SpillValve,
			FuelTap,
			TractorHydraulics,
			TractorThrottle,
			DestroyWaspNest,
			FlatbedHatch,
			FuelCap,
			FuelHatch,
			Starter,
			Steer,
			GifuStall,
			DoorL,
			DoorR,
			RadioV,
			RadioT,
			RadioV2,
			RadioT2,
			Choke,
			Sunflap
		}

		public class MPCarController : AxisCarController
		{
			protected override void GetInput(out float throttleInput, out float brakeInput, out float steerInput, out float handbrakeInput, out float clutchInput, out bool startEngineInput, out int targetGear)
			{
				throttleInput = this.remoteThrottleInput;
				brakeInput = this.remoteBrakeInput;
				steerInput = this.remoteSteerInput;
				handbrakeInput = this.remoteHandbrakeInput;
				clutchInput = this.remoteClutchInput;
				startEngineInput = this.remoteStartEngineInput;
				targetGear = this.remoteTargetGear;
			}

			public MPCarController()
			{
			}

			public ObjectSyncComponent osc;

			public float remoteThrottleInput;

			public float remoteBrakeInput;

			public float remoteSteerInput;

			public float remoteHandbrakeInput;

			public float remoteClutchInput;

			public bool remoteStartEngineInput;

			public int remoteTargetGear;
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

			internal bool <FindFSMs>b__122_14()
			{
				return false;
			}

			internal bool <FindFSMs>b__122_15()
			{
				return false;
			}

			internal bool <FindFSMs>b__122_16()
			{
				return false;
			}

			internal bool <HookEvents>b__125_3()
			{
				return false;
			}

			public static readonly PlayerVehicle.<>c <>9 = new PlayerVehicle.<>c();

			public static Func<bool> <>9__122_14;

			public static Func<bool> <>9__122_15;

			public static Func<bool> <>9__122_16;

			public static Func<bool> <>9__125_3;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass122_0
		{
			public <>c__DisplayClass122_0()
			{
			}

			internal bool <FindFSMs>b__6()
			{
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Sunflap, false, this.fsm.Fsm.GetFsmFloat("Angle").Value, "");
				return false;
			}

			internal bool <FindFSMs>b__7()
			{
				if (this.<>4__this.isHolding)
				{
					this.<>4__this.isHolding = false;
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				}
				return false;
			}

			internal bool <FindFSMs>b__8()
			{
				if (this.<>4__this.isHolding)
				{
					this.<>4__this.isHolding = false;
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				}
				return false;
			}

			internal bool <FindFSMs>b__9()
			{
				this.<>4__this.isHolding = true;
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				return false;
			}

			internal bool <FindFSMs>b__10()
			{
				this.<>4__this.isHolding = true;
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				return false;
			}

			internal bool <FindFSMs>b__11()
			{
				this.<>4__this.isHolding = true;
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				return false;
			}

			internal bool <FindFSMs>b__12()
			{
				this.<>4__this.isHolding = true;
				NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				return false;
			}

			internal bool <FindFSMs>b__13()
			{
				if (this.<>4__this.isHolding)
				{
					this.<>4__this.isHolding = false;
					NetLocalPlayer.Instance.WriteVehicleSwitchMessage(this.<>4__this.syncComponent, PlayerVehicle.SwitchIDs.Choke, false, -1f, string.Format("{0}K{1}", this.fsm.Fsm.GetFsmFloat("LeverPos").Value, this.fsm.Fsm.GetFsmFloat("Throttle").Value));
				}
				return false;
			}

			public PlayMakerFSM fsm;

			public PlayerVehicle <>4__this;
		}
	}
}
