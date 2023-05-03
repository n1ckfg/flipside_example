/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flipside.Sets {
	/// <summary>
	/// Adds a missing material setter so we can auto-generate a MethodInfo
	/// object to pass to Delegate.CreateDelegate() then on to
	/// UnityEventTools.AddPersistentListener().
	/// </summary>
	public class MaterialSettings : MonoBehaviour {
		MeshRenderer meshRenderer;
		public int materialIndex = 0;

		private void Awake () {
			meshRenderer = GetComponent<MeshRenderer> ();
		}

		public void Enable () {
			meshRenderer.enabled = true;
		}

		public void Disable () {
			meshRenderer.enabled = false;
		}

		public void SetMaterial (Material material) {
			if (materialIndex == 0) {
				meshRenderer.material = material;
			} else {
				meshRenderer.materials[materialIndex] = material;
			}
		}

		public void SetSharedMaterial (Material material) {
			if (materialIndex == 0) {
				meshRenderer.sharedMaterial = material;
			} else {
				meshRenderer.sharedMaterials[materialIndex] = material;
			}
		}

		public void SetMaterialIndex (int index) {
			materialIndex = index;
		}
	}
}
