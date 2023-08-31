using UnityEngine;
using System.Collections.Generic;

public class LifecycleWatcher : MonoBehaviour {
	public string tag;
	public bool watchAwake = false;
	public bool watchDestroy = false;
	public bool watchEnable = false;
	public bool watchDisable = false;
	
	public bool watchLife = true;
	public bool watchActive = false;
	
	void Awake() {
		if( watchAwake || watchLife )
			_Log( "Awake()" );
	}
	
	void OnEnable() {
		if( watchEnable || watchActive )
			_Log( "OnEnable()" );
	}
	
	void OnDisable() {
		if( watchDisable || watchActive )
			_Log( "OnDisable()" );
	}
	
	void OnDestroy() {
		if( watchDestroy || watchLife )
			_Log( "OnDestroy()" );
	}
	
	private void _Log( string message ) {
		var tagString = string.IsNullOrEmpty( tag ) ? gameObject.name : tag;
		Debug.Log( Time.frameCount+" \""+tagString+"\" | "+message+", hc: "+GetHashCode(), this );
	}
}
