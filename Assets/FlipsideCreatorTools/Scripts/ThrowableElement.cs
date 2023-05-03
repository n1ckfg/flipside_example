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

namespace Flipside.Sets {

	/// <summary>
	/// Throwable props can be thrown and hit TargetElement objects.
	/// </summary>
	[RequireComponent (typeof (PropElement))]
	public class ThrowableElement : MonoBehaviour {

		[Tooltip ("Should it auto-destroy on contact?")]
		public bool destroyOnContact = true;

		[Tooltip ("Points won when you hit a target")]
		public int addPointsOnTargetHit = 1;

		[Tooltip ("Points won when you hit another player")]
		public int addPointsOnPlayerHit = 1;

		[Tooltip ("Points to subtract from the player that was hit")]
		public int subPointsOnPlayerHit = 1;

		[Space (10)]
		public UnityEvent OnHit = new UnityEvent ();
		public UnityEvent OnHitTarget = new UnityEvent ();
		public UnityEvent OnHitPlayer = new UnityEvent ();

		private string thrownBy = "";
		private PropElement prop;
		private float waitToCollide = 0f;
		private Rigidbody rb;

		private void Awake () {
			prop = GetComponent<PropElement> ();
			rb = GetComponent<Rigidbody> ();
		}

		private void OnEnable () {
			waitToCollide = Time.time + 0.01f;

			prop.OnBeginInteraction.AddListener (Grabbed);
		}

		private void OnDestroy () {
			prop.OnBeginInteraction.RemoveListener (Grabbed);
		}

		public string ThrownBy () {
			return thrownBy;
		}

		/// <summary>
		/// Manually assign a user by their number in the user order.
		/// </summary>
		/// <param name="userNumber">User number between 1-5 (up to 5 users).</param>
		public void AssignUser (int userNumber) {
			if (userNumber < 1 || userNumber > 5) {
				Debug.LogWarning ("User number must be between 1-5.");
				return;
			}

			thrownBy = "Player " + userNumber.ToString ();
		}

		private void Grabbed () {
			thrownBy = "Player 1";
		}

		private void OnCollisionEnter (Collision col) {
			if (Time.time < waitToCollide || thrownBy == "") return;

			if (addPointsOnTargetHit != 0) {
				TargetElement target = col.gameObject.GetComponent<TargetElement> () ?? col.gameObject.GetComponentInParent<TargetElement> ();
				if (target != null) {
					target.Hit (thrownBy, addPointsOnTargetHit);

					OnHitTarget.Invoke ();
				}
			}

			// Note: We don't simulate hitting players here since Creator Tools isn't multiplayer

			OnHit.Invoke ();

			if (destroyOnContact) {
				Debug.Log ("Destroying " + name);
				Destroy (gameObject);
			}
		}
	}
}