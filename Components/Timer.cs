using UnityEngine;
using System.Collections.Generic;

public struct Timer {
	private static List<Timer> s_timers = new List<Timer>();
	
	private float m_startTime;
	private float m_duration;
	private bool m_independent;
	
	public bool IsDone {
		get { return (Time.time > m_startTime + m_duration + Time.deltaTime); }
	}
	
	public bool IsStarted {
		get { return m_startTime >= 0f; }
	}
	
	public float Factor {
		get {
			if( !IsStarted ) { return 0f; }
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
		for( var i = 0; i < s_timers.Count; i++ ) {
			var timer = s_timers[i];
			timer.m_startTime += amount;
			s_timers[i] = timer;
		}
		Debug.LogError( "FIXME: THIS DOES NOT WORK WITH A STRUCT!" );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
