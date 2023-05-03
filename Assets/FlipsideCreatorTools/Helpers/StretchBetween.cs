/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;

namespace Flipside.Avatars {

	/// <summary>
	/// Stretch an object along its Y axis between a start and end transform.
	/// If a second end transform is added, the end can be specified as an
	/// offset position between the two end positions. This can be useful for
	/// finding the sweet spot between two body parts.
	/// </summary>
	public class StretchBetween : MonoBehaviour {
		public Transform startTransform;
		public Transform endTransform;
		public Transform endTransformB;
		public float endOffset = 0f;

		private void Update () {
			var start = startTransform.position;
			var end = (endTransformB == null)
				? endTransform.position
				: Vector3.Lerp (endTransform.position, endTransformB.position, endOffset);

			transform.position = Vector3.Lerp (start, end, 0.5f);
			transform.up = start - end;
			var y = Vector3.Distance (start, end) / 2f / transform.parent.lossyScale.y;
			if (double.IsNaN (y)) y = 0f;
			transform.localScale = new Vector3 (transform.localScale.x, y, transform.localScale.z);
		}
	}
}