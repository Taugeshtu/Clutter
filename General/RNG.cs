﻿using UnityEngine;
using System.Collections.Generic;
using SysRandom = System.Random;

public static partial class RNG {
	private static readonly SysRandom _sysRandom = new SysRandom();
	
#region Points
	private static Vector3[] s_cubeShifts = {
		Vector3.forward *0.5f,
		Vector3.right *0.5f,
		Vector3.back *0.5f,
		Vector3.left *0.5f,
		Vector3.down *0.5f,
		Vector3.up *0.5f,
	};
	private static Quaternion[] s_cubeRotations = {
		Quaternion.identity,
		Quaternion.AngleAxis( 90f, Vector3.up ),
		Quaternion.AngleAxis( 180f, Vector3.up ),
		Quaternion.AngleAxis( 270f, Vector3.up ),
		Quaternion.AngleAxis( 90f, Vector3.right ),
		Quaternion.AngleAxis( -90f, Vector3.right ),
	};
	
	public static Vector3 OnUnitCube() {
		var x = Random.Range( -0.5f, 0.5f );
		var y = Random.Range( -0.5f, 0.5f );
		var flatPoint = new Vector3( x, y, 0.5f );
		
		var side = Random.Range( 0, 6 );
		var rotation = s_cubeRotations[side];
		return rotation *flatPoint;
	}
	
	public static Vector3 InUnitCube() {
		var x = (float) (_sysRandom.NextDouble() - 0.5) *2;
		var y = (float) (_sysRandom.NextDouble() - 0.5) *2;
		var z = (float) (_sysRandom.NextDouble() - 0.5) *2;
		return new Vector3( x, y, z );
	}
#endregion
}
