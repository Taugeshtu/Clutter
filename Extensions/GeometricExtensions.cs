using UnityEngine;
using System.Collections.Generic;

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
#endregion
	
	
#region Pose
	public static void ApplyTo( this Pose pose, Transform target, Space space = Space.World ) {
		target.Apply( pose, space );
	}
	
	public static void Apply( this Transform target, Pose pose, Space space = Space.World ) {
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
}
