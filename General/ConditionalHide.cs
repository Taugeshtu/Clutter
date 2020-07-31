using UnityEngine;
using System;
using System.Collections.Generic;

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true )]
public class ConditionalHideAttribute : PropertyAttribute {
	public string Source { get; private set; }
	public int EnumIndex { get; private set; }
	public bool Inverted { get; private set; }
	
#region Implementation
	public ConditionalHideAttribute( string boolConditionField, bool invert = false ) {
		Source = boolConditionField;
		Inverted = invert;
	}

	public ConditionalHideAttribute( string enumConditionField, int enumIndex, bool invert = false ) {
		Source = enumConditionField;
		EnumIndex = enumIndex;
		Inverted = invert;
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
#endregion
	
	
#region Temporary
#endregion
}
