/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
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

	public class TeleportObjectTo : MonoBehaviour {

		[Tooltip ("The position to teleport the object to.")]
		public Transform teleportPosition;

		[Tooltip ("An external object to teleport. If unset, it uses the object its attached to.")]
		public Transform objectToMove;

		public bool resetVelocity = true;

		private Rigidbody _rb;
		private bool rbChecked = false;

		private Rigidbody rb {
			get {
				if (!rbChecked) {
					_rb = objectToMove.gameObject.GetComponent<Rigidbody> ();
					rbChecked = true;
				}
				return _rb;
			}
		}

		private void Awake () {
			if (objectToMove == null) objectToMove = transform;
		}

		public void Teleport () {
			if (teleportPosition == null) {
				Debug.LogWarning ("Please specify a transform in the teleport position property to teleport the object to.");
				return;
			}

			if (resetVelocity && rb != null) {
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}

			objectToMove.position = teleportPosition.position;
			objectToMove.rotation = teleportPosition.rotation;
		}
	}
}