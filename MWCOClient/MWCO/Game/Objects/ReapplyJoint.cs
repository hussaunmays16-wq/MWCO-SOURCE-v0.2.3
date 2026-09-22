using System;
using UnityEngine;

namespace MWCO.Game.Objects
{
	public class ReapplyJoint : MonoBehaviour
	{
		private void Update()
		{
			if (this.backupGO == null)
			{
				this.backupGO = GameObject.CreatePrimitive(3);
				ComponentUtil.CopyComponent<ConfigurableJoint>(base.gameObject.GetComponent<ConfigurableJoint>(), this.backupGO);
				this.pos = base.gameObject.transform.localPosition;
				this.rot = base.gameObject.transform.localRotation;
				this.backupGO.SetActive(false);
			}
			if (base.GetComponents<ConfigurableJoint>().Length <= 2)
			{
				ComponentUtil.CopyComponent<ConfigurableJoint>(this.backupGO.GetComponent<ConfigurableJoint>(), base.gameObject);
				base.gameObject.transform.localPosition = this.pos;
				base.gameObject.transform.localRotation = this.rot;
			}
			if (this.locked)
			{
				base.gameObject.transform.localPosition = this.pos;
				base.gameObject.transform.localRotation = this.rot;
			}
		}

		public ReapplyJoint()
		{
		}

		public GameObject backupGO;

		private Vector3 pos;

		private Quaternion rot;

		public bool locked;
	}
}
