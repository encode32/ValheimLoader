using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ValheimLoader
{
	public static class HarmonyLoader
	{
		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static void LoadMods()
		{
			try
			{
				Harmony.DEBUG = true;
				string path = Path.Combine(AssemblyDirectory, "..");
				FileLog.logPath = Path.Combine(path, "HarmonyLoader.log");
				try
				{
					File.Delete(FileLog.logPath);
				}
				catch
				{
				}
				string modPath = Path.Combine(path, "Mods");
				if (!Directory.Exists(modPath))
				{
					try
					{
						Directory.CreateDirectory(modPath);
						return;
					}
					catch
					{
						return;
					}
				}
				AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
				{
					Debug.Log("Trying to load assembly: " + args.Name);
					AssemblyName assemblyName = new AssemblyName(args.Name);
					string text3 = Path.Combine(modPath, assemblyName.Name + ".dll");
					if (!File.Exists(text3))
					{
						return null;
					}
					return HarmonyLoader.LoadAssembly(text3);
				};

				foreach (string assemblyPath in Directory.EnumerateFiles(modPath, "*.dll"))
				{
					if (!string.IsNullOrEmpty(assemblyPath))
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyPath);
						string modId = "valheim.mod." + fileNameWithoutExtension;
						HarmonyLoader.Log(modId, "Loading from " + assemblyPath);
						try
						{
							Assembly assembly = HarmonyLoader.LoadAssembly(assemblyPath);
							Harmony harmony = new Harmony(modId);
							harmony.PatchAll(assembly);
						}
						catch (Exception e)
						{
							HarmonyLoader.LogError(modId, "Failed to load: " + assemblyPath);
						}
					}
				}
			}
			finally
			{
				FileLog.FlushBuffer();
			}
		}

		private static Assembly LoadAssembly(string assemblyPath)
		{
			return Assembly.LoadFrom(assemblyPath);
		}

		private static void Log(string harmonyId, object message)
		{
			Debug.Log(string.Format("[HarmonyLoader {0}] {1}", harmonyId, message));
		}

		private static void LogError(string harmonyId, object message)
		{
			Debug.LogError(string.Format("[HarmonyLoader {0}] {1}", harmonyId, message));
		}
	}
}
