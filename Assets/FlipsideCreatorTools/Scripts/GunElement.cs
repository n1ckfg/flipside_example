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

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	/// <summary>
	/// Guns can shoot BulletElement objects.
	/// </summary>
	[RequireComponent (typeof (PropElement))]
	public class GunElement : MonoBehaviour {

		[Tooltip ("A prefab with a BulletElement component attached")]
		public BulletElement bullet;

		[Tooltip ("The point on the gun at which to instantiate the bullet and fire along its forward")]
		public Transform fireFrom;

		[Tooltip ("The velocity to apply to the bullet")]
		public float velocity = 100f;

		[Tooltip ("Number of rounds in a clip, or -1 for infinite ammo")]
		public int bulletLimit = -1;

		[Space (10)]
		public UnityEvent OnFire = new UnityEvent ();

		[Space (10)]
		public UnityEvent OnEmptyFire = new UnityEvent ();

		private PropElement prop;
		private string grabbedBy = "";
		private int bulletsUsed = 0;

		private int bulletPoolLength = 20;

		private BulletElement[] bulletPool;

		private void Awake () {
			prop = GetComponent<PropElement> ();

			bulletPool = new BulletElement[bulletPoolLength];

			for (int i = 0; i < bulletPool.Length; i++) {
				BulletElement inst = (BulletElement) Instantiate (bullet);
				inst.gameObject.SetActive (false);
				bulletPool[i] = inst;
			}
		}

		private void OnEnable () {
			prop.OnBeginInteraction.AddListener (Grabbed);
			prop.OnUseButtonDown.AddListener (Fire);
			bulletsUsed = 0;
		}

		private void OnDisable () {
			prop.OnBeginInteraction.RemoveListener (Grabbed);
			prop.OnUseButtonDown.RemoveListener (Fire);
		}

		private void Grabbed () {
			grabbedBy = "Player 1";
		}

		public void Fire () {
			if (bulletLimit > 0 && bulletsUsed >= bulletLimit) {
				Debug.Log ("No ammo :(");
				OnEmptyFire.Invoke ();
				return;
			}

#if UNITY_EDITOR || FLIPSITE_CREATOR_TOOLS
			if (prop != null) {
				var hand = prop.GetAttachedHand ();
				if (hand != null) hand.TriggerHaptics (0.3f);
			}
#endif

			BulletElement b = GetBulletFromPool ();
			b.ShotBy (grabbedBy);
			b.FireFrom (fireFrom, velocity);
			Debug.Log ("Bang!");
			bulletsUsed++;
			OnFire.Invoke ();
		}

		public void Reload () {
			Debug.Log ("Gun reloaded");
			bulletsUsed = 0;
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

			grabbedBy = "Player " + userNumber.ToString ();
		}

		private BulletElement GetBulletFromPool () {
			for (int i = 0; i < bulletPoolLength; i++) {
				if (!bulletPool[i].gameObject.activeInHierarchy) {
					return bulletPool[i];
				}
			}

			// Take the first one anyway
			bulletPool[0].gameObject.SetActive (false);
			return bulletPool[0];
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (GunElement))]
	public class GunElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			GunElement gun = (GunElement) target;

			if (GUILayout.Button ("Fire Gun")) {
				gun.Fire ();
			}

			if (GUILayout.Button ("Reload Gun")) {
				gun.Reload ();
			}
		}
	}

#endif
}
