using UnityEngine;
using System.Collections.Generic;
using System;

public static class GeometricExtensions {
	
#region Plane
	public static bool GetSide( this Plane plane, Vector3 point, bool invert ) {
		if( invert ) {
			return !plane.GetSide( point );
		}
		else {
			return plane.GetSide( point );
		}
	}
	
	public static Vector3 Cast( this Plane plane, Ray ray ) {
		var castDistance = 0f;
		var castHit = plane.Raycast( ray, out castDistance );
		if( castHit ) {
			return ray.GetPoint( castDistance );
		}
		else {
			return Vector.Invalid3;
		}
	}
	
	public static bool IsValid( this Plane plane ) {
		return plane.normal.IsValid() && !float.IsNaN( plane.distance );
	}
	
	public static Plane BestFittingPlane( this IEnumerable<Vector3> points ) {
		var average = Vector3.zero;
		var pointsCount = 0;
		foreach( var point in points ) {
			average += point;
			pointsCount += 1;
		}
		
		if( pointsCount < 3 )
			throw new System.ArgumentOutOfRangeException( "points", pointsCount, "Must be at least 3 points in the list or more" );
		
		average /= pointsCount;
		
		// Calculate the covariance matrix
		float xx = 0f, xy = 0f, xz = 0f, yy = 0f, yz = 0f, zz = 0f;
		foreach( var point in points ) {
			var diff = point - average;
			xx += diff.x * diff.x;
			xy += diff.x * diff.y;
			xz += diff.x * diff.z;
			yy += diff.y * diff.y;
			yz += diff.y * diff.z;
			zz += diff.z * diff.z;
		}
		
		float detX = yy * zz - yz * yz;
		float detY = xx * zz - xz * xz;
		float detZ = xx * yy - xy * xy;
		
		// Find the normal with the largest determinant
		Vector3 normal;
		if( detX > detY && detX > detZ )
			normal = new Vector3( detX, xz * yz - xy * zz, xy * yz - xz * yy );
		else if( detY > detZ )
			normal = new Vector3( xz * yz - xy * zz, detY, xy * xz - yz * xx );
		else
			normal = new Vector3( xy * yz - xz * yy, xy * xz - yz * xx, detZ );
		normal.Normalize();
		
		return new Plane( normal, average );
	}
	
	// This will give height along "cutPlaneNormal" that will slice the rectangle defined by frame, size such that
	// area UNDER the Plane( cutPlaneNormal, cutPlaneNormal *height ) is areaPortion of rect's total area
	public static float AreaFitCut( this (Pose frame, Vector2 size) rect, Vector3 cutPlaneNormal, float areaPortion ) {
		var tangent = Vector3.forward;
		Vector3.OrthoNormalize( ref cutPlaneNormal, ref tangent );
		var cutPlaneFrame = new Pose( Vector3.zero, Quaternion.LookRotation( tangent, cutPlaneNormal ) );
		rect.frame = cutPlaneFrame.InverseTransform( rect.frame );
		var cutFramePlaneHeight = AreaFitCutHorizontal( rect, areaPortion );
		return cutFramePlaneHeight;
	}
	// See "AreaFitCut", except here the plane is assumed horizontal
	public static float AreaFitCutHorizontal( this (Pose frame, Vector2 size) rect, float areaPortion ) {
		var halfSize = rect.size /2;
		var A = rect.frame.Transform( new Vector3( halfSize.x, 0, halfSize.y ) );
		var B = rect.frame.Transform( new Vector3(-halfSize.x, 0, halfSize.y ) );
		var C = rect.frame.Transform( new Vector3(-halfSize.x, 0,-halfSize.y ) );
		var D = rect.frame.Transform( new Vector3( halfSize.x, 0,-halfSize.y ) );
		var deviation = Mathf.Abs( A.y - B.y ) + Mathf.Abs( A.y - C.y ) + Mathf.Abs( A.y - D.y );
		if( deviation.EpsilonEquals( 0f ) )
			return A.y;
		
		var totalArea = rect.size.x *rect.size.y;
		var areaWanted = totalArea *areaPortion;
		
		var corners = new Vector3[] { A, B, C, D };
		System.Array.Sort( corners, (a, b) => a.y.CompareTo( b.y ) );
		var high = corners[3];
		var midHigh = corners[2];
		var midLow = corners[1];
		var low = corners[0];
		
		var factorMidHigh = Mathf.InverseLerp( high.y, midLow.y, midHigh.y );
		var xMidHigh = Vector3.Lerp( high, midLow, factorMidHigh );
		
		var factorMidLow = Mathf.InverseLerp( low.y, midHigh.y, midLow.y );
		var xMidLow = Vector3.Lerp( low, midHigh, factorMidLow );
		
		var areaTopTriangle = Vector3.Cross( (high - midHigh), (xMidHigh - midHigh) ).magnitude /2;
		var areaMid = Vector3.Cross( (midHigh - xMidHigh), (midLow - xMidHigh) ).magnitude;
		var areaBottomTriangle = areaTopTriangle;
		
		if( areaWanted < areaBottomTriangle ) {
			var factor = Mathf.Sqrt( areaWanted /areaBottomTriangle );
			return Mathf.Lerp( low.y, midLow.y, factor );
		}
		else if( areaWanted < (areaBottomTriangle + areaMid) ) {
			var wantedAreaInMid = areaWanted - areaBottomTriangle;
			var factor = wantedAreaInMid /areaMid;
			return Mathf.Lerp( midLow.y, midHigh.y, factor );
		}
		else if( areaWanted < totalArea ) {
			var missingAreaInHigh = totalArea - areaWanted;
			var factor = Mathf.Sqrt( missingAreaInHigh /areaTopTriangle );
			return Mathf.Lerp( high.y, midHigh.y, factor );
		}
		else {
			return high.y;
		}
	}
	
