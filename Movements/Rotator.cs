using UnityEngine;
using System.Collections.Generic;

public class Rotator : MonoBehaviour {
	public float DegreesPerSecond = 90f;
	public Vector3 Axis = Vector3.up;
	
#region Implementation
	void Update() {
		var rotation = Quaternion.AngleAxis( DegreesPerSecond *Time.deltaTime, Axis );
		transform.rotation = rotation *transform.rotation;
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
