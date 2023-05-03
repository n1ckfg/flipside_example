using System;

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
using Flipside.Helpers;

#endif

namespace Flipside.Sets {

	public class TeleportTarget : MonoBehaviour {
#if UNITY_EDITOR

		[CustomEditor (typeof (TeleportTarget), true)]
		[CanEditMultipleObjects]
		[ExecuteInEditMode]
		public class TeleportTargetEditor : Editor {

			public override void OnInspectorGUI () {
				TeleportTarget tt = (TeleportTarget) target;
				SerializedObject so = new SerializedObject (target);

				SerializedProperty _useFloor = so.FindProperty ("useFloor");
				EditorGUILayout.PropertyField (_useFloor, true);

				EditorGUI.BeginDisabledGroup (tt.useFloor == false);
				SerializedProperty _floorDimensions = so.FindProperty ("floorDimensions");
				EditorGUILayout.PropertyField (_floorDimensions, true);
				EditorGUI.EndDisabledGroup ();

				EditorGUI.BeginDisabledGroup (tt.useFloor == true);
				SerializedProperty _range = so.FindProperty ("range");
				EditorGUILayout.PropertyField (_range, true);
				EditorGUI.EndDisabledGroup ();

				SerializedProperty _snapToPosition = so.FindProperty ("snapToPosition");
				EditorGUILayout.PropertyField (_snapToPosition, true);
				//Greys out position offset if snapToPosition is off
				EditorGUI.BeginDisabledGroup (tt.snapToPosition == false);
				SerializedProperty _positionOffset = so.FindProperty ("positionOffset");
				EditorGUILayout.PropertyField (_positionOffset, new GUIContent ("Position Offset"), true);
				EditorGUI.EndDisabledGroup ();

				SerializedProperty _snapToRotation = so.FindProperty ("snapToRotation");
				EditorGUILayout.PropertyField (_snapToRotation, true);

				SerializedProperty _teleportEvent = so.FindProperty ("teleportEvent");
				EditorGUILayout.PropertyField (_teleportEvent, true);
				SerializedProperty _leavingEvent = so.FindProperty ("leavingEvent");
				EditorGUILayout.PropertyField (_leavingEvent, true);

				so.ApplyModifiedProperties ();
			}
		}

#endif

		/// <summary>
		/// Whether to use a floor attached to this object instead of the NavMesh or range sphere
		/// This is useful for TeleportTargets that don't Snap To Position but move around so they can't rely on the Nav Mesh
		/// </summary>
		public bool useFloor = false;

		/// <summary>
		/// The dimensions of the floor
		/// </summary>
		public Bounds floorDimensions = new Bounds (Vector3.zero, new Vector3 (1f, 0, 1f));

		/// <summary>
		/// Distance from centre that counts as teleporting to this target
		/// </summary>
		public float range = 0.5f;

		/// <summary>
		/// Whether to snap the teleporter aim to this target's position
		/// </summary>
		public bool snapToPosition = true;

		/// <summary>
		/// Whether to snap the teleporter aim to this target's rotation
		/// </summary>
		public bool snapToRotation = true;

		/// <summary>
		/// Local position offset for where the teleport aim should snap to
		/// </summary>
		public Vector3 positionOffset;

		/// <summary>
		/// Even called when you teleport to this TeleportTarget
		/// </summary>
		public UnityEvent teleportEvent;

		/// <summary>
		/// Event called when you teleport away from this TeleportTarget
		/// </summary>
		public UnityEvent leavingEvent;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private void OnEnable () {
			HandTeleporter.RegisterTeleportTarget (transform);
		}

		private void OnDisable () {
			HandTeleporter.UnregisterTeleportTarget (transform);
		}

#endif

		/// <summary>
		/// Teleport an actor with the specified Flipside user ID here. If you know the
		/// users in your production ahead of time, this can be used to build transitions
		/// for specific actors into your shows.
		/// </summary>
		public void TeleportActorByID (string userId) {
			// Due to the Unity editor being single-user only and not having user IDs,
			// coupled with the ability to fire several of these at a time for
			// orchestrating group teleports, we can't simulate this in the editor.
		}

		/// <summary>
		/// Teleport an actor to this target by their order in the multiplayer list.
		/// This can be used to teleport actors to specific locations during a show.
		/// </summary>
		public void TeleportActorNumber (int actorNumber = 0) {
			// Due to the Unity editor being single-user only, coupled with the ability
			// to fire several of these at a time for orchestrating group teleports,
			// we only simulate this in the editor for player 1 (count starting at 0).

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS
			if (actorNumber == 0) {
				var teleporter = Flipside.Helpers.PlayerController.Instance.rightHand.GetComponent<Flipside.Helpers.HandTeleporter> ();
				teleporter.TeleportTo (transform);
			}
#endif
		}

		/// <summary>
		/// Teleport an individual viewer to this target by their order in their own
		/// multiplayer list. This can be used to teleport individual or several viewers
		/// to specific locations during a show.
		/// </summary>
		public void TeleportViewerNumber (int viewerNumber = 0) {
			// Due to the Unity editor being single-user only, coupled with the ability
			// to fire several of these at a time for orchestrating group teleports,
			// we only simulate this in the editor for player 1 (count starting at 0).

#if UNITY_EDITOR
			if (viewerNumber == 0) {
				var teleporter = Flipside.Helpers.PlayerController.Instance.rightHand.GetComponent<Flipside.Helpers.HandTeleporter> ();
				teleporter.TeleportTo (transform);
			}
#endif
		}

#if UNITY_EDITOR

