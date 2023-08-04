using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using float2 = UnityEngine.Vector2;
using float3 = UnityEngine.Vector3;

public static class Springy {
	public struct Coeffs : System.IEquatable<Coeffs> {
		public float k1, k2, k3, dtCritical;
		
		public Coeffs( float ownFreq, float dampFactor, float response ) {
			var piF2 = 2 *Mathf.PI *ownFreq;
			k1 = dampFactor /(Mathf.PI *ownFreq);
			k2 = 1 /(piF2 * piF2);
			k3 = response *dampFactor /piF2;
			dtCritical = 0.8f *(Mathf.Sqrt( 4 *k2 + k1 *k1 ) - k1);
		}
		
		public bool Equals( Coeffs other ) {
			return
				(k1 == other.k1)
			 && (k2 == other.k2)
			 && (k3 == other.k3)
			 && (dtCritical == other.dtCritical);
		}
		
		public override bool Equals( object obj ) {
			if( obj is Coeffs )
				return Equals( (Coeffs) obj );
			return false;
		}
		
		public override int GetHashCode() {
			unchecked {
				var hash = 17;
				hash = hash * 23 + k1.GetHashCode();
				hash = hash * 23 + k2.GetHashCode();
				hash = hash * 23 + k3.GetHashCode();
				hash = hash * 23 + dtCritical.GetHashCode();
				return hash;
			}
		}
		
		public static bool operator ==( Coeffs a, Coeffs b ) {
			return a.Equals( b );
		}
		
		public static bool operator !=( Coeffs a, Coeffs b ) {
			return !a.Equals( b );
		}
	}
	
	
	// Float
	public struct Float {
		public float p, v;
		
		private float prevX;
		public Coeffs ks;
		
		public Float( float ownFreq, float dampFactor, float response, float startX ) : this( new Coeffs( ownFreq, dampFactor, response ), startX ) {}
		public Float( Coeffs ks, float startX ) {
			p = startX;
			v = 0;
			prevX = startX;
			this.ks = ks;
		}
		public float Tick( float x, float dt ) {
			var dx = (x - prevX) /dt;
			return Tick( x, dx, dt );
		}
		public float Tick( float x, float dx, float dt ) {
			return x.Springy( dx, ref p, ref v, dt, ks );
		}
	}
	
	
	// Vector2
	public struct Vector2 {
		public float2 p, v;
		
		private float2 prevX;
		public Coeffs ks;
		
		public Vector2( float ownFreq, float dampFactor, float response, float2 startX ) : this( new Coeffs( ownFreq, dampFactor, response ), startX ) {}
		public Vector2( Coeffs ks, float2 startX ) {
			p = startX;
			v = float2.zero;
			prevX = startX;
			this.ks = ks;
		}
		public float2 Tick( float2 x, float dt ) {
			var dx = (x - prevX) /dt;
			return Tick( x, dx, dt );
		}
		public float2 Tick( float2 x, float2 dx, float dt ) {
			return x.Springy( dx, ref p, ref v, dt, ks );
		}
	}
	
	
	// Vector3
	public struct Vector3 {
		public float3 p, v;
		
		private float3 prevX;
		public Coeffs ks;
		
		public Vector3( float ownFreq, float dampFactor, float response, float3 startX ) : this( new Coeffs( ownFreq, dampFactor, response ), startX ) {}
		public Vector3( Coeffs ks, float3 startX ) {
			p = startX;
			v = float3.zero;
			prevX = startX;
			this.ks = ks;
		}
		public float3 Tick( float3 x, float dt ) {
			var dx = (x - prevX) /dt;
			return Tick( x, dx, dt );
		}
		public float3 Tick( float3 x, float3 dx, float dt ) {
			return x.Springy( dx, ref p, ref v, dt, ks );
		}
	}
}

public static class StringyExt {
	public static float Springy( this float x, float dx, ref float y, ref float dy, float dt, float freq, float damp, float response ) {
		var ks = new Springy.Coeffs( freq, damp, response );
		return x.Springy( dx, ref y, ref dy, dt, ks );
	}
	public static float2 Springy( this float2 x, float2 dx, ref float2 y, ref float2 dy, float dt, float freq, float damp, float response ) {
		var ks = new Springy.Coeffs( freq, damp, response );
		return x.Springy( dx, ref y, ref dy, dt, ks );
	}
	public static float3 Springy( this float3 x, float3 dx, ref float3 y, ref float3 dy, float dt, float freq, float damp, float response ) {
		var ks = new Springy.Coeffs( freq, damp, response );
		return x.Springy( dx, ref y, ref dy, dt, ks );
	}
	
	// Meat & potatoes. These methods must be IDENTICAL
	public static float Springy( this float x, float dx, ref float y, ref float dy, float dt, Springy.Coeffs ks ) {
		var iterations = Mathf.CeilToInt( dt /ks.dtCritical );
		dt = dt /iterations;
		for( var i = 0; i < iterations; i++ ) {
			y += dy *dt;
			dy += ((x +dx*ks.k3 -y -dy*ks.k1) /ks.k2) *dt;
		}
		return y;
	}
	public static float2 Springy( this float2 x, float2 dx, ref float2 y, ref float2 dy, float dt, Springy.Coeffs ks ) {
		var iterations = Mathf.CeilToInt( dt /ks.dtCritical );
		dt = dt /iterations;
		for( var i = 0; i < iterations; i++ ) {
			y += dy *dt;
			dy += ((x +dx*ks.k3 -y -dy*ks.k1) /ks.k2) *dt;
		}
		return y;
	}
	public static float3 Springy( this float3 x, float3 dx, ref float3 y, ref float3 dy, float dt, Springy.Coeffs ks ) {
		var iterations = Mathf.CeilToInt( dt /ks.dtCritical );
		dt = dt /iterations;
		for( var i = 0; i < iterations; i++ ) {
			y += dy *dt;
			dy += ((x +dx*ks.k3 -y -dy*ks.k1) /ks.k2) *dt;
		}
		return y;
	}
}
