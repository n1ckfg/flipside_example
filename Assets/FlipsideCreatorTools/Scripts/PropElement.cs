/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

using Flipside.Helpers;

#endif

namespace Flipside.Sets {

	[Serializable]
	public class PropElement : MonoBehaviour {
		public bool gravity = true;

		public UnityEvent OnBeginInteraction = new UnityEvent ();

		public UnityEvent OnUseButtonDown = new UnityEvent ();

		public UnityEvent OnUseButtonUp = new UnityEvent ();

		public UnityEvent OnEndInteraction = new UnityEvent ();

		private Rigidbody rb;

		private void Awake () {
			rb = GetComponent<Rigidbody> ();
			if (gravity == true) {
				if (rb == null) {
					rb = gameObject.AddComponent<Rigidbody> ();
					rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
				}
				rb.useGravity = true;
			}
		}

		private void Reset () {
			var navMod = GetComponent<NavMeshModifier> ();
			if (navMod == null) {
				navMod = gameObject.AddComponent<NavMeshModifier> ();
			}
			navMod.ignoreFromBuild = true;
		}

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private HandController attachedHand;

		public void AttachToHand (HandController hand) {
			if (attachedHand != null) DetachHand ();

			attachedHand = hand;
			attachedHand.OnTriggerDown.AddListener (HandleTriggerDown);
			attachedHand.OnTriggerUp.AddListener (HandleTriggerUp);

			transform.SetParent (attachedHand.transform);
			if (rb != null) rb.isKinematic = true;

			if (OnBeginInteraction != null) OnBeginInteraction.Invoke ();
		}

		public void DetachHand () {
			if (rb != null) rb.isKinematic = false;
			transform.SetParent (null);

			if (attachedHand != null) {
				attachedHand.OnTriggerDown.RemoveListener (HandleTriggerDown);
				attachedHand.OnTriggerUp.RemoveListener (HandleTriggerUp);
				attachedHand = null;
			}

			if (OnEndInteraction != null) OnEndInteraction.Invoke ();
		}

		public HandController GetAttachedHand () {
			return attachedHand;
		}

		public void ApplyForce (Vector3 velocity, Vector3 angularVelocity) {
			if (rb == null) return;

			rb.velocity = velocity;
			rb.angularVelocity = angularVelocity;
		}

		private void HandleTriggerDown () {
			if (OnUseButtonDown != null) OnUseButtonDown.Invoke ();
		}

		private void HandleTriggerUp () {
			if (OnUseButtonUp != null) OnUseButtonUp.Invoke ();
		}

#endif
	}
}