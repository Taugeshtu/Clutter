using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DesignCurve {
	public enum SegmentType {
		Linear,
		ConstLeft,
		ConstRight,
		// etc...
		Quad,
		InverseQuad,
	}
	
	public List<Vector2> Points = new List<Vector2>() { Vector2.zero, Vector2.one, };
	public List<SegmentType> Segments = new List<SegmentType>() { SegmentType.Linear };
	
	public float this[float x] {
		get {
			if( Points.Count == 0 )
				return 0f;
			if( Points.Count == 1 )
				return Points[0].y;
			
			if( x < 0 )
				return Points[0].y;
			else if( x > 1 )
				return Points[Points.Count - 1].y;
			else {
				var place = _FindPlace( x );
				return _Evaluate( place, x );
			}
		}
	}
	
	public void AddPoint( float x ) {
		var place = _FindPlace( x );
		var y = _Evaluate( place, x );
		
		Points.Insert( place + 1, new Vector2( x, y ) );
		Segments[place] = SegmentType.Linear;
		Segments.Insert( place + 1, SegmentType.Linear );
	}
	
	public void AddPoint( Vector2 p ) {
		var place = _FindPlace( p.x );
		
		Points.Insert( place + 1, p );
		Segments[place] = SegmentType.Linear;
		Segments.Insert( place + 1, SegmentType.Linear );
	}
	
	public void DeletePoint( int index ) {
		if( index <= 0 || index >= Points.Count - 1 )
			return;
		
		Points.RemoveAt( index );
		Segments.RemoveAt( index - 1 );
	}
	
	private int _FindPlace( float x ) {
		for( var i = 1; i < Points.Count; i++ ) {
			var point = Points[i];
			if( point.x > x )
				return i - 1;
		}
		return 0;
	}
	
	private float _Evaluate( int place, float x ) {
		var pointA = Points[place];
		var pointB = Points[place + 1];
		var segment = Segments[place];
		var factor = Mathf.InverseLerp( pointA.x, pointB.x, x );
		
		switch( segment ) {
			case SegmentType.Linear:
				return Mathf.Lerp( pointA.y, pointB.y, factor );
			case SegmentType.ConstLeft:
				return pointA.y;
			case SegmentType.ConstRight:
				return pointB.y;
			case SegmentType.Quad:
				return Mathf.Lerp( pointA.y, pointB.y, factor *factor );
			case SegmentType.InverseQuad:
				var invFactor = 1f - factor;
				return Mathf.Lerp( pointA.y, pointB.y, 1 - invFactor *invFactor );
		}
		return 0f;
	}
	
	public void Sanitize() {
		if( Points.Count == 0 )
			Points.Add( Vector2.zero );
		if( Points.Count == 1 )
			Points.Add( Vector2.one );
		if( Segments.Count == 0 )
			Segments.Add( SegmentType.Linear );
		
		var sizeDiff = Segments.Count - (Points.Count - 1);
		if( sizeDiff > 0 ) {
			Segments.RemoveRange( Segments.Count - sizeDiff, sizeDiff );
		}
	}
}
