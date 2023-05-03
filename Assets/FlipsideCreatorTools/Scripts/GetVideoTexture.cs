/**
 * Copyright (c) 2022 Flipside XR Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@flipsidexr.com
 * Website: https://www.flipsidexr.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flipside.Sets {
	//will acquire a texture from an attached VideoElement and assign it to the targetMaterial
	public class GetVideoTexture : MonoBehaviour {
		public Material targetMaterial;

		public string textureName = "_MainTex";

		private void Start () {
		}

		private void ChangeTexture (Texture newTexture) {
		}
	}
}
