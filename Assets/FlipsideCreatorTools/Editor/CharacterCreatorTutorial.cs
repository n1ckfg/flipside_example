/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Flipside.Sets;
using Flipside.Helpers;

namespace Flipside.Avatars {

	[InitializeOnLoad]
	[ExecuteInEditMode]
	public class CharacterCreatorTutorial : EditorWindow {
		private static Font titleFont;
		private static Font paragraphFont;
		private static CharacterCreatorTutorial window;

		public static void CheckVRSettings () {
#if UNITY_EDITOR
			UnityEngine.XR.XRSettings.enabled = true;
			PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
			PlayerSettings.colorSpace = ColorSpace.Linear;
#endif
		}

		[MenuItem ("Flipside Creator Tools/Open Flipside Creator Tools", false, 2)]
		public static void OpenMenu () {
			CheckVRSettings ();

			window = GetWindow<CharacterCreatorTutorial> ("Flipside");
			if (window != null) {
				window.Show ();
			}
		}

		/// <summary>
		/// Rename Android bundles to add _android suffix.
		/// </summary>
		/// <param name="bundlePath">Path to bundle folder</param>
		private static void RenameAndroidBundles (string bundlePath) {
			string[] bundles = Directory.GetFiles (bundlePath, "*", SearchOption.TopDirectoryOnly);
			foreach (string bundle in bundles) {
				if (bundle.EndsWith ("Android") || bundle.EndsWith (".manifest") || bundle.EndsWith ("_android")) {
					continue;
				}

				string android_bundle = bundle.Replace ("/Android/", "/") + "_android";

				if (File.Exists (android_bundle)) File.Delete (android_bundle);

				File.Move (bundle, android_bundle);
			}
		}

