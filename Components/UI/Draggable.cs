using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
	private Vector3 _startDragPosition;
	private Vector2 _dragOffset;
	
	public bool IsDragged { get; private set; }
	
	public void OnBeginDrag( PointerEventData eventData ) {
		IsDragged = true;
		_startDragPosition = transform.position;
		
		// Convert the mouse position to world point in canvas space
		RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var globalMousePos );
		_dragOffset = _startDragPosition - globalMousePos;
	}
	
	public void OnEndDrag( PointerEventData eventData ) {
		IsDragged = false;
	}
	
	public void OnDrag( PointerEventData eventData ) {
		if( !IsDragged ) return;
		
		// Convert the mouse position to world point in canvas space
		if( RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var globalMousePos ) ) {
			// Update the position of the object being dragged
			transform.position = globalMousePos + (Vector3) _dragOffset;
		}
	}
}
