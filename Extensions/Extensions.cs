using UnityEngine;
using System.Collections.Generic;

public static class Extensions {
	
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
	
	public static void TrimEnd( this System.Text.StringBuilder builder, int count ) {
		var removeStart = builder.Length - count;
		builder.Remove( removeStart, count );
	}
	
#region Temporary
#endregion
}
