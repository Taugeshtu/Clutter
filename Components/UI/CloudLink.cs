using UnityEngine;
using UnityEngine.UI;

public class CloudLink : MonoBehaviour {
	public CloudItem nodeA;
	public CloudItem nodeB;
	
	void Update() {
		if( nodeA != null && nodeB != null )
			UpdateVisual();
	}
	
	public void UpdateVisual() {
		if( nodeA == null || nodeB == null )
			return;
		
		var start = nodeA.transform.localPosition;
		var end = nodeB.transform.localPosition;
		transform.localPosition = start;
		
		var diff = (end - start);
		transform.rotation = Quaternion.FromToRotation( Vector3.right, diff.normalized );
		
		var rectTransform = (RectTransform) transform;
		rectTransform.sizeDelta = rectTransform.sizeDelta.WithX( diff.magnitude );
	}
}