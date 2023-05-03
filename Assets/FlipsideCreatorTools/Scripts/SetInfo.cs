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
using UnityEngine.Rendering;
using Flipside.Helpers;

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

#endif

namespace Flipside.Sets {

	public class SetInfo : MonoBehaviour {

		[Space (5)]
		[Tooltip ("Disable VR mode so you can test other cameras in the Unity editor")]
		public bool disableVRModeInEditor = false;

		[Space (10)]
		[Tooltip ("The name of your set")]
		public string setName = "";

		[Tooltip ("Credits for this set (e.g., Creative Commons)")]
		public string attribution = "";

		[Tooltip ("Alternate thumbnail to use instead of the automatically generated thumbnail.")]
		[AssetPath.Attribute (typeof (Texture2D))]
		public string thumbnail = "";

		[Space (10)]
		[Tooltip ("The root object within the set hierarchy")]
		public GameObject rootObject;

		[Tooltip ("The audience object within the set hierarchy")]
		public GameObject audience;

		[HideInInspector]
		[SerializeField]
		public ResourceUsageData resourceUsage;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private Camera[] cameras;

		private void Awake () {
			if (!disableVRModeInEditor) {
				cameras = GetComponentsInChildren<Camera> (true);

				Debug.LogFormat ("Loading with VR mode enabled, found {0} cameras.", cameras.Length);

				foreach (Camera c in cameras) {
					c.stereoTargetEye = StereoTargetEyeMask.None;
					c.cullingMask &= ~(1 << LayerMask.NameToLayer ("UI"));
					if (c.targetTexture == null)
						c.gameObject.SetActive (false);

					var listener = c.gameObject.GetComponent<AudioListener> ();
					if (listener != null) Destroy (listener);
				}

				GameObject controllerResource = Resources.Load ("PlayerController") as GameObject;
				GameObject playerInstance = GameObject.Instantiate (controllerResource);
			} else {
				cameras = GetComponentsInChildren<Camera> (true);
				bool camEnabled = false;

				Debug.LogFormat ("Loading with VR mode disabled, found {0} cameras.", cameras.Length);

				foreach (Camera c in cameras) {
					c.stereoTargetEye = StereoTargetEyeMask.None;
					c.cullingMask &= ~(1 << LayerMask.NameToLayer ("UI"));
					if (c.targetTexture == null)
						c.gameObject.SetActive (false);
				}

				if (!camEnabled) {
					if (cameras.Length > 0) {
						cameras[0].gameObject.SetActive (true);
					} else {
						var cobj = new GameObject ("Main Camera");
						cobj.transform.position = new Vector3 (0f, 1.5f, 3);
						cobj.transform.eulerAngles = new Vector3 (0f, 180f, 0f);

						var c = cobj.AddComponent<Camera> ();
						c.stereoTargetEye = StereoTargetEyeMask.None;
						c.cullingMask &= ~(1 << LayerMask.NameToLayer ("UI"));

						cobj.AddComponent<AudioListener> ();
					}
				}
			}

			Instantiate (Resources.Load<GameObject> ("PlaybackControls"));
		}

#endif

#if UNITY_EDITOR

		private void Update () {
			if (rootObject == null) {
				Debug.LogError ("The Root Object setting cannot be empty in your SetInfo component.");
			}

			if (audience == null) {
				Debug.LogError ("The Audience setting cannot be empty in your SetInfo component.");
			}

			CameraPreviews ();
		}

		private void CameraPreviews () {
			if (Input.GetKeyDown (KeyCode.Alpha1)) {
				SwitchToCamera (0);
			}
			if (Input.GetKeyDown (KeyCode.Alpha2)) {
				SwitchToCamera (1);
			}
			if (Input.GetKeyDown (KeyCode.Alpha3)) {
				SwitchToCamera (2);
			}
			if (Input.GetKeyDown (KeyCode.Alpha4)) {
				SwitchToCamera (3);
			}
			if (Input.GetKeyDown (KeyCode.Alpha5)) {
				SwitchToCamera (4);
			}
			if (Input.GetKeyDown (KeyCode.Alpha6)) {
				SwitchToCamera (5);
			}
			if (Input.GetKeyDown (KeyCode.Alpha7)) {
				SwitchToCamera (6);
			}
			if (Input.GetKeyDown (KeyCode.Alpha8)) {
				SwitchToCamera (7);
			}
			if (Input.GetKeyDown (KeyCode.Alpha9)) {
				SwitchToCamera (8);
			}
			if (Input.GetKeyDown (KeyCode.Alpha0)) {
				SwitchToCamera (-1); // Back to VR view
			}
		}

