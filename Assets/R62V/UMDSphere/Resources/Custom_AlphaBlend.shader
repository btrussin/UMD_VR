Shader "Custom/Custom_AlphaBlend" {
	Properties {
		_MainTex("Base (RGB) Transparent (A)", 2D) = "white" {}
	}

	SubShader{
		Tags {Queue = Transparent}
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		Fog{ Mode Off }
		ZWrite Off
		Cull Off
		ColorMaterial AmbientAndDiffuse

		Pass {
			SetTexture[_MainTex] {
				combine texture * primary
			}
		}
	}
}
