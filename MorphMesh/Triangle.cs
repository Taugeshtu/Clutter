using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {
// This gonna be a TEMPORARY struct to get/set triangle data
public struct Triangle {
	public long Generation;
	public int ID;
	
	public Vertex A;
	public Vertex B;
	public Vertex C;
	
	private MorphMesh m_mesh;
	
	public bool IsValid {
		get {
			return (Generation == m_mesh.m_generation);
		}
	}
	
#region Implementation
	public Triangle( MorphMesh mesh, int ownID, ref Vertex a, ref Vertex b, ref Vertex c ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		ID = ownID;
		
		A = a;
		B = b;
		C = c;
		
		_RegisterVertices();
	}
#endregion
	
	
#region Public
	public void SetVertices( ref Vertex a, ref Vertex b, ref Vertex c ) {
		_UnRegisterVertices();
		
		A = a;
		B = b;
		C = c;
		
		_RegisterVertices();
	}
	
	public void Flip() {
		var tempV = B;
		B = C;
		C = tempV;
	}
#endregion
	
	
#region Private
	private void _RegisterVertices() {
		_RegisterVertex( ref A );
		_RegisterVertex( ref B );
		_RegisterVertex( ref C );
	}
	
	private void _UnRegisterVertices() {
		_UnRegisterVertex( ref A );
		_UnRegisterVertex( ref B );
		_UnRegisterVertex( ref C );
	}
	
	private void _RegisterVertex( ref Vertex v ) {
		v.RegisterTriangle( ID );
	}
	
	private void _UnRegisterVertex( ref Vertex v ) {
		v.UnRegisterTriangle( ID );
	}
#endregion
	
	
#region Temporary
#endregion
}
}
