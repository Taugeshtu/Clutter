using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollow : MonoBehaviour {
	public Transform Target;
	public bool Follow = true;
	[Range( 0, 1 )] public float Immediacy = 0.5f;
	
#region Implementation
#endregion
	
	
#region Public
	void LateUpdate() {
		if( (Target != null) && Follow ) {
			var deltaScaling = 60f *Time.deltaTime;
			transform.position = Vector3.Lerp( transform.position, Target.position, Immediacy *deltaScaling );
		}
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
