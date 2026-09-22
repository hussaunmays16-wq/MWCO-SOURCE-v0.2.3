using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using HutongGames.PlayMaker;
using MWCO.Game;
using MWCO.Game.Components;
using MWCO.Game.Objects;
using MWCO.Network;
using MWCO.UI;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO
{
	internal static class DevTools
	{
		public static void OnInit()
		{
			if (DevTools.gameAlreadyLoaded)
			{
				return;
			}
			DevTools.gameAlreadyLoaded = true;
			MWCO.UI.Console.RegisterCommand("help", delegate(string[] args)
			{
				MWCO.UI.Console.ConsoleMessage("Available Commands: \n\n   -clearfps (clears unwanted items)\n   -fixskins (fix no texture skins on players)\n" + (DevTools.CheatsEnabled ? "   -savepos (show current coordinates)\n   -goto [spotName]\n   -gethere [gameObjectName]\n   -gotoitem [gameObjectName]\n   -gotoxyz [x] [y] [z] [Optional: Rotation]   -noclip (fly around!)\n   -getcars (teleports cars to player home)\n   -getkeys (gives all keys to all cars)\n   -settime [hour] (set time of day, 1-24 hour)\n   -forcegamesave (force game save instead of using the toilet)\n   -addmoney [amount]\n" : "   -enablecheats") + "\n......................................................");
			});
			MWCO.UI.Console.RegisterCommand("goto", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length == 1)
				{
					string text = "";
					foreach (KeyValuePair<string, Vector4> keyValuePair in DevTools.Spots)
					{
						text = text + keyValuePair.Key + " ";
					}
					MWCO.UI.Console.ConsoleMessage("Valid Spots: " + text + ".");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				string text2 = args[1].ToLower();
				foreach (KeyValuePair<string, Vector4> keyValuePair2 in DevTools.Spots)
				{
					if (keyValuePair2.Key.ToLower() == text2)
					{
						DevTools.SetPosition(keyValuePair2.Value.x, keyValuePair2.Value.y, keyValuePair2.Value.z, keyValuePair2.Value.w);
						MWCO.UI.Console.ConsoleMessage(string.Format("Teleported to {0}!", keyValuePair2));
						return;
					}
				}
				MWCO.UI.Console.ConsoleMessage(args[1] + " is an invalid spot. Type 'goto' with no parameters to see all the available spots!");
			});
			MWCO.UI.Console.RegisterCommand("cloneplayer", delegate(string[] args)
			{
				Object.Instantiate<GameObject>(DevTools.netManager.players[0].characterGameObject).transform.position = DevTools.netManager.players[0].characterGameObject.transform.position;
			});
			MWCO.UI.Console.RegisterCommand("enablecheats", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					DevTools.CheatsEnabled = true;
					MWCO.UI.Console.ConsoleMessage("Console cheat commands and F4 menu enabled.");
					return;
				}
				MWCO.UI.Console.ConsoleMessage("Cheats are already enabled!");
			});
			MWCO.UI.Console.RegisterCommand("forcegamesave", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				MPController.Instance.ForceGameSave();
			});
			MWCO.UI.Console.RegisterCommand("fixskins", delegate(string[] args)
			{
				foreach (NetPlayer netPlayer in NetManager.Instance.players)
				{
					if (netPlayer != null)
					{
						netPlayer.ApplySkin();
					}
				}
			});
			MWCO.UI.Console.RegisterCommand("gethere", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length == 1)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'gethere [gameObjectName]'.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				string text3 = string.Join(" ", args.Skip(1).ToArray<string>());
				GameObject gameObject = GameObject.Find(text3);
				if (gameObject == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find " + text3 + ".");
					return;
				}
				gameObject.transform.rotation = DevTools.localPlayer.transform.rotation;
				gameObject.transform.position = DevTools.localPlayer.transform.position + DevTools.localPlayer.transform.rotation * Vector3.forward * 5f;
				MWCO.UI.Console.ConsoleMessage("Teleported " + text3 + " to you!");
			});
			MWCO.UI.Console.RegisterCommand("gotoitem", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length == 1)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'gotoitem [gameObjectName]'.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				string text4 = string.Join(" ", args.Skip(1).ToArray<string>());
				GameObject gameObject2 = GameObject.Find(text4);
				if (gameObject2 == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find " + text4 + ".");
					return;
				}
				DevTools.localPlayer.transform.position = gameObject2.transform.position + Vector3.up * 2f;
				MWCO.UI.Console.ConsoleMessage("Teleported to " + text4 + " !");
			});
			MWCO.UI.Console.RegisterCommand("settime", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (!DevTools.netManager.IsHost)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Only host can set world time!");
					return;
				}
				if (args.Length == 1)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'settime [hour]'");
					return;
				}
				if (GameWorld.Instance == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find game world.");
					return;
				}
				int num;
				if (int.TryParse(args[1], out num))
				{
					GameWorld.Instance.WorldTime = (float)num;
					MWCO.UI.Console.ConsoleMessage(string.Format("Set time to {0}!", num));
					return;
				}
				MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find game world.");
			});
			MWCO.UI.Console.RegisterCommand("gotoxyz", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length < 4)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'gotoxyz [x] [y] [z] [Optional: Rotation]'.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				DevTools.localPlayer.transform.position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
				if (args.Length == 5)
				{
					DevTools.localPlayer.transform.eulerAngles = new Vector3(0f, float.Parse(args[4]), 0f);
				}
				MWCO.UI.Console.ConsoleMessage(string.Concat(new string[]
				{
					"Teleported to ",
					args[1],
					", ",
					args[2],
					", ",
					args[3],
					"!"
				}));
			});
			MWCO.UI.Console.RegisterCommand("savepos", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				Logger.Info("Saved Position:");
				Logger.Info(string.Format("Pos: {0}", DevTools.localPlayer.transform.position));
				Logger.Info(string.Format("Rot: {0}", DevTools.localPlayer.transform.eulerAngles));
				MWCO.UI.Console.ConsoleMessage("Saved your position!");
			});
			MWCO.UI.Console.RegisterCommand("getkeys", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				PlayerHouse.getKeys = true;
				PlayerHouse.getKeyes();
				MWCO.UI.Console.ConsoleMessage("Got keys for Bachglotz, Hayosiko, Gifu and Ruscko!");
			});
			MWCO.UI.Console.RegisterCommand("getcars", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				MWCO.UI.Console.ConsoleMessage("Not available yet.");
			});
			MWCO.UI.Console.RegisterCommand("clearfps", delegate(string[] args)
			{
				try
				{
					GameObject[] array = new GameObject[3];
					for (int i = 0; i < array.Length; i++)
					{
						Object.Destroy(array[i]);
					}
					MWCO.UI.Console.ConsoleMessage("Optimization done.");
				}
				catch (Exception)
				{
					MWCO.UI.Console.ConsoleMessage("Already optimized.");
				}
			});
			MWCO.UI.Console.RegisterCommand("addmoney", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				try
				{
					FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value += float.Parse(args[1]);
				}
				catch (Exception)
				{
					MWCO.UI.Console.ConsoleMessage("Wrong usage! do addmoney [amount]");
				}
			});
			MWCO.UI.Console.RegisterCommand("noclip", delegate(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				DevTools.airBreak = !DevTools.airBreak;
				if (DevTools.airBreak)
				{
					MWCO.UI.Console.ConsoleMessage("noclip: ON");
					return;
				}
				MWCO.UI.Console.ConsoleMessage("noclip: OFF");
			});
			MWCO.UI.Console.RegisterCommand("lateload", delegate(string[] args)
			{
				DevTools.lateLoad = true;
			});
		}

		public static void BtnConfig()
		{
			foreach (PlayMakerFSM playMakerFSM in from fsm in GameObject.Find("ButtonContinue").GetComponents<PlayMakerFSM>()
				where fsm.gameObject.name == "ButtonContinue" && fsm.FsmName == "SetSize"
				select fsm)
			{
				DevTools.btnFsm = playMakerFSM;
			}
			if (DevTools.btnFsm != null)
			{
				EventHook.Add(DevTools.btnFsm, "State 1", () => false, false, false);
			}
		}

		public static PlayMakerFSM GetFsm()
		{
			return DevTools.btnFsm;
		}

		public static void BtnClick(PlayMakerFSM btnFsm)
		{
			try
			{
				DevTools.netManager.ShowLoadingScreen(true);
				Application.LoadLevel("GAME");
				DevTools.mainMenu = false;
			}
			catch
			{
				MPController.Instance.ShowMessageBox("You must have an existing save to play online!");
				if (DevTools.netManager.IsPlayer)
				{
					DevTools.netManager.Disconnect();
				}
			}
		}

		public static void BtnSoon()
		{
			MPController.Instance.ShowMessageBox("Coming soon!");
		}

		public static void LoadMenuModel(bool destroy)
		{
			if (!destroy)
			{
				DevTools.player.MenuSpawn();
				DevTools.player.characterGameObject.transform.position = new Vector3(-1f, -0.83f, -6.5f);
				DevTools.player.characterGameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
				DevTools.player.characterGameObject.transform.localScale = new Vector3(4.3f, 4.3f, 4.3f);
				return;
			}
			DevTools.player.Dispose();
		}

		public static void OnGUI()
		{
			if (!DevTools.devView)
			{
				return;
			}
			GUI.skin.label.fontSize = 12;
			DevTools.devMenuButtonsRect.x = 5f;
			DevTools.devMenuButtonsRect.y = 0f;
			DevTools.NewSection("", false);
			DevTools.Checkbox("Net stats", ref DevTools.netStats);
			DevTools.Checkbox("Noclip", ref DevTools.airBreak);
			DevTools.Checkbox("Spawner", ref DevTools.displayVehicleSpawner);
			DevTools.Checkbox("Teleport to", ref DevTools.displayTeleportBtns);
			if (DevTools.displayTeleportBtns)
			{
				if (DevTools.displayVehicleSpawner && DevTools.wasOn)
				{
					DevTools.displayTeleportBtns = false;
					DevTools.wasOn = false;
					return;
				}
				DevTools.NewSection("\t\t\t\tPlaces:", true);
				DevTools.argos[0] = null;
				if (DevTools.Action("Friend"))
				{
					DevTools.netManager.isdown = true;
					DevTools.argos[0] = "Friend";
				}
				if (DevTools.Action("Home"))
				{
					DevTools.argos[0] = "Home";
					DevTools.teleportTo = true;
				}
				if (DevTools.Action("Peraportti"))
				{
					DevTools.argos[0] = "Peraportti";
					DevTools.teleportTo = true;
				}
				if (DevTools.Action("Repair shop"))
				{
					DevTools.argos[0] = "Fleetari";
					DevTools.teleportTo = true;
				}
				if (DevTools.Action("Island"))
				{
					DevTools.argos[0] = "Island";
					DevTools.teleportTo = true;
				}
				if (DevTools.Action("OldHome"))
				{
					DevTools.argos[0] = "OldHome";
					DevTools.teleportTo = true;
				}
				if (DevTools.Action("Drag"))
				{
					DevTools.argos[0] = "Drag";
					DevTools.teleportTo = true;
				}
				if (DevTools.Action("Jokke"))
				{
					DevTools.argos[0] = "Jokkeold";
					DevTools.teleportTo = true;
				}
				DevTools.wasOn = true;
			}
			if (DevTools.displayVehicleSpawner)
			{
				if (DevTools.displayTeleportBtns && DevTools.wasOn1)
				{
					DevTools.displayVehicleSpawner = false;
					DevTools.wasOn1 = false;
					return;
				}
				DevTools.displayTeleportBtns = false;
				DevTools.NewSection("\t\t\t\tVehicles:", true);
				DevTools.argos[0] = null;
				if (DevTools.Action("Sorbet"))
				{
					DevTools.argos[0] = "SORBET(190-200psi)";
					DevTools.spawnVehicle = true;
				}
				if (DevTools.Action("Gifu"))
				{
					DevTools.argos[0] = "GIFU(750/450psi)";
					DevTools.spawnVehicle = true;
				}
				if (DevTools.Action("Kekmet"))
				{
					DevTools.argos[0] = "KEKMET(350-400psi)";
					DevTools.spawnVehicle = true;
				}
				if (DevTools.Action("Backglotz"))
				{
					DevTools.argos[0] = "BACHGLOTZ(1905kg)";
					DevTools.spawnVehicle = true;
				}
				DevTools.wasOn1 = true;
			}
		}

		private static void NewSection(string title, bool o)
		{
			DevTools.devMenuButtonsRect.x = 5f;
			if (!o)
			{
				DevTools.devMenuButtonsRect.y = DevTools.devMenuButtonsRect.y + 5f;
			}
			else
			{
				DevTools.devMenuButtonsRect.y = DevTools.devMenuButtonsRect.y + 22.5f;
			}
			GUI.color = Color.white;
			GUI.Label(DevTools.devMenuButtonsRect, title);
			DevTools.devMenuButtonsRect.x = DevTools.devMenuButtonsRect.x + 120f;
		}

		private static void Checkbox(string name, ref bool state)
		{
			GUI.color = (state ? Color.green : Color.white);
			if (GUI.Button(DevTools.devMenuButtonsRect, name))
			{
				state = !state;
			}
			DevTools.devMenuButtonsRect.x = DevTools.devMenuButtonsRect.x + 110f;
		}

		private static bool Action(string name)
		{
			GUI.color = Color.white;
			bool flag = GUI.Button(DevTools.devMenuButtonsRect, name);
			DevTools.devMenuButtonsRect.x = DevTools.devMenuButtonsRect.x + 110f;
			return flag;
		}

		private static void DrawClosestObjectNames()
		{
			for (int i = 0; i < Object.FindObjectsOfType<GameObject>().Length; i++)
			{
				if (!DevTools.localPlayer || (Object.FindObjectsOfType<GameObject>()[i].transform.position - DevTools.localPlayer.transform.position).sqrMagnitude <= 10f)
				{
					Vector3 vector = Camera.main.WorldToScreenPoint(Object.FindObjectsOfType<GameObject>()[i].transform.position);
					if (vector.z >= 0f)
					{
						GUI.Label(new Rect(vector.x, (float)Screen.height - vector.y, 500f, 20f), Object.FindObjectsOfType<GameObject>()[i].name);
					}
				}
			}
		}

		public static void Restart()
		{
			DevTools.joinedsetup = false;
			DevTools.isDone = 0;
		}

		public static void Update()
		{
			if (DevTools.localPlayer == null)
			{
				DevTools.localPlayer = GameObject.Find("PLAYER");
				DevTools.playerCamera = GameObject.Find("FPSCamera");
				if (DevTools.playerCamera != null)
				{
					DevTools.playerCamera.transform.GetChild(7).GetChild(0).GetChild(3)
						.gameObject.AddComponent<CollisionDetection>();
					foreach (PlayMakerFSM playMakerFSM in DevTools.playerCamera.GetComponents<PlayMakerFSM>())
					{
						if (playMakerFSM.FsmName == "Blindness")
						{
							DevTools.blindnessFsm = playMakerFSM;
							EventHook.Add(DevTools.blindnessFsm, "Reset 2", delegate
							{
								DevTools.blindnessFsm.Fsm.GetFsmFloat("Intensity").Value = 0f;
								DevTools.blindnessFsm.Fsm.GetFsmFloat("MaxAllergy").Value = 128f;
								return false;
							}, false, false);
						}
					}
				}
			}
			else
			{
				DevTools.UpdatePlayer();
			}
			if (Input.GetKeyDown(285) && DevTools.CheatsEnabled)
			{
				DevTools.devView = !DevTools.devView;
			}
			if (Input.GetKeyDown(291))
			{
				DevTools.netStats = !DevTools.netStats;
			}
			if (Input.GetKeyDown(283))
			{
				DevTools.netManager.showNameTags = !DevTools.netManager.showNameTags;
			}
			if (MPController.Instance.actualboat != null && !DevTools.joinedsetup)
			{
				DevTools.netManager.DoSetup();
				DevTools.joinedsetup = true;
			}
			if (MPController.Instance.boat != null && DevTools.isDone == 0)
			{
				DevTools.netManager.JoinedSetup();
				GamePlayer.Instance.LoadRaycast();
				DevTools.RemoveDeath();
				if (MPController.Instance.modLoaderExists && DevTools.netManager.IsHost)
				{
					Chat.Instance.AddMessage("Mod loader detected: Only use supported mods. Make sure all players have an identical mod folder otherwise bugs will occur.", MessageSeverity.Error);
				}
				DevTools.isDone++;
			}
			if (Input.GetKeyDown(323))
			{
				try
				{
					if (GamePlayer.Instance.isDropped() || GamePlayer.Instance.isPicked())
					{
						GamePlayer.Instance.LookForObject();
					}
				}
				catch
				{
				}
			}
		}

		public static void UpdatePlayer()
		{
			if (DevTools.spawnVehicle)
			{
				string ourObjectName = DevTools.argos[0];
				GameObject gameObject = GameObject.Find(DevTools.argos[0]);
				IEnumerable<GameObject> enumerable = (Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]).Cast<GameObject>();
				Func<GameObject, bool> func;
				Func<GameObject, bool> <>9__0;
				if ((func = <>9__0) == null)
				{
					func = (<>9__0 = (GameObject go) => go.name == ourObjectName);
				}
				using (IEnumerator<GameObject> enumerator = enumerable.Where(func).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						gameObject = enumerator.Current;
					}
				}
				if (gameObject == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find " + ourObjectName + ".");
					return;
				}
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(true);
				}
				gameObject.transform.rotation = DevTools.localPlayer.transform.rotation;
				Vector3 vector = DevTools.localPlayer.transform.rotation * Vector3.forward * 5f;
				vector.y += 0.3f;
				gameObject.transform.position = DevTools.localPlayer.transform.position + vector;
				Logger.Debug("Sending reposition sync for " + gameObject.name);
				ObjectSyncComponent component = gameObject.GetComponent<ObjectSyncComponent>();
				if (component != null)
				{
					component.SendObjectSync(ObjectSyncManager.SyncTypes.ForceSetOwner, false, false);
				}
				ObjectSyncComponent component2 = gameObject.GetComponent<ObjectSyncComponent>();
				if (component2 != null)
				{
					component2.SendEnterSync();
				}
				MWCO.UI.Console.ConsoleMessage("Teleported " + ourObjectName + " to you!");
				DevTools.spawnVehicle = false;
			}
			if (DevTools.teleportTo)
			{
				try
				{
					string ourSpot = DevTools.argos[0].ToLower();
					IEnumerable<KeyValuePair<string, Vector4>> spots = DevTools.Spots;
					Func<KeyValuePair<string, Vector4>, bool> func2;
					Func<KeyValuePair<string, Vector4>, bool> <>9__1;
					if ((func2 = <>9__1) == null)
					{
						func2 = (<>9__1 = (KeyValuePair<string, Vector4> spot) => spot.Key.ToLower() == ourSpot);
					}
					foreach (KeyValuePair<string, Vector4> keyValuePair in spots.Where(func2))
					{
						DevTools.SetPosition(keyValuePair.Value.x, keyValuePair.Value.y, keyValuePair.Value.z, keyValuePair.Value.w);
						MWCO.UI.Console.ConsoleMessage(string.Format("Teleported to {0}!", keyValuePair));
					}
					DevTools.teleportTo = false;
				}
				catch (Exception)
				{
				}
			}
			if (DevTools.airBreak)
			{
				float num = 0.5f;
				if (Input.GetKey(323))
				{
					num += 3f;
				}
				if (Input.GetKey(324))
				{
					num -= 0.25f;
				}
				DevTools.localPlayer.GetComponent<CharacterController>().enabled = false;
				if (Input.GetKey(43))
				{
					DevTools.localPlayer.transform.position = DevTools.localPlayer.transform.position + Vector3.up * num;
				}
				if (Input.GetKey(45))
				{
					DevTools.localPlayer.transform.position = DevTools.localPlayer.transform.position - Vector3.up * num;
				}
				if (Input.GetKey(119))
				{
					DevTools.localPlayer.transform.position = DevTools.localPlayer.transform.position + DevTools.playerCamera.transform.rotation * Vector3.forward * num;
				}
				if (Input.GetKey(115))
				{
					DevTools.localPlayer.transform.position = DevTools.localPlayer.transform.position - DevTools.playerCamera.transform.rotation * Vector3.forward * num;
				}
				if (Input.GetKey(97))
				{
					DevTools.localPlayer.transform.position = DevTools.localPlayer.transform.position - DevTools.localPlayer.transform.rotation * Vector3.right * num;
				}
				if (Input.GetKey(100))
				{
					DevTools.localPlayer.transform.position = DevTools.localPlayer.transform.position + DevTools.localPlayer.transform.rotation * Vector3.right * num;
				}
				DevTools.wasOn2 = true;
				return;
			}
			if (!DevTools.airBreak && DevTools.wasOn2)
			{
				DevTools.wasOn2 = false;
				DevTools.localPlayer.GetComponent<CharacterController>().enabled = true;
			}
		}

		public static void EnableDeath()
		{
			DevTools.localPlayer.GetComponent<CharacterController>().enabled = false;
			DevTools.localPlayer.GetComponent<CharacterMotor>().enabled = false;
			DevTools.deathObj.SetActive(true);
		}

		private static void RemoveDeath()
		{
			foreach (PlayMakerFSM playMakerFSM in GameObject.Find("Systems").GetComponentsInChildren<PlayMakerFSM>(true))
			{
				if (playMakerFSM.FsmName == "Activate Dead Body" && playMakerFSM.gameObject.name == "Death")
				{
					playMakerFSM.enabled = false;
					DevTools.deathObj = playMakerFSM.gameObject;
					return;
				}
			}
		}

		public static void DumpObject(Transform obj, string file)
		{
			StringBuilder bldr = new StringBuilder();
			Utils.PrintTransformTree(obj, 0, delegate(int level, string text)
			{
				for (int i = 0; i < level; i++)
				{
					bldr.Append("    ");
				}
				bldr.Append(text + "\n");
			});
			File.WriteAllText(file, bldr.ToString());
		}

		private static void SetPosition(float x, float y, float z, float rot = -1f)
		{
			DevTools.localPlayer.transform.position = new Vector3(x, y, z);
			if (rot != -1f)
			{
				DevTools.localPlayer.transform.eulerAngles = new Vector3(0f, rot, 0f);
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static DevTools()
		{
		}

		private static bool devView = false;

		private static bool mainMenu = true;

		public static PlayMakerFSM[] fsms = new PlayMakerFSM[10];

		private static bool airBreak = false;

		private static bool displayVehicleSpawner = false;

		private static bool spawnVehicle = false;

		private static bool displayTeleportBtns = false;

		private static bool teleportTo = false;

		public static bool gameAlreadyLoaded = false;

		public static bool lateLoad = false;

		private static bool wasOn = false;

		private static bool wasOn1 = false;

		private static bool wasOn2 = false;

		private static int isDone = 0;

		public static string[] argos = new string[1];

		public static bool netStats = false;

		public static bool displayPlayerDebug = false;

		private static PlayMakerFSM btnFsm = null;

		public static PlayMakerFSM blindnessFsm = null;

		public static NetManager netManager = NetManager.Instance;

		public static GameObject localPlayer = null;

		public static GameObject playerCamera = null;

		private const float DEV_MENU_BUTTON_WIDTH = 110f;

		private const float TITLE_SECTION_WIDTH = 120f;

		private static Rect devMenuButtonsRect = new Rect(5f, 0f, 110f, 22.5f);

		public static bool CheatsEnabled = false;

		private static Dictionary<string, Vector4> Spots = new Dictionary<string, Vector4>
		{
			{
				"Home",
				new Vector4(-1286.197f, 0.44f, 1076.415f, 30f)
			},
			{
				"OldHome",
				new Vector4(-10f, -0.3f, 7.6f, 180f)
			},
			{
				"Island",
				new Vector4(-851.5f, -2.9f, 516.6f, 163f)
			},
			{
				"Peraportti",
				new Vector4(-1729.715f, 3.4507f, 942.3057f, 180f)
			},
			{
				"Fleetari",
				new Vector4(1550f, 4.8f, 734.3f, 62f)
			},
			{
				"Pigman",
				new Vector4(-171.1f, -3.9f, 1024.9f, 133f)
			},
			{
				"Jokkeold",
				new Vector4(1939.4f, 7.1f, -222.5f, 126f)
			},
			{
				"Jokkenew",
				new Vector4(-1285.6f, 0.2f, 1088.3f, 210f)
			},
			{
				"Drag",
				new Vector4(-1312.6f, 2.2f, -937.6f, 99f)
			}
		};

		public static NetPlayer player = new NetPlayer(NetManager.Instance, NetWorld.Instance, SteamUser.GetSteamID());

		private static bool joinedsetup;

		private static GameObject deathObj = null;

		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			// Note: this type is marked as 'beforefieldinit'.
			static <>c()
			{
			}

			public <>c()
			{
			}

			internal void <OnInit>b__27_0(string[] args)
			{
				MWCO.UI.Console.ConsoleMessage("Available Commands: \n\n   -clearfps (clears unwanted items)\n   -fixskins (fix no texture skins on players)\n" + (DevTools.CheatsEnabled ? "   -savepos (show current coordinates)\n   -goto [spotName]\n   -gethere [gameObjectName]\n   -gotoitem [gameObjectName]\n   -gotoxyz [x] [y] [z] [Optional: Rotation]   -noclip (fly around!)\n   -getcars (teleports cars to player home)\n   -getkeys (gives all keys to all cars)\n   -settime [hour] (set time of day, 1-24 hour)\n   -forcegamesave (force game save instead of using the toilet)\n   -addmoney [amount]\n" : "   -enablecheats") + "\n......................................................");
			}

			internal void <OnInit>b__27_1(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length == 1)
				{
					string text = "";
					foreach (KeyValuePair<string, Vector4> keyValuePair in DevTools.Spots)
					{
						text = text + keyValuePair.Key + " ";
					}
					MWCO.UI.Console.ConsoleMessage("Valid Spots: " + text + ".");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				string text2 = args[1].ToLower();
				foreach (KeyValuePair<string, Vector4> keyValuePair2 in DevTools.Spots)
				{
					if (keyValuePair2.Key.ToLower() == text2)
					{
						DevTools.SetPosition(keyValuePair2.Value.x, keyValuePair2.Value.y, keyValuePair2.Value.z, keyValuePair2.Value.w);
						MWCO.UI.Console.ConsoleMessage(string.Format("Teleported to {0}!", keyValuePair2));
						return;
					}
				}
				MWCO.UI.Console.ConsoleMessage(args[1] + " is an invalid spot. Type 'goto' with no parameters to see all the available spots!");
			}

			internal void <OnInit>b__27_2(string[] args)
			{
				Object.Instantiate<GameObject>(DevTools.netManager.players[0].characterGameObject).transform.position = DevTools.netManager.players[0].characterGameObject.transform.position;
			}

			internal void <OnInit>b__27_3(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					DevTools.CheatsEnabled = true;
					MWCO.UI.Console.ConsoleMessage("Console cheat commands and F4 menu enabled.");
					return;
				}
				MWCO.UI.Console.ConsoleMessage("Cheats are already enabled!");
			}

			internal void <OnInit>b__27_4(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				MPController.Instance.ForceGameSave();
			}

			internal void <OnInit>b__27_5(string[] args)
			{
				foreach (NetPlayer netPlayer in NetManager.Instance.players)
				{
					if (netPlayer != null)
					{
						netPlayer.ApplySkin();
					}
				}
			}

			internal void <OnInit>b__27_6(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length == 1)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'gethere [gameObjectName]'.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				string text = string.Join(" ", args.Skip(1).ToArray<string>());
				GameObject gameObject = GameObject.Find(text);
				if (gameObject == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find " + text + ".");
					return;
				}
				gameObject.transform.rotation = DevTools.localPlayer.transform.rotation;
				gameObject.transform.position = DevTools.localPlayer.transform.position + DevTools.localPlayer.transform.rotation * Vector3.forward * 5f;
				MWCO.UI.Console.ConsoleMessage("Teleported " + text + " to you!");
			}

			internal void <OnInit>b__27_7(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length == 1)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'gotoitem [gameObjectName]'.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				string text = string.Join(" ", args.Skip(1).ToArray<string>());
				GameObject gameObject = GameObject.Find(text);
				if (gameObject == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find " + text + ".");
					return;
				}
				DevTools.localPlayer.transform.position = gameObject.transform.position + Vector3.up * 2f;
				MWCO.UI.Console.ConsoleMessage("Teleported to " + text + " !");
			}

			internal void <OnInit>b__27_8(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (!DevTools.netManager.IsHost)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Only host can set world time!");
					return;
				}
				if (args.Length == 1)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'settime [hour]'");
					return;
				}
				if (GameWorld.Instance == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find game world.");
					return;
				}
				int num;
				if (int.TryParse(args[1], out num))
				{
					GameWorld.Instance.WorldTime = (float)num;
					MWCO.UI.Console.ConsoleMessage(string.Format("Set time to {0}!", num));
					return;
				}
				MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find game world.");
			}

			internal void <OnInit>b__27_9(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (args.Length < 4)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Invalid syntax. Use 'gotoxyz [x] [y] [z] [Optional: Rotation]'.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				DevTools.localPlayer.transform.position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
				if (args.Length == 5)
				{
					DevTools.localPlayer.transform.eulerAngles = new Vector3(0f, float.Parse(args[4]), 0f);
				}
				MWCO.UI.Console.ConsoleMessage(string.Concat(new string[]
				{
					"Teleported to ",
					args[1],
					", ",
					args[2],
					", ",
					args[3],
					"!"
				}));
			}

			internal void <OnInit>b__27_10(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				if (DevTools.localPlayer == null)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Couldn't find local player.");
					return;
				}
				Logger.Info("Saved Position:");
				Logger.Info(string.Format("Pos: {0}", DevTools.localPlayer.transform.position));
				Logger.Info(string.Format("Rot: {0}", DevTools.localPlayer.transform.eulerAngles));
				MWCO.UI.Console.ConsoleMessage("Saved your position!");
			}

			internal void <OnInit>b__27_11(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				PlayerHouse.getKeys = true;
				PlayerHouse.getKeyes();
				MWCO.UI.Console.ConsoleMessage("Got keys for Bachglotz, Hayosiko, Gifu and Ruscko!");
			}

			internal void <OnInit>b__27_12(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				MWCO.UI.Console.ConsoleMessage("Not available yet.");
			}

			internal void <OnInit>b__27_13(string[] args)
			{
				try
				{
					GameObject[] array = new GameObject[3];
					for (int i = 0; i < array.Length; i++)
					{
						Object.Destroy(array[i]);
					}
					MWCO.UI.Console.ConsoleMessage("Optimization done.");
				}
				catch (Exception)
				{
					MWCO.UI.Console.ConsoleMessage("Already optimized.");
				}
			}

			internal void <OnInit>b__27_14(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				try
				{
					FsmVariables.GlobalVariables.FindFsmFloat("PlayerMoney").Value += float.Parse(args[1]);
				}
				catch (Exception)
				{
					MWCO.UI.Console.ConsoleMessage("Wrong usage! do addmoney [amount]");
				}
			}

			internal void <OnInit>b__27_15(string[] args)
			{
				if (!DevTools.CheatsEnabled)
				{
					MWCO.UI.Console.ConsoleMessage("ERROR: Cheats not enabled.");
					return;
				}
				DevTools.airBreak = !DevTools.airBreak;
				if (DevTools.airBreak)
				{
					MWCO.UI.Console.ConsoleMessage("noclip: ON");
					return;
				}
				MWCO.UI.Console.ConsoleMessage("noclip: OFF");
			}

			internal void <OnInit>b__27_16(string[] args)
			{
				DevTools.lateLoad = true;
			}

			internal bool <BtnConfig>b__28_1(PlayMakerFSM fsm)
			{
				return fsm.gameObject.name == "ButtonContinue" && fsm.FsmName == "SetSize";
			}

			internal bool <BtnConfig>b__28_0()
			{
				return false;
			}

			internal bool <Update>b__41_0()
			{
				DevTools.blindnessFsm.Fsm.GetFsmFloat("Intensity").Value = 0f;
				DevTools.blindnessFsm.Fsm.GetFsmFloat("MaxAllergy").Value = 128f;
				return false;
			}

			public static readonly DevTools.<>c <>9 = new DevTools.<>c();

			public static MWCO.UI.Console.CommandDelegate <>9__27_0;

			public static MWCO.UI.Console.CommandDelegate <>9__27_1;

			public static MWCO.UI.Console.CommandDelegate <>9__27_2;

			public static MWCO.UI.Console.CommandDelegate <>9__27_3;

			public static MWCO.UI.Console.CommandDelegate <>9__27_4;

			public static MWCO.UI.Console.CommandDelegate <>9__27_5;

			public static MWCO.UI.Console.CommandDelegate <>9__27_6;

			public static MWCO.UI.Console.CommandDelegate <>9__27_7;

			public static MWCO.UI.Console.CommandDelegate <>9__27_8;

			public static MWCO.UI.Console.CommandDelegate <>9__27_9;

			public static MWCO.UI.Console.CommandDelegate <>9__27_10;

			public static MWCO.UI.Console.CommandDelegate <>9__27_11;

			public static MWCO.UI.Console.CommandDelegate <>9__27_12;

			public static MWCO.UI.Console.CommandDelegate <>9__27_13;

			public static MWCO.UI.Console.CommandDelegate <>9__27_14;

			public static MWCO.UI.Console.CommandDelegate <>9__27_15;

			public static MWCO.UI.Console.CommandDelegate <>9__27_16;

			public static Func<PlayMakerFSM, bool> <>9__28_1;

			public static Func<bool> <>9__28_0;

			public static Func<bool> <>9__41_0;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass42_0
		{
			public <>c__DisplayClass42_0()
			{
			}

			internal bool <UpdatePlayer>b__0(GameObject go)
			{
				return go.name == this.ourObjectName;
			}

			public string ourObjectName;

			public Func<GameObject, bool> <>9__0;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass42_1
		{
			public <>c__DisplayClass42_1()
			{
			}

			internal bool <UpdatePlayer>b__1(KeyValuePair<string, Vector4> spot)
			{
				return spot.Key.ToLower() == this.ourSpot;
			}

			public string ourSpot;

			public Func<KeyValuePair<string, Vector4>, bool> <>9__1;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass46_0
		{
			public <>c__DisplayClass46_0()
			{
			}

			internal void <DumpObject>b__0(int level, string text)
			{
				for (int i = 0; i < level; i++)
				{
					this.bldr.Append("    ");
				}
				this.bldr.Append(text + "\n");
			}

			public StringBuilder bldr;
		}
	}
}