	public static float CutArea( this (Pose frame, Vector2 size) rect, Plane plane ) {
		var tangent = Vector3.forward;
		var cutPlaneNormal = plane.normal;
		Vector3.OrthoNormalize( ref cutPlaneNormal, ref tangent );
		var cutPlaneFrame = new Pose( Vector3.zero, Quaternion.LookRotation( tangent, cutPlaneNormal ) );
		rect.frame = cutPlaneFrame.InverseTransform( rect.frame );
		var cutFramePlaneHeight = CutAreaHorizontal( rect, -plane.distance );
		return cutFramePlaneHeight;
	}
	public static float CutAreaHorizontal( this (Pose frame, Vector2 size) rect, float planeHeight ) {
		var totalArea = rect.size.x *rect.size.y;
		var halfSize = rect.size /2;
		var A = rect.frame.Transform( new Vector3( halfSize.x, 0, halfSize.y ) );
		var B = rect.frame.Transform( new Vector3(-halfSize.x, 0, halfSize.y ) );
		var C = rect.frame.Transform( new Vector3(-halfSize.x, 0,-halfSize.y ) );
		var D = rect.frame.Transform( new Vector3( halfSize.x, 0,-halfSize.y ) );
		
		var corners = new Vector3[] { A, B, C, D };
		System.Array.Sort( corners, (a, b) => a.y.CompareTo( b.y ) );
		var high = corners[3];
		var midHigh = corners[2];
		var midLow = corners[1];
		var low = corners[0];
		
		var factorMidHigh = Mathf.InverseLerp( high.y, midLow.y, midHigh.y );
		var xMidHigh = Vector3.Lerp( high, midLow, factorMidHigh );
		
		var factorMidLow = Mathf.InverseLerp( low.y, midHigh.y, midLow.y );
		var xMidLow = Vector3.Lerp( low, midHigh, factorMidLow );
		
		var areaTopTriangle = Vector3.Cross( (high - midHigh), (xMidHigh - midHigh) ).magnitude /2;
		var areaMid = Vector3.Cross( (midHigh - xMidHigh), (midLow - xMidHigh) ).magnitude;
		var areaBottomTriangle = areaTopTriangle;
		
		if( planeHeight < low.y )
			return 0;
		else if( planeHeight < midLow.y ) {
			var factor = Mathf.InverseLerp( low.y, midLow.y, planeHeight );
			var a = low;
			var b = Vector3.Lerp( low, midLow, factor );
			var c = Vector3.Lerp( low, xMidLow, factor );
			return Vector3.Cross( (b - a), (c - a) ).magnitude /2;
		}
		else if( planeHeight < midHigh.y ) {
			var factor = Mathf.InverseLerp( midLow.y, midHigh.y, planeHeight );
			return areaBottomTriangle + areaMid *factor;
		}
		else if( planeHeight < high.y ) {
			var factor = Mathf.InverseLerp( high.y, midHigh.y, planeHeight );
			var a = high;
			var b = Vector3.Lerp( high, midHigh, factor );
			var c = Vector3.Lerp( high, xMidHigh, factor );
			return totalArea - Vector3.Cross( (b - a), (c - a) ).magnitude /2;
		}
		else {
			return totalArea;
		}
	}
	
