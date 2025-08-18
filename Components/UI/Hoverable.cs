using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public bool IsHovered { get; protected set; }
	
	public event Action a_hoverStarted;
	public event Action a_hoverHappening;
	public event Action a_hoverEnded;
	
	public static Hoverable CurrentHovered { get; protected set; }
	public static event Action<Hoverable> a_globalHoverStarted;
	public static event Action<Hoverable> a_globalHoverHappening;
	public static event Action<Hoverable> a_globalHoverEnded;
	
	void Update() {
		if (IsHovered) {
			a_hoverHappening?.Invoke();
			a_globalHoverHappening?.Invoke(this);
		}
	}
	
	public void OnPointerEnter(PointerEventData eventData) {
		if (IsHovered) return; // Already hovered, shouldn't happen but safety first
		
		IsHovered = true;
		CurrentHovered = this;
		
		a_hoverStarted?.Invoke();
		a_globalHoverStarted?.Invoke(this);
	}
	
	public void OnPointerExit(PointerEventData eventData) {
		if (!IsHovered) return; // Not hovered, shouldn't happen but safety first
		
		IsHovered = false;
		if (CurrentHovered == this) {
			CurrentHovered = null;
		}
		
		a_hoverEnded?.Invoke();
		a_globalHoverEnded?.Invoke(this);
	}
	
	void OnDisable() {
		// Clean up if disabled while hovered
		if (IsHovered) {
			IsHovered = false;
			if (CurrentHovered == this) {
				CurrentHovered = null;
			}
			a_hoverEnded?.Invoke();
			a_globalHoverEnded?.Invoke(this);
		}
	}
}