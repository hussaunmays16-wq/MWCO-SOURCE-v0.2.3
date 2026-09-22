using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MWCO.UI
{
	public class Console : MonoBehaviour
	{
		public static void RegisterCommand(string command, Console.CommandDelegate commandDelegate)
		{
			if (!Console.Commands.ContainsKey(command))
			{
				Console.Commands.Add(command, commandDelegate);
			}
		}

		public static bool ExecuteCommand(string command)
		{
			string[] array = command.Split(new char[] { ' ' });
			if (array.Length == 0)
			{
				return false;
			}
			Console.CommandDelegate commandDelegate;
			if (Console.Commands.TryGetValue(array[0].ToLower(), out commandDelegate))
			{
				commandDelegate(array);
				return true;
			}
			return false;
		}

		public static Console Instance
		{
			[CompilerGenerated]
			get
			{
				return Console.<Instance>k__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				Console.<Instance>k__BackingField = value;
			}
		}

		private void Awake()
		{
			Console.Instance = this;
			this.consoleText = base.transform.GetChild(0).GetChild(0).GetComponent<Text>();
			this.consoleRect = this.consoleText.GetComponent<RectTransform>();
			this.inputField = base.transform.GetChild(1).GetChild(0).GetComponent<InputField>();
			this.scrollRect = base.transform.GetChild(0).GetComponent<ScrollRect>();
			base.transform.GetChild(1).GetChild(1).GetComponent<Button>()
				.onClick.AddListener(new UnityAction(this.OnSendButtonClicked));
			this.inputField.onEndEdit.AddListener(new UnityAction<string>(this.HandleInput));
		}

		private void OnEnable()
		{
			this.inputField.ActivateInputField();
			this.inputField.Select();
			this.inputField.text = "";
			this.RefreshConsole();
			NetLocalPlayer instance = NetLocalPlayer.Instance;
			if (((instance != null) ? instance.currentVehicle : null) == null)
			{
				FsmVariables.GlobalVariables.GetFsmBool("PlayerInMenu").Value = true;
				FsmVariables.GlobalVariables.GetFsmBool("PlayerStop").Value = true;
				this.disabledVariables = true;
			}
		}

		private void OnDisable()
		{
			this.inputField.DeactivateInputField();
			if (this.disabledVariables)
			{
				NetLocalPlayer instance = NetLocalPlayer.Instance;
				if (((instance != null) ? instance.currentVehicle : null) == null)
				{
					FsmVariables.GlobalVariables.GetFsmBool("PlayerInMenu").Value = false;
					FsmVariables.GlobalVariables.GetFsmBool("PlayerStop").Value = false;
					this.disabledVariables = false;
				}
			}
		}

		private void OnDestroy()
		{
			if (Console.Instance == this)
			{
				Console.Instance = null;
			}
		}

		private void OnSendButtonClicked()
		{
			this.HandleInput(this.inputField.text);
		}

		public void HandleInput(string input = "")
		{
			if (input == "")
			{
				input = this.inputField.text.Trim();
			}
			if (string.IsNullOrEmpty(input))
			{
				return;
			}
			this.AddMessage("> " + input);
			if (!Console.ExecuteCommand(input))
			{
				this.AddMessage("<color=orange>[Invalid]:</color> Command '" + input + "' doesn't exist");
			}
			this.inputHistory.Add(input);
			this.historyIndex = -1;
			this.inputField.text = "";
			this.inputField.ActivateInputField();
		}

		private void Update()
		{
			if (!this.inputField.isFocused)
			{
				return;
			}
			if (Input.GetKeyDown(273))
			{
				this.CycleHistory(false);
			}
			if (Input.GetKeyDown(274))
			{
				this.CycleHistory(true);
			}
			if (Input.GetKeyDown(13))
			{
				this.HandleInput("");
			}
		}

		private void CycleHistory(bool forward)
		{
			if (this.inputHistory.Count == 0)
			{
				return;
			}
			if (forward)
			{
				this.historyIndex++;
				if (this.historyIndex >= this.inputHistory.Count)
				{
					this.historyIndex = -1;
				}
			}
			else if (this.historyIndex == -1)
			{
				this.historyIndex = this.inputHistory.Count - 1;
			}
			else
			{
				this.historyIndex--;
			}
			this.inputField.text = ((this.historyIndex == -1) ? "" : this.inputHistory[this.historyIndex]);
			this.inputField.MoveTextEnd(false);
		}

		public void AddMessage(string message)
		{
			if (message.StartsWith("["))
			{
				message = "<color=orange>" + message.Split(new char[] { ':' })[0] + "</color>" + message.Substring(message.Split(new char[] { ':' })[0].Length);
			}
			string text = "<color=yellow>[" + DateTime.Now.ToString("HH:mm:ss") + "]</color> " + message;
			this.messages.Add(text.TrimEnd(new char[] { '\n', '\r' }));
			if (this.messages.Count > 500)
			{
				this.messages.RemoveAt(0);
			}
			else if (this.consoleText.text.Length > 14500 && this.messages.Count > 20)
			{
				for (int i = 0; i < 20; i++)
				{
					this.messages.RemoveAt(i);
				}
			}
			this.RefreshConsole();
		}

		private void RefreshConsole()
		{
			this.consoleText.text = string.Join("\n", this.messages.ToArray());
			this.consoleRect.sizeDelta = new Vector2(this.consoleRect.sizeDelta.x, this.consoleText.preferredHeight);
			this.scrollRect.verticalNormalizedPosition = 0f;
		}

		public static void ConsoleMessage(string message)
		{
			if (Console.Instance != null)
			{
				Console.Instance.AddMessage(message);
			}
		}

		public void Clear()
		{
			this.messages.Clear();
			this.consoleText.text = "";
		}

		public Console()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Console()
		{
		}

		private static readonly Dictionary<string, Console.CommandDelegate> Commands = new Dictionary<string, Console.CommandDelegate>();

		[Header("UI")]
		private Text consoleText;

		private RectTransform consoleRect;

		private InputField inputField;

		private ScrollRect scrollRect;

		[CompilerGenerated]
		private static Console <Instance>k__BackingField;

		private readonly List<string> messages = new List<string>();

		private readonly List<string> inputHistory = new List<string>();

		private int historyIndex = -1;

		private const int MAX_MESSAGES = 500;

		private const int MAX_CHARACTERS = 14500;

		private int loading;

		private bool disabledVariables;

		public delegate void CommandDelegate(string[] args);
	}
}
