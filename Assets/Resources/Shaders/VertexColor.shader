// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/VertexColor"
{
	SubShader
    {
		Tags { "RenderType"="Opaque" }
		LOD 200

        Pass
        {
            Cull Front
            Fog { Mode Off }
            ZWrite Off
            ZTest Always
            Blend One Zero
            ColorMask RGBA

		    CGPROGRAM

            #pragma vertex   my_vert
            #pragma fragment my_frag
            #pragma target   3.0

		    struct VertexInput
            {
                float4 pos   : POSITION;
                float4 color : COLOR;
            };

            struct VertexOutput
            {
                float4 pos   : SV_POSITION;
                float4 color : COLOR;
            };

            VertexOutput my_vert( VertexInput vertex_input )
            {
                VertexOutput vertex_output;
                vertex_output.pos   = UnityObjectToClipPos( vertex_input.pos );
                vertex_output.color = vertex_input.color;

                return vertex_output;
            }

            float4 my_frag( VertexOutput vertex_output ) : COLOR
            {
                float4 color = vertex_output.color;

                return color;
            }

            ENDCG
        }
	}
}
