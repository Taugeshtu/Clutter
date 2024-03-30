using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CloudView : MonoBehaviour {
	[SerializeField] private RectTransform _viewport;
	[SerializeField] private CloudItem _testPrefab;
	
	private Vector2 _viewportSize => new Vector2( _viewport.rect.width, _viewport.rect.height );
	
	void LateUpdate() {
		if( _viewportSize.x == 0 || _viewportSize.y == 0 )
			return; // not ready yet, need to wait for layout to rebuild
		
		var aspect = _viewportSize.x /_viewportSize.y;
		var items = GetComponentsInChildren<CloudItem>();
		
		var iterations = 3;
		for( var i = 0; i < iterations; i++ )
			_RelaxIteration( items, items.Where( item => !item.IsDragged ), aspect );
	}
	
	private void _RelaxIteration( IEnumerable<CloudItem> allItems, IEnumerable<CloudItem> moveableItems, float aspectBias ) {
		var newPositions = new Dictionary<CloudItem, Vector3>();
		var idealDistanceFactor = 1.75f;
		var repulseExponent = 0.1f;
		var attractMaximum = 0f;
		var friction = 0.13f;
		
		foreach( var a in moveableItems ) {
			var repulsion = Vector2.zero;
			var attraction = Vector2.zero;
			foreach( var b in allItems ) {
				if( b == a ) continue;
				
				var diff = a.transform.localPosition - b.transform.localPosition;
				var idealDistance = (a.Size + b.Size) *idealDistanceFactor /2;
				
				var idealB2A = diff.normalized.ComponentMul( idealDistance );
				var idealShift = (idealB2A - diff)/2;	// /2 because the other element will also move
				repulsion += idealShift.CoAlignedComponent( diff ).XY();
				attraction += idealShift.CoAlignedComponent( -diff ).XY();
			}
			repulsion.y /= aspectBias;
			repulsion *= repulseExponent;
			attraction = attraction.MagnitudeClamped( attractMaximum );
			
			var shift = repulsion.XY0() + attraction.XY0();
			if( shift.magnitude > friction )
				newPositions[a] = a.transform.localPosition + shift;
		}
		
		foreach( var pair in newPositions ) {
			pair.Key.transform.localPosition = pair.Value;
		}
	}

}
