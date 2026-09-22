using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace MWCO.Utilities
{
	internal class ErrorHandling
	{
		[DllImport("user32.dll")]
		private static extern int MessageBox(IntPtr hwnd, string text, string caption, uint type);

		public static void Raise(string message)
		{
			ErrorHandling.MessageBox(new IntPtr(0), message, "MWCO: Error", 65552U);
			Logger.Fatal(message);
			Application.Quit();
		}

		public static void Invoke(string name, ErrorHandling.InvokeCall call)
		{
			try
			{
				call();
			}
			catch (Exception ex)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Invoke failure at " + name + "!");
				stringBuilder.Append("\n\n" + ex.Message);
				stringBuilder.Append("\n\n" + ex.StackTrace);
				ErrorHandling.Raise(stringBuilder.ToString());
			}
		}

		public static void Assert(bool condition, string message)
		{
			if (condition)
			{
				return;
			}
			Logger.Fatal("[ASSERTION FAILED] " + message);
			ErrorHandling.Raise(message);
		}

		public ErrorHandling()
		{
		}

		public delegate void InvokeCall();
	}
}
