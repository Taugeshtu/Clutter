using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Clutter;

public class CloudView : MonoBehaviour {
	[SerializeField] private RectTransform _viewport;
	
	private Vector2 _viewportSize => new Vector2( _viewport.rect.width, _viewport.rect.height );
	
	void LateUpdate() {
		if( _viewportSize.x == 0 || _viewportSize.y == 0 )
			return; // not ready yet, need to wait for layout to rebuild
		
		var aspect = _viewportSize.x /_viewportSize.y;
		var items = GetComponentsInChildren<CloudItem>();
		var moveableItems = items.Where( item => !item.IsDragged ).ToList();
		var links = GetComponentsInChildren<CloudLink>();
		
		var iterations = 3;
		var hadUpdates = false;
		for( var i = 0; i < iterations; i++ ) {
			hadUpdates |= _AttractIteration( items, moveableItems, links, aspect );
			hadUpdates |= _RelaxIteration( items, moveableItems, aspect );
		}
		
		if( hadUpdates ) {
			foreach( var link in links )
				link.UpdateVisual();
		}
	}
	
	private bool _AttractIteration( IEnumerable<CloudItem> allItems, IEnumerable<CloudItem> moveableItems, IEnumerable<CloudLink> links, float aspectBias ) {
		var positionUpdates = new List<(CloudItem item, Vector3 position)>();
		var idealDistanceFactor = 1.75f;
		var attractMaximum = 0f;
		var friction = 0.13f;
		
		var attractions = new TwoKeyDictionary<CloudItem, CloudItem, CloudLink>();
		foreach( var link in links ) {
			attractions.Add( link.nodeA, link.nodeB, link );
		}
		
		foreach( var a in moveableItems ) {
			var attraction = Vector2.zero;
			foreach( var b in allItems ) {
				if( b == a ) continue;
				
				var diff = a.transform.localPosition - b.transform.localPosition;
				var idealDistance = (a.Size + b.Size) *idealDistanceFactor /2;
				
				var idealB2A = diff.normalized.ComponentMul( idealDistance );
				var idealShift = (idealB2A - diff)/2;	// /2 because the other element will also move
				
				if( attractions.TryGetValue( a, b, out var linkAB )
				 || attractions.TryGetValue( b, a, out var linkBA ) )
					attraction += idealShift.CoAlignedComponent( -diff ).XY();
			}
			attraction = attraction.MagnitudeClamped( attractMaximum );
			
			if( attraction.magnitude > friction )
				positionUpdates.Add( (a, a.transform.localPosition + attraction.XY0()) );
		}
		
		foreach( var (item, position) in positionUpdates ) {
			item.transform.localPosition = position;
		}
		
		return positionUpdates.Count > 0;
	}
	
	private bool _RelaxIteration( IEnumerable<CloudItem> allItems, IEnumerable<CloudItem> moveableItems, float aspectBias ) {
		var positionUpdates = new List<(CloudItem item, Vector3 position)>();
		var idealDistanceFactor = 1.75f;
		var repulseExponent = 0.1f;
		var friction = 0.13f;
		
		foreach( var a in moveableItems ) {
			var repulsion = Vector2.zero;
			foreach( var b in allItems ) {
				if( b == a ) continue;
				
				var diff = a.transform.localPosition - b.transform.localPosition;
				var idealDistance = (a.Size + b.Size) *idealDistanceFactor /2;
				
				var idealB2A = diff.normalized.ComponentMul( idealDistance );
				var idealShift = (idealB2A - diff)/2;	// /2 because the other element will also move
				repulsion += idealShift.CoAlignedComponent( diff ).XY();
			}
			repulsion.y /= aspectBias;
			repulsion *= repulseExponent;
			
			if( repulsion.magnitude > friction )
				positionUpdates.Add( (a, a.transform.localPosition + repulsion.XY0()) );
		}
		
		foreach( var (item, position) in positionUpdates ) {
			item.transform.localPosition = position;
		}
		
		return positionUpdates.Count > 0;
	}
}
