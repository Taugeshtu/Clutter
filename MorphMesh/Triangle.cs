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
	
#region Implementation
	public Triangle( long generation, int ownID ) {
		Generation = generation;
		ID = ownID;
		
		A = Vertex.Invalid;
		B = Vertex.Invalid;
		C = Vertex.Invalid;
	}
	
	public Triangle( long generation, int ownID, ref Vertex a, ref Vertex b, ref Vertex c ) {
		Generation = generation;
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
