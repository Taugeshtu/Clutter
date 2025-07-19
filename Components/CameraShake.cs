using UnityEngine;
using System.Collections.Generic;

using Clutter;

[SingularBehaviour( false, false, true )]
public class CameraShake : MonoSingular<CameraShake> {
	[System.Serializable]
	private class ShakeLayer {
		[Header( "Scaling" )]
		public int _layerOffset;
		public float _threshold = 0;
		public float _ampLimit = 1;
		public float _noiseFrequencyHz = 10f;
		public float _maxDuration = 1f;
		
		[Header( "Amplitudes" )]
		public float _translation = 0.1f;
		public Vector3 _rotation = Vector3.one *10f;
		
		public float Trauma { get; set; }
		public float Amplitude => Proc.P2( Trauma ).AtMost( _ampLimit );
		
		public (Vector3 t, Vector3 r) Process() {
			Trauma -= Time.deltaTime /_maxDuration;
			Trauma = Trauma.Clamped01();
			var amplitude = Amplitude;
			if( amplitude < _threshold )
				return (Vector3.zero, Vector3.zero);
			amplitude = Mathf.InverseLerp( _threshold, 1, amplitude );
			
			float _GetNoise( int axis, float frequency ) {
				var value = Mathf.PerlinNoise( axis *0.129f, Time.time *frequency );
				return value *2 - 1f;
			}
			var xNoise = _GetNoise( _layerOffset + 0, _noiseFrequencyHz );
			var yNoise = _GetNoise( _layerOffset + 1024, _noiseFrequencyHz );
			var zNoise = _GetNoise( _layerOffset + 2098, _noiseFrequencyHz );
			var transationShake = new Vector3( xNoise, yNoise, zNoise ) *(_translation *amplitude);
			
			var pitchNoise = _GetNoise( _layerOffset + 13225, _noiseFrequencyHz );
			var yawNoise = _GetNoise( _layerOffset + 4687, _noiseFrequencyHz );
			var rollNoise = _GetNoise( _layerOffset + 5226, _noiseFrequencyHz );
			var rotationShake = new Vector3( pitchNoise, yawNoise, rollNoise ).ComponentMul( _rotation ) *amplitude;
			return (transationShake, rotationShake);
		}
	}
	
	[SerializeField] private ShakeLayer _layer1;
	[SerializeField] private ShakeLayer _layer2;
	[SerializeField] private float m_minorTrauma = 0.4f;
	[SerializeField] private float m_majorTrauma = 1f;
	
	private Vector3 m_savedLocalPosition;
	private Quaternion m_savedLocalRotation;
	
#region Implementation
	void Awake() {
		m_savedLocalPosition = transform.localPosition;
		m_savedLocalRotation = transform.localRotation;
	}
	
	void LateUpdate() {
		var shakeL1 = _layer1.Process();
		var shakeL2 = _layer2.Process();
		transform.localPosition = m_savedLocalPosition + shakeL1.t + shakeL2.t;
		transform.localRotation = m_savedLocalRotation *Quaternion.Euler( shakeL1.r + shakeL2.r );
	}
#endregion
	
	
#region Public
	public static void MakeAShake( bool isMajor ) {
		if( s_Instance != null ) {
			var traumaToAdd = isMajor ? s_Instance.m_majorTrauma : s_Instance.m_minorTrauma;
			AddTrauma( traumaToAdd );
		}
	}
	public static void AddTrauma( float amount ) {
		if( s_Instance != null ) {
			s_Instance._layer1.Trauma += amount;
			s_Instance._layer2.Trauma += amount;
		}
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
