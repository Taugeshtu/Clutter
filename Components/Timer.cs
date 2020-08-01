using UnityEngine;
using System.Collections.Generic;

// TODO: figure out how to switch that to struct, so it's super-duper light
public class Timer {
	private static List<Timer> s_timers = new List<Timer>();
	
	private float m_startTime = -50;
	private float m_duration;
	private bool m_independent = false;
	
	public bool IsDone {
		get { return (Time.time > m_startTime + m_duration + Time.deltaTime); }
	}
	
	public bool IsStarted {
		get { return m_startTime >= 0f; }
	}
	
	public float Factor {
		get {
			if( !IsStarted ) { return 1f; }	// really have to consider reworking that on CK's end. This is a crutch
			return Mathf.InverseLerp( m_startTime, m_startTime + m_duration, Time.time );
		}
	}
	
	public float Elapsed {
		get {
			if( !IsStarted ) { return 0; }
			return Time.time - m_startTime;
		}
	}
	
#region Implementation
	public static Timer Off {
		get {
			var result = new Timer( -50, 0 );
			return result;
		}
	}
	
	public Timer( float duration, bool independent = false ) : this( Time.time, duration, independent ) {}
	
	private Timer( float startTime, float duration, bool independent = false ) {
		m_startTime = startTime;
		m_duration = duration;
		m_independent = independent;
		
		s_timers.Add( this );
	}
#endregion
	
	
#region Public
	public static void ShiftAll( float amount ) {
		foreach( var timer in s_timers ) {
			if( timer.m_independent ) { continue; }
			timer.m_startTime += amount;
		}
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
