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
		Sin,
	}
	
	[Range( 0, 1 )] public float NeighbourBlendingBand = 0;
	[HideInInspector] public List<Vector2> Points = new List<Vector2>() { Vector2.zero, Vector2.one, };
	[HideInInspector] public List<SegmentType> Segments = new List<SegmentType>() { SegmentType.Linear };
	
	public virtual float this[float x] {
		get {
			if( Points.Count == 0 )
				return 0f;
			if( Points.Count == 1 )
				return Points[0].y;
			
			if( x <= 0 )
				return Points[0].y;
			else if( x >= 1 )
				return Points[Points.Count - 1].y;
			else {
				var place = _FindPlace( x );
				return _Evaluate( place, x, NeighbourBlendingBand );
			}
		}
	}
	
	public void AddPoint( float x ) {
		var place = _FindPlace( x );
		var y = _Evaluate( place, x, NeighbourBlendingBand );
		
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
	
	private float _Evaluate( int place, float x, float blendBand ) {
		var pointA = Points[place];
		var pointB = Points[place + 1];
		var segment = Segments[place];
		var factor = Mathf.InverseLerp( pointA.x, pointB.x, x );
		
		var a = pointA.y;
		var b = pointB.y;
		var result = 0f;
		if( segment == SegmentType.Linear )			result = Mathf.LerpUnclamped( a, b, factor );
		if( segment == SegmentType.ConstLeft )		result = a;
		if( segment == SegmentType.ConstRight )		result = b;
		if( segment == SegmentType.Quad )			result = Mathf.LerpUnclamped( a, b, _QuadFactor( a, b, factor ) );
		if( segment == SegmentType.InverseQuad )	result = Mathf.LerpUnclamped( a, b, _QuadFactor( b, a, factor ) );
		if( segment == SegmentType.Sin ) {
			var sinFactor = (Mathf.Sin( -Mathf.PI *0.5f + factor *Mathf.PI ) + 1) *0.5f;
			result = Mathf.LerpUnclamped( a, b, sinFactor );
		}
		
		if( blendBand == 0 )
			return result;
		
		var neighboutPlace = (factor < 0.5f) ? place - 1 : place + 1;
		if( neighboutPlace < 0 || neighboutPlace >= Segments.Count )
			return result;
		
		var blendFactor = 1f;
		var halfBand = blendBand *0.5f;
		if( factor < halfBand )		blendFactor = Mathf.Lerp( 0.5f, 1, factor /halfBand );
		if( factor > 1 - halfBand )	blendFactor = Mathf.Lerp( 0.5f, 1, (1 - factor) /halfBand );
		if( blendFactor == 1 )
			return result;
		
		// blendFactor = (blendFactor + 1) *0.5f;
		
		var neighbourResult = _Evaluate( neighboutPlace, x, 0 );
		return Mathf.SmoothStep( neighbourResult, result, blendFactor );
	}
	
	private float _QuadFactor( float a, float b, float f ) {
		if( a <= b )
			return f *f;
		else
			return 1 - (1 - f) *(1 - f);
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
