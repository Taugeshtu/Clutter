using UnityEngine;
using System.Collections.Generic;

public class Timer {
	private static List<Timer> s_timers = new List<Timer>();
	
	private float m_startTime;
	private float m_duration;
	private bool m_independent = false;
	
	public bool IsDone {
		get { return (Time.time > m_startTime + m_duration); }
	}
	
	public bool IsStarted {
		get { return m_startTime >= 0f; }
	}
	
	public float Factor {
		get { return Mathf.InverseLerp( m_startTime, m_startTime + m_duration, Time.time ); }
	}
	
#region Implementation
	public static Timer Off {
		get {
			var result = new Timer( 0 );
			result.m_startTime = -50;
			return result;
		}
	}
	
	public Timer( float duration, bool independent = false ) {
		m_startTime = Time.time;
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
