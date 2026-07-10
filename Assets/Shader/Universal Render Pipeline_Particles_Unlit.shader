Shader "Universal Render Pipeline/Particles/Unlit"
{
	Properties
	{
		_BaseMap ("Base Map", 2D) = "white" {}
		_BaseColor ("Base Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+101"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

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

			sampler2D _BaseMap;
			float4 _BaseMap_ST;
			fixed4 _BaseColor;

			v2f vert(appdata input)
			{
				v2f output;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				output.color = input.color * _BaseColor;
				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				return tex2D(_BaseMap, input.uv) * input.color;
			}
			ENDCG
		}
	}
}
