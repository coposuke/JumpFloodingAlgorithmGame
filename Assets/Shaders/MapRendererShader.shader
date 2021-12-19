Shader "Unlit/MapRendererShader"
{
    Properties
    {
		_Ratio ("Ratio", Range(0.0, 1.0)) = 0.5
		_MainTex ("Texture", 2D) = "white" {}
        _OutputTex ("Output", 2D) = "white" {}
        _OutputNormalTex ("OutputNormal", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Name "Rendering Map(MeshRenderer)"

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			float _Ratio;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _OutputTex;
			float4 _OutputTex_ST;
			sampler2D _OutputNormalTex;
			float4 _OutputNormalTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				float dist = tex2D(_OutputTex, i.uv).b;
				float grad = dist * 6.28218 * 30.0 - _Time.y * 10.0;
				float3 output = lerp(float3(1,1,1), cos(float3(0,2,4) + grad) * 0.15 + 0.8, step(0.001, dist));

				fixed4 col = lerp(
					tex2D(_MainTex, i.uv),
					fixed4(output, 0.0),
					_Ratio
				);

				return col;
            }
            ENDCG
        }
    }
}
