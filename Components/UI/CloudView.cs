using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Clutter;

public class CloudView : MonoBehaviour {
	[SerializeField] private RectTransform _viewport;
	
	[Header( "Settings" )]
	[SerializeField] private float _idealDistanceFactor = 1.75f;
	[SerializeField] private float _attractMaximum = 0.0f;
	[SerializeField] private float _safeZoneSizeFactor = 3;
	[SerializeField] private float _repulseExponent = 0.1f;
	[SerializeField] private float _spacing = 0;
	[SerializeField] private float _friction = 0.13f;
	[SerializeField] private int _iterations = 3;
	
	private Vector2 _viewportSize => new Vector2( _viewport.rect.width, _viewport.rect.height );
	
	void LateUpdate() {
		if( _viewportSize.x == 0 || _viewportSize.y == 0 )
			return; // not ready yet, need to wait for layout to rebuild
		
		var aspect = _viewportSize.x /_viewportSize.y;
		var items = GetComponentsInChildren<CloudItem>();
		var moveableItems = items.Where( item => !item.IsDragged ).ToList();
		var links = GetComponentsInChildren<CloudLink>( false );
		
		var hadUpdates = false;
		for( var i = 0; i < _iterations; i++ ) {
			hadUpdates |= _AttractIteration( items, moveableItems, links, aspect );
			hadUpdates |= _RelaxIteration( items, moveableItems );
		}
	}
	
	private bool _AttractIteration( IEnumerable<CloudItem> allItems, IEnumerable<CloudItem> moveableItems, IEnumerable<CloudLink> links, float aspectBias ) {
		var positionUpdates = new List<(CloudItem item, Vector3 position)>();
		
		var attractions = new TwoKeyDictionary<CloudItem, CloudItem, CloudLink>();
		foreach( var link in links ) {
			attractions.Add( link.nodeA, link.nodeB, link );
		}
		
		foreach( var a in moveableItems ) {
			var attraction = Vector2.zero;
			foreach( var b in allItems ) {
				if( b == a ) continue;
				
				var diff = a.transform.localPosition - b.transform.localPosition;
				var idealDistance = (a.Size + b.Size) *_idealDistanceFactor /2;
				
				var idealB2A = diff.normalized.ComponentMul( idealDistance );
				var idealShift = (idealB2A - diff)/2;	// /2 because the other element will also move
				
				if( attractions.TryGetValue( a, b, out var linkAB )
				 || attractions.TryGetValue( b, a, out var linkBA ) )
					attraction += idealShift.CoAlignedComponent( -diff ).XY();
			}
			attraction = attraction.MagnitudeClamped( _attractMaximum );
			
			if( attraction.magnitude > _friction )
				positionUpdates.Add( (a, a.transform.localPosition + attraction.XY0()) );
		}
		
		foreach( var (item, position) in positionUpdates ) {
			item.transform.localPosition = position;
		}
		
		return positionUpdates.Count > 0;
	}
	
	private bool _RelaxIteration( IEnumerable<CloudItem> allItems, IEnumerable<CloudItem> moveableItems ) {
		var positionUpdates = new List<(CloudItem item, Vector2 position)>();
		
		foreach( var a in moveableItems ) {
			var repulsion = Vector2.zero;
			foreach( var b in allItems ) {
				if( b == a ) continue;
				
				var diff = a.Position - b.Position;
				var halfSumSize = (a.Size + b.Size) /2;
				halfSumSize *= _idealDistanceFactor;
				halfSumSize += Vector2.one *_spacing;
				
				// I need not the "projected on", but rather some kind of "where it intersects"
				var factorX = (diff *Mathf.Sign( diff.x )).ProjectedOn( Vector2.right ).magnitude /halfSumSize.x;
				var factorY = (diff *Mathf.Sign( diff.y )).ProjectedOn( Vector2.up ).magnitude /halfSumSize.y;
				var depenetrationX = diff /factorX;
				var depenetrationY = diff /factorY;
				var shift = Vector2.zero;
				if( factorX < 1 && factorY < 1 ) {
					// we pick the one with lower magnitude
					shift = (depenetrationX.magnitude < depenetrationY.magnitude)
									? -diff + depenetrationX
									: -diff + depenetrationY;
				}
				else if( factorX < 1 ) {
					shift = -diff + depenetrationX;
				}
				else if( factorY < 1 ) {
					shift = -diff + depenetrationY;
				}
				// else skip, no depenetration happening
				
				repulsion += shift;
			}
			repulsion *= _repulseExponent;
			
			if( repulsion.magnitude > _friction )
				positionUpdates.Add( (a, a.Position + repulsion) );
		}
		
		foreach( var (item, position) in positionUpdates ) {
			item.Position = position;
		}
		
		return positionUpdates.Count > 0;
	}
}
