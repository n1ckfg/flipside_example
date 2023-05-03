/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System;

namespace Flipside.Avatars {

	public class CharacterCreator : Editor {

		[MenuItem ("Flipside Creator Tools/Create Character From Selected Model", false, 31)]
		public static void CreateAvatarFromModelNoColliders () {
			CreateAvatarFromModel ();
		}

		public static void CreateAvatarFromModel () {
			EditorUtility.DisplayProgressBar ("Create Avatar From Model: ", "Setup", 0.2f);

			int userID = ApiClient.GetCreatorID ();

			if (userID <= 0) {
				GUILayout.Label ("Please log in under Flipside Creator Tools > Sign Into Flipside Account to continue.");
				return;
			}

			//get selected object
			string folderPath = AssetDatabase.GetAssetPath (Selection.activeObject);
			folderPath = folderPath.Substring (0, folderPath.LastIndexOf ("/") + 1);

			EditorUtility.DisplayProgressBar ("Create Avatar From Model: ", "Importing Model", 0.3f);

			try {
				ModelImporter importer = (ModelImporter) ModelImporter.GetAtPath (AssetDatabase.GetAssetPath (Selection.activeObject));
				importer.animationType = ModelImporterAnimationType.Human;
				string assetPath = Application.dataPath + "/../" + AssetDatabase.GetAssetPath (Selection.activeObject);
				assetPath = Path.GetDirectoryName (assetPath);
				string texturePath = assetPath + "/Textures";
				Directory.CreateDirectory (texturePath);
				importer.ExtractTextures (texturePath);
				importer.materialLocation = ModelImporterMaterialLocation.External;
				importer.SaveAndReimport ();
			} catch {
				EditorUtility.ClearProgressBar ();
				Debug.LogError ("Error: Object selected for import is not a Model!");
				return;
			}

			GameObject original = Selection.activeObject as GameObject;

			string avatarName = "avatar-" + userID + "-" + Regex.Replace (original.name, "([a-z])([A-Z])", "$1-$2", RegexOptions.Compiled).ToLower ().Replace ('_', '-').Replace (" ", "-").Replace ("--", "-");

			EditorUtility.DisplayProgressBar ("Create Avatar From Model: ", "Create Scene", 0.5f);

			//create scene
			var scenePath = folderPath + avatarName + ".unity";
			var scene = EditorSceneManager.NewScene (NewSceneSetup.EmptyScene, NewSceneMode.Single);
			EditorSceneManager.SaveScene (scene, scenePath);
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
			RenderSettings.ambientGroundColor = Color.black;
			RenderSettings.ambientEquatorColor = Color.grey;
			RenderSettings.ambientSkyColor = Color.white;
			RenderSettings.skybox = null;

			EditorUtility.DisplayProgressBar ("Create Avatar From Model: ", "Create Avatar Object", 1f);

			//create avatar object
			var avatarInScene = Instantiate (original);
			avatarInScene.name = avatarName;
			AvatarModelReferences references = avatarInScene.AddComponent<AvatarModelReferences> ();

			// Add Collider Generator
			GenerateColliders genColliders = avatarInScene.AddComponent<GenerateColliders> ();
			genColliders.DoOpenSelector ();

			//disable transparency
			SkinnedMeshRenderer[] meshs = avatarInScene.GetComponentsInChildren<SkinnedMeshRenderer> ();

			foreach (var mesh in meshs) {
				foreach (var material in mesh.sharedMaterials) {
					material.SetFloat ("_Mode", 0);
					material.SetInt ("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
					material.SetInt ("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
					material.SetInt ("_ZWrite", 1);
					material.DisableKeyword ("_ALPHATEST_ON");
					material.DisableKeyword ("_ALPHABLEND_ON");
					material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = -1;
				}
			}

			references.FindEmptyReferences ();
			Selection.SetActiveObjectWithContext (avatarInScene, avatarInScene);
			EditorSceneManager.SaveScene (scene);

			//create bundle
			AssetImporter.GetAtPath (scenePath).SetAssetBundleNameAndVariant (avatarName, "");

			RunImporters (references);

			EditorUtility.ClearProgressBar ();
		}

		public static void RunImporters (AvatarModelReferences avaterModelReferences) {
			List<IImport> importers = new List<IImport> ();
			importers.Add (new CharacterCreatorImport ());
			importers.Add (new DazImport ());
			importers.Add (new WolfImport ());
			importers.Add (new FuseImport ());

			foreach (IImport importer in importers) {
				if (importer.CanAutoSetup (avaterModelReferences)) {
					importer.Setup (avaterModelReferences);
				}
			}
		}
	}
}