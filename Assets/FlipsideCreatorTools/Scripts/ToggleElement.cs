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
	/// Like ColliderElement but keeps track of its on/off state and alternates
	/// between triggering OnActivated and OnDeactivated Unity events in sets.
	/// </summary>
	public class ToggleElement : ColliderElement {

		[Space (10)]
		[Tooltip ("Should the toggle start deactivated (unchecked) or activated (checked)?")]
		public bool initialState = false;

		[Space (10)]
		public UnityEvent OnActivated = new UnityEvent ();

		public UnityEvent OnDeactivated = new UnityEvent ();

		private bool state = false;

		private void Awake () {
			Collider collider = GetComponent<Collider> ();
			if (!collider.isTrigger) collider.isTrigger = true;

			ResetState ();
		}

		public void ResetState () {
			state = initialState;
		}

		public void ToggleState () {
			state = !state;

			if (state) {
				OnActivated.Invoke ();
			} else {
				OnDeactivated.Invoke ();
			}
		}

		public void ToggleOn () {
			if (!state) {
				state = !state;
				OnActivated.Invoke ();
			}
		}

		public void ToggleOff () {
			if (state) {
				state = !state;
				OnDeactivated.Invoke ();
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (!ShouldTrigger (other)) return;
			OnEnter.Invoke ();

			ToggleState ();
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (ToggleElement))]
	public class ToggleElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			ToggleElement ce = (ToggleElement) target;

			if (GUILayout.Button ("Fire Enter Event")) {
				ce.OnEnter.Invoke ();
			}

			if (GUILayout.Button ("Fire Exit Event")) {
				ce.OnExit.Invoke ();
			}

			if (GUILayout.Button ("Fire Activated Event")) {
				ce.OnActivated.Invoke ();
			}

			if (GUILayout.Button ("Fire Deactivated Event")) {
				ce.OnDeactivated.Invoke ();
			}
		}
	}

#endif
}