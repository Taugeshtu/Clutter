using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {
	protected Vector3 _startDragPosition;
	protected Vector3 _startDragPointer;
	
	public bool IsDragged { get; protected set; }
	
	public static Draggable CurrentDraggable { get; protected set; }
	public static event Action<Draggable> a_dragStarted;
	public static event Action<Draggable> a_dragHappening;
	public static event Action<Draggable> a_dragEnded;
	
	public virtual void OnPointerDown(PointerEventData eventData) {
		// Capture initial positions immediately when pointer goes down
		// This prevents the visual jump that occurs when OnBeginDrag captures after movement
		RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var pointer );
		_startDragPosition = transform.position;
		_startDragPointer = pointer;
	}
	
	public virtual void OnBeginDrag( PointerEventData eventData ) {
		IsDragged = true;
		CurrentDraggable = this;
		a_dragStarted?.Invoke( this );
	}
	
	public virtual void OnEndDrag( PointerEventData eventData ) {
		IsDragged = false;
		CurrentDraggable = null;
		a_dragEnded?.Invoke( this );
	}
	
	public virtual void OnDrag( PointerEventData eventData ) {
		if( !IsDragged ) return;
		
		// Convert the mouse position to world point in canvas space
		if( RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var pointer ) ) {
			// Update the position of the object being dragged
			transform.position = _startDragPosition + (pointer - _startDragPointer);
		}
		a_dragHappening?.Invoke( this );
	}
}