/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Flipside {

	[InitializeOnLoad]
	public class DuplicateSceneEditor : EditorWindow {
		private string lastAssetPath = "";
		private string folderPath = "";
		private string sceneName = "";

		private static DuplicateSceneEditor window;

		[MenuItem ("Flipside Creator Tools/Duplicate Selected Scene", false, 34)]
		public static void ShowDuplicateSceneWindow () {
			window = (DuplicateSceneEditor) EditorWindow.GetWindow (typeof (DuplicateSceneEditor), true, "Duplicate Scene");
			window.Show ();
		}

		private void OnInspectorUpdate () {
			Repaint ();
		}

		private void OnGUI () {
			int userID = ApiClient.GetCreatorID ();

			if (userID <= 0) {
				GUILayout.Label ("Please log in under Flipside Creator Tools > Sign Into Flipside Account to continue.");
				return;
			}

			if (Selection.activeObject == null) {
				GUILayout.Label ("Please select a Unity scene file in the Project window.");
				return;
			}

			string assetPath = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (assetPath == "") {
				GUILayout.Label ("Please select a Unity scene file in the Project window.");
				return;
			}

			if (lastAssetPath != assetPath) {
				if (!assetPath.EndsWith (".unity")) {
					GUILayout.Label ("Please select a Unity scene file in the Project window.");
					return;
				}

				lastAssetPath = assetPath;
				folderPath = assetPath.Substring (0, assetPath.LastIndexOf ("/") + 1);
				string originalName = assetPath.Replace (folderPath, "").Replace (".unity", "");
				sceneName = originalName + "-Copy";
			}

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("New Scene Name:", GUILayout.Width (120f));
			sceneName = GUILayout.TextField (sceneName);
			GUILayout.EndHorizontal ();

			string newPath = folderPath + sceneName + ".unity";

			if (GUILayout.Button ("Duplicate")) {
				if (File.Exists (newPath)) {
					Debug.LogError ("Please enter a new name for your duplicate scene");
				} else {
					// Duplicate scene, create new asset bundle label, and open it
					FileUtil.CopyFileOrDirectory (assetPath, newPath);
					AssetDatabase.Refresh ();

					string prefix = AssetImporter.GetAtPath (assetPath).assetBundleName.Split ('-')[0];
					string label = prefix + "-" + userID + "-" + Regex.Replace (sceneName, "([a-z])([A-Z])", "$1-$2", RegexOptions.Compiled).ToLower ().Replace ("_", "-").Replace (" ", "-").Replace ("--", "-");

					var newObj = AssetImporter.GetAtPath (newPath);
					newObj.SetAssetBundleNameAndVariant (label, "");

					EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
					EditorSceneManager.OpenScene (newPath);

					Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object> (newPath);

					window.Close ();
				}
			}

			if (File.Exists (newPath)) {
				GUILayout.Label ("Please enter a new name for your duplicate scene");
			}
		}
	}
}