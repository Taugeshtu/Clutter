using UnityEngine;

public class RasterCurve {
	[SerializeField] private AnimationCurve m_curve;
	[SerializeField] private int m_precision  = 512;
	
	private bool m_isBaked;
	private float[] m_bakedData;
	
	public AnimationCurve RawCurve {
		get {
			return m_curve;
		}
	}
	
#region Implementation
#endregion
	
	
#region Public
	public float Evaluate( float inValue ) {
		if( !m_isBaked ) {
			_Bake();
		}
		
		var curveStart = m_curve[0].time;
		var curveEnd = m_curve[m_curve.length - 1].time;
		var factor = Mathf.InverseLerp( curveStart, curveEnd, inValue );
		/* ToDo: make it so that wrap modes count as well here */
		factor = Mathf.Clamp01( factor );
		
		var index = Mathf.RoundToInt( factor *(m_precision - 1) );
		return m_bakedData[index];
	}
#endregion
	
	
#region Private
	private void _Bake() {
		m_isBaked = true;
		m_bakedData = new float[m_precision];
		
		for( var index = 0; index < m_precision; index++ ) {
			var curveStart = m_curve[0].time;
			var curveEnd = m_curve[m_curve.length - 1].time;
			var factor = index /(m_precision - 1f);
			var time = Mathf.Lerp( curveStart, curveEnd, factor );
			m_bakedData[index] = m_curve.Evaluate( time );
		}
	}
#endregion
	
	
#region Temp
#endregion
}
