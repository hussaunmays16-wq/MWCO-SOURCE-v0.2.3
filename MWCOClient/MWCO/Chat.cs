using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HutongGames.PlayMaker;
using MWCO.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MWCO
{
	public class Chat : MonoBehaviour
	{
		private void Awake()
		{
			if (Chat.Instance == null)
			{
				Chat.Instance = this;
			}
			this.chatText = base.transform.GetChild(0).GetChild(0).GetComponent<Text>();
			this.chatGroup = this.chatText.GetComponent<CanvasGroup>();
			this.chatRect = this.chatText.GetComponent<RectTransform>();
			this.inputView = base.transform.GetChild(1).gameObject;
			this.inputField = this.inputView.transform.GetChild(0).GetComponent<InputField>();
			this.scrollRect = base.transform.GetChild(0).GetComponent<ScrollRect>();
			base.transform.GetChild(1).GetChild(1).GetComponent<Button>()
				.onClick.AddListener(new UnityAction(this.OnSendButtonClicked));
			this.inputField.onEndEdit.AddListener(new UnityAction<string>(this.HandleInput));
			this.HideInput();
		}

		private void Update()
		{
			if (this.inputView.activeSelf)
			{
				if (Input.GetKeyDown(27))
				{
					this.HideInput();
				}
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
					return;
				}
			}
			else if (this.hideTimer > 0f)
			{
				this.hideTimer -= Time.deltaTime;
				if (this.hideTimer <= 0f)
				{
					this.TimeElapsed();
				}
			}
		}

		public void ShowInput()
		{
			if (this.inputView.activeSelf)
			{
				return;
			}
			this.inputView.SetActive(true);
			this.chatGroup.alpha = 1f;
			this.inputField.ActivateInputField();
			this.inputField.Select();
			this.inputField.text = "";
			this.RefreshChat();
			NetLocalPlayer instance = NetLocalPlayer.Instance;
			if (((instance != null) ? instance.currentVehicle : null) == null)
			{
				FsmVariables.GlobalVariables.GetFsmBool("PlayerInMenu").Value = true;
				FsmVariables.GlobalVariables.GetFsmBool("PlayerStop").Value = true;
				this.disabledVariables = true;
			}
		}

		public void HideInput()
		{
			this.inputField.DeactivateInputField();
			this.inputView.SetActive(false);
			Canvas.ForceUpdateCanvases();
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
			this.hideTimer = 10f;
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

		private void RefreshChat()
		{
			this.chatText.text = string.Join("\n", this.messages.ToArray());
			this.chatRect.sizeDelta = new Vector2(this.chatRect.sizeDelta.x, this.chatText.preferredHeight);
			this.scrollRect.verticalNormalizedPosition = 0f;
		}

		private void OnSendButtonClicked()
		{
			this.HandleInput(this.inputField.text);
		}

		public void HandleInput(string input = "")
		{
			if (input == "`")
			{
				return;
			}
			if (input == "")
			{
				input = this.inputField.text.Trim();
			}
			if (string.IsNullOrEmpty(input))
			{
				return;
			}
			NetManager.Instance.SendChatMsg(input);
			this.inputHistory.Add(input);
			this.historyIndex = -1;
			this.inputField.text = "";
			this.inputField.ActivateInputField();
			this.HideInput();
		}

		public void AddMessage(string message, MessageSeverity severity = MessageSeverity.Info)
		{
			this.chatGroup.alpha = 1f;
			string text = "<color=";
			switch (severity)
			{
			case MessageSeverity.Info:
				text += "white";
				break;
			case MessageSeverity.Error:
				text += "red";
				break;
			case MessageSeverity.Warning:
				text += "yellow";
				break;
			case MessageSeverity.Player:
				text += "lime";
				break;
			case MessageSeverity.Chat:
				text += "white";
				break;
			}
			text += ">";
			this.messages.Add(text + message + "</color>");
			if (this.messages.Count > 500)
			{
				this.messages.RemoveAt(0);
			}
			else if (this.chatText.text.Length > 14000)
			{
				this.messages.RemoveAt(0);
			}
			this.RefreshChat();
			this.hideTimer = 10f;
		}

		public void TimeElapsed()
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.FadeOut());
		}

		private IEnumerator FadeOut()
		{
			Chat.<FadeOut>d__24 <FadeOut>d__ = new Chat.<FadeOut>d__24(0);
			<FadeOut>d__.<>4__this = this;
			return <FadeOut>d__;
		}

		public Chat()
		{
		}

		[Header("UI")]
		private Text chatText;

		private CanvasGroup chatGroup;

		private RectTransform chatRect;

		private InputField inputField;

		private ScrollRect scrollRect;

		private GameObject inputView;

		private readonly List<string> messages = new List<string>();

		private readonly List<string> inputHistory = new List<string>();

		private int historyIndex = -1;

		private const int MAX_MESSAGES = 500;

		private const int MAX_CHARACTERS = 14000;

		public static Chat Instance;

		private float hideTimer;

		private bool disabledVariables;

		[CompilerGenerated]
		private sealed class <FadeOut>d__24 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <FadeOut>d__24(int <>1__state)
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
				Chat chat = this;
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
				}
				if (chat.chatGroup.alpha <= 0f)
				{
					return false;
				}
				chat.chatGroup.alpha -= Time.deltaTime * 3f;
				this.<>2__current = null;
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

			public Chat <>4__this;
		}
	}
}
