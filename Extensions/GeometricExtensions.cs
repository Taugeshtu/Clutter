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
	
	// Note: will return "to" in "from"s local space
	public static Pose InverseTransform( this Transform from, Pose to ) {
		return from.GetPose().InverseTransform( to );
	}
	public static Pose InverseTransform( this Pose from, Pose to ) {
		var invertedFromRotation = from.rotation.Inverted();
		var position = invertedFromRotation *( to.position - from.position );
		var rotation = invertedFromRotation *to.rotation;
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
}
