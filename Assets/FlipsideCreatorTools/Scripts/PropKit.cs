/**
 * Copyright (c) 2021 The Campfire Union Inc - All Rights Reserved.
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
using Flipside.Helpers;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

using UnityEngine.Events;
using UnityEngine.XR;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

#endif

namespace Flipside.Sets {

	[Serializable]
	public class PropInfo {
		public PropElement propElement;

		public string displayName = "";

		[HideInInspector]
		[SerializeField]
		public Bounds bounds;

		[HideInInspector]
		[SerializeField]
		public Vector3 originalScale;

		[AssetPath.Attribute (typeof (Texture2D))]
		public string thumbnail = "";
	}

	public class PropKit : MonoBehaviour {

		[Space (5)]
		[Tooltip ("The name of your kit")]
		public string kitName = "";

		[Tooltip ("Credits for this kit (e.g., Creative Commons)")]
		public string attribution = "";

		[Tooltip ("Alternate thumbnail to use instead of the automatically generated thumbnail.")]
		[AssetPath.Attribute (typeof (Texture2D))]
		public string thumbnail = "";

		[Space (10)]
		[Tooltip ("The list of props in this kit.")]
		[NonReorderable]
		[SerializeField]
		public PropInfo[] propList = new PropInfo[0];

		[HideInInspector]
		[SerializeField]
		public ResourceUsageData resourceUsage;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private int activeProp = 0;
		private GameObject currentProp = null;
		private Vector3 positionOnStand = new Vector3 (0f, 1.25f, 0.5f);
		private Helpers.PlayerController player;
		private bool lastLeftButtonState = false;
		private bool lastRightButtonState = false;

		private void Awake () {
			foreach (var prop in propList) {
				if (prop == null) continue;

				// Ensure props that aren't prefabs are disabled
				if (prop.propElement.gameObject.activeInHierarchy) {
					prop.propElement.gameObject.SetActive (false);
				}
			}

			GameObject controllerResource = Resources.Load ("PlayerController") as GameObject;
			GameObject playerInstance = Instantiate (controllerResource);
			player = playerInstance.GetComponent<Helpers.PlayerController> ();

			SetupPropTestEnvironment ();
		}

		private void SetupPropTestEnvironment () {
			GameObject propEnv = Resources.Load ("PropTestingEnvironment") as GameObject;
			GameObject envInst = Instantiate (propEnv);
		}

		private void OnEnable () {
			ShowProp (0);
		}

		private void Update () {
			// Toggle active prop with the left (previous) and right (next) primary buttons, or spacebar to advance

			if (Input.GetKeyDown (KeyCode.Space)) {
				ShowNextProp ();
			}

			var leftInput = player.leftHand.GetInputDevice ();
			if (leftInput != null && leftInput.isValid) {
				bool buttonState = false;
				if (leftInput.TryGetFeatureValue (CommonUsages.primaryButton, out buttonState)) {
					if (buttonState != lastLeftButtonState && buttonState == true) {
						ShowPreviousProp ();
					}
					lastLeftButtonState = buttonState;
				}
			}

			var rightInput = player.rightHand.GetInputDevice ();
			if (rightInput != null && rightInput.isValid) {
				bool buttonState = false;
				if (rightInput.TryGetFeatureValue (CommonUsages.primaryButton, out buttonState)) {
					if (buttonState != lastRightButtonState && buttonState == true) {
						ShowNextProp ();
					}
					lastRightButtonState = buttonState;
				}
			}
		}

		public GameObject ShowProp (int index, bool destroyPrevious = true) {
			if (destroyPrevious && currentProp != null) Destroy (currentProp);

			activeProp = index;

			var prop = propList[activeProp];
			if (prop == null) return null;

			currentProp = Instantiate (prop.propElement.gameObject);
			currentProp.transform.position = positionOnStand;
			currentProp.transform.rotation = Quaternion.identity;
			currentProp.SetActive (true);

			return currentProp;
		}

		private void ShowNextProp () {
			int index = (activeProp >= propList.Length - 1) ? 0 : activeProp + 1;

			ShowProp (index);
		}

		private void ShowPreviousProp () {
			int index = (activeProp > 0) ? activeProp - 1 : propList.Length - 1;
			if (index < 0) index = 0;

			ShowProp (index);
		}

		public GameObject CreatePropLight () {
			GameObject lightObject = new GameObject ("Prop Light");

			Light light = lightObject.AddComponent<Light> ();
			light.type = LightType.Directional;
			light.intensity = 0.9f;
			light.transform.eulerAngles = new Vector3 (0f, 165f, 0f);

			return lightObject;
		}

#endif
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (PropKit))]
	public class PropKitEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			PropKit propkit = (PropKit) target;
			SerializedObject so = new SerializedObject (propkit);

			DrawResourceUsage (propkit, so);
		}

		private bool resourcesVisible = true;

		private void DrawResourceUsage (PropKit component, SerializedObject so) {
			if (!component.resourceUsage.initialized) {
				component.resourceUsage.UpdateInfo (component);
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
			GUILayout.Label (ResourceUsageLimitPropKits.Windows.meshCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Android.meshCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Skinned Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.skinnedMeshCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Windows.skinnedMeshCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Android.skinnedMeshCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Props", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.propCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Windows.propCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Android.propCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Lights", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.lightCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Windows.lightCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Android.lightCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.Space (6);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Vertices", leftHeader, col1Opts);
			GUILayout.EndHorizontal ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.propVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label ("-", rightAlign, col3Opts);
			GUILayout.Label ("-", rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Skinned Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.skinnedMeshVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Windows.skinnedMeshVertexCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Android.skinnedMeshVertexCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Total", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.vertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Windows.meshVertexCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitPropKits.Android.meshVertexCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.Space (3);

			if (GUILayout.Button ("Refresh values")) {
				component.resourceUsage.UpdateInfo (component);
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