using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEditor.Animations;
using Flipside.Avatars;
using System.Reflection;

/// <summary>
/// This is a Custom editor window that allows user to select file or folder to import BVH from and adjust import settings
/// </summary>
public class SelectorWindowBVH : EditorWindow {

	// The wrap mode to apply to the animation clips being created
	private WrapMode clipWrapMode;

	// If set to true by the user it will add the model(s) with animation attached to the scene
	private bool addToScene = true;

	// If set to true by the user it will create prefabs for for each animation
	private bool createPrefab = true;

	// Holds the file or folder to Import from, is set by using button that opens file/folder selector
	private string bvhPath = "Click File or Folder button below to populate";

	// Name of file name without path to be displayed in label
	private string modelFileName = "File Name";

	// Name of folder the files will be imported into, used to display in label
	private string destinationFolder = "Folder";

	// This is set true if its a file and false if its a folder so we know if we need to traverse a directory to get more files
	private bool bvhIsFile = true;

	// This is set if an file or folder has been selected and controls if import button is visiable so an import can't happen without selecting bvh first
	private bool bvhSelected = false;

	// If set to true the system will generate a Mechanim animation
	private bool createMechanimSelected = true;

	// If set to true the system will generate a controller for mechanim animation (if add to scene is true it will also create controller as it is needed to add to scene)
	private bool createControllerSelected = true;

	// If set to true a copy of the model will be made and imported in legacy mode, then a legacy animation will be made
	private bool createLegacySelected = false;

	// Last values to detect change and insure at least one import type is selected
	private bool createMechanimSelectedLast = true;

	private bool createLegacySelectedLast = false;

	// For file/foldre popup dialouge, to filet out all but the files we are intrested in (BVH)
	private string[] filters = { "Bvh files", "bvh", "All files", "*" };

	/// <summary>
	/// Used before opening the window to allow BvhImportanimation singelton to set initial data to display in window
	/// </summary>
	/// <param name="newFileName">File name to display as label</param>
	/// <param name="newDestinationFolder">Destination folder to display as label</param>
	public void SetData (string newFileName, string newDestinationFolder) {
		modelFileName = newFileName;
		destinationFolder = newDestinationFolder;
	}

