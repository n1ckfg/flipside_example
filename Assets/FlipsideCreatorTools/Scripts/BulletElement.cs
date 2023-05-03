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
using UnityEngine.Events;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

using Flipside.Helpers;

#endif

namespace Flipside.Sets {

	/// <summary>
	/// Bullets can be shot by GunElement props and hit players and TargetElement objects.
	/// </summary>
	public class BulletElement : MonoBehaviour {

		[Tooltip ("Should it auto-destroy on contact?")]
		public bool destroyOnContact = true;

		[Tooltip ("Points won when you hit a target")]
		public int addPointsOnTargetHit = 1;

		[Tooltip ("Points won when you hit another player")]
		public int addPointsOnPlayerHit = 1;

		[Tooltip ("Points to subtract from the player that was hit")]
		public int subPointsOnPlayerHit = 1;

		[Space (10)]
		public UnityEvent OnFired = new UnityEvent ();

		public UnityEvent OnHit = new UnityEvent ();
		public UnityEvent OnHitTarget = new UnityEvent ();
		public UnityEvent OnHitPlayer = new UnityEvent ();

		private string shotBy = "";
		private float waitToCollide = 0f;
		private Rigidbody rb;

		private void Awake () {
			rb = GetComponent<Rigidbody> ();
		}

		private void OnEnable () {
			waitToCollide = Time.time + 0.02f;
		}

		public void ShotBy (string userId) {
			Debug.Log ("Bullet shot by " + userId);
			shotBy = userId;
		}

		public string ShotBy () {
			return shotBy;
		}

		public void FireFrom (Transform pos, float velocity) {
			transform.position = pos.position;
			transform.rotation = pos.rotation;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			gameObject.SetActive (true);
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.AddForce (transform.forward * velocity, ForceMode.VelocityChange);
			OnFired.Invoke ();
		}

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private void OnCollisionEnter (Collision col) {
			if (Time.time < waitToCollide || shotBy == "") return;

			if (addPointsOnTargetHit != 0) {
				TargetElement target = col.gameObject.GetComponent<TargetElement> () ?? col.gameObject.GetComponentInParent<TargetElement> ();
				if (target != null) {
					target.Hit (shotBy, addPointsOnTargetHit);

					OnHitTarget.Invoke ();
				}
			}

			// Note: We don't simulate hitting players here since Creator Tools isn't multiplayer

			OnHit.Invoke ();

			if (destroyOnContact) {
				// Return to object pool
				gameObject.SetActive (false);
			}
		}

#endif
	}
}