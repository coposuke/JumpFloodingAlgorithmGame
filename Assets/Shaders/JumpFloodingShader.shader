Shader "Unlit/JumpFloodingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		CGINCLUDE
		float3 jumpFlooding_Compare(float2 seed, float2 self, float2 currentSeed, float currentDist)
		{
			float dist = distance(seed, self);

			bool isEmpty = 0.0 == seed.x + seed.y;
			bool isFarther = currentDist < dist;

			if (isEmpty || isFarther)
				return float3(currentSeed, currentDist);
			else
				return float3(seed, dist);
		}

		float3 jumpFlooding(in float2 fragCoord, in sampler2D channel, in float2 onePixel, float stepLength)
		{
			float2 self = fragCoord;
			float3 data = float3(0.0, 0.0, 1e+5);

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					float2 neighbor = self + float2(x, y) * onePixel * stepLength;
					float4 pointPosition = tex2D(channel, frac(neighbor));
					data = jumpFlooding_Compare(pointPosition.xy, self, data.xy, data.z);
				}
			}

			return data;
		}
		ENDCG
			
        Pass
        {
			Name "Convert To Seed"

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

			float _StepLength;

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float4 color = tex2D(_MainTex, i.uv);
				float valid = step(1e-3, color.a * (color.x + color.y));

				float3x3 sobelFilter = float3x3(-1, -2, -1, 0, 0, 0, 1, 2, 1);
				float2 sobel = float2(0.0, 0.0);

				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						float c = tex2D(_MainTex, i.uv + float2(x, y) * _MainTex_TexelSize.xy).r;
						sobel.x += c * sobelFilter[x + 1][y + 1];
						sobel.y += c * sobelFilter[y + 1][x + 1];
					}
				}

				float edge = step(1e-3, max(abs(sobel.x), abs(sobel.y)));
				return float4(edge * i.uv, 0.0, valid);
            }
            ENDCG
        }

        Pass
        {
			Name "Jump Flooding Algorythm (Multiple Passes)"

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

			float _StepLength;

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float3 data = jumpFlooding(i.uv, _MainTex, _MainTex_TexelSize.xy, _StepLength);
				return float4(data, tex2D(_MainTex, i.uv).a);
            }
            ENDCG
        }
			
        Pass
        {
			Name "Convert To Normal"

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

			float _StepLength;

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float4 color = tex2D(_MainTex, i.uv);
                float2 seed = color.xy;
                float2 normal = normalize(i.uv - seed) * 0.5 + 0.5;
                return float4(normal, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