	public static float SubmersionFactor( this (Vector3 a, Vector3 b) edge, Plane plane ) {
		var aAbovePlane = plane.GetSide( edge.a );
		var bAbovePlane = plane.GetSide( edge.b );
		if( aAbovePlane == bAbovePlane ) {
			return aAbovePlane ? 0 : 1;
		}
		else {
			var projectedAbove = aAbovePlane ? edge.a.ProjectedOn( plane.normal ) : edge.b.ProjectedOn( plane.normal );
			var projectedBelow = aAbovePlane ? edge.b.ProjectedOn( plane.normal ) : edge.a.ProjectedOn( plane.normal );
			return Mathf.InverseLerp( projectedBelow.magnitude, projectedAbove.magnitude, -plane.distance );
		}
	}
#endregion
	
	
#region Pose
	public static void SetPose( this Transform target, Pose pose, Space space = Space.World ) {
		if( space == Space.World ) {
			target.position = pose.position;
			target.rotation = pose.rotation;
		}
		else {
			target.localPosition = pose.position;
			target.localRotation = pose.rotation;
		}
	}
	
	public static Pose GetPose( this Transform source, Space space = Space.World ) {
		if( space == Space.World ) {
			return new Pose( source.position, source.rotation );
		}
		else {
			return new Pose( source.localPosition, source.localRotation );
		}
	}
	
	public static Pose WithPosition( this Pose pose, Vector3 newPosition ) {
		return new Pose( newPosition, pose.rotation );
	}
	public static Pose WithRotation( this Pose pose, Quaternion newRotation ) {
		return new Pose( pose.position, newRotation );
	}
	
	// Transforming
	public static Vector3 Transform( this Pose pose, Vector3 localPosition ) {
		return (pose.rotation *localPosition) + pose.position;
	}
	
	public static Vector3 InverseTransform( this Pose pose, Vector3 worldPosition ) {
		return pose.rotation.Inverted() *(worldPosition - pose.position);
	}
	
	public static Vector3 TransformDirection( this Pose pose, Vector3 localDirection ) {
		return pose.rotation *localDirection;
	}
	
	public static Vector3 InverseTransformDirection( this Pose pose, Vector3 worldDirection ) {
		return pose.rotation.Inverted() *worldDirection;
	}
	
	public static Pose Transform( this Transform transform, Pose localPose ) {
		return transform.GetPose().Transform( localPose );
	}
	
	public static Pose Transform( this Pose ownPose, Pose localPose ) {
		var position = ownPose.Transform( localPose.position );
		var rotation = ownPose.rotation *localPose.rotation;
		return new Pose( position, rotation );
	}
	
	// Note: will return "target" in "from"s local space
	public static Pose InverseTransform( this Transform from, Pose target ) {
		return from.GetPose().InverseTransform( target );
	}
	public static Pose InverseTransform( this Pose from, Pose target ) {
		var invertedFromRotation = from.rotation.Inverted();
		var position = invertedFromRotation *( target.position - from.position );
		var rotation = invertedFromRotation *target.rotation;
		return new Pose( position, rotation );
	}
	
	// Lerps
	public static Pose Lerp( this Pose from, Pose to, float factor ) {
		var position = Vector3.Lerp( from.position, to.position, factor );
		var rotation = Quaternion.Lerp( from.rotation, to.rotation, factor );
		return new Pose( position, rotation );
	}
	
	public static Pose LerpUnclamped( this Pose from, Pose to, float factor ) {
		var position = Vector3.LerpUnclamped( from.position, to.position, factor );
		var rotation = Quaternion.LerpUnclamped( from.rotation, to.rotation, factor );
		return new Pose( position, rotation );
	}
	
	public static Pose Slerp( this Pose from, Pose to, float factor ) {
		var position = Vector3.Slerp( from.position, to.position, factor );
		var rotation = Quaternion.Slerp( from.rotation, to.rotation, factor );
		return new Pose( position, rotation );
	}
	
