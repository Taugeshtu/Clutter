using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Clutter.Mesh {
// A transient struct to get/set vertex data
public struct Vertex : IEnumerable<Triangle>, IEnumerable {
	private MorphMesh m_mesh;
	internal VertexOwnership m_ownership;
	private List<Vector3> _positions { get { return m_mesh.m_positions; } }
	// TODO: UVs, colors, etc
	
	public long Generation;
	public int Index;
	
	public bool IsValid {
		get {
			return (Index != MorphMesh.c_invalidID) && (Generation == m_mesh.m_generation);
		}
	}
	
	public Vector3 Position {
		get { return _positions[Index]; }
		set { _positions[Index] = value; }
	}
	
#region Implementation
	internal Vertex( MorphMesh mesh, int index ) {
		m_mesh = mesh;
		Generation = mesh.m_generation;
		Index = index;
		m_ownership = new VertexOwnership( mesh, index );
	}
	
	System.Collections.IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	public IEnumerator<Triangle> GetEnumerator() {
		foreach( var ownerID in m_ownership ) {
			yield return m_mesh.GetTriangle( ownerID );
		}
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
