// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Common/DT2D"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SecondTex("Second Texture", 2D) = "white" {}
		_InTex("Input Texture", 2D) = "white" {}
		_WaveTex("Wave Texture", 2D) = "white" {}
		_EdgeTex("Wave Texture", 2D) = "white" {}
		_Tween("Tween", Range(0, 1)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _SecondTex;
			sampler2D _InTex;
			sampler2D _WaveTex;
			sampler2D _EdgeTex;

			float4 _MainTex_TexelSize;
			float4 _WaveTex_ST; 

			float _Tween;

			float4 frag(v2f i) : SV_Target
			{
				float4 color1 = tex2D(_MainTex, i.uv); //
				float4 color2 = tex2D(_SecondTex, i.uv); //
				float4 color3 = tex2D(_InTex, i.uv); //
				float4 color4 = tex2D(_WaveTex, i.uv + _WaveTex_ST.zw);
				float4 color5 = tex2D(_EdgeTex, i.uv);
				
				float2 disp = float2 ((color4.x - 0.5) * (i.uv.x - 0.5 +_WaveTex_ST.z ), (color4.x-0.5) * (i.uv.y - 0.5 +_WaveTex_ST.w) ) ;

				disp = (disp* _Tween*(color5.x));

				color1 = tex2D(_MainTex, i.uv - disp);
				color2 = tex2D(_SecondTex, i.uv - disp);
				color3 = tex2D(_InTex, i.uv - disp);

				float4  color = lerp(color1, color2, color3.z);
				return color;
			}
			ENDCG
		}
	}
}