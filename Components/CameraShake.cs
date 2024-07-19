using UnityEngine;
using System.Collections.Generic;

using Clutter;

[SingularBehaviour( false, true, true )]
public class CameraShake : MonoSingular<CameraShake> {
	[Header( "Scaling" )]
	[SerializeField] private float m_noiseGrain = 10f;
	[SerializeField] private float m_maxDuration = 1f;
	[SerializeField] private float m_minorValue = 0.4f;
	[SerializeField] private float m_majorValue = 1f;
	
	[Header( "Amplitudes" )]
	[SerializeField] private float m_translation = 0.1f;
	[SerializeField] private Vector3 m_rotation = Vector3.one *10f;
	
	private float m_trauma = 0f;
	private Vector3 m_savedLocalPosition;
	private Quaternion m_savedLocalRotation;
	
	private float _amplitude {
		get { return Proc.P2( m_trauma ); }
	}
	
#region Implementation
	void Awake() {
		m_savedLocalPosition = transform.localPosition;
		m_savedLocalRotation = transform.localRotation;
	}
	
	void LateUpdate() {
		m_trauma -= Time.deltaTime /m_maxDuration;
		m_trauma = Mathf.Clamp01( m_trauma );
		var amplitude = _amplitude;
		
		var xNoise = _GetNoise( 0 );
		var yNoise = _GetNoise( 1 );
		var zNoise = _GetNoise( 2 );
		var transationShake = new Vector3( xNoise, yNoise, zNoise ) *(m_translation *amplitude);
		
		var pitchNoise = _GetNoise( 3 );
		var yawNoise = _GetNoise( 4 );
		var rollNoise = _GetNoise( 5 );
		var rotationShake = new Vector3( pitchNoise, yawNoise, rollNoise ).ComponentMul( m_rotation ) *amplitude;
		
		transform.localPosition = m_savedLocalPosition + transationShake;
		transform.localRotation = m_savedLocalRotation *Quaternion.Euler( rotationShake );
	}
#endregion
	
	
#region Public
	public static void MakeAShake( bool isMajor ) {
		s_Instance.m_trauma += (isMajor ? s_Instance.m_majorValue : s_Instance.m_minorValue);
	}
	
	public static void AddTrauma( float amount ) {
		s_Instance.m_trauma += amount;
	}
#endregion
	
	
#region Private
	private float _GetNoise( int axis ) {
		var value = Mathf.PerlinNoise( axis /10f, Time.time *m_noiseGrain );
		return value *2 - 1f;
	}
#endregion
	
	
#region Temporary
#endregion
}
