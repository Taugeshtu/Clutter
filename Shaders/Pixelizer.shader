Shader "Tau/Pixelizer" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_RoundFactor ("Round Factor", Float) = 100
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf BlinnPhong
		/*
		half4 LightingWrapLambert (SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
			half diff = NdotL * 0.5 + 0.5;
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
			c.a = s.Alpha;
			return c;
		}
		*/
		struct Input {
			float2 uv_MainTex;
		};
		sampler2D _MainTex;
		float _RoundFactor;
		void surf (Input IN, inout SurfaceOutput o) {
			float2 _new_uv = IN.uv_MainTex;
			_new_uv = _new_uv - fmod(_new_uv*_RoundFactor, 1)/_RoundFactor;
			o.Albedo = tex2D (_MainTex, _new_uv).rgb;
		}
		ENDCG
	}
	Fallback "Diffuse"
}