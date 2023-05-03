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
using UnityEngine.Events;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	/// <summary>
	/// Triggers Unity events on colliders in sets.
	/// </summary>
	public class ColliderElement : MonoBehaviour {

		public enum CollidesWith {
			Everything,
			Hands,
			IndexFinger,
			CustomTag,
			Objects
		}

		public CollidesWith triggerFor = CollidesWith.Hands;

		[Tooltip ("Attach a CustomTag component to objects and make sure this value matches")]
		public string customTag = "";

		[Tooltip ("A list of specific objects to collide with")]
		public GameObject[] objectList = new GameObject[0];

		[Space (10)]
		public UnityEvent OnEnter = new UnityEvent ();
		public UnityEvent OnEnterLocalOnly = new UnityEvent (); // For local-only effects like haptics

		public UnityEvent OnExit = new UnityEvent ();
		public UnityEvent OnExitLocalOnly = new UnityEvent (); // For local-only effects like haptics

		private string fingerTag = "Finger";
		private string handTag = "Hand";

		private void Awake () {
			Collider collider = GetComponent<Collider> ();
			if (!collider.isTrigger) collider.isTrigger = true;
		}

		public void DisableCollider () {
			enabled = false;
		}

		protected bool ShouldTrigger (Collider other) {
			if (!enabled) return false; // Don't trigger if ColliderElement is diabled

			CustomTag ct;

			switch (triggerFor) {
				case CollidesWith.Everything:
					break;

				case CollidesWith.Hands:
					//if (!other.CompareTag (handTag)) return false;
					ct = other.GetComponent<CustomTag> ();
					if (ct == null) return false;
					if (ct.tagName != handTag) return false;
					break;

				case CollidesWith.IndexFinger:
					//if (!other.CompareTag (fingerTag)) return false;
					ct = other.GetComponent<CustomTag> ();
					if (ct == null) return false;
					if (ct.tagName != fingerTag) return false;
					break;

				case CollidesWith.CustomTag:
					ct = other.GetComponent<CustomTag> ();
					if (ct == null) return false;
					if (ct.tagName != customTag) return false;
					break;

				case CollidesWith.Objects:
					foreach (var obj in objectList) {
						if (other.gameObject == obj) return true;
					}
					return false;
			}
			return true;
		}

		private void OnTriggerEnter (Collider other) {
			if (!ShouldTrigger (other)) return;
			OnEnter.Invoke ();
			OnEnterLocalOnly.Invoke ();
		}

		private void OnTriggerExit (Collider other) {
			if (!ShouldTrigger (other)) return;
			OnExit.Invoke ();
			OnExitLocalOnly.Invoke ();
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (ColliderElement))]
	public class ColliderElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			ColliderElement ce = (ColliderElement) target;

			if (GUILayout.Button ("Fire Enter Event")) {
				ce.OnEnter.Invoke ();
			}

			if (GUILayout.Button ("Fire Enter Local-Only Event")) {
				ce.OnEnterLocalOnly.Invoke ();
			}

			if (GUILayout.Button ("Fire Exit Event")) {
				ce.OnExit.Invoke ();
			}

			if (GUILayout.Button ("Fire Exit Local-Only Event")) {
				ce.OnExitLocalOnly.Invoke ();
			}
		}
	}

#endif
}