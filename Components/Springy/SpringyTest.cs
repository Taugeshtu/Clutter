using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringyTest : MonoBehaviour {
	[SerializeField] private Transform _sprungRoot;
	
	[Header( "Params" )]
	[SerializeField] private float _ownFrequency = 10;
	[SerializeField] private float _dampening = 1;
	[SerializeField] private float _response = 0.3f;
	
	private Springy.Vector3 _spring;
	
	void Update() {
		if( _sprungRoot == null )
			return;
		
		var ks = new Springy.Coeffs( _ownFrequency, _dampening, _response );
		if( _spring.ks != ks ) {
			Debug.Log( "Spring regen!" );
			_spring = new Springy.Vector3( _ownFrequency, _dampening, _response, transform.position );
		}
		
		var updateF = 50f;
		var timestep = 1f /updateF;
		timestep = Time.deltaTime;
		_sprungRoot.position = _spring.Tick( transform.position, timestep );
	}
}
