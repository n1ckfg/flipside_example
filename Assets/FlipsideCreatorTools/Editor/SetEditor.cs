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
using UnityEngine.AI;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;
using Flipside.Sets;
using System;

public class SetEditor : EditorWindow {
	private static string setName = "";

	[MenuItem ("Flipside Creator Tools/Create Set", false, 33)]
	public static void CreateSet () {
		var window = (SetEditor) EditorWindow.GetWindow (typeof (SetEditor), true, "Create Set");
		window.Show ();
	}

	[MenuItem ("Flipside Creator Tools/Create Set From Current Scene", false, 32)]
	public static void CreateSetFromCurrent () {
		int userID = Flipside.ApiClient.GetCreatorID ();

		if (userID <= 0) {
			GUILayout.Label ("Please log in under Flipside Creator Tools > Sign Into Flipside Account to continue.");
			return;
		}

		var scene = SceneManager.GetActiveScene ();

		if (scene.rootCount > 0) {
			foreach (var go in scene.GetRootGameObjects ()) {
				var info = go.GetComponent<SetInfo> ();
				if (info != null) {
					Debug.LogError ("Scene is already a set.");
					return;
				}
			}
		}

		setName = scene.name;

		if (setName == "") {
			Debug.LogError ("Please save your scene before converting it into a set.");
			return;
		}

		string localPath = scene.path;
		string label = "set-" + userID + "-" + Regex.Replace (setName, "([a-z])([A-Z])", "$1-$2", RegexOptions.Compiled).ToLower ().Replace ("_", "-").Replace (" ", "-").Replace ("--", "-");

		Debug.Log ("Creating new set from scene: " + setName);

		var setInfo = CreateSetInfoObject (setName);
		var setObj = setInfo.gameObject;
		AddRootObject (setInfo);
		AddAudienceComponent (setInfo);

		foreach (var go in scene.GetRootGameObjects ()) {
			if (go == setObj) {
				continue;
			} else {
				go.transform.SetParent (setInfo.rootObject.transform, true);
			}
		}

		setInfo.resourceUsage = new Flipside.Helpers.ResourceUsageData ();
		setInfo.resourceUsage.UpdateInfo (setObj);

		AssetImporter.GetAtPath (localPath).SetAssetBundleNameAndVariant (label, "");

		EditorSceneManager.MarkSceneDirty (scene);
	}

	[MenuItem ("Flipside Creator Tools/Add Audience Component To Current Scene", false, 35)]
	public static void AddAudienceToCurrent () {
		var scene = SceneManager.GetActiveScene ();

		if (scene.rootCount == 0) {
			Debug.LogError ("Scene does not contain a set.");
			return;
		}

		if (scene.rootCount > 1) {
			Debug.LogError ("Scene should contain only one root element.");
			return;
		}

		var setInfo = scene.GetRootGameObjects ()[0].GetComponent<SetInfo> ();
		if (setInfo == null) {
			Debug.LogError ("Scene is not a set.");
			return;
		}

		if (setInfo.rootObject == null) {
			Debug.LogError ("The Root Object setting cannot be empty in your SetInfo component.");
			return;
		}

		if (setInfo.audience != null) {
			Debug.Log ("Scene already contains an audience component.");
			return;
		}

		AddAudienceComponent (setInfo);
	}

	private void OnGUI () {
		int userID = Flipside.ApiClient.GetCreatorID ();

		if (userID <= 0) {
			GUILayout.Label ("Please log in under Flipside Creator Tools > Sign Into Flipside Account to continue.");
			return;
		}

		GUILayout.Label ("Step 1. Choose a name for your set.");
		GUILayout.Space (5);
		setName = EditorGUILayout.TextField ("Set Name", setName);
		GUILayout.Space (5);

		if (setName.Trim () != "" && GUI.Button (new Rect (5, 50, 100, 20), "Create Set")) {
			string folderPath = GetSelectedFolder ();
			string label = "set-" + userID + "-" + Regex.Replace (setName, "([a-z])([A-Z])", "$1-$2", RegexOptions.Compiled).ToLower ().Replace ("_", "-").Replace (" ", "-").Replace ("--", "-");
			string setFolder = folderPath + "/" + setName;
			string scenePath = setFolder + "/" + label + ".unity";
			string[] res = scenePath.Split (new string[] { "/Assets/" }, StringSplitOptions.None);
			string localPath = "Assets/" + res[1];

			Debug.Log ("Creating new set at " + localPath);

			Directory.CreateDirectory (folderPath + "/" + setName);

			var scene = CreateNewScene (localPath);
			var setInfo = CreateSetInfoObject (setName);
			var setObj = setInfo.gameObject;
			AddRootObject (setInfo);
			AddAudienceComponent (setInfo);

			EditorSceneManager.SaveScene (scene, localPath);
			AssetImporter.GetAtPath (localPath).SetAssetBundleNameAndVariant (label, "");
			Selection.SetActiveObjectWithContext (setObj, setObj);

			var window = (SetEditor) EditorWindow.GetWindow (typeof (SetEditor));
			window.Close ();
		}
	}

	private static Scene CreateNewScene (string localPath) {
		var scene = EditorSceneManager.NewScene (NewSceneSetup.EmptyScene, NewSceneMode.Single);
		EditorSceneManager.SaveScene (scene, localPath);
		RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
		RenderSettings.ambientLight = Color.grey;
		RenderSettings.ambientIntensity = 0.5f;
		return scene;
	}

	private static SetInfo CreateSetInfoObject (string setName) {
		var setObj = new GameObject (setName);
		var setInfo = setObj.AddComponent<SetInfo> ();
		setInfo.setName = setName;
		return setInfo;
	}

	private static void AddRootObject (SetInfo setInfo) {
		var contents = new GameObject ("Contents");
		contents.transform.SetParent (setInfo.gameObject.transform);
		setInfo.rootObject = contents;

		var navSurface = contents.AddComponent<NavMeshSurface> ();
		navSurface.collectObjects = CollectObjects.Children;
	}

	private static void AddAudienceComponent (SetInfo setInfo) {
		var audiencePrefab = Resources.Load ("Audience");
		var audience = PrefabUtility.InstantiatePrefab (audiencePrefab as GameObject) as GameObject;
		audience.name = "Audience";
		audience.transform.SetParent (setInfo.rootObject.transform);
		audience.transform.localPosition = new Vector3 (0f, 0f, 5f);
		audience.transform.localEulerAngles = new Vector3 (0f, 180f, 0f);
		setInfo.audience = audience;
	}

	private static string GetSelectedFolder () {
		string path = Application.dataPath;

		if (Selection.activeObject != null) {
			var p = AssetDatabase.GetAssetPath (Selection.activeObject);

			var attrs = File.GetAttributes (p);
			if ((attrs & FileAttributes.Directory) == FileAttributes.Directory) {
				path = Path.GetFullPath (p).Replace ("\\", "/");
			} else {
				path = Path.GetDirectoryName (Path.GetFullPath (p)).Replace ("\\", "/");
			}
		}

		return path;
	}
}