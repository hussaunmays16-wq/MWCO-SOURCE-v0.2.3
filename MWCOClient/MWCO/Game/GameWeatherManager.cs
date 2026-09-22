using System;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Game.Objects.PickupableTypes;
using MWCO.Network;
using MWCO.Network.Messages;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO.Game
{
	internal class GameWeatherManager : IGameObjectCollector
	{
		public float CloudsX
		{
			get
			{
				return this.weatherSystemFSM.FsmVariables.GetFsmFloat("CloudsX").Value;
			}
			set
			{
				this.weatherSystemFSM.FsmVariables.GetFsmFloat("CloudsX").Value = value;
			}
		}

		public float CloudsZ
		{
			get
			{
				return this.weatherSystemFSM.FsmVariables.GetFsmFloat("CloudsZ").Value;
			}
			set
			{
				this.weatherSystemFSM.FsmVariables.GetFsmFloat("CloudsZ").Value = value;
			}
		}

		public float WeatherPos
		{
			get
			{
				return this.weatherSystemFSM.FsmVariables.GetFsmFloat("WeatherPos").Value;
			}
			set
			{
				this.weatherSystemFSM.FsmVariables.GetFsmFloat("WeatherPos").Value = value;
			}
		}

		public float CloudsRot
		{
			get
			{
				return this.weatherSystemFSM.FsmVariables.GetFsmFloat("CloudsRotY").Value;
			}
			set
			{
				this.weatherSystemFSM.FsmVariables.GetFsmFloat("CloudsRotY").Value = value;
			}
		}

		public GameWeatherManager()
		{
			GameWeatherManager.Instance = this;
		}

		public void CollectGameObject(GameObject gameObject, int assignedID = 0)
		{
			if (gameObject.name != "Clouds")
			{
				return;
			}
			ErrorHandling.Assert(gameObject != null, "cloudSystem couldn't be found!");
			this.weatherSystemFSM = Utils.GetPlaymakerScriptByName(gameObject, "Weather");
			this.cloudObjects = gameObject.transform.GetChild(0);
		}

		public void SetupWeatherStates()
		{
			FsmState[] states = this.weatherSystemFSM.Fsm.States;
			for (int i = 0; i < states.Length; i++)
			{
				FsmState fsmState = states[i];
				string sn = fsmState.Name;
				if (sn.Length < 3)
				{
					EventHook.Add(this.weatherSystemFSM, sn, delegate
					{
						if (NetManager.Instance.IsHost)
						{
							this.SendWeatherSync(sn);
						}
						return false;
					}, false, false);
				}
			}
			GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				foreach (PlayMakerFSM playMakerFSM in array[i].GetComponents<PlayMakerFSM>())
				{
					if (playMakerFSM.FsmName == "Screw" && playMakerFSM.name == "BoltPM" && playMakerFSM.GetComponent<Bolt>() == null)
					{
						Bolt bolt = playMakerFSM.gameObject.AddComponent<Bolt>();
						bolt.BoltID = GameVehicleDatabase.Instance.bolts.Count;
						GameVehicleDatabase.Instance.bolts.Add(bolt);
					}
				}
			}
		}

		public void DestroyObjects()
		{
			this.weatherSystemFSM = null;
		}

		public void DestroyObject(GameObject gameObject)
		{
			if (this.weatherSystemFSM != null && this.weatherSystemFSM.gameObject == gameObject)
			{
				this.weatherSystemFSM = null;
			}
		}

		public void SetWeather(WeatherUpdateMessage message)
		{
			try
			{
				this.WeatherPos = message.weatherPos;
				this.CloudsX = message.cloudsX;
				this.CloudsZ = message.cloudsZ;
				this.CloudsRot = message.cloudsRot;
				this.cloudObjects.gameObject.SetActive(message.CloudsEnabled);
			}
			catch (Exception)
			{
			}
		}

		private void SendWeatherSync(string stateName)
		{
			WeatherUpdateMessage weatherUpdateMessage = new WeatherUpdateMessage();
			this.WriteWeather(weatherUpdateMessage);
			weatherUpdateMessage.StateName = stateName;
			weatherUpdateMessage.CloudsEnabled = this.cloudObjects.gameObject.activeSelf;
			NetManager.Instance.BroadcastMessage<WeatherUpdateMessage>(weatherUpdateMessage, 2, 0);
		}

		public void ReadWeatherSync(WeatherUpdateMessage msg)
		{
			this.SetWeather(msg);
			this.cloudObjects.gameObject.SetActive(msg.CloudsEnabled);
			this.cloudObjects.localPosition = new Vector3(msg.cloudsX, this.cloudObjects.localPosition.y, msg.cloudsZ);
			this.cloudObjects.localEulerAngles = new Vector3(0f, msg.cloudsRot, 0f);
			this.weatherSystemFSM.SendEvent("MP_" + msg.StateName);
		}

		public void WriteWeather(WeatherUpdateMessage message)
		{
			message.weatherPos = this.WeatherPos;
			message.cloudsX = this.CloudsX;
			message.cloudsZ = this.CloudsZ;
			message.cloudsRot = this.CloudsRot;
			message.CloudsEnabled = this.cloudObjects.transform.gameObject.activeSelf;
		}

		public static GameWeatherManager Instance;

		public PlayMakerFSM weatherSystemFSM;

		private Transform cloudObjects;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass17_0
		{
			public <>c__DisplayClass17_0()
			{
			}

			internal bool <SetupWeatherStates>b__0()
			{
				if (NetManager.Instance.IsHost)
				{
					this.<>4__this.SendWeatherSync(this.sn);
				}
				return false;
			}

			public string sn;

			public GameWeatherManager <>4__this;
		}
	}
}
