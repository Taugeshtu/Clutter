using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using DRAW = Draw;	// solving name collisions

namespace Clutter.Mesh {

public enum PlaneSide {
	Front,
	Back,
	Intersecting
}

// This gonna be a TEMPORARY struct to get/set triangle data
public struct Triangle : IEnumerable<Vertex>, IEnumerable, IEquatable<Triangle> {
	private MorphMesh m_mesh;
	
	private Vertex m_cachedA;
	private Vertex m_cachedB;
	private Vertex m_cachedC;
	
	private int _indexIndex { get { return Index *3; } }
	private List<int> _indeces { get { return m_mesh.m_indeces; } }
	private int _indexA { get { return _indeces[_indexIndex + 0]; } }
	private int _indexB { get { return _indeces[_indexIndex + 1]; } }
	private int _indexC { get { return _indeces[_indexIndex + 2]; } }
	
	public long Generation;
	public int Index;
	
	public bool IsValid {
		get {
			return (Generation == m_mesh.m_generation);
		}
	}
	
	public bool IsDegenerate {
		get {
			var indexA = _indexA;
			var indexB = _indexB;
			var indexC = _indexC;
			if( (indexA != indexB) && (indexA != indexC) && (indexB != indexC) ) {
				return false;
			}
			return true;
		}
	}
	
	public Vertex A {
		get {
			if( !m_cachedA.IsValid ) { m_cachedA = m_mesh.GetVertex( _indexA ); }
			return m_cachedA;
		}
	}
	public Vertex B {
		get {
			if( !m_cachedB.IsValid ) { m_cachedB = m_mesh.GetVertex( _indexB ); }
			return m_cachedB;
		}
	}
	public Vertex C {
		get {
			if( !m_cachedC.IsValid ) { m_cachedC = m_mesh.GetVertex( _indexC ); }
			return m_cachedC;
		}
	}
	
	public Vertex this[int localVertexIndex] {
		get {
			localVertexIndex = localVertexIndex.Wrap( 3 );
			if( localVertexIndex == 0 ) { return A; }
			if( localVertexIndex == 1 ) { return B; }
			else { return C; }
		}
	}
	
	// I know these are a bit confusing. Basically, "vector from A to B"
	public Vector3 AB { get { return B.Position - A.Position; } }
	public Vector3 BC { get { return C.Position - B.Position; } }
	public Vector3 CA { get { return A.Position - C.Position; } }
	
	public Vector3 AC { get { return C.Position - A.Position; } }
	public Vector3 CB { get { return B.Position - C.Position; } }
	public Vector3 BA { get { return A.Position - B.Position; } }
	
	public IEnumerable<Edge> Edges {
		get {
			yield return new Edge( m_mesh, A, B );
			yield return new Edge( m_mesh, B, C );
			yield return new Edge( m_mesh, C, A );
		}
	}
	
	public Plane Plane {
		get { return new Plane( A.Position, B.Position, C.Position ); }
	}
	public Vector3 Normal {
		get {
			return AC.Cross( AB );
		}
	}
	public Vector3 Center {
		get {
			return (A.Position + B.Position + C.Position) /3f;
		}
	}
	
	// TODO: average vertex color, setting - all to the same color
	// public Color Color { get; set; }
	
	public IEnumerable<Triangle> VertexNeighbours {
		get {
			foreach( var vertex in this ) {
				foreach( var tris in vertex ) {
					if( tris.Index == Index ) { continue; }
					yield return tris;
				}
			}
		}
	}
	
	public IEnumerable<Triangle> EdgeNeighbours {
		get {
			foreach( var edge in Edges ) {
				foreach( var tris in edge.Triangles ) {
					if( tris.Index == Index ) { continue; }
					yield return tris;
				}
			}
		}
	}
	
#region Implementation
	internal Triangle( MorphMesh mesh, int ownID ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		Index = ownID;
		
		m_cachedA = new Vertex( mesh, MorphMesh.c_invalidID );
		m_cachedB = new Vertex( mesh, MorphMesh.c_invalidID );
		m_cachedC = new Vertex( mesh, MorphMesh.c_invalidID );
	}
	
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<Vertex> GetEnumerator() {
		yield return A;
		yield return B;
		yield return C;
	}
	
	public override bool Equals( object other ) {
		if( other is Triangle ) {
			return Equals( (Triangle) other );
		}
		return false;
	}
	public bool Equals( Triangle other ) {
		return (Generation == other.Generation) && (Index == other.Index) && (m_mesh == other.m_mesh);
	}
	public override int GetHashCode() {
		return (int) Generation + 23 *Index;
	}
	
	public static bool operator ==( Triangle a, Triangle b ) {
		return a.Equals( b );
	}
	public static bool operator !=( Triangle a, Triangle b ) {
		return !a.Equals( b );
	}
	
	public override string ToString() {
		return "T_"+Index+": ("+A.Index+","+B.Index+","+C.Index+") @"+Generation;
	}
#endregion
	
	
#region Public
	public void Flip() {
		var indexIndex = _indexIndex;
		
		var indexB = _indeces[indexIndex + 1];
		var indexC = _indeces[indexIndex + 2];
		
		_indeces[indexIndex + 1] = indexC;
		_indeces[indexIndex + 2] = indexB;
		
		var c = m_cachedC;
		m_cachedC = m_cachedB;
		m_cachedB = c;
	}
	
	public PlaneSide GetPlaneSide( Plane plane ) {
		var hasFront = false;
		var hasBack = false;
		
		foreach( var vertex in this ) {
			var isFront = plane.GetSide( vertex.Position );
			hasFront = hasFront || isFront;
			hasBack = hasBack || (!isFront);
		}
		
		if( hasFront ) {
			if( hasBack ) {
				return PlaneSide.Intersecting;
			}
			else {
				return PlaneSide.Front;
			}
		}
		if( hasBack ) {
			return PlaneSide.Back;
		}
		Log.Error( "This triangle is neither in front nor back, nor intersecting! WHAT THE HELL?!" );
		return PlaneSide.Back;
	}
	
	public void Draw( Color? color = null, float size = 1, float duration = 2 ) {
		foreach( var edge in Edges ) {
			edge.Draw( color, size, duration );
		}
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
