using UnityEngine;
using System.Collections.Generic;

public struct Square {
	private Vector2 m_min;
	private Vector2 m_max;
	
	public Vector2 min {
		get { return m_min; }
		set {
			m_min = value;
			_Sanitize();
		}
	}
	public Vector2 max {
		get { return m_max; }
		set {
			m_max = value;
			_Sanitize();
		}
	}
	
	public Vector2 center {
		get {
			return (m_min + m_max) /2;
		}
		set {
			var halfSize = size /2;
			m_min = value - halfSize;
			m_max = value + halfSize;
		}
	}
	
	public Vector2 size {
		get {
			return m_max - m_min;
		}
		set {
			var savedCenter = center;
			var halfSize = value.Abs() /2;
			m_min = savedCenter - halfSize;
			m_max = savedCenter + halfSize;
		}
	}
	
	public Vector2 cornerSW {
		get { return m_min; }
	}
	public Vector2 cornerNW {
		get { return (m_min + Vector2.up *size.y); }
	}
	public Vector2 cornerNE {
		get { return m_max; }
	}
	public Vector2 cornerSE {
		get { return (m_min + Vector2.right *size.x); }
	}
	
	public Range xRange {
		get { return new Range( m_min.x, m_max.x ); }
	}
	public Range yRange {
		get { return new Range( m_min.y, m_max.y ); }
	}
	
#region Implementation
	public Square( Vector2 center, Vector2 size ) {
		var halfSize = size.Abs() /2;
		m_min = center - halfSize;
		m_max = center + halfSize;
	}
#endregion
	
	
#region Public
	public bool Contains( Vector2 point ) {
		var isInside = (point.x >= m_min.x) && (point.x <= m_max.x);
		isInside = isInside && (point.y >= m_min.y) && (point.y <= m_max.y);
		
		return isInside;
	}
	
	public bool Intersects( Square other ) {
		return xRange.Intersects( other.xRange ) && yRange.Intersects( other.yRange );
	}
	
	public Vector2 ClosestPoint( Vector2 point ) {
		if( Contains( point ) ) {
			return point;
		}
		
		var closestPointX = Mathf.Min( Mathf.Max( m_min.x, point.x ), m_max.x );
		var closestPointY = Mathf.Min( Mathf.Max( m_min.y, point.y ), m_max.y );
		return new Vector2( closestPointX, closestPointY );
	}
	
	public Vector2 ClosestPoint( Square other ) {	// returns a point within THIS square that's closest to OTHER
		var closestX = 0f;
		if( m_max.x < other.m_min.x ) {
			closestX = m_max.x;
		}
		else {
			closestX = Mathf.Max( m_min.x, other.m_min.x );
		}
		
		var closestY = 0f;
		if( m_max.y < other.m_min.y ) {
			closestY = m_max.y;
		}
		else {
			closestY = Mathf.Max( m_min.y, other.m_min.y );
		}
		
		return new Vector2( closestX, closestY );
	}
	
	public float Distance( Vector2 point ) {
		if( Contains( point ) ) {
			return 0f;
		}
		
		return Vector2.Distance( ClosestPoint( point ), point );
	}
	
	public float Distance( Square other ) {
		var closestPoint = other.ClosestPoint( this );
		return Distance( closestPoint );
	}
#endregion
	
	
#region Private
	private void _Sanitize() {
		var newMin = new Vector2( Mathf.Min( m_min.x, m_max.x ), Mathf.Min( m_min.y, m_max.y ) );
		var newMax = new Vector2( Mathf.Max( m_min.x, m_max.x ), Mathf.Max( m_min.y, m_max.y ) );
		
		m_min = newMin;
		m_max = newMax;
	}
#endregion
	
	
#region Temporary
#endregion
}