		private static void BuildSetBundle (string bundlePath, bool publish = true) {
			CheckVRSettings ();
			Debug.Log ("Building set bundle, please wait...");
			var scene = SceneManager.GetActiveScene ();

			if (scene.rootCount == 0) {
				Debug.LogError ("The scene is empty. There should be at least one object in the scene.");
				return;
			} else if (scene.rootCount > 1) {
				var setInfoTemp = GameObject.FindObjectsOfType<SetInfo> ();
				if (setInfoTemp.Length == 0) {
					Debug.LogError ("There is no SetInfo component in the scene or the SetInfo component has been disabled.");
					return;
				} else if (setInfoTemp.Length == 1) {
					//Move any loose objects under the root object
					foreach (var looseObject in scene.GetRootGameObjects ()) {
						if (looseObject != setInfoTemp[0].gameObject) {
							looseObject.transform.SetParent (setInfoTemp[0].rootObject.transform, true);
						}
					}
				} else if (setInfoTemp.Length > 1) {
					Debug.LogError ("There is more than one SetInfo component in the scene. You should put the contents of one SetInfo inside of the other SetInfo instead of having two of them.");
					return;
				}
			}

			var root = scene.GetRootGameObjects ()[0];
			var setInfo = root.GetComponent<SetInfo> ();

			if (setInfo == null) {
				Debug.LogError ("The SetInfo component was not found on root scene object.");
				return;
			}

			if (setInfo.rootObject == null) {
				Debug.LogError ("The Root Object setting cannot be empty in your SetInfo component.");
				return;
			}

			if (setInfo.audience == null) {
				Debug.LogError ("The Audience setting cannot be empty in your SetInfo component.");
				return;
			}

			if (setInfo.name.Trim (' ') == "") {
				Debug.LogError ("Please add a set name in your SetInfo component before publishing.");
				return;
			}

			if (root.GetComponentInChildren<Camera> () != null) {
				foreach (var camera in root.GetComponentsInChildren<Camera> ()) {
					camera.stereoTargetEye = StereoTargetEyeMask.None;
				}
			}

			//clean up audio listeners
			foreach (var audie in root.GetComponentsInChildren<AudioListener> (true)) {
				DestroyImmediate (audie);
			}

			//Ensure props don't have duplicate names
			var propNames = new Dictionary<string, int> ();
			foreach (var prop in root.GetComponentsInChildren<PropElement> (true)) {
				var propName = prop.name;
				if (propNames.ContainsKey (propName)) {
					propNames[propName]++;
					while (propNames.ContainsKey (string.Format ("{0}{1}", propName, propNames[propName]))) { //just in case there's something else with this name
						propNames[propName]++;
					}
					prop.name = string.Format ("{0}{1}", propName, propNames[propName]);
				} else {
					propNames.Add (propName, 0);
				}
			}

			var nms = setInfo.rootObject.GetComponent<NavMeshSurface> ();

			if (nms == null) {
				Debug.LogError ("The NavMeshSurface component was not found on " + setInfo.rootObject.name);
				return;
			}

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;

			Thumbnailer.TakeThumbnail (bundlePath, assetBundleName);

			nms.BuildNavMesh ();

			EditorSceneManager.SaveOpenScenes ();

			string fullBundlePath = bundlePath + "/" + assetBundleName;

			if (File.Exists (fullBundlePath)) {
				File.Delete (fullBundlePath);
			}

			string manifest = fullBundlePath + ".manifest";

			if (File.Exists (manifest)) {
				File.Delete (manifest);
			}

			BuildPipeline.BuildAssetBundles (bundlePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

			if (BuildPipeline.IsBuildTargetSupported (BuildTargetGroup.Android, BuildTarget.Android)) {
				if (ResourceUsageLimitSets.WithinAndroidLimits (setInfo)) {
					bundlePath = bundlePath + "/Android/";
					Directory.CreateDirectory (bundlePath);
					BuildPipeline.BuildAssetBundles (bundlePath, BuildAssetBundleOptions.None, BuildTarget.Android);

					RenameAndroidBundles (bundlePath);
				} else {
					Debug.LogWarning ("Lower resource usage to build for Android");
					string androidBundlePath = bundlePath + "/Android/" + assetBundleName;
					if (File.Exists (androidBundlePath)) {
						File.Delete (androidBundlePath);
					}
				}
			} else {
				Debug.LogWarning ("Android builds not supported");
			}

			Debug.Log ("Set bundle saved to:\n" + fullBundlePath);
			if (publish) {
				Debug.Log ("Publishing set bundle, please wait...");
				PublishSetBundle ();
			}
		}

		private static void BuildPropKitBundle (string bundlePath, bool publish = true) {
			CheckVRSettings ();
			Debug.Log ("Building prop kit bundle, please wait...");
			var scene = SceneManager.GetActiveScene ();

			if (scene.rootCount == 0) {
				Debug.LogError ("The scene is empty. There should be at least one object in the scene.");
				return;
			} else if (scene.rootCount > 1) {
				var propKitTemp = GameObject.FindObjectsOfType<PropKit> ();
				if (propKitTemp.Length == 0) {
					Debug.LogError ("There is no PropKit component in the scene or the PropKit component has been disabled.");
					return;
				} else if (propKitTemp.Length == 1) {
					//Move any loose objects under the root object
					foreach (var looseObject in scene.GetRootGameObjects ()) {
						if (looseObject != propKitTemp[0].gameObject) {
							looseObject.transform.SetParent (propKitTemp[0].transform, true);
						}
					}
				} else if (propKitTemp.Length > 1) {
					Debug.LogError ("There is more than one PropKit component in the scene. You should put the contents of one PropKit inside of the other PropKit instead of having two of them.");
					return;
				}
			}

			var root = scene.GetRootGameObjects ()[0];
			var propKit = root.GetComponent<PropKit> ();

			if (propKit == null) {
				Debug.LogError ("The PropKit component was not found on root scene object.");
				return;
			}

			//Get dimensions and scale
			if (propKit.propList != null) {
				foreach (var propInfo in propKit.propList) {
					if (propInfo.propElement == null) {
						Debug.LogErrorFormat ("The Prop named {0} is missing or unassigned.", propInfo.displayName);
						return;
					}
					propInfo.originalScale = propInfo.propElement.transform.localScale;
					var storedRotation = propInfo.propElement.transform.rotation;
					propInfo.propElement.transform.rotation = Quaternion.identity; //rotation will make scaling the bounds difficult

					Physics.SyncTransforms (); //forces it to calculate bounds with new rotation

					var propColliders = propInfo.propElement.GetComponentsInChildren<Collider> ();

					propInfo.bounds = new Bounds (propInfo.propElement.transform.position, Vector3.zero);
					foreach (var propCollider in propColliders) {
						var colliderBounds = propCollider.bounds;
						propInfo.bounds.Encapsulate (colliderBounds);
					}
					var localBounds = propInfo.bounds;
					localBounds.center = propInfo.propElement.transform.InverseTransformPoint (localBounds.center);
					localBounds.size = propInfo.propElement.transform.InverseTransformVector (localBounds.size); //this is what counted on no rotation
					propInfo.bounds = localBounds;
					propInfo.propElement.transform.rotation = storedRotation;
				}
			} else {
				Debug.LogError ("The PropKit has no props assigned to propList");
				return;
			}

			if (root.GetComponentInChildren<Camera> () != null) {
				foreach (var camera in root.GetComponentsInChildren<Camera> ()) {
					camera.stereoTargetEye = StereoTargetEyeMask.None;
				}
			}

			//clean up audio listeners
			foreach (var audio in root.GetComponentsInChildren<AudioListener> (true)) {
				DestroyImmediate (audio);
			}

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;

			EditorSceneManager.SaveOpenScenes ();

			Thumbnailer.TakeThumbnail (bundlePath, assetBundleName);
			Thumbnailer.TakePropThumbnails (bundlePath, assetBundleName);

			string fullBundlePath = bundlePath + "/" + assetBundleName;

			if (File.Exists (fullBundlePath)) {
				File.Delete (fullBundlePath);
			}

			string manifest = fullBundlePath + ".manifest";

			if (File.Exists (manifest)) {
				File.Delete (manifest);
			}

			BuildPipeline.BuildAssetBundles (bundlePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

			if (BuildPipeline.IsBuildTargetSupported (BuildTargetGroup.Android, BuildTarget.Android)) {
				if (ResourceUsageLimitPropKits.WithinAndroidLimits (propKit)) {
					bundlePath = bundlePath + "/Android/";
					Directory.CreateDirectory (bundlePath);
					BuildPipeline.BuildAssetBundles (bundlePath, BuildAssetBundleOptions.None, BuildTarget.Android);

					RenameAndroidBundles (bundlePath);
				} else {
					Debug.LogWarning ("Lower resource usage to build for Android");
					string androidBundlePath = bundlePath + "/Android/" + assetBundleName;
					if (File.Exists (androidBundlePath)) {
						File.Delete (androidBundlePath);
					}
				}
			} else {
				Debug.LogWarning ("Android builds not supported");
			}

			Debug.Log ("Prop kit bundle saved to:\n" + fullBundlePath);
			if (publish) {
				Debug.Log ("Publishing prop kit bundle, please wait...");
				PublishPropKitBundle ();
			}
		}

		private static void BuildAvatarBundle (AvatarModelReferences avatarModelReferences, string bundlePath, bool publish = true) {
			CheckVRSettings ();
			Debug.Log ("Building character bundle, please wait...");
			Scene scene = EditorSceneManager.GetActiveScene ();

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;

			if (avatarModelReferences.name.Trim (' ') == "") {
				Debug.LogError ("Please add a character name in your AvatarModelReferences component before publishing.");
				return;
			}

			EditorUtility.DisplayProgressBar ("BuildAvatarBundle: ", "Taking Thumbnail", 0.3f);
			Thumbnailer.TakeThumbnail (bundlePath, assetBundleName);

			EditorUtility.DisplayProgressBar ("BuildAvatarBundle: ", "Clearing Blend Shapes", 0.6f);

			avatarModelReferences.ClearBlendShapes ();

			EditorUtility.ClearProgressBar ();

			string fullBundlePath = bundlePath + "/" + assetBundleName;

			if (File.Exists (fullBundlePath)) {
				File.Delete (fullBundlePath);
			}

			string manifest = fullBundlePath + ".manifest";

			if (File.Exists (manifest)) {
				File.Delete (manifest);
			}

			BuildPipeline.BuildAssetBundles (bundlePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

			if (BuildPipeline.IsBuildTargetSupported (BuildTargetGroup.Android, BuildTarget.Android)) {
				if (ResourceUsageLimitAvatars.WithinAndroidLimits (avatarModelReferences)) {
					bundlePath = bundlePath + "/Android/";
					Directory.CreateDirectory (bundlePath);
					BuildPipeline.BuildAssetBundles (bundlePath, BuildAssetBundleOptions.None, BuildTarget.Android);

					RenameAndroidBundles (bundlePath);
				} else {
					Debug.LogWarning ("Lower resource usage to build for Android");
					string androidBundlePath = bundlePath + "/Android/" + assetBundleName;
					if (File.Exists (androidBundlePath)) {
						File.Delete (androidBundlePath);
					}
				}
			} else {
				Debug.LogWarning ("Android builds not supported");
			}

			Debug.Log ("Character bundle saved to:\n" + fullBundlePath);
			if (publish) {
				Debug.Log ("Publishing character bundle, please wait...");
				PublishAvatarBundle ();
			}
		}

		public void Update () {
			Repaint ();
		}

		private Texture2D MakeBackgroundTexture (int width, int height, Color col) {
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix[i] = col;

			Texture2D result = new Texture2D (width, height);
			result.SetPixels (pix);
			result.Apply ();

			return result;
		}

		private void OnGUI () {
			SetFonts ();
			DrawLogo ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			if (!Application.unityVersion.StartsWith (FlipsideSettings.currentUnityVersion)) {
				GUIStyle warningBackground = new GUIStyle ();
				warningBackground.normal.background = MakeBackgroundTexture (8, 8, new Color (0.8f, 0f, 0f, 1f));
				warningBackground.fixedHeight = 140;
				GUILayout.BeginHorizontal (warningBackground);
				DrawTitle ("UNITY VERSION MISMATCH DETECTED!", Color.white);
				GUILayout.EndHorizontal ();
				warningBackground.fixedHeight = 0;
				GUILayout.BeginHorizontal (warningBackground);
				DrawParagraph ("Current version: " + Application.unityVersion, Color.white);
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal (warningBackground);
				DrawParagraph ("Compatible version: " + FlipsideSettings.fullUnityVersion, Color.white);
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal (warningBackground);
				DrawButton ("Click Here for Compatible Version", () => Application.OpenURL ("https://www.flipsidexr.com/docs/1.0/creator-tools/installation-and-setup"));
				GUILayout.EndHorizontal ();
			}

			if (!IsFlipsideIDSet ()) {
				DrawManualAndAccountLinks ("https://www.flipsidexr.com/docs/2022.1/creator-tools");
				DrawVersion ();
				return;
			}

			SetInfo setInfo = FindObjectOfType<SetInfo> ();
			if (setInfo != null) {
				DrawManualAndAccountLinks ("https://www.flipsidexr.com/docs/2022.1/creator-tools/tutorials/creating-a-custom-set");
				TutorialSetCreator (setInfo);
				DrawVersion ();
				return;
			}

			PropKit propKit = FindObjectOfType<PropKit> ();
			if (propKit != null) {
				DrawManualAndAccountLinks ("https://www.flipsidexr.com/docs/1.0/creator-tools/creating-prop-kits");
				TutorialPropKitCreator (propKit);
				DrawVersion ();
				return;
			}

			AvatarModelReferences avatarModelReferences = FindObjectOfType<AvatarModelReferences> ();
			if (avatarModelReferences == null) {
				DrawManualAndAccountLinks ("https://www.flipsidexr.com/docs/2021.1/creator-tools");
				TutorialPrepCharacterModel ();
				DrawVersion ();
				return;
			}

			string assetBundlePath = Application.dataPath + "/../AssetBundles";
			Directory.CreateDirectory (assetBundlePath);

			AssetImporter assetImporter = AssetImporter.GetAtPath (EditorSceneManager.GetActiveScene ().path);
			if (assetImporter == null) {
				DrawManualAndAccountLinks ("https://www.flipsidexr.com/docs/2022.1/creator-tools");
				DrawParagraph ("Please save the current scene to continue.");
				DrawVersion ();
				return;
			}

			string assetFullPath = assetBundlePath + "/" + assetImporter.assetBundleName;

			TutorialBuildBundle (avatarModelReferences, assetBundlePath, assetFullPath);

			if (avatarModelReferences != null) {
				RemoveIllegalComponents (avatarModelReferences);

				TutorialRequiredFixesList (avatarModelReferences, assetBundlePath, assetFullPath);
			}

			DrawVersion ();
		}

		private static void SetFonts () {
			titleFont = (Font) Resources.Load ("Quicksand-Medium", typeof (Font));
			paragraphFont = (Font) Resources.Load ("Quicksand-Regular", typeof (Font));
		}

		#region Publishing

		private static bool publishHandlersAdded = false;
		private static float uploadProgress = 0f;
		private static int displayUploaded = -1; // avatar ID
		private static string displayError = ""; // error message

		private static void AddPublishHandlers () {
			if (!publishHandlersAdded) {
				ApiClient.OnError += PublishErrorHandler;
				ApiClient.OnProgress += PublishProgressHandler;
				publishHandlersAdded = true;
			}
		}

		private static void PublishErrorHandler (string err) {
			Debug.LogError ("Publish Error: " + err);
			uploadProgress = 0f;
			displayUploaded = -1;
			displayError = err;
		}

		private static void PublishProgressHandler (float progress) {
			uploadProgress = progress;
		}

		private static void PublishSetBundle () {
			AddPublishHandlers ();

			uploadProgress = 0.001f;
			displayUploaded = -1;
			displayError = "";

			ApiClient.PublishSet ((int id) => {
				Debug.LogFormat ("Set published with ID {0} at {1}", id, DateTime.Now.ToString ("h:mm tt - MMMM d, yyyy"));
				uploadProgress = 0f;
				displayUploaded = id;
				displayError = "";
			});
		}

		private static void PublishAvatarBundle () {
			AddPublishHandlers ();

			uploadProgress = 0.001f;
			displayUploaded = -1;
			displayError = "";

			ApiClient.PublishAvatar ((int id) => {
				Debug.LogFormat ("Character published with ID {0} at {1}", id, DateTime.Now.ToString ("h:mm tt - MMMM d, yyyy"));
				uploadProgress = 0f;
				displayUploaded = id;
				displayError = "";
			});
		}

		private static void PublishPropKitBundle () {
			AddPublishHandlers ();

			uploadProgress = 0.001f;
			displayUploaded = -1;
			displayError = "";

			ApiClient.PublishPropKit ((int id) => {
				Debug.LogFormat ("Prop kit published with ID {0} at {1}", id, DateTime.Now.ToString ("h:mm tt - MMMM d, yyyy"));
				uploadProgress = 0f;
				displayUploaded = id;
				displayError = "";
			});
		}

		private static void DisplayProgressbar () {
			if (uploadProgress > 0f) {
				var rect = EditorGUILayout.GetControlRect (false, EditorGUIUtility.singleLineHeight);
				EditorGUI.ProgressBar (rect, uploadProgress, "Uploading...");
				GUILayout.Space (7);
			}
		}

		private static void DisplayUploaded () {
			if (displayUploaded != -1) {
				DrawParagraph ("Published at " + DateTime.Now.ToString ("h:mm tt - MMMM d, yyyy"));
			}
		}

		private static void DisplayUploadError () {
			if (displayError != "") {
				DrawParagraph (displayError, new Color (0.8f, 0f, 0f, 1f));
			}
		}

		#endregion Publishing

		private static void TutorialRequiredFixesList (AvatarModelReferences avatarModelReferences, string assetBundlePath, string assetFullPath) {
			if (avatarModelReferences.animator == null) {
				DrawTitle ("CONNECT ANIMATOR");
				DrawParagraph ("Link the animator that controls your avatar.");
			} else if (!avatarModelReferences.animator.isHuman) {
				DrawTitle ("SET HUMANOID RIG");
				DrawParagraph ("Go to your model import settings and set Rig -> Animation Type to Humanoid. Then hit apply.");
			}

			SkinnedMeshRenderer[] meshs = avatarModelReferences.GetComponentsInChildren<SkinnedMeshRenderer> ();
			foreach (var mesh in meshs) {
				if (!mesh.updateWhenOffscreen) {
					DrawTitle ("FIX OFFSCREEN MESHES");
					DrawParagraph ("Make sure each mesh has Update When Offscreen checked.");
					break;
				}
			}

			if (avatarModelReferences.centerEye == null) {
				DrawTitle ("SETUP CENTER EYE");
				DrawParagraph ("Create a transform in your bone structure named 'centereye'. this will be used to map your avatar's head to the position of the VR camera.");
			}

			if (avatarModelReferences.mesh == null) {
				DrawTitle ("CHOOSE FACE MESH");
				DrawParagraph ("Link the mesh used for your blend shapes.");
			} else if (avatarModelReferences.mesh.sharedMesh.blendShapeCount <= 16 && (avatarModelReferences.expressionType == AvatarModelReferences.ExpressionType.blendShapes)) {
				DrawTitle ("LIP SYNCING (OPTIONAL)");
				DrawParagraph ("Make sure your mesh has the correct blend shape array if you want facial animations.");
			}

			string assetBundleName = AssetImporter.GetAtPath (avatarModelReferences.gameObject.scene.path).assetBundleName;

			if (assetBundleName == "") {
				DrawTitle ("MISSING ASSET BUNDLE LABEL");
				DrawParagraph ("An asset bundle label was not found on this character.");
				DrawParagraph ("Please select the scene file in the Project window and add an asset bundle label to continue.");
				return;
			}

			DrawButton ("Auto-Fix Character", () => avatarModelReferences.FindEmptyReferences (), 25);

			AssetImporter newImporter = ModelImporter.GetAtPath (AssetDatabase.GetAssetPath (Selection.activeObject));
			try {
				ModelImporter newModelImporter = (ModelImporter) newImporter;
				if (newModelImporter != null && newModelImporter.animationType == ModelImporterAnimationType.Human) {
					DrawButton ("Setup New Character", () => CharacterCreator.CreateAvatarFromModel (), 25);
				}
			} catch { }
		}

		private static void RemoveIllegalComponents (AvatarModelReferences avatarModelReferences) {
			if (avatarModelReferences.GetComponentInChildren<Camera> () != null) {
				foreach (var flareLayer in avatarModelReferences.GetComponentsInChildren<FlareLayer> ()) {
					DestroyImmediate (flareLayer);
				}

				foreach (var audioListener in avatarModelReferences.GetComponentsInChildren<AudioListener> ()) {
					DestroyImmediate (audioListener);
				}

				foreach (var camera in avatarModelReferences.GetComponentsInChildren<Camera> ()) {
					DestroyImmediate (camera);
				}
			}
		}

		private static void TutorialBuildBundle (AvatarModelReferences avatarModelReferences, string assetBundlePath, string assetFullPath) {
			DrawManualAndAccountLinks ("https://www.flipsidexr.com/docs/2021.1/creator-tools/tutorials/creating-a-custom-character");

			if (ReadyToUpload (avatarModelReferences)) {
				DrawTitle ("CHARACTER: " + avatarModelReferences.characterName.ToUpper ());
				DrawParagraph ("Setup your character in the Inspector window when the \"" + avatarModelReferences.gameObject.name + "\" object is selected in the Hierarchy window.");
				DrawParagraph ("Press Play to preview your character in VR.");

				EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

				DrawButton ("Build & Publish Character", () => BuildAvatarBundle (avatarModelReferences, assetBundlePath, true), 25);

				DisplayProgressbar ();
				DisplayUploaded ();
				DisplayUploadError ();

				EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

				DrawParagraph ("Additional options:");
				DrawButton ("Build Character Bundle Only", () => BuildAvatarBundle (avatarModelReferences, assetBundlePath, false), 25);

				if (File.Exists (assetFullPath)) {
					DrawButton ("Publish Without Rebuilding", () => {
						Debug.Log ("Publishing character bundle, please wait...");
						PublishAvatarBundle ();
					});
					DrawButton ("Find Character Bundle Files", () => EditorUtility.RevealInFinder (assetFullPath), 25);
					DrawButton ("Edit Character Online", () => Application.OpenURL ("https://www.flipsidexr.com/characters/publish?bundle_name=" + Path.GetFileName (assetBundlePath)));
				}
			} else {
				DrawTitle ("COMPLETE TO DO LIST");
				DrawParagraph ("See the list below for more steps you need to complete before you can continue.");
				DrawParagraph ("Try clicking Auto-Fix Character in the advanced tools below to automatically resolve this issue.");
			}
		}

		private static void TutorialPrepCharacterModel () {
			AssetImporter importer = ModelImporter.GetAtPath (AssetDatabase.GetAssetPath (Selection.activeObject));
			ModelImporter modelImporter = null;

			try {
				modelImporter = (ModelImporter) importer;
			} catch { }

			DrawTitle ("CREATE A CHARACTER");

			if (modelImporter == null) { //make sure a model is selected
				DrawParagraph ("To begin, drag your character model into the Assets folder and then select it.");
				EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));
				TutorialPrepSet ();
				EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));
				TutorialPrepKit ();
			} else if (modelImporter.animationType != ModelImporterAnimationType.Human) { //make sure the model is a humanoid
				DrawParagraph ("Go to the Inspector window (to the right)\n\nClick Rig -> Animation Type -> Humanoid\n\nThen hit Apply.");
			} else { //seems legit, turn it into a character
				DrawParagraph ("Now that your character model is selected, click Import Your Character to setup your character.");
				DrawButton ("Import Your Character", () => CharacterCreator.CreateAvatarFromModel (), 25);
			}
		}

