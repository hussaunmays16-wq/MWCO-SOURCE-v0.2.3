using System;
using System.IO;
using System.Reflection;
using MWCO.Game.Hooks;
using MWCO.UI;
using MWCO.Utilities;
using UnityEngine;

namespace MWCO
{
	public class Client
	{
		public static void StartEncrypted(string filePath)
		{
			Client.AssemblyPath = filePath;
			Client.currentDirectory = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(Client.Data))
			{
				Directory.CreateDirectory(Client.Data);
			}
			Logger.Initialize();
			PlayMakerActionHooks.Install();
			string fullPath = Path.GetFullPath(Path.Combine(Client.currentDirectory, "..\\..\\data\\mpdata"));
			if (!File.Exists(fullPath))
			{
				ErrorHandling.Raise("Asset Bundle does not exist at the specified path!\n");
			}
			else
			{
				Client.assetBundle = AssetBundle.CreateFromFile(fullPath);
			}
			Client.uiGameObject = new GameObject("MWCO UI", new Type[] { typeof(MPGUI) });
			Client.controllerGameObject = new GameObject("MWCO Controller", new Type[] { typeof(MPController) });
		}

		public static void Start()
		{
			Client.currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Client.Data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MWCO");
			Client.AssemblyPath = Client.currentDirectory;
			if (!Directory.Exists(Client.Data))
			{
				Directory.CreateDirectory(Client.Data);
			}
			Logger.Initialize();
			PlayMakerActionHooks.Install();
			string fullPath = Path.GetFullPath(Path.Combine(Client.currentDirectory, "..\\..\\data\\mpdata"));
			if (!File.Exists(fullPath))
			{
				ErrorHandling.Raise("Asset Bundle does not exist at the specified path!\n");
			}
			else
			{
				Client.assetBundle = AssetBundle.CreateFromFile(fullPath);
			}
			Client.uiGameObject = new GameObject("MWCO UI", new Type[] { typeof(MPGUI) });
			Client.controllerGameObject = new GameObject("MWCO Controller", new Type[] { typeof(MPController) });
		}

		public static void Restart()
		{
			Object.Destroy(Client.controllerGameObject);
			Client.controllerGameObject = new GameObject("MWCO Controller", new Type[] { typeof(MPController) });
		}

		public static string GetPath(string file)
		{
			return Path.GetDirectoryName(Client.AssemblyPath) + "\\" + file;
		}

		public static T LoadAsset<T>(string name) where T : Object
		{
			return Client.assetBundle.LoadAsset<T>(name);
		}

		public Client()
		{
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Client()
		{
		}

		private static AssetBundle assetBundle;

		public static GameObject controllerGameObject;

		public static GameObject uiGameObject;

		public static string ModVersion = "0.2.3-v032h1";

		public static string Data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MWCO");

		public static string currentDirectory = null;

		public static string AssemblyPath;
	}
}
