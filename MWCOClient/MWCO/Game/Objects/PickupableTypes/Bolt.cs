using System;
using MWCO.Network;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class Bolt : MonoBehaviour
	{
		private void Awake()
		{
			this.screwFsm = base.GetComponent<PlayMakerFSM>();
		}

		public void SendTighten()
		{
			NetLocalPlayer.Instance.WriteBoltMessage(this.BoltID, true);
		}

		public void SendUntighten()
		{
			NetLocalPlayer.Instance.WriteBoltMessage(this.BoltID, false);
		}

		public void ReadTightenMessage(bool tighten)
		{
			if (this.screwFsm == null)
			{
				this.screwFsm = base.GetComponent<PlayMakerFSM>();
			}
			if (tighten)
			{
				this.screwFsm.SendEvent("TIGHTEN");
			}
			else
			{
				this.screwFsm.SendEvent("UNTIGHTEN");
			}
			MasterAudio.PlaySound3DAtTransform("CarBuilding", base.transform, 1f, new float?(1f), 0f, "bolt_screw", false, false);
		}

		public Bolt()
		{
		}

		public int BoltID = -1;

		public PlayMakerFSM screwFsm;
	}
}
