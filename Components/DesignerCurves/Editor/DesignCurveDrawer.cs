using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

using SegmentType = DesignCurve.SegmentType;

[CustomPropertyDrawer(typeof(DesignCurve))]
public class InteractiveImagePropertyDrawer : PropertyDrawer {
	private const int _SamplesPerSegmentPreview = 6;
	private const int _SamplesPerSegmentFull = 20;
	private const int _FloatingInputThreshold = 30;
	private static Vector2 _FloatingInputSize = new Vector2( _RulerPointWidth, EditorGUIUtility.singleLineHeight );
	private static Vector2 _DeleteButtonSize = new Vector2( 20, 20 );
	private const float _DoubleClickTime = 0.35f;
	
	private static GUIStyle _LeftAligned = new GUIStyle( EditorStyles.numberField ) { alignment = TextAnchor.MiddleLeft };
	private static GUIStyle _RightAligned = new GUIStyle( _LeftAligned ) { alignment = TextAnchor.MiddleRight };
	
	private const float _ImageHeight = 150f;
	private const float _Padding = 10f;
	private const float _RulerPointWidth = 30f;
	private const float _RulerSpacing = 3f;
	
	private DesignCurve _curve;
	private float _currentHeight = EditorGUIUtility.singleLineHeight;
	private System.DateTime _lastClickTime;
	
