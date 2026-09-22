using System;
using UnityEngine;

namespace MWCO.Game.Components
{
	internal class ObjectSyncPlayerComponent : MonoBehaviour
	{
		private void Start()
		{
		}

		private void OnTriggerEnter(Collider other)
		{
			ObjectSyncComponent component = other.GetComponent<ObjectSyncComponent>();
			if (component != null)
			{
				component.SendEnterSync();
			}
		}

		private void OnTriggerExit(Collider other)
		{
			ObjectSyncComponent component = other.GetComponent<ObjectSyncComponent>();
			if (component != null)
			{
				component.SendExitSync();
			}
		}

		public ObjectSyncPlayerComponent()
		{
		}
	}
}
