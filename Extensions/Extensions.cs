using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class Extensions {
#region GOs and Components
	public static List<T> FindInstances<T>( bool searchInactive ) where T: Component {
		var result = new List<T>();
		
		var scene = SceneManager.GetActiveScene();
		foreach( var root in scene.GetRootGameObjects() ) {
			result.AddRange( root.GetComponentsInChildren<T>( searchInactive ) );
		}
		
		return result;
	}
	
	public static T FindInstance<T>( bool searchInactive ) where T: Component {
		var instances = FindInstances<T>( searchInactive );
		if( instances.Count == 0 ) {
			return null;
		}
		
		return instances[0];
	}
	
	public static T GetAddComponent<T>( this Component component ) where T : Component {
		return component.gameObject.GetAddComponent<T>();
	}
	
	public static T GetAddComponent<T>( this GameObject go ) where T : Component {
		T result = default( T );
		var found = go.TryGetComponent<T>( out result );
		if( !found ) {
			result = go.AddComponent<T>();
		}
		return result;
	}
#endregion


#region Textures
	public static Vector2Int GetSize( this Texture texture ) {
		return new Vector2Int( texture.width, texture.height );
	}
#endregion
}
