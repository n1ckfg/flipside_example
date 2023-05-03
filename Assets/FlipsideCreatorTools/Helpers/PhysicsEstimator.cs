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
using Unity.Labs.SuperScience;

namespace Flipside.Helpers {
	/// <summary>
	/// Tracks the movement of an object and estimates its velocity so you can
	/// call ReleaseObject() to have that velocity applied when it stops being
	/// kinematic or controlled by an animation.
	/// 
	/// Note: If the object is controlled by an animation, disable the animator
	/// before calling ReleaseObject().
	/// </summary>
	[RequireComponent (typeof (Rigidbody))]
	public class PhysicsEstimator : MonoBehaviour {
		private PhysicsTracker tracker;
		private Rigidbody rb;

		private void Awake () {
			tracker = new PhysicsTracker ();
			rb = GetComponent<Rigidbody> ();
		}

		private void Update () {
			tracker.Update (transform.position, transform.rotation, Time.smoothDeltaTime);
		}

		public void ReleaseObject () {
			rb.velocity = tracker.Velocity;
			rb.angularVelocity = tracker.AngularVelocity;
		}
	}
}