		private static void TutorialPrepSet () {
			DrawTitle ("CREATE A SET");

			DrawButton ("Create Set", () => SetEditor.CreateSet ());
			DrawButton ("Create Set From Current Scene", () => SetEditor.CreateSetFromCurrent ());
		}

		private static void TutorialPrepKit () {
			DrawTitle ("CREATE A PROP KIT");

			DrawButton ("Create Prop Kit", () => PropKitEditor.CreatePropKit ());
		}

		private static void TutorialSetCreator (SetInfo setInfo) {
			string bundlePath = Application.dataPath + "/../AssetBundles";
			Directory.CreateDirectory (bundlePath);
			var activeScene = EditorSceneManager.GetActiveScene ();
			var activeObj = AssetImporter.GetAtPath (activeScene.path);
			if (activeObj == null) return;
			string assetBundleName = activeObj.assetBundleName;
			string fullPath = bundlePath + "/" + assetBundleName;

			if (assetBundleName == "") {
				DrawTitle ("MISSING ASSET BUNDLE LABEL");
				DrawParagraph ("An asset bundle label was not found on this set.");
				DrawParagraph ("Please select the scene file in the Project window and add an asset bundle label to continue.");
				return;
			}

			DrawTitle ("SET: " + setInfo.setName.ToUpper ());

			DrawParagraph ("Build your set under the \"" + setInfo.gameObject.name + "/Contents\" object in the Hierarchy window.");
			DrawParagraph ("Add PropElement and other components to create props and interactive elements.");
			DrawParagraph ("Press Play to preview and test your set in VR.");

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			DrawButton ("Build & Publish Set", () => BuildSetBundle (bundlePath, true), 25);

			DisplayProgressbar ();
			DisplayUploaded ();
			DisplayUploadError ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			DrawParagraph ("Additional options:");
			DrawButton ("Build Set Bundle Only", () => BuildSetBundle (bundlePath, false), 25);

			if (File.Exists (fullPath)) {
				DrawButton ("Publish Without Rebuilding", () => {
					Debug.Log ("Publishing set bundle, please wait...");
					PublishSetBundle ();
				});
				DrawButton ("Find Set Bundle Files", () => EditorUtility.RevealInFinder (fullPath), 25);
				DrawButton ("Edit Set Online", () => Application.OpenURL ("https://www.flipsidexr.com/sets/publish?bundle_name=" + assetBundleName));
			}
		}

