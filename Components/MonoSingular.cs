using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.Text;

namespace Clutter {
public abstract class MonoSingular<T> : MonoBehaviour where T: MonoSingular<T>, new() {
	private static StringBuilder s_builder = new StringBuilder();
	private static string s_typeString = typeof( T ).ToString();
	private static T s_actualInstance;
	private static BehaviourSettings s_behaviour;
	
	public static T s_Instance {
		get {
			if( s_behaviour == null ) {
				var mockSpawned = new T();
				s_behaviour = mockSpawned.Behaviour;
				DestroyImmediate( mockSpawned );
			}
			
			if( s_actualInstance != null ) { return s_actualInstance; }
			
			var foundInstances = _FindInstances( s_behaviour.SearchInactive );
			if( foundInstances.Count == 0 ) {
				if( s_behaviour.SpawnIfMissing ) {
					s_actualInstance = _SpawnInstance();
				}
				else {
					_logWarning( "No instances found, strategy is 'Don't spawn if missing'. Things will break" );
				}
			}
			else if( foundInstances.Count == 1 ) {
				s_actualInstance = foundInstances[0];
			}
			else {
				_logWarning( "More than one. Will try to not fail by using the first one" );
				s_actualInstance = foundInstances[0];
			}
			
			if( s_behaviour.DontDestroy ) {
				DontDestroyOnLoad( s_actualInstance.gameObject );
			}
			
			return s_actualInstance;
		}
	}
	
#region Behaviour settings
	protected class BehaviourSettings {
		public bool SpawnIfMissing { get; private set; }
		public bool DontDestroy { get; private set; }
		public bool SearchInactive { get; private set; }
		
		public BehaviourSettings( bool spawnIfMissing, bool dontDestroy, bool searchInactive = true ) {
			SpawnIfMissing = spawnIfMissing;
			DontDestroy = dontDestroy;
			SearchInactive = searchInactive;
		}
	}
	
	protected abstract BehaviourSettings Behaviour { get; }
#endregion
	
	
#region Public
#endregion
	
	
#region Internals
	protected static void _log( string message ) {
		var formatted = _FormatMessage( message );
		Debug.Log( formatted, s_actualInstance );
	}
	
	protected static void _logWarning( string message ) {
		var formatted = _FormatMessage( message );
		Debug.LogWarning( formatted, s_actualInstance );
	}
	
	protected static void _logError( string message ) {
		var formatted = _FormatMessage( message );
		Debug.LogError( formatted, s_actualInstance );
	}
#endregion
	
	
#region Private
	private static List<T> _FindInstances( bool searchInactive ) {
		var result = new List<T>();
		
		var scene = SceneManager.GetActiveScene();
		foreach( var root in scene.GetRootGameObjects() ) {
			result.AddRange( root.GetComponentsInChildren<T>( searchInactive ) );
		}
		
		return result;
	}
	
	private static T _SpawnInstance() {
		var holder = new GameObject( s_typeString );
		return holder.AddComponent<T>();
	}
	
	private static string _FormatMessage( string message ) {
		s_builder.Length = 0;
		
		s_builder.Append( "[" );
		s_builder.Append( s_typeString );
		s_builder.Append( "] " );
		s_builder.Append( message );
		
		return s_builder.ToString();
	}
#endregion
}
}
