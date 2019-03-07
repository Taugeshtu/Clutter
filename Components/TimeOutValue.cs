using UnityEngine;
using System.Collections.Generic;

public enum TOBehaviour {
	Refresh,
	Persist,
	Accumulate
}

public interface ITimeOut {
	void Start( float duration, TOBehaviour behaviour );
}

public class TimeOut : ITimeOut {
	private float m_start;
	private float m_duration;
	
	public bool IsAvailable {
		get { return GetAtTime( Time.time ); }
	}
	
#region Public
	public void Start( float duration, TOBehaviour behaviour ) {
		switch( behaviour ) {
			case TOBehaviour.Refresh:
				_SetTimerFromNow( duration );
				break;
			case TOBehaviour.Persist:
				if( IsAvailable ) {
					_SetTimerFromNow( duration );
				}
				break;
			case TOBehaviour.Accumulate:
				if( IsAvailable ) {
					_SetTimerFromNow( duration );
				}
				else {
					m_duration += duration;
				}
				break;
		}
	}
	
	public bool GetAtTime( float time ) {
		return time >= (m_start + m_duration);
	}
	
	public override string ToString() {
		return "#"+GetHashCode()+"From "+m_start.ToString( "00.00" )+" for "+m_duration.ToString( "00.00" );
	}
#endregion
	
	
#region Private
	private void _SetTimerFromNow( float duration ) {
		m_start = Time.time;
		m_duration = duration;
	}
#endregion
}
