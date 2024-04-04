using System.Diagnostics;
using UnityEngine;
using UnityEditor;

public static class AppFoldersOpener {
	[MenuItem("Tools/Open Persistent data", false, -200)]
	private static void OpenPersistentData() {
		Process.Start( Application.persistentDataPath );
	}
	
	[MenuItem("Tools/Open Streaming Assets", false, -150)]
	private static void OpenSteamingAssets() {
		Process.Start( Application.streamingAssetsPath );
	}
}
