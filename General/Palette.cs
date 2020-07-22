using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Palette {
	private const float h = 0.5f;
	private const float q = 0.25f;
	private const float l = h + q;
	
	// Basics:
	public static Color clear { get				{ return new Color( 0, 0, 0, 0 ); } }
	public static Color white { get				{ return new Color( 1, 1, 1 ); } }
	public static Color gray { get				{ return new Color( h, h, h ); } }
	public static Color grey { get				{ return gray; } }
	public static Color black { get				{ return new Color( 0, 0, 0 ); } }
	
	// Spectrum:
	public static Color red { get				{ return new Color( 1, 0, 0 ); } }
	
	public static Color orange { get			{ return new Color( 1, h, 0 ); } }
	public static Color yellow { get			{ return new Color( 1, 0.92f, 0.016f ); } }
	public static Color lime { get				{ return new Color( h, 1, 0 ); } }
	
	public static Color green { get				{ return new Color( 0, 1, 0 ); } }
	
	public static Color aquamarine { get		{ return new Color( 0, 1, h ); } }
	public static Color cyan { get				{ return new Color( 0, 1, 1 ); } }
	public static Color marine { get			{ return new Color( 0, h, 1 ); } }
	
	public static Color blue { get				{ return new Color( 0, 0, 1 ); } }
	
	public static Color violet { get			{ return new Color( h, 0, 1 ); } }
	public static Color magenta { get			{ return new Color( 1, 0, 1 ); } }
	public static Color rose { get				{ return new Color( 1, 0, h ); } }
	public static Color pink { get				{ return rose; } }
	
	// Dark Basics:
	public static Color darkGray { get			{ return new Color( q, q, q ); } }
	public static Color darkGrey { get			{ return darkGray; } }
	
	// Dark Spectrum
	public static Color darkRed { get			{ return new Color( h, 0, 0 ); } }
	
	public static Color darkOrange { get		{ return new Color( h, q, 0 ); } }
	public static Color darkYellow { get		{ return new Color( h, h, 0 ); } }
	public static Color darkLime { get			{ return new Color( q, h, 0 ); } }
	
	public static Color darkGreen { get			{ return new Color( 0, h, 0 ); } }
	
	public static Color darkAquamarine { get	{ return new Color( 0, h, q ); } }
	public static Color darkCyan { get			{ return new Color( 0, h, h ); } }
	public static Color darkMarine { get		{ return new Color( 0, q, h ); } }
	
	public static Color darkBlue { get			{ return new Color( 0, 0, h ); } }
	
	public static Color darkViolet { get		{ return new Color( q, 0, h ); } }
	public static Color darkMagenta { get		{ return new Color( h, 0, h ); } }
	public static Color darkRose { get			{ return new Color( h, 0, q ); } }
	public static Color darkPink { get			{ return darkRose; } }
	
	// Light Basics:
	public static Color lightGray { get			{ return new Color( l, l, l ); } }
	public static Color lightGrey { get			{ return lightGray; } }
	
	// Light Spectrum
	public static Color lightRed { get			{ return new Color( 1, h, h ); } }
	
	public static Color lightOrange { get		{ return new Color( 1, l, h ); } }
	public static Color lightYellow { get		{ return new Color( 1, 1, h ); } }
	public static Color lightLime { get			{ return new Color( l, 1, h ); } }
	
	public static Color lightGreen { get		{ return new Color( h, 1, h ); } }
	
	public static Color lightAquamarine { get	{ return new Color( h, 1, l ); } }
	public static Color lightCyan { get			{ return new Color( h, 1, 1 ); } }
	public static Color lightMarine { get		{ return new Color( h, l, 1 ); } }
	
	public static Color lightBlue { get			{ return new Color( h, h, 1 ); } }
	
	public static Color lightViolet { get		{ return new Color( l, h, 1 ); } }
	public static Color lightMagenta { get		{ return new Color( 1, h, 1 ); } }
	public static Color lightRose { get			{ return new Color( 1, h, l ); } }
	public static Color lightPink { get			{ return lightRose; } }
}