	public static Pose SlerpUnclamped( this Pose from, Pose to, float factor ) {
		var position = Vector3.SlerpUnclamped( from.position, to.position, factor );
		var rotation = Quaternion.SlerpUnclamped( from.rotation, to.rotation, factor );
		return new Pose( position, rotation );
	}
#endregion
	
	
#region Bounds
	public static Bounds Sanitized( this Bounds a ) {
		var min = Vector.Min( a.min, a.max );
		var max = Vector.Max( a.min, a.max );
		
		var center = (min + max) /2;
		var size = max - min;
		return new Bounds( center, size );
	}
	
	public static Vector3 ClosestPoint( this Bounds a, Bounds b ) {	// returns a point within bounds "a" that's closest to bounds "b"
		var closestX = 0f;
		if( a.max.x < b.min.x ) {
			closestX = a.max.x;
		}
		else {
			closestX = Mathf.Max( a.min.x, b.min.x );
		}
		
		var closestY = 0f;
		if( a.max.y < b.min.y ) {
			closestY = a.max.y;
		}
		else {
			closestY = Mathf.Max( a.min.y, b.min.y );
		}
		
		var closestZ = 0f;
		if( a.max.z < b.min.z ) {
			closestZ = a.max.z;
		}
		else {
			closestZ = Mathf.Max( a.min.z, b.min.z );
		}
		
		return new Vector3( closestX, closestY, closestZ );
	}
	
	public static float Distance( this Bounds a, Vector3 point ) {
		var squaredDistance = a.SqrDistance( point );
		return Mathf.Sqrt( squaredDistance );
	}
	
	public static float Distance( this Bounds a, Bounds b ) {
		var closestPointInB = b.ClosestPoint( a );
		return a.Distance( closestPointInB );
	}
#endregion
	
	
#region Quaternion
	public static Quaternion Inverted( this Quaternion rotation ) {
		return Quaternion.Inverse( rotation );
	}
#endregion


#region Misc
	public static Vector3Int HilbertToXYZ( int index, int order ) {
		var mask = (1 << order) - 1;
		var x = 0;
		var y = 0;
		var z = 0;
		
		for( var s = 1; s < (1 << order); s <<= 1 ) {
			var rx = 1 & (index >> 2);
			var ry = 1 & (index >> 1);
			var rz = 1 & index;
			
			var t = rx ^ rz;
			x ^= (~ry & mask) & (t ^ -ry);
			y ^= ry & (t ^ rx);
			z ^= rz & (t ^ rx);
			
			index >>= 3;
		}
		
		return new Vector3Int( x, y, z );
	}
	
	public static Rect GetScreenSpaceRect( this RectTransform rectTransform ) {
		var size = Vector2.Scale( rectTransform.rect.size, rectTransform.lossyScale );
		var rect = new Rect( rectTransform.position.x, rectTransform.position.y, size.x, size.y );
		rect.x -= (rectTransform.pivot.x *size.x);
		rect.y -= ((1.0f - rectTransform.pivot.y) *size.y);
		return rect;
	}
	
	public static ulong MortonEncoded( this Vector3Int position ) {
		UInt64 _splitBy3( UInt32 a ) {
			UInt64 x = a & 0x1fffff; // we only look at the first 21 bits
			x = (x | x << 32) & 0x1f00000000ffff; // shift left 32 bits, OR with self, and 00011111000000000000000000000000000000001111111111111111
			x = (x | x << 16) & 0x1f0000ff0000ff; // shift left 32 bits, OR with self, and 00011111000000000000000011111111000000000000000011111111
			x = (x | x << 8) & 0x100f00f00f00f00f; // shift left 32 bits, OR with self, and 0001000000001111000000001111000000001111000000001111000000000000
			x = (x | x << 4) & 0x10c30c30c30c30c3; // shift left 32 bits, OR with self, and 0001000011000011000011000011000011000011000011000011000100000000
			x = (x | x << 2) & 0x1249249249249249;
			return x;
		}
		
		return _splitBy3( (UInt32) position.x ) | _splitBy3( (UInt32) position.y ) << 1 | _splitBy3( (UInt32) position.z ) << 2;
	}
#endregion
}
