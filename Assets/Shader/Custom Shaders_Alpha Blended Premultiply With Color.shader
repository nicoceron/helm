Shader "Custom Shaders/Alpha Blended Premultiply With Color"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+100"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			v2f vert(appdata input)
			{
				v2f output;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.color = input.color * _Color;
				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, input.uv) * input.color;
				color.rgb *= color.a;
				return color;
			}
			ENDCG
		}
	}
}