		private static void TutorialPropKitCreator (PropKit propKit) {
			DrawTitle ("KIT: " + propKit.kitName.ToUpper ());

			string bundlePath = Application.dataPath + "/../AssetBundles";
			Directory.CreateDirectory (bundlePath);
			var activeScene = EditorSceneManager.GetActiveScene ();
			var activeObj = AssetImporter.GetAtPath (activeScene.path);
			if (activeObj == null) return;
			string assetBundleName = activeObj.assetBundleName;
			string fullPath = bundlePath + "/" + assetBundleName;

			if (assetBundleName == "") {
				DrawTitle ("MISSING ASSET BUNDLE LABEL");
				DrawParagraph ("An asset bundle label was not found on this prop kit.");
				DrawParagraph ("Please select the scene file in the Project window and add an asset bundle label to continue.");
				return;
			}

			DrawParagraph ("Build your prop kit by adding prefabs to the prop list on the PropKit component in the Inspector window.");
			DrawParagraph ("The prop list can include any prefab or object with a PropElement component attached to it.");
			DrawParagraph ("Press Play to preview and test your props in VR.");

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			DrawButton ("Build & Publish Prop Kit", () => BuildPropKitBundle (bundlePath, true), 25);

			DisplayProgressbar ();
			DisplayUploaded ();
			DisplayUploadError ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			DrawParagraph ("Additional options:");
			DrawButton ("Build Prop Kit Bundle Only", () => BuildPropKitBundle (bundlePath, false), 25);

			if (File.Exists (fullPath)) {
				DrawButton ("Publish Without Rebuilding", () => {
					Debug.Log ("Publishing prop kit bundle, please wait...");
					PublishPropKitBundle ();
				});
				DrawButton ("Find Prop Kit Bundle File", () => EditorUtility.RevealInFinder (fullPath), 25);
				DrawButton ("Edit Prop Kit Online", () => Application.OpenURL ("https://www.flipsidexr.com/propkits/publish?bundle_name=" + assetBundleName));
			}

			/*DrawButton ("Take Prop Thumbnail Photos", () => {
				Directory.CreateDirectory (bundlePath);
				Thumbnailer.TakePropThumbnails (bundlePath, assetBundleName);
			});*/
		}

