using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
public struct Edge : IEnumerable<Vertex>, IEnumerable, IEquatable<Edge> {
	private static HashSet<int> t_worksetA = new HashSet<int>();
	private static HashSet<int> t_worksetB = new HashSet<int>();
	
	private MorphMesh m_mesh;
	private Vertex m_cachedA;
	private Vertex m_cachedB;
	
	public long Generation;
	public int IndexA;
	public int IndexB;
	
	public bool IsValid {
		get {
			return (Generation == m_mesh.m_generation);
		}
	}
	
	public Vertex A {
		get {
			if( !m_cachedA.IsValid ) { m_cachedA = m_mesh.GetVertex( IndexA ); }
			return m_cachedA;
		}
	}
	public Vertex B {
		get {
			if( !m_cachedB.IsValid ) { m_cachedB = m_mesh.GetVertex( IndexB ); }
			return m_cachedB;
		}
	}
	
	public Vertex this[int localVertexIndex] {
		get {
			localVertexIndex = localVertexIndex.Wrap( 2 );
			if( localVertexIndex == 0 ) { return A; }
			else { return B; }
		}
	}
	
	public Vector3 AB { get { return B.Position - A.Position; } }
	public Vector3 Mid {
		get {
			return (A.Position + B.Position) /2f;
		}
	}
	
	public IEnumerable<Triangle> Triangles {
		get {
			t_worksetA.Clear();
			foreach( var ownerID in A.m_ownership ) {
				t_worksetA.Add( ownerID );
			}
			
			foreach( var ownerID in B.m_ownership ) {
				if( t_worksetA.Contains( ownerID ) ) {
					yield return m_mesh.GetTriangle( ownerID );
				}
			}
		}
	}
	
#region Implementation
	internal Edge( MorphMesh mesh, int indexA, int indexB ) : this( mesh, mesh.GetVertex( indexA ), mesh.GetVertex( indexB ) ) {}
	internal Edge( MorphMesh mesh, Vertex a, Vertex b ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		IndexA = a.Index;
		IndexB = b.Index;
		
		m_cachedA = a;
		m_cachedB = b;
	}
	
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<Vertex> GetEnumerator() {
		yield return A;
		yield return B;
	}
	
	public override bool Equals( object other ) {
		if( other is Edge ) {
			return Equals( (Edge) other );
		}
		return false;
	}
	public bool Equals( Edge other ) {
		return (Generation == other.Generation) && (m_mesh == other.m_mesh) && (IndexA == other.IndexA) && (IndexB == other.IndexB);
	}
	public override int GetHashCode() {
		return (int) Generation + 23 *IndexA + 48 *IndexB;
	}
	
	public static bool operator ==( Edge a, Edge b ) {
		return a.Equals( b );
	}
	public static bool operator !=( Edge a, Edge b ) {
		return !a.Equals( b );
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
