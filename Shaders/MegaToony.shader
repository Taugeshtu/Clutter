Shader "Tau/MegaToony" {
	Properties {
		_Color ( "Main Color", Color ) = (0.5, 0.5, 0.5, 1)
		_MainTex ( "Texture", 2D ) = "white" {}
		_Ramp ( "Toon Ramp (RGB)", 2D ) = "gray" {}
		_Distortion ( "Lighting distortion", Range( 0.0, 1.0 ) ) = 0.1
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
			#pragma surface surf ToonRamp
			#pragma lighting ToonRamp exclude_path:prepass
			
			sampler2D _MainTex;
			sampler2D _Ramp;
			float4 _Color;
			float _Distortion;
			
			struct Input {
				float3 worldPos;	// could be used for 3-dimensional light brushing
			};
			
			void surf( Input IN, inout SurfaceOutput o ) {
				o.Albedo = _Color;
				
				float2 screenUV = IN.worldPos.xz;
				screenUV.x += IN.worldPos.y *0.5;
				screenUV.y += IN.worldPos.y *0.5;
				screenUV *= 0.1;
				o.Alpha = tex2D( _MainTex, screenUV ).r *2;
			}
			
			inline half4 LightingToonRamp( SurfaceOutput s, half3 lightDir, half atten ) {
				#ifndef USING_DIRECTIONAL_LIGHT
				lightDir = normalize( lightDir );
				#endif
				
				fixed lNormal = dot( s.Normal, lightDir );
				fixed distorted = s.Alpha *_Distortion;
				fixed light = saturate( length( _LightColor0.rgb ) );
				fixed d;
				
				d = distorted + 0.5 + light *0.5;
				d *= lNormal;
				d = saturate( d );
				
				half3 ramp = tex2D( _Ramp, fixed2( d, 0.5 ) ).rgb;
				
				half4 c;
				c.rgb = s.Albedo *_LightColor0.rgb *ramp *2 + floor( ramp + 0.0001 );
				c.a = 0;
				
				return c;
			}
		ENDCG
	}
	Fallback "Diffuse"
}