using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler {
	private bool _isDoubleClick = false;
	private float _doubleClickTime = 0.3f;
	private float _lastClickTime;
	
	public Action<ClickDetector> a_clicked;
	public Action<ClickDetector> a_doubleClicked;
	
	public void OnPointerClick( PointerEventData eventData ) {
		if( eventData.clickCount == 2 ) {
			_isDoubleClick = true;
			a_doubleClicked?.Invoke( this );
		}
		else if( eventData.clickCount == 1 ) {
			if( Time.time - _lastClickTime < _doubleClickTime ) {
				_isDoubleClick = true;
				a_doubleClicked?.Invoke( this );
			}
			else {
				_isDoubleClick = false;
				a_clicked?.Invoke( this );
				// Handle single click action if needed
			}
		}
		_lastClickTime = Time.time;
	}
}
