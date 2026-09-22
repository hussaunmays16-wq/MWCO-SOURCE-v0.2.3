using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects
{
	public class BootBox : MonoBehaviour
	{
		public void Initialize()
		{
			Logger.Debug("Boot Freeze Box added, vehicle: " + this.VehicleType);
			base.name = "BootBox";
			base.transform.localEulerAngles = Vector3.zero;
			if (this.VehicleType == "SORBET(190-200psi)")
			{
				base.transform.localPosition = new Vector3(0f, 0f, 0.2f);
				base.transform.localScale = new Vector3(1.65f, 1.36f, 4.36f);
			}
			if (this.VehicleType == "GIFU(750/450psi)")
			{
				base.transform.localPosition = Vector3.zero;
				base.transform.localScale = new Vector3(2.05f, 1.45f, 1.85f);
			}
			if (this.VehicleType == "BACHGLOTZ(1905kg)")
			{
				base.transform.localPosition = new Vector3(0f, 0f, -2f);
				base.transform.localScale = new Vector3(3f, 1.5f, 5.85f);
			}
			if (this.VehicleType.StartsWith("JONNEZ ES"))
			{
				base.transform.localPosition = new Vector3(0f, 0.284f, -0.716f);
			}
			if (this.VehicleType.StartsWith("KEKMET"))
			{
				base.transform.localPosition = Vector3.zero;
				base.transform.localEulerAngles = Vector3.zero;
			}
			if (this.VehicleType == "MACHTWAGEN")
			{
				base.transform.localPosition = new Vector3(0f, 0f, -0.6f);
				base.transform.localScale = new Vector3(1.5f, 1.45f, 3.4f);
			}
			if (this.VehicleType == "CORRIS")
			{
				base.transform.localPosition = new Vector3(0f, 0f, -0.3f);
				base.transform.localScale = new Vector3(1.45f, 1.16f, 2.94f);
			}
			Object.Destroy(base.GetComponent<MeshRenderer>());
		}

		private void OnTriggerEnter(Collider other)
		{
			try
			{
				if (BootBox.IsPickupable(other.gameObject))
				{
					ObjectSyncComponent component = other.GetComponent<ObjectSyncComponent>();
					if (component != null)
					{
						using (List<ObjectSyncComponent>.Enumerator enumerator = this.pickupables.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.ObjectID == component.ObjectID)
								{
									return;
								}
							}
						}
						this.pickupables.Add(component);
						if (this.isControlling && !component.GetComponent<Rigidbody>().isKinematic)
						{
							Logger.Debug("Taking force control of " + component.name);
							component.TakeSyncControl();
							component.SendConstantSync(true, false);
						}
					}
				}
			}
			catch
			{
			}
		}

		private void OnTriggerExit(Collider other)
		{
			try
			{
				if (BootBox.IsPickupable(other.gameObject))
				{
					ObjectSyncComponent component = other.GetComponent<ObjectSyncComponent>();
					if (component != null)
					{
						for (int i = this.pickupables.Count - 1; i >= 0; i--)
						{
							ObjectSyncComponent objectSyncComponent = this.pickupables[i];
							if (objectSyncComponent.ObjectID == component.ObjectID)
							{
								if (this.isControlling)
								{
									Logger.Debug("Removing any sync control " + objectSyncComponent.name);
									if (objectSyncComponent.IsOwnerSelf())
									{
										objectSyncComponent.SendConstantSync(false, false);
									}
								}
								this.pickupables.RemoveAt(i);
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				string text = "prevented bootbox error: ";
				Exception ex2 = ex;
				Logger.Debug(text + ((ex2 != null) ? ex2.ToString() : null));
			}
		}

		public static bool IsPickupable(GameObject gameObject)
		{
			return (gameObject.CompareTag("PART") || gameObject.CompareTag("ITEM")) && gameObject.GetComponent<Rigidbody>();
		}

		public void ClaimOwnershipOfItems()
		{
			this.isControlling = true;
			foreach (ObjectSyncComponent objectSyncComponent in this.pickupables)
			{
				try
				{
					if (!(objectSyncComponent == null))
					{
						Logger.Debug("Taking force control of " + objectSyncComponent.name);
						objectSyncComponent.TakeSyncControl();
						objectSyncComponent.SendConstantSync(true, false);
					}
				}
				catch
				{
					Logger.Debug(string.Format("Claim ownership failure. (Empty? {0}", objectSyncComponent == null));
				}
			}
		}

		public void UnclaimOwnership()
		{
			this.isControlling = false;
			foreach (ObjectSyncComponent objectSyncComponent in this.pickupables)
			{
				try
				{
					Logger.Debug("Removing any sync control" + objectSyncComponent.name);
					if (objectSyncComponent.IsOwnerSelf())
					{
						objectSyncComponent.SendConstantSync(false, false);
					}
				}
				catch
				{
					Logger.Debug(string.Format("Unclaim ownership failure. (Empty? {0}", objectSyncComponent == null));
				}
			}
		}

		public void ClientPickedUp(int objectId)
		{
			foreach (ObjectSyncComponent objectSyncComponent in this.pickupables)
			{
				if (objectSyncComponent.ObjectID == objectId)
				{
					Logger.Debug("BOOTBOX client picked up, GIVING CONTROL " + objectSyncComponent.name);
					if (objectSyncComponent.IsOwnerSelf())
					{
						objectSyncComponent.SendConstantSync(false, false);
					}
				}
			}
		}

		public void ClientDroppedPickupable(int objectId)
		{
			foreach (ObjectSyncComponent objectSyncComponent in this.pickupables)
			{
				if (objectSyncComponent.ObjectID == objectId)
				{
					Logger.Debug("BOOTBOX client dropped inside TAKING CONTROL " + objectSyncComponent.name);
					objectSyncComponent.TakeSyncControl();
					objectSyncComponent.SendConstantSync(true, false);
				}
			}
		}

		private void Update()
		{
		}

		public BootBox()
		{
		}

		public string VehicleType;

		public GameObject DriversSeat;

		public ObjectSyncComponent syncComponent;

		private List<ObjectSyncComponent> pickupables = new List<ObjectSyncComponent>();

		private GameObject trigger;

		public BootBox.OnEnter onEnter = delegate
		{
		};

		public BootBox.OnLeave onLeave = delegate
		{
		};

		private bool isControlling;

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

			internal void <.ctor>b__19_0()
			{
			}

			internal void <.ctor>b__19_1()
			{
			}

			public static readonly BootBox.<>c <>9 = new BootBox.<>c();

			public static BootBox.OnEnter <>9__19_0;

			public static BootBox.OnLeave <>9__19_1;
		}
	}
}
