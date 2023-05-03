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
	/// Converts buttons on sets into StandardButton buttons.
	/// </summary>
	public class ButtonElement : MonoBehaviour {

		[Tooltip ("The direction that the button should press inward")]
		public Vector3 pressDirection;

		[Tooltip ("Disable this ONLY if the connected actions broadcast their own multiplayer events.")]
		public bool broadcastEvents = true;

		[Space (10)]
		public UnityEvent OnButtonDown;

		public UnityEvent OnButtonUp;

		private Vector3 defaultPosition;
		private Vector3 pressedPosition;

		private void Start () {
			//store current position
			defaultPosition = transform.localPosition;
			pressedPosition = defaultPosition + pressDirection;
		}

		internal void DisplayButtonPosition (bool isPressed) {
			transform.localPosition = isPressed ? pressedPosition : defaultPosition;
		}

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private void OnTriggerEnter (Collider other) {
			CustomTag customTag = other.GetComponent<CustomTag> ();
			if (customTag != null && customTag.tagName == "Hand") {
				OnButtonDown.Invoke ();
			}
		}

		private void OnTriggerExit (Collider other) {
			CustomTag customTag = other.GetComponent<CustomTag> ();
			if (customTag != null && customTag.tagName == "Hand") {
				OnButtonUp.Invoke ();
			}
		}
		
#endif

#if UNITY_EDITOR

		private void OnDrawGizmosSelected () {
			if (pressDirection.magnitude == 0f)
				return;
			if (transform.parent == null)
				return;
			//find face of button
			var corners = new Vector3[4];
			var mf = GetComponentInChildren<MeshFilter> ();
			if (mf != null) {
				//pick corners of bounds based on press direction
				var bounds = mf.sharedMesh.bounds;
				var min = bounds.min;
				var max = bounds.max;
				var localPressDir = mf.transform.InverseTransformVector (transform.parent.TransformVector (pressDirection));
				if (Mathf.Abs (localPressDir.x) > Mathf.Abs (localPressDir.y) && Mathf.Abs (localPressDir.x) > Mathf.Abs (localPressDir.z)) {
					if (localPressDir.x > 0) {
						corners[0] = mf.transform.TransformPoint (new Vector3 (min.x, min.y, min.z));
						corners[1] = mf.transform.TransformPoint (new Vector3 (min.x, max.y, min.z));
						corners[2] = mf.transform.TransformPoint (new Vector3 (min.x, max.y, max.z));
						corners[3] = mf.transform.TransformPoint (new Vector3 (min.x, min.y, max.z));
					} else {
						corners[0] = mf.transform.TransformPoint (new Vector3 (max.x, min.y, min.z));
						corners[1] = mf.transform.TransformPoint (new Vector3 (max.x, max.y, min.z));
						corners[2] = mf.transform.TransformPoint (new Vector3 (max.x, max.y, max.z));
						corners[3] = mf.transform.TransformPoint (new Vector3 (max.x, min.y, max.z));
					}
				} else if (Mathf.Abs (localPressDir.y) > Mathf.Abs (localPressDir.x) && Mathf.Abs (localPressDir.y) > Mathf.Abs (localPressDir.z)) {
					if (localPressDir.y > 0) {
						corners[0] = mf.transform.TransformPoint (new Vector3 (min.x, min.y, min.z));
						corners[1] = mf.transform.TransformPoint (new Vector3 (max.x, min.y, min.z));
						corners[2] = mf.transform.TransformPoint (new Vector3 (max.x, min.y, max.z));
						corners[3] = mf.transform.TransformPoint (new Vector3 (min.x, min.y, max.z));
					} else {
						corners[0] = mf.transform.TransformPoint (new Vector3 (min.x, max.y, min.z));
						corners[1] = mf.transform.TransformPoint (new Vector3 (max.x, max.y, min.z));
						corners[2] = mf.transform.TransformPoint (new Vector3 (max.x, max.y, max.z));
						corners[3] = mf.transform.TransformPoint (new Vector3 (min.x, max.y, max.z));
					}
				} else if (Mathf.Abs (localPressDir.z) > Mathf.Abs (localPressDir.x) && Mathf.Abs (localPressDir.z) > Mathf.Abs (localPressDir.y)) {
					if (localPressDir.z > 0) {
						corners[0] = mf.transform.TransformPoint (new Vector3 (min.x, min.y, min.z));
						corners[1] = mf.transform.TransformPoint (new Vector3 (max.x, min.y, min.z));
						corners[2] = mf.transform.TransformPoint (new Vector3 (max.x, max.y, min.z));
						corners[3] = mf.transform.TransformPoint (new Vector3 (min.x, max.y, min.z));
					} else {
						corners[0] = mf.transform.TransformPoint (new Vector3 (min.x, min.y, max.z));
						corners[1] = mf.transform.TransformPoint (new Vector3 (max.x, min.y, max.z));
						corners[2] = mf.transform.TransformPoint (new Vector3 (max.x, max.y, max.z));
						corners[3] = mf.transform.TransformPoint (new Vector3 (min.x, max.y, max.z));
					}
				}
			} else {
				//no way of getting bounds?
				return;
			}
			//draw face of button at unpressed position
			Gizmos.color = new Color (1f, 1f, 1f);
			Gizmos.DrawLine (corners[0], corners[1]);
			Gizmos.DrawLine (corners[1], corners[2]);
			Gizmos.DrawLine (corners[2], corners[3]);
			Gizmos.DrawLine (corners[3], corners[0]);
			//draw pressed position of button
			var dirOffset = transform.parent.TransformVector (pressDirection);
			Gizmos.color = new Color (0.5f, 1f, 1f, 0.7f);
			Gizmos.DrawLine (corners[0] + dirOffset, corners[1] + dirOffset);
			Gizmos.DrawLine (corners[1] + dirOffset, corners[2] + dirOffset);
			Gizmos.DrawLine (corners[2] + dirOffset, corners[3] + dirOffset);
			Gizmos.DrawLine (corners[3] + dirOffset, corners[0] + dirOffset);
		}

#endif
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (ButtonElement))]
	public class ButtonElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			ButtonElement ce = (ButtonElement) target;

			if (EditorApplication.isPlaying) {
				if (GUILayout.Button ("Fire Button Down Event")) {
					ce.OnButtonDown.Invoke ();
					ce.DisplayButtonPosition (true);
				}

				if (GUILayout.Button ("Fire Button Up Event")) {
					ce.OnButtonUp.Invoke ();
					ce.DisplayButtonPosition (false);
				}
			}
		}
	}

#endif
}
