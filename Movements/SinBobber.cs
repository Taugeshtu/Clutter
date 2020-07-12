using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinBobber : MonoBehaviour {
	public Vector3 Axis = Vector3.up;
	public float Period = 1f;
	public float Amplitude = 0.1f;
	
	private float m_time = 0;
	private Vector3 m_localPosition = Vector3.zero;
	
#region Implementation
	void Start() {
		m_localPosition = transform.localPosition;
	}
	
	void Update() {
		var factor = Mathf.Repeat( m_time /Period, 1f );
		_SetNewPosition( factor );
		
		m_time += Time.deltaTime;
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
	private void _SetNewPosition( float factor ) {
		var sine = Mathf.Sin( factor *2 *Mathf.PI );
		var shift = (sine *Amplitude) *Axis;
		transform.localPosition = m_localPosition + shift;
	}
#endregion
	
	
#region Temporary
#endregion
}
