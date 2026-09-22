using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Places
{
	public class Factory : MonoBehaviour
	{
		private void Start()
		{
			Factory.Instance = this;
			foreach (PlayMakerFSM playMakerFSM in base.GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.name.StartsWith("Pick"))
				{
					EventHook.AddWithSync(playMakerFSM, "Wait player", null, false);
					EventHook.AddWithSync(playMakerFSM, "Check old", null, false);
				}
				else if (playMakerFSM.name == "TriggerBox")
				{
					this.triggerBoxFsm = playMakerFSM;
					EventHook.Add(playMakerFSM, "Assemble", delegate
					{
						if (this.receiver)
						{
							this.receiver = false;
						}
						else
						{
							NetLocalPlayer.Instance.WriteFSMBoolMessage(67670, "", true);
						}
						return false;
					}, false, false);
				}
				else if (playMakerFSM.FsmName == "Knob")
				{
					EventHook.AddWithSync(playMakerFSM, "Mouse off 2", null, false);
					if (playMakerFSM.name == "Power")
					{
						EventHook.AddWithSync(playMakerFSM, "Switch", null, false);
					}
					else
					{
						EventHook.AddWithSync(playMakerFSM, "Increase", null, false);
						EventHook.AddWithSync(playMakerFSM, "Decrease", null, false);
						EventHook.AddWithSync(playMakerFSM, "Rotate knob", null, false);
					}
				}
				else if (playMakerFSM.name == "cable plug(itemx)")
				{
					EventHook.AddWithSync(playMakerFSM, "Heater on", null, false);
					EventHook.AddWithSync(playMakerFSM, "Heater off", null, false);
				}
				else if (playMakerFSM.FsmName == "LOD")
				{
					playMakerFSM.gameObject.AddComponent<LODManager>().Initialize(playMakerFSM, -1f, true, "LODdistance", "Object");
				}
			}
		}

		public void ReadTriggerBoxMessage()
		{
			this.receiver = true;
			GameObject gameObject = null;
			float num = 100f;
			foreach (GameObject gameObject2 in from GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]
				where go.name == "packages box(Clone)"
				select go)
			{
				if (Vector3.Distance(this.triggerBoxFsm.transform.position, gameObject2.transform.position) < num)
				{
					num = Vector3.Distance(this.triggerBoxFsm.transform.position, gameObject2.transform.position);
					gameObject = gameObject2;
				}
			}
			if (gameObject == null)
			{
				Logger.Debug("[FACTORY] ERROR: Null part object for triggerBoxFsm");
				return;
			}
			this.triggerBoxFsm.Fsm.GetFsmGameObject("Part").Value = gameObject;
			this.triggerBoxFsm.SendEvent("MP_Assemble");
		}

		public Factory()
		{
		}

		[CompilerGenerated]
		private bool <Start>b__3_0()
		{
			if (this.receiver)
			{
				this.receiver = false;
			}
			else
			{
				NetLocalPlayer.Instance.WriteFSMBoolMessage(67670, "", true);
			}
			return false;
		}

		public static Factory Instance;

		private PlayMakerFSM triggerBoxFsm;

		private bool receiver;

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

			internal bool <ReadTriggerBoxMessage>b__4_0(GameObject go)
			{
				return go.name == "packages box(Clone)";
			}

			public static readonly Factory.<>c <>9 = new Factory.<>c();

			public static Func<GameObject, bool> <>9__4_0;
		}
	}
}
