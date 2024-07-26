using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Sounder {
	[SerializeField] private AudioSource _source;
	[SerializeField] private AudioClip[] _clips;
	[Tooltip( "Span of time in which new sounding requests will be ignored" )] [SerializeField] private int _repeatDeadzoneMS;
	
	private Repeater _repeater;
	
	public void Play( float volumeScale = 1 ) {
		if( _repeater == null )
			_repeater = new Repeater( _repeatDeadzoneMS *0.001f );
		
		if( _repeater[true] ) {
			var clip = _clips[Random.Range( 0, _clips.Length )];
			_source.PlayOneShot( clip, volumeScale );
			Debug.Log( "Playing.. "+Time.time.ToString( "00.000" )+" | "+Time.frameCount );
		}
	}
}
