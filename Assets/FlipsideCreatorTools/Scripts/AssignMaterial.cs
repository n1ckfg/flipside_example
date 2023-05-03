/**
 * Copyright (c) 2021 Flipside XR Inc - All Rights Reserved.
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
	//will assign a material to a list of renderers
	public class AssignMaterial : MonoBehaviour {

		//What renderers should receive this material?
		public Renderer[] targets;

		public void Assign (Material newMaterial) {
			if (targets == null || newMaterial == null)
				return;
			foreach (var target in targets) {
				target.sharedMaterial = newMaterial;
			}
		}
	}
}
