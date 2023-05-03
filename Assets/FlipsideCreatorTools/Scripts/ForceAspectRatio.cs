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

	//Will force a specified aspect ratio on an attached camera
	//useful for cameras targeting a render texture rather than the screen
	public class ForceAspectRatio : MonoBehaviour {
		public float aspect = 1.4f;

		public void Start () {
			var cam = GetComponent<Camera> ();
			if (cam != null) cam.aspect = aspect;
		}
	}
}