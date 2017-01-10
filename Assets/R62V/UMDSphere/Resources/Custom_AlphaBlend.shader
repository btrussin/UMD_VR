Shader "Custom/Custom_AlphaBlend" {
	Properties {
		_MainTex ("Texture", 2D) = ""
	}
	SubShader{
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Zwrite Off
		Tags {Queue = Transparent}

		BindChannels {
			Bind "vertex", vertex
			Bind "color", color
			Bind "texcoord1", texcoord
		}

		Pass {
			SetTexture[_MainTex]
		}
	}
}
