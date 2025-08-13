using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {
	protected Vector3 _startDragPosition;
	protected Vector3 _startDragPointer;
	
	public bool IsDragged { get; protected set; }
	public event Action a_dragStarted;
	public event Action a_dragHappening;
	public event Action a_dragEnded;
	
	public static Draggable CurrentDraggable { get; protected set; }
	public static event Action<Draggable> a_globalDragStarted;
	public static event Action<Draggable> a_globalDragHappening;
	public static event Action<Draggable> a_globalDragEnded;
	
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
		a_dragStarted?.Invoke();
		a_globalDragStarted?.Invoke( this );
	}
	
	public virtual void OnEndDrag( PointerEventData eventData ) {
		IsDragged = false;
		CurrentDraggable = null;
		a_dragEnded?.Invoke();
		a_globalDragEnded?.Invoke( this );
	}
	
	public virtual void OnDrag( PointerEventData eventData ) {
		if( !IsDragged ) return;
		
		// Convert the mouse position to world point in canvas space
		if( RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var pointer ) ) {
			// Update the position of the object being dragged
			transform.position = _startDragPosition + (pointer - _startDragPointer);
		}
		a_dragHappening?.Invoke();
		a_globalDragHappening?.Invoke( this );
	}
}