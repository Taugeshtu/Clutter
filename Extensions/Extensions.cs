using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class Extensions {
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
	
#region Temporary
#endregion
}
