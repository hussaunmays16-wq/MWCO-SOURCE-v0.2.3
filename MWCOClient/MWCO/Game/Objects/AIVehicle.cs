using System;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class AIVehicle : ISyncedObject
	{
		public float Steering
		{
			get
			{
				float num;
				try
				{
					num = this.dynamics.carController.steering;
				}
				catch
				{
					Chat.Instance.AddMessage("STEERING ERROR" + this.parentGameObject.name, MessageSeverity.Info);
					num = 0f;
				}
				return num;
			}
			set
			{
				this.dynamics.carController.steering = value;
			}
		}

		public float Throttle
		{
			get
			{
				float num;
				try
				{
					num = this.dynamics.carController.throttleInput;
				}
				catch
				{
					Chat.Instance.AddMessage("THROTTLE ERROR" + this.parentGameObject.name, MessageSeverity.Info);
					num = 0f;
				}
				return num;
			}
			set
			{
				this.dynamics.carController.throttleInput = value;
			}
		}

		public float Brake
		{
			get
			{
				float num;
				try
				{
					num = this.dynamics.carController.brakeInput;
				}
				catch
				{
					Chat.Instance.AddMessage("BRAKE ERROR" + this.parentGameObject.name, MessageSeverity.Info);
					num = 0f;
				}
				return num;
			}
			set
			{
				this.dynamics.carController.brakeInput = value;
			}
		}

		public float TargetSpeed
		{
			get
			{
				float num;
				try
				{
					num = this.throttleFsm.FsmVariables.GetFsmFloat("TargetSpeed").Value;
				}
				catch
				{
					Chat.Instance.AddMessage("TARGERSPEED ERROR" + this.parentGameObject.name, MessageSeverity.Info);
					num = 0f;
				}
				return num;
			}
			set
			{
				this.remoteTargetSpeed = value;
			}
		}

		public int Waypoint
		{
			get
			{
				return Convert.ToInt32(this.navigationFsm.FsmVariables.GetFsmGameObject("Waypoint").Value.name);
			}
		}

		public GameObject WaypointSet
		{
			set
			{
				this.navigationFsm.FsmVariables.GetFsmGameObject("Waypoint").Value = value;
			}
		}

		public int Route
		{
			get
			{
				string name = this.navigationFsm.FsmVariables.GetFsmGameObject("Waypoint").Value.transform.parent.name;
				if (name == "BusRoute")
				{
					return 0;
				}
				if (name == "DirtRoad")
				{
					return 1;
				}
				if (name == "Highway")
				{
					return 2;
				}
				if (name == "HomeRoad")
				{
					return 3;
				}
				if (name == "RoadRace")
				{
					return 4;
				}
				if (name == "Trackfield")
				{
					return 5;
				}
				return 6;
			}
		}

		public int WaypointStart
		{
			get
			{
				return this.navigationFsm.FsmVariables.GetFsmInt("WaypointStart").Value;
			}
			set
			{
				this.navigationFsm.FsmVariables.GetFsmInt("WaypointStart").Value = value;
			}
		}

		public int WaypointEnd
		{
			get
			{
				return this.navigationFsm.FsmVariables.GetFsmInt("WaypointEnd").Value;
			}
			set
			{
				this.navigationFsm.FsmVariables.GetFsmInt("WaypointEnd").Value = value;
			}
		}

		public AIVehicle(GameObject go, ObjectSyncComponent osc)
		{
			try
			{
				this.gameObject = go;
				this.syncComponent = osc;
				this.parentGameObject = go;
				string name = go.name;
				if (name == "AMIS2" || name == "KYLAJANI")
				{
					this.type = AIVehicle.VehicleTypes.Amis;
				}
				else if (name == "BUS")
				{
					this.type = AIVehicle.VehicleTypes.Bus;
				}
				else if (name == "FITTAN" && this.parentGameObject.transform.FindChild("Navigation") != null)
				{
					this.type = AIVehicle.VehicleTypes.Fitan;
				}
				else if (this.parentGameObject.transform.FindChild("NavigationCW") != null || this.parentGameObject.transform.FindChild("NavigationCCW") != null)
				{
					this.type = AIVehicle.VehicleTypes.TrafficDirectional;
				}
				else
				{
					this.type = AIVehicle.VehicleTypes.Traffic;
				}
				this.rigidbody = this.parentGameObject.GetComponent<Rigidbody>();
				this.dynamics = this.parentGameObject.GetComponent<CarDynamics>();
				this.throttleFsm = Utils.GetPlaymakerScriptByName(this.parentGameObject, "Throttle");
				if (this.type == AIVehicle.VehicleTypes.TrafficDirectional)
				{
					this.directionFsm = Utils.GetPlaymakerScriptByName(this.parentGameObject, "Direction");
				}
				else
				{
					try
					{
						this.navigationFsm = Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("Navigation").gameObject, "Navigation");
					}
					catch (Exception)
					{
					}
				}
				this.FindFSMs();
				this.EventHooks();
			}
			catch (Exception ex)
			{
				Logger.Debug("AIVehicle Error " + ex.Message);
			}
		}

		public void FindFSMs()
		{
			foreach (PlayMakerFSM playMakerFSM in this.parentGameObject.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.gameObject.name == "NPC_CARS" && playMakerFSM.FsmName == "Bus Setup")
				{
					this.setupAll(playMakerFSM);
					PlayMakerFSM playMakerFSM2 = playMakerFSM;
					Logger.Debug(((playMakerFSM2 != null) ? playMakerFSM2.ToString() : null) + " [SETUP FSM] ");
				}
				else if (playMakerFSM.gameObject.name == "NPC_CARS" && playMakerFSM.FsmName == "Kuski Setup")
				{
					this.setupAll(playMakerFSM);
					PlayMakerFSM playMakerFSM3 = playMakerFSM;
					Logger.Debug(((playMakerFSM3 != null) ? playMakerFSM3.ToString() : null) + " [SETUP FSM] ");
				}
				else if (playMakerFSM.gameObject.name == "NPC_CARS" && playMakerFSM.FsmName == "Amis Setup")
				{
					this.setupAll(playMakerFSM);
					PlayMakerFSM playMakerFSM4 = playMakerFSM;
					Logger.Debug(((playMakerFSM4 != null) ? playMakerFSM4.ToString() : null) + " [SETUP FSM] ");
				}
				else if (playMakerFSM.gameObject.name == "NPC_CARS" && playMakerFSM.FsmName == "Amis2 Setup")
				{
					this.setupAll(playMakerFSM);
					PlayMakerFSM playMakerFSM5 = playMakerFSM;
					Logger.Debug(((playMakerFSM5 != null) ? playMakerFSM5.ToString() : null) + " [SETUP FSM] ");
				}
				else if (playMakerFSM.gameObject.name == "NPC_CARS" && playMakerFSM.FsmName == "GreenMenace")
				{
					this.setupAll(playMakerFSM);
					PlayMakerFSM playMakerFSM6 = playMakerFSM;
					Logger.Debug(((playMakerFSM6 != null) ? playMakerFSM6.ToString() : null) + " [SETUP FSM] ");
				}
				else if (playMakerFSM.FsmName == "Navigation" && playMakerFSM.gameObject.name == "NavigationCW")
				{
					this.navigationCWgo = playMakerFSM.gameObject;
					this.isClockwise = 1f;
					this.navigationFsm = playMakerFSM;
				}
				else if (playMakerFSM.FsmName == "Navigation" && playMakerFSM.gameObject.name == "NavigationCCW")
				{
					this.navigationCCWgo = playMakerFSM.gameObject;
					this.isClockwise = 0f;
					this.navigationFsm = playMakerFSM;
				}
			}
		}

		public void setupAll(PlayMakerFSM fsm)
		{
		}

		public Transform ObjectTransform()
		{
			return this.parentGameObject.transform;
		}

		public bool PeriodicSyncEnabled()
		{
			return NetManager.Instance.IsHost;
		}

		public bool CanSync()
		{
			bool flag;
			try
			{
				if (!NetManager.Instance.IsHost)
				{
					flag = false;
				}
				else if (this.gameObject.name == "BUS")
				{
					this.isSyncing = true;
					flag = true;
				}
				else if (this.rigidbody.velocity.sqrMagnitude >= 0.01f)
				{
					this.isSyncing = true;
					flag = true;
				}
				else
				{
					this.isSyncing = false;
					flag = false;
				}
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public bool ShouldTakeOwnership()
		{
			return NetManager.Instance.IsHost;
		}

		public float[] ReturnSyncedVariables(bool sendAllVariables)
		{
			if (this.isSyncing)
			{
				return new float[] { this.Steering, this.Throttle, this.Brake, this.TargetSpeed, this.isClockwise };
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
					this.TargetSpeed = variables[3];
					if (this.type == AIVehicle.VehicleTypes.TrafficDirectional && this.isClockwise != variables[4])
					{
						this.isClockwise = variables[4];
						if (this.isClockwise == 1f)
						{
							this.navigationCWgo.SetActive(true);
							this.navigationCCWgo.SetActive(false);
							this.navigationFsm = Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("NavigationCW").gameObject, "Navigation");
						}
						else
						{
							this.navigationCWgo.SetActive(false);
							this.navigationCCWgo.SetActive(true);
							this.navigationFsm = Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("NavigationCCW").gameObject, "Navigation");
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void SyncTakenByForce()
		{
		}

		public void OwnerSetToRemote()
		{
			this.gameObject.SetActive(true);
		}

		public void OwnerRemoved()
		{
		}

		public void ConstantSyncChanged(bool newValue)
		{
		}

		public void EventHooks()
		{
			EventHook.AddWithSync(this.directionFsm, "CW", delegate
			{
				this.isClockwise = 1f;
				return false;
			}, false);
			EventHook.AddWithSync(this.directionFsm, "CCW", delegate
			{
				this.isClockwise = 0f;
				return false;
			}, false);
			if (this.type == AIVehicle.VehicleTypes.Bus)
			{
				foreach (PlayMakerFSM playMakerFSM in this.parentGameObject.GetComponentsInChildren<PlayMakerFSM>(true))
				{
					if (playMakerFSM.FsmName == "Activate" && playMakerFSM.gameObject.name.StartsWith("StopButton"))
					{
						EventHook.AddWithSync(playMakerFSM, "Wait player", null, false);
						EventHook.AddWithSync(playMakerFSM, "Activate", null, false);
					}
					else if ((!(playMakerFSM.FsmName == "Start") || !(playMakerFSM.gameObject.name == "Route")) && (!(playMakerFSM.FsmName == "Door") || !(playMakerFSM.gameObject.name == "Route")))
					{
						if (playMakerFSM.FsmName == "Button" && playMakerFSM.gameObject.name == "Ticket")
						{
							EventHook.AddWithSync(playMakerFSM, "Wait", null, false);
							EventHook.AddWithSync(playMakerFSM, "Pay trip", null, false);
						}
						else if (playMakerFSM.FsmName == "PlayerTrigger" && playMakerFSM.gameObject.name == "DriveTrigger")
						{
							EventHook.Add(playMakerFSM, "Player in car", delegate
							{
								NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 1);
								return false;
							}, false, false);
							EventHook.Add(playMakerFSM, "Wait for player", delegate
							{
								NetLocalPlayer.Instance.LeaveVehicle();
								return false;
							}, false, false);
						}
					}
				}
				Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("Navigation").gameObject, "Navigation");
				this.busTrigger = Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("LOD").FindChild("PlayerTrigger").GetChild(0)
					.gameObject, "PlayerTrigger");
				this.busDriver = Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("LOD").FindChild("Passengers").gameObject, "Activate");
				EventHook.AddWithSync(Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("LOD").FindChild("PlayerTrigger").gameObject, "PlayerTrigger"), "Press return", null, false);
			}
			if (this.type == AIVehicle.VehicleTypes.Amis || this.type == AIVehicle.VehicleTypes.Fitan)
			{
				PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("CrashEvent").gameObject, "Crash");
				Utils.GetPlaymakerScriptByName(this.parentGameObject.transform.FindChild("Navigation").gameObject, "Navigation");
				EventHook.SyncAllEvents(playmakerScriptByName, () => (this.syncComponent.Owner != this.steamID && this.syncComponent.Owner != 0UL) || (this.syncComponent.Owner == 0UL && !NetManager.Instance.IsHost));
			}
			if (NetManager.Instance.IsOnline && !NetManager.Instance.IsHost)
			{
				this.syncComponent.RequestObjectSync();
			}
		}

		[CompilerGenerated]
		private bool <EventHooks>b__57_0()
		{
			this.isClockwise = 1f;
			return false;
		}

		[CompilerGenerated]
		private bool <EventHooks>b__57_1()
		{
			this.isClockwise = 0f;
			return false;
		}

		[CompilerGenerated]
		private bool <EventHooks>b__57_2()
		{
			NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 1);
			return false;
		}

		[CompilerGenerated]
		private bool <EventHooks>b__57_4()
		{
			return (this.syncComponent.Owner != this.steamID && this.syncComponent.Owner != 0UL) || (this.syncComponent.Owner == 0UL && !NetManager.Instance.IsHost);
		}

		private ObjectSyncComponent syncComponent;

		private bool isSyncing;

		private GameObject gameObject;

		private Rigidbody rigidbody;

		private GameObject parentGameObject;

		private PlayMakerFSM throttleFsm;

		private PlayMakerFSM navigationFsm;

		private GameObject navigationCWgo;

		private GameObject navigationCCWgo;

		private PlayMakerFSM directionFsm;

		private PlayMakerFSM ticketFsm;

		private PlayMakerFSM busTrigger;

		private PlayMakerFSM busDriver;

		private CarDynamics dynamics;

		private float isClockwise;

		public AIVehicle.VehicleTypes type;

		private float remoteTargetSpeed;

		private float steamID = SteamUser.GetSteamID().m_SteamID;

		public enum VehicleTypes
		{
			Bus,
			Amis,
			Traffic,
			TrafficDirectional,
			Fitan
		}

		private class MPCarController : AxisCarController
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

			internal bool <EventHooks>b__57_3()
			{
				NetLocalPlayer.Instance.LeaveVehicle();
				return false;
			}

			public static readonly AIVehicle.<>c <>9 = new AIVehicle.<>c();

			public static Func<bool> <>9__57_3;
		}
	}
}
