using System;
using System.Collections.Generic;
using MWCO.Game.Components;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class TrafficManager : MonoBehaviour
	{
		private void Awake()
		{
			TrafficManager.routes = base.gameObject.transform.FindChild("Routes").gameObject;
			this.GetHighwayCars();
		}

		private void GetHighwayCars()
		{
			Transform transform = base.gameObject.transform.FindChild("VehiclesHighway");
			for (int i = 0; i < transform.childCount; i++)
			{
			}
		}

		private void Update()
		{
			if (this.timeToSync <= 0f)
			{
				foreach (KeyValuePair<Transform, ObjectSyncComponent> keyValuePair in this.npcCars)
				{
					keyValuePair.Value.SendObjectSync(ObjectSyncManager.SyncTypes.GenericSync, true, false);
				}
				this.timeToSync = 5f;
				return;
			}
			this.timeToSync -= Time.deltaTime;
		}

		public static GameObject GetWaypoint(float waypoint, int route)
		{
			GameObject gameObject = null;
			switch (route)
			{
			case 0:
				gameObject = TrafficManager.routes.transform.FindChild("BusRoute").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			case 1:
				gameObject = TrafficManager.routes.transform.FindChild("DirtRoad").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			case 2:
				gameObject = TrafficManager.routes.transform.FindChild("Highway").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			case 3:
				gameObject = TrafficManager.routes.transform.FindChild("HomeRoad").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			case 4:
				gameObject = TrafficManager.routes.transform.FindChild("RoadRace").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			case 5:
				gameObject = TrafficManager.routes.transform.FindChild("Trackfield").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			case 6:
				gameObject = TrafficManager.routes.transform.FindChild("Village").FindChild(waypoint.ToString() ?? "").gameObject;
				break;
			}
			if (gameObject == null)
			{
				Logger.Info(string.Format("Couldn't find waypoint, waypoint: {0}, route: {1}", waypoint, route));
			}
			return gameObject;
		}

		public TrafficManager()
		{
		}

		public static GameObject routes;

		public Dictionary<Transform, ObjectSyncComponent> npcCars = new Dictionary<Transform, ObjectSyncComponent>();

		private const float SYNC_INTERVAL = 5f;

		public float timeToSync;
	}
}
