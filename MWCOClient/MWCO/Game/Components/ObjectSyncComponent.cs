using System;
using MWCO.Game.Objects;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Math;
using MWCO.Network;
using MWCO.Network.Messages;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Game.Components
{
	public class ObjectSyncComponent : MonoBehaviour
	{
		public ObjectSyncComponent Setup(ObjectSyncManager.ObjectTypes type, int objectID)
		{
			if (base.gameObject.IsPrefab())
			{
				Object.Destroy(this);
				return null;
			}
			if (!NetWorld.Instance.playerIsLoading && !NetManager.Instance.IsHost && objectID == ObjectSyncManager.AUTOMATIC_ID && type != ObjectSyncManager.ObjectTypes.PlayerVehicle && type != ObjectSyncManager.ObjectTypes.Mod && !base.name.StartsWith("BottleBeerFly") && !base.name.StartsWith("empty cup") && !base.name.StartsWith("empty bottle"))
			{
				Logger.Debug("Ignoring spawned object as client is not host!");
				Object.Destroy(base.gameObject);
				return null;
			}
			this.ObjectType = type;
			this.ObjectID = objectID;
			this.Name = base.transform.name;
			this.ObjectID = ObjectSyncManager.Instance.AddNewObject(this, this.ObjectID);
			if ((!NetWorld.Instance.playerIsLoading && !this.isSetup) || this.ObjectType == ObjectSyncManager.ObjectTypes.Satsuma || base.gameObject.name == "ServiceBrochure")
			{
				this.CreateObjectSubtype();
			}
			return this;
		}

		private void Start()
		{
			if (NetWorld.Instance.playerIsLoading && !this.isSetup)
			{
				this.CreateObjectSubtype();
			}
		}

		public void CreateObjectSubtype()
		{
			switch (this.ObjectType)
			{
			case ObjectSyncManager.ObjectTypes.Pickupable:
				this.syncedObject = base.gameObject.AddComponent<Pickupable>();
				break;
			case ObjectSyncManager.ObjectTypes.PlayerVehicle:
				this.syncedObject = new PlayerVehicle(base.gameObject, this);
				break;
			case ObjectSyncManager.ObjectTypes.AIVehicle:
				this.isAI = true;
				this.syncedObject = new AIVehicle(base.gameObject, this);
				break;
			case ObjectSyncManager.ObjectTypes.GarageDoor:
				this.syncedObject = new GarageDoor(base.gameObject);
				break;
			case ObjectSyncManager.ObjectTypes.CarDoor:
				this.syncedObject = new CarDoor(base.gameObject);
				break;
			case ObjectSyncManager.ObjectTypes.CarDoorF:
				this.syncedObject = new CarDoorF(base.gameObject);
				break;
			case ObjectSyncManager.ObjectTypes.Boot:
				this.syncedObject = new Boot(base.gameObject);
				break;
			case ObjectSyncManager.ObjectTypes.PlayerHouse:
				this.syncedObject = new PlayerHouse(base.gameObject, this);
				break;
			case ObjectSyncManager.ObjectTypes.Train:
				this.syncedObject = new Train(base.gameObject, this);
				break;
			case ObjectSyncManager.ObjectTypes.Transform:
				this.syncedObject = new TransformSync(base.gameObject);
				break;
			}
			this.isSetup = true;
		}

		private void Update()
		{
			this.InterpolatePos();
			this.ApplyReceivedWheelRpms();
			if (this.Name != base.transform.name)
			{
				this.Name = base.transform.name;
				ObjectSyncManager.Instance.ObjectIDs[this.ObjectID] = this;
			}
			if ((base.gameObject.name.Contains("Hose") || this.ObjectType == ObjectSyncManager.ObjectTypes.Pickupable || this.ObjectType == ObjectSyncManager.ObjectTypes.AIVehicle) && this.syncedObject == null)
			{
				this.CreateObjectSubtype();
			}
			if (this.renderer == null && this.hasRenderer && this.ObjectType == ObjectSyncManager.ObjectTypes.Pickupable && this.sendConstantSync)
			{
				GamePlayer.Instance.DropStolenObject();
			}
			if (this.isAI && !NetManager.Instance.IsHost)
			{
				this.aiTimeSinceLast -= Time.deltaTime;
				if (this.aiTimeSinceLast <= 0f)
				{
					this.DeactivateAIVehicle();
				}
			}
			if (DevTools.lateLoad && this.myLateLoad && (this.ObjectType == ObjectSyncManager.ObjectTypes.Pickupable || this.ObjectType == ObjectSyncManager.ObjectTypes.Satsuma))
			{
				this.myLateLoad = false;
				this.CreateObjectSubtype();
				Logger.Debug("Now doing late setup for '" + base.gameObject.name + "'");
				return;
			}
			if (this.Owner == 0UL && NetManager.Instance.players.Count > 0)
			{
				if (!NetManager.Instance.IsHost)
				{
					this.Owner = (ulong)NetManager.Instance.players[0].SteamId;
				}
				else
				{
					this.Owner = SteamUser.GetSteamID().m_SteamID;
				}
			}
			if (this.syncedObject != null && !this.initial)
			{
				this.initial = true;
				if (this.ObjectType == ObjectSyncManager.ObjectTypes.Pickupable && this.renderer == null)
				{
					this.renderer = base.gameObject.GetComponent<Renderer>();
					if (this.renderer != null)
					{
						this.hasRenderer = true;
					}
				}
				this.interpolator.CurrentPosition = this.syncedObject.ObjectTransform().position;
				this.interpolator.CurrentRotation = this.syncedObject.ObjectTransform().rotation;
			}
			this.timeToSendSync -= Time.deltaTime;
			if (this.timeToSendSync <= 0f)
			{
				if (this.isSetup && this.initial)
				{
					if (this.SyncEnabled)
					{
						if (this.sendConstantSync)
						{
							this.SendObjectSync(ObjectSyncManager.SyncTypes.GenericSync, true, false);
						}
						else if (this.syncedObject.CanSync())
						{
							this.SendObjectSync(ObjectSyncManager.SyncTypes.GenericSync, true, false);
							if (this.ObjectType == ObjectSyncManager.ObjectTypes.AIVehicle || this.ObjectType == ObjectSyncManager.ObjectTypes.Train)
							{
								if (NetManager.Instance.IsHost && !this.IsCloseToPlayer(300f, true))
								{
									this.SyncEnabled = false;
								}
							}
							else if (this.Name.StartsWith("haybale"))
							{
								if (!this.IsCloseToPlayer(20f, true))
								{
									this.SyncEnabled = false;
								}
							}
							else if (this.ObjectType == ObjectSyncManager.ObjectTypes.Transform && NetManager.Instance.IsHost && !this.IsCloseToPlayer(50f, true))
							{
								this.SyncEnabled = false;
							}
						}
					}
					else if (this.ObjectType == ObjectSyncManager.ObjectTypes.AIVehicle || this.ObjectType == ObjectSyncManager.ObjectTypes.Train)
					{
						if (NetManager.Instance.IsHost && this.IsCloseToPlayer(300f, true))
						{
							this.SyncEnabled = true;
						}
					}
					else if (this.Name.StartsWith("haybale"))
					{
						if (this.IsCloseToPlayer(20f, false))
						{
							this.SyncEnabled = true;
						}
					}
					else if (this.ObjectType == ObjectSyncManager.ObjectTypes.Transform)
					{
						if (NetManager.Instance.IsHost && this.IsCloseToPlayer(50f, true))
						{
							this.SyncEnabled = true;
						}
					}
					else if (this.syncedObject.PeriodicSyncEnabled() && ObjectSyncManager.Instance.ShouldPeriodicSync(this.Owner, this.SyncEnabled))
					{
						this.SendObjectSync(ObjectSyncManager.SyncTypes.PeriodicSync, true, false);
					}
				}
				this.timeToSendSync = this.SYNC_INTERVAL;
			}
		}

		private bool IsCloseToPlayer(float distance, bool others = true)
		{
			if (DevTools.localPlayer == null)
			{
				return false;
			}
			if (Vector3.Distance(DevTools.localPlayer.transform.position, this.syncedObject.ObjectTransform().position) < distance)
			{
				return true;
			}
			if (others)
			{
				foreach (NetPlayer netPlayer in NetManager.Instance.players)
				{
					if (netPlayer != null && Vector3.Distance(netPlayer.characterGameObject.transform.position, this.syncedObject.ObjectTransform().position) < distance / 2f)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		private void CarStallCheck()
		{
			if (this.timeToCheck <= 0f)
			{
				try
				{
					if (this.pv.driveTrain.canStall && this.pv.starterFsm.Fsm.GetFsmFloat("Revs").Value > 100f)
					{
						this.pv.driveTrain.canStall = false;
					}
					else if (this.pv.driveTrain.canStall && this.pv.starterFsm.Fsm.GetFsmFloat("Revs").Value <= 10f)
					{
						this.pv.driveTrain.canStall = true;
					}
				}
				catch (Exception ex)
				{
					Debug.Log(this.pv.ParentGameObject.name + " " + ex.ToString());
				}
				this.timeToCheck = 2f;
				return;
			}
			this.timeToCheck -= Time.deltaTime;
		}

		public void SendObjectSync(ObjectSyncManager.SyncTypes type, bool sendVariables, bool syncWasRequested)
		{
			if (this.ObjectType == ObjectSyncManager.ObjectTypes.Transform)
			{
				NetLocalPlayer.Instance.SendObjectSync(this.ObjectID, base.transform.position, base.transform.rotation, type, null);
				return;
			}
			Vector3Message bodyVelocity = null;
			Vector3Message bodyAngularVelocity = null;
			float[] wheelRpms = null;
			try
			{
				Rigidbody syncedRigidbody = this.GetSyncedRigidbody();
				if (syncedRigidbody != null && !syncedRigidbody.isKinematic)
				{
					bodyVelocity = Utils.GameVec3ToNet(syncedRigidbody.velocity);
					bodyAngularVelocity = Utils.GameVec3ToNet(syncedRigidbody.angularVelocity);
				}
				PlayerVehicle playerVehicle = this.syncedObject as PlayerVehicle;
				if (playerVehicle != null && playerVehicle.DriverIsLocal)
				{
					wheelRpms = playerVehicle.GetWheelRpms();
				}
			}
			catch (Exception ex)
			{
				Logger.Debug("SendObjectSync body-state gather failed: " + ex);
			}
			if (sendVariables)
			{
				NetLocalPlayer.Instance.SendObjectSync(this.ObjectID, this.syncedObject.ObjectTransform().position, this.syncedObject.ObjectTransform().rotation, type, this.syncedObject.ReturnSyncedVariables(true), bodyVelocity, bodyAngularVelocity, wheelRpms);
				return;
			}
			NetLocalPlayer.Instance.SendObjectSync(this.ObjectID, this.syncedObject.ObjectTransform().position, this.syncedObject.ObjectTransform().rotation, type, null, bodyVelocity, bodyAngularVelocity, wheelRpms);
		}

		public void RequestObjectSync()
		{
			NetLocalPlayer.Instance.RequestObjectSync(this.ObjectID);
		}

		public void SyncRequestAccepted()
		{
			this.Owner = SteamUser.GetSteamID().m_SteamID;
			Logger.Info("Sync request accepted, object: " + base.gameObject.name);
			if (this.ObjectType != ObjectSyncManager.ObjectTypes.PlayerVehicle)
			{
				this.SyncEnabled = true;
			}
		}

		public void SendEnterSync()
		{
			try
			{
				if (this.ObjectType != ObjectSyncManager.ObjectTypes.Transform)
				{
					if (this.Owner == ObjectSyncManager.NO_OWNER && this.syncedObject.ShouldTakeOwnership())
					{
						this.SendObjectSync(ObjectSyncManager.SyncTypes.SetOwner, true, false);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void SendExitSync()
		{
			if (this.Owner == ObjectSyncManager.Instance.steamID.m_SteamID && this.ObjectType != ObjectSyncManager.ObjectTypes.AIVehicle)
			{
				this.Owner = ObjectSyncManager.NO_OWNER;
				this.SyncEnabled = false;
				this.SendObjectSync(ObjectSyncManager.SyncTypes.RemoveOwner, false, false);
			}
		}

		public void TakeSyncControl()
		{
			if (this.Owner != SteamUser.GetSteamID().m_SteamID && this.ObjectType != ObjectSyncManager.ObjectTypes.AIVehicle)
			{
				this.SendObjectSync(ObjectSyncManager.SyncTypes.ForceSetOwner, true, false);
			}
		}

		public void OwnerSetToRemote(ulong newOwner)
		{
			this.Owner = newOwner;
			if (this.syncedObject != null)
			{
				this.syncedObject.OwnerSetToRemote();
			}
		}

		public void OwnerRemoved()
		{
			this.Owner = ObjectSyncManager.NO_OWNER;
			if (this.syncedObject != null)
			{
				this.syncedObject.OwnerRemoved();
			}
		}

		public void SyncTakenByForce()
		{
			if (this.syncedObject != null)
			{
				this.syncedObject.SyncTakenByForce();
			}
		}

		public void SendConstantSync(bool newValue, bool changed = true)
		{
			this.sendConstantSync = newValue;
			if (changed)
			{
				ISyncedObject syncedObject = this.syncedObject;
				if (syncedObject != null)
				{
					syncedObject.ConstantSyncChanged(newValue);
				}
			}
			this.SyncEnabled = true;
		}

		public void HandleSyncedVariables(float[] syncedVariables, CSteamID sender)
		{
			ISyncedObject syncedObject = this.syncedObject;
			if (syncedObject == null)
			{
				return;
			}
			syncedObject.HandleSyncedVariables(syncedVariables, sender);
		}

		public bool IsOwnerSelf()
		{
			return this.Owner == SteamUser.GetSteamID().m_SteamID;
		}

		public void SetPositionAndRotation(Vector3 pos, Quaternion rot)
		{
			if (!NetManager.Instance.IsHost && this.ObjectType == ObjectSyncManager.ObjectTypes.Transform && !this.npcRaycastDisabled)
			{
				foreach (PlayMakerFSM playMakerFSM in base.GetComponents<PlayMakerFSM>())
				{
					if (playMakerFSM.FsmName == "Raycast")
					{
						playMakerFSM.enabled = false;
						this.npcRaycastDisabled = true;
					}
				}
			}
			if (this.syncedObject == null)
			{
				Logger.Debug("Tried to set position of object '" + base.gameObject.name + "' but object isn't setup. (This is usually fine)");
				this.CreateObjectSubtype();
				Logger.Debug("Now doing late setup for '" + base.gameObject.name + "'");
				return;
			}
			if (this.syncedObject != null)
			{
				if (this.ObjectType == ObjectSyncManager.ObjectTypes.AIVehicle)
				{
					this.aiTimeSinceLast = 1f;
					this.isAI = true;
				}
				if (this.ShouldInterpolate(pos))
				{
					this.syncReceiveTime = NetManager.Instance.GetNetworkClock();
					this.targetPos = pos;
					this.targetRot = rot;
					this.interpolator.SetTarget(pos, rot);
					return;
				}
				if (this.ShouldSyncPosition())
				{
					this.syncedObject.ObjectTransform().position = pos;
				}
				this.syncedObject.ObjectTransform().rotation = rot;
			}
		}

		public void ActivateAIVehicle()
		{
			if (!base.gameObject.GetTopmostParent().activeSelf)
			{
				base.gameObject.GetTopmostParent().SetActive(true);
			}
			base.gameObject.SetActive(true);
		}

		private void DeactivateAIVehicle()
		{
			base.gameObject.SetActive(false);
		}

		public void InterpolatePos()
		{
			if (this.syncedObject != null && this.syncReceiveTime > 0UL)
			{
				float num = (NetManager.Instance.GetNetworkClock() - this.syncReceiveTime) / 50f;
				if (num <= 1f)
				{
					Vector3 position = this.syncedObject.ObjectTransform().position;
					Quaternion rotation = this.syncedObject.ObjectTransform().rotation;
					this.interpolator.Evaluate(ref position, ref rotation, num);
					if (this.ShouldSyncPosition())
					{
						this.syncedObject.ObjectTransform().position = this.interpolator.CurrentPosition;
					}
					this.syncedObject.ObjectTransform().rotation = this.interpolator.CurrentRotation;
					return;
				}
				if (this.syncedObject.ObjectTransform().position == this.targetPos && this.syncedObject.ObjectTransform().rotation == this.targetRot)
				{
					this.syncReceiveTime = 0UL;
					this.interpolator.CurrentPosition = this.syncedObject.ObjectTransform().position;
					this.interpolator.CurrentRotation = this.syncedObject.ObjectTransform().rotation;
					return;
				}
				if (this.ShouldSyncPosition())
				{
					this.syncedObject.ObjectTransform().position = this.targetPos;
				}
				this.syncedObject.ObjectTransform().rotation = this.targetRot;
				this.interpolator.CurrentPosition = this.targetPos;
				this.interpolator.CurrentRotation = this.targetRot;
				this.syncReceiveTime = 0UL;
			}
		}

		private bool ShouldSyncPosition()
		{
			return this.ObjectType != ObjectSyncManager.ObjectTypes.CarDoor && this.ObjectType != ObjectSyncManager.ObjectTypes.Boot && this.ObjectType != ObjectSyncManager.ObjectTypes.CarDoorF && this.ObjectType != ObjectSyncManager.ObjectTypes.BootF;
		}

		private bool ShouldInterpolate(Vector3 pos)
		{
			if (this.hasReceivedBodyState && this.ObjectType == ObjectSyncManager.ObjectTypes.PlayerVehicle && this.lastReceivedBodySpeedSqr < 0.0025f)
			{
				// v0.3.2 backport: parked vehicle — snap to the synced pose instead of interpolating (kills parked rubberband)
				return false;
			}
			return Vector3.Distance(pos, this.syncedObject.ObjectTransform().position) <= 50f && (this.ObjectType == ObjectSyncManager.ObjectTypes.PlayerVehicle || this.ObjectType == ObjectSyncManager.ObjectTypes.AIVehicle || (this.ObjectType == ObjectSyncManager.ObjectTypes.Pickupable && base.gameObject.GetComponent<CarPartOld>() == null) || this.ObjectType == ObjectSyncManager.ObjectTypes.CarDoor || this.ObjectType == ObjectSyncManager.ObjectTypes.Boot || this.ObjectType == ObjectSyncManager.ObjectTypes.CarDoorF || this.ObjectType == ObjectSyncManager.ObjectTypes.BootF || this.ObjectType == ObjectSyncManager.ObjectTypes.Transform);
		}

		public ISyncedObject GetObjectSubtype()
		{
			return this.syncedObject;
		}

		private Rigidbody GetSyncedRigidbody()
		{
			if (this.rigidbodyLookupDone)
			{
				return this.cachedRigidbody;
			}
			if (this.syncedObject == null)
			{
				return null;
			}
			Transform transform = this.syncedObject.ObjectTransform();
			if (transform != null)
			{
				this.cachedRigidbody = transform.GetComponentInParent<Rigidbody>();
			}
			this.rigidbodyLookupDone = true;
			return this.cachedRigidbody;
		}

		public void SetRemoteBodyState(Vector3 velocity, Vector3 angularVelocity, bool hasAngularVelocity, float[] wheelRpms)
		{
			// v0.3.2 backport: apply the owner's rigidbody velocity with the pose so leftover
			// local physics from interpolation cannot keep dragging the vehicle.
			try
			{
				Rigidbody syncedRigidbody = this.GetSyncedRigidbody();
				if (syncedRigidbody != null && !syncedRigidbody.isKinematic)
				{
					syncedRigidbody.velocity = velocity;
					if (hasAngularVelocity)
					{
						syncedRigidbody.angularVelocity = angularVelocity;
					}
				}
				this.lastReceivedBodySpeedSqr = velocity.sqrMagnitude;
				this.hasReceivedBodyState = true;
				this.receivedWheelRpms = wheelRpms;
			}
			catch (Exception ex)
			{
				Logger.Debug("SetRemoteBodyState failed: " + ex);
			}
		}

		private void ApplyReceivedWheelRpms()
		{
			// v0.3.2 backport: wheel roll sync (best-effort — rotates wheel transforms on remote copies)
			if (this.receivedWheelRpms == null || this.receivedWheelRpms.Length == 0)
			{
				return;
			}
			try
			{
				if (this.wheelColliders == null)
				{
					if (this.syncedObject == null)
					{
						return;
					}
					Transform transform = this.syncedObject.ObjectTransform();
					if (transform == null)
					{
						return;
					}
					this.wheelColliders = transform.GetComponentsInChildren<WheelCollider>(true);
				}
				int num = System.Math.Min(this.wheelColliders.Length, this.receivedWheelRpms.Length);
				for (int i = 0; i < num; i++)
				{
					WheelCollider wheelCollider = this.wheelColliders[i];
					if (wheelCollider != null)
					{
						wheelCollider.transform.Rotate(this.receivedWheelRpms[i] * 6f * Time.deltaTime, 0f, 0f, Space.Self);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Debug("ApplyReceivedWheelRpms failed: " + ex);
				this.receivedWheelRpms = null;
			}
		}

		public ObjectSyncComponent()
		{
		}

		public string Name;

		public bool SyncEnabled;

		public ulong Owner;

		public int ObjectID = ObjectSyncManager.AUTOMATIC_ID;

		public ObjectSyncManager.ObjectTypes ObjectType;

		public TransformInterpolator interpolator = new TransformInterpolator();

		private ulong syncReceiveTime;

		private Vector3 targetPos;

		private Quaternion targetRot;

		private bool myLateLoad = true;

		private bool sendConstantSync;

		public ISyncedObject syncedObject;

		private Renderer renderer;

		private bool hasRenderer;

		private bool isSetup;

		private float SYNC_INTERVAL = 0.05f;

		private float timeToSendSync;

		private bool satsumaLate;

		private bool initial;

		private bool timerRunning;

		private bool isCar;

		private PlayerVehicle pv;

		private float timeToCheck;

		private bool aiTimer;

		private float aiTimeSinceLast;

		public bool isAI;

		private bool npcRaycastDisabled;

		private Rigidbody cachedRigidbody;

		private bool rigidbodyLookupDone;

		private bool hasReceivedBodyState;

		private float lastReceivedBodySpeedSqr;

		private float[] receivedWheelRpms;

		private WheelCollider[] wheelColliders;
	}
}
