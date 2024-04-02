using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent( typeof( Image ) )]
public class RectSelector : Draggable {
	[SerializeField] private RectTransform _rect;
	private Graphic _catcher;
	private Vector3 _selectStart;
	
	public bool Active {
		get => _catcher.raycastTarget;
		set => _catcher.raycastTarget = value;
	}
	public Action<Rect> a_selection;
	
	void Awake() {
		_catcher = GetComponent<Graphic>();
		_rect.gameObject.SetActive( false );
	}
	
	public override void OnBeginDrag( PointerEventData eventData ) {
		IsDragged = true;
		
		RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var localPointer );
		_selectStart = localPointer;
		_rect.gameObject.SetActive( true );
		_rect.localPosition = _selectStart;
		_rect.sizeDelta = Vector2.one *2;
	}
	
	public override void OnDrag( PointerEventData eventData ) {
		if( !IsDragged ) return;
		
		RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var localPointer );
		_rect.localPosition = transform.InverseTransformPoint( (_selectStart + localPointer) /2 );
		_rect.sizeDelta = (localPointer - _selectStart).Abs();
	}
	
	public override void OnEndDrag( PointerEventData eventData ) {
		IsDragged = false;
		
		RectTransformUtility.ScreenPointToWorldPointInRectangle( (RectTransform) transform, eventData.position, eventData.pressEventCamera, out var localPointer );
		_rect.gameObject.SetActive( false );
		
		var rect = new Rect();
		rect.min = Vector.Min( _selectStart, localPointer );
		rect.max = Vector.Max( _selectStart, localPointer );
		a_selection?.Invoke( rect );
	}
}
