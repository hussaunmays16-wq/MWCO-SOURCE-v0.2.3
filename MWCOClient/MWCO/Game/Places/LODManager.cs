using System;
using MWCO.Network;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game.Places
{
	public class LODManager : MonoBehaviour
	{
		public void Initialize(PlayMakerFSM lod, float normalDist, bool custom = false, string fsmFloat = "LOD", string objName = "Object")
		{
			if (normalDist == -1f)
			{
				normalDist = lod.Fsm.GetFsmFloat(fsmFloat).Value;
			}
			this.normalLOD = normalDist;
			this.LODfsm = lod;
			this.distString = fsmFloat;
			this.CustomHandling = custom;
			if (this.CustomHandling)
			{
				this.lodObject = lod.Fsm.GetFsmGameObject(objName).Value;
				Object.Destroy(lod);
			}
			if (base.GetComponent<Rigidbody>() != null)
			{
				Logger.Debug("CHECKINTERVAL SET FOR " + base.name);
				this.checkInterval = 1f;
			}
		}

		private void Update()
		{
			if (this.timer <= 0f)
			{
				if (this.IsCloseToPlayer())
				{
					this.IncreaseLOD();
				}
				else
				{
					this.ResetLOD();
				}
				this.timer = this.checkInterval;
				return;
			}
			this.timer -= Time.deltaTime;
		}

		public void IncreaseLOD()
		{
			if (this.CustomHandling)
			{
				this.lodObject.SetActive(true);
				return;
			}
			if (this.LODfsm != null)
			{
				this.LODfsm.Fsm.GetFsmFloat(this.distString).Value = this.extendedLOD;
			}
		}

		public void ResetLOD()
		{
			if (this.CustomHandling)
			{
				this.lodObject.SetActive(false);
				return;
			}
			if (this.LODfsm != null)
			{
				this.LODfsm.Fsm.GetFsmFloat(this.distString).Value = this.normalLOD;
			}
		}

		private bool IsCloseToPlayer()
		{
			if (GameWorld.Instance.Player != null && Vector3.Distance(GameWorld.Instance.Player.Object.transform.position, base.transform.position) < this.normalLOD)
			{
				return true;
			}
			foreach (NetPlayer netPlayer in NetManager.Instance.players)
			{
				if (netPlayer != null && netPlayer.characterGameObject != null && Vector3.Distance(netPlayer.characterGameObject.transform.position, base.transform.position) < this.normalLOD)
				{
					return true;
				}
			}
			return false;
		}

		public LODManager()
		{
		}

		private float normalLOD = 80f;

		private float extendedLOD = 4000f;

		public PlayMakerFSM LODfsm;

		private float checkInterval = 2f;

		private float timer;

		private string distString;

		private bool CustomHandling;

		public GameObject lodObject;
	}
}
