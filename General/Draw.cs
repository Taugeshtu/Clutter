﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Draw {
	private const float c_crossSize = 0.5f;
	private const float c_arrowSize = 0.1f;
	
	private static Color s_color = Color.magenta;
	
#region Line
	public static void Line( Vector3 from, Vector3 to, Color? color = null, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		Debug.DrawLine( from, to, drawColor, duration );
	}
#endregion
	
	
#region Arrow
	public static void Arrow( Ray ray, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		Arrow( ray.origin, ray.direction, color, size, duration );
	}
	public static void Arrow( Vector3 origin, Vector3 direction, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		var tangent = Vector3.right;
		var binormal = Vector3.forward;
		Vector3.OrthoNormalize( ref direction, ref tangent, ref binormal );
		
		var points = new Vector3[] {
			origin - direction *size + tangent *size /3,
			origin - direction *size - tangent *size /3,
			origin - direction *size + binormal *size /3,
			origin - direction *size - binormal *size /3
		};
		
		var drawColor = (color.HasValue) ? color.Value : s_color;
		foreach( var point in points ) {
			Debug.DrawRay( origin, point - origin, drawColor, duration );
		}
	}
#endregion
	
	
#region Ray
	public static void Ray( Ray ray, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		Ray( ray.origin, ray.direction, color, size, duration );
	}
	public static void Ray( Vector3 origin, Vector3 direction, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		Debug.DrawLine( origin, origin + direction, drawColor, duration );
		Arrow( origin + direction, direction, drawColor, size, duration );
	}
	
	public static void RayFromTo( Vector3 from, Vector3 to, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		Debug.DrawLine( from, to, drawColor, duration );
		Arrow( to, to - from, drawColor, size, duration );
	}
	
	public static void RayCross( Ray ray, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		RayCross( ray.origin, ray.direction, color, size, duration );
	}
	public static void RayCross( Vector3 origin, Vector3 direction, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		Ray( origin, direction, color, size, duration );
		Cross( origin, color, size, duration );
	}
	
	public static void RayFromToCross( Vector3 from, Vector3 to, Color? color = null, float size = c_arrowSize, float duration = 0f ) {
		RayFromTo( from, to, color, size, duration );
		Cross( from, color, size, duration );
	}
#endregion
	
	
#region Cross
	public static void Cross( Vector3 position, Color? color = null, float size = c_crossSize, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		Debug.DrawRay( position - Vector3.up *size, Vector3.up *size *2, drawColor, duration );
		Debug.DrawRay( position - Vector3.right *size, Vector3.right *size *2, drawColor, duration );
		Debug.DrawRay( position - Vector3.forward *size, Vector3.forward *size *2, drawColor, duration );
	}
#endregion
	
	
#region Rect
	public static void Rect( Rect rect, Color? color = null, float depth = 0f, float duration = 0f ) {
		var corner = new Vector3( rect.xMin, rect.yMin, depth );
		var up = Vector3.up *rect.height;
		var right = Vector3.right *rect.width;
		var forward = Vector3.zero;
		Cube( corner, up, right, forward, color, duration );
	}
#endregion
	
	
#region Cube
	public static void CubeAA( Vector3 center, Color? color = null, float size = c_crossSize, float duration = 0f ) {
		var corner = center - (Vector3.up + Vector3.forward + Vector3.right) *0.5f *size;
		Cube( corner, Vector3.up *size, Vector3.right *size, Vector3.forward *size, color, duration );
	}
	
	public static void CubeCentered( Vector3 center, Vector3 up, Vector3 right, Vector3 forward, Color? color = null, float duration = 0f ) {
		var corner = center - up - right - forward;
		Cube( corner, up *2, right *2, forward *2, color, duration );
	}
	
	public static void Cube( Vector3 corner, Vector3 up, Vector3 right, Vector3 forward, Color? color = null, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		Debug.DrawRay( corner, up, drawColor, duration );
		Debug.DrawRay( corner, right, drawColor, duration );
		Debug.DrawRay( corner, forward, drawColor, duration );
		
		Debug.DrawRay( corner + up, right, drawColor, duration );
		Debug.DrawRay( corner + up, forward, drawColor, duration );
		
		Debug.DrawRay( corner + right, up, drawColor, duration );
		Debug.DrawRay( corner + right, forward, drawColor, duration );
		
		Debug.DrawRay( corner + forward, up, drawColor, duration );
		Debug.DrawRay( corner + forward, right, drawColor, duration );
		
		var oppositeCorner = corner + up + right + forward;
		Debug.DrawRay( oppositeCorner, -up, drawColor, duration );
		Debug.DrawRay( oppositeCorner, -right, drawColor, duration );
		Debug.DrawRay( oppositeCorner, -forward, drawColor, duration );
	}
#endregion
	
	
#region Diamond
	public static void Diamond( Vector3 position, Color? color = null, float size = c_crossSize, float duration = 0f ) {
		Diamond( position, Quaternion.identity, color, size, duration );
	}
	public static void Diamond( Vector3 position, Quaternion rotation, Color? color = null, float size = c_crossSize, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		var up = position + (rotation *Vector3.up) *size;
		var down = position + (rotation *Vector3.down) *size;;
		var corners = new Vector3[4] {
			position + (rotation *Vector3.forward) *size,
			position + (rotation *Vector3.right) *size,
			position + (rotation *Vector3.back) *size,
			position + (rotation *Vector3.left) *size
		};
		
		for( var i = 0; i < 4; i++ ) {
			Debug.DrawLine( corners[i], up, drawColor, duration );
			Debug.DrawLine( corners[i], down, drawColor, duration );
			
			var otherIndex = (i + 1) %4;
			Debug.DrawLine( corners[i], corners[otherIndex], drawColor, duration );
		}
	}
#endregion
	
	
#region Circle
	public static void Circle( Vector3 center, Vector3 forward, Vector3 right, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, forward, right, new Vector2( forward.magnitude, right.magnitude ), color, segments, duration );
	}
	public static void Circle( Vector3 center, Vector3 forward, Vector3 right, float size, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, forward, right, Vector2.one *size, color, segments, duration );
	}
	public static void Circle( Vector3 center, Vector3 forward, Vector3 right, Vector2 size, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, Quaternion.LookRotation( forward, right ) *Quaternion.AngleAxis( 90f, Vector3.forward ), size, color, segments, duration );
	}
	
	public static void Circle( Vector3 center, Vector3 normal, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, normal, Vector2.one, color, segments, duration );
	}
	public static void Circle( Vector3 center, Vector3 normal, float size, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, normal, Vector2.one *size, color, segments, duration );
	}
	public static void Circle( Vector3 center, Vector3 normal, Vector2 size, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, Quaternion.FromToRotation( Vector3.up, normal ), size, color, segments, duration );
	}
	
	public static void Circle( Vector3 center, Quaternion orientation, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, orientation, Vector2.one, color, segments, duration );
	}
	public static void Circle( Vector3 center, Quaternion orientation, float size, Color? color = null, int segments = 24, float duration = 0f ) {
		Circle( center, orientation, Vector2.one *size, color, segments, duration );
	}
	public static void Circle( Vector3 center, Quaternion orientation, Vector2 size, Color? color = null, int segments = 24, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		segments = Mathf.Max( segments, 3 );
		var angle = 360f /segments;
		for( var i = 0; i < segments; i++ ) {
			var a = Quaternion.AngleAxis( angle *i, Vector3.up ) *Vector3.forward;
			var b = Quaternion.AngleAxis( angle *(i + 1), Vector3.up ) *Vector3.forward;
			a.Scale( size.X0Y() );
			b.Scale( size.X0Y() );
			
			a = center + orientation *a;
			b = center + orientation *b;
			
			Debug.DrawLine( a, b, drawColor, duration );
		}
	}