		private void DrawFloor () {
			Gizmos.color = new Color (0.4f, 0.0f, 0.6f);
			//Draw rectangle around floor
			Gizmos.DrawLine (
				transform.TransformPoint (new Vector3 (floorDimensions.min.x, floorDimensions.center.y, floorDimensions.min.z)),
				transform.TransformPoint (new Vector3 (floorDimensions.min.x, floorDimensions.center.y, floorDimensions.max.z)));
			Gizmos.DrawLine (
				transform.TransformPoint (new Vector3 (floorDimensions.min.x, floorDimensions.center.y, floorDimensions.min.z)),
				transform.TransformPoint (new Vector3 (floorDimensions.max.x, floorDimensions.center.y, floorDimensions.min.z)));
			Gizmos.DrawLine (
				transform.TransformPoint (new Vector3 (floorDimensions.min.x, floorDimensions.center.y, floorDimensions.max.z)),
				transform.TransformPoint (new Vector3 (floorDimensions.max.x, floorDimensions.center.y, floorDimensions.max.z)));
			Gizmos.DrawLine (
				transform.TransformPoint (new Vector3 (floorDimensions.max.x, floorDimensions.center.y, floorDimensions.min.z)),
				transform.TransformPoint (new Vector3 (floorDimensions.max.x, floorDimensions.center.y, floorDimensions.max.z)));
		}

		private void OnDrawGizmosSelected () {
			var centre = transform.TransformPoint (positionOffset);
			var crossThick = 0.01f;
			var crossLength = 0.3f;

			if (snapToPosition) {
				if (useFloor) {
					DrawFloor ();
				} else {
					//Draw spherical range
					Gizmos.color = new Color (0.4f, 0.0f, 0.6f);
					Gizmos.DrawWireSphere (transform.position, range * transform.lossyScale.y);
				}

				//draw offset position
				Gizmos.color = new Color (0.9f, 0.9f, 0.3f);

				Gizmos.DrawLine (centre + new Vector3 (crossThick, 0, crossThick), centre + new Vector3 (crossThick, 0, crossLength));
				Gizmos.DrawLine (centre + new Vector3 (crossThick, 0, crossThick), centre + new Vector3 (crossLength, 0, crossThick));

				Gizmos.DrawLine (centre + new Vector3 (-crossThick, 0, crossThick), centre + new Vector3 (-crossThick, 0, crossLength));
				Gizmos.DrawLine (centre + new Vector3 (-crossThick, 0, crossThick), centre + new Vector3 (-crossLength, 0, crossThick));

				Gizmos.DrawLine (centre + new Vector3 (crossThick, 0, -crossThick), centre + new Vector3 (crossThick, 0, -crossLength));
				Gizmos.DrawLine (centre + new Vector3 (crossThick, 0, -crossThick), centre + new Vector3 (crossLength, 0, -crossThick));

				Gizmos.DrawLine (centre + new Vector3 (-crossThick, 0, -crossThick), centre + new Vector3 (-crossThick, 0, -crossLength));
				Gizmos.DrawLine (centre + new Vector3 (-crossThick, 0, -crossThick), centre + new Vector3 (-crossLength, 0, -crossThick));
			} else {
				if (useFloor) {
					DrawFloor ();
				} else {
					var scaledRange = range * transform.lossyScale.y;
					if (UnityEngine.AI.NavMesh.SamplePosition (transform.position, out UnityEngine.AI.NavMeshHit navMeshHit, scaledRange, 0b1111111111111111)) {
						Gizmos.color = new Color (0.4f, 0.0f, 0.6f, 0.5f);
						Gizmos.DrawWireSphere (transform.position, range * transform.lossyScale.y);
						centre = transform.position;
						centre.y = navMeshHit.position.y;
						//adjust radius for how wide it would be on intersecting the NavMesh
						var heightDifference = Mathf.Abs (transform.position.y - centre.y) / (scaledRange);
						var heightDifference2 = Mathf.Abs (transform.position.y - centre.y - 0.05f) / (scaledRange);
						var rad1 = scaledRange * Mathf.Sqrt (1f - heightDifference * heightDifference);
						var rad2 = scaledRange * Mathf.Sqrt (1f - heightDifference2 * heightDifference2);
						for (int i = 0; i < 16; i++) {
							var angle1 = Mathf.PI / 8f * i;
							var angle2 = Mathf.PI / 8f * (i + 1);
							Gizmos.color = new Color (0.9f, 0.9f, 0.3f);
							Gizmos.DrawLine (centre + new Vector3 (Mathf.Sin (angle1), 0, Mathf.Cos (angle1)) * rad1, centre + new Vector3 (Mathf.Sin (angle2), 0, Mathf.Cos (angle2)) * rad1);
							Gizmos.color = new Color (0.9f, 0.9f, 0.3f, 0.5f);
							Gizmos.DrawLine (centre + Vector3.up * 0.05f + new Vector3 (Mathf.Sin (angle1), 0, Mathf.Cos (angle1)) * rad2, centre + Vector3.up * 0.05f + new Vector3 (Mathf.Sin (angle2), 0, Mathf.Cos (angle2)) * rad2);
						}
					} else {
						//warning that this is out of range of the NavMesh
						Gizmos.color = new Color (1f, 0.1f, 0.1f);
						Gizmos.DrawWireSphere (transform.position, range * transform.lossyScale.y);
					}
				}
			}
		}

#endif
	}
}