	/// <summary>
	/// Draws the window and responds to button presses by calling functions on BvhImportAnimations singelton
	/// </summary>
	private void OnGUI () {
		GUILayout.Label ("Selected Model", EditorStyles.boldLabel);
		EditorGUILayout.SelectableLabel (modelFileName, EditorStyles.textField, GUILayout.Height (EditorGUIUtility.singleLineHeight));

		GUILayout.Label ("Destination Folder", EditorStyles.boldLabel);
		EditorGUILayout.SelectableLabel (destinationFolder, EditorStyles.textField, GUILayout.Height (EditorGUIUtility.singleLineHeight));

		GUILayout.Label ("Mechanim Animation", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal ();
		createMechanimSelected = EditorGUILayout.ToggleLeft ("  Import", createMechanimSelected);
		createControllerSelected = EditorGUILayout.ToggleLeft ("  Create Controller", createControllerSelected);
		EditorGUILayout.EndHorizontal ();

		GUILayout.Label ("Legacy Animation", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal ();
		createLegacySelected = EditorGUILayout.ToggleLeft ("  Import", createLegacySelected);
		GUILayout.Label ("Wrap Mode:");
		clipWrapMode = (WrapMode) EditorGUILayout.EnumPopup ("", clipWrapMode, GUILayout.Width (80));
		GUILayout.Space (25);
		EditorGUILayout.EndHorizontal ();

		if (createMechanimSelected == false && createLegacySelected == false) {
			if (createMechanimSelectedLast != createMechanimSelected) {
				createLegacySelected = true;
				createLegacySelectedLast = true;
			}
			if (createLegacySelectedLast != createLegacySelected) {
				createMechanimSelected = true;
				createMechanimSelectedLast = true;
			}
		}

		createMechanimSelectedLast = createMechanimSelected;
		createLegacySelectedLast = createLegacySelected;

		GUILayout.Label ("Options", EditorStyles.boldLabel);
		addToScene = EditorGUILayout.ToggleLeft ("  Add To Scene", addToScene);
		createPrefab = EditorGUILayout.ToggleLeft ("  Create Prefab", createPrefab);

		if (bvhPath == "") {
			GUILayout.Label ("Bvh", EditorStyles.boldLabel);
		} else {
			if (bvhIsFile == true) {
				GUILayout.Label ("Bvh (File)", EditorStyles.boldLabel);
			} else {
				GUILayout.Label ("Bvh (Folder)", EditorStyles.boldLabel);
			}
		}

		EditorGUILayout.SelectableLabel (Path.GetFileName (bvhPath), EditorStyles.textField, GUILayout.Height (EditorGUIUtility.singleLineHeight));
		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal ();
		{
			if (GUILayout.Button ("Choose File for Import")) {
				string filePath = EditorUtility.OpenFilePanelWithFilters ("Select BVH file to create animation from", BvhImportAnimation.GetDefaultFolderPath (), filters);

				if (filePath != "") {
					bvhPath = filePath;
					bvhSelected = true;
					bvhIsFile = true;
				} else {
					bvhPath = "Click File or Folder button below to populate";
					bvhSelected = false;
				}
			}
			if (GUILayout.Button ("Choose Folder for Import")) {
				string filePath = EditorUtility.OpenFolderPanel ("Select BVH file to create animation from", BvhImportAnimation.GetDefaultFolderPath (), "");

				if (filePath != "") {
					bvhPath = filePath;
					bvhSelected = true;
					bvhIsFile = false;
				} else {
					bvhPath = "Click File or Folder button below to populate";
					bvhSelected = false;
				}
			}
		}
		EditorGUILayout.EndHorizontal ();

		GUILayout.Space (20);
		if (bvhSelected == true) {
			if (GUILayout.Button ("Import")) { //&& bvhSelected == true
				BvhImportAnimation.ImportBvhForSelectedModel (clipWrapMode, addToScene, createPrefab, bvhIsFile, bvhPath, createMechanimSelected, createLegacySelected, createControllerSelected);
				Close ();
			}
		}
	}
}

/// <summary>
/// Error window is a custom editor window used to display errors as a popup.  Use BvhImportAnimation.ShowError() to open this window with a specific message.
/// </summary>
public class ErrorWindow : EditorWindow {

	// Message to use in text box area
	private string errorMessage = "Error not set!";

	// Style for error text box
	private GUIStyle lableStyle;

	// Size of text box
	private int size = 3;

	/// <summary>
	/// Used to set the data this window will display prior to opening
	/// </summary>
	/// <param name="newError">Error to display in the text box</param>
	/// <param name="newSize">Size of the text box, so we can have longer messages if needed</param>
	public void SetData (string newError, int newSize) {
		errorMessage = newError;
		lableStyle = EditorStyles.textArea;
		lableStyle.normal.textColor = new Color (1f, 0f, 0f);
		size = newSize;
	}

	/// <summary>
	/// Draw window and handle butotn clicks
	/// </summary>
	private void OnGUI () {
		GUILayout.Space (10);
		EditorGUILayout.SelectableLabel (errorMessage, lableStyle, GUILayout.Height (EditorGUIUtility.singleLineHeight * size));
		GUILayout.Space (10);

		if (GUILayout.Button ("OK")) {
			Close ();
		}
	}
}

/// <summary>
/// This singelton class is used to control all aspects of import and is accessed by using the flipside menue (Flipside Creator Tools -> Import BVH for Selected Model)
/// <br>We only support importing BVH data that was exported from Flipside.  The model you choose before using (Import BVH for Selected Model) should match the one that was exported.</br>
/// </summary>
public class BvhImportAnimation : MonoBehaviour {

	// This obtained using the GetDefaultFolderPath () function which populates it if empty, is the root folder flipside exports BVH data to
	private static string defaultFolderPath = "";

	// Holds the script that is temporarily added to the model with animation so we can perfom export with objects in structure BVHAnimationLoader is expecting
	private static BVHAnimationLoader bvhLoader;

	// Our reference to the selector window to avoid recreating it every time its opened
	private static SelectorWindowBVH selectorWindowBVH;

	// Our reference to the error window to avoid recreating it every time its opened
	private static ErrorWindow errorWindow;

	// Our reference to the selected object, it could change between time selector window is opend and import is hit so we remeber what it was at the start
	private static UnityEngine.Object selectedObject;

	// The base directory we will export all anmiation/model/prefabs to
	private static string baseImportFolder = "Assets/Flipside Bvh Import/";

	/// <summary>
	/// Convinence function to open an error popup with a specific message and height
	/// </summary>
	/// <param name="errorMessage">Error message to display</param>
	/// <param name="size">Number of lines in error message, defaults to 3 if empty, should be 3 or higher</param>
	public static void ShowError (string errorMessage, int size = 3) {
		// Create the window if it dosn't exist already
		if (errorWindow == null) {
			errorWindow = ScriptableObject.CreateInstance<ErrorWindow> ();
			errorWindow.titleContent.text = "Error:";
			errorWindow.name = "Error:";
		}

		float windowExtra = 0;

		// If its bigger we need to make the window bigger as well
		if (size > 3) {
			windowExtra = 20 * (size - 3);
		}

		float windowWidth = 300f;
		float windowHeight = 90f + windowExtra;

		errorWindow.SetData (errorMessage, size);

		// Center window on screen and display
		errorWindow.ShowUtility ();
		errorWindow.position = new Rect ((Screen.currentResolution.width / 2f) - (windowWidth / 2f), (Screen.currentResolution.height / 2f) - (windowHeight / 2f), windowWidth, windowHeight);
	}

	/// <summary>
	/// Will create the path to Flipside Studios export folder and remember it to avoid re creating every time
	/// </summary>
	/// <returns>Path to Flipside Studio's default export folder.</returns>
	public static string GetDefaultFolderPath () {
		if (defaultFolderPath == "") {
			string docs = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			string wdir = docs.Replace ('\\', '/') + "/Flipside Studio/BVH Exports/";
			defaultFolderPath = Directory.CreateDirectory (wdir).FullName;
		}

		return defaultFolderPath;
	}

	/// <summary>
	/// Main function to preform import. Called from Menu at top of unity editor
	/// </summary>
	[MenuItem ("Flipside Creator Tools/Import BVH for Selected Model", false, 36)]
	public static void ImportBVHforSelectedModel () {
		selectedObject = Selection.activeObject;

		// We first see if the selected object is a model and prompt user with error popup if it isn't
		try {
			ModelImporter importer = (ModelImporter) ModelImporter.GetAtPath (AssetDatabase.GetAssetPath (selectedObject));
			importer.SaveAndReimport ();
		} catch {
			ShowError ("Object selected for import is not a Model!");
			return;
		}

		// Get file info
		string fileNameNoExt = Path.GetFileNameWithoutExtension (AssetDatabase.GetAssetPath (selectedObject));
		string destinationFolder = baseImportFolder + fileNameNoExt;

		// Create the window if it dosn't exist already
		if (selectorWindowBVH == null) {
			selectorWindowBVH = ScriptableObject.CreateInstance<SelectorWindowBVH> ();
			selectorWindowBVH.titleContent.text = "Import Options:";
			selectorWindowBVH.name = "Import Options:";
		}

		float windowWidth = 400f;
		float windowHeight = 350f;

		// Center window on screen and display
		selectorWindowBVH.SetData (fileNameNoExt, destinationFolder);
		selectorWindowBVH.ShowUtility ();
		selectorWindowBVH.position = new Rect ((Screen.currentResolution.width / 2f) - (windowWidth / 2f), (Screen.currentResolution.height / 2f) - (windowHeight / 2f), windowWidth, windowHeight);
	}

	/// <summary>
	/// Called from selector window this function will call CreateAnimationFromBvh() for a single file or each file in a folder if bvhIsFile = false
	/// </summary>
	/// <param name="clipWrapMode">Sets Clip to this wrap mode after creating it</param>
	/// <param name="addToScene">Adds to the current scene if this is true</param>
	/// <param name="createPrefab">Creates prefabs for each animation if this is set to true</param>
	/// <param name="bvhIsFile">True if bvhPath points to a file, False if it is a folder</param>
	/// <param name="bvhPath">File or folder path to import from</param>
	public static void ImportBvhForSelectedModel (WrapMode clipWrapMode, bool addToScene, bool createPrefab, bool bvhIsFile, string bvhPath, bool createMechanim, bool createLegacy, bool createController) {
		// List of files to process
		List<string> bvhFilePaths = new List<string> ();

		if (bvhIsFile == true) {
			// We just add the one file to process if (bvhIsFile) flag is set to true
			bvhFilePaths.Add (bvhPath);
		} else {
			// This is a folder path so we add all bvh files under folder (bvhPath) to the list of files to process
			string[] bvhFiles = Directory.GetFiles (bvhPath, "*.bvh");

			foreach (string file in bvhFiles) {
				bvhFilePaths.Add (file);
			}
		}

		bool filesImported = false;

		// Process each file
		foreach (string file in bvhFilePaths) {
			bool success = CreateAnimationFromBvh (clipWrapMode, addToScene, createPrefab, file, createMechanim, createLegacy, createController);

			if (success == true) {
				filesImported = true;
			}
		}

		// If no files were imported we see if it was a file or folder and notify user of issue
		if (filesImported == false) {
			if (bvhIsFile)
				ShowError ("No animations created. The bvh file provided did not contain data for this model.  Ensure your using bvh files exported from Flipside Studio for this model.", 4);
			else
				ShowError ("No animations created. The bvh files provided did not contain data for this model.  Ensure your using bvh files exported from Flipside Studio for this model.", 4);
		}
	}

	/// <summary>
	/// This function will copy the selected model to the Flipside Import directory and convert it to legacy.  If it already exists it uses one there
	/// <br>Once copy is created or found we create an Animation clip and save it under Flipside Import directory</br>
	/// <br>It also creates the scene object and prefab if requested</br>
	/// </summary>
	/// <param name="clipWrapMode">Sets Clip to this wrap mode after creating it</param>
	/// <param name="addToScene">Adds to the current scene if this is true</param>
	/// <param name="createPrefab">Creates prefabs for each animation if this is set to true</param>
	/// <param name="bvhPath">File or folder path to import from</param>
	/// <returns></returns>
	public static bool CreateAnimationFromBvh (WrapMode clipWrapMode, bool addToScene, bool createPrefab, string bvhPath, bool createMechanim, bool createLegacy, bool createController) {
		// We need to instantiate orignial avatar in scene as BVHAnimationLoader will need it to function correctly
		GameObject avatarInScene = null;

		try {
			// Get selected model and instantiate it in the scene
			GameObject original = selectedObject as GameObject;
			avatarInScene = Instantiate (original);

			// Create the string to append to file names and game objects so we know what type of animation wrap it has
			// This also allows people to import multiple times with different wraps without overwritting old
			string wrapModeFileID = "-wd";

			switch (clipWrapMode) {
				case WrapMode.Once:
					wrapModeFileID = "-wo";
					break;

				case WrapMode.Loop:
					wrapModeFileID = "-wl";
					break;

				case WrapMode.ClampForever:
					wrapModeFileID = "-wc";
					break;

				case WrapMode.PingPong:
					wrapModeFileID = "-wp";
					break;
			}

			// Path to import file to, in the Assets root directory
			string fileNameNoExt = Path.GetFileNameWithoutExtension (AssetDatabase.GetAssetPath (selectedObject));
			DirectoryInfo parentBvhDir = Directory.GetParent (bvhPath);

			// If the file is at the root of the file system for some reason
			string parentDirName = parentBvhDir.Name;
			if (parentDirName.Contains (":") == true) {
				parentDirName = parentDirName[0].ToString ();
			}

			// Create various paths needed for output
			string assetPath = baseImportFolder + fileNameNoExt + "/Animations/" + parentDirName + "/" + Path.GetFileName (AssetDatabase.GetAssetPath (selectedObject));
			string assetPathNoFile = Path.GetDirectoryName (assetPath);

			string modelPath = baseImportFolder + fileNameNoExt + "/Model/" + Path.GetFileName (AssetDatabase.GetAssetPath (selectedObject));
			string modelPathNoFile = Path.GetDirectoryName (modelPath);

			string texturePath = modelPathNoFile + "/Textures";

			// Need to ensure directoreis exists before creating files
			Directory.CreateDirectory (assetPathNoFile);
			Directory.CreateDirectory (Path.GetDirectoryName (modelPath));

			// Check to see if we already imported this model once.  If so we don't bother doing it again, it will break references to other prefabes created earlier
			if (File.Exists (modelPath) == false) {
				// Copy asset and re import under Flipside Studio Bvh Import Directory with legacy settings
				AssetDatabase.CopyAsset (AssetDatabase.GetAssetPath (selectedObject), modelPath);

				ModelImporter importer = (ModelImporter) ModelImporter.GetAtPath (modelPath);
				importer.animationType = ModelImporterAnimationType.Legacy;

				Directory.CreateDirectory (texturePath);
				importer.ExtractTextures (texturePath);
				importer.materialLocation = ModelImporterMaterialLocation.External;
				importer.SaveAndReimport ();
			}

			// Load the new or existing legacy model into temp
			GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject> (modelPath);
			// Create the animation
			AnimationClip newClip = CreateAnimationClip (bvhPath, avatarInScene, assetPathNoFile, wrapModeFileID, createLegacy);
			// Set the new clip to correct wrap mode
			newClip.wrapMode = clipWrapMode;

			// Create the new avatar in the scene so we can make a prefab if needed
			GameObject newAvatarInScene = Instantiate (temp);
			newAvatarInScene.name = Path.GetFileNameWithoutExtension (bvhPath) + wrapModeFileID;
			Animation anim = newAvatarInScene.GetComponent<Animation> ();
			anim.clip = newClip;

			// Get avatarInSceen's Animator
			Animator avatarInSceneAnimator = avatarInScene.GetComponent<Animator> ();
			Animation avatarInSceneAnimation = avatarInScene.GetComponent<Animation> ();

			// Create a mechanim version of the animation
			if (createMechanim == true) CreateMecanimAnimationClipFromLegacyAnimation (bvhPath, avatarInScene, newAvatarInScene, assetPathNoFile, (addToScene || createController));

			// User wants prefab so we create the directory and save out the new model as a prefab
			if (createLegacy == true && createPrefab == true) {
				Directory.CreateDirectory (baseImportFolder + fileNameNoExt + "/Prefabs/" + parentDirName);
				string prefabName = baseImportFolder + fileNameNoExt + "/Prefabs/" + parentDirName + "/" + Path.GetFileNameWithoutExtension (bvhPath) + wrapModeFileID + ".prefab";
				PrefabUtility.SaveAsPrefabAssetAndConnect (newAvatarInScene, prefabName, InteractionMode.AutomatedAction);
			}

			// If the user choose to have a Mechanim animation created we clean up object and save prefab if needed
			if (createMechanim == true) {
				avatarInScene.name = Path.GetFileNameWithoutExtension (bvhPath);

				if (avatarInSceneAnimator != null) {
					avatarInSceneAnimator.applyRootMotion = false;
				}
				if (avatarInSceneAnimation != null) {
					DestroyImmediate (avatarInSceneAnimation);
				}

				if (createPrefab == true) {
					Directory.CreateDirectory (baseImportFolder + fileNameNoExt + "/Prefabs/" + parentDirName);
					string prefabName = baseImportFolder + fileNameNoExt + "/Prefabs/" + parentDirName + "/" + Path.GetFileNameWithoutExtension (bvhPath) + ".prefab";
					PrefabUtility.SaveAsPrefabAssetAndConnect (avatarInScene, prefabName, InteractionMode.AutomatedAction);
				}
			} else {
				// Destroy the instantiated orignal since we arn't making a mechanim animation
				DestroyImmediate (avatarInScene);
			}

			// If user did not select legacy animation then clean up the model we used to copy the animation from
			if (createLegacy == false) {
				DestroyImmediate (newAvatarInScene);

				string[] modelDirs = Directory.GetDirectories (modelPathNoFile);

				foreach (string dir in modelDirs) {
					Directory.Delete (dir, true);
				}

				Directory.Delete (modelPathNoFile, true);
			}

			// If we don't want to add to scene we just destory the original, otherwise user wanted it so we leave it in scene
			if (addToScene == false) {
				DestroyImmediate (avatarInScene);
				DestroyImmediate (newAvatarInScene);
			}
		} catch (Exception ex) {
			// On errors log it to user and destroy the instantiated avatarInScene as cleanup
			Debug.Log (ex);
			if (avatarInScene != null)
				DestroyImmediate (avatarInScene);
			return false;
		}

		return true;
	}

	public static string[] GetActualMuscleNames () {
		string[] ActualMuscleNames = HumanTrait.MuscleName;

		// Most muscle names from HumanTrait.MuscleName can be used as key in Mecanim Animation but a few don't match when you add manually, this fixes names

		// Left
		ActualMuscleNames[55] = "LeftHand.Thumb.1 Stretched";
		ActualMuscleNames[56] = "LeftHand.Thumb.Spread";
		ActualMuscleNames[57] = "LeftHand.Thumb.2 Stretched";
		ActualMuscleNames[58] = "LeftHand.Thumb.3 Stretched";

		ActualMuscleNames[59] = "LeftHand.Index.1 Stretched";
		ActualMuscleNames[60] = "LeftHand.Index.Spread";
		ActualMuscleNames[61] = "LeftHand.Index.2 Stretched";
		ActualMuscleNames[62] = "LeftHand.Index.3 Stretched";

		ActualMuscleNames[63] = "LeftHand.Middle.1 Stretched";
		ActualMuscleNames[64] = "LeftHand.Middle.Spread";
		ActualMuscleNames[65] = "LeftHand.Middle.2 Stretched";
		ActualMuscleNames[66] = "LeftHand.Middle.3 Stretched";

		ActualMuscleNames[67] = "LeftHand.Ring.1 Stretched";
		ActualMuscleNames[68] = "LeftHand.Ring.Spread";
		ActualMuscleNames[69] = "LeftHand.Ring.2 Stretched";
		ActualMuscleNames[70] = "LeftHand.Ring.3 Stretched";

		ActualMuscleNames[71] = "LeftHand.Little.1 Stretched";
		ActualMuscleNames[72] = "LeftHand.Little.Spread";
		ActualMuscleNames[73] = "LeftHand.Little.2 Stretched";
		ActualMuscleNames[74] = "LeftHand.Little.3 Stretched";

		// Right
		ActualMuscleNames[75] = "RightHand.Thumb.1 Stretched";
		ActualMuscleNames[76] = "RightHand.Thumb.Spread";
		ActualMuscleNames[77] = "RightHand.Thumb.2 Stretched";
		ActualMuscleNames[78] = "RightHand.Thumb.3 Stretched";

		ActualMuscleNames[79] = "RightHand.Index.1 Stretched";
		ActualMuscleNames[80] = "RightHand.Index.Spread";
		ActualMuscleNames[81] = "RightHand.Index.2 Stretched";
		ActualMuscleNames[82] = "RightHand.Index.3 Stretched";

		ActualMuscleNames[83] = "RightHand.Middle.1 Stretched";
		ActualMuscleNames[84] = "RightHand.Middle.Spread";
		ActualMuscleNames[85] = "RightHand.Middle.2 Stretched";
		ActualMuscleNames[86] = "RightHand.Middle.3 Stretched";

		ActualMuscleNames[87] = "RightHand.Ring.1 Stretched";
		ActualMuscleNames[88] = "RightHand.Ring.Spread";
		ActualMuscleNames[89] = "RightHand.Ring.2 Stretched";
		ActualMuscleNames[90] = "RightHand.Ring.3 Stretched";

		ActualMuscleNames[91] = "RightHand.Little.1 Stretched";
		ActualMuscleNames[92] = "RightHand.Little.Spread";
		ActualMuscleNames[93] = "RightHand.Little.2 Stretched";
		ActualMuscleNames[94] = "RightHand.Little.3 Stretched";

		return ActualMuscleNames;
	}

	public static void FindScale (HumanPoseHandler targetHandler, Transform targetTransform, ref float avgScale, ref Vector3 bodyOffset) {
		avgScale = 1f;
		bodyOffset = Vector3.zero;

		if (targetHandler != null && targetTransform != null) {
			HumanPose currentPose = new HumanPose ();

			targetTransform.position = Vector3.zero;

			targetHandler.GetHumanPose (ref currentPose);

			bodyOffset = currentPose.bodyPosition;

			targetTransform.position = Vector3.one;

			targetHandler.GetHumanPose (ref currentPose);

			Vector3 OffsetBody = currentPose.bodyPosition;

			Vector3 scale = OffsetBody - bodyOffset;
			avgScale = (scale.x + scale.y + scale.z) / 3f;

			targetTransform.position = Vector3.zero;
		}
	}

	public static AnimationClip CreateMecanimAnimationClipFromLegacyAnimation (string path, GameObject sourceObj, GameObject targetObj, string animationPath, bool createController) {
		// Setup path and file names
		string clipName = Path.GetFileNameWithoutExtension (path);
		string clipPath = animationPath + "/" + clipName + ".anim";
		string controllerPath = animationPath + "/" + clipName + ".controller";

		// Create Clip
		AnimationClip clip = new AnimationClip ();
		clip.name = "Test Output Clip";
		clip.legacy = false;

		// Get Actual Muscle name, for some reason the finger names arn't correct in array
		string[] MuscleName = GetActualMuscleNames ();

		// Populate new clip with data
		HumanPose currentPose = new HumanPose ();
		HumanPose originalPose = new HumanPose ();
		Animator sourceAnimator = sourceObj.GetComponent<Animator> ();
		Transform hipTransformSource = sourceAnimator.GetBoneTransform (HumanBodyBones.Hips);
		HumanPoseHandler humanPoseHandlerSource = new HumanPoseHandler (sourceAnimator.avatar, sourceObj.transform);
		humanPoseHandlerSource.GetHumanPose (ref originalPose);   // Save original pose
		humanPoseHandlerSource.SetHumanPose (ref currentPose);    // Set pose to default, all normalized muscles to zero to get hip to root offset in this context

		// Save original hip offset
		Vector3 originalLocalHip = hipTransformSource.position;
		Debug.Log ("originalLocalHip: " + originalLocalHip);

		// Reset back to original defaults to make animation copy work correctly
		humanPoseHandlerSource.SetHumanPose (ref originalPose);

		float timeIncrement = 1f / 60f;
		float startTime = timeIncrement * 2;    // Start on even frame #2
		timeIncrement = timeIncrement * 2;      // Increment for every second frame (with start time we will jump between even frames starting at 2) [2,4,6..]

		Dictionary<string, List<Keyframe>> allNewKeyFrames = new Dictionary<string, List<Keyframe>> ();

		string locationKey = "localPosition";
		string rotationKey = "localRotation";

		// Add Postion keys
		if (allNewKeyFrames.ContainsKey (locationKey + ".x") == false) {
			allNewKeyFrames.Add (locationKey + ".x", new List<Keyframe> ());
		}

		if (allNewKeyFrames.ContainsKey (locationKey + ".y") == false) {
			allNewKeyFrames.Add (locationKey + ".y", new List<Keyframe> ());
		}

		if (allNewKeyFrames.ContainsKey (locationKey + ".z") == false) {
			allNewKeyFrames.Add (locationKey + ".z", new List<Keyframe> ());
		}

		// Add Rotation keys
		if (allNewKeyFrames.ContainsKey (rotationKey + ".x") == false) {
			allNewKeyFrames.Add (rotationKey + ".x", new List<Keyframe> ());
		}

		if (allNewKeyFrames.ContainsKey (rotationKey + ".y") == false) {
			allNewKeyFrames.Add (rotationKey + ".y", new List<Keyframe> ());
		}

		if (allNewKeyFrames.ContainsKey (rotationKey + ".z") == false) {
			allNewKeyFrames.Add (rotationKey + ".z", new List<Keyframe> ());
		}

		if (allNewKeyFrames.ContainsKey (rotationKey + ".w") == false) {
			allNewKeyFrames.Add (rotationKey + ".w", new List<Keyframe> ());
		}

		// More Keys
		allNewKeyFrames.Add ("LeftFootT.x", new List<Keyframe> ());
		allNewKeyFrames.Add ("LeftFootT.y", new List<Keyframe> ());
		allNewKeyFrames.Add ("LeftFootT.z", new List<Keyframe> ());

		allNewKeyFrames.Add ("LeftFootQ.x", new List<Keyframe> ());
		allNewKeyFrames.Add ("LeftFootQ.y", new List<Keyframe> ());
		allNewKeyFrames.Add ("LeftFootQ.z", new List<Keyframe> ());
		allNewKeyFrames.Add ("LeftFootQ.w", new List<Keyframe> ());

		allNewKeyFrames.Add ("RightFootT.x", new List<Keyframe> ());
		allNewKeyFrames.Add ("RightFootT.y", new List<Keyframe> ());
		allNewKeyFrames.Add ("RightFootT.z", new List<Keyframe> ());

		allNewKeyFrames.Add ("RightFootQ.x", new List<Keyframe> ());
		allNewKeyFrames.Add ("RightFootQ.y", new List<Keyframe> ());
		allNewKeyFrames.Add ("RightFootQ.z", new List<Keyframe> ());
		allNewKeyFrames.Add ("RightFootQ.w", new List<Keyframe> ());

		// Get legacy Animation
		Animation legacyAnimation = sourceObj.GetComponent<Animation> ();

		// Find average scale and body offset
		Vector3 bodyOffset = Vector3.zero;
		float avgScale = 1f;

		FindScale (humanPoseHandlerSource, sourceObj.transform, ref avgScale, ref bodyOffset);

		// Fix Y of bodyOffset (Unknown why this is necessary for just Y but this fixes issue where vertical alignment is slightly off; arrived at it by trial and error)
		bodyOffset.y = bodyOffset.y - originalPose.bodyPosition.y;

		// We go through the legacy animation clip and sample at every second interval, starting at 2 (2,4,6...)
		for (float i = startTime; i <= legacyAnimation.clip.length; i = i + timeIncrement) {
			legacyAnimation.transform.position = Vector3.zero;

			// Set the legacy model to position in animation for this frame
			legacyAnimation.clip.SampleAnimation (legacyAnimation.gameObject, i);

			// Grab the human pose data for frame (Gives us the nomralized muscle values for the frame)
			humanPoseHandlerSource.GetHumanPose (ref currentPose);

			Vector3 newBodyPos = currentPose.bodyPosition - bodyOffset;
			newBodyPos = newBodyPos / avgScale;

			// Add body position to animation accounting for offset
			allNewKeyFrames[locationKey + ".x"].Add (new Keyframe (i, newBodyPos.x));
			allNewKeyFrames[locationKey + ".y"].Add (new Keyframe (i, newBodyPos.y));
			allNewKeyFrames[locationKey + ".z"].Add (new Keyframe (i, newBodyPos.z));

			// Add body rotation to animation
			allNewKeyFrames[rotationKey + ".x"].Add (new Keyframe (i, currentPose.bodyRotation.x));
			allNewKeyFrames[rotationKey + ".y"].Add (new Keyframe (i, currentPose.bodyRotation.y));
			allNewKeyFrames[rotationKey + ".z"].Add (new Keyframe (i, currentPose.bodyRotation.z));
			allNewKeyFrames[rotationKey + ".w"].Add (new Keyframe (i, currentPose.bodyRotation.w));

			// Get Rotation and postion of body and feet
			VecQuad bodyVQ = new VecQuad (newBodyPos, currentPose.bodyRotation);
			VecQuad LeftFootVQ = new VecQuad (sourceAnimator.GetBoneTransform (HumanBodyBones.LeftFoot));
			VecQuad RightFootVQ = new VecQuad (sourceAnimator.GetBoneTransform (HumanBodyBones.RightFoot));
			LeftFootVQ = GetGoalVQ (sourceAnimator, HumanBodyBones.LeftFoot, bodyVQ, LeftFootVQ);
			RightFootVQ = GetGoalVQ (sourceAnimator, HumanBodyBones.RightFoot, bodyVQ, RightFootVQ);

			// Add Left Foot Rotation and Position
			allNewKeyFrames["LeftFootT.x"].Add (new Keyframe (i, LeftFootVQ.v.x));
			allNewKeyFrames["LeftFootT.y"].Add (new Keyframe (i, LeftFootVQ.v.y));
			allNewKeyFrames["LeftFootT.z"].Add (new Keyframe (i, LeftFootVQ.v.z));

			allNewKeyFrames["LeftFootQ.x"].Add (new Keyframe (i, LeftFootVQ.q.x));
			allNewKeyFrames["LeftFootQ.y"].Add (new Keyframe (i, LeftFootVQ.q.y));
			allNewKeyFrames["LeftFootQ.z"].Add (new Keyframe (i, LeftFootVQ.q.z));
			allNewKeyFrames["LeftFootQ.w"].Add (new Keyframe (i, LeftFootVQ.q.w));

			// Add Right Foot Rotation and Position
			allNewKeyFrames["RightFootT.x"].Add (new Keyframe (i, RightFootVQ.v.x));
			allNewKeyFrames["RightFootT.y"].Add (new Keyframe (i, RightFootVQ.v.y));
			allNewKeyFrames["RightFootT.z"].Add (new Keyframe (i, RightFootVQ.v.z));

			allNewKeyFrames["RightFootQ.x"].Add (new Keyframe (i, RightFootVQ.q.x));
			allNewKeyFrames["RightFootQ.y"].Add (new Keyframe (i, RightFootVQ.q.y));
			allNewKeyFrames["RightFootQ.z"].Add (new Keyframe (i, RightFootVQ.q.z));
			allNewKeyFrames["RightFootQ.w"].Add (new Keyframe (i, RightFootVQ.q.w));

			// Add value of each muscle for this frame to animation
			int keyIndex = 0;
			foreach (float keyValue in currentPose.muscles) {
				string muscleName = MuscleName[keyIndex];

				// First time we need to add the bone name to the dictonary
				if (allNewKeyFrames.ContainsKey (muscleName) == false) {
					allNewKeyFrames.Add (muscleName, new List<Keyframe> ());
				}

				// Each muscle will add its value for this frame to the dictonary
				allNewKeyFrames[muscleName].Add (new Keyframe (i, keyValue));
				keyIndex++;
			}
		}

		// We want to write all frames for each muscle all at once, go through muscles writting all frames for each into animation clip
		foreach (KeyValuePair<string, List<Keyframe>> muscle in allNewKeyFrames) {
			if (muscle.Key.Contains (locationKey) || muscle.Key.Contains (rotationKey)) {
				clip.SetCurve ("", typeof (Transform), muscle.Key, new AnimationCurve (muscle.Value.ToArray ()));
			} else {
				clip.SetCurve ("", typeof (Animator), muscle.Key, new AnimationCurve (muscle.Value.ToArray ()));
			}
		}

		// Creates the controller
		var controller = AnimatorController.CreateAnimatorControllerAtPath (controllerPath);

		// Add StateMachines
		var rootStateMachine = controller.layers[0].stateMachine;

		// Add States
		AnimatorState newClipState = rootStateMachine.AddState (clipName);
		newClipState.name = clipName;

		// Assign clip to Controller state
		newClipState.motion = clip;

		// Assign controller to the source
		sourceAnimator.runtimeAnimatorController = controller;

		// Itterate over frames of newley created animation to ensure it is in correct position by comparing hip locations
		Animator orignalAnimator = targetObj.AddComponent<Animator> ();
		orignalAnimator.avatar = sourceAnimator.avatar;

		// Cleanup
		allNewKeyFrames.Clear ();

		// Cleanup
		allNewKeyFrames.Clear ();

		// Save Clip to file
		AssetDatabase.CreateAsset (clip, clipPath);
		AssetDatabase.SaveAssets ();
		AssetDatabase.ImportAsset (clipPath);
		AssetDatabase.SaveAssets ();

		// Cleanup, we don't need Animator on orignal.  We were just using it to get Hip position
		DestroyImmediate (orignalAnimator);

		// If a controller is requested we create it and assgin the clip
		if (createController == false) {
			File.Delete (controllerPath);
		}

		return clip;
	}

	/// <summary>
	/// This function creates an animation clip using bvh in (path) by adding (BVHAnimationLoader) to (originalModel) and running import functions
	/// </summary>
	/// <param name="path">Full path to bvh file</param>
	/// <param name="originalModel">Original model that bvh data should map to exactly</param>
	/// <param name="animationPath">The folder to export the clip to</param>
	/// <param name="wrapModeFileID">The string to add to end of filename that indicates its wrap mode</param>
	/// <returns></returns>
	public static AnimationClip CreateAnimationClip (string path, GameObject originalModel, string animationPath, string wrapModeFileID, bool saveClip) {
		string clipName = Path.GetFileNameWithoutExtension (path);

		AnimationClip outClip;

		// Make same adjustments to characters that happen during character creation imports
		AvatarModelReferences references = originalModel.AddComponent<AvatarModelReferences> ();
		references.FindEmptyReferences ();
		CharacterCreator.RunImporters (references);
		DestroyImmediate (references);

		// Setup BVHAnimationLoader
		bvhLoader = originalModel.AddComponent<BVHAnimationLoader> ();

		bvhLoader.boneRenamingMap = new BVHAnimationLoader.FakeDictionary[0];
		bvhLoader.autoPlay = false;
		bvhLoader.autoStart = false;
		bvhLoader.flexibleBoneNames = true;

		bvhLoader.filename = path;
		bvhLoader.clipName = "temp";

		bvhLoader.targetAvatar = FindObjectOfType<Animator> ();

		bvhLoader.parseFile ();
		bvhLoader.loadAnimation ();

		// If Legacy animation wasn't requested we don't save out the file, just return the clip for processing
		if (saveClip == true) {
			Directory.CreateDirectory (Path.GetDirectoryName (animationPath));

			// Create the clip
			string clipPath = animationPath + "/" + clipName + wrapModeFileID + ".anim";
			AssetDatabase.CreateAsset (bvhLoader.clip, clipPath);
			AssetDatabase.SaveAssets ();
			AssetDatabase.ImportAsset (clipPath);

			AssetDatabase.SaveAssets ();
		}

		outClip = bvhLoader.clip;

		DestroyImmediate (bvhLoader);
		bvhLoader = null;

		return outClip;
	}

	/// <summary>
	/// Convenience class to pass Vector and Quaternion back from functions
	/// </summary>
	public class VecQuad {
		public Vector3 v;
		public Quaternion q;

		public VecQuad (Transform source) {
			if (source != null) {
				v = source.position;
				q = source.rotation;
			}
		}

		public VecQuad (Vector3 vec, Quaternion quad) {
			v = vec;
			q = quad;
		}
	}

	/// <summary>
	/// Finds the normalized position and rotation used in animation keyframes based using requried paramaters
	/// </summary>
	/// <param name="sourceAnimator">Animator to use to get avatar and scale</param>
	/// <param name="avatarBoneGoal">Bone you want to get Normalized values for (Goal)</param>
	/// <param name="animatorBodyVQ">Position and rotation of the Avatar Body</param>
	/// <param name="skeletonVQ">Original Position and rotation</param>
	/// <returns></returns>
	public static VecQuad GetGoalVQ (Animator sourceAnimator, HumanBodyBones avatarBoneGoal, VecQuad animatorBodyVQ, VecQuad skeletonVQ) {
		Avatar avatar = sourceAnimator.avatar;
		float humanScale = sourceAnimator.humanScale;
		return GetGoalVQ (avatar, humanScale, avatarBoneGoal, animatorBodyVQ, skeletonVQ);
	}

	/// <summary>
	/// Finds the normalized position and rotation used in animation keyframes based using requried paramaters
	/// </summary>
	/// <param name="avatar">Avatar to get data from</param>
	/// <param name="humanScale">scale of Avatar</param>
	/// <param name="avatarBoneGoal">Bone you want to get Normalized values for (Goal)</param>
	/// <param name="animatorBodyVQ">Position and rotation of the Avatar Body</param>
	/// <param name="skeletonVQ">Original Position and rotation</param>
	/// <returns></returns>
	public static VecQuad GetGoalVQ (Avatar avatar, float humanScale, HumanBodyBones avatarBoneGoal, VecQuad animatorBodyVQ, VecQuad skeletonVQ) {
		// Get bone as ID and check if valid
		int boneGoalId = (int) avatarBoneGoal;
		if (boneGoalId == (int) HumanBodyBones.LastBone) throw new InvalidOperationException ("Invalid bone id.");

		// Check if we can get access to Methods
		MethodInfo methodGetAxisLength = typeof (Avatar).GetMethod ("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (methodGetAxisLength == null) throw new InvalidOperationException ("GetAxisLength method not found.");

		MethodInfo methodGetPostRotation = typeof (Avatar).GetMethod ("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (methodGetPostRotation == null) throw new InvalidOperationException ("GetPostRotation method not found.");

		// Calculate Goal
		Quaternion postRotation = (Quaternion) methodGetPostRotation.Invoke (avatar, new object[] { boneGoalId });
		var goalVQ = new VecQuad (skeletonVQ.v, skeletonVQ.q * postRotation);
		if (avatarBoneGoal == HumanBodyBones.LeftFoot || avatarBoneGoal == HumanBodyBones.RightFoot) {
			float axislength = (float) methodGetAxisLength.Invoke (avatar, new object[] { boneGoalId });
			Vector3 footBottom = new Vector3 (axislength, 0, 0);
			goalVQ.v += (goalVQ.q * footBottom);
		}
		// IK goal are in avatar body local space
		Quaternion invRootQ = Quaternion.Inverse (animatorBodyVQ.q);
		goalVQ.v = invRootQ * (goalVQ.v - animatorBodyVQ.v);
		goalVQ.q = invRootQ * goalVQ.q;
		goalVQ.v /= humanScale;

		return goalVQ;
	}
}