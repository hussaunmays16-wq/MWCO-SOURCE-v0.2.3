using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game;
using MWCO.Game.Components;
using MWCO.Game.Objects;
using MWCO.Network.Messages;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Network
{
	internal class NetLocalPlayer : NetPlayer
	{
		public NetLocalPlayer(NetManager netManager, NetWorld netWorld, CSteamID steamId)
			: base(netManager, netWorld, steamId)
		{
			NetLocalPlayer.Instance = this;
			this.steamID = steamId;
			GameDoorsManager.Instance.onDoorsOpen = delegate(GameObject door)
			{
				OpenDoorsMessage openDoorsMessage = new OpenDoorsMessage
				{
					position = Utils.GameVec3ToNet(door.transform.position),
					open = true
				};
				netManager.BroadcastMessage<OpenDoorsMessage>(openDoorsMessage, 2, 0);
			};
			GameDoorsManager.Instance.onDoorsClose = delegate(GameObject door)
			{
				OpenDoorsMessage openDoorsMessage2 = new OpenDoorsMessage();
				openDoorsMessage2.position = Utils.GameVec3ToNet(door.transform.position);
				openDoorsMessage2.open = false;
				netManager.BroadcastMessage<OpenDoorsMessage>(openDoorsMessage2, 2, 0);
			};
			LightSwitchManager.Instance.onLightSwitchUsed = delegate(GameObject lswitch, bool turnedOn)
			{
				LightSwitchMessage lightSwitchMessage = new LightSwitchMessage();
				lightSwitchMessage.pos = Utils.GameVec3ToNet(lswitch.transform.position);
				lightSwitchMessage.toggle = turnedOn;
				netManager.BroadcastMessage<LightSwitchMessage>(lightSwitchMessage, 2, 0);
			};
			if (this.animManager == null)
			{
				this.animManager = new PlayerAnimManager();
			}
		}

		public override void Update()
		{
			if (this.state == NetPlayer.State.DrivingVehicle && GameWorld.Instance.Player.Object.transform.parent == null)
			{
				this.SwitchState(NetPlayer.State.OnFoot);
			}
			if (this.state == NetPlayer.State.Passenger || this.state == NetPlayer.State.Backseater || this.state == NetPlayer.State.DrivingVehicle)
			{
				this.timeToUpdate -= Time.deltaTime;
				if (this.timeToUpdate <= 0f && this.netManager.IsPlaying)
				{
					this.SendHeadSync();
				}
			}
			if (this.BankChangeCooldown > 0f)
			{
				this.BankChangeCooldown -= Time.deltaTime;
			}
			this.timeToUpdate -= Time.deltaTime;
			if (this.timeToUpdate <= 0f && this.netManager.IsPlaying)
			{
				if (this.animManager != null)
				{
					this.animManager.PACKETS_LEFT_TO_SYNC--;
					if (this.animManager.PACKETS_LEFT_TO_SYNC <= 0)
					{
						this.animManager.PACKETS_LEFT_TO_SYNC = this.animManager.PACKETS_TOTAL_FOR_SYNC;
						this.SendAnimSync(false);
					}
				}
				if (this.state == NetPlayer.State.OnFoot)
				{
					this.SendOnFootSync();
				}
				if (this.MyMoney != FsmVariables.GlobalVariables.GetFsmFloat("PlayerMoney").Value)
				{
					if (!this.MoneySet)
					{
						this.MyMoney = FsmVariables.GlobalVariables.GetFsmFloat("PlayerMoney").Value;
						this.MoneySet = true;
						return;
					}
					float num = FsmVariables.GlobalVariables.GetFsmFloat("PlayerMoney").Value - this.MyMoney;
					Chat.Instance.AddMessage(string.Format("{0} {1} {2:n0}mk", this.GetName(), (num > 0f) ? "gained" : "spent", (num > 0f) ? num : (num * -1f)), MessageSeverity.Warning);
					this.MyMoney = FsmVariables.GlobalVariables.GetFsmFloat("PlayerMoney").Value;
					this.WriteFSMFloatMessage(67420, "Money", num);
				}
				if (this.MyBankMoney != FsmVariables.GlobalVariables.GetFsmFloat("PlayerBankAccount").Value)
				{
					if (!this.BankMoneySet)
					{
						this.MyBankMoney = FsmVariables.GlobalVariables.GetFsmFloat("PlayerBankAccount").Value;
						this.BankMoneySet = true;
						return;
					}
					float num2 = FsmVariables.GlobalVariables.GetFsmFloat("PlayerBankAccount").Value - this.MyBankMoney;
					if (this.BankChangeCooldown > 0f)
					{
						Logger.Debug(string.Format("Prevented bank charge during cooldown of {0}", num2));
						FsmVariables.GlobalVariables.GetFsmFloat("PlayerBankAccount").Value = this.MyBankMoney;
					}
					else
					{
						Chat.Instance.AddMessage(string.Format("{0} {1} {2:n0}mk {3} the bank.", new object[]
						{
							this.GetName(),
							(num2 > 0f) ? "added" : "deducted",
							(num2 > 0f) ? num2 : (num2 * -1f),
							(num2 > 0f) ? "to" : "from"
						}), MessageSeverity.Warning);
						this.MyBankMoney = FsmVariables.GlobalVariables.GetFsmFloat("PlayerBankAccount").Value;
						this.WriteFSMFloatMessage(67421, "BankMoney", num2);
					}
				}
				this.timeToUpdate += 0.1f;
			}
		}

		public void ApplyMoneyChange(float change, string name)
		{
			this.MyMoney += change;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerMoney").Value = this.MyMoney;
			Chat.Instance.AddMessage(string.Format("{0} {1} {2:n0}mk", name, (change > 0f) ? "gained" : "spent", (change > 0f) ? change : (change * -1f)), MessageSeverity.Warning);
		}

		public void ApplyBankMoneyChange(float change, string name)
		{
			this.MyBankMoney += change;
			FsmVariables.GlobalVariables.GetFsmFloat("PlayerBankAccount").Value = this.MyBankMoney;
			this.BankChangeCooldown = 5f;
			Chat.Instance.AddMessage(string.Format("{0} {1} {2:n0}mk {3} the bank.", new object[]
			{
				name,
				(change > 0f) ? "added" : "deducted",
				(change > 0f) ? change : (change * -1f),
				(change > 0f) ? "to" : "from"
			}), MessageSeverity.Warning);
		}

		private bool SendOnFootSync()
		{
			GamePlayer player = GameWorld.Instance.Player;
			if (player == null)
			{
				return false;
			}
			GameObject @object = player.Object;
			if (@object == null)
			{
				return false;
			}
			PlayerSyncMessage playerSyncMessage = new PlayerSyncMessage();
			playerSyncMessage.position = Utils.GameVec3ToNet(@object.transform.position);
			playerSyncMessage.rotation = Utils.GameQuatToNet(@object.transform.rotation);
			playerSyncMessage.aimPitch = player.FPSCamera.eulerAngles.x;
			if (!NetManager.Instance.IsHost)
			{
				playerSyncMessage.PlayerAttributes = new float[6];
				playerSyncMessage.PlayerAttributes[0] = FsmVariables.GlobalVariables.GetFsmFloat("PlayerThirst").Value;
				playerSyncMessage.PlayerAttributes[1] = FsmVariables.GlobalVariables.GetFsmFloat("PlayerHunger").Value;
				playerSyncMessage.PlayerAttributes[2] = FsmVariables.GlobalVariables.GetFsmFloat("PlayerStress").Value;
				playerSyncMessage.PlayerAttributes[3] = FsmVariables.GlobalVariables.GetFsmFloat("PlayerUrine").Value;
				playerSyncMessage.PlayerAttributes[4] = FsmVariables.GlobalVariables.GetFsmFloat("PlayerFatigue").Value;
				playerSyncMessage.PlayerAttributes[5] = FsmVariables.GlobalVariables.GetFsmFloat("PlayerDirtiness").Value;
			}
			return this.netManager.BroadcastMessage<PlayerSyncMessage>(playerSyncMessage, 0, 0);
		}

		private bool SendHeadSync()
		{
			GamePlayer player = GameWorld.Instance.Player;
			if (player == null)
			{
				return false;
			}
			GameObject @object = player.Object;
			if (@object == null)
			{
				return false;
			}
			PlayerSyncMessage playerSyncMessage = new PlayerSyncMessage();
			playerSyncMessage.position = Utils.GameVec3ToNet(Vector3.zero);
			playerSyncMessage.aimYaw = @object.transform.localEulerAngles.y;
			playerSyncMessage.aimPitch = player.FPSCamera.localEulerAngles.x;
			if (this.pissing)
			{
				playerSyncMessage.PissRate = ((this.pissRate == 10f) ? 10f : (FsmVariables.GlobalVariables.GetFsmFloat("PlayerUrine").Value * 10f));
			}
			if (this.smoking)
			{
				playerSyncMessage.Smoking = this.smokeMode;
			}
			if (!this.netManager.BroadcastMessage<PlayerSyncMessage>(playerSyncMessage, 0, 0))
			{
				return false;
			}
			this.timeToUpdate = 0.000100000005f;
			return true;
		}

		private bool SendAnimSync(bool pissSmoke = false)
		{
			GamePlayer player = GameWorld.Instance.Player;
			if (player == null)
			{
				return false;
			}
			GameObject @object = player.Object;
			if (@object == null)
			{
				return false;
			}
			if (@object.GetComponentInChildren<CharacterMotor>() == null)
			{
				return false;
			}
			AnimSyncMessage animSyncMessage = new AnimSyncMessage();
			animSyncMessage.isRunning = Utils.GetPlaymakerScriptByName(@object, "Running").Fsm.ActiveStateName == "Run";
			if (Utils.GetPlaymakerScriptByName(@object, "Reach").Fsm.GetFsmFloat("Position").Value != 0f)
			{
				animSyncMessage.isLeaning = true;
			}
			else
			{
				animSyncMessage.isLeaning = false;
			}
			animSyncMessage.isGrounded = @object.GetComponentInChildren<CharacterMotor>().grounded;
			animSyncMessage.activeHandState = this.animManager.GetActiveHandState(@object);
			animSyncMessage.swearId = int.MaxValue;
			RaycastHit raycastHit;
			if (this.animManager.GetHandState(animSyncMessage.activeHandState) == PlayerAnimManager.HandStateId.MiddleFingering)
			{
				animSyncMessage.swearId = Utils.GetPlaymakerScriptByName(@object, "PlayerFunctions").Fsm.GetFsmInt("RandomInt").Value;
			}
			else if (this.animManager.GetHandState(animSyncMessage.activeHandState) == PlayerAnimManager.HandStateId.Hitting && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), ref raycastHit, 1.25f) && raycastHit.collider.gameObject.name.Contains("MPPlayerModel"))
			{
				animSyncMessage.swearId = 99;
				this.animManager.HandleSwearingWithObject(Random.Range(1, 17), raycastHit.collider.gameObject);
			}
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(@object, "Speech");
			if (playmakerScriptByName.ActiveStateName == "Swear")
			{
				animSyncMessage.swearId = this.animManager.Swears_Offset + playmakerScriptByName.Fsm.GetFsmInt("RandomInt").Value;
			}
			else if (playmakerScriptByName.ActiveStateName == "Drunk speech")
			{
				animSyncMessage.swearId = this.animManager.DrunkSpeaking_Offset + playmakerScriptByName.Fsm.GetFsmInt("RandomInt").Value;
			}
			else if (playmakerScriptByName.ActiveStateName == "Yes gestures")
			{
				animSyncMessage.swearId = this.animManager.Agreeing_Offset + playmakerScriptByName.Fsm.GetFsmInt("RandomInt").Value;
			}
			animSyncMessage.crouchPosition = Utils.GetPlaymakerScriptByName(@object, "Crouch").Fsm.GetFsmFloat("Position").Value;
			if (Utils.GetPlaymakerScriptByName(@object.transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").gameObject, "Drunk Mode").Fsm.GetFsmFloat("DrunkYmax").Value >= 4.5f)
			{
				animSyncMessage.isDrunk = true;
			}
			else
			{
				animSyncMessage.isDrunk = false;
			}
			if (!this.animManager.AreDrinksPreloaded())
			{
				this.animManager.PreloadDrinkObjects(@object);
			}
			animSyncMessage.drinkId = this.animManager.GetDrinkingObject(@object);
			if (this.pissing)
			{
				animSyncMessage.pissRate = ((this.pissRate == 10f) ? 10f : (FsmVariables.GlobalVariables.GetFsmFloat("PlayerUrine").Value * 10f));
				RaycastHit raycastHit2;
				if (Physics.Raycast(GamePlayer.Instance.Object.transform.position, GamePlayer.Instance.Object.transform.TransformDirection(Vector3.forward), ref raycastHit2, 1.1f) && raycastHit2.collider.gameObject.name.Contains("MPPlayerModel"))
				{
					animSyncMessage.swearId = -99;
				}
			}
			if (this.smoking)
			{
				animSyncMessage.smoking = this.smokeMode;
			}
			return this.netManager.BroadcastMessage<AnimSyncMessage>(animSyncMessage, 0, 0);
		}

		public override void EnterVehicle(ObjectSyncComponent vehicle, int passenger)
		{
			base.EnterVehicle(vehicle, passenger);
			VehicleEnterMessage vehicleEnterMessage = new VehicleEnterMessage();
			vehicleEnterMessage.objectID = vehicle.ObjectID;
			vehicleEnterMessage.passenger = passenger;
			this.netManager.BroadcastMessage<VehicleEnterMessage>(vehicleEnterMessage, 2, 0);
			if (passenger == 0)
			{
				vehicle.TakeSyncControl();
			}
		}

		public override void LeaveVehicle()
		{
			base.LeaveVehicle();
			this.netManager.BroadcastMessage<VehicleLeaveMessage>(new VehicleLeaveMessage(), 2, 0);
		}

		public void WriteVehicleStateMessage(ObjectSyncComponent vehicle, string state, bool ignition)
		{
			VehicleStateMessage vehicleStateMessage = new VehicleStateMessage();
			vehicleStateMessage.objectID = vehicle.ObjectID;
			vehicleStateMessage.state = state;
			vehicleStateMessage.ignition = ignition;
			this.netManager.BroadcastMessage<VehicleStateMessage>(vehicleStateMessage, 2, 0);
		}

		public void WriteVehicleSwitchMessage(ObjectSyncComponent vehicle, PlayerVehicle.SwitchIDs switchID, bool newValue, float newValueFloat, string newValueString = "")
		{
			VehicleSwitchMessage vehicleSwitchMessage = new VehicleSwitchMessage();
			vehicleSwitchMessage.objectID = vehicle.ObjectID;
			vehicleSwitchMessage.switchID = (int)switchID;
			vehicleSwitchMessage.switchValue = newValue;
			if (newValueFloat != -1f)
			{
				vehicleSwitchMessage.SwitchValueFloat = newValueFloat;
			}
			if (newValueString != "")
			{
				vehicleSwitchMessage.SwitchValueString = newValueString;
			}
			this.netManager.BroadcastMessage<VehicleSwitchMessage>(vehicleSwitchMessage, 2, 0);
		}

		public void WriteHouseSwitchMessage(ObjectSyncComponent house, PlayerHouse.SwitchIDs switchID, bool newValue, float newValueFloat)
		{
			HouseSwitchMessage houseSwitchMessage = new HouseSwitchMessage();
			houseSwitchMessage.objectID = house.ObjectID;
			houseSwitchMessage.switchID = (int)switchID;
			houseSwitchMessage.switchValue = newValue;
			if (newValueFloat != -1f)
			{
				houseSwitchMessage.SwitchValueFloat = newValueFloat;
			}
			this.netManager.BroadcastMessage<HouseSwitchMessage>(houseSwitchMessage, 2, 0);
		}

		public void WriteCarPartMessage(ObjectSyncComponent osc, bool assemble = false)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			SatsumaSwitchMessage satsumaSwitchMessage = new SatsumaSwitchMessage();
			satsumaSwitchMessage.objectID = osc.ObjectID;
			satsumaSwitchMessage.Assemble = assemble;
			this.netManager.BroadcastMessage<SatsumaSwitchMessage>(satsumaSwitchMessage, 2, 0);
		}

		public void WriteFSMBoolMessage(int objectID, string fsmName, bool newBool)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			FSMValueChangeMessage fsmvalueChangeMessage = new FSMValueChangeMessage();
			fsmvalueChangeMessage.objectID = objectID;
			fsmvalueChangeMessage.fsmValueName = fsmName;
			fsmvalueChangeMessage.NewBool = newBool;
			this.netManager.BroadcastMessage<FSMValueChangeMessage>(fsmvalueChangeMessage, 2, 0);
		}

		public void WriteFSMFloatMessage(int objectID, string fsmName, float newFloat)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			FSMValueChangeMessage fsmvalueChangeMessage = new FSMValueChangeMessage();
			fsmvalueChangeMessage.objectID = objectID;
			fsmvalueChangeMessage.fsmValueName = fsmName;
			fsmvalueChangeMessage.NewFloat = newFloat;
			this.netManager.BroadcastMessage<FSMValueChangeMessage>(fsmvalueChangeMessage, 2, 0);
		}

		public void WriteFSMStringMessage(int objectID, string fsmName, string newString)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			FSMValueChangeMessage fsmvalueChangeMessage = new FSMValueChangeMessage();
			fsmvalueChangeMessage.objectID = objectID;
			fsmvalueChangeMessage.fsmValueName = fsmName;
			fsmvalueChangeMessage.NewString = newString;
			this.netManager.BroadcastMessage<FSMValueChangeMessage>(fsmvalueChangeMessage, 2, 0);
		}

		public void WriteHostTriggerAction(int id)
		{
			Logger.Debug("Writing host trigger actino ID " + id.ToString());
			if (MPController.Instance.boat == null)
			{
				return;
			}
			SatsumaSwitchMessage satsumaSwitchMessage = new SatsumaSwitchMessage();
			satsumaSwitchMessage.objectID = id;
			this.netManager.BroadcastMessage<SatsumaSwitchMessage>(satsumaSwitchMessage, 2, 0);
		}

		public void WriteTriggerMessage(int triggerId, int partId, int assembId = 0)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			SatsumaSwitchMessage satsumaSwitchMessage = new SatsumaSwitchMessage
			{
				objectID = assembId,
				TriggerID = triggerId,
				PartObjectID = partId
			};
			this.netManager.BroadcastMessage<SatsumaSwitchMessage>(satsumaSwitchMessage, 2, 0);
		}

		public void WriteBoltMessage(int boltID, bool tighten)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			SatsumaSwitchMessage satsumaSwitchMessage = new SatsumaSwitchMessage
			{
				objectID = boltID,
				Tighten = tighten
			};
			this.netManager.BroadcastMessage<SatsumaSwitchMessage>(satsumaSwitchMessage, 2, 0);
		}

		public void WritePaintMessage(GameObject obj, Color color)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			SatsumaSwitchMessage satsumaSwitchMessage = new SatsumaSwitchMessage();
			satsumaSwitchMessage.objectID = obj.GetComponent<ObjectSyncComponent>().ObjectID;
			satsumaSwitchMessage.Color = Utils.GameColorToNet(color);
			this.netManager.BroadcastMessage<SatsumaSwitchMessage>(satsumaSwitchMessage, 2, 0);
		}

		public void WriteCarPartFloatMessage(ObjectSyncComponent osc, float value)
		{
			if (MPController.Instance.boat == null)
			{
				return;
			}
			SatsumaSwitchMessage satsumaSwitchMessage = new SatsumaSwitchMessage();
			satsumaSwitchMessage.objectID = osc.ObjectID;
			satsumaSwitchMessage.FloatValue = value;
			this.netManager.BroadcastMessage<SatsumaSwitchMessage>(satsumaSwitchMessage, 2, 0);
		}

		public void WriteFleetariOrderListMessage(bool[] list)
		{
			OrderListMessage orderListMessage = new OrderListMessage();
			orderListMessage.fleetari = true;
			orderListMessage.FleetariOrderList = list;
			this.netManager.BroadcastMessage<OrderListMessage>(orderListMessage, 2, 0);
		}

		public void WriteOrderListMessage(string[] list)
		{
			OrderListMessage orderListMessage = new OrderListMessage();
			orderListMessage.fleetari = false;
			orderListMessage.OrderList = list;
			this.netManager.BroadcastMessage<OrderListMessage>(orderListMessage, 2, 0);
		}

		public void SendSatsumaMessage()
		{
			new SatsumaSwitchMessage();
			GameObject gameObject = GameObject.Find("SATSUMA(557kg, 248)");
			foreach (Transform transform in this.GetAllChildren(gameObject))
			{
				if (transform.GetComponent<ObjectSyncComponent>() != null)
				{
					Logger.Debug(string.Concat(new string[]
					{
						transform.gameObject.name,
						" is a child of ",
						transform.parent.name,
						". ",
						gameObject.transform.FindChild(transform.gameObject.name) ? "yes" : "no"
					}));
				}
			}
		}

		public void WriteSpawnState(FullWorldSyncMessage msg)
		{
			msg.spawnPosition = Utils.GameVec3ToNet(this.GetPosition());
			msg.spawnRotation = Utils.GameQuatToNet(this.GetRotation());
			msg.pickedUpObject = ushort.MaxValue;
		}

		public List<Transform> GetAllChildren(GameObject gameObject)
		{
			List<Transform> list = new List<Transform>();
			foreach (object obj in gameObject.transform)
			{
				Transform transform = (Transform)obj;
				list.Add(transform);
				List<Transform> allChildren = this.GetAllChildren(transform.gameObject);
				if (allChildren.Count > 0)
				{
					list.AddRange(allChildren);
				}
			}
			return list;
		}

		public void SendObjectSync(int objectID, Vector3 pos, Quaternion rot, ObjectSyncManager.SyncTypes syncType, float[] syncedVariables)
		{
			ObjectSyncMessage objectSyncMessage = new ObjectSyncMessage();
			objectSyncMessage.objectID = objectID;
			objectSyncMessage.position = Utils.GameVec3ToNet(pos);
			objectSyncMessage.rotation = Utils.GameQuatToNet(rot);
			objectSyncMessage.SyncType = (int)syncType;
			if (syncedVariables != null)
			{
				objectSyncMessage.SyncedVariables = syncedVariables;
			}
			this.netManager.BroadcastMessage<ObjectSyncMessage>(objectSyncMessage, 1, 0);
		}

		public void WritePickupMessage(int objectID, bool pickUp)
		{
			ObjectSyncMessage objectSyncMessage = new ObjectSyncMessage();
			objectSyncMessage.objectID = objectID;
			objectSyncMessage.PickUp = pickUp;
			this.netManager.BroadcastMessage<ObjectSyncMessage>(objectSyncMessage, 2, 0);
		}

		public void RequestObjectSync(int objectID)
		{
			ObjectSyncRequestMessage objectSyncRequestMessage = new ObjectSyncRequestMessage();
			objectSyncRequestMessage.objectID = objectID;
			this.netManager.BroadcastMessageToSteamID<ObjectSyncRequestMessage>(NetManager.Instance.players[0].SteamId, objectSyncRequestMessage, 2, 0);
		}

		public void SendObjectSyncResponse(CSteamID sender, int objectID, bool accepted)
		{
			ObjectSyncResponseMessage objectSyncResponseMessage = new ObjectSyncResponseMessage();
			objectSyncResponseMessage.objectID = objectID;
			objectSyncResponseMessage.accepted = accepted;
			this.netManager.BroadcastMessageToSteamID<ObjectSyncResponseMessage>(sender, objectSyncResponseMessage, 2, 0);
		}

		public void SendEventHookSync(int fsmID, int fsmEventID, string fsmEventName = "none")
		{
			EventHookSyncMessage eventHookSyncMessage = new EventHookSyncMessage();
			eventHookSyncMessage.fsmID = fsmID;
			eventHookSyncMessage.fsmEventID = fsmEventID;
			eventHookSyncMessage.request = false;
			if (fsmEventName != "none")
			{
				eventHookSyncMessage.FsmEventName = fsmEventName;
			}
			this.netManager.BroadcastMessage<EventHookSyncMessage>(eventHookSyncMessage, 2, 0);
		}

		public void SendEventHookSyncToSteamID(CSteamID sender, int fsmID, int fsmEventID, string fsmEventName = "none")
		{
			EventHookSyncMessage eventHookSyncMessage = new EventHookSyncMessage();
			eventHookSyncMessage.fsmID = fsmID;
			eventHookSyncMessage.fsmEventID = fsmEventID;
			eventHookSyncMessage.request = false;
			if (fsmEventName != "none")
			{
				eventHookSyncMessage.FsmEventName = fsmEventName;
			}
			this.netManager.BroadcastMessageToSteamID<EventHookSyncMessage>(sender, eventHookSyncMessage, 2, 0);
		}

		public void RequestEventHookSync(int fsmID)
		{
			EventHookSyncMessage eventHookSyncMessage = new EventHookSyncMessage();
			eventHookSyncMessage.fsmID = fsmID;
			eventHookSyncMessage.fsmEventID = -1;
			eventHookSyncMessage.request = true;
			this.netManager.BroadcastMessageToSteamID<EventHookSyncMessage>(NetManager.Instance.players[0].SteamId, eventHookSyncMessage, 2, 0);
		}

		protected override void SwitchState(NetPlayer.State newState)
		{
			if (this.state == newState)
			{
				return;
			}
			base.SwitchState(newState);
			this.timeToUpdate = 0f;
		}

		public override Vector3 GetPosition()
		{
			GamePlayer player = GameWorld.Instance.Player;
			if (player == null)
			{
				return Vector3.zero;
			}
			GameObject @object = player.Object;
			if (@object == null)
			{
				return Vector3.zero;
			}
			return @object.transform.position;
		}

		public override Quaternion GetRotation()
		{
			GamePlayer player = GameWorld.Instance.Player;
			if (player == null)
			{
				return Quaternion.identity;
			}
			GameObject @object = player.Object;
			if (@object == null)
			{
				return Quaternion.identity;
			}
			return @object.transform.rotation;
		}

		public override string GetName()
		{
			return SteamFriends.GetPersonaName();
		}

		public override void Teleport(Vector3 pos, Quaternion rot)
		{
			GamePlayer player = GameWorld.Instance.Player;
			if (player == null)
			{
				return;
			}
			GameObject @object = player.Object;
			if (@object == null)
			{
				return;
			}
			@object.transform.position = pos;
			@object.transform.rotation = rot;
		}

		internal void WriteHouseSwitchMessage(ObjectSyncComponent syncComponent, bool v1, int v2)
		{
			throw new NotImplementedException();
		}

		public static NetLocalPlayer Instance;

		private float timeToUpdate;

		public const float SYNC_INTERVAL = 0.1f;

		private CSteamID steamID;

		public bool isSeatbelted;

		private float MyMoney;

		private float MyBankMoney;

		private bool MoneySet;

		private bool BankMoneySet;

		private float BankChangeCooldown;

		public float pissRate;

		public bool pissing;

		public bool smoking;

		public int smokeMode;

		public bool pickUp;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass5_0
		{
			public <>c__DisplayClass5_0()
			{
			}

			internal void <.ctor>b__0(GameObject door)
			{
				OpenDoorsMessage openDoorsMessage = new OpenDoorsMessage
				{
					position = Utils.GameVec3ToNet(door.transform.position),
					open = true
				};
				this.netManager.BroadcastMessage<OpenDoorsMessage>(openDoorsMessage, 2, 0);
			}

			internal void <.ctor>b__1(GameObject door)
			{
				OpenDoorsMessage openDoorsMessage = new OpenDoorsMessage();
				openDoorsMessage.position = Utils.GameVec3ToNet(door.transform.position);
				openDoorsMessage.open = false;
				this.netManager.BroadcastMessage<OpenDoorsMessage>(openDoorsMessage, 2, 0);
			}

			internal void <.ctor>b__2(GameObject lswitch, bool turnedOn)
			{
				LightSwitchMessage lightSwitchMessage = new LightSwitchMessage();
				lightSwitchMessage.pos = Utils.GameVec3ToNet(lswitch.transform.position);
				lightSwitchMessage.toggle = turnedOn;
				this.netManager.BroadcastMessage<LightSwitchMessage>(lightSwitchMessage, 2, 0);
			}

			public NetManager netManager;
		}
	}
}
