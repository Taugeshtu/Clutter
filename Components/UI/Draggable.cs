using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
	protected Vector3 _startDragPosition;
	protected Vector3 _startDragPointer;
	
	public bool IsDragged { get; protected set; }
	
	public virtual void OnBeginDrag( PointerEventData eventData ) {
		IsDragged = true;
		
		// Convert the mouse position to world point in canvas space
		RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var pointer );
		_startDragPosition = transform.position;
		_startDragPointer = pointer;
	}
	
	public virtual void OnEndDrag( PointerEventData eventData ) {
		IsDragged = false;
	}
	
	public virtual void OnDrag( PointerEventData eventData ) {
		if( !IsDragged ) return;
		
		// Convert the mouse position to world point in canvas space
		if( RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var pointer ) ) {
			// Update the position of the object being dragged
			transform.position = _startDragPosition + (pointer - _startDragPointer);
		}
	}
}
