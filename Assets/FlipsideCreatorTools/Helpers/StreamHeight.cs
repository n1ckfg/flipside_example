using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flipside.Helpers {

	/// <summary>
	/// Simulates height-adjustable streams of water.
	///
	/// Adjust the height of a material by adjusting its _Cutoff value.
	///
	/// Will also adjust the position of a particle system or other effect
	/// to follow the base position.
	/// </summary>
	public class StreamHeight : MonoBehaviour {
		public Transform particleEffect;

		private MeshRenderer meshRenderer;

		private void Awake () {
			meshRenderer = GetComponent<MeshRenderer> ();
		}

		private void OnTriggerStay (Collider other) {
			UpdateHeight (GetHeight (other));
		}

		private void OnTriggerExit (Collider other) {
			UpdateHeight (0);
		}

		private float GetHeight (Collider collider) {
			return collider.transform.position.y + collider.bounds.size.y;
		}

		private void UpdateHeight (float newHeight) {
			Vector3 newPosition = new Vector3 (transform.position.x, newHeight, transform.position.z);

			if (meshRenderer != null) {
				newHeight /= transform.localScale.y;
				meshRenderer.material.SetFloat ("_Cutoff", newHeight);
			}

			if (particleEffect != null) {
				particleEffect.position = newPosition;
			}
		}
	}
}