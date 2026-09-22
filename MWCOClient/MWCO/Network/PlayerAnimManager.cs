using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MWCO.Game;
using MWCO.Game.Objects;
using MWCO.Network.Messages;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Network
{
	public class PlayerAnimManager
	{
		private string GetAnimationName(PlayerAnimManager.AnimationId animation)
		{
			return this.AnimationNames[(int)animation];
		}

		public void SetupAnimations(GameObject character)
		{
			this.characterGameObject = character;
			this.characterAnimationComponent = this.characterGameObject.GetComponentInChildren<Animation>();
			string text = "ANIMATIONS SETUP! ";
			string name = character.name;
			string text2 = " ";
			GameObject gameObject = this.characterGameObject;
			Logger.Debug(text + name + text2 + ((gameObject != null) ? gameObject.name : null));
			this.characterAnimationComponent["Jump"].layer = 1;
			this.characterAnimationComponent["Drunk"].layer = 2;
			this.characterAnimationComponent["Drunk"].blendMode = 1;
			this.characterAnimationComponent["Lean"].layer = 3;
			this.characterAnimationComponent["Lean"].blendMode = 1;
			this.characterAnimationComponent["Finger"].layer = 3;
			this.characterAnimationComponent["Finger"].blendMode = 1;
			this.characterAnimationComponent["Hitchhike"].layer = 3;
			this.characterAnimationComponent["Hitchhike"].blendMode = 1;
			this.characterAnimationComponent["Hit"].layer = 3;
			this.characterAnimationComponent["Hit"].blendMode = 1;
			this.characterAnimationComponent["Push"].layer = 3;
			this.characterAnimationComponent["Push"].blendMode = 1;
			this.characterAnimationComponent["Drink"].layer = 3;
			this.characterAnimationComponent["Drink"].blendMode = 1;
			this.RegisterAnimStates();
		}

		private PlayerAnimManager.AnimationId GetAnimationFromStance(PlayerAnimManager.StanceId stance, bool standingAnim = true)
		{
			if (stance != PlayerAnimManager.StanceId.Crouching)
			{
				if (stance != PlayerAnimManager.StanceId.CrouchingLow)
				{
					if (standingAnim)
					{
						return PlayerAnimManager.AnimationId.Standing;
					}
					return PlayerAnimManager.AnimationId.Walk;
				}
				else
				{
					if (standingAnim)
					{
						return PlayerAnimManager.AnimationId.CrouchingLow;
					}
					return PlayerAnimManager.AnimationId.CrouchingLowWalk;
				}
			}
			else
			{
				if (standingAnim)
				{
					return PlayerAnimManager.AnimationId.Crouching;
				}
				return PlayerAnimManager.AnimationId.CrouchingWalk;
			}
		}

		public PlayerAnimManager.HandStateId GetHandState(byte handState)
		{
			return (PlayerAnimManager.HandStateId)handState;
		}

		public byte GetActiveHandState(GameObject gameObject)
		{
			GameObject gameObject2 = gameObject.transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").gameObject;
			byte b = 0;
			while ((int)b < this.HandStateNames.Length)
			{
				string text = this.HandStateNames[(int)b];
				if (gameObject2.transform.FindChild(text).gameObject.activeInHierarchy)
				{
					return b;
				}
				b += 1;
			}
			return byte.MaxValue;
		}

		public bool AreDrinksPreloaded()
		{
			return PlayerAnimManager.Drinks.Count != 0;
		}

		public void PreloadDrinkObjects(GameObject character)
		{
			GameObject gameObject = character.transform.FindChild("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Drink/Hand").gameObject;
			byte b = 0;
			while ((int)b < this.DrinkObjectNames.Length)
			{
				GameObject gameObject2 = gameObject.transform.FindChild(this.DrinkObjectNames[(int)b]).gameObject;
				ErrorHandling.Assert(gameObject2, "Unable to find drink object - " + this.DrinkObjectNames[(int)b]);
				PlayerAnimManager.Drinks.Add(gameObject2);
				b += 1;
			}
		}

		public byte GetDrinkingObject(GameObject character)
		{
			GameObject gameObject = character.transform.FindChild("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Drink/Hand").gameObject;
			byte b = 0;
			while ((int)b < this.DrinkObjectNames.Length)
			{
				if (gameObject.transform.FindChild(this.DrinkObjectNames[(int)b]).gameObject.activeInHierarchy)
				{
					return b;
				}
				b += 1;
			}
			return byte.MaxValue;
		}

		public void SetDrinkingObject(byte drinkingObjectId)
		{
			if (drinkingObjectId == 255)
			{
				this.PlayActionAnim(PlayerAnimManager.AnimationId.Drinking, false);
				if (this.ourDrinkObject != null)
				{
					Object.DestroyObject(this.ourDrinkObject);
					this.ourDrinkObject = null;
				}
				return;
			}
			string text = this.DrinkObjectNames[(int)drinkingObjectId];
			GameObject gameObject = null;
			try
			{
				foreach (GameObject gameObject2 in PlayerAnimManager.Drinks)
				{
					if (gameObject2.name == text)
					{
						gameObject = gameObject2;
					}
				}
			}
			catch (Exception)
			{
				Chat.Instance.AddMessage("Can't get drink game objects! Avoided crash but may become buggy. PlayerAnimManager:261", MessageSeverity.Error);
			}
			try
			{
				if (this.ourDrinkObject != null)
				{
					Object.DestroyObject(this.ourDrinkObject);
				}
				this.ourDrinkObject = Object.Instantiate<GameObject>(gameObject);
				this.ourDrinkObject.SetActive(true);
			}
			catch (Exception)
			{
				Chat.Instance.AddMessage("Can't Instantiate drinking object to spawn! Avoided crash but may become buggy. PlayerAnimManager:274", MessageSeverity.Error);
			}
			Transform transform = this.characterGameObject.transform.FindChild("pelvis/spine_mid/shoulders/collar_left/shoulder(leftx)/arm(leftx)/hand_left/finger_left");
			this.ourDrinkObject.transform.SetParent(transform);
			this.ourDrinkObject.transform.localPosition = new Vector3(this.DrinkOffsets[(int)drinkingObjectId, 0], this.DrinkOffsets[(int)drinkingObjectId, 1], this.DrinkOffsets[(int)drinkingObjectId, 2]);
			this.ourDrinkObject.transform.localEulerAngles = new Vector3(this.DrinkRotations[(int)drinkingObjectId, 0], this.DrinkRotations[(int)drinkingObjectId, 1], this.DrinkRotations[(int)drinkingObjectId, 2]);
			this.ourDrinkObject.layer = 0;
			if (this.ourDrinkObject.transform.childCount != 0)
			{
				if (this.ourDrinkObject.name.StartsWith("Hand"))
				{
					this.ourDrinkObject.transform.GetChild(2).gameObject.layer = 0;
					this.ourDrinkObject.transform.GetChild(2).localPosition = new Vector3(0f, 0f, 0f);
					Object.DestroyObject(this.ourDrinkObject.transform.GetChild(0).gameObject);
					Object.DestroyObject(this.ourDrinkObject.transform.GetChild(1).gameObject);
				}
				else
				{
					this.ourDrinkObject.transform.GetChild(0).gameObject.layer = 0;
				}
			}
			this.PlayActionAnim(PlayerAnimManager.AnimationId.Drinking, true);
		}

		public void PlayAnimation(PlayerAnimManager.AnimationId animation, bool force = false, bool mainLayer = true)
		{
			if (this.characterAnimationComponent == null)
			{
				return;
			}
			if (!force && this.currentAnim == animation && mainLayer)
			{
				return;
			}
			if (this.pauseAnimations)
			{
				return;
			}
			string animationName = this.GetAnimationName(animation);
			if (force)
			{
				this.characterAnimationComponent.Play(animationName);
			}
			else
			{
				this.characterAnimationComponent.CrossFade(animationName);
			}
			if (mainLayer)
			{
				this.currentAnim = animation;
				this.activeAnimationState = this.characterAnimationComponent[animationName];
			}
		}

		public void DisableAnimationComponent()
		{
			if (this.characterAnimationComponent == null)
			{
				return;
			}
			this.pauseAnimations = true;
			this.characterAnimationComponent.CrossFade(this.GetAnimationName(PlayerAnimManager.AnimationId.CrouchingLow), 0.1f);
			this.currentAnim = PlayerAnimManager.AnimationId.CrouchingLow;
			this.activeAnimationState = this.characterAnimationComponent[this.GetAnimationName(PlayerAnimManager.AnimationId.CrouchingLow)];
		}

		public void EnableAnimationComponent()
		{
			if (this.characterAnimationComponent == null)
			{
				return;
			}
			this.pauseAnimations = false;
			this.characterAnimationComponent.enabled = true;
		}

		private void BlendOutAnimation(PlayerAnimManager.AnimationId animation)
		{
			if (this.characterAnimationComponent == null)
			{
				return;
			}
			this.characterAnimationComponent.Blend(this.GetAnimationName(animation), 0f);
		}

		private void PlayActionAnim(PlayerAnimManager.AnimationId animation, bool play)
		{
			if (this.characterGameObject == null)
			{
				Chat.Instance.AddMessage("CHARACTER OBJECT IS NULL, WHY??????", MessageSeverity.Info);
				return;
			}
			string animationName = this.GetAnimationName(animation);
			if (play)
			{
				this.characterAnimationComponent[animationName].wrapMode = 8;
				this.characterAnimationComponent[animationName].speed = 1f;
				this.characterAnimationComponent[animationName].enabled = true;
				this.characterAnimationComponent[animationName].weight = 1f;
				return;
			}
			this.characterAnimationComponent[animationName].wrapMode = 1;
			if (this.characterAnimationComponent[animationName].time > this.characterAnimationComponent[animationName].length)
			{
				this.characterAnimationComponent[animationName].time = this.characterAnimationComponent[animationName].length;
			}
			this.characterAnimationComponent[animationName].speed = -1f;
			this.characterAnimationComponent[animationName].weight = 1f;
		}

		public void CheckBlendedOutAnimationStates()
		{
			if (this.characterAnimationComponent == null)
			{
				return;
			}
			if (this.characterAnimationComponent["Jump"].time != 0f && this.characterAnimationComponent["Jump"].weight == 0f)
			{
				this.characterAnimationComponent["Jump"].enabled = false;
				this.characterAnimationComponent["Jump"].time = 0f;
			}
			if (this.characterAnimationComponent["Drunk"].time != 0f && this.characterAnimationComponent["Drunk"].weight == 0f)
			{
				this.characterAnimationComponent["Drunk"].enabled = false;
				this.characterAnimationComponent["Drunk"].time = 0f;
			}
		}

		private void RegisterAnimStates()
		{
			PlayerAnimManager.LeaningState leaningState = new PlayerAnimManager.LeaningState();
			leaningState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(leaningState);
			PlayerAnimManager.JumpState jumpState = new PlayerAnimManager.JumpState();
			jumpState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(jumpState);
			PlayerAnimManager.FingerState fingerState = new PlayerAnimManager.FingerState();
			fingerState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(fingerState);
			PlayerAnimManager.HitchhikeState hitchhikeState = new PlayerAnimManager.HitchhikeState();
			hitchhikeState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(hitchhikeState);
			PlayerAnimManager.DrunkState drunkState = new PlayerAnimManager.DrunkState();
			drunkState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(drunkState);
			PlayerAnimManager.HitState hitState = new PlayerAnimManager.HitState();
			hitState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(hitState);
			PlayerAnimManager.PushState pushState = new PlayerAnimManager.PushState();
			pushState.Initialize(this.characterGameObject, this.characterAnimationComponent);
			this.states.Add(pushState);
		}

		public void HandleAnimations(AnimSyncMessage msg, GameObject character)
		{
			this.isRunning = msg.isRunning;
			this.HandleCrouchStates(msg.crouchPosition);
			this.HandleDrinking(msg.drinkId);
			this.HandleSwearing(msg.swearId);
			foreach (PlayerAnimManager.AnimState animState in this.states)
			{
				animState.TryActivate(msg);
			}
		}

		public void HandleOnFootMovementAnimations(float speed)
		{
			if (speed <= 0.001f)
			{
				this.PlayAnimation(this.GetAnimationFromStance(this.currentStance, true), false, true);
				return;
			}
			if (this.isRunning)
			{
				this.PlayAnimation(PlayerAnimManager.AnimationId.Running, false, true);
				return;
			}
			this.PlayAnimation(this.GetAnimationFromStance(this.currentStance, false), false, true);
		}

		public void SyncVerticalHeadLook(GameObject characterGameObject, float progress)
		{
			characterGameObject.transform.FindChild("pelvis/spine_mid/shoulders/head").rotation *= Quaternion.Euler(0f, 0f, -this.aimRot);
		}

		private void HandleCrouchStates(float crouchRotation)
		{
			if (crouchRotation < 0.85f)
			{
				this.currentStance = PlayerAnimManager.StanceId.CrouchingLow;
				return;
			}
			if (crouchRotation < 1.4f)
			{
				this.currentStance = PlayerAnimManager.StanceId.Crouching;
				return;
			}
			this.currentStance = PlayerAnimManager.StanceId.Standing;
		}

		private void HandleDrinking(byte DrinkID)
		{
			byte b = this.currentDrinkId;
			this.currentDrinkId = DrinkID;
			if (b != this.currentDrinkId)
			{
				this.SetDrinkingObject(this.currentDrinkId);
			}
		}

		public void HandleSwearing(int SwearID)
		{
			try
			{
				if (SwearID == -99)
				{
					FsmVariables.GlobalVariables.GetFsmFloat("PlayerThirst").Value -= 1f;
				}
				else if (SwearID != 2147483647 && SwearID != this.currentSwearId)
				{
					if (SwearID >= this.DrunkSpeaking_Offset)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget("Drunk", this.characterGameObject.transform, 1f, new float?((float)1), 0f, SwearID.ToString());
					}
					else if (SwearID >= this.Agreeing_Offset)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget("Yes", this.characterGameObject.transform, 8f, new float?((float)1), 0f, SwearID.ToString());
					}
					else if (SwearID >= this.Swears_Offset)
					{
						MasterAudio.PlaySound3DFollowTransformAndForget("Swearing", this.characterGameObject.transform, 1f, new float?((float)1), 0f, SwearID.ToString());
					}
					else if (SwearID == 99)
					{
						GamePlayer player = GameWorld.Instance.Player;
						if (player != null)
						{
							GameObject gameObject = player.Object.gameObject;
							if (gameObject != null)
							{
								Utils.GetPlaymakerScriptByName(gameObject, "Speech").Fsm.BroadcastEvent("SWEARING", false);
								MPController.Instance.Punched();
							}
						}
					}
					else
					{
						MasterAudio.PlaySound3DFollowTransformAndForget("Fuck", this.characterGameObject.transform, 1f, new float?((float)1), 0f, SwearID.ToString());
					}
				}
				this.currentSwearId = SwearID;
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("swear bug" + ex.Message, MessageSeverity.Error);
			}
		}

		public void HandleSwearingWithObject(int SwearID, GameObject characterGameObject)
		{
			if (SwearID == -99)
			{
				FsmVariables.GlobalVariables.GetFsmFloat("PlayerThirst").Value -= 1f;
			}
			else if (SwearID != 2147483647 && SwearID != this.currentSwearId)
			{
				if (SwearID >= this.DrunkSpeaking_Offset)
				{
					MasterAudio.PlaySound3DFollowTransformAndForget("Drunk", characterGameObject.transform, 1f, new float?((float)1), 0f, SwearID.ToString());
				}
				else if (SwearID >= this.Agreeing_Offset)
				{
					MasterAudio.PlaySound3DFollowTransformAndForget("Yes", characterGameObject.transform, 8f, new float?((float)1), 0f, SwearID.ToString());
				}
				else if (SwearID >= this.Swears_Offset)
				{
					MasterAudio.PlaySound3DFollowTransformAndForget("Swearing", characterGameObject.transform, 1f, new float?((float)1), 0f, SwearID.ToString());
				}
				else if (SwearID == 99)
				{
					GamePlayer player = GameWorld.Instance.Player;
					if (player != null)
					{
						GameObject gameObject = player.Object.gameObject;
						if (gameObject != null)
						{
							Utils.GetPlaymakerScriptByName(gameObject, "Speech").Fsm.BroadcastEvent("SWEARING", false);
						}
					}
				}
				else
				{
					MasterAudio.PlaySound3DFollowTransformAndForget("Fuck", characterGameObject.transform, 1f, new float?((float)1), 0f, SwearID.ToString());
				}
			}
			this.currentSwearId = SwearID;
		}

		public PlayerAnimManager()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PlayerAnimManager()
		{
		}

		public List<PlayerAnimManager.AnimState> states = new List<PlayerAnimManager.AnimState>();

		private GameObject characterGameObject;

		private Animation characterAnimationComponent;

		public PlayerAnimManager.AnimationId currentAnim = PlayerAnimManager.AnimationId.Standing;

		public AnimationState activeAnimationState;

		public int PACKETS_LEFT_TO_SYNC;

		public int PACKETS_TOTAL_FOR_SYNC = 5;

		private int currentSwearId = int.MaxValue;

		public int Swears_Offset = 100;

		public int Agreeing_Offset = 200;

		public int DrunkSpeaking_Offset = 300;

		private string[] AnimationNames = new string[]
		{
			"Walk", "Idle", "Jump", "Drunk", "Lean", "Finger", "Hitchhike", "Crouch", "CrouchLow", "CrouchWalk",
			"CrouchLowWalk", "Run", "Hit", "Push", "Drink"
		};

		private string[] HandStateNames = new string[] { "MiddleFinger", "Lift", "Fist", "Hand Push", "Drink/Hand" };

		private static List<GameObject> Drinks = new List<GameObject>();

		private GameObject ourDrinkObject;

		private string[] DrinkObjectNames = new string[] { "HandJuice", "HandMilk", "HandSpray", "Coffee", "CoffeeGranny", "BeerBottle", "BoozeBottle", "ShotGlass", "MilkGlass" };

		private float[,] DrinkOffsets = new float[,]
		{
			{ -0.008f, -0.016f, 0.005f },
			{ -0.025f, -0.02f, 0.015f },
			{ -0.01f, 0f, 0.01f },
			{ -0.015f, 0.01f, 0.01f },
			{ -0.015f, 0.011f, 0.01f },
			{ -0.012f, -0.008f, 0.015f },
			{ -0.02f, -0.02f, 0.021f },
			{ -0.02f, 0.01f, 0.01f },
			{ -0.02f, 0.005f, 0.012f }
		};

		private float[,] DrinkRotations = new float[,]
		{
			{ 5f, 140f, 295f },
			{ 5f, 140f, 295f },
			{ 350f, 190f, 210f },
			{ 310f, 150f, 273f },
			{ 310f, 150f, 273f },
			{ 308f, 147f, 295f },
			{ 308f, 147f, 295f },
			{ 308f, 147f, 295f },
			{ 310f, 150f, 273f }
		};

		public bool pauseAnimations;

		private bool isRunning;

		public float aimRot;

		private PlayerAnimManager.StanceId currentStance;

		private byte currentDrinkId = byte.MaxValue;

		private GameObject hookHit;

		public enum AnimationId
		{
			Walk,
			Standing,
			Jumping,
			Drunk,
			Leaning,
			Finger,
			Hitchhike,
			Crouching,
			CrouchingLow,
			CrouchingWalk,
			CrouchingLowWalk,
			Running,
			Hitting,
			Pushing,
			Drinking
		}

		private enum StanceId
		{
			Standing,
			Crouching,
			CrouchingLow
		}

		public enum HandStateId
		{
			MiddleFingering,
			Lifting,
			Hitting,
			Pushing,
			Drinking
		}

		public class AnimState : PlayerAnimManager
		{
			public void Initialize(GameObject characterGameObject, Animation characterAnimationComponent)
			{
				this.characterGameObject = characterGameObject;
				this.characterAnimationComponent = characterAnimationComponent;
			}

			public virtual bool CanActivate(AnimSyncMessage msg)
			{
				return false;
			}

			public virtual void Activate()
			{
			}

			public virtual void Deactivate()
			{
			}

			public void TryActivate(AnimSyncMessage msg)
			{
				bool flag = this.CanActivate(msg);
				if (!this.isActive && flag)
				{
					this.Activate();
					this.isActive = true;
					return;
				}
				if (this.isActive && !flag)
				{
					this.Deactivate();
					this.isActive = false;
				}
			}

			public AnimState()
			{
			}

			public bool isActive;
		}

		private class LeaningState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return msg.isLeaning;
			}

			public override void Activate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Leaning, true);
			}

			public override void Deactivate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Leaning, false);
			}

			public LeaningState()
			{
			}
		}

		private class JumpState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return !msg.isGrounded;
			}

			public override void Activate()
			{
				base.PlayAnimation(PlayerAnimManager.AnimationId.Jumping, true, true);
			}

			public override void Deactivate()
			{
				base.BlendOutAnimation(PlayerAnimManager.AnimationId.Jumping);
			}

			public JumpState()
			{
			}
		}

		private class FingerState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return base.GetHandState(msg.activeHandState) == PlayerAnimManager.HandStateId.MiddleFingering;
			}

			public override void Activate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Finger, true);
			}

			public override void Deactivate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Finger, false);
			}

			public FingerState()
			{
			}
		}

		private class HitchhikeState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return base.GetHandState(msg.activeHandState) == PlayerAnimManager.HandStateId.Lifting;
			}

			public override void Activate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Hitchhike, true);
			}

			public override void Deactivate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Hitchhike, false);
			}

			public HitchhikeState()
			{
			}
		}

		private class DrunkState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return msg.isDrunk;
			}

			public override void Activate()
			{
				base.PlayAnimation(PlayerAnimManager.AnimationId.Drunk, false, false);
			}

			public override void Deactivate()
			{
				base.BlendOutAnimation(PlayerAnimManager.AnimationId.Drunk);
			}

			public DrunkState()
			{
			}
		}

		private class HitState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return base.GetHandState(msg.activeHandState) == PlayerAnimManager.HandStateId.Hitting;
			}

			public override void Activate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Hitting, true);
			}

			public override void Deactivate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Hitting, false);
			}

			public HitState()
			{
			}
		}

		private class PushState : PlayerAnimManager.AnimState
		{
			public override bool CanActivate(AnimSyncMessage msg)
			{
				return base.GetHandState(msg.activeHandState) == PlayerAnimManager.HandStateId.Pushing;
			}

			public override void Activate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Pushing, true);
			}

			public override void Deactivate()
			{
				base.PlayActionAnim(PlayerAnimManager.AnimationId.Pushing, false);
			}

			public PushState()
			{
			}
		}
	}
}
