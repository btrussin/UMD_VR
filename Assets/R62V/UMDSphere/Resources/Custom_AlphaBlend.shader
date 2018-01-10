Shader "Custom/Custom_AlphaBlend" {
	Properties {
		_MainTex("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
	}

	SubShader{
		Tags {Queue = Transparent RenderType = TransparentCutout}
		BlendOp Max
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
