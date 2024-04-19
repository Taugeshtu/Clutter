using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct RangeInt {
	private int m_min;
	private int m_max;
	
	public int min {
		get { return m_min; }
		set {
			m_min = value;
			_Sanitize();
		}
	}
	
	public int max {
		get { return m_max; }
		set {
			m_max = value;
			_Sanitize();
		}
	}
	
	public int size {
		get { return m_max - m_min; }
	}
	
	public bool IsValid {
		get { return (m_min <= m_max); }
	}
	
	public override string ToString() {
		return $"[{min}, {max}]";
	}
	
	#region Implementation
	public static RangeInt Invalid {
		get { return new RangeInt( 0, -1, false ); }
	}
	
	public static RangeInt ZeroOne {
		get { return new RangeInt( 0, 1, false ); }
	}
	
	public RangeInt( int x ) : this( x, x, false ) {}
	public RangeInt( int start, int end ) : this( start, end, true ) {}
	private RangeInt( int start, int end, bool sanitize ) {
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
			m_min = (int) x;
			m_max = (int) x;
			return;
		}
		
		m_min = (int) Mathf.Min( m_min, x );
		m_max = (int) Mathf.Max( m_max, x );
	}
	
	public void Encapsulate( RangeInt other ) {
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
	
	public bool Intersects( RangeInt other ) {
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
	public int Constrain( int x ) {
		if( x < m_min ) { return m_min; }
		if( x > m_max ) { return m_max; }
		return x;
	}
	
	public float Transform( float localValue ) {
		return  m_min + localValue *size;
	}
	
	public int InverseTransform( float globalValue ) {
		return Mathf.RoundToInt( (globalValue - m_min) /size );
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
