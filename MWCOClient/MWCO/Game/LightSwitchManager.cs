using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MWCO.Game.Objects;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class LightSwitchManager : IGameObjectCollector
	{
		public LightSwitchManager()
		{
			LightSwitchManager.Instance = this;
		}

		~LightSwitchManager()
		{
			LightSwitchManager.Instance = null;
		}

		private bool IsLightSwitch(GameObject gameObject)
		{
			return gameObject.name.StartsWith("switch_");
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (this.IsLightSwitch(gameObject) && this.GetLightSwitchByGameObject(gameObject) == null)
			{
				this.AddLightSwitch(gameObject);
			}
		}

		public void DestroyObjects()
		{
			this.lightSwitches.Clear();
		}

		public void DestroyObject(GameObject gameObject)
		{
			if (this.IsLightSwitch(gameObject))
			{
				LightSwitch lightSwitchByGameObject = this.GetLightSwitchByGameObject(gameObject);
				if (lightSwitchByGameObject != null)
				{
					this.lightSwitches.Remove(lightSwitchByGameObject);
				}
			}
		}

		public void AddLightSwitch(GameObject lightGO)
		{
			PlayMakerFSM playmakerScriptByName = Utils.GetPlaymakerScriptByName(lightGO, "Use");
			if (playmakerScriptByName == null)
			{
				return;
			}
			bool flag = false;
			if (playmakerScriptByName.FsmVariables.FindFsmBool("Switch") != null)
			{
				flag = true;
			}
			if (flag)
			{
				LightSwitch light = new LightSwitch(lightGO);
				this.lightSwitches.Add(light);
				Logger.Info("Registered new light switch: " + lightGO.name);
				light.onLightSwitchUse = delegate(GameObject lightObj, bool turnedOn)
				{
					this.onLightSwitchUsed(lightGO, !light.SwitchStatus);
				};
			}
		}

		public LightSwitch FindLightSwitch(Vector3 pos)
		{
			foreach (LightSwitch lightSwitch in this.lightSwitches)
			{
				if (Vector3.Distance(lightSwitch.Position, pos) < 0.1f)
				{
					return lightSwitch;
				}
			}
			return null;
		}

		public LightSwitch GetLightSwitchByGameObject(GameObject gameObject)
		{
			foreach (LightSwitch lightSwitch in this.lightSwitches)
			{
				if (lightSwitch.GameObject == gameObject)
				{
					return lightSwitch;
				}
			}
			return null;
		}

		public static LightSwitchManager Instance;

		public List<LightSwitch> lightSwitches = new List<LightSwitch>();

		public LightSwitchManager.OnLightSwitchUsed onLightSwitchUsed;

		public delegate void OnLightSwitchUsed(GameObject lightSwitch, bool turnedOn);

		[CompilerGenerated]
		private sealed class <>c__DisplayClass10_0
		{
			public <>c__DisplayClass10_0()
			{
			}

			internal void <AddLightSwitch>b__0(GameObject lightObj, bool turnedOn)
			{
				this.<>4__this.onLightSwitchUsed(this.lightGO, !this.light.SwitchStatus);
			}

			public LightSwitchManager <>4__this;

			public GameObject lightGO;

			public LightSwitch light;
		}
	}
}
