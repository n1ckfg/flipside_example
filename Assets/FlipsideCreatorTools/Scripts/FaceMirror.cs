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
using Flipside.Sets;

#if UNITY_EDITOR

using Flipside.Helpers;
using UnityEditor;

#endif

namespace Flipside.Avatars {

	public enum FaceToMirror {
		OnGrab,
		P1,
		P2,
		P3,
		P4,
		P5
	}

	/// <summary>
	/// Animates faces on props and other non-character objects.
	/// </summary>
	public class FaceMirror : MonoBehaviour {

		[Tooltip ("Which actor's face should this mirror?")]
		public FaceToMirror mirrorFace = FaceToMirror.OnGrab;

		[Tooltip ("Stop mirroring the OnGrab face when the actor releases the associated PropElement")]
		public bool unmirrorOnRelease = true;

		[Tooltip ("If attached to a prop and the PropElement is not on the same GameObject, link it here")]
		public PropElement propElement;

		[Space (10)]
		[Tooltip ("The eyes bones, used to rotate the eyes")]
		public List<Transform> eyes;

		[Tooltip ("Should these eyes ignore eye targets such as characters?")]
		public bool ignoreEyeTargets = false;

		[HideInInspector]
		public ulong uniqueID = 0;

		private void Reset () {
			if (uniqueID == 0) {
				GenerateUniqueID ();
			}
		}

		public void GenerateUniqueID () {
			FaceMirror[] all = FindObjectsOfType<FaceMirror> ();

			uniqueID = GenerateRandomID ();
			while (!IsUnique (all)) {
				uniqueID = GenerateRandomID ();
			}
		}

		private ulong GenerateRandomID () {
			return (ulong) Random.Range (1, 4294967295) + 100_000_000_000_000;
		}

		private bool IsUnique (FaceMirror[] all) {
			foreach (FaceMirror fm in all) {
				if (fm == this) continue;
				if (uniqueID == fm.uniqueID) return false;
			}
			return true;
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (FaceMirror))]
	public class FaceMirrorEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			FaceMirror fm = (FaceMirror) target;

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			GUILayout.BeginHorizontal ();
			GUILayout.Label (string.Format ("Unique ID: {0}", fm.uniqueID));
			if (GUILayout.Button ("Regenerate")) {
				fm.GenerateUniqueID ();
			}
			GUILayout.EndHorizontal ();
		}
	}

#endif
}