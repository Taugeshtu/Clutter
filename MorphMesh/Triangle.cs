using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
// This gonna be a TEMPORARY struct to get/set triangle data
public struct Triangle : IEnumerable<Vertex>, IEnumerable {
	public long Generation;
	public int ID;
	
	private MorphMesh m_mesh;
	
	private int _indexIndex { get { return _trianglesMap[ID] *3; } }
	private Mapping<int, int> _trianglesMap { get { return m_mesh.m_trianglesMap; } }
	private List<int> _indeces { get { return m_mesh.m_indeces; } }
	
	public bool IsValid {
		get {
			return (Generation == m_mesh.m_generation);
		}
	}
	
	private Vertex m_cachedA;
	private Vertex m_cachedB;
	private Vertex m_cachedC;
	
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
	
#region Implementation
	internal Triangle( MorphMesh mesh, int ownID ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		ID = ownID;
		
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
			m_mesh.GetVertex( oldVertexIndex ).Ownership.RemoveOwner( ID );
		}
		
		newSet.Remove( indexA, indexB, indexC );
		foreach( var newVertexIndex in newSet ) {
			m_mesh.GetVertex( newVertexIndex ).Ownership.AddOwner( ID );
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
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
}
