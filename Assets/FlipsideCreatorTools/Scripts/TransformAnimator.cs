/**
 * Copyright (c) 2021 The Campfire Union Inc - All Rights Reserved.
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
using UnityEngine.Events;

namespace Flipside.Helpers {

	/// <summary>
	/// Provides methods for applying float values to individual elements of a Transform, adjusting
	/// for different input and output value ranges.
	/// </summary>
	public class TransformAnimator : MonoBehaviour {
		public Vector2 inputRange = new Vector2 (0f, 1f);
		public Vector2 outputRange = new Vector2 (0f, 1f);

		public void LocalPositionX (float val) {
			transform.localPosition = new Vector3 (ApplyRange (val), transform.localPosition.y, transform.localPosition.z);
		}

		public void LocalPositionY (float val) {
			transform.localPosition = new Vector3 (transform.localPosition.x, ApplyRange (val), transform.localPosition.z);
		}

		public void LocalPositionZ (float val) {
			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, ApplyRange (val));
		}

		public void WorldPositionX (float val) {
			transform.position = new Vector3 (ApplyRange (val), transform.position.y, transform.position.z);
		}

		public void WorldPositionY (float val) {
			transform.position = new Vector3 (transform.position.x, ApplyRange (val), transform.position.z);
		}

		public void WorldPositionZ (float val) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, ApplyRange (val));
		}

		public void LocalEulerX (float val) {
			transform.localEulerAngles = new Vector3 (ApplyRange (val), transform.localEulerAngles.y, transform.localEulerAngles.z);
		}

		public void LocalEulerY (float val) {
			transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, ApplyRange (val), transform.localEulerAngles.z);
		}

		public void LocalEulerZ (float val) {
			transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, ApplyRange (val));
		}

		public void LocalScale (float val) {
			float adjusted = ApplyRange (val);
			transform.localScale = new Vector3 (adjusted, adjusted, adjusted);
		}

		public void LocalScaleX (float val) {
			transform.localScale = new Vector3 (ApplyRange (val), transform.localScale.y, transform.localScale.z);
		}

		public void LocalScaleY (float val) {
			transform.localScale = new Vector3 (transform.localScale.x, ApplyRange (val), transform.localScale.z);
		}

		public void LocalScaleZ (float val) {
			transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y, ApplyRange (val));
		}

		private float ApplyRange (float val) {
			float normal = Mathf.InverseLerp (inputRange.x, inputRange.y, val);
			return Mathf.Lerp (outputRange.x, outputRange.y, normal);
		}
	}
}