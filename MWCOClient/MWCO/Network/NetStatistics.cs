using System;
using System.Runtime.CompilerServices;
using MWCO.Utilities;
using Steamworks;
using UnityEngine;

namespace MWCO.Network
{
	internal class NetStatistics
	{
		public NetStatistics(NetManager netManager)
		{
			this.netManager = netManager;
		}

		private void SetupLineMaterial()
		{
			if (this.lineMaterial != null)
			{
				return;
			}
			Shader shader = Shader.Find("GUI/Text Shader");
			ErrorHandling.Assert(shader != null, "Shader not found!");
			this.lineMaterial = new Material(shader);
			ErrorHandling.Assert(this.lineMaterial != null, "Failed to setup material!");
		}

		public void NewFrame()
		{
			this.packetsSendLastFrame = this.packetsSendCurrentFrame;
			this.packetsReceivedLastFrame = this.packetsReceivedCurrentFrame;
			this.packetsSendCurrentFrame = 0;
			this.packetsReceivedCurrentFrame = 0;
			this.bytesSentLastFrame = this.bytesSentCurrentFrame;
			this.bytesReceivedLastFrame = this.bytesReceivedCurrentFrame;
			this.maxBytesReceivedInHistory = this.bytesSentCurrentFrame;
			this.maxBytesSentInHistory = this.bytesReceivedCurrentFrame;
			for (int i = 0; i < 99; i++)
			{
				this.bytesSentHistory[i] = this.bytesSentHistory[i + 1];
				if (this.maxBytesSentInHistory < this.bytesSentHistory[i])
				{
					this.maxBytesSentInHistory = this.bytesSentHistory[i];
				}
				this.bytesReceivedHistory[i] = this.bytesReceivedHistory[i + 1];
				if (this.maxBytesReceivedInHistory < this.bytesReceivedHistory[i])
				{
					this.maxBytesReceivedInHistory = this.bytesReceivedHistory[i];
				}
			}
			this.bytesSentHistory[99] = this.bytesSentCurrentFrame;
			this.bytesReceivedHistory[99] = this.bytesReceivedCurrentFrame;
			this.bytesSentCurrentFrame = 0L;
			this.bytesReceivedCurrentFrame = 0L;
		}

		public void RecordSendMessage(int messageId, long bytes)
		{
			this.bytesSentCurrentFrame += bytes;
			this.bytesSentTotal += bytes;
			this.packetsSendCurrentFrame++;
			this.packetsSendTotal++;
		}

		public void RecordReceivedMessage(int messageId, long bytes)
		{
			this.bytesReceivedCurrentFrame += bytes;
			this.bytesReceivedTotal += bytes;
			this.packetsReceivedCurrentFrame++;
			this.packetsReceivedTotal++;
		}

		private void DrawStatHelper(ref Rect rct, string name, long value, int critical = -1, bool bytes = false)
		{
			GUI.color = Color.white;
			GUI.Label(rct, name);
			GUI.color = ((critical != -1 && value >= (long)critical) ? Color.red : Color.white);
			rct.x += rct.width;
			if (bytes)
			{
				GUI.Label(rct, this.FormatBytes(value));
			}
			else
			{
				GUI.Label(rct, value.ToString());
			}
			rct.x -= rct.width;
			rct.y += rct.height;
		}

		private void DrawTextHelper(ref Rect rct, string name, string text)
		{
			GUI.color = Color.white;
			GUI.Label(rct, name);
			rct.x += rct.width;
			GUI.Label(rct, text);
			rct.x -= rct.width;
			rct.y += rct.height;
		}

		private void DrawLineHelper(Vector2 start, Vector2 end, Color color)
		{
			GL.Color(color);
			GL.Vertex3(start.x, (float)Screen.height - start.y, 0f);
			GL.Vertex3(end.x, (float)Screen.height - end.y, 0f);
		}

