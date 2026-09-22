using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using HutongGames.PlayMaker;
using MWCO.Game;
using MWCO.Network;
using MWCO.Network.Messages;
using MWCO.UI;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MWCO
{
	internal class MPController : MonoBehaviour
	{
		public GameObject PlayerGameObject
		{
			get
			{
				if (this.cachedPlayerGameObject == null)
				{
					this.cachedPlayerGameObject = GameObject.Find("PLAYER");
				}
				return this.cachedPlayerGameObject;
			}
		}

		public int AccessLevel
		{
			get
			{
				return this.accesslevel;
			}
		}

		private void Start()
		{
			if (MPController.Instance != null && MPController.Instance != this)
			{
				Object.Destroy(this);
				return;
			}
			MPController.Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			this.HTTPRequestCompletedCallback = new CallResult<HTTPRequestCompleted_t>(new CallResult<HTTPRequestCompleted_t>.APIDispatchDelegate(this.OnHTTPRequestCompleted));
			SteamAPI.Init();
			if (this.netManager == null)
			{
				this.netManager = new NetManager();
			}
			IMGUIUtils.Setup();
			DevTools.OnInit();
			Application.LoadLevel("MainMenu");
			Application.logMessageReceived += new Application.LogCallback(this.HandleLog);
			if (this.DEVELOPEROPTIONS)
			{
				this.accesslevel = 4;
				MPController.access = true;
				this.timerIsRunning = true;
			}
			else
			{
				this.LoadButtons();
			}
			this.netManager.RefreshSave();
			this.GAME_BUILD_ID = SteamApps.GetAppBuildId();
			MPController.GenerateIdiNahouiMethods("C:\\Users\\nikat\\Desktop\\MWCO\\lol.txt");
		}

		public void Punched()
		{
			if (NetLocalPlayer.Instance.currentVehicle != null)
			{
				return;
			}
			if (this.punchRecovery <= 0f)
			{
				this.punchCount = 0;
			}
			this.punchRecovery = 60f;
			this.punchCount++;
			if (this.punchCount >= 3)
			{
				this.KnockOutObj.SetActive(true);
				this.punchCount = 0;
			}
		}

		private void ClearDebris()
		{
			if (GameObject.Find("MSCLoader") != null)
			{
				this.modLoaderExists = true;
				Object.Destroy(GameObject.Find("MSCUnloader"));
			}
			Object.Destroy(GameObject.Find("Scene/Quad 7"));
			Object.Destroy(GameObject.Find("InterfaceActive/ColorButtonsNew"));
			this.MoveButtons();
			this.backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.6f));
			this.backgroundTexture.Apply();
			this.style.font = Client.LoadAsset<Font>("Assets/UI/Roboto-Regular.ttf");
			this.style.fontSize = 22;
			this.roboStyle.font = Client.LoadAsset<Font>("Assets/UI/Roboto-Black.ttf");
			this.roboStyle.fontSize = 24;
			if (GameObject.Find("MSCLoader MB(Clone)") != null)
			{
				Object.Destroy(GameObject.Find("MSCLoader MB(Clone)"));
			}
		}

		private void MoveButtons()
		{
			GameObject[] array = new GameObject[3];
			Transform transform = GameObject.Find("Interface/Buttons/ButtonContinue").transform;
			array[0] = Object.Instantiate<GameObject>(transform.gameObject);
			array[1] = (GameObject)Object.Instantiate(transform.gameObject, new Vector3(0.245f, -1.77f, -5.8f), Quaternion.identity);
			array[2] = GameObject.Find("Interface/Buttons/ButtonQuit");
			array[0].transform.SetParent(transform.parent);
			array[0].transform.localPosition = new Vector3(0.337f, 0.14f, 0f);
			array[0].transform.localRotation = transform.localRotation;
			array[0].transform.localScale = transform.localScale;
			for (int i = 0; i < array.Length; i++)
			{
				if (i < 3)
				{
					array[i].AddComponent<CustomButton>().Setup(i, false);
				}
				else
				{
					array[i].transform.position += array[i].transform.localPosition * 10f;
				}
			}
			Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault((GameObject go) => go.name == "Licence").transform.GetChild(1).GetChild(0).gameObject.AddComponent<CustomButton>().Setup(3, false);
			transform.gameObject.AddComponent<CustomButton>().Setup(4, false);
			MeshRenderer component = GameObject.Find("Scene/LOGO/logo 1").GetComponent<MeshRenderer>();
			Texture2D texture2D = Client.LoadAsset<Texture2D>("Assets/Textures/Logo-HQ.png");
			component.material = new Material(component.material)
			{
				mainTexture = texture2D
			};
			GameObject.Find("Interface/Songs").transform.localPosition = new Vector3(1.9318f, 1.3694f, 3.7054f);
			if (!File.Exists(Path.Combine(Application.persistentDataPath, "savefile.txt")))
			{
				this.ShowMessageBox("Save File not detected. Please create a New Game in single player and press quit once loaded. This process is required to start playing online.");
			}
		}

		private void OnLevelSwitch(string newLevelName)
		{
			Logger.Debug("Switching from " + this.currentLevelName + " to " + newLevelName);
			if (this.SINGLEPLAYER)
			{
				if (this.currentLevelName == "GAME" && newLevelName == "MainMenu")
				{
					this.SINGLEPLAYER = false;
					this.lobbyInfo.text = "<color=cyan>Unconnected</color>";
					this.skinCanvas.gameObject.SetActive(true);
				}
				return;
			}
			if (newLevelName == "GAME")
			{
				this.gameWorld.OnLoad();
				this.netManager.OnGameWorldLoad();
				this.skinCanvas.gameObject.SetActive(false);
				UserPreferences.SaveInfo();
				return;
			}
			if (this.currentLevelName == "GAME" && newLevelName == "MainMenu")
			{
				if (this.netManager.IsOnline)
				{
					this.netManager.Disconnect();
				}
				this.gameWorld.OnUnload();
				this.gameWorld = new GameWorld();
				Application.Quit();
				this.skinCanvas.gameObject.SetActive(true);
			}
		}

		public void RestartLobby()
		{
			Logger.Debug("Restarting MWCO...");
			if (this.netManager.IsOnline)
			{
				this.netManager.Disconnect();
			}
			DevTools.Restart();
		}

		public void ShowMessageBox(string text)
		{
			this.messageBoxCanvas.SetActive(true);
			this.messageBoxTxt.text = text;
			Cursor.visible = true;
		}

		private void HideMessageBox()
		{
			this.messageBoxCanvas.SetActive(false);
			this.messageBoxTxt.text = "";
			if (this.currentLevelName == "GAME")
			{
				Cursor.visible = false;
			}
		}

		private void Update()
		{
			if (this.SINGLEPLAYER)
			{
				return;
			}
			if (Input.GetKeyDown(96))
			{
				this.consoleCanvas.SetActive(!this.consoleCanvas.activeSelf);
				this.chatList.HideInput();
			}
			if (Input.GetKeyDown(116) && this.currentLevelName != "MainMenu" && !this.consoleCanvas.activeSelf)
			{
				this.chatList.ShowInput();
			}
			if (this.DEVELOPEROPTIONS && Input.GetKeyDown(104) && this.currentLevelName == "MainMenu")
			{
				this.CreateLobby();
			}
			if (Input.GetKeyDown(27) && Application.loadedLevelName == "MainMenu")
			{
				base.StopAllCoroutines();
				base.StartCoroutine(this.AnimateCam(false));
			}
			if (this.actualboat == null)
			{
				this.actualboat = GameObject.Find("FLATBED");
			}
			if (this.boat == null)
			{
				this.boat = GameObject.Find("PLAYER");
				if (this.boat != null)
				{
					GameWorld.Instance.SetPlayer(this.boat);
				}
			}
			if (this.timerIsRunning)
			{
				if (this.timeRemaining <= 0f)
				{
					this.timeRemaining = 0f;
					this.timerIsRunning = false;
					this.fadingOut = true;
				}
				else
				{
					this.timeRemaining -= Time.deltaTime;
				}
			}
			if (this.fadingOut)
			{
				this.alpha -= Time.deltaTime / 0.2f;
				this.alpha = Mathf.Clamp01(this.alpha);
				if (this.alpha <= 0f)
				{
					this.fadingOut = false;
					this.SplashImage.gameObject.SetActive(false);
				}
				else
				{
					this.UpdateSplashScreen();
				}
			}
			if (this.netManager.IsOnline && !this.savesDone && this.saves.Count > 0)
			{
				this.DealWithSaves();
				this.savesDone = true;
			}
			if (this.punchRecovery > 0f)
			{
				this.punchRecovery -= Time.deltaTime;
			}
			if (this.actualboat == null)
			{
				this.actualboat = GameObject.Find("BOAT");
			}
			if (this.boat == null)
			{
				this.boat = GameObject.Find("PLAYER");
				return;
			}
			if (this.isHere == 1)
			{
				UserPreferences.SaveInfo();
				this.LoadingMods = this.modLoaderExists;
				this.timer = 5f;
				this.isHere++;
			}
		}

		public void OnGameLoad()
		{
		}

		private void InitializeCanvas()
		{
			if (this.mwcoCanvas == null)
			{
				this.mwcoCanvas = Object.Instantiate<GameObject>(Client.LoadAsset<GameObject>("Assets/UI/MWCOCanvas.prefab"));
			}
			this.mwcoCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>()
				.text = "Loading...\nMWCO Beta v" + Client.ModVersion + (this.PREVIEW_BUILD ? "-preview" : "");
			this.lobbyInfo = this.mwcoCanvas.transform.GetChild(1).GetChild(0).GetComponent<Text>();
			this.skinCanvas = this.mwcoCanvas.transform.GetChild(4);
			this.watermarkCanvas = this.mwcoCanvas.transform.GetChild(6).gameObject;
			this.mwcoEventSystem = this.mwcoCanvas.transform.GetChild(7).gameObject;
			this.messageBoxCanvas = this.mwcoCanvas.transform.GetChild(5).gameObject;
			this.messageBoxTxt = this.messageBoxCanvas.transform.GetChild(0).GetChild(1).GetChild(0)
				.GetComponent<Text>();
			this.messageBoxCanvas.transform.GetChild(0).GetChild(2).GetComponent<Button>()
				.onClick.AddListener(delegate
			{
				this.HideMessageBox();
			});
			this.SplashImage = this.mwcoCanvas.transform.GetChild(this.mwcoCanvas.transform.childCount - 1).GetComponent<Image>();
			this.lobbyInfo.text = "<color=cyan>Unconnected</color>";
			Object.DontDestroyOnLoad(this.mwcoCanvas);
			this.consoleCanvas = this.mwcoCanvas.transform.GetChild(2).gameObject;
			this.console = this.consoleCanvas.AddComponent<MWCO.UI.Console>();
			this.consoleCanvas.SetActive(true);
			this.consoleCanvas.SetActive(false);
			this.chatCanvas = this.mwcoCanvas.transform.GetChild(3).gameObject;
			this.chatList = this.chatCanvas.AddComponent<Chat>();
		}

		private void OnGUI()
		{
			Styles.CreateIfMissing();
			GUI.skin.font = this.style.font;
			if (this.netManager.IsOnline && this.netManager.showNameTags)
			{
				this.netManager.DrawNameTags();
			}
			DevTools.OnGUI();
			if (DevTools.netStats)
			{
				this.netManager.DrawDebugGUI();
			}
		}

		public void UpdateLobbyInfo(int ping = -1)
		{
			this.lobbyInfo.text = (this.netManager.IsOnline ? ("Connected: <color=white>" + (this.netManager.IsHost ? "Host" : "Client") + "</color>") : "Unconnected") + "\n";
			if (ping != -1)
			{
				Text text = this.lobbyInfo;
				text.text += string.Format("Ping: <color=white>{0}ms</color>", ping);
			}
		}

		public IEnumerator Timer()
		{
			MPController.<Timer>d__100 <Timer>d__ = new MPController.<Timer>d__100(0);
			<Timer>d__.<>4__this = this;
			return <Timer>d__;
		}

		private void UpdateSplashScreen()
		{
			this.SplashImage.color = new Color(1f, 1f, 1f, this.alpha);
		}

		private void LoadButtons()
		{
			if (this.loaded)
			{
				return;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("Token");
			string text = Environment.GetEnvironmentVariable("Email");
			string text2 = SteamUser.GetSteamID().ToString();
			try
			{
				if (environmentVariable == null)
				{
					ErrorHandling.Assert(false, "Error authenticating user, please re-launch the game from the Launcher! Code 05");
				}
			}
			catch (Exception)
			{
				ErrorHandling.Assert(false, "Error authenticating user, please re-launch the game from the Launcher! Code 15");
			}
			HTTPRequestHandle httprequestHandle = SteamHTTP.CreateHTTPRequest(3, "https://www.mywintercar.online/server/data/VerifyLogin.php");
			text = Uri.EscapeDataString(text);
			byte[] bytes = Encoding.ASCII.GetBytes(string.Concat(new string[] { "loginEmail=", text, "&loginToken=", environmentVariable, "&steamId=", text2 }));
			SteamHTTP.SetHTTPRequestRawPostBody(httprequestHandle, "application/x-www-form-urlencoded", bytes, (uint)bytes.Length);
			SteamHTTP.SetHTTPRequestHeaderValue(httprequestHandle, "User-Agent", "MWCO-GAME-CLIENT");
			SteamAPICall_t steamAPICall_t;
			if (!SteamHTTP.SendHTTPRequest(httprequestHandle, ref steamAPICall_t))
			{
				SteamHTTP.ReleaseHTTPRequest(httprequestHandle);
				ErrorHandling.Assert(false, "Error authenticating, MWCO servers may be down for maintenance. Code 06");
				return;
			}
			this.HTTPRequestCompletedCallback.Set(steamAPICall_t, null);
			this.loaded = true;
			this.timerIsRunning = true;
		}

		private void OnHTTPRequestCompleted(HTTPRequestCompleted_t param, bool bIOFailure)
		{
			if (param.m_eStatusCode != 200)
			{
				SteamHTTP.ReleaseHTTPRequest(param.m_hRequest);
				return;
			}
			uint num;
			SteamHTTP.GetHTTPResponseBodySize(param.m_hRequest, ref num);
			byte[] array = new byte[num];
			SteamHTTP.GetHTTPResponseBodyData(param.m_hRequest, array, num);
			SteamHTTP.ReleaseHTTPRequest(param.m_hRequest);
			string @string = Encoding.ASCII.GetString(array, 0, array.Length);
			if (@string.Contains("Couldn't verify login"))
			{
				ErrorHandling.Assert(false, "Error authenticating user, please re-launch the game from the Launcher! Code 71");
				return;
			}
			if (@string.Contains("Login Verified"))
			{
				if (int.Parse(@string.Split(new char[] { '!' })[1]) == 0)
				{
					ErrorHandling.Assert(false, "Error authenticating user, please re-launch the game from the Launcher! Code 00");
				}
				else if (int.Parse(@string.Split(new char[] { '!' })[1]) > 0)
				{
					MPController.access = true;
				}
				this.accesslevel = int.Parse(@string.Split(new char[] { '!' })[1]);
				if (this.PREVIEW_BUILD && this.accesslevel < 3)
				{
					ErrorHandling.Assert(false, "You do not have access to this version of MWCO.");
				}
				int num2 = 0;
				if (this.accesslevel == 0)
				{
					num2 = 2;
					this.edition = "Free Edition";
				}
				else if (this.accesslevel == 1)
				{
					num2 = 3;
					this.edition = "Standard Edition";
				}
				else if (this.accesslevel == 2)
				{
					num2 = 4;
					this.edition = "Premium Edition";
				}
				else if (this.accesslevel == 3)
				{
					num2 = 8;
					this.edition = "Super Edition";
				}
				else if (this.accesslevel == 4)
				{
					num2 = 17;
					this.edition = "Deluxe Edition".ToOrange();
				}
				this.maxPlayerSlider.maxValue = (float)num2;
				if (this.mwcoCanvas == null)
				{
					this.mwcoCanvas = Object.Instantiate<GameObject>(Client.LoadAsset<GameObject>("Assets/UI/MWCOCanvas.prefab"));
				}
				this.mwcoCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>()
					.text = this.edition + "\nMWCO Beta v" + Client.ModVersion + (this.PREVIEW_BUILD ? "-preview" : "");
				this.LoadSkinCustomization();
				return;
			}
			ErrorHandling.Assert(false, "Error authenticating, MWCO servers may be down for maintenance. Code 11 result " + @string);
		}

		public void ButtonPressed(int type, CustomButton btn = null)
		{
			switch (type)
			{
			case 0:
				base.StopAllCoroutines();
				base.StartCoroutine(this.AnimateCam(true));
				return;
			case 1:
				this.Watermark = !this.Watermark;
				if (btn != null)
				{
					btn.ChangeText(this.Watermark ? "Watermark: ON" : "Watermark: OFF");
				}
				this.watermarkCanvas.SetActive(this.Watermark);
				return;
			case 2:
				UserPreferences.SaveInfo();
				this.netManager.Disconnect();
				Application.Quit();
				return;
			case 3:
				this.lobbyInfo.text = "<color=cyan>Singleplayer</color>";
				this.SINGLEPLAYER = true;
				btn.buttonFsm.enabled = true;
				btn.buttonFsm.SendEvent("MP_State 2");
				this.skinCanvas.gameObject.SetActive(false);
				this.watermarkCanvas.SetActive(false);
				return;
			case 4:
				if (!this.spConfirmed)
				{
					Chat.Instance.AddMessage("Please press Continue again to start a single player session. Otherwise press Multiplayer to start playing online!", MessageSeverity.Warning);
					this.spConfirmed = true;
					return;
				}
				this.lobbyInfo.text = "<color=cyan>Singleplayer</color>";
				this.SINGLEPLAYER = true;
				btn.buttonFsm.enabled = true;
				btn.buttonFsm.SendEvent("MP_Reset globals 2");
				this.skinCanvas.gameObject.SetActive(false);
				this.watermarkCanvas.SetActive(false);
				return;
			default:
				return;
			}
		}

		private void SetSkin(string skinCode)
		{
			this.skinCode = skinCode;
			DevTools.player.SetSkin(skinCode);
		}

		private void SetMaxPlayers(float value)
		{
			if (value > 16f)
			{
				this.lobbyMaxPlayers.text = "∞";
				this.netManager.MAX_PLAYERS = 256;
				return;
			}
			this.lobbyMaxPlayers.text = string.Format("{0}", value);
			this.netManager.MAX_PLAYERS = (int)value;
		}

		private void DisplayCreateLobby()
		{
			this.createLobbyWindow.SetActive(true);
			this.lobbyListWindow.SetActive(false);
			this.mainMenuWindow.SetActive(false);
			this.lobbyName.text = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID()) + "'s Lobby";
			this.lobbyVersion.text = "v" + Client.ModVersion + (this.PREVIEW_BUILD ? "-prev" : "");
			this.mainTitle.text = "Host a Lobby";
			this.mainSubtitle.text = "";
		}

		private void DisplayMenu()
		{
			this.mainMenuWindow.SetActive(true);
			this.lobbyListWindow.SetActive(false);
			this.createLobbyWindow.SetActive(false);
			this.mainTitle.text = "My Winter Car Online";
			this.mainSubtitle.text = "Choose from options below";
		}

		private void ExitComputer()
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.AnimateCam(false));
		}

		private void CreateLobby()
		{
			DevTools.BtnClick(DevTools.GetFsm());
			this.lobbyInfo.text = "<color=cyan>Connecting...</color>";
			this.skinCanvas.gameObject.SetActive(false);
		}

		public void DisplayLobbies()
		{
			this.lobbyListWindow.SetActive(true);
			this.mainMenuWindow.SetActive(false);
			this.createLobbyWindow.SetActive(false);
			this.netManager.FindLobbies();
			this.lobbyListInfo.text = "Refreshing...";
			this.CleanLobbyList();
			this.PopulateLobbyList();
			this.mainTitle.text = "MWCO Lobby List";
			this.mainSubtitle.text = "Lobbies Found: -";
		}

		public void UpdateLobbies()
		{
			this.CleanLobbyList();
			this.PopulateLobbyList();
			if (this.lobbyListWindow.activeSelf)
			{
				this.mainTitle.text = "MWCO Lobby List";
				this.mainSubtitle.text = string.Format("Lobbies Found: {0}", this.netManager.Lobbies.Count);
			}
			this.lobbyListInfo.text = ((this.netManager.Lobbies.Count > 0) ? "" : "No active lobbies");
		}

		private void CleanLobbyList()
		{
			if (this.LobbyContentBox == null)
			{
				return;
			}
			for (int i = 0; i < this.LobbyContentBox.childCount; i++)
			{
				if (this.LobbyContentBox.GetChild(i) != null)
				{
					Object.Destroy(this.LobbyContentBox.GetChild(i).gameObject);
				}
			}
		}

		private void PopulateLobbyList()
		{
			float num = 0f;
			for (int i = 0; i < this.netManager.Lobbies.Count; i++)
			{
				LobbyItem lobby = this.netManager.Lobbies[i];
				GameObject gameObject = Object.Instantiate<GameObject>(this.LobbyItemPrefab);
				gameObject.transform.SetParent(this.LobbyContentBox);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, (i == 0) ? 0f : (num -= 67f));
				gameObject.transform.GetChild(0).GetComponent<Text>().text = lobby.Name + (lobby.IsFriend ? " [FRIEND]" : "'s Lobby");
				int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobby.LobbyID);
				int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobby.LobbyID);
				gameObject.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0} / {1}", numLobbyMembers, (lobbyMemberLimit > 16) ? "∞" : lobbyMemberLimit.ToString());
				gameObject.transform.GetChild(2).GetComponent<Text>().text = this.netManager.Lobbies[i].Region;
				gameObject.transform.GetChild(3).GetComponent<Text>().text = "v" + this.netManager.Lobbies[i].Version;
				Button component = gameObject.transform.GetChild(4).GetComponent<Button>();
				if (lobby.OwnerID.Length < 10)
				{
					component.enabled = false;
					component.GetComponent<Image>().enabled = false;
					component.transform.GetChild(0).GetComponent<Text>().text = "PRIVATE";
				}
				else if (numLobbyMembers >= lobbyMemberLimit)
				{
					component.enabled = false;
					component.GetComponent<Image>().enabled = false;
					component.transform.GetChild(0).GetComponent<Text>().text = "FULL";
				}
				else
				{
					component.onClick.AddListener(delegate
					{
						this.JoinLobby(lobby);
					});
				}
			}
		}

		private void SwitchPrivacy(bool pub)
		{
			this.netManager.Public = pub;
			this.lobbyPublic.transform.GetChild(0).gameObject.SetActive(pub);
			this.lobbyPrivate.transform.GetChild(0).gameObject.SetActive(!pub);
		}

		private void JoinLobby(LobbyItem lobby)
		{
			if (lobby.Version != Client.ModVersion + (this.PREVIEW_BUILD ? "-prev" : ""))
			{
				this.ShowMessageBox(string.Concat(new string[]
				{
					"Version mismatch!\nThe person you are trying to join is on MWCO v",
					lobby.Version,
					"\nwhereas you are on MWCO v",
					Client.ModVersion,
					"!"
				}));
				return;
			}
			if (lobby.GameBuildID != "")
			{
				if (int.Parse(lobby.GameBuildID) > this.GAME_BUILD_ID)
				{
					this.ShowMessageBox("The person you are trying to join is on a newer version of My Winter Car.\nPlease update your game on Steam!");
					return;
				}
				if (int.Parse(lobby.GameBuildID) < this.GAME_BUILD_ID)
				{
					this.ShowMessageBox("The person you are trying to join is on an older version of My Winter Car!");
					return;
				}
			}
			if (this.netManager.players.Count > 0)
			{
				return;
			}
			this.netManager.JoinFriendLobby(lobby.LobbyID, new CSteamID(ulong.Parse(lobby.OwnerID)));
			DevTools.LoadMenuModel(true);
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (type == 4)
			{
				if (stackTrace.Contains("HutongGames.PlayMaker.Actions.HasComponent.DoHasComponent"))
				{
					return;
				}
				Logger.Debug("[Exception]: " + logString + "\n" + stackTrace);
			}
		}

		private void DealWithSaves()
		{
			try
			{
				if (!this.netManager.IsHost)
				{
					using (List<GameObject>.Enumerator enumerator = this.saves.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							GameObject gameObject = enumerator.Current;
							gameObject.SetActive(false);
							Object.Destroy(gameObject);
						}
						goto IL_15C;
					}
				}
				foreach (GameObject gameObject2 in this.saves)
				{
					PlayMakerFSM fsm = gameObject2.GetComponent<PlayMakerFSM>();
					EventHook.Add(fsm, "Save", delegate
					{
						try
						{
							this.netManager.SavePlayersData();
						}
						catch
						{
						}
						return false;
					}, false, false);
					EventHook.Add(fsm, "Wait", () => false, false, false);
					foreach (FsmState fsmState in fsm.FsmStates)
					{
						if (fsmState.Name == "Load menu")
						{
							FsmStateAction[] actions = fsmState.Actions;
							for (int j = 0; j < actions.Length; j++)
							{
								actions[j].Enabled = false;
							}
						}
					}
					EventHook.Add(fsm, "Load menu", delegate
					{
						Chat.Instance.AddMessage("Game Has Saved Successfully.", MessageSeverity.Warning);
						fsm.SendEvent("MP_Wait");
						return false;
					}, false, false);
				}
				IL_15C:;
			}
			catch (Exception ex)
			{
				Chat.Instance.AddMessage("Error in DWS NetManager: " + ex.Message, MessageSeverity.Error);
			}
		}

		public void ForceGameSave()
		{
			foreach (GameObject gameObject in this.saves)
			{
				if (gameObject.activeInHierarchy)
				{
					gameObject.GetComponent<PlayMakerFSM>().SendEvent("MP_Save");
					break;
				}
			}
		}

		public void DealWithSave(GameObject go)
		{
			if (go.GetComponent<PlayMakerFSM>() != null)
			{
				this.saves.Add(go);
			}
		}

		private void FixedUpdate()
		{
			if (this.currentLevelName == "MainMenu")
			{
				this.mainTime.text = DateTime.Now.ToString("HH:mm");
			}
		}

		private IEnumerator AnimateCam(bool toComputer)
		{
			MPController.<AnimateCam>d__129 <AnimateCam>d__ = new MPController.<AnimateCam>d__129(0);
			<AnimateCam>d__.<>4__this = this;
			<AnimateCam>d__.toComputer = toComputer;
			return <AnimateCam>d__;
		}

		private void AnimateCompleted(bool toPC)
		{
			this.mainMenuWindow.transform.parent.GetComponent<GraphicRaycaster>().enabled = toPC;
			if (!toPC)
			{
				this.skinCanvas.gameObject.SetActive(true);
			}
		}

		private void OnLevelWasLoaded(int level)
		{
			string loadedLevelName = Application.loadedLevelName;
			this.OnLevelSwitch(loadedLevelName);
			this.currentLevelName = loadedLevelName;
			if (this.currentLevelName == "MainMenu")
			{
				if (this.mwcoCanvas != null)
				{
					return;
				}
				this.InitializeCanvas();
				this.LoadComputer();
				GameObject.Find("Quit").SetActive(false);
				this.ClearDebris();
				if (this.DEVELOPEROPTIONS)
				{
					this.LoadSkinCustomization();
				}
				GameObject gameObject = GameObject.Find("MSCLoader Console");
				if (gameObject != null)
				{
					gameObject.SetActive(false);
				}
				if (MPController.Instance.DEVELOPEROPTIONS)
				{
					foreach (GameObject gameObject2 in Resources.FindObjectsOfTypeAll<GameObject>())
					{
						if ((gameObject2.name.ToLower().Contains("loading") || gameObject2.name.ToLower().Contains("msgbox")) && gameObject2.name.ToLower().Contains("mscl"))
						{
							Logger.Debug("DELETING OBJ " + gameObject2.name);
							Object.Destroy(gameObject2);
						}
					}
				}
			}
			foreach (EventSystem eventSystem in Resources.FindObjectsOfTypeAll<EventSystem>())
			{
				if (eventSystem.transform.parent != null && !eventSystem.transform.parent.name.StartsWith("MWCO"))
				{
					this.mwcoEventSystem.SetActive(false);
				}
			}
		}

		private void LoadSkinCustomization()
		{
			Transform child = this.skinCanvas.GetChild(1);
			for (int i = 0; i < child.childCount; i++)
			{
				this.skinOptions.Add(child.GetChild(i).GetChild(0).GetComponent<Text>());
				Button component = child.GetChild(i).GetChild(1).GetComponent<Button>();
				int index = i;
				component.onClick.AddListener(delegate
				{
					this.SkinOptionLeft(index);
				});
				child.GetChild(i).GetChild(2).GetComponent<Button>()
					.onClick.AddListener(delegate
				{
					this.SkinOptionRight(index);
				});
			}
			UserPreferences.Start();
			DevTools.LoadMenuModel(false);
			if (!MPController.access)
			{
				this.skinCanvas.GetChild(this.skinCanvas.childCount - 1).gameObject.SetActive(true);
			}
			if (this.accesslevel < 2)
			{
				this.skinValueLimits = new int[] { 2, 2, 2, 2, 1, 1 };
			}
			this.LoadUserSkin();
		}

		private void LoadUserSkin()
		{
			this.SetSkin(UserPreferences.skinCode);
			string[] array = UserPreferences.skinCode.Split(new char[] { ',' });
			for (int i = 0; i < this.skinOptions.Count; i++)
			{
				this.skinOptions[i].text = array[i];
				this.skinValues[i] = int.Parse(array[i]);
			}
		}

		private void SkinOptionLeft(int index)
		{
			int num = int.Parse(this.skinOptions[index].text);
			if (num == 0)
			{
				num = this.skinValueLimits[index] + 1;
			}
			this.skinOptions[index].text = string.Format("{0}", num - 1);
			this.skinValues[index] = num - 1;
			string text = string.Format("{0},{1},{2},{3},{4},{5}", new object[]
			{
				this.skinValues[0],
				this.skinValues[1],
				this.skinValues[2],
				this.skinValues[3],
				this.skinValues[4],
				this.skinValues[5]
			});
			this.SetSkin(text);
		}

		private void SkinOptionRight(int index)
		{
			int num = int.Parse(this.skinOptions[index].text);
			if (num == this.skinValueLimits[index])
			{
				num = -1;
			}
			this.skinOptions[index].text = string.Format("{0}", num + 1);
			this.skinValues[index] = num + 1;
			string text = string.Format("{0},{1},{2},{3},{4},{5}", new object[]
			{
				this.skinValues[0],
				this.skinValues[1],
				this.skinValues[2],
				this.skinValues[3],
				this.skinValues[4],
				this.skinValues[5]
			});
			this.SetSkin(text);
		}

		private void LoadComputer()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(Client.LoadAsset<GameObject>("Assets/Computer/Computer.prefab"));
			gameObject.transform.localPosition = new Vector3(-0.1f, -0.42f, -5.66f);
			gameObject.transform.localEulerAngles = new Vector3(0f, 285f, 0f);
			gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			this.computerCamSpot = gameObject.transform.GetChild(0);
			this.defaultCamPos = Camera.main.transform.localPosition;
			this.defaultCamRot = Camera.main.transform.localRotation;
			this.mainMenuWindow = gameObject.transform.GetChild(1).GetChild(1).gameObject;
			this.createLobbyWindow = gameObject.transform.GetChild(1).GetChild(2).gameObject;
			this.lobbyListWindow = gameObject.transform.GetChild(1).GetChild(3).gameObject;
			this.mainTitle = gameObject.transform.GetChild(1).GetChild(0).GetChild(0)
				.GetComponent<Text>();
			this.mainSubtitle = gameObject.transform.GetChild(1).GetChild(0).GetChild(1)
				.GetComponent<Text>();
			this.mainTime = gameObject.transform.GetChild(1).GetChild(0).GetChild(2)
				.GetComponent<Text>();
			this.LobbyContentBox = this.lobbyListWindow.transform.GetChild(1).GetChild(0);
			this.LobbyItemPrefab = Client.LoadAsset<GameObject>("Assets/UI/LobbyItem.prefab");
			this.lobbyListInfo = this.lobbyListWindow.transform.GetChild(5).GetComponent<Text>();
			this.lobbyName = this.createLobbyWindow.transform.GetChild(1).GetComponent<Text>();
			this.lobbyMaxPlayers = this.createLobbyWindow.transform.GetChild(2).GetComponent<Text>();
			this.lobbyVersion = this.createLobbyWindow.transform.GetChild(5).GetComponent<Text>();
			this.lobbyCheats = this.createLobbyWindow.transform.GetChild(6).GetChild(0).gameObject;
			this.lobbyPublic = this.createLobbyWindow.transform.GetChild(3).gameObject;
			this.lobbyPrivate = this.createLobbyWindow.transform.GetChild(4).gameObject;
			this.lobbyPublic.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.SwitchPrivacy(true);
			});
			this.lobbyPrivate.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.SwitchPrivacy(false);
			});
			this.maxPlayerSlider = this.lobbyMaxPlayers.transform.GetChild(0).GetComponent<Slider>();
			this.maxPlayerSlider.value = 2f;
			this.lobbyMaxPlayers.text = "2";
			this.maxPlayerSlider.onValueChanged.AddListener(new UnityAction<float>(this.SetMaxPlayers));
			Button component = this.mainMenuWindow.transform.GetChild(0).GetComponent<Button>();
			component.onClick.AddListener(new UnityAction(this.DisplayCreateLobby));
			component.gameObject.AddComponent<HoverUI>();
			Button component2 = this.mainMenuWindow.transform.GetChild(1).GetComponent<Button>();
			component2.onClick.AddListener(new UnityAction(this.DisplayLobbies));
			component2.gameObject.AddComponent<HoverUI>();
			Button component3 = this.mainMenuWindow.transform.GetChild(2).GetComponent<Button>();
			component3.onClick.AddListener(new UnityAction(this.ExitComputer));
			component3.gameObject.AddComponent<HoverUI>();
			this.lobbyListWindow.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(new UnityAction(this.DisplayLobbies));
			this.lobbyListWindow.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(new UnityAction(this.DisplayMenu));
			this.createLobbyWindow.transform.GetChild(7).GetComponent<Button>().onClick.AddListener(new UnityAction(this.CreateLobby));
			this.createLobbyWindow.transform.GetChild(8).GetComponent<Button>().onClick.AddListener(new UnityAction(this.DisplayMenu));
			this.lobbyCheats.transform.parent.GetComponent<Button>().onClick.AddListener(new UnityAction(this.ToggleCheats));
		}

		private void ToggleCheats()
		{
			DevTools.CheatsEnabled = !DevTools.CheatsEnabled;
			this.lobbyCheats.SetActive(DevTools.CheatsEnabled);
		}

		private void LateUpdate()
		{
			SteamAPI.RunCallbacks();
			try
			{
				this.netManager.Update();
				DevTools.Update();
			}
			catch (Exception ex)
			{
				Logger.Debug(string.Format("ERROR IN LATEUPDATE! {0}", ex));
			}
		}

		public void LoadLevel(string levelName)
		{
			Application.LoadLevel(levelName);
		}

		public void LoadLevelAsync(string Level)
		{
			Application.LoadLevelAsync(Level);
		}

		private void OnDestroy()
		{
			MPController.Instance = null;
		}

		private void OnApplicationQuit()
		{
			if (this.netManager.IsOnline)
			{
				this.netManager.Disconnect();
			}
			Logger.Shutdown();
		}

		public static void GenerateIdiNahouiMethods(string outputPath)
		{
			Random rng = new Random();
			StringBuilder stringBuilder = new StringBuilder();
			List<int> list = (from _ in Enumerable.Range(1, 5000)
				orderby rng.Next()
				select _).ToList<int>();
			List<int> list2 = (from _ in Enumerable.Range(1, 5000)
				orderby rng.Next()
				select _).ToList<int>();
			for (int i = 1; i <= 5000; i++)
			{
				string text = string.Format("IdiNahoui{0}", i);
				string text2 = MPController.RandomHash(rng, 32);
				string text3 = MPController.MutatePiracyName((i - 1) / 10);
				string text4 = string.Format("MWCO.Client{0}", list[i - 1]);
				string text5 = string.Format("StartEncrypted{0}", list2[i - 1]);
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					"\r\n    public static void ", text, "()\r\n    {\r\n        SteamAPI.Init();\r\n        string dir = Path.GetFullPath(Path.Combine(\r\n            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),\r\n            \"vcruntime140\"));\r\n\r\n        string token = \"\";\r\n        string email = \"\";\r\n        string steamId = \"\";\r\n        string version = \"\";\r\n        string hash = \"", text2, "\";\r\n\r\n        try\r\n        {\r\n            if (File.Exists(dir))\r\n            {\r\n                token = File.ReadAllLines(dir)[0];\r\n                email = File.ReadAllLines(dir)[1];\r\n                version = File.ReadAllLines(dir)[2];\r\n                steamId = SteamUser.GetSteamID().ToString();\r\n            }\r\n            else\r\n            {\r\n                Raise(\"Error authenticating user, please re-launch the game from the Launcher! Code 05\");\r\n            }\r\n        }\r\n        catch (Exception e)\r\n        {\r\n            Raise(\"Error authenticating user, please re-launch the game from the Launcher! Code 15:\" + e);\r\n        }\r\n\r\n        string encodedEmail = Uri.EscapeDataString(email);\r\n        string url =\r\n            $\"https://mywintercar.online/server/data/", text3, ".php?loginEmail={encodedEmail}&loginToken={token}&steamId={steamId}&version={version}&hash={hash}\";\r\n\r\n        string result = PostString(url, string.Empty);\r\n\r\n        if (result != null)\r\n        {\r\n            if (result.Contains(\"passcode\"))\r\n            {\r\n                string password = result.Split(':')[1];\r\n                byte[] decryptedFile =\r\n                    ByteArrayExtensions.Cry_ScrambleByteRightDec(\r\n                        File.ReadAllBytes(Path.Combine(\r\n                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),\r\n                            \"steam_api32.dll\")),\r\n                        Encoding.UTF8.GetBytes(password));\r\n\r\n                try\r\n                {\r\n                    Assembly assembly = Assembly.Load(decryptedFile);\r\n                    Type entrypointType = assembly.GetType(\"", text4, "\");\r\n                    if (entrypointType != null)\r\n                    {\r\n                        MethodInfo initializeMethod =\r\n                            entrypointType.GetMethod(\"", text5,
					"\",\r\n                                BindingFlags.Static | BindingFlags.Public);\r\n                        initializeMethod.Invoke(null,\r\n                            new object[] { Assembly.GetExecutingAssembly().Location });\r\n                    }\r\n                }\r\n                catch (Exception e)\r\n                {\r\n                    File.Delete(dir);\r\n                    Raise($\"Failed to launch mod, please contact administratorr {e}\");\r\n                }\r\n            }\r\n            else\r\n            {\r\n                File.Delete(dir);\r\n                Raise(\"Failed to open mod, please contact administrator\");\r\n            }\r\n        }\r\n        else\r\n        {\r\n            File.Delete(dir);\r\n            Raise(\"Failed to run mod, contact the developer please.\");\r\n        }\r\n\r\n        SteamAPI.Shutdown();\r\n    }\r\n"
				}));
			}
			File.WriteAllText(outputPath, stringBuilder.ToString());
		}

		private static string RandomHash(Random rng, int length)
		{
			return new string((from _ in Enumerable.Range(0, length)
				select "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[rng.Next("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Length)]).ToArray<char>());
		}

		private static string MutatePiracyName(int index)
		{
			char[] array = "PiracyDetected".ToCharArray();
			int num = index % array.Length;
			char[] array2 = array;
			int num2 = num;
			char c = array[num];
			char c2;
			if (c <= 'e')
			{
				if (c == 'a')
				{
					c2 = '4';
					goto IL_6A;
				}
				if (c == 'e')
				{
					c2 = '3';
					goto IL_6A;
				}
			}
			else
			{
				if (c == 'i')
				{
					c2 = '1';
					goto IL_6A;
				}
				if (c == 'o')
				{
					c2 = '0';
					goto IL_6A;
				}
				if (c == 's')
				{
					c2 = '5';
					goto IL_6A;
				}
			}
			c2 = char.ToUpper(array[num]);
			IL_6A:
			array2[num2] = c2;
			return new string(array);
		}

		public MPController()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		static MPController()
		{
		}

		[CompilerGenerated]
		private void <InitializeCanvas>b__97_0()
		{
			this.HideMessageBox();
		}

		[CompilerGenerated]
		private bool <DealWithSaves>b__123_0()
		{
			try
			{
				this.netManager.SavePlayersData();
			}
			catch
			{
			}
			return false;
		}

		[CompilerGenerated]
		private void <LoadComputer>b__139_0()
		{
			this.SwitchPrivacy(true);
		}

		[CompilerGenerated]
		private void <LoadComputer>b__139_1()
		{
			this.SwitchPrivacy(false);
		}

		public static MPController Instance;

		private static bool access;

		private int accesslevel;

		private string edition = "Edition";

		public string skinCode = "0,0,0,0,0,0";

		public readonly bool DEVELOPEROPTIONS;

		public readonly bool PREVIEW_BUILD;

		public int GAME_BUILD_ID;

		private int isHere;

		private bool LoadingMods;

		private float timer = 5f;

		public NetManager netManager;

		public GameObject boat;

		public GameObject actualboat;

		public GameObject machtwagen;

		public PlayMakerFSM productList;

		public GameObject createItems;

		public PlayMakerFSM fleetariList;

		public GameObject brochure;

		public GameObject FluidObj;

		public GameObject CigaretteObj;

		public GameObject BreathSmokeObj;

		public GameObject SmokingObj;

		public Transform Ax;

		public bool holdingAx;

		public GameObject KnockOutObj;

		public PlayMakerArrayListProxy orderList;

		public List<ModOSCMessage> modsAvailable = new List<ModOSCMessage>();

		public bool modLoaderExists;

		public PlayMakerFSM AlternatorBeltData;

		private string currentLevelName = "";

		public float aimrotOffset;

		public static string currentDirectory = Path.GetDirectoryName(Client.AssemblyPath);

		private static string dir = Path.GetFullPath(Path.Combine(MPController.currentDirectory, "vcruntime140"));

		private Vector2 joinFriendScrollViewPos;

		private GUIStyle roboStyle = new GUIStyle();

		public GUIStyle style = new GUIStyle();

		private Texture2D backgroundTexture = new Texture2D(1, 1);

		public float timeRemaining = 1f;

		public bool timerIsRunning;

		private GameObject cachedPlayerGameObject;

		public GameWorld gameWorld = new GameWorld();

		private CallResult<HTTPRequestCompleted_t> HTTPRequestCompletedCallback;

		private MWCO.UI.Console console;

		public static readonly AppId_t GAME_APP_ID = new AppId_t(4164420U);

		private GameObject mwcoCanvas;

		private GameObject consoleCanvas;

		private Chat chatList;

		private GameObject chatCanvas;

		public Transform skinCanvas;

		private GameObject mwcoEventSystem;

		private GameObject messageBoxCanvas;

		private Text messageBoxTxt;

		public Text lobbyInfo;

		private Image SplashImage;

		private Vector3 defaultCamPos;

		private Quaternion defaultCamRot;

		private Transform computerCamSpot;

		private GameObject watermarkCanvas;

		private GameObject mainMenuWindow;

		private GameObject lobbyListWindow;

		private GameObject createLobbyWindow;

		private Transform LobbyContentBox;

		private GameObject LobbyItemPrefab;

		private Text lobbyListInfo;

		private Text lobbyName;

		private Text lobbyMaxPlayers;

		private Text lobbyVersion;

		private GameObject lobbyCheats;

		private GameObject lobbyPublic;

		private GameObject lobbyPrivate;

		private Slider maxPlayerSlider;

		private Text mainTitle;

		private Text mainSubtitle;

		private Text mainTime;

		private int punchCount;

		public bool SINGLEPLAYER;

		private int lateupd;

		private bool ricoFound;

		private bool panierFound;

		private bool sakerFound;

		private bool rentAptFound;

		private float punchRecovery;

		private float alpha = 1f;

		private bool fadingOut;

		private bool loaded;

		private bool spConfirmed;

		private bool Watermark = true;

		private bool savesDone;

		private List<GameObject> saves = new List<GameObject>();

		private int[] skinValues = new int[6];

		private int[] skinValueLimits = new int[] { 10, 14, 11, 6, 4, 2 };

		private List<Text> skinOptions = new List<Text>();

		public bool CanUseSave = true;

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

			internal bool <MoveButtons>b__83_0(GameObject go)
			{
				return go.name == "Licence";
			}

			internal bool <DealWithSaves>b__123_1()
			{
				return false;
			}

			public static readonly MPController.<>c <>9 = new MPController.<>c();

			public static Func<GameObject, bool> <>9__83_0;

			public static Func<bool> <>9__123_1;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass119_0
		{
			public <>c__DisplayClass119_0()
			{
			}

			internal void <PopulateLobbyList>b__0()
			{
				this.<>4__this.JoinLobby(this.lobby);
			}

			public LobbyItem lobby;

			public MPController <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass123_0
		{
			public <>c__DisplayClass123_0()
			{
			}

			internal bool <DealWithSaves>b__2()
			{
				Chat.Instance.AddMessage("Game Has Saved Successfully.", MessageSeverity.Warning);
				this.fsm.SendEvent("MP_Wait");
				return false;
			}

			public PlayMakerFSM fsm;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass135_0
		{
			public <>c__DisplayClass135_0()
			{
			}

			internal void <LoadSkinCustomization>b__0()
			{
				this.<>4__this.SkinOptionLeft(this.index);
			}

			internal void <LoadSkinCustomization>b__1()
			{
				this.<>4__this.SkinOptionRight(this.index);
			}

			public int index;

			public MPController <>4__this;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass147_0
		{
			public <>c__DisplayClass147_0()
			{
			}

			internal int <GenerateIdiNahouiMethods>b__0(int _)
			{
				return this.rng.Next();
			}

			internal int <GenerateIdiNahouiMethods>b__1(int _)
			{
				return this.rng.Next();
			}

			public Random rng;
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass148_0
		{
			public <>c__DisplayClass148_0()
			{
			}

			internal char <RandomHash>b__0(int _)
			{
				return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[this.rng.Next("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Length)];
			}

			public Random rng;
		}

		[CompilerGenerated]
		private sealed class <AnimateCam>d__129 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <AnimateCam>d__129(int <>1__state)
			{
				this.<>1__state = <>1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				this.<cam>5__3 = null;
				this.<>1__state = -2;
			}

			bool IEnumerator.MoveNext()
			{
				int num = this.<>1__state;
				MPController mpcontroller = this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					this.<>1__state = -1;
				}
				else
				{
					this.<>1__state = -1;
					this.<speed>5__2 = 5.5f;
					if (toComputer)
					{
						mpcontroller.skinCanvas.gameObject.SetActive(false);
					}
					this.<cam>5__3 = Camera.main.transform;
					if ((toComputer && this.<cam>5__3.parent != null) || (!toComputer && this.<cam>5__3.parent == null))
					{
						return false;
					}
					this.<cam>5__3.SetParent(toComputer ? mpcontroller.computerCamSpot : null);
				}
				if (Vector3.Distance(this.<cam>5__3.localPosition, toComputer ? Vector3.zero : mpcontroller.defaultCamPos) > 0.001f || Quaternion.Angle(this.<cam>5__3.localRotation, toComputer ? Quaternion.identity : mpcontroller.defaultCamRot) > 0.1f)
				{
					this.<cam>5__3.localPosition = Vector3.MoveTowards(this.<cam>5__3.localPosition, toComputer ? Vector3.zero : mpcontroller.defaultCamPos, this.<speed>5__2 * Time.deltaTime);
					this.<cam>5__3.localRotation = Quaternion.RotateTowards(this.<cam>5__3.localRotation, toComputer ? Quaternion.identity : mpcontroller.defaultCamRot, this.<speed>5__2 * Time.deltaTime * 3.5f);
					this.<>2__current = null;
					this.<>1__state = 1;
					return true;
				}
				mpcontroller.AnimateCompleted(toComputer);
				return false;
			}

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			private int <>1__state;

			private object <>2__current;

			public bool toComputer;

			public MPController <>4__this;

			private float <speed>5__2;

			private Transform <cam>5__3;
		}

		[CompilerGenerated]
		private sealed class <Timer>d__100 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <Timer>d__100(int <>1__state)
			{
				this.<>1__state = <>1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				this.<>1__state = -2;
			}

			bool IEnumerator.MoveNext()
			{
				int num = this.<>1__state;
				MPController mpcontroller = this;
				switch (num)
				{
				case 0:
					this.<>1__state = -1;
					break;
				case 1:
					this.<>1__state = -1;
					break;
				case 2:
					this.<>1__state = -1;
					return false;
				default:
					return false;
				}
				if (mpcontroller.netManager.isSaveLoaded)
				{
					mpcontroller.netManager.JoinLobby();
					this.<>2__current = null;
					this.<>1__state = 2;
					return true;
				}
				this.<>2__current = new WaitForSeconds(1f);
				this.<>1__state = 1;
				return true;
			}

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			private int <>1__state;

			private object <>2__current;

			public MPController <>4__this;
		}
	}
}