		private void SwitchToCamera (int num) {
			if (num == -1) {
				for (int i = 0; i < cameras.Length; i++) {
					cameras[i].gameObject.SetActive (false);
				}

				UnityEngine.XR.XRSettings.showDeviceView = true;
				return;
			}

			if (num < 0 || num >= cameras.Length) return;

			for (int i = 0; i < cameras.Length; i++) {
				if (UnityEngine.XR.XRSettings.loadedDeviceName == "Oculus") {
					UnityEngine.XR.XRSettings.showDeviceView = false;
				}

				cameras[i].gameObject.SetActive ((i == num) ? true : false);
			}
		}

		public void OnDrawGizmos () {
			float height = 2;
			Gizmos.color = Color.white;
			GUIStyle guiStyle = new GUIStyle ();
			guiStyle.normal.textColor = Color.white;
			guiStyle.font = (Font) Resources.Load ("Quicksand-Medium", typeof (Font));

			Gizmos.DrawLine (new Vector3 (0, 0, 0), new Vector3 (0, height, 0));
			Handles.Label (new Vector3 (0, height, 0), "  Actor Spawn\n  (0, 0, 0)", guiStyle);

			Gizmos.DrawWireCube (Vector3.zero, new Vector3 (1, 0, 1));
		}

#endif
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (SetInfo))]
	public class SetInfoEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			SetInfo set = (SetInfo) target;
			SerializedObject so = new SerializedObject (set);

			DrawResourceUsage (set, so);
		}

		private bool resourcesVisible = true;

		private void DrawResourceUsage (SetInfo component, SerializedObject so) {
			if (!component.resourceUsage.initialized) {
				component.resourceUsage.UpdateInfo (component.gameObject);
				so.ApplyModifiedProperties ();
			}

			GUILayout.Space (5);

			resourcesVisible = EditorGUILayout.Foldout (resourcesVisible, "Resource Usage");

			if (!resourcesVisible) return;

			GUILayout.BeginVertical (EditorStyles.helpBox);

			GUIStyle leftAlign = new GUIStyle (GUI.skin.label) { alignment = TextAnchor.MiddleLeft, wordWrap = true };
			GUIStyle rightAlign = new GUIStyle (GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			GUIStyle leftHeader = new GUIStyle (GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
			GUIStyle rightHeader = new GUIStyle (GUI.skin.label) { alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Bold };

			GUILayoutOption[] col1Opts = new GUILayoutOption[0];
			GUILayoutOption col2Opts = GUILayout.Width (60f);
			GUILayoutOption col3Opts = GUILayout.Width (62f);
			GUILayoutOption col4Opts = GUILayout.Width (68f);
			GUILayoutOption colJoinedOpts = GUILayout.Width (130f);

			GUILayout.BeginHorizontal (EditorStyles.boldLabel);
			GUILayout.Label ("", leftHeader, col1Opts);
			GUILayout.Label ("Scene", rightAlign, col2Opts);
			GUILayout.Label ("Recommended Max", rightAlign, colJoinedOpts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Resources", leftHeader, col1Opts);
			GUILayout.Label ("PC", rightHeader, col3Opts);
			GUILayout.Label ("Mobile", rightHeader, col4Opts);
			GUILayout.EndHorizontal ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.meshCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitSets.Windows.meshCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitSets.Android.meshCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Skinned Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.skinnedMeshCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitSets.Windows.skinnedMeshCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitSets.Android.skinnedMeshCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Props", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.propCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitSets.Windows.propCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitSets.Android.propCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Lights", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.lightCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitSets.Windows.lightCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitSets.Android.lightCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.Space (6);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Vertices", leftHeader, col1Opts);
			GUILayout.EndHorizontal ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.meshVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label ("-", rightAlign, col3Opts);
			GUILayout.Label ("-", rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Skinned Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.skinnedMeshVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label ("-", rightAlign, col3Opts);
			GUILayout.Label ("-", rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Props", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.propVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label ("-", rightAlign, col3Opts);
			GUILayout.Label ("-", rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Total", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.vertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitSets.Windows.meshVertexCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitSets.Android.meshVertexCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.Space (3);

			if (GUILayout.Button ("Refresh values")) {
				component.resourceUsage.UpdateInfo (component.gameObject);
				EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
				Repaint ();
				return;
			}

			GUILayout.Space (3);

			GUILayout.EndVertical ();
		}
	}

#endif
}