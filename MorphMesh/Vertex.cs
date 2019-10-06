using UnityEngine;
using System.Collections.Generic;

namespace Clutter.Mesh {
// This gonna be a TEMPORARY struct to get/set vertex data
public struct Vertex {
	public const int c_ownersFast = 5;
	
	public int Generation;
	public int Index;
	public Vector3 Position;
	
	// TODO: UVs, colors, etc
	
	public HashSet<int> Triangles;
	
	public bool IsValid { get { return Index != MorphMesh.c_invalidID; } }
	
#region Implementation
	public static Vertex Invalid {
		get {
			return new Vertex( MorphMesh.c_invalidID, MorphMesh.c_invalidID, VectorExtensions.Invalid3 );
		}
	}
	
	public Vertex( int generation, int id, Vector3 position ) {
		Generation = generation;
		Index = id;
		Position = position;
		
		Triangles = new HashSet<int>();
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
	internal void RegisterTriangle( int triangleID ) {
		Triangles.Add( triangleID );
	}
	
	internal void UnRegisterTriangle( int triangleID ) {
		Triangles.Remove( triangleID );
	}
#endregion
	
	
#region Temporary
#endregion
}
}
