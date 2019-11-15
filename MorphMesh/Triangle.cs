﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DRAW = Draw;	// solving name collisions

namespace Clutter.Mesh {
// This gonna be a TEMPORARY struct to get/set triangle data
public struct Triangle : IEnumerable<Vertex>, IEnumerable {
	private MorphMesh m_mesh;
	
	private int _indexIndex { get { return Index *3; } }
	private List<int> _indeces { get { return m_mesh.m_indeces; } }
	
	public bool IsValid {
		get {
			return (Generation == m_mesh.m_generation);
		}
	}
	
	private Vertex m_cachedA;
	private Vertex m_cachedB;
	private Vertex m_cachedC;
	
	public long Generation;
	public int Index;
	
	public Vertex A {
		get {
			if( !m_cachedA.IsValid ) { m_cachedA = m_mesh.GetVertex( _indeces[_indexIndex + 0] ); }
			return m_cachedA;
		}
	}
	public Vertex B {
		get {
			if( !m_cachedB.IsValid ) { m_cachedB = m_mesh.GetVertex( _indeces[_indexIndex + 1] ); }
			return m_cachedB;
		}
	}
	public Vertex C {
		get {
			if( !m_cachedC.IsValid ) { m_cachedC = m_mesh.GetVertex( _indeces[_indexIndex + 2] ); }
			return m_cachedC;
		}
	}
	
	// I know these are a bit confusing. Basically, "vector from A to B"
	public Vector3 AB { get { return B.Position - A.Position; } }
	public Vector3 BC { get { return C.Position - B.Position; } }
	public Vector3 CA { get { return A.Position - C.Position; } }
	
	public Vector3 AC { get { return C.Position - A.Position; } }
	public Vector3 CB { get { return B.Position - C.Position; } }
	public Vector3 BA { get { return A.Position - B.Position; } }
	
	public IEnumerable<Vertex[]> Edges {
		get {
			yield return new Vertex[] { A, B };
			yield return new Vertex[] { B, C };
			yield return new Vertex[] { C, A };
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
#endregion
	
	
#region Public
	public void SetVertices( ref Vertex a, ref Vertex b, ref Vertex c ) {
		var indexIndex = _indexIndex;
		
		var indexA = _indeces[indexIndex + 0];
		var indexB = _indeces[indexIndex + 1];
		var indexC = _indeces[indexIndex + 2];
		
		_indeces[indexIndex + 0] = a.Index;
		_indeces[indexIndex + 1] = b.Index;
		_indeces[indexIndex + 2] = c.Index;
		
		var oldSet = new HashSet<int>() { indexA, indexB, indexC };
		var newSet = new HashSet<int>() { a.Index, b.Index, c.Index };
		
		oldSet.Remove( MorphMesh.c_invalidID, a.Index, b.Index, c.Index );
		foreach( var oldVertexIndex in oldSet ) {
			m_mesh.GetVertex( oldVertexIndex ).m_ownership.RemoveOwner( Index );
		}
		
		newSet.Remove( indexA, indexB, indexC );
		foreach( var newVertexIndex in newSet ) {
			m_mesh.GetVertex( newVertexIndex ).m_ownership.AddOwner( Index );
		}
		
		Debug.LogError( "Setting verts. Old: "+oldSet.Dump()+"; New: "+newSet.Dump() );
		
		m_cachedA = a;
		m_cachedB = b;
		m_cachedC = c;
	}
	
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
	
	public List<Triangle> GetVertexNeighbours() {
		var result = new List<Triangle>();
		
		foreach( var vertex in this ) {
			foreach( var tris in vertex ) {
				if( tris.Index == Index ) { continue; }
				
				result.Add( tris );
			}
		}
		
		return result;
	}
	
	public List<Triangle> GetEdgeNeighbours() {
		var result = new List<Triangle>();
		
		var setA = new HashSet<int>();
		var setB = new HashSet<int>();
		
		foreach( var edge in Edges ) {
			setA.Clear();
			foreach( var ownerID in edge[0].m_ownership ) {
				if( ownerID != Index ) {
					setA.Add( ownerID );
				}
			}
			
			setB.Clear();
			foreach( var ownerID in edge[1].m_ownership ) {
				if( ownerID != Index ) {
					setB.Add( ownerID );
				}
			}
			
			setA.IntersectWith( setB );
			foreach( var trisID in setA ) {
				result.Add( m_mesh.GetTriangle( trisID ) );
			}
		}
		
		return result;
	}
	
	public void Draw( Color? color = null, float size = 1, float duration = 2 ) {
		DRAW.RayFromTo( A.Position, B.Position, color, 1, 2 );
		DRAW.RayFromTo( B.Position, C.Position, color, 1, 2 );
		DRAW.RayFromTo( C.Position, A.Position, color, 1, 2 );
	}
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