	// xtr(TODO):
		// cursor with evaluator on the other end
	// max(TODO):
		// dragging points around
	
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
		return _currentHeight;
	}
	
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginProperty( position, label, property );
		var target = property.serializedObject.targetObject;
		
		if( _curve == null )
			_curve = _GetDesignCurve( property );
		_curve.Sanitize();
		
		var labelRect = position;
		labelRect.height = EditorGUIUtility.singleLineHeight;
		_currentHeight = EditorGUIUtility.singleLineHeight;
		EditorGUI.PropertyField( labelRect, property, label, false );
		
		if( !property.isExpanded ) {
			var previewRect = new Rect( labelRect );
			var previewWidth = position.width - EditorGUIUtility.labelWidth - 3;
			previewRect.position += Vector2.right *(position.width - previewWidth);
			previewRect.width = previewWidth;
			EditorGUI.DrawRect( previewRect, Color.black.Mix( Color.white, 0.5f ) );
			_DrawCurve( _curve, previewRect, Color.cyan, _SamplesPerSegmentPreview );
		}
		
		if( property.isExpanded ) {
			var currentEvent = Event.current;
			var imageRect = new Rect( position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, _ImageHeight );
			imageRect.position += Vector2.right *_Padding;
			imageRect.width -= _Padding *2;
			var isHovered = imageRect.Contains( currentEvent.mousePosition );
			
			var curveColor = Color.cyan;
			_currentHeight += _ImageHeight;
			EditorGUI.DrawRect( imageRect, Color.black.Mix( Color.white, 0.5f ) );
			_DrawTicks( _curve, imageRect, Color.black );
			_DrawTails( _curve.Points[0], _curve.Points[_curve.Points.Count - 1], imageRect, Color.black );
			_DrawCurve( _curve, imageRect, curveColor, _SamplesPerSegmentFull );
			
			_currentHeight += EditorGUIUtility.singleLineHeight;
			_DrawRuler( target, _curve, imageRect );
			
			if( isHovered ) {
				var isClick = false;
				if( currentEvent.type == EventType.MouseDown ) {
					isClick = true;
				}
				
				var localPixel = (currentEvent.mousePosition - imageRect.min);
				localPixel.y = imageRect.height - localPixel.y;
				var localPosition = localPixel.ComponentDiv( imageRect.size );
				_DrawFloatingControls( target, isClick, _curve, imageRect, localPosition );
				
				if( isClick ) {
					var currentTime = System.DateTime.Now;
					var timeSinceLastClick = currentTime - _lastClickTime;
					if( timeSinceLastClick.TotalSeconds < _DoubleClickTime ) {
						Undo.RecordObject( target, "DC add point" );
						_curve.AddPoint( localPosition );
					}
					_lastClickTime = currentTime;
					
					currentEvent.Use();
				}
			}
			
			// forcing Unity to provide us with snappy updates!
			EditorUtility.SetDirty( property.serializedObject.targetObject );
		}
		
		EditorGUI.EndProperty();
	}
	
	private DesignCurve _GetDesignCurve( SerializedProperty property ) {
		var serializedObject = property.serializedObject;
		
		var targetObject = serializedObject.targetObject;
		var targetType = targetObject.GetType();
		var fieldInfo = targetType.GetField( property.propertyPath,
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
		
		return fieldInfo.GetValue( targetObject ) as DesignCurve;
	}
	
	private static void _DrawCurve( DesignCurve curve, Rect imageRect, Color color, int samplesPerSegment ) {
		Handles.color = color;
		
		var pointsToDraw = new List<Vector2>( curve.Points );
		for( var i = 0; i < curve.Segments.Count; i++ ) {
			var pointA = curve.Points[i];
			var pointB = curve.Points[i + 1];
			
			var segment = curve.Segments[i];
			if( segment == SegmentType.Linear ) {}
			else if( segment == SegmentType.ConstLeft )
				pointsToDraw.Add( new Vector2( pointB.x - 0.001f, pointA.y ) );
			else if( segment == SegmentType.ConstRight )
				pointsToDraw.Add( new Vector2( pointA.x + 0.001f, pointB.y ) );
			else {
				var xStep = (pointB - pointA).x /(samplesPerSegment + 1);
				for( var sample = 0; sample < samplesPerSegment; sample++ ) {
					var x = pointA.x + xStep *(sample + 1);
					var y = curve[x];
					pointsToDraw.Add( new Vector2( x, y ) );
				}
			}
		}
		pointsToDraw.Sort( (a, b) => a.x.CompareTo( b.x ) );
		
		for( var i = 1; i < pointsToDraw.Count; i++ ) {
			var a = pointsToDraw[i - 1];
			a.y = 1 - a.y;
			a = a.ComponentMul( imageRect.size ) + imageRect.min;
			
			var b = pointsToDraw[i];
			b.y = 1 - b.y;
			b = b.ComponentMul( imageRect.size ) + imageRect.min;
			
			Handles.DrawLine( a, b );
			Handles.DrawLine( a + Vector2.down, b + Vector2.down );
		}
	}
	
	private static void _DrawTicks( DesignCurve curve, Rect imageRect, Color color ) {
		Handles.color = color;
		
		for( var i = 0; i < curve.Points.Count; i++ ) {
			var point = curve.Points[i];
			var pixelPoint = point.WithY( 1 - point.y ).ComponentMul( imageRect.size ) + imageRect.position;
			var groundedPixelPoint = point.WithY( 1 ).ComponentMul( imageRect.size ) + imageRect.position;
			Handles.DrawDottedLine( groundedPixelPoint, pixelPoint, 3f );
		}
	}
	
	private static void _DrawRuler( Object target, DesignCurve curve, Rect position ) {
		// draw input fields and enum fields for the thing
		var yOffset = _ImageHeight;
		var prevRectOffset = 0f;
		for( var i = 0; i < curve.Points.Count; i++ ) {
			var x = curve.Points[i].x;
			var rect = new Rect( position.position, Vector2.zero );
			var xOffset = x *position.width - x *_RulerPointWidth;
			rect.position += Vector2.right *xOffset;
			rect.position += Vector2.up *yOffset;
			
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.width = _RulerPointWidth;
			
			var style = (i == curve.Points.Count - 1) ? _RightAligned : _LeftAligned;
			var newX = EditorGUI.FloatField( rect, x, style );
			if( newX != x ) {
				Undo.RecordObject( target, "Change DC point timing" );
				curve.Points[i] = curve.Points[i].WithX( newX );
			}
			
			if( i > 0 ) {
				var segment = curve.Segments[i - 1];
				var segmentRect = new Rect( position.position, Vector2.zero );
				var segmentOffset = (prevRectOffset + _RulerPointWidth) + _RulerSpacing;
				segmentRect.position += Vector2.right *segmentOffset;
				segmentRect.position += Vector2.up *yOffset;
				
				segmentRect.height = EditorGUIUtility.singleLineHeight;
				segmentRect.width = xOffset - segmentOffset - _RulerSpacing;
				var newSegment = (SegmentType) EditorGUI.EnumPopup( segmentRect, segment );
				if( newSegment != segment ) {
					Undo.RecordObject( target, "Change DC segment type" );
					curve.Segments[i - 1] = newSegment;
				}
			}
			
			prevRectOffset = xOffset;
		}
	}
	
	private static void _DrawFloatingControls( Object target, bool isClick, DesignCurve curve, Rect imageRect, Vector2 localPosition ) {
		var minDistance = float.MaxValue;
		var closestPointIndex = -1;
		for( var i = 0; i < curve.Points.Count; i++ ) {
			var point = curve.Points[i];
			var groundedPoint = point.WithY( 0 );
			var liftedPoint = point.WithY( 1 );
			
			var distanceMetric = (groundedPoint - localPosition).sqrMagnitude;
			if( distanceMetric < minDistance ) {
				minDistance = distanceMetric;
				closestPointIndex = i;
			}
			
			distanceMetric = (liftedPoint - localPosition).sqrMagnitude;
			if( distanceMetric < minDistance ) {
				minDistance = distanceMetric;
				closestPointIndex = i;
			}
		}
		
		if( closestPointIndex != -1 ) {
			var point = curve.Points[closestPointIndex];
			var groundedPoint = point.WithY( 0 );
			var liftedPoint = point.WithY( 1 );
			
			var pixelDistance = (groundedPoint - localPosition).ComponentMul( imageRect.size ).magnitude;
			if( pixelDistance < _FloatingInputThreshold ) {
				var floatPosition = point.WithY( 1 );
				floatPosition = floatPosition.ComponentMul( imageRect.size - _FloatingInputSize );
				floatPosition += imageRect.min;
				var fRect = new Rect( floatPosition, _FloatingInputSize );
				
				var style = (closestPointIndex == curve.Points.Count - 1) ? _RightAligned : _LeftAligned;
				var newValue = EditorGUI.FloatField( fRect, point.y, style );
				if( newValue != point.y ) {
					point.y = newValue;
					Undo.RecordObject( target, "Change DC point value" );
					curve.Points[closestPointIndex] = point;
				}
			}
			
			pixelDistance = (liftedPoint - localPosition).ComponentMul( imageRect.size ).magnitude;
			if( (pixelDistance < _DeleteButtonSize.x) && (closestPointIndex != 0) && (closestPointIndex != curve.Points.Count - 1) ) {
				var floatPosition = point.WithY( 0 );
				floatPosition = floatPosition.ComponentMul( imageRect.size );
				floatPosition += imageRect.min + Vector2.left *_DeleteButtonSize.x /2;
				var fRect = new Rect( floatPosition, _DeleteButtonSize );
				var closeButtonContent = EditorGUIUtility.IconContent( "d_winbtn_mac_close_h@2x" );
				GUI.Label( fRect, closeButtonContent );
				
				if( fRect.Contains( Event.current.mousePosition ) && isClick ) {
					Undo.RecordObject( target, "DC delete point" );
					curve.DeletePoint( closestPointIndex );
				}
			}
		}
	}
	
	private static void _DrawTails( Vector2 start, Vector2 end, Rect imageRect, Color color ) {
		Handles.color = color;
		
		var dash = 3f;
		var a = start.WithY( 1 - start.y ).ComponentMul( imageRect.size ) + imageRect.position;
		var b = a + Vector2.left *_Padding;
		Handles.DrawDottedLine( a, b, dash );
		Handles.DrawDottedLine( a + Vector2.down, b + Vector2.down, dash );
		
		a = end.WithY( 1 - end.y ).ComponentMul( imageRect.size ) + imageRect.position;
		b = a + Vector2.right *_Padding;
		Handles.DrawDottedLine( a, b, dash );
		Handles.DrawDottedLine( a + Vector2.down, b + Vector2.down, dash );
	}
}