		private void DrawGraph(Rect drawRect)
		{
			this.SetupLineMaterial();
			this.lineMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix();
			GL.Begin(1);
			this.DrawLineHelper(new Vector2(drawRect.x, drawRect.y), new Vector2(drawRect.x + drawRect.width, drawRect.y), Color.gray);
			this.DrawLineHelper(new Vector2(drawRect.x, drawRect.y), new Vector2(drawRect.x, drawRect.y + drawRect.height), Color.gray);
			this.DrawLineHelper(new Vector2(drawRect.x + drawRect.width, drawRect.y), new Vector2(drawRect.x + drawRect.width, drawRect.y + drawRect.height), Color.gray);
			this.DrawLineHelper(new Vector2(drawRect.x, drawRect.y + drawRect.height), new Vector2(drawRect.x + drawRect.width, drawRect.y + drawRect.height), Color.gray);
			float num = drawRect.width / 100f;
			for (int i = 0; i < 100; i++)
			{
				long num2 = ((i > 0) ? this.bytesSentHistory[i - 1] : 0L);
				float num3 = drawRect.y + drawRect.height * Mathf.Clamp01(1f - (float)num2 / Mathf.Max(1f, (float)this.maxBytesSentInHistory));
				Vector2 vector;
				vector..ctor(drawRect.x + num * (float)Mathf.Max(i - 1, 0), num3);
				float num4 = drawRect.y + drawRect.height * Mathf.Clamp01(1f - (float)this.bytesSentHistory[i] / Mathf.Max(1f, (float)this.maxBytesSentInHistory));
				Vector2 vector2;
				vector2..ctor(drawRect.x + num * (float)i, num4);
				this.DrawLineHelper(vector, vector2, Color.red);
				num2 = ((i > 0) ? this.bytesReceivedHistory[i - 1] : 0L);
				num3 = drawRect.y + drawRect.height * Mathf.Clamp01(1f - (float)num2 / Mathf.Max(1f, (float)this.maxBytesReceivedInHistory));
				vector..ctor(drawRect.x + num * (float)Mathf.Max(i - 1, 0), num3);
				num4 = drawRect.y + drawRect.height * Mathf.Clamp01(1f - (float)this.bytesReceivedHistory[i] / Mathf.Max(1f, (float)this.maxBytesReceivedInHistory));
				vector2..ctor(drawRect.x + num * (float)i, num4);
				this.DrawLineHelper(vector, vector2, Color.green);
			}
			GL.End();
			GL.PopMatrix();
		}

		private string FormatBytes(long bytes)
		{
			if (bytes >= 1048576L)
			{
				return ((float)bytes / 1048576f).ToString("0.00") + " MB";
			}
			if (bytes >= 1024L)
			{
				return ((float)bytes / 1024f).ToString("0.00") + " KB";
			}
			return bytes.ToString() + " B";
		}

