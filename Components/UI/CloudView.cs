using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Clutter;

public class CloudView : MonoBehaviour {
	[SerializeField] private RectTransform _viewport;
	
	[Header( "Settings" )]
	[SerializeField] private float _attractMaximum = 0.0f;
	[SerializeField] private float _safeZoneSizeFactor = 3;
	[SerializeField] private float _repulseExponent = 0.1f;
	[SerializeField] private float _repulsionSoftZoneExpansion = 0;
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
			// hadUpdates |= _AttractIteration( items, moveableItems, links );
			hadUpdates |= _RelaxIteration( items );
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
				var idealDistance = (a.Size + b.Size) /2;
				
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
	
	private bool _RelaxIteration( IEnumerable<CloudItem> items ) {
		var positionUpdates = new List<(CloudItem item, Vector2 position)>();
		
		foreach( var a in items ) {
			if( a.IsDragged ) continue;
			
			var repulsion = Vector2.zero;
			foreach( var b in items ) {
				if( b == a ) continue;
				
				var diff = a.Position - b.Position;
				if( diff.magnitude < Mathf.Epsilon )	// guarding against clumped together items
					diff = Random.insideUnitCircle.normalized;
				var halfSumSize = (a.Size + b.Size) /2;
				halfSumSize += Vector2.one *_spacing;
				
				// deciding if ideal position is in horizontal offset, or vertical one
				var diffAspect = (diff.x /diff.y).Abs();
				var sizeAspect = halfSumSize.x /halfSumSize.y;
				var depenetration = (diffAspect >= sizeAspect)
									? Vector2.right *Mathf.Sign( diff.x ) *halfSumSize.x
									: Vector2.up *Mathf.Sign( diff.y ) *halfSumSize.y;
				var shift = Vector2.zero;
				var depenetrationMagnitudeDiff = diff.ProjectedOn( depenetration ).magnitude - halfSumSize.ProjectedOn( depenetration ).magnitude;
				if( depenetrationMagnitudeDiff <= 0 ) {
					shift = depenetration - diff.ProjectedOn( depenetration );
					if( !b.IsDragged )	// accounting for immovable items
						shift /= 2;
				}
				else {
					// flattening diff when inside horizontal/vertical "stripe" of b.Size
					shift = diff;
					var substractedSize = halfSumSize;
					if( shift.x.Abs() < (halfSumSize.x /2) ) { shift.x = 0; substractedSize.x = 0; }
					if( shift.y.Abs() < (halfSumSize.y /2) ) { shift.y = 0; substractedSize.y = 0; }
					substractedSize.x *= Mathf.Sign( shift.x );
					substractedSize.y *= Mathf.Sign( shift.y );
					shift -= substractedSize;
					
					var softFactor = Mathf.InverseLerp( 0, (halfSumSize *_repulsionSoftZoneExpansion).magnitude, shift.magnitude );
					softFactor = 1 - softFactor.Clamp01();
					shift *= softFactor;
					
					shift = Vector2.zero;
				}
				
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
