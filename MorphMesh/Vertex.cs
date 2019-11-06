﻿using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {
// A transient struct to get/set vertex data
public struct Vertex {
	public long Generation;
	public int Index;
	internal VertexOwnership Ownership;
	
	private MorphMesh m_mesh;
	private List<Vector3> _positions { get { return m_mesh.m_positions; } }
	// TODO: UVs, colors, etc
	
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
		Ownership = new VertexOwnership( mesh, index );
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
