using System;
using System.Runtime.CompilerServices;
using MWCO.Game.Components;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class CarPart : MonoBehaviour
	{
		private void Awake()
		{
			try
			{
				this.SetupBolts();
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("[CarPart] Bolt Setup failed on {0}. Reason {1}", base.name, ex));
			}
		}

		public void Initialize()
		{
			try
			{
				this.GetFSM();
				this.HookEvents();
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("[CarPart] Setup failed on {0}. Reason {1}", base.name, ex));
			}
		}

		private void Update()
		{
			if (this.syncComponent == null)
			{
				this.syncComponent = base.GetComponent<ObjectSyncComponent>();
			}
			if (this.checkAttachTimer > 0f)
			{
				this.checkAttachTimer -= Time.deltaTime;
				if (this.checkAttachTimer <= 0f && this.removalFsm.Fsm.GetFsmFloat("Tightness").Value > 0f && !this.myTriggerFsm.Fsm.GetFsmBool("Installed").Value)
				{
					Logger.Debug("Was detected uninstalled, installing now?");
					this.myTriggerFsm.SendEvent("MP_Install 1");
				}
			}
			this.CheckAttachment();
		}

		private void CheckAttachment()
		{
			if (base.CompareTag("Untagged"))
			{
				if (this.syncComponent.enabled)
				{
					this.syncComponent.enabled = false;
					Logger.Debug("[CarPart] Disabled OSC of " + base.name + " after checking Tag.");
					return;
				}
			}
			else if (!this.syncComponent.enabled)
			{
				this.syncComponent.enabled = true;
				Logger.Debug("[CarPart] Enabled OSC of " + base.name + ".");
			}
		}

		private void GetFSM()
		{
			foreach (PlayMakerFSM playMakerFSM in base.GetComponents<PlayMakerFSM>())
			{
				if (playMakerFSM.FsmName == "Data")
				{
					this.removalFsm = playMakerFSM;
				}
			}
			if (base.name == "VIN1010")
			{
				this.myTriggerFsm = this.removalFsm.Fsm.GetFsmGameObject("InstallPoint").Value.GetComponent<PlayMakerFSM>();
			}
		}

		private void HookEvents()
		{
			if (this.removalFsm != null)
			{
				EventHook.AddWithSync(this.removalFsm, "Remove", delegate
				{
					this.syncComponent.enabled = true;
					return false;
				}, false);
				if (base.name == "VIN1010")
				{
					EventHook.Add(this.removalFsm, "Bolted", delegate
					{
						try
						{
							this.checkAttachTimer = 0.2f;
							this.syncComponent.enabled = false;
						}
						catch (Exception)
						{
						}
						return false;
					}, false, false);
				}
			}
			PlayMakerFSM[] componentsInChildren = base.GetComponentsInChildren<PlayMakerFSM>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				PlayMakerFSM fsm = componentsInChildren[i];
				if (fsm.FsmName == "Screw")
				{
					if (fsm.name == "OpenCap")
					{
						EventHook.AddWithSync(fsm, "Mouse off 2", null, false);
						EventHook.AddWithSync(fsm, "Screw", null, false);
						EventHook.AddWithSync(fsm, "Unscrew", null, false);
					}
					else if (base.name.StartsWith("Oil filter"))
					{
						EventHook.Add(fsm, "Get scroll", () => false, false, false);
						EventHook.AddWithSync(fsm, "Screw 2", null, false);
						EventHook.AddWithSync(fsm, "Unscrew 2", null, false);
						EventHook.AddWithSync(fsm, "Set", delegate
						{
							if (fsm.Fsm.LastTransition.EventName.StartsWith("MP_"))
							{
								float num = fsm.Fsm.GetFsmFloat("Tightness").Value;
								num = Mathf.Round(num * 400f);
								fsm.Fsm.GetFsmFloat("Tightness").Value = -num;
							}
							return false;
						}, true);
						EventHook.AddWithSync(fsm, "Wait1 2", null, false);
					}
				}
				else if (fsm.FsmName == "HandRotate" && fsm.name == "Pivot")
				{
					EventHook.AddWithSync(fsm, "Wait player", null, false);
					EventHook.AddWithSync(fsm, "Counterwise", null, false);
					EventHook.AddWithSync(fsm, "Clockwise", null, false);
					EventHook.AddWithSync(fsm, "Wait", null, false);
				}
			}
		}

		private void SetupBolts()
		{
			foreach (PlayMakerFSM playMakerFSM in base.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.FsmName == "Screw" && (playMakerFSM.name == "BoltPM" || base.name.StartsWith("SPRKPLUG")) && playMakerFSM.GetComponent<Bolt>() == null)
				{
					Bolt bolt = playMakerFSM.gameObject.AddComponent<Bolt>();
					bolt.BoltID = GameVehicleDatabase.Instance.bolts.Count;
					GameVehicleDatabase.Instance.bolts.Add(bolt);
				}
			}
		}

		public CarPart()
		{
		}

		[CompilerGenerated]
		private bool <HookEvents>b__9_0()
		{
			this.syncComponent.enabled = true;
			return false;
		}

		[CompilerGenerated]
		private bool <HookEvents>b__9_1()
		{
			try
			{
				this.checkAttachTimer = 0.2f;
				this.syncComponent.enabled = false;
			}
			catch (Exception)
			{
			}
			return false;
		}

		public PlayMakerFSM removalFsm;

		public ObjectSyncComponent syncComponent;

		public PlayMakerFSM myTriggerFsm;

		private float checkAttachTimer;

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

			internal bool <HookEvents>b__9_2()
			{
				return false;
			}

			public static readonly CarPart.<>c <>9 = new CarPart.<>c();

			public static Func<bool> <>9__9_2;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass9_0
		{
			public <>c__DisplayClass9_0()
			{
			}

			internal bool <HookEvents>b__3()
			{
				if (this.fsm.Fsm.LastTransition.EventName.StartsWith("MP_"))
				{
					float num = this.fsm.Fsm.GetFsmFloat("Tightness").Value;
					num = Mathf.Round(num * 400f);
					this.fsm.Fsm.GetFsmFloat("Tightness").Value = -num;
				}
				return false;
			}

			public PlayMakerFSM fsm;
		}
	}
}
