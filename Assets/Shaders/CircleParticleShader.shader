Shader "Unlit/CircleParticleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		CGPROGRAM
		#pragma surface surf Standard addshadow fullforwardshadows
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup
		
		struct ParticleData
		{
			float active;
			float radius;
			float2 position;
			float2 velocity;
		};

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<ParticleData> _ParticleData;
		#endif

		void setup()
		{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			const float SIZE = 5.0;
			ParticleData data = _ParticleData[unity_InstanceID];
			unity_ObjectToWorld._11_21_31_41 = float4(SIZE, 0, 0, 0);
			unity_ObjectToWorld._12_22_32_42 = float4(0, SIZE, 0, 0);
			unity_ObjectToWorld._13_23_33_43 = float4(0, 0, SIZE, 0);
			unity_ObjectToWorld._14_24_34_44 = float4(data.position, 0, 1);
			unity_WorldToObject = unity_ObjectToWorld;
			unity_WorldToObject._14_24_34 *= -1;
			unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
			#endif
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			ParticleData data = _ParticleData[unity_InstanceID];
			clip(data.active - 0.5);
			#endif

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = c.a;
		}
		ENDCG
	
	}
	FallBack "Diffuse"
}
