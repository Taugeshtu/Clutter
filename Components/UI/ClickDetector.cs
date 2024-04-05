using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler {
	private const float _DefaultDoubleClickTime = 0.3f;
	[SerializeField] private bool _overrideDoubleClickDuration = false;
	
	[ConditionalHide( "_overrideDoubleClickDuration" )]
	[SerializeField] private float _doubleClickTime = _DefaultDoubleClickTime;
	private float _lastClickTime;
	
	public Action<ClickDetector> a_clicked;
	public Action<ClickDetector> a_doubleClicked;
	
	public void OnPointerClick( PointerEventData eventData ) {
		if( eventData.clickCount == 2 ) {
			a_doubleClicked?.Invoke( this );
		}
		else if( eventData.clickCount == 1 ) {
			if( Time.time - _lastClickTime < _doubleClickTime ) {
				a_doubleClicked?.Invoke( this );
			}
			else {
				a_clicked?.Invoke( this );
			}
		}
		_lastClickTime = Time.time;
	}
}
