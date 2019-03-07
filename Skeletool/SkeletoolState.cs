using UnityEngine;
using System;
using System.Collections.Generic;

namespace SkeletoolInner {
[Serializable]
public class SkeletoolPose {
	public string Name;
	public Transform Root;
	
#region Public
	public SkeletoolPose( string name, Transform root ) {
		Name = name;
		Root = root;
	}
	
	public void Nuke() {
		_IterateAllNonTransforms(
			(x) => { GameObject.Destroy( x ); }
		);
	}
	
	public void NukeImmediate() {
		_IterateAllNonTransforms(
			(x) => { GameObject.DestroyImmediate( x ); }
		);
	}
#endregion
	
	
#region Private
	private void _IterateAllNonTransforms( Action<Component> callback ) {
		if( Root == null ) {
			return;
		}
		
		var componentsList = new List<Component>();
		var children = Root.GetComponentsInChildren<Transform>();
		
		foreach( var child in children ) {
			var childComponents = child.GetComponents<Component>();
			foreach( var component in childComponents ) {
				if( !(component is Transform) ) {
					componentsList.Add( component );
				}
			}
		}
		
		foreach( var component in componentsList ) {
			callback( component );
		}
	}
#endregion
	
	
#region Temporary
#endregion
}
}