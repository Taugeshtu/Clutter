using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ValueCurve {
	[SerializeField] private float m_timeScale = 1f;
	[SerializeField] private float m_valueScale = 1f;
	[SerializeField] private List<Vector2> m_points;
	private bool m_isDirty = true;
	
#region Implementation
#endregion
	
	
#region Public
	public float Evaluate( float timePoint ) {
		_Sort();
		timePoint = timePoint /m_timeScale;
		
		Vector2 left;
		Vector2 right;
		_GetPairForPoint( timePoint, out left, out right );
		
		var factor = Mathf.InverseLerp( left.x, right.x, timePoint );
		return Mathf.Lerp( left.y, right.y, factor ) *m_valueScale;
	}
#endregion
	
	
#region Private
	private void _Sort() {
		if( !m_isDirty ) {
			return;
		}
		
		m_points.Sort(
			(a, b) => { return a.x.CompareTo( b.x ); }
		);
		
		m_isDirty = false;
	}
	
	private void _GetPairForPoint( float timePoint, out Vector2 left, out Vector2 right ) {
		left = Vector2.zero;
		right = Vector2.zero;
		
		var leftFound = false;
		var rightFound = false;
		
		foreach( var point in m_points ) {
			if( point.x < timePoint ) {
				leftFound = true;
				left = point;
				continue;
			}
			else {
				rightFound = true;
				right = point;
				break;
			}
		}
		
		if( !leftFound ) {
			left = new Vector2( timePoint, right.y );
		}
		if( !rightFound ) {
			right = new Vector2( timePoint, left.y );
		}
	}
#endregion
	
	
#region Temporary
#endregion
}
