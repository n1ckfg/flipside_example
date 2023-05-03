/**
 * Copyright (c) 2020 Flipside XR Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@flipsidexr.com
 * Website: https://www.flipsidexr.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Flipside.Avatars;
using Flipside.Sets;
using UnityEngine.SceneManagement;

[InitializeOnLoadAttribute]
public static class EnsureSetInfoHierarchy {

	static EnsureSetInfoHierarchy () {
		EditorApplication.hierarchyChanged += OnHierarchyChanged;
	}

	private static void OnHierarchyChanged () {
		if (Application.isPlaying) return; //don't apply this while testing

		Scene scene = SceneManager.GetActiveScene ();

		SetInfo setInfo = GameObject.FindObjectOfType<SetInfo> ();
		if (setInfo != null) {
			FixSet (scene, setInfo);
		}

		AvatarModelReferences avs = GameObject.FindObjectOfType<AvatarModelReferences> ();
		if (avs != null) {
			FixAvatar (scene, avs);
		}
	}

	private static void FixSet (Scene scene, SetInfo setInfo) {
		// Make sure SetInfo is in the root
		if (setInfo.transform.parent != null) {
			setInfo.transform.SetParent (null);
		}

		// Make sure SetInfo has a root object under it
		if (setInfo.rootObject == null) {
			foreach (Transform t in setInfo.transform) {
				if (t.name == "Contents") {
					setInfo.rootObject = t.gameObject;
					break;
				}
			}

			if (setInfo.rootObject == null) {
				GameObject rootObject = new GameObject ("Contents");
				rootObject.transform.SetParent (setInfo.transform);
				setInfo.rootObject = rootObject;
			}
		}

		// Make sure the root object is a direct child of SetInfo
		if (setInfo.rootObject.transform.parent != setInfo.transform) {
			setInfo.rootObject.transform.SetParent (setInfo.transform, true);
		}

		// Move any top-level objects under root object
		foreach (var go in scene.GetRootGameObjects ()) {
			if (go != setInfo.gameObject) {
				go.transform.SetParent (setInfo.rootObject.transform, true);
			}
		}

		// Move any siblings of root objct under it
		foreach (Transform t in setInfo.transform) {
			if (t.gameObject != setInfo.rootObject) {
				t.transform.SetParent (setInfo.rootObject.transform, true);
			}
		}

		// Fix cameras to not target the HMD eyes
		Camera[] cams = setInfo.gameObject.GetComponentsInChildren<Camera> (true);

		foreach (Camera cam in cams) {
			cam.stereoTargetEye = StereoTargetEyeMask.None;
		}
	}

	private static void FixAvatar (Scene scene, AvatarModelReferences avs) {
		// Make sure AvatarModelReferences is in the root
		if (avs.transform.parent != null) {
			avs.transform.SetParent (null);
		}

		// Move any top-level objects under AvatarModelReferences
		foreach (var go in scene.GetRootGameObjects ()) {
			if (go != avs.gameObject) {
				go.transform.SetParent (avs.transform, true);
			}
		}
	}
}