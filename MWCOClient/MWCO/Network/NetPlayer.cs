using System;
using Boo.Lang;
using MWCO.Game;
using MWCO.Game.Components;
using MWCO.Game.Objects;
using MWCO.Math;
using MWCO.Network.Messages;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Network
{
	internal class NetPlayer : IDisposable
	{
		public CSteamID SteamId
		{
			get
			{
				return this.steamId;
			}
		}

		public bool IsSpawned
		{
			get
			{
				return this.characterGameObject != null;
			}
		}

		public void SnapPlayerToGrid()
		{
			Vector3 localPosition = this.characterGameObject.transform.localPosition;
			float num = this.FindClosestValue(localPosition.x, this.xGrid);
			float num2 = this.FindClosestValue(localPosition.z, this.zGrid);
			this.characterGameObject.transform.localPosition = new Vector3(num, 1.54f, num2);
		}

		private float FindClosestValue(float currentValue, float[] values)
		{
			float num = values[0];
			float num2 = Mathf.Abs(currentValue - num);
			foreach (float num3 in values)
			{
				float num4 = Mathf.Abs(currentValue - num3);
				if (num4 < num2)
				{
					num = num3;
					num2 = num4;
				}
			}
			return num;
		}

		private float[] GenerateZGrid(float start, float end, float gap)
		{
			int num = Mathf.CeilToInt(Mathf.Abs(end - start) / gap) + 1;
			float[] array = new float[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = start + (float)i * Mathf.Sign(end - start) * gap;
			}
			return array;
		}

		public NetPlayer(NetManager netManager, NetWorld netWorld, CSteamID steamId)
		{
			this.netManager = netManager;
			this.netWorld = netWorld;
			this.steamId = steamId;
			if (this.animManager == null)
			{
				this.animManager = new PlayerAnimManager();
				string text = "AnimManager: CREATED ANIMATIONS! ";
				GameObject gameObject = this.characterGameObject;
				Logger.Debug(text + ((gameObject != null) ? gameObject.name : null));
			}
			this.zGrid = this.GenerateZGrid(this.zStart, this.zEnd, this.zGap);
			this.INTERPOLATION_TIME = 100UL;
		}

		public void Spawn()
		{
			Logger.Debug(string.Concat(new string[]
			{
				"[NetPlayer] Spawning player ",
				this.GetName(),
				" now (",
				Application.loadedLevelName,
				")"
			}));
			this.loadedModel = Client.LoadAsset<GameObject>("Assets/MPPlayerModel/MPPlayerModel.prefab");
			this.characterGameObject = (GameObject)Object.Instantiate(this.loadedModel, this.interpolator.CurrentPosition, this.interpolator.CurrentRotation);
			CapsuleCollider capsuleCollider = this.characterGameObject.AddComponent<CapsuleCollider>();
			capsuleCollider.radius = 0.1f;
			capsuleCollider.height = 0.55f;
			capsuleCollider.isTrigger = true;
			if (this.animManager == null)
			{
				this.animManager = new PlayerAnimManager();
				Logger.Debug("AnimManager: CREATED ANIMS LATE!");
			}
			else
			{
				string text = "AnimManager: We had already (from NetLocal) our animManager ";
				GameObject gameObject = this.characterGameObject;
				Logger.Debug(text + ((gameObject != null) ? gameObject.name : null));
			}
			this.animManager.SetupAnimations(this.characterGameObject);
			this.SetupIK();
			if (this.pickedUpObjectNetId != 65535)
			{
				this.UpdatePickedUpObject(true, false);
			}
			if (this.currentVehicle != null)
			{
				this.SitInCurrentVehicle();
			}
			this.head = this.characterGameObject.transform.FindChild("pelvis/spine_mid/shoulders/head");
			this.LoadHatsAndShades();
			this.ApplySkin();
			this.SetupPissParticles();
			this.SetupCigarette();
		}

		private void LoadHatsAndShades()
		{
			if (this.characterGameObject == null)
			{
				return;
			}
			Transform child = this.head.GetChild(1);
			for (int i = 0; i < child.childCount; i++)
			{
				this.Hats.Add(child.GetChild(i).gameObject);
			}
			Transform child2 = this.head.GetChild(2);
			for (int j = 0; j < child2.childCount; j++)
			{
				this.Shades.Add(child2.GetChild(j).gameObject);
			}
			Transform child3 = this.head.parent.GetChild(3);
			for (int k = 0; k < child3.childCount; k++)
			{
				this.Extras.Add(child3.GetChild(k).gameObject);
			}
			this.phoneObject = this.head.GetChild(3).GetChild(0).GetChild(0)
				.gameObject;
			Transform child4 = this.head.GetChild(3).GetChild(1);
			for (int l = 0; l < 3; l++)
			{
				this.nozzleObjects[l] = child4.GetChild(l).GetChild(0).gameObject;
			}
		}

		public void SetupPissParticles()
		{
			try
			{
				if (this.PissParticles == null && this.head != null)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(MPController.Instance.FluidObj);
					this.PissParticles = gameObject.GetComponent<EllipsoidParticleEmitter>();
					gameObject.transform.SetParent(this.head);
					this.PissParticles.transform.localPosition = new Vector3(0.05f, 0.17f, -0.02f);
					this.PissParticles.transform.localEulerAngles = new Vector3(0f, 0f, 235f);
				}
			}
			catch
			{
				Logger.Debug("Failed to add piss");
			}
			if (this.PissHandTarget == null && this.head != null)
			{
				GameObject gameObject2 = GameObject.CreatePrimitive(0);
				this.PissHandTarget = gameObject2.transform;
				this.PissHandTarget.SetParent(this.characterGameObject.transform);
				this.PissHandTarget.localPosition = new Vector3(-0.02f, 0.035f, 0.04f);
				this.PissHandTarget.localRotation = Quaternion.identity;
				gameObject2.name = "PissHandTarget";
				gameObject2.GetComponent<SphereCollider>().enabled = false;
				gameObject2.GetComponent<MeshRenderer>().enabled = false;
			}
		}

		public void SetupCigarette()
		{
			if (this.Cigarette == null && this.head != null)
			{
				this.Cigarette = Object.Instantiate<GameObject>(MPController.Instance.CigaretteObj);
				string text = "Cigarette of ";
				string name = this.Cigarette.transform.name;
				string text2 = " to ";
				Transform transform = this.head;
				Logger.Debug(text + name + text2 + ((transform != null) ? transform.name : null));
				this.Cigarette.transform.SetParent(this.head);
				this.Cigarette.transform.localPosition = new Vector3(0.046f, 0.13f, -0.07f);
				this.Cigarette.transform.localEulerAngles = new Vector3(285f, 180f, 180f);
				this.Cigarette.SetActive(false);
				GameObject gameObject = Object.Instantiate<GameObject>(MPController.Instance.BreathSmokeObj);
				this.SmokeParticles = gameObject.GetComponent<EllipsoidParticleEmitter>();
				this.SmokeParticles.transform.SetParent(this.head);
				this.SmokeParticles.transform.localPosition = new Vector3(-0.01f, 0.04f, 0f);
				this.SmokeParticles.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
				this.SmokeParticles.emit = true;
			}
			if (this.CigaretteHandTarget == null && this.Cigarette != null)
			{
				GameObject gameObject2 = GameObject.CreatePrimitive(0);
				this.CigaretteHandTarget = gameObject2.transform;
				this.CigaretteHandTarget.SetParent(this.Cigarette.transform);
				this.CigaretteHandTarget.localPosition = new Vector3(-0.01f, 0.07f, -0.12f);
				this.CigaretteHandTarget.localEulerAngles = Vector3.zero;
				gameObject2.name = "CigaretteHandTarget";
				gameObject2.GetComponent<SphereCollider>().enabled = false;
				gameObject2.GetComponent<MeshRenderer>().enabled = false;
			}
		}

		private void SetupIK()
		{
			if (this.leftArmIK == null)
			{
				this.leftArmIK = this.characterGameObject.AddComponent<IKManager>();
				this.leftArmIK.uppperArm_OffsetRotation = new Vector3(200f, 90f, 0f);
				this.leftArmIK.forearm_OffsetRotation = new Vector3(120f, 90f, 0f);
				this.leftArmIK.upperArm = this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.transform;
				this.leftArmIK.forearm = this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.transform;
				this.leftArmIK.hand = this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.transform;
			}
			if (this.rightArmIK == null)
			{
				this.rightArmIK = this.characterGameObject.AddComponent<IKManager>();
				this.rightArmIK.uppperArm_OffsetRotation = new Vector3(90f, 90f, 0f);
				this.rightArmIK.forearm_OffsetRotation = new Vector3(90f, 90f, 0f);
				this.rightArmIK.upperArm = this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
					.GetChild(1)
					.GetChild(0)
					.transform;
				this.rightArmIK.forearm = this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
					.GetChild(1)
					.GetChild(0)
					.GetChild(0)
					.transform;
				this.rightArmIK.hand = this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
					.GetChild(1)
					.GetChild(0)
					.GetChild(0)
					.GetChild(0)
					.transform;
			}
			GameObject gameObject = GameObject.CreatePrimitive(0);
			gameObject.transform.SetParent(this.characterGameObject.transform.GetChild(1));
			gameObject.transform.position = this.characterGameObject.transform.position;
			gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			gameObject.transform.localPosition = new Vector3(-0.1f, -0.07f, 0.12f);
			gameObject.transform.GetComponent<SphereCollider>().isTrigger = true;
			gameObject.GetComponent<MeshRenderer>().enabled = false;
			this.leftArmIK.elbow = gameObject.transform;
			GameObject gameObject2 = GameObject.CreatePrimitive(0);
			gameObject2.transform.SetParent(this.characterGameObject.transform.GetChild(1));
			gameObject2.transform.position = this.characterGameObject.transform.position;
			gameObject2.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			gameObject2.transform.localPosition = new Vector3(-0.095f, -0.07f, -0.12f);
			gameObject2.transform.GetComponent<SphereCollider>().isTrigger = true;
			gameObject2.GetComponent<MeshRenderer>().enabled = false;
			this.rightArmIK.elbow = gameObject2.transform;
			this.DisableIK();
		}

		private void EnableIK()
		{
			this.leftArmIK.enabled = true;
			this.rightArmIK.enabled = true;
		}

		private void DisableIK()
		{
			try
			{
				this.leftArmIK.enabled = false;
				this.rightArmIK.enabled = false;
				this.leftArmIK.target = null;
				this.rightArmIK.target = null;
			}
			catch
			{
			}
		}

		public void ApplySittingPose()
		{
			this.characterGameObject.transform.localEulerAngles = Vector3.zero;
			if (this.vehicleParent == "CORRIS")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.36f, 0.68f, -0.03f);
				}
				else if (this.state == NetPlayer.State.Passenger)
				{
					this.characterGameObject.transform.localPosition = new Vector3(0.34f, 0.71f, -0.03f);
				}
			}
			else if (this.vehicleParent == "SORBET(190-200psi)")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.367f, 0.6934f, 0.046f);
				}
			}
			else if (this.vehicleParent == "MACHTWAGEN")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.3532f, 0.6885f, -0.08128f);
				}
				else if (this.state == NetPlayer.State.Passenger)
				{
					this.characterGameObject.transform.localPosition = new Vector3(0.36f, 0.7f, -0.04f);
				}
			}
			else if (this.vehicleParent == "BACHGLOTZ(1905kg)")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.444f, 0.796f, -0.02f);
				}
			}
			else if (this.vehicleParent == "GIFU(750/450psi)")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.725f, 2.25f, 2.78f);
				}
				else if (this.state == NetPlayer.State.Passenger)
				{
					this.characterGameObject.transform.localPosition = new Vector3(0.682f, 2.25f, 2.78f);
				}
			}
			else if (this.vehicleParent == "KEKMET(350-400psi)")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.03f, 1.65f, -0.704f);
				}
				else if (this.state == NetPlayer.State.Passenger)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.005757f, 2.685845f, -0.3687656f);
				}
			}
			else if (this.vehicleParent == "JONNEZ ES(Clone)")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(0f, 1.16f, -0.25f);
				}
			}
			else if (this.vehicleParent == "Panier250")
			{
				if (this.state == NetPlayer.State.DrivingVehicle)
				{
					this.characterGameObject.transform.localPosition = new Vector3(-0.294f, 1.1325f, -0.0706f);
				}
				else if (this.state == NetPlayer.State.Passenger)
				{
					Vector3 localPosition = this.characterGameObject.transform.localPosition;
					localPosition.y -= 0.05f;
					this.characterGameObject.transform.localPosition = localPosition;
				}
			}
			this.characterGameObject.transform.GetChild(1).localEulerAngles = new Vector3(4f, 275f, 332f);
			this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
				.localEulerAngles = new Vector3(0.5f, 0f, 356f);
			if (this.state == NetPlayer.State.DrivingVehicle)
			{
				if (this.vehicleParent == "BACHGLOTZ(1905kg)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(23.5f, 55f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 15f, 289f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(346f, 314f, 318f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(5f, 8f, 309f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 52f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 88f, 336f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 70f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else if (this.vehicleParent == "GIFU(750/450psi)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(23.5f, 55f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(335f, 352f, 296f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(343f, 301f, 325f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 350f, 271f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 88f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 96f, 343f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 85.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else if (this.vehicleParent == "JONNEZ ES(Clone)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(22f, 102.5f, 8.47f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(23.5f, 55f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(324f, 348f, 291f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(56f, 335f, 322f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.localEulerAngles = new Vector3(342.5f, 267.7f, 2.2f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(338f, 301f, 322f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(18f, 350f, 278f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(282f, 17.4f, 347.4f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(7f, 66.5f, 0f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 128f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(353f, 98f, 19f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(4f, 0f, 125.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
					this.characterGameObject.transform.GetChild(1).localEulerAngles = new Vector3(4f, 275f, 326f);
				}
				else if (this.vehicleParent == "KEKMET(350-400psi)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 7f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 19f, 311f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(340f, 310f, 6f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 344f, 311f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 75.5f, 10f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 52f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(0f, 112f, 0f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 70f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(23.5f, 55f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 15f, 289f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(338f, 314f, 318f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 350f, 271f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 52f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 88f, 336f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 70f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				this.defaultLegPos1 = this.characterGameObject.transform.GetChild(3).localEulerAngles;
				this.defaultLegPos2 = this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles;
			}
			else if (this.state == NetPlayer.State.Passenger)
			{
				if (this.vehicleParent == "RCO_RUSCKO12(270)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 309f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(16f, 267f, 298f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(334f, 316f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(354f, 58f, 278f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 324f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 102f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(22.1f, 358f, 283f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 90f, 329f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 79.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 256f);
				}
				else if (this.vehicleParent == "GIFU(750/450psi)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 309f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(16f, 267f, 298f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(334f, 316f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(354f, 58f, 278f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 88f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 96f, 353f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 69.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else if (this.currentVehicle.Name == "BUS")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 325f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 15f, 289f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(328f, 310f, 330f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 350f, 271f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(358f, 358f, 117f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 90f, 342f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 119.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else if (this.vehicleParent == "KEKMET(350-400psi)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 7f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 19f, 311f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(340f, 310f, 6f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 344f, 311f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(359f, 72.5f, 8f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 52f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 104f, 11f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 39.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 325f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 15f, 289f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(328f, 310f, 330f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 350f, 271f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 52f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 90f, -9f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 39.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
			}
			else if (this.state == NetPlayer.State.Backseater)
			{
				if (this.vehicleParent == "RCO_RUSCKO12(270)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 309f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(16f, 267f, 298f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(334f, 316f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(354f, 58f, 278f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 324f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 102f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(22.1f, 358f, 283f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 90f, 329f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 79.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 256f);
				}
				else if (this.vehicleParent == "GIFU(750/450psi)")
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(23.5f, 55f, 316f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(335f, 342f, 261f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(338f, 301f, 335f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 350f, 271f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 346f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 88f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(2.1f, 358f, 256f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 96f, 353f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 69.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 252f);
				}
				else
				{
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(33.5f, 55f, 325f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(333f, 15f, 289f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.localEulerAngles = new Vector3(328f, 310f, 330f);
					this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
						.GetChild(1)
						.GetChild(0)
						.GetChild(0)
						.localEulerAngles = new Vector3(15f, 350f, 271f);
					this.characterGameObject.transform.GetChild(2).localEulerAngles = new Vector3(345f, 82.5f, 324f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).localEulerAngles = new Vector3(353f, 358f, 102f);
					this.characterGameObject.transform.GetChild(2).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(22.1f, 358f, 283f);
					this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(11f, 90f, 329f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(354f, 0f, 79.5f);
					this.characterGameObject.transform.GetChild(3).GetChild(0).GetChild(0)
						.localEulerAngles = new Vector3(357f, 4.2f, 256f);
					this.characterGameObject.transform.localPosition = new Vector3(this.characterGameObject.transform.localPosition.x, 0.62f, this.characterGameObject.transform.localPosition.z);
				}
			}
			this.characterGameObject.transform.GetChild(1).GetChild(0).GetChild(0)
				.GetChild(2)
				.localEulerAngles = new Vector3(0f, 5f, 332f);
			if (this.state == NetPlayer.State.DrivingVehicle && this.currentVehicle != null)
			{
				this.EnableIK();
				PlayerVehicle playerVehicle = (PlayerVehicle)this.currentVehicle.syncedObject;
				if (playerVehicle.steeringIK != null)
				{
					this.leftArmIK.target = playerVehicle.steeringIK;
				}
				else
				{
					this.leftArmIK.enabled = false;
				}
				if (playerVehicle.gearstickIK != null && this.vehicleParent != "FERNDALE(1630kg)")
				{
					this.rightArmIK.target = playerVehicle.gearstickIK;
					return;
				}
				this.rightArmIK.enabled = false;
			}
		}

		public void FootDown()
		{
			if (this.state == NetPlayer.State.DrivingVehicle && !this.footdown && this.currentVehicle != null)
			{
				Vector3 localEulerAngles = this.characterGameObject.transform.GetChild(3).localEulerAngles;
				Vector3 localEulerAngles2 = this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles;
				this.characterGameObject.transform.GetChild(3).localEulerAngles = new Vector3(localEulerAngles.x, localEulerAngles.y, localEulerAngles.z + 13f);
				this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = new Vector3(localEulerAngles2.x, localEulerAngles2.y, localEulerAngles2.z - 26.5f);
				this.footdown = true;
			}
		}

		public void FootUp()
		{
			if (this.state == NetPlayer.State.DrivingVehicle && this.footdown)
			{
				if (this.currentVehicle != null)
				{
					this.characterGameObject.transform.GetChild(3).localEulerAngles = this.defaultLegPos1;
					this.characterGameObject.transform.GetChild(3).GetChild(0).localEulerAngles = this.defaultLegPos2;
				}
				this.footdown = false;
			}
		}

		public void MenuSpawn()
		{
			this.loadedModel = Client.LoadAsset<GameObject>("Assets/MPPlayerModel/MPPlayerModel.prefab");
			this.characterGameObject = (GameObject)Object.Instantiate(this.loadedModel, this.interpolator.CurrentPosition, this.interpolator.CurrentRotation);
			this.head = this.characterGameObject.transform.FindChild("pelvis/spine_mid/shoulders/head");
			this.LoadHatsAndShades();
		}

		public void SetSkin(string skinCode)
		{
			this.skin = skinCode;
			try
			{
				this.ApplySkin();
			}
			catch (Exception ex)
			{
				string text = "Failed to apply skin: ";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		public void ApplySkin()
		{
			SkinnedMeshRenderer component = this.characterGameObject.transform.Find("bodymesh").gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component != null)
			{
				string[] array = this.skin.Split(new char[] { ',' });
				int num = int.Parse(array[0]);
				int num2 = int.Parse(array[1]);
				int num3 = int.Parse(array[2]);
				int num4 = int.Parse(array[3]);
				int num5 = int.Parse(array[4]);
				int num6 = int.Parse(array[5]);
				Texture2D texture2D = Client.LoadAsset<Texture2D>(string.Format("Assets/Skins/{0}/Textures/MPchar_face03.dds", num));
				Texture2D texture2D2 = Client.LoadAsset<Texture2D>(string.Format("Assets/Skins/{0}/Textures/MPchar_shirt06.dds", num2));
				Texture2D texture2D3 = Client.LoadAsset<Texture2D>(string.Format("Assets/Skins/{0}/Textures/MPchar_pants03.dds", num3));
				Material[] materials = component.materials;
				materials[0].SetTexture("_MainTex", texture2D2);
				materials[1].SetTexture("_MainTex", texture2D3);
				materials[2].SetTexture("_MainTex", texture2D);
				component.materials = materials;
				this.ApplyHat(num4);
				this.ApplyShades(num5);
				this.ApplyExtras(num6);
			}
		}

		public void Dispose()
		{
			if (this.currentVehicle != null)
			{
				this.LeaveVehicle();
			}
			if (this.characterGameObject != null)
			{
				Object.Destroy(this.characterGameObject);
				this.characterGameObject = null;
			}
		}

		public void GiveHat(string hatModelPath)
		{
			GameObject gameObject = Client.LoadAsset<GameObject>(hatModelPath);
			this.hatGo = (GameObject)Object.Instantiate(gameObject, this.characterGameObject.transform.FindChild("pelvis").FindChild("spine_mid").FindChild("shoulders")
				.FindChild("head")
				.transform.position, this.characterGameObject.transform.FindChild("pelvis").FindChild("spine_mid").FindChild("shoulders")
				.FindChild("head")
				.transform.rotation);
			this.hatGo.transform.SetParent(this.head);
			this.hatGo.transform.localPosition = this.HAT_OFFSET;
			if (this.hatGo.name.StartsWith("cowboy_hat"))
			{
				this.hatGo.transform.localRotation = Quaternion.Euler(12f, 270f, 90f);
			}
			else if (this.hatGo.name.StartsWith("santa_hat"))
			{
				this.hatGo.transform.localRotation = Quaternion.Euler(290f, 90f, 0f);
				this.hatGo.transform.localPosition = new Vector3(-0.05f, -0.004f, -0.009f);
			}
			else
			{
				this.hatGo.transform.localRotation = Quaternion.Euler(6f, 270f, 90f);
			}
			this.hasHat = true;
		}

		public void ApplyShades(int id)
		{
			foreach (GameObject gameObject in this.Shades)
			{
				gameObject.SetActive(false);
			}
			if (id != 0)
			{
				this.Shades[id - 1].SetActive(true);
			}
		}

		public void ApplyHat(int id)
		{
			foreach (GameObject gameObject in this.Hats)
			{
				gameObject.SetActive(false);
			}
			if (id != 0)
			{
				this.Hats[id - 1].SetActive(true);
			}
		}

		public void ApplyExtras(int id)
		{
			foreach (GameObject gameObject in this.Extras)
			{
				gameObject.SetActive(false);
			}
			if (id != 0)
			{
				this.Extras[id - 1].SetActive(true);
			}
		}

		public bool SendPacket(byte[] data, EP2PSend sendType, int channel = 0)
		{
			return SteamNetworking.SendP2PPacket(this.steamId, data, (uint)data.Length, sendType, channel);
		}

		public virtual void Update()
		{
			if (!this.IsSpawned)
			{
				return;
			}
			if (this.timerIsRunning)
			{
				if (this.timeRemaining <= 0f)
				{
					this.characterGameObject.GetComponent<Animation>().enabled = false;
					this.ApplySittingPose();
					this.timeRemaining = 0f;
					this.timerIsRunning = false;
					this.isSitting = true;
					this.halfSit = false;
				}
				else if (this.timeRemaining <= 0.5f && !this.halfSit)
				{
					this.animManager.DisableAnimationComponent();
					this.timeRemaining -= Time.deltaTime;
					this.halfSit = true;
				}
				else
				{
					this.timeRemaining -= Time.deltaTime;
				}
			}
			try
			{
				if (this.timeSincePickup > 0f)
				{
					this.timeSincePickup -= Time.deltaTime;
				}
				else if (this.rightArmIK.target != null)
				{
					(this.rightArmIK.target.gameObject.GetComponent<ObjectSyncComponent>().syncedObject as Pickupable).EnableParams();
					this.pickedUpObject = null;
					this.rightArmIK.target = null;
				}
			}
			catch
			{
			}
			this.HandleSmoking();
			if (!this.isLoadedIntoWorld && Application.loadedLevelName != "MainMenu" && this.characterGameObject.transform.position != Vector3.zero)
			{
				GameHouseDatabase.Instance.channel1Fsm.SendEvent("MP_Play radio 2");
				this.isLoadedIntoWorld = true;
			}
			if (this.characterGameObject && this.syncReceiveTime > 0UL)
			{
				float num = (this.netManager.GetNetworkClock() - this.syncReceiveTime) / this.INTERPOLATION_TIME;
				float num2 = 0f;
				if (num <= 2f)
				{
					if (this.isSitting)
					{
						float num3 = Mathf.DeltaAngle(this.aimRot, this.newRot);
						this.newRot = this.aimRot + num3;
						this.aimRot = Mathf.Lerp(this.aimRot, this.newRot, Mathf.Clamp01(num));
						num3 = Mathf.DeltaAngle(this.aimPitch, this.newPitch);
						this.newPitch = this.aimPitch + num3;
						this.aimPitch = Mathf.Lerp(this.aimPitch, this.newPitch, Mathf.Clamp01(num));
						this.SyncVerticalHeadLook();
						return;
					}
					Vector3 currentPosition = this.interpolator.CurrentPosition;
					Vector3 zero = Vector3.zero;
					Quaternion identity = Quaternion.identity;
					this.interpolator.Evaluate(ref zero, ref identity, num);
					Vector3 vector = zero - currentPosition;
					vector.y = 0f;
					num2 = vector.magnitude;
					this.UpdateCharacterPosition();
					float num4 = Mathf.DeltaAngle(this.aimRot, this.newRot);
					this.newRot = this.aimRot + num4;
					this.aimRot = Mathf.Lerp(this.aimRot, this.newRot, Mathf.Clamp01(num));
					this.SyncVerticalHeadLook();
				}
				if (this.animManager != null)
				{
					this.animManager.HandleOnFootMovementAnimations(num2);
					this.animManager.CheckBlendedOutAnimationStates();
				}
				if (this.currentVehicle != null && this.characterGameObject.transform.localEulerAngles != Vector3.zero)
				{
					this.SitInCurrentVehicle();
					return;
				}
				if (this.currentVehicle == null && this.characterGameObject.transform.parent != null)
				{
					this.LeaveVehicle();
				}
			}
		}

		public void HoldPhone(bool hold)
		{
			this.leftArmIK.enabled = hold;
			this.phoneObject.transform.parent.gameObject.SetActive(hold);
			this.leftArmIK.target = (hold ? this.phoneObject.transform : null);
			this.leftArmIK.forearm_OffsetRotation = new Vector3(hold ? 40f : 120f, 90f, 0f);
			if (!hold && this.currentVehicle != null && this.state == NetPlayer.State.DrivingVehicle)
			{
				PlayerVehicle playerVehicle = this.currentVehicle.GetObjectSubtype() as PlayerVehicle;
				if (playerVehicle != null && playerVehicle.ParentGameObject.name == "MACHTWAGEN")
				{
					this.leftArmIK.enabled = true;
					this.leftArmIK.target = playerVehicle.steeringIK;
				}
			}
		}

		public void HoldNozzle(bool hold, string type = "")
		{
			int num = (type.Contains("Gasoline") ? 0 : (type.Contains("Diesel") ? 1 : 2));
			this.nozzleObjects[num].transform.parent.gameObject.SetActive(hold);
			this.leftArmIK.enabled = true;
			this.leftArmIK.target = this.nozzleObjects[num].transform;
			this.leftArmIK.forearm_OffsetRotation = new Vector3(40f, 90f, 0f);
		}

		public void DropNozzle()
		{
			bool flag = false;
			foreach (GameObject gameObject in this.nozzleObjects)
			{
				if (gameObject.transform.parent.gameObject.activeSelf)
				{
					flag = true;
					gameObject.transform.parent.gameObject.SetActive(false);
				}
			}
			if (flag)
			{
				if (this.state != NetPlayer.State.DrivingVehicle)
				{
					this.leftArmIK.enabled = false;
					this.leftArmIK.target = null;
				}
				this.leftArmIK.forearm_OffsetRotation = new Vector3(120f, 90f, 0f);
			}
		}

		private void SyncVerticalHeadLook()
		{
			if (!this.isSitting)
			{
				this.head.rotation *= Quaternion.Euler(0f, 0f, -this.aimRot);
				return;
			}
			if ((this.aimRot < 270f && this.aimRot > 180f) || this.aimRot < -90f)
			{
				this.aimRot = 270f;
			}
			else if ((this.aimRot > 90f && this.aimRot < 180f) || this.aimRot > 450f)
			{
				this.aimRot = 90f;
			}
			this.head.localRotation = Quaternion.Euler(-this.aimRot, 5f, -this.aimPitch);
		}

		public void DrawNametag()
		{
			if (this.characterGameObject != null)
			{
				Vector3 vector = Camera.main.WorldToScreenPoint(this.characterGameObject.transform.position + Vector3.up * 1.3f);
				if (vector.z > 0f)
				{
					GUI.skin.label.fontSize = 15;
					float num = 180f;
					vector.x -= num / 2f;
					if (this.accesslevel == 4)
					{
						GUI.color = Color.black;
						GUI.Label(new Rect(vector.x + 1f, (float)Screen.height - vector.y + 31f, num, 40f), "♛ " + this.GetName());
						GUI.color = Color.yellow;
						GUI.Label(new Rect(vector.x, (float)Screen.height - vector.y + 30f, num, 40f), "♛ " + this.GetName());
						return;
					}
					GUI.color = Color.black;
					GUI.Label(new Rect(vector.x, (float)Screen.height - vector.y + 31f, num, 40f), this.GetName());
					GUI.color = Color.cyan;
					GUI.Label(new Rect(vector.x, (float)Screen.height - vector.y + 30f, num, 40f), this.GetName());
				}
			}
		}

		public void HandleSynchronize(PlayerSyncMessage msg)
		{
			if (msg.HasPissRate)
			{
				if (msg.PissRate > 0f)
				{
					if (!this.leftArmIK.enabled)
					{
						this.leftArmIK.enabled = true;
						this.leftArmIK.target = this.PissHandTarget;
					}
					this.PissParticles.maxEmission = msg.PissRate;
				}
				else
				{
					if (this.leftArmIK.target == this.PissHandTarget)
					{
						this.leftArmIK.enabled = false;
						this.leftArmIK.target = null;
					}
					this.PissParticles.maxEmission = 0f;
				}
			}
			if (msg.HasSmoking)
			{
				if (msg.Smoking > 0)
				{
					if (!this.Cigarette.activeSelf)
					{
						this.Cigarette.SetActive(true);
					}
					if (!this.rightArmIK.enabled)
					{
						this.rightArmIK.enabled = true;
						this.rightArmIK.target = this.CigaretteHandTarget;
					}
					if (msg.Smoking == 1)
					{
						if (this.inhaling)
						{
							Logger.Debug("[NetPlayer] OUTHALING DETECTED");
							this.inhaling = false;
							this.inhaleTimer /= 1.5f;
						}
					}
					else if (msg.Smoking == 2 && !this.inhaling)
					{
						this.inhaling = true;
						this.inhaleTimer = 0f;
					}
				}
				else if (this.rightArmIK.target == this.CigaretteHandTarget)
				{
					this.rightArmIK.enabled = false;
					this.rightArmIK.target = null;
				}
				else if (this.SmokeParticles.maxEmission > 0f)
				{
					this.SmokeParticles.maxEmission = 0f;
				}
				this.smoking = msg.Smoking > 0;
				if (this.Cigarette.activeSelf && msg.Smoking == 0)
				{
					this.Cigarette.SetActive(false);
				}
			}
			if (this.currentVehicle != null)
			{
				if (!(Utils.NetVec3ToGame(msg.position) != Vector3.zero))
				{
					if (this.isSitting)
					{
						if (this.state == NetPlayer.State.Passenger || this.state == NetPlayer.State.Backseater)
						{
							this.newRot = msg.aimYaw;
							this.newPitch = msg.aimPitch;
						}
						else
						{
							this.newRot = msg.aimYaw;
							this.newPitch = msg.aimPitch;
						}
						this.syncReceiveTime = this.netManager.GetNetworkClock();
					}
					return;
				}
				this.LeaveVehicle();
			}
			if (this.state != NetPlayer.State.OnFoot)
			{
				Chat.Instance.AddMessage("Received on foot update but player is not on foot.", MessageSeverity.Info);
			}
			Vector3 vector = Utils.NetVec3ToGame(msg.position);
			Quaternion quaternion = Utils.NetQuatToGame(msg.rotation);
			this.interpolator.SetTarget(vector, quaternion);
			this.newRot = msg.aimPitch;
			this.syncReceiveTime = this.netManager.GetNetworkClock();
			if (msg.HasPlayerAttributes)
			{
				this.playerAttributes = msg.PlayerAttributes;
			}
			if (!this.IsSpawned)
			{
				this.Teleport(vector, quaternion);
				return;
			}
		}

		public void HandleAnimSynchronize(AnimSyncMessage msg)
		{
			PlayerAnimManager playerAnimManager = this.animManager;
			if (playerAnimManager != null)
			{
				playerAnimManager.HandleAnimations(msg, this.characterGameObject);
			}
			if (msg.pissRate > 0f)
			{
				if (!this.leftArmIK.enabled)
				{
					this.leftArmIK.enabled = true;
					this.leftArmIK.target = this.PissHandTarget;
				}
				this.PissParticles.maxEmission = msg.pissRate;
			}
			else
			{
				if (this.leftArmIK.target == this.PissHandTarget)
				{
					this.leftArmIK.enabled = false;
					this.leftArmIK.target = null;
				}
				this.PissParticles.maxEmission = 0f;
			}
			if (msg.smoking > 0)
			{
				if (!this.Cigarette.activeSelf)
				{
					this.Cigarette.SetActive(true);
				}
				if (!this.rightArmIK.enabled)
				{
					this.rightArmIK.enabled = true;
					this.rightArmIK.target = this.CigaretteHandTarget;
				}
				if (msg.smoking == 1)
				{
					if (this.inhaling)
					{
						Logger.Debug("[NetPlayer] OUTHALING DETECTED");
						this.inhaling = false;
						this.inhaleTimer /= 1.5f;
					}
				}
				else if (msg.smoking == 2 && !this.inhaling)
				{
					this.inhaling = true;
					this.inhaleTimer = 0f;
				}
			}
			else if (this.rightArmIK.target == this.CigaretteHandTarget)
			{
				this.rightArmIK.enabled = false;
				this.rightArmIK.target = null;
			}
			else if (this.SmokeParticles.maxEmission > 0f)
			{
				this.SmokeParticles.maxEmission = 0f;
			}
			this.smoking = msg.smoking > 0;
			if (this.Cigarette.activeSelf && msg.smoking == 0)
			{
				this.Cigarette.SetActive(false);
			}
		}

		private void HandleSmoking()
		{
			if (!this.smoking)
			{
				return;
			}
			if (this.inhaling)
			{
				this.inhaleTimer += Time.deltaTime;
			}
			else if (this.inhaleTimer >= 0f)
			{
				Logger.Debug("Outhaling " + this.inhaleTimer.ToString());
				this.SmokeParticles.maxEmission = 8f;
				this.inhaleTimer -= Time.deltaTime;
			}
			else
			{
				this.SmokeParticles.maxEmission = 0f;
			}
			this.SetCigarettePos(this.inhaling);
		}

		private void SetCigarettePos(bool inhaling)
		{
			if (this.currentVehicle != null && this.state == NetPlayer.State.DrivingVehicle)
			{
				this.Cigarette.transform.localPosition = new Vector3(-0.014f, 0.03f, -0.012f);
				this.CigaretteHandTarget.localPosition = new Vector3(0.1f, 0.13f, 0.01f);
				return;
			}
			if (inhaling != this.previousInhalingState)
			{
				if (inhaling)
				{
					this.targetCigPos = new Vector3(-0.014f, 0.03f, -0.012f);
					this.targetHandTargetPos = new Vector3(0.1f, 0.13f, 0.01f);
				}
				else
				{
					this.targetCigPos = new Vector3(0.046f, 0.13f, -0.07f);
					this.targetHandTargetPos = new Vector3(-0.01f, 0.07f, -0.12f);
				}
				this.oldCigPos = this.Cigarette.transform.localPosition;
				this.oldHandTargetPos = this.CigaretteHandTarget.localPosition;
				this.animationTimer = 0f;
				this.isAnimating = true;
				this.previousInhalingState = inhaling;
			}
			if (this.isAnimating)
			{
				this.animationTimer += Time.deltaTime;
				float num = this.animationTimer / 0.25f;
				this.Cigarette.transform.localPosition = Vector3.Lerp(this.oldCigPos, this.targetCigPos, num);
				this.CigaretteHandTarget.localPosition = Vector3.Lerp(this.oldHandTargetPos, this.targetHandTargetPos, num);
				if (num >= 1f)
				{
					this.isAnimating = false;
					this.Cigarette.transform.localPosition = this.targetCigPos;
					this.CigaretteHandTarget.localPosition = this.targetHandTargetPos;
				}
			}
		}

		private void SitInCurrentVehicle()
		{
			if (this.currentVehicle != null && this.IsSpawned)
			{
				bool flag = false;
				try
				{
					if (this.currentVehicle.Name == "BUS")
					{
						flag = true;
					}
				}
				catch
				{
				}
				if (flag)
				{
					this.characterGameObject.transform.SetParent(this.currentVehicle.transform, false);
					this.SnapPlayerToGrid();
				}
				else
				{
					PlayerVehicle playerVehicle = this.currentVehicle.GetObjectSubtype() as PlayerVehicle;
					if (this.characterGameObject.transform.parent == null)
					{
						this.characterGameObject.transform.SetParent(playerVehicle.ParentGameObject.transform, false);
					}
				}
				if (this.steamId != this.netManager.localPlayer.steamId && !this.timerIsRunning && this.characterGameObject.GetComponent<Animation>().enabled)
				{
					foreach (PlayerAnimManager.AnimState animState in this.animManager.states)
					{
						if (animState.isActive)
						{
							animState.Deactivate();
						}
					}
					this.animManager.DisableAnimationComponent();
					this.timeRemaining = 1f;
					this.timerIsRunning = true;
				}
			}
		}

		public virtual void EnterVehicle(ObjectSyncComponent vehicle, int passenger)
		{
			if (this.currentVehicle != null)
			{
				Logger.Debug("Can not leave Driving mode. Stop moving and unfasten seatbelt!");
			}
			else if (this.state != NetPlayer.State.OnFoot)
			{
				Logger.Debug("Entered vehicle but player is not on foot.");
			}
			this.currentVehicle = vehicle;
			this.vehicleParent = (vehicle.GetObjectSubtype() as PlayerVehicle).ParentGameObject.name;
			bool flag = false;
			try
			{
				if (this.currentVehicle.Name == "BUS")
				{
					flag = true;
				}
			}
			catch
			{
			}
			NetPlayer.State state;
			if (flag)
			{
				state = NetPlayer.State.Passenger;
			}
			else
			{
				PlayerVehicle playerVehicle = this.currentVehicle.GetObjectSubtype() as PlayerVehicle;
				if (passenger == 0)
				{
					playerVehicle.CurrentDrivingState = PlayerVehicle.DrivingStates.Driver;
					state = NetPlayer.State.DrivingVehicle;
					if (this.steamId != this.netManager.localPlayer.steamId)
					{
						playerVehicle.SeatTransform.gameObject.SetActive(false);
						playerVehicle.rigidbody.isKinematic = true;
						playerVehicle.rigidbody.isKinematic = false;
					}
				}
				else if (passenger == 1)
				{
					playerVehicle.CurrentDrivingState = PlayerVehicle.DrivingStates.Passenger;
					state = NetPlayer.State.Passenger;
				}
				else
				{
					playerVehicle.CurrentDrivingState = PlayerVehicle.DrivingStates.Backseater;
					state = NetPlayer.State.Backseater;
				}
			}
			this.SitInCurrentVehicle();
			this.SwitchState(state);
		}

		public virtual void LeaveVehicle()
		{
			if (this.currentVehicle != null && this.state == NetPlayer.State.OnFoot)
			{
				Logger.Debug("Player is leaving vehicle but he is not in vehicle.");
				this.isSitting = false;
				this.timerIsRunning = false;
				if (this.steamId != this.netManager.localPlayer.steamId)
				{
					this.animManager.EnableAnimationComponent();
				}
				this.characterGameObject.transform.SetParent(null);
				this.currentVehicle = null;
				this.DisableIK();
				return;
			}
			if (this.IsSpawned)
			{
				try
				{
					this.isSitting = false;
					this.timerIsRunning = false;
					if (this.steamId != this.netManager.localPlayer.steamId)
					{
						this.animManager.EnableAnimationComponent();
					}
					this.characterGameObject.transform.SetParent(null);
					bool flag = false;
					try
					{
						if (this.currentVehicle.Name == "BUS")
						{
							flag = true;
						}
					}
					catch
					{
					}
					if (!flag && this.currentVehicle != null)
					{
						PlayerVehicle playerVehicle = this.currentVehicle.GetObjectSubtype() as PlayerVehicle;
						Transform seatTransform = playerVehicle.SeatTransform;
						this.Teleport(seatTransform.position, seatTransform.rotation);
						if (this.steamId != this.netManager.localPlayer.steamId && this.state == NetPlayer.State.DrivingVehicle)
						{
							seatTransform.gameObject.SetActive(true);
						}
						playerVehicle.CurrentDrivingState = PlayerVehicle.DrivingStates.None;
					}
				}
				catch (Exception ex)
				{
					Logger.Debug(string.Format("Error leaving vehicle: {0}", ex));
					this.characterGameObject.transform.SetParent(null);
				}
			}
			this.currentVehicle = null;
			this.DisableIK();
			this.SwitchState(NetPlayer.State.OnFoot);
		}

		protected virtual void SwitchState(NetPlayer.State newState)
		{
			this.state = newState;
		}

		public virtual void Teleport(Vector3 pos, Quaternion rot)
		{
			this.interpolator.Teleport(pos, rot);
			this.UpdateCharacterPosition();
		}

		private void UpdateCharacterPosition()
		{
			if (this.characterGameObject != null)
			{
				this.characterGameObject.transform.position = this.interpolator.CurrentPosition + this.CHARACTER_OFFSET;
				this.characterGameObject.transform.rotation = this.interpolator.CurrentRotation;
			}
		}

		private void UpdatePickedupPosition()
		{
			if (this.pickedUpObject != null)
			{
				this.pickedUpObject.transform.position = this.pickedUpObjectInterpolator.CurrentPosition;
				this.pickedUpObject.transform.rotation = this.pickedUpObjectInterpolator.CurrentRotation;
			}
		}

		public virtual Vector3 GetPosition()
		{
			return this.interpolator.CurrentPosition;
		}

		public virtual Quaternion GetRotation()
		{
			return this.interpolator.CurrentRotation;
		}

		public float[] GetAttributes()
		{
			return this.playerAttributes;
		}

		public virtual string GetName()
		{
			return SteamFriends.GetFriendPersonaName(this.steamId);
		}

		public void PickupObject(ushort netId)
		{
			this.pickedUpObjectNetId = netId;
			if (this.IsSpawned)
			{
				this.UpdatePickedUpObject(true, false);
			}
		}

		public void ReleaseObject(bool drop)
		{
			if (this.IsSpawned)
			{
				this.UpdatePickedUpObject(false, drop);
			}
			this.pickedUpObjectNetId = ushort.MaxValue;
		}

		public void ReadPickupMessage(bool pickup, GameObject go = null)
		{
			if (this.currentVehicle != null)
			{
				return;
			}
			if (pickup && Vector3.Distance(this.characterGameObject.transform.position, go.transform.position) < 3f)
			{
				this.timeSincePickup = 0.15f;
				if (this.pickedUpObject == null)
				{
					this.pickedUpObject = go;
					this.rightArmIK.enabled = true;
					this.rightArmIK.target = this.pickedUpObject.transform;
					if (!go.activeSelf)
					{
						go.SetActive(true);
					}
					if (go.transform.parent != null && go.tag != "RAGDOLL")
					{
						go.transform.SetParent(null);
						return;
					}
				}
			}
			else if (this.pickedUpObject != null)
			{
				this.pickedUpObject = null;
				this.rightArmIK.enabled = false;
				this.rightArmIK.target = null;
			}
		}

		private void UpdatePickedUpObject(bool pickup, bool drop)
		{
			if (pickup)
			{
				this.pickedUpObject = this.netWorld.GetPickupableGameObject((int)this.pickedUpObjectNetId);
				ErrorHandling.Assert(this.pickedUpObject != null, "Player tried to pickup object that does not exists in world. Net id: " + this.pickedUpObjectNetId.ToString());
				this.oldPickupableLayer = this.pickedUpObject.layer;
				this.pickedUpObject.layer = 4;
				this.pickedUpObject.GetComponent<Rigidbody>().isKinematic = true;
				this.rightArmIK.target = this.pickedUpObject.transform;
				return;
			}
			ErrorHandling.Assert(this.pickedUpObject != null, "Tried to drop item however player has no item in hands.");
			this.pickedUpObject.layer = this.oldPickupableLayer;
			this.pickedUpObject.GetComponent<Rigidbody>().isKinematic = false;
			if (!drop)
			{
				float num = 50f;
				this.pickedUpObject.GetComponent<Rigidbody>().AddForce(this.pickedUpObject.transform.forward * num, 1);
			}
			this.pickedUpObject = null;
			this.rightArmIK.target = null;
		}

		private CSteamID steamId = CSteamID.Nil;

		public GameObject loadedModel;

		public uint currentPing;

		private Vector3 CHARACTER_OFFSET = new Vector3(0f, 0.6f, 0f);

		private Vector3 HAT_OFFSET = new Vector3(-0.056f, 0f, -0.009f);

		public float timeRemaining = 1f;

		public bool timerIsRunning;

		protected NetManager netManager;

		protected PlayerAnimManager animManager;

		public int accesslevel;

		public bool sleepRequested;

		public GameObject characterGameObject;

		private IKManager leftArmIK;

		private IKManager rightArmIK;

		private IKManager rightLegIK;

		private GameObject leftElbow;

		private GameObject rightElbow;

		private EllipsoidParticleEmitter PissParticles;

		private GameObject hatGo;

		private GameObject shadesGo;

		private List<GameObject> Hats = new List<GameObject>();

		private List<GameObject> Shades = new List<GameObject>();

		private List<GameObject> Extras = new List<GameObject>();

		public bool hasHandshake;

		public string modelName = "SuitModel";

		public bool hasHat;

		public bool isConnected;

		private TransformInterpolator interpolator = new TransformInterpolator();

		private TransformInterpolator pickedUpObjectInterpolator = new TransformInterpolator();

		private ulong INTERPOLATION_TIME;

		private ulong syncReceiveTime;

		protected NetPlayer.State state;

		public string skin = "0,0,0,0,0,0";

		public ObjectSyncComponent currentVehicle;

		protected NetWorld netWorld;

		public GameObject pickedUpObject;

		private ushort pickedUpObjectNetId = ushort.MaxValue;

		private int oldPickupableLayer;

		private int hatsApplied;

		private float timeSincePickup;

		private readonly float[] xGrid = new float[] { -1f, -0.5f, 0.5f, 1f };

		private float[] zGrid;

		private readonly float zStart = 3.66f;

		private readonly float zEnd = -5.36f;

		private readonly float zGap = 0.76f;

		private Transform PissHandTarget;

		private Transform CigaretteHandTarget;

		private GameObject Cigarette;

		private EllipsoidParticleEmitter SmokeParticles;

		private Vector3 defaultLegPos1;

		private Vector3 defaultLegPos2;

		private Vector3 pushedLegPos1;

		private Vector3 pushedLegPos2;

		private bool footdown;

		private bool halfSit;

		private bool isLoadedIntoWorld;

		private GameObject phoneObject;

		private GameObject[] nozzleObjects = new GameObject[3];

		private float aimRot;

		private float newRot;

		private float aimPitch;

		private float newPitch;

		private Transform head;

		public bool isSitting;

		private float inhaleTimer;

		private bool inhaling;

		private bool smoking;

		private float animationTimer;

		private Vector3 oldCigPos;

		private Vector3 oldHandTargetPos;

		private Vector3 targetCigPos;

		private Vector3 targetHandTargetPos;

		private bool isAnimating;

		private bool previousInhalingState;

		private string vehicleParent;

		private float[] playerAttributes;

		protected enum State
		{
			OnFoot,
			DrivingVehicle,
			Passenger,
			Backseater
		}
	}
}
