 Shader "Tau/NoClip" {
	Properties {
		_MainColor( "Color", color ) = (1, 1, 1, 0.5)
		_MainTex( "Texture", 2D ) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Cull Off
		CGPROGRAM
			#pragma surface surf Lambert
			#pragma target 2.0
			
			struct Input {
				float2 uv_MainTex;
			};
			
			sampler2D _MainTex;
			float4 _MainColor;
			
			void surf( Input IN, inout SurfaceOutput o ){
				o.Albedo = tex2D( _MainTex, IN.uv_MainTex ).rgb;
				o.Albedo *= _MainColor;
			}
		ENDCG
	}
	Fallback "Diffuse"
}