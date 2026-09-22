using System;
using UnityEngine;

namespace MWCO.Game
{
	internal static class GameCallbacks
	{
		public static GameCallbacks.OnObjectPickup onObjectPickup;

		public static GameCallbacks.OnObjectRelease onObjectRelease;

		public static GameCallbacks.OnLocalPlayerCreated onLocalPlayerCreated;

		public static GameCallbacks.OnWorldLoad onWorldLoad;

		public static GameCallbacks.OnWorldUnload onWorldUnload;

		public static GameCallbacks.OnPlayMakerObjectCreate onPlayMakerObjectCreate;

		public static GameCallbacks.OnPlayMakerObjectDestroy onPlayMakerObjectDestroy;

		public static GameCallbacks.OnPlayMakerObjectActivate onPlayMakerObjectActivate;

		public static GameCallbacks.OnPlayMakerSetPosition onPlayMakerSetPosition;

		public delegate void OnObjectPickup(GameObject gameObj);

		public delegate void OnObjectRelease(bool drop);

		public delegate void OnLocalPlayerCreated();

		public delegate void OnWorldLoad();

		public delegate void OnWorldUnload();

		public delegate void OnPlayMakerObjectCreate(GameObject instance, GameObject prefab);

		public delegate void OnPlayMakerObjectDestroy(GameObject instance);

		public delegate void OnPlayMakerObjectActivate(GameObject instance, bool activate);

		public delegate void OnPlayMakerSetPosition(GameObject instance, Vector3 newPosition, Space space);
	}
}
