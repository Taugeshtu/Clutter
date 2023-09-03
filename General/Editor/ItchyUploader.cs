using UnityEngine;
using UnityEditor;
using System.IO;

public class ItchUploader : EditorWindow {
	private static string _butletPath = "";
	
	private static string _buildFolder = "";
	private static string _username = "";
	private static string _game = "";
	private static string _channel = "";
	
	[MenuItem( "Tools/Push -> itch.io" )]
	public static void ShowWindow() {
		_buildFolder = PlayerPrefs.GetString( "itchy_buildFolder", "_Build" );
		_username = PlayerPrefs.GetString( "itchy_username" );
		_game = PlayerPrefs.GetString( "itchy_game" );
		_channel = PlayerPrefs.GetString( "itchy_channel" );
		
		_butletPath = ProcessLauncher.FindExecutablePath( "butler.exe" );
		
		EditorWindow.GetWindow<ItchUploader>( "Itch.io Uploader" );
	}
	
	void OnGUI() {
		if( string.IsNullOrEmpty( _butletPath ) ) {
			GUILayout.Label( "itch.io butler not found! Make sure it's installed and added to system's PATH", EditorStyles.boldLabel );
			return;
		}
		
		GUILayout.Label( "Fill in the details:", EditorStyles.boldLabel );
		
		_buildFolder = EditorGUILayout.TextField("Build Folder:", _buildFolder);
		_username = EditorGUILayout.TextField("Username:", _username);
		_game = EditorGUILayout.TextField("Game:", _game);
		_channel = EditorGUILayout.TextField("Channel:", _channel);
		
		if( string.IsNullOrEmpty( _buildFolder )
		 || string.IsNullOrEmpty( _username )
		 || string.IsNullOrEmpty( _game )
		 || string.IsNullOrEmpty( _channel ) ) {
			GUILayout.Label( "Make sure all fields are set" );
			return;
		}
		
		if( GUILayout.Button( "Upload" ) ) {
			PlayerPrefs.SetString( "itchy_buildFolder", _buildFolder );
			PlayerPrefs.SetString( "itchy_username", _username );
			PlayerPrefs.SetString( "itchy_game", _game );
			PlayerPrefs.SetString( "itchy_channel", _channel );
			_Upload();
		}
	}
	
	private void _Upload() {
		var buildPath = Path.Combine( new DirectoryInfo( Application.dataPath ).Parent.FullName, _buildFolder ).Replace( '\\', '/' );
		var args = $"push {buildPath} {_username}/{_game}:{_channel}";
		ProcessLauncher.Launch( _butletPath, args, true, true );
	}
}
