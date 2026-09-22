using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MWCO.Game.Objects;
using UnityEngine;

namespace MWCO.Game
{
	internal class GameDoorsManager : IGameObjectCollector
	{
		public GameDoorsManager()
		{
			GameDoorsManager.Instance = this;
		}

		~GameDoorsManager()
		{
			GameDoorsManager.Instance = null;
		}

		public void DestroyObjects()
		{
			this.doors.Clear();
		}

		public void HandleDoorsAction(GameDoor door, bool open)
		{
			if (open)
			{
				GameDoorsManager.OnDoorsOpen onDoorsOpen = this.onDoorsOpen;
				if (onDoorsOpen == null)
				{
					return;
				}
				onDoorsOpen(door.GameObject);
				return;
			}
			else
			{
				GameDoorsManager.OnDoorsClose onDoorsClose = this.onDoorsClose;
				if (onDoorsClose == null)
				{
					return;
				}
				onDoorsClose(door.GameObject);
				return;
			}
		}

		private bool IsDoorGameObject(GameObject gameObject)
		{
			if (!gameObject.name.StartsWith("Door"))
			{
				return false;
			}
			if (gameObject.transform.childCount == 0)
			{
				return false;
			}
			Transform child = gameObject.transform.GetChild(0);
			if (child == null || child.name != "Pivot")
			{
				return false;
			}
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(gameObject, "Use");
			if (playmakerScriptByName == null)
			{
				return false;
			}
			bool flag = false;
			FsmEvent[] fsmEvents = playmakerScriptByName.FsmEvents;
			for (int i = 0; i < fsmEvents.Length; i++)
			{
				if (fsmEvents[i].Name == "OPENDOOR")
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (this.IsDoorGameObject(gameObject) && this.GetDoorByGameObject(gameObject) == null)
			{
				GameDoor gameDoor = new GameDoor(this, gameObject);
				this.doors.Add(gameDoor);
			}
		}

		public void DestroyObject(GameObject gameObject)
		{
			if (!this.IsDoorGameObject(gameObject))
			{
				return;
			}
			GameDoor doorByGameObject = this.GetDoorByGameObject(gameObject);
			if (doorByGameObject != null)
			{
				this.doors.Remove(doorByGameObject);
			}
		}

		public GameDoor FindGameDoors(Vector3 position)
		{
			foreach (GameDoor gameDoor in this.doors)
			{
				if (gameDoor.Position == position)
				{
					return gameDoor;
				}
			}
			return null;
		}

		public GameDoor GetDoorByGameObject(GameObject gameObject)
		{
			foreach (GameDoor gameDoor in this.doors)
			{
				if (gameDoor.GameObject == gameObject)
				{
					return gameDoor;
				}
			}
			return null;
		}

		public static GameDoorsManager Instance;

		public List<GameDoor> doors = new List<GameDoor>();

		public GameDoorsManager.OnDoorsOpen onDoorsOpen;

		public GameDoorsManager.OnDoorsClose onDoorsClose;

		public delegate void OnDoorsOpen(GameObject door);

		public delegate void OnDoorsClose(GameObject door);
	}
}
