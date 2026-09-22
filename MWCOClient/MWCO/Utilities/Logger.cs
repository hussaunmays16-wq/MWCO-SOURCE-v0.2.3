using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using MWCO.UI;
using UnityEngine;

namespace MWCO.Utilities
{
	internal class Logger
	{
		public static void Initialize()
		{
			if (!Directory.Exists(Path.Combine(Client.Data, "Logs\\Client")))
			{
				Directory.CreateDirectory(Path.Combine(Client.Data, "Logs\\Client"));
			}
			try
			{
				Logger._writer = new StreamWriter(Path.Combine(Client.Data, "Logs\\Client\\" + Logger.FileName));
			}
			catch
			{
			}
			Logger._writer.WriteLine(string.Format("Started diagnostics logging for MWCO at {0:dd-M-yyyy-HH:mm:ss}.", DateTime.Now));
			Logger._writer.WriteLine("Current MWCO Version: " + Client.ModVersion);
			Logger._writer.WriteLine(string.Format("OS: {0}", Logger.RegistryKey.GetValue("productName")));
			Logger._writer.WriteLine(string.Format("CPU: {0} | Cores: {1}", SystemInfo.processorType, SystemInfo.processorCount));
			Logger._writer.WriteLine(string.Format("RAM: {0} MB", SystemInfo.systemMemorySize));
			Logger._writer.WriteLine(string.Format("GPU: {0} | VRAM: {1} MB | Graphics API: {2}", SystemInfo.graphicsDeviceName, SystemInfo.graphicsMemorySize, SystemInfo.graphicsDeviceVersion));
			Logger._writer.WriteLine("--------------------------------------------------------------------------");
		}

		private static void Write(string message)
		{
			StreamWriter writer = Logger._writer;
			if (writer == null)
			{
				return;
			}
			writer.WriteLine(message);
		}

		public static void Info(string message)
		{
			StackFrame stackFrame = new StackFrame(1);
			Logger.Write(string.Format("{0:HH:mm:ss} [INFO] {1} -> {2} | {3}", new object[]
			{
				DateTime.Now,
				stackFrame.GetMethod().DeclaringType,
				stackFrame.GetMethod().Name,
				message
			}));
		}

		public static void Warning(string message)
		{
			StackFrame stackFrame = new StackFrame(1);
			Logger.Write(string.Format("{0:HH:mm:ss} [WARNING] {1} -> {2} | {3}", new object[]
			{
				DateTime.Now,
				stackFrame.GetMethod().DeclaringType,
				stackFrame.GetMethod().Name,
				message
			}));
		}

		public static void Error(string message)
		{
			StackFrame stackFrame = new StackFrame(1);
			Logger.Write(string.Format("{0:HH:mm:ss} [ERROR] {1} -> {2} | {3}", new object[]
			{
				DateTime.Now,
				stackFrame.GetMethod().DeclaringType,
				stackFrame.GetMethod().Name,
				message
			}));
		}

		public static void Fatal(string message)
		{
			StackFrame stackFrame = new StackFrame(1);
			Logger.Write(string.Format("{0:HH:mm:ss} [FATAL] {1} -> {2} | {3}", new object[]
			{
				DateTime.Now,
				stackFrame.GetMethod().DeclaringType,
				stackFrame.GetMethod().Name,
				message
			}));
		}

		public static void Debug(string message)
		{
			StackFrame stackFrame = new StackFrame(1);
			Logger.Write(string.Format("{0:HH:mm:ss} [DEBUG] {1} -> {2} | {3}", new object[]
			{
				DateTime.Now,
				stackFrame.GetMethod().DeclaringType,
				stackFrame.GetMethod().Name,
				message
			}));
			MWCO.UI.Console.ConsoleMessage(message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0]);
		}

		public static void Shutdown()
		{
			Logger._writer.Flush();
		}

		public Logger()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Logger()
		{
		}

		private static readonly string FileName = string.Format("MWCO_{0:dd-M-yyyy}.log", DateTime.Now);

		private static StreamWriter _writer;

		private static readonly RegistryKey RegistryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
	}
}
