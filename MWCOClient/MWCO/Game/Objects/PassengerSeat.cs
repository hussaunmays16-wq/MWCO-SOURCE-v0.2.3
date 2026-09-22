using System;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects
{
	internal class PassengerSeat : MonoBehaviour
	{
		private void Start()
		{
			Logger.Debug("Passenger seat added, vehicle: " + this.VehicleType);
			base.name = "PassengerTrigger";
			this.guiGameObject = GameObject.Find("GUI");
			foreach (PlayMakerFSM playMakerFSM in this.guiGameObject.GetComponentsInChildren<PlayMakerFSM>())
			{
				if (playMakerFSM.FsmName == "Logic")
				{
					this.iconsFsm = playMakerFSM;
				}
				else if (playMakerFSM.FsmName == "SetText" && playMakerFSM.gameObject.name == "Interaction")
				{
					this.textFsm = playMakerFSM;
				}
				else if (this.iconsFsm != null && this.textFsm != null)
				{
					break;
				}
			}
			if (!this.rear)
			{
				if (this.VehicleType == "CORRIS")
				{
					base.transform.localPosition = new Vector3(0.33f, -0.02f, 0.42f);
					base.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
					base.transform.localEulerAngles = Vector3.zero;
				}
				if (this.VehicleType == "SORBET(190-200psi)")
				{
					base.transform.localPosition = new Vector3(0.356f, -0.004f, 0.397f);
					base.transform.localEulerAngles = Vector3.zero;
				}
				if (this.VehicleType == "GIFU(750/450psi)")
				{
					base.transform.localPosition = new Vector3(-this.DriversSeat.transform.localPosition.x, -this.DriversSeat.transform.localPosition.y, -this.DriversSeat.transform.localPosition.z + 0.15f);
					base.transform.localEulerAngles = Vector3.zero;
				}
				if (this.VehicleType == "BACHGLOTZ(1905kg)")
				{
					base.transform.localPosition = new Vector3(0.7f, 0f, -0.1f);
					base.transform.localEulerAngles = Vector3.zero;
				}
				if (this.VehicleType.StartsWith("JONNEZ ES"))
				{
					base.transform.localPosition = new Vector3(0f, 0.284f, -0.716f);
				}
				if (this.VehicleType.StartsWith("KEKMET"))
				{
					base.transform.localPosition = new Vector3(0f, 1.2f, 0f);
				}
				if (this.VehicleType == "MACHTWAGEN")
				{
					base.transform.localPosition = new Vector3(0.4f, 0f, 0.3f);
					base.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				}
				Object.Destroy(base.GetComponentInChildren<MeshRenderer>());
				return;
			}
			if (this.VehicleType == "CORRIS")
			{
				base.transform.localPosition = new Vector3(0f, -0.03f, -0.35f);
				base.transform.localScale = new Vector3(0.82f, 0.2f, 0.3f);
				base.transform.localEulerAngles = Vector3.zero;
			}
			if (this.VehicleType == "SORBET(190-200psi)")
			{
				base.transform.localPosition = new Vector3(0f, 0.046f, -0.403f);
				base.transform.localScale = new Vector3(0.7f, 0.2f, 0.2f);
			}
			if (this.VehicleType == "GIFU(750/450psi)")
			{
				base.transform.localPosition = new Vector3(-0.054f, 0.014f, -0.568f);
				base.transform.localEulerAngles = Vector3.zero;
				base.transform.localScale = new Vector3(1.62f, 0.2f, 0.35f);
			}
			if (this.VehicleType == "BACHGLOTZ(1905kg)")
			{
				base.transform.localPosition = new Vector3(0f, 0f, -1.12f);
				base.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
				base.transform.localScale = new Vector3(0.02618f, 0.09687f, 2.0537f);
			}
			if (this.VehicleType == "MACHTWAGEN")
			{
				base.transform.localPosition = new Vector3(0f, 0f, -0.6f);
				base.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				base.transform.localScale = new Vector3(1f, 0.1f, 0.1f);
			}
			if (this.VehicleType.StartsWith("FLATBED"))
			{
				base.transform.localPosition = new Vector3(-this.DriversSeat.transform.localPosition.x - 0.25f, -this.DriversSeat.transform.localPosition.y, -this.DriversSeat.transform.localPosition.z - 1.35f);
			}
			Object.Destroy(base.GetComponentInChildren<MeshRenderer>());
		}

		private void OnTriggerEnter(Collider other)
		{
			try
			{
				if (other.gameObject.name == "PLAYER")
				{
					this.canSit = true;
					if (!this.isSitting)
					{
						this.showGUI = true;
					}
					if (this.player == null)
					{
						this.player = other.gameObject;
						this.motor = this.player.GetComponentInChildren<CharacterMotor>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("Error with passenger seat ontriggerenter {0}", ex));
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.name == "PLAYER")
			{
				this.canSit = false;
				this.showGUI = false;
				this.iconsFsm.Fsm.GetFsmBool("GUIpassenger").Value = false;
				this.textFsm.Fsm.GetFsmString("GUIinteraction").Value = "";
			}
		}

		public void ForceLeaveSeat()
		{
			if (this.isSitting)
			{
				this.isSitting = false;
				Collider[] components = this.player.GetComponents<Collider>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].enabled = true;
				}
				this.player.transform.SetParent(null);
				this.player.transform.rotation = new Quaternion(0f, this.player.transform.rotation.y, 0f, this.player.transform.rotation.w);
				this.motor.enabled = true;
				this.showGUI = true;
				this.onLeave();
				NetLocalPlayer.Instance.LeaveVehicle();
			}
		}

		private void Update()
		{
			if (this.showGUI)
			{
				this.textFsm.Fsm.GetFsmString("GUIinteraction").Value = "ENTER PASSENGER MODE";
				this.iconsFsm.Fsm.GetFsmBool("GUIpassenger").Value = true;
			}
			if (Input.GetKeyDown(13))
			{
				if (!this.isSitting && this.canSit)
				{
					this.isSitting = true;
					this.player.transform.parent = base.gameObject.transform;
					this.currentPosition = this.player.transform.localPosition;
					this.motor.enabled = false;
					this.player.transform.FindChild("Sphere").GetComponentInChildren<SphereCollider>().enabled = false;
					this.showGUI = false;
					this.iconsFsm.Fsm.GetFsmBool("GUIpassenger").Value = false;
					this.textFsm.Fsm.GetFsmString("GUIinteraction").Value = "";
					Collider[] array = this.player.GetComponents<Collider>();
					for (int i = 0; i < array.Length; i++)
					{
						array[i].enabled = false;
					}
					this.onEnter();
					if (this.rear)
					{
						NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 2);
					}
					else
					{
						NetLocalPlayer.Instance.EnterVehicle(this.syncComponent, 1);
					}
				}
				else if (this.isSitting)
				{
					this.isSitting = false;
					Collider[] array = this.player.GetComponents<Collider>();
					for (int i = 0; i < array.Length; i++)
					{
						array[i].enabled = true;
					}
					this.player.transform.SetParent(null);
					this.player.transform.rotation = new Quaternion(0f, this.player.transform.rotation.y, 0f, this.player.transform.rotation.w);
					this.motor.enabled = true;
					this.showGUI = true;
					this.onLeave();
					NetLocalPlayer.Instance.LeaveVehicle();
				}
			}
			if (this.isSitting)
			{
				this.player.transform.localPosition = this.currentPosition;
			}
		}

		public PassengerSeat()
		{
		}

		public string VehicleType;

		public GameObject DriversSeat;

		public ObjectSyncComponent syncComponent;

		public bool rear;

		private GameObject player;

		private bool canSit;

		private bool isSitting;

		private bool showGUI;

		private CharacterMotor motor;

		private GameObject guiGameObject;

		private PlayMakerFSM iconsFsm;

		private PlayMakerFSM textFsm;

		public PassengerSeat.OnEnter onEnter = delegate
		{
		};

		public PassengerSeat.OnLeave onLeave = delegate
		{
		};

		private Vector3 currentPosition;

		public delegate void OnEnter();

		public delegate void OnLeave();

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

			internal void <.ctor>b__22_0()
			{
			}

			internal void <.ctor>b__22_1()
			{
			}

			public static readonly PassengerSeat.<>c <>9 = new PassengerSeat.<>c();

			public static PassengerSeat.OnEnter <>9__22_0;

			public static PassengerSeat.OnLeave <>9__22_1;
		}
	}
}
