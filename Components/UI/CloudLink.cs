using UnityEngine;
using UnityEngine.UI;

public class CloudLink : MonoBehaviour {
	public CloudItem nodeA;
	public CloudItem nodeB;
	
	public void UpdateVisual() {
		if( nodeA == null || nodeB == null )
			return;
		
		var startPos = nodeA.transform.position;
		var endPos = nodeB.transform.position;
		var midPoint = (startPos + endPos) / 2f;
		transform.position = midPoint;
		
		var distance = (endPos - startPos).magnitude;
		transform.localScale = Vector3.one.WithZ( distance );
	}
}