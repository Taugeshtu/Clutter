using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.Text;

namespace Clutter {
[System.AttributeUsage( System.AttributeTargets.Class, Inherited = false )]
public class SingularBehaviour : System.Attribute {
	public bool SpawnIfMissing { get; private set; }
	public bool DontDestroy { get; private set; }
	public bool SearchInactive { get; private set; }
	
	public static SingularBehaviour Default {
		get {
			return new SingularBehaviour( false, false, false );;
		}
	}
	
	public SingularBehaviour( bool spawnIfMissing, bool dontDestroy, bool searchInactive ) {
		SpawnIfMissing = spawnIfMissing;
		DontDestroy = dontDestroy;
		SearchInactive = searchInactive;
	}
}

public abstract class MonoSingular<T> : MonoBehaviour where T: MonoSingular<T>, new() {
	private static StringBuilder s_builder = new StringBuilder();
	private static string s_typeString = typeof( T ).ToString();
	private static T s_actualInstance;
	
	public static T s_Instance {
		get {
			if( s_actualInstance != null ) { return s_actualInstance; }
			
			var behaviour = System.Attribute.GetCustomAttribute( typeof( T ), typeof( SingularBehaviour ) ) as SingularBehaviour;
			if( behaviour == null ) {
				behaviour = SingularBehaviour.Default;
				_logWarning( "Behaviour is not defined! Consider using [SingularBehaviour] attribute; using Default for now" );
			}
			
			var foundInstances = Extensions.FindInstances<T>( behaviour.SearchInactive );
			if( foundInstances.Count == 0 ) {
				if( behaviour.SpawnIfMissing ) {
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
			
			if( behaviour.DontDestroy && (s_actualInstance != null) ) {
				DontDestroyOnLoad( s_actualInstance.gameObject );
			}
			
			return s_actualInstance;
		}
	}
	
	
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