		private static bool ReadyToUpload (AvatarModelReferences avatarModelReferences) {
			bool modelReadyToUpload = true;

			if (avatarModelReferences.GetComponentInChildren<Camera> () != null) {
				modelReadyToUpload = false;
			}

			if (avatarModelReferences.GetComponentInChildren<FlareLayer> () != null) {
				modelReadyToUpload = false;
			}

			if (avatarModelReferences.GetComponentInChildren<AudioListener> () != null) {
				modelReadyToUpload = false;
			}

			if (avatarModelReferences.animator == null) {
				modelReadyToUpload = false;
			} else if (!avatarModelReferences.animator.isHuman) {
				modelReadyToUpload = false;
			}

			SkinnedMeshRenderer[] meshs = avatarModelReferences.GetComponentsInChildren<SkinnedMeshRenderer> ();
			foreach (var mesh in meshs) {
				if (!mesh.updateWhenOffscreen) {
					modelReadyToUpload = false;
					break;
				}
			}

			if (avatarModelReferences.centerEye == null) {
				modelReadyToUpload = false;
			}

			if (avatarModelReferences.mesh == null) {
				modelReadyToUpload = false;
			}

			return modelReadyToUpload;
		}

		private static bool IsFlipsideIDSet () {
			int userID = ApiClient.GetCreatorID ();

			return userID > 0;
		}

