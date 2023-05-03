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
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Flipside.Sets;

public class PropKitEditor : EditorWindow {
	private static string kitName = "";

	[MenuItem ("Flipside Creator Tools/Create Prop Kit", false, 34)]
	public static void CreatePropKit () {
		kitName = "";
		var window = (PropKitEditor) EditorWindow.GetWindow (typeof (PropKitEditor), true, "Create Prop Kit");
		window.Show ();
	}

	private void OnGUI () {
		int userID = Flipside.ApiClient.GetCreatorID ();

		if (userID <= 0) {
			GUILayout.Label ("Please log in under Flipside Creator Tools > Sign Into Flipside Account to continue.");
			return;
		}

		GUILayout.Label ("Choose a name or your prop kit.");
		GUILayout.Space (5);
		kitName = EditorGUILayout.TextField ("Kit Name", kitName);
		GUILayout.Space (5);

		if (kitName.Trim () != "" && GUI.Button (new Rect (5, 45, 100, 20), "Create Kit")) {
			string folderPath = GetSelectedFolder ();
			string label = "kit-" + userID + "-" + Regex.Replace (kitName, "([a-z])([A-Z])", "$1-$2", RegexOptions.Compiled).ToLower ().Replace ("_", "-").Replace (" ", "-").Replace ("--", "-");
			string kitFolder = folderPath + "/" + kitName;
			string scenePath = kitFolder + "/" + label + ".unity";

			Debug.Log ("Creating new prop kit at " + scenePath + "\nLabel: " + label);

			Directory.CreateDirectory (kitFolder + "/Prefabs");

			var scene = CreateNewScene (scenePath);
			var propKit = CreatePropKitObject (kitName);
			var kitObj = propKit.gameObject;

			EditorSceneManager.SaveScene (scene, scenePath);
			AssetImporter.GetAtPath (scenePath).SetAssetBundleNameAndVariant (label, "");
			Selection.SetActiveObjectWithContext (kitObj, kitObj);

			var window = (PropKitEditor) EditorWindow.GetWindow (typeof (PropKitEditor));
			window.Close ();
		}
	}

	private static Scene CreateNewScene (string path) {
		var scene = EditorSceneManager.NewScene (NewSceneSetup.EmptyScene, NewSceneMode.Single);
		EditorSceneManager.SaveScene (scene, path);
		return scene;
	}

	private static PropKit CreatePropKitObject (string kitName) {
		var kitObj = new GameObject (kitName);
		var propKit = kitObj.AddComponent<PropKit> ();
		propKit.kitName = kitName;
		return propKit;
	}

	private static string GetSelectedFolder () {
		Type projectWindowUtilType = typeof (ProjectWindowUtil);
		System.Reflection.MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod ("GetActiveFolderPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		object obj = getActiveFolderPath.Invoke (null, new object[0]);
		return obj.ToString ();
	}
}