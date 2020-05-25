using UnityEngine;
using System.Collections.Generic;

public struct Range {
	private float m_min;
	private float m_max;
	
	public float min {
		get { return m_min; }
		set {
			m_min = value;
			_Sanitize();
		}
	}
	
	public float max {
		get { return m_max; }
		set {
			m_max = value;
			_Sanitize();
		}
	}
	
	public float size {
		get { return m_max - m_min; }
	}
	
	public bool IsValid {
		get { return (m_min <= m_max); }
	}
	
#region Implementation
	public static Range Invalid {
		get { return new Range( 0, -1, false ); }
	}
	
	public static Range ZeroOne {
		get { return new Range( 0, 1, false ); }
	}
	
	public Range( float x ) : this( x, x, false ) {}
	public Range( float start, float end ) : this( start, end, true ) {}
	private Range( float start, float end, bool sanitize ) {
		m_min = start;
		m_max = end;
		
		if( sanitize ) {
			_Sanitize();
		}
	}
#endregion
	
	
#region Public
	public void Encapsulate( float x ) {
		if( !IsValid ) {
			m_min = x;
			m_max = x;
			return;
		}
		
		m_min = Mathf.Min( m_min, x );
		m_max = Mathf.Max( m_max, x );
	}
	
	public void Encapsulate( Range other ) {
		if( !other.IsValid ) {
			return;
		}
		
		if( !IsValid ) {
			m_min = other.m_min;
			m_max = other.m_max;
			return;
		}
		
		Encapsulate( other.m_min );
		Encapsulate( other.m_max );
	}
	
	public bool Contains( float x ) {
		return (x >= m_min) && (x <= m_max);
	}
	
	public bool Intersects( Range other ) {
		if( !IsValid || !other.IsValid ) {
			return false;
		}
		
		if( other.m_min > m_max ) {
			return false;
		}
		
		return (other.m_max >= m_min);
	}
	
	public float Constrain( float x ) {
		if( x < m_min ) { return m_min; }
		if( x > m_max ) { return m_max; }
		return x;
	}
#endregion
	
	
#region Private
	private void _Sanitize() {
		if( m_max < m_min ) {
			var store = m_max;
			m_max = m_min;
			m_min = store;
		}
	}
#endregion
	
	
#region Temporary
#endregion
}
