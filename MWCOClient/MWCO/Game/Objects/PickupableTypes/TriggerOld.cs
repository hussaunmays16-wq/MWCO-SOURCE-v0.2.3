using System;
using UnityEngine;

namespace MWCO.Game.Objects.PickupableTypes
{
	internal class TriggerOld : MonoBehaviour
	{
		public void Awake()
		{
			if (this.IsSetup)
			{
				return;
			}
			this.GetFSM();
			this.IsSetup = true;
		}

		private void GetFSM()
		{
			if (base.name.ToLower().StartsWith("VINP") || base.name.StartsWith("Fuse"))
			{
				this.triggerFsm = base.gameObject.GetComponent<PlayMakerFSM>();
			}
		}

		public void LoadAttachedObject(GameObject gameObject)
		{
			this.triggerFsm.Fsm.GetFsmGameObject("Part").Value = gameObject;
			this.triggerFsm.SendEvent("MP_Assemble");
			if (gameObject.name.StartsWith("alternator") && gameObject.name.Contains("belt"))
			{
				MPController.Instance.AlternatorBeltData.Fsm.GetFsmFloat("Wear").Value = 12f;
				MPController.Instance.AlternatorBeltData.Fsm.GetFsmBool("Installed").Value = true;
			}
		}

		public TriggerOld()
		{
		}

		public int belongsToObjectID;

		public PlayMakerFSM triggerFsm;

		public GameObject carPart;

		public PlayMakerFSM dataFSM;

		public bool IsSetup;

		public bool receiver;
	}
}
