using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering;
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
	
	public static IEnumerable<Transform> GetChildren( this Transform transform ) {
		for( var i = 0; i < transform.childCount; i++ ) {
			yield return transform.GetChild( i );
		}
	}
#endregion


#region Textures
	public static Vector2Int GetSize( this Texture texture ) {
		return new Vector2Int( texture.width, texture.height );
	}
	
	public static Texture2D Clone( this Texture2D source ) {
		var target = new Texture2D( source.width, source.height, source.graphicsFormat, TextureCreationFlags.None );
		target.filterMode = source.filterMode;
		Graphics.CopyTexture( source, target );
		return target;
	}
	
	public static RenderTexture Clone( this RenderTexture source ) {
		var target = new RenderTexture( source );
		target.filterMode = source.filterMode;
		Graphics.CopyTexture( source, target );
		return target;
	}
#endregion
}