		public void Draw()
		{
			GUI.color = Color.white;
			Rect statsWindowRect = new Rect((float)(Screen.width - 300 - 10), (float)(Screen.height - 600 - 10), 300f, 600f);
			GUI.Window(666, statsWindowRect, delegate(int window)
			{
				Rect rect;
				rect..ctor(10f, 20f, 200f, 25f);
				GUI.Label(rect, string.Format("Traffic graph (last {0} frames):", 100));
				rect.y += 25f;
				Rect rect2;
				rect2..ctor(rect.x, rect.y, 280f, 100f);
				GUI.color = new Color(0f, 0f, 0f, 0.35f);
				IMGUIUtils.DrawPlainColorRect(rect2);
				rect2.x += statsWindowRect.x;
				rect2.y += statsWindowRect.y;
				this.DrawGraph(rect2);
				GUI.color = Color.white;
				rect.y += 5f;
				rect.x += 5f;
				IMGUIUtils.DrawSmallLabel(this.FormatBytes(this.maxBytesSentInHistory) + " sent/frame", rect, Color.red, true);
				rect.y += 12f;
				IMGUIUtils.DrawSmallLabel(this.FormatBytes(this.maxBytesReceivedInHistory) + " recv/frame", rect, Color.green, true);
				rect.y -= 7f;
				rect.x -= 5f;
				rect.y += rect2.height;
				rect.height = 20f;
				GUI.color = Color.black;
				IMGUIUtils.DrawPlainColorRect(new Rect(0f, rect.y, 300f, 2f));
				rect.y += 2f;
				GUI.color = new Color(0f, 0f, 0f, 0.5f);
				IMGUIUtils.DrawPlainColorRect(new Rect(0f, rect.y, 300f, 600f - rect.y));
				this.DrawStatHelper(ref rect, "packetsSendTotal", (long)this.packetsSendTotal, -1, false);
				this.DrawStatHelper(ref rect, "packetsReceivedTotal", (long)this.packetsReceivedTotal, -1, false);
				this.DrawStatHelper(ref rect, "packetsSendLastFrame", (long)this.packetsSendLastFrame, 1000, false);
				this.DrawStatHelper(ref rect, "packetsReceivedLastFrame", (long)this.packetsReceivedLastFrame, 1000, false);
				this.DrawStatHelper(ref rect, "packetsSendCurrentFrame", (long)this.packetsSendCurrentFrame, 1000, false);
				this.DrawStatHelper(ref rect, "packetsReceivedCurrentFrame", (long)this.packetsReceivedCurrentFrame, 1000, false);
				this.DrawStatHelper(ref rect, "bytesSendTotal", this.bytesSentTotal, -1, true);
				this.DrawStatHelper(ref rect, "bytesReceivedTotal", this.bytesReceivedTotal, -1, true);
				this.DrawStatHelper(ref rect, "bytesSendLastFrame", this.bytesSentLastFrame, 1000, true);
				this.DrawStatHelper(ref rect, "bytesReceivedLastFrame", this.bytesReceivedLastFrame, 1000, true);
				this.DrawStatHelper(ref rect, "bytesSendCurrentFrame", this.bytesSentCurrentFrame, 1000, true);
				this.DrawStatHelper(ref rect, "bytesReceivedCurrentFrame", this.bytesReceivedCurrentFrame, 1000, true);
				rect.y += 2f;
				GUI.color = Color.black;
				IMGUIUtils.DrawPlainColorRect(new Rect(0f, rect.y, 300f, 2f));
				rect.y += 2f;
				this.DrawTextHelper(ref rect, "Steam session state:", "");
				P2PSessionState_t p2PSessionState_t = default(P2PSessionState_t);
				if (this.netManager.GetP2PSessionState(out p2PSessionState_t))
				{
					this.DrawTextHelper(ref rect, "Is Connecting", p2PSessionState_t.m_bConnecting.ToString());
					this.DrawTextHelper(ref rect, "Is connection active", (p2PSessionState_t.m_bConnectionActive == 0) ? "no" : "yes");
					this.DrawTextHelper(ref rect, "Using relay?", (p2PSessionState_t.m_bConnectionActive == 0) ? "no" : "yes");
					this.DrawTextHelper(ref rect, "Session error", Utils.P2PSessionErrorToString(p2PSessionState_t.m_eP2PSessionError));
					this.DrawTextHelper(ref rect, "Bytes queued for send", this.FormatBytes((long)p2PSessionState_t.m_nBytesQueuedForSend));
					this.DrawTextHelper(ref rect, "Packets queued for send", p2PSessionState_t.m_nPacketsQueuedForSend.ToString());
					this.DrawTextHelper(ref rect, "Remote ip", "HIDDEN");
					this.DrawTextHelper(ref rect, "Remote port", p2PSessionState_t.m_nRemotePort.ToString());
					return;
				}
				this.DrawTextHelper(ref rect, "Session inactive.", "");
			}, "Network statistics");
		}

		private const int HISTORY_SIZE = 100;

		private long[] bytesReceivedHistory = new long[100];

		private long[] bytesSentHistory = new long[100];

		private long maxBytesReceivedInHistory;

		private long maxBytesSentInHistory;

		private int packetsSendTotal;

		private int packetsReceivedTotal;

		private int packetsSendLastFrame;

		private int packetsReceivedLastFrame;

		private int packetsSendCurrentFrame;

		private int packetsReceivedCurrentFrame;

		private long bytesSentTotal;

		private long bytesReceivedTotal;

		private long bytesSentLastFrame;

		private long bytesReceivedLastFrame;

		private long bytesSentCurrentFrame;

		private long bytesReceivedCurrentFrame;

		private NetManager netManager;

