/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
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
	/// Resets the teleport position of any TeleportObjectTo component on contact.
	/// </summary>
	public class TeleportObjectsOnContact : MonoBehaviour {

		private void OnTriggerEnter (Collider other) {
			TeleportObjectTo teleporter = other.GetComponent<TeleportObjectTo> ();
			if (teleporter != null) teleporter.Teleport ();
		}

		private void OnCollisionEnter (Collision collision) {
			TeleportObjectTo teleporter = collision.gameObject.GetComponent<TeleportObjectTo> ();
			if (teleporter != null) teleporter.Teleport ();
		}
	}
}