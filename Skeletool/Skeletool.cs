using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SkeletoolInner;

public class Skeletool : MonoBehaviour {
	public class PlayOrder {
		public string Name { get; private set; }
		public AnimationCurve Curve { get; private set; }
		public float Duration { get; private set; }
		public PlayOrder( string name, AnimationCurve curve, float duration ) {
			Name = name;
			Curve = curve;
			Duration = duration;
		}
	}
	
	private const string c_idlePoseName = "GroundZero";
	private const string c_currentPoseName = "LastReachedPose";
	
	[SerializeField] private Transform m_visualRoot;
	[SerializeField] private List<SkeletoolPose> m_poses;
	
	private SkeletoolPose m_idlePose;
	private SkeletoolPose m_lastReachedPose;
	private Queue<PlayOrder> m_orders = new Queue<PlayOrder>();
	
	public bool IsPlaying { get; private set; }
	public float Factor { get; private set; }
	
	public event Action<PlayOrder> a_donePlaying = (x) => {};
	public event Action<PlayOrder> a_startedPlaying = (x) => {};
	
#region Public
	public PlayOrder Enqueue( string name, AnimationCurve curve, float duration ) {
		var order = new PlayOrder( name, curve, duration );
		return Enqueue( order );
	}
	
	public PlayOrder Enqueue( PlayOrder order ) {
		m_orders.Enqueue( order );
		return order;
	}
	
	public PlayOrder GoIdle( AnimationCurve curve, float duration ) {
		var order = new PlayOrder( c_idlePoseName, curve, duration );
		return Enqueue( order );
	}
	
	public void StopAll() {
		StopCurrent();
		m_orders.Clear();
	}
	
	public void StopCurrent() {
		StopAllCoroutines();
		IsPlaying = false;
		Factor = 0f;
		_FinishedMoving();
	}
#endregion
	
	
#region UnityCallbacks
	void Awake() {
		foreach( var pose in m_poses ) {
			pose.NukeImmediate();
			pose.Root.gameObject.SetActive( true );
		}
		
		var idleObject = Instantiate( m_visualRoot );
		idleObject.SetParent( m_visualRoot.parent );
		idleObject.SetSiblingIndex( 1 );
		idleObject.gameObject.name = c_idlePoseName;
		
		m_idlePose = new SkeletoolPose( c_idlePoseName, idleObject );
		m_idlePose.NukeImmediate();
		
		var lastReachedObject = Instantiate( m_visualRoot );
		lastReachedObject.SetParent( m_visualRoot.parent );
		lastReachedObject.SetSiblingIndex( 2 );
		lastReachedObject.gameObject.name = c_currentPoseName;
		
		m_lastReachedPose = new SkeletoolPose( c_currentPoseName, lastReachedObject );
		m_lastReachedPose.NukeImmediate();
	}
	
	void LateUpdate() {
		if( !IsPlaying ) {
			if( m_orders.Count > 0 ) {
				var orderToPlay = m_orders.Dequeue();
				StartCoroutine( _PlayRoutine( orderToPlay ) );
			}
		}
	}
#endregion
	
	
#region Private
	private SkeletoolPose _GetPoseForOrder( PlayOrder order ) {
		foreach( var pose in m_poses ) {
			if( pose.Name == order.Name ) {
				return pose;
			}
		}
		
		if( order.Name == m_idlePose.Name ) {
			return m_idlePose;
		}
		
		return default( SkeletoolPose );
	}
	
	private IEnumerator _PlayRoutine( PlayOrder order ) {
		IsPlaying = true;
		Factor = 0f;
		a_startedPlaying( order );
		
		var startTime = Time.time;
		var endTime = startTime + order.Duration;
		var pose = _GetPoseForOrder( order );
		
		while( Time.time <= endTime ) {
			Factor = Mathf.InverseLerp( startTime, endTime, Time.time );
			var value = order.Curve.Evaluate( Factor );
			
			_AssumePosition( m_visualRoot, m_lastReachedPose.Root, pose.Root, value );
			yield return new WaitForEndOfFrame();
		}
		
		_FinishedMoving();
		a_donePlaying( order );
		IsPlaying = false;
	}
	
	private void _AssumePosition( Transform target, Transform from, Transform to, float factor ) {
		for( var i = 0; i < target.childCount; i++ ) {
			var visualChild = target.GetChild( i );
			var fromChild = from.GetChild( i );
			var toChild = to.GetChild( i );
			
			visualChild.localPosition = Vector3.LerpUnclamped( fromChild.localPosition, toChild.localPosition, factor );
			visualChild.localRotation = Quaternion.LerpUnclamped( fromChild.localRotation, toChild.localRotation, factor );
			visualChild.localScale = Vector3.LerpUnclamped( fromChild.localScale, toChild.localScale, factor );
			
			if( visualChild.childCount != 0 ) {
				_AssumePosition( visualChild, fromChild, toChild, factor );
			}
		}
	}
	
	private void _FinishedMoving() {
		_Copy( m_visualRoot, m_lastReachedPose.Root );
	}
	
	private void _Copy( Transform from, Transform target ) {
		for( var i = 0; i < from.childCount; i++ ) {
			var fromChild = from.GetChild( i );
			var targetChild = target.GetChild( i );
			
			targetChild.localPosition = fromChild.localPosition;
			targetChild.localRotation = fromChild.localRotation;
			targetChild.localScale = fromChild.localScale;
			
			if( fromChild.childCount != 0 ) {
				_Copy( fromChild, targetChild );
			}
		}
	}
#endregion
	
	
#region Temporary
#endregion
}