		private Material lineMaterial;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass29_0
		{
			public <>c__DisplayClass29_0()
			{
			}

			internal void <Draw>b__0(int window)
			{
				Rect rect;
				rect..ctor(10f, 20f, 200f, 25f);
				GUI.Label(rect, string.Format("Traffic graph (last {0} frames):", 100));
				rect.y += 25f;
				Rect rect2;
				rect2..ctor(rect.x, rect.y, 280f, 100f);
				GUI.color = new Color(0f, 0f, 0f, 0.35f);
				IMGUIUtils.DrawPlainColorRect(rect2);
				rect2.x += this.statsWindowRect.x;
				rect2.y += this.statsWindowRect.y;
				this.<>4__this.DrawGraph(rect2);
				GUI.color = Color.white;
				rect.y += 5f;
				rect.x += 5f;
				IMGUIUtils.DrawSmallLabel(this.<>4__this.FormatBytes(this.<>4__this.maxBytesSentInHistory) + " sent/frame", rect, Color.red, true);
				rect.y += 12f;
				IMGUIUtils.DrawSmallLabel(this.<>4__this.FormatBytes(this.<>4__this.maxBytesReceivedInHistory) + " recv/frame", rect, Color.green, true);
				rect.y -= 7f;
				rect.x -= 5f;
				rect.y += rect2.height;
				rect.height = 20f;
				GUI.color = Color.black;
				IMGUIUtils.DrawPlainColorRect(new Rect(0f, rect.y, 300f, 2f));
				rect.y += 2f;
				GUI.color = new Color(0f, 0f, 0f, 0.5f);
				IMGUIUtils.DrawPlainColorRect(new Rect(0f, rect.y, 300f, 600f - rect.y));
				this.<>4__this.DrawStatHelper(ref rect, "packetsSendTotal", (long)this.<>4__this.packetsSendTotal, -1, false);
				this.<>4__this.DrawStatHelper(ref rect, "packetsReceivedTotal", (long)this.<>4__this.packetsReceivedTotal, -1, false);
				this.<>4__this.DrawStatHelper(ref rect, "packetsSendLastFrame", (long)this.<>4__this.packetsSendLastFrame, 1000, false);
				this.<>4__this.DrawStatHelper(ref rect, "packetsReceivedLastFrame", (long)this.<>4__this.packetsReceivedLastFrame, 1000, false);
				this.<>4__this.DrawStatHelper(ref rect, "packetsSendCurrentFrame", (long)this.<>4__this.packetsSendCurrentFrame, 1000, false);
				this.<>4__this.DrawStatHelper(ref rect, "packetsReceivedCurrentFrame", (long)this.<>4__this.packetsReceivedCurrentFrame, 1000, false);
				this.<>4__this.DrawStatHelper(ref rect, "bytesSendTotal", this.<>4__this.bytesSentTotal, -1, true);
				this.<>4__this.DrawStatHelper(ref rect, "bytesReceivedTotal", this.<>4__this.bytesReceivedTotal, -1, true);
				this.<>4__this.DrawStatHelper(ref rect, "bytesSendLastFrame", this.<>4__this.bytesSentLastFrame, 1000, true);
				this.<>4__this.DrawStatHelper(ref rect, "bytesReceivedLastFrame", this.<>4__this.bytesReceivedLastFrame, 1000, true);
				this.<>4__this.DrawStatHelper(ref rect, "bytesSendCurrentFrame", this.<>4__this.bytesSentCurrentFrame, 1000, true);
				this.<>4__this.DrawStatHelper(ref rect, "bytesReceivedCurrentFrame", this.<>4__this.bytesReceivedCurrentFrame, 1000, true);
				rect.y += 2f;
				GUI.color = Color.black;
				IMGUIUtils.DrawPlainColorRect(new Rect(0f, rect.y, 300f, 2f));
				rect.y += 2f;
				this.<>4__this.DrawTextHelper(ref rect, "Steam session state:", "");
				P2PSessionState_t p2PSessionState_t = default(P2PSessionState_t);
				if (this.<>4__this.netManager.GetP2PSessionState(out p2PSessionState_t))
				{
					this.<>4__this.DrawTextHelper(ref rect, "Is Connecting", p2PSessionState_t.m_bConnecting.ToString());
					this.<>4__this.DrawTextHelper(ref rect, "Is connection active", (p2PSessionState_t.m_bConnectionActive == 0) ? "no" : "yes");
					this.<>4__this.DrawTextHelper(ref rect, "Using relay?", (p2PSessionState_t.m_bConnectionActive == 0) ? "no" : "yes");
					this.<>4__this.DrawTextHelper(ref rect, "Session error", Utils.P2PSessionErrorToString(p2PSessionState_t.m_eP2PSessionError));
					this.<>4__this.DrawTextHelper(ref rect, "Bytes queued for send", this.<>4__this.FormatBytes((long)p2PSessionState_t.m_nBytesQueuedForSend));
					this.<>4__this.DrawTextHelper(ref rect, "Packets queued for send", p2PSessionState_t.m_nPacketsQueuedForSend.ToString());
					this.<>4__this.DrawTextHelper(ref rect, "Remote ip", "HIDDEN");
					this.<>4__this.DrawTextHelper(ref rect, "Remote port", p2PSessionState_t.m_nRemotePort.ToString());
					return;
				}
				this.<>4__this.DrawTextHelper(ref rect, "Session inactive.", "");
			}

			public Rect statsWindowRect;

			public NetStatistics <>4__this;
		}
	}
}
