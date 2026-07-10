Shader "Toon/Ice Effect" {
	Properties {
		_TColor ("Top Color", Vector) = (0.64,0.94,0.64,1)
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		_BottomColor ("Bottom Color", Vector) = (0.23,0,0.95,1)
		_RimBrightness ("Rim Brightness", Range(3, 4)) = 3.2
		[Toggle] _DynamicEmissionLM ("Dynamic Emission (Lightmapper)", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
}