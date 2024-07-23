using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Basically:
- Init with repetition period, optionally specify deadzone as well
- Query IsShot() feeding in your input - it'll clap back once every "period" seconds, but ignore key held during deadzone
- query .x to get a true once every _period seconds

it is unsafe for multiple uses within a frame; if you need a value - cache it
*/
public class Repeater {
	public float Period;
	public float PreBufferTime;	// this is game design "niceness" - even if you are this much "early" to press a key for a repeat, it'll still work
	
	private bool _bufferedHot = false;
	private float _lastShotTime = float.MinValue;
	
	public Repeater() {}
	public Repeater( float period ) : this( period, 0.15f ) {}
	public Repeater( float period, float preBufferTime ) {
		Period = period;
		PreBufferTime = preBufferTime;
	}
	
	public bool this[bool isHot] {
		get {
			var isInRepeat = Time.time < (_lastShotTime + Period);
			var isInBufferingZone = Time.time < (_lastShotTime + Period - PreBufferTime);
			
			if( !isInRepeat ) {
				if( isHot || _bufferedHot ) {
					_bufferedHot = false;
					_lastShotTime = Time.time;
				}
				return isHot;
			}
			else if( isInBufferingZone ) {
				_bufferedHot |= isHot;
				return false;
			}
			else {
				// no internal state changes - we're ignoring flutter on "isHot"
				return false;
			}
		}
	}
}
