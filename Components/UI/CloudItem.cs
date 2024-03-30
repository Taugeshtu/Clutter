using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CloudItem : MonoBehaviour {
	[SerializeField] private Draggable _draggable;
	
	public Vector2 Size => ((RectTransform) transform).sizeDelta;
	public bool IsDragged => _draggable == null ? false : _draggable.IsDragged;
}
