using System;
using MWCO.Game;
using MWCO.Game.Components;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Network;
using MWCO.Network.Messages;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO
{
	public class InstanceDebugger : MonoBehaviour
	{
		private void Awake()
		{
			this.Name = base.name;
		}

		private void Update()
		{
			if (this.Name != base.name)
			{
				this.Name = base.name;
				this.SetupOSC();
				return;
			}
			this.frameCounter++;
			if (this.frameCounter > 30)
			{
				this.SetupOSC();
			}
		}

		private void SetupOSC()
		{
			if (this.oscSetup)
			{
				Logger.Debug("[InstanceDebugger] Tried to setup osc twice, name changed twice?");
				return;
			}
			if (!GamePickupableDatabase.IsPickupable(base.gameObject))
			{
				if (base.GetComponent<PlayMakerFSM>() == null)
				{
					base.enabled = false;
					return;
				}
				if (!GameVehicleDatabase.Instance.IsRivett(base.gameObject) && !base.name.StartsWith("SPRKPLUG"))
				{
					base.enabled = false;
					Logger.Debug("[InstanceDebugger] Getting rid of " + this.Name + ", Is it not a pickupable?");
					return;
				}
				Logger.Debug("[InstanceDebugger] Setting up carpart " + this.Name + " that isn't detected as a pickupable");
				if (base.GetComponent<CarPart>() == null)
				{
					base.gameObject.AddComponent<CarPart>().Initialize();
					GameVehicleDatabase.Instance.carParts.Add(base.gameObject);
				}
			}
			PickupableMetaDataComponent component = base.GetComponent<PickupableMetaDataComponent>();
			if (component == null)
			{
				string text = "NO METADATA ASSIGNED TO ";
				string name = base.name;
				string text2 = " parent of ";
				GameObject gameObject = base.gameObject;
				Logger.Debug(text + name + text2 + ((gameObject != null) ? gameObject.GetTopmostParent().name : null));
				return;
			}
			NetWorld.Instance.RegisterPickupable(base.gameObject, false);
			PickupableSpawnMessage pickupableSpawnMessage = new PickupableSpawnMessage();
			pickupableSpawnMessage.prefabId = component.prefabId;
			pickupableSpawnMessage.transform.position = Utils.GameVec3ToNet(base.transform.position);
			pickupableSpawnMessage.transform.rotation = Utils.GameQuatToNet(base.transform.rotation);
			pickupableSpawnMessage.active = base.gameObject.activeSelf;
			if (!NetWorld.Instance.playerIsLoading && !NetManager.Instance.IsHost && !base.name.StartsWith("BottleBeerFly") && !base.name.StartsWith("empty cup") && !base.name.StartsWith("empty bottle"))
			{
				Logger.Debug("OPMOC Ignoring " + base.gameObject.name + " object as client is not host!");
				Object.Destroy(base.gameObject);
				return;
			}
			ObjectSyncComponent component2 = base.GetComponent<ObjectSyncComponent>();
			if (base.GetComponents<ObjectSyncComponent>().Length > 1)
			{
				Logger.Debug(base.name + " has multiple OSC!");
				ObjectSyncComponent[] components = base.GetComponents<ObjectSyncComponent>();
				for (int i = 0; i < components.Length; i++)
				{
					Object.Destroy(components[i]);
				}
				ObjectSyncComponent objectSyncComponent = base.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, -1);
				pickupableSpawnMessage.id = objectSyncComponent.ObjectID;
			}
			else if (component2 != null)
			{
				Logger.Debug(string.Format("{0} has one OSC existing? ID:{1}", base.name, base.GetComponent<ObjectSyncComponent>().ObjectID));
				Object.Destroy(base.GetComponent<ObjectSyncComponent>());
				ObjectSyncComponent objectSyncComponent2 = base.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, -1);
				pickupableSpawnMessage.id = objectSyncComponent2.ObjectID;
			}
			else
			{
				ObjectSyncComponent objectSyncComponent3 = base.gameObject.AddComponent<ObjectSyncComponent>().Setup(ObjectSyncManager.ObjectTypes.Pickupable, -1);
				pickupableSpawnMessage.id = objectSyncComponent3.ObjectID;
			}
			bool flag = false;
			if (NetManager.Instance.IsHost)
			{
				flag = true;
			}
			else if (base.name.StartsWith("BottleBeerFly") || base.name.StartsWith("empty cup") || base.name.StartsWith("empty bottle"))
			{
				flag = true;
			}
			if (flag)
			{
				NetManager.Instance.BroadcastMessage<PickupableSpawnMessage>(pickupableSpawnMessage, 2, 0);
			}
			this.oscSetup = true;
			base.enabled = false;
		}

		public InstanceDebugger()
		{
		}

		private string Name;

		private bool oscSetup;

		private int frameCounter;
	}
}
