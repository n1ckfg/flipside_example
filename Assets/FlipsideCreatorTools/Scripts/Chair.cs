using System;

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

	public class Chair : MonoBehaviour {
		public Transform seatPosition;

		public Transform leftFootPosition;
		public Transform rightFootPosition;

		public void Sit () {
			//get player from teleportTarget
			//somehow send message to player's body / IK system that it should position
		}

		public void Stand () {
			//get player from teleportTarget
		}

#if UNITY_EDITOR

		// some setup for when you add the component or reset it
		private void Reset () {
			MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();
			Transform attachTo = transform;

			if (meshRenderer != null) {
				// Add a common parent so the mesh can easily be rotated to match the chair
				var parentObj = Instantiate (Resources.Load ("BlankGameObject")) as GameObject;
				parentObj.name = gameObject.name + " Chair";
				parentObj.transform.position = transform.position;
				parentObj.transform.rotation = transform.rotation;

				if (gameObject.transform.parent != null) {
					parentObj.transform.SetParent (gameObject.transform.parent, true);
				}

				gameObject.transform.SetParent (parentObj.transform, true);
				attachTo = parentObj.transform;

				parentObj.AddComponent<Chair> ();

				Selection.activeGameObject = parentObj;

				DestroyImmediate (this);
				return;
			}

			if (seatPosition == null) {
				var seatObj = Instantiate (Resources.Load ("BlankGameObject")) as GameObject;
				seatObj.name = "Seat";
				seatPosition = seatObj.transform;
			}
			seatPosition.SetParent (attachTo, false);
			seatPosition.rotation = Quaternion.LookRotation (new Vector3 (transform.forward.x, 0, transform.forward.z), Vector3.up);

			if (leftFootPosition == null) {
				var leftFootObj = Instantiate (Resources.Load ("BlankGameObject")) as GameObject;
				leftFootObj.name = "Left Foot";
				leftFootPosition = leftFootObj.transform;
			}
			leftFootPosition.SetParent (attachTo, false);
			leftFootPosition.rotation = seatPosition.rotation;

			if (rightFootPosition == null) {
				var rightFootObj = Instantiate (Resources.Load ("BlankGameObject")) as GameObject;
				rightFootObj.name = "Right Foot";
				rightFootPosition = rightFootObj.transform;
			}
			rightFootPosition.SetParent (attachTo, false);
			rightFootPosition.rotation = seatPosition.rotation;

			//find bounding box
			var bounds = new Bounds (transform.position, Vector3.zero);
			var mrs = GetComponentsInChildren<MeshRenderer> ();
			foreach (var mr in mrs) {
				bounds.Encapsulate (mr.bounds);
			}

			//position seat and feet according to that bounding box
			seatPosition.position = bounds.center;
			var seatHeight = bounds.center.y - bounds.min.y;
			var forwardOffset = transform.forward;
			forwardOffset.y = 0f;
			var rightOffset = Quaternion.Euler (0, -30f, 0) * forwardOffset * seatHeight * 0.55f;
			leftFootPosition.position = bounds.center + new Vector3 (rightOffset.x, -seatHeight, rightOffset.z);
			var leftOffset = Quaternion.Euler (0, 30f, 0) * forwardOffset * seatHeight * 0.55f;
			rightFootPosition.position = bounds.center + new Vector3 (leftOffset.x, -seatHeight, leftOffset.z);

			var tt = GetComponent<TeleportTarget> ();
			if (tt == null)
				tt = seatPosition.gameObject.AddComponent<TeleportTarget> ();

			tt.teleportEvent = new UnityEvent ();
			UnityEditor.Events.UnityEventTools.AddPersistentListener (tt.teleportEvent, Sit);
			tt.leavingEvent = new UnityEvent ();
			UnityEditor.Events.UnityEventTools.AddPersistentListener (tt.leavingEvent, Stand);
			tt.positionOffset = new Vector3 (0, (bounds.min.y - transform.position.y) * transform.lossyScale.y, 0);
		}

		private void OnDrawGizmos () {
			if (seatPosition == null || rightFootPosition == null || leftFootPosition == null) {
				return;
			}

			Gizmos.color = new Color (0.3f, 0.9f, 0.9f);
			if (Vector3.Angle (rightFootPosition.forward + leftFootPosition.forward, seatPosition.forward) > 20f) {
				UnityEditor.Handles.Label (seatPosition.position, "Seat position should face same direction as feet");
				Gizmos.color = new Color (1f, 0.1f, 0.1f);
			}
			Gizmos.DrawCube (seatPosition.position, Vector3.one * 0.1f);
			var kneePosition = rightFootPosition.position;
			kneePosition.y = seatPosition.position.y;
			Gizmos.DrawLine (seatPosition.position, kneePosition);
			Gizmos.DrawLine (kneePosition, rightFootPosition.position);
			Gizmos.DrawLine (rightFootPosition.position, rightFootPosition.position + rightFootPosition.forward * 0.2f);
			kneePosition = leftFootPosition.position;
			kneePosition.y = seatPosition.position.y;
			Gizmos.DrawLine (seatPosition.position, kneePosition);
			Gizmos.DrawLine (kneePosition, leftFootPosition.position);
			Gizmos.DrawLine (leftFootPosition.position, leftFootPosition.position + leftFootPosition.forward * 0.2f);
		}

#endif
	}
}