		private static bool IsModelSelected () {
			return true;
		}

		#region TUTORIAL_BOX_DRAWING

		private static void DrawManualAndAccountLinks (string docsLink = "https://www.flipsidexr.com/docs/2021.1/creator-tools") {
			DrawButton ("User Manual: Flipside Creator Tools", () => Application.OpenURL (docsLink), 25);

			int creatorID = ApiClient.GetCreatorID ();

			if (creatorID > 0) {
				DrawButton ("Sign Out (Flipside Creator ID: " + creatorID + ")", () => AuthorizationEditor.SignOut ());
			} else {
				DrawButton ("Sign Into Flipside Account", () => AuthorizationEditor.ShowSignInWindow ());
			}

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));
		}

		private void DrawLogo () {
			Texture logo = (Texture) Resources.Load ("Flipside-Creator-Tools-logo");
			float hPadding = this.position.width * 0.2f;
			float vPadding = 15f;
			Rect logoPos = new Rect (hPadding, vPadding, this.position.width - (hPadding * 2f), (this.position.width - (hPadding * 2f)) * (64f / 1024f));
			GUI.DrawTexture (logoPos, logo, ScaleMode.ScaleAndCrop);
			GUILayout.Space (((this.position.width - (hPadding * 2f)) * (64f / 1024f)) + (vPadding * 1.8f));
		}

		private static void DrawVersion () {
			GUILayout.FlexibleSpace ();

			GUIStyle versionBackgroundStyle = new GUIStyle ();
			versionBackgroundStyle.normal.background = (Texture2D) Resources.Load ("Version-Background");

			GUILayout.BeginHorizontal (versionBackgroundStyle);

			GUIStyle titleStyle = new GUIStyle ();
			titleStyle.font = titleFont;
			titleStyle.wordWrap = true;
			titleStyle.padding = new RectOffset (25, 25, 10, 15);
			titleStyle.normal.textColor = Color.white;
			titleStyle.alignment = TextAnchor.UpperCenter;

			EditorGUILayout.LabelField (string.Format ("VERSION {0}", FlipsideSettings.creatorToolsVersion), titleStyle);

			GUILayout.EndHorizontal ();
		}

		private static void DrawSelectModel () {
			DrawTitle ("SELECT YOUR MODEL");
			DrawParagraph ("Drag your character model into the Project window, then select it.");
		}

		private static void DrawParagraph (string description, Color? colorOverride = null) {
			GUIStyle paragraphStyle = new GUIStyle ();

			paragraphStyle.font = paragraphFont;
			paragraphStyle.wordWrap = true;
			paragraphStyle.padding = new RectOffset (25, 25, 10, 10);
			paragraphStyle.normal.textColor = colorOverride.GetValueOrDefault ((GUI.skin.name == "LightSkin") ? Color.black : Color.white);
			paragraphStyle.alignment = TextAnchor.UpperLeft;
			paragraphStyle.fontSize = 13;

			EditorGUILayout.LabelField (description, paragraphStyle);
		}

		private static void DrawTitle (string title, Color? colorOverride = null) {
			GUIStyle titleStyle = new GUIStyle ();

			titleStyle.font = titleFont;
			titleStyle.wordWrap = true;
			titleStyle.padding = new RectOffset (25, 25, 10, 5);
			titleStyle.normal.textColor = colorOverride.GetValueOrDefault ((GUI.skin.name == "LightSkin") ? Color.black : Color.white);
			titleStyle.alignment = TextAnchor.LowerCenter;
			titleStyle.fontSize = 20;

			EditorGUILayout.LabelField (title, titleStyle);
			GUILayout.Space (10);
		}

		private static void DrawButton (string text, Action onClicked, int height = 25) {
			GUIStyle buttonStyle = new GUIStyle ();

			buttonStyle.font = titleFont;
			buttonStyle.normal.textColor = Color.white;
			buttonStyle.margin = new RectOffset (25, 25, 15, 15);
			buttonStyle.fixedHeight = height;
			buttonStyle.normal.background = (Texture2D) Resources.Load ("Button");
			buttonStyle.hover.background = (Texture2D) Resources.Load ("Button-Hover");
			buttonStyle.hover.textColor = Color.white;

			buttonStyle.alignment = TextAnchor.MiddleCenter;

			if (GUILayout.Button (text, buttonStyle)) {
				onClicked ();
			}
		}

		#endregion TUTORIAL_BOX_DRAWING
	}
}