#endregion
	
	
#region Sphere
	public static void Sphere( Vector3 center, Color? color = null, float size = c_crossSize, float duration = 0f ) {
		Circle( center, Quaternion.identity, Vector2.one *size, color, 24, duration );
		Circle( center, Quaternion.AngleAxis( 90f, Vector3.right ), Vector2.one *size, color, 24, duration );
		Circle( center, Quaternion.AngleAxis( 90f, Vector3.forward ), Vector2.one *size, color, 24, duration );
	}
#endregion
	
	
#region Cone
	public static void Cone( Vector3 start, Quaternion orientation, float height, Vector2 baseSize, Vector2 topSize, Color? color = null, int ribs = 4, float duration = 0f ) {
		var drawColor = (color.HasValue) ? color.Value : s_color;
		
		ribs = Mathf.Max( ribs, 1 );
		var segments = ribs *4;
		var upshift = orientation *Vector3.up *height;
		
		Circle( start, orientation, baseSize, drawColor, segments, duration );
		Circle( start + upshift, orientation, topSize, drawColor, segments, duration );
		
		var angle = 360f /ribs;
		for( var i = 0; i < ribs; i++ ) {
			var a = Quaternion.AngleAxis( angle *i, Vector3.up ) *Vector3.forward;
			var b = Quaternion.AngleAxis( angle *i, Vector3.up ) *Vector3.forward;
			a.Scale( baseSize.X0Y() );
			b.Scale( topSize.X0Y() );
			
			a = start + orientation *a;
			b = start + upshift + orientation *b;
			
			Debug.DrawLine( a, b, drawColor, duration );
		}
	}
#endregion
	
	
#region Cast
	public static void SphereCast( Ray ray, float radius, float distance, Color? color = null, float duration = 0f ) {
		var p1 = ray.origin;
		var p2 = p1 + ray.direction.normalized *distance;
		Ray( ray.origin, ray.direction *(distance - radius), color, radius, duration );
		Sphere( p1, color, radius, duration );
		Sphere( p2, color, radius, duration );
	}
#endregion
}