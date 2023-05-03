/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.XR;

#if UNITY_EDITOR

using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;

#endif

public enum fsBoneAxis {
	x_axis,
	y_axis,
	z_axis
}

public enum colliderGenerationType {
	humanoid,
	boxes,
	capsules,
	spheres
}

public enum fColliderType {
	boxes,
	capsules,
	spheres
}

// Used to indicate the desired complexity of collider generation
public enum ColliderLevel {
	None = 0,                                       // No colliders, just remove component
	Minimal = 1,                                    // (tip of index fingers and thumbs, hands, chest, and lower arms)
	Medium = 2,                                     // Minimal plus (upper arms, head, and upper/lower legs)
	Full = 3                                        // Minimal plus Medium plus (all fingers, neck, and feet)
}

public class MecanimBone {
	public HumanBodyBones humanBodyBone;            // Mecanim bone type
	public Transform bone;                          // Mecanim bone transform
	public MecanimBone parentMecanimBone;           // Parent of this node in tree

	public Vector3 childMecanimPoint;               // next mecanim bone position
	public List<MecanimBone> childrenMecanim;       // Normaly 1 except fingers/toes (Is the next valid Mechanim bone in this tranforms children, can skip bones)
	public List<Transform> childrenNonMecanim;      //

	public Vector3 maxBounds = Vector3.zero;        // Bound box maximum and minimum points
	public Vector3 minBounds = Vector3.zero;

	public Vector3 vertexCenter = Vector3.zero;     // Calculated point that is center of the collider
}

// This structure holds all the data we need to store for each SkinnedMeshRenderer
public struct SkinnedRendererData {

	// All the bones for this SkinnedMeshRenderer
	public Transform[] bones;

	// Lookup table to map bone instance ID's to bone indexes for bone[] array above
	public Dictionary<int, int> boneID_toBoneIndex;

	// These map a bone index to a list of vertexs that belong to that bone
	// Each vertex can have up to 4 bones affecting it so we have 4 lists

	public Dictionary<int, List<Vector3>> vertexByBone_01;
	public Dictionary<int, List<Vector3>> vertexByBone_02;
	public Dictionary<int, List<Vector3>> vertexByBone_03;
	public Dictionary<int, List<Vector3>> vertexByBone_04;

	// Reference to source skinnedMeshRenderer
	public SkinnedMeshRenderer skinnedMeshRenderer;

	// For display in selector window
	public int vertexCount;

	// Set in selector window to indicate if this skinnedMeshRenderer should be included in collision detection
	public bool includeInDetection;
}

public class SkinnedRendererVertexData {

	// Set in selector window to indicate if this skinnedMeshRenderer should be included in collision detection
	public bool includeInDetection;

	// Reference to source skinnedMeshRenderer
	public SkinnedMeshRenderer skinnedMeshRenderer;

	// All the bones for this SkinnedMeshRenderer
	public Transform[] bones;

	// These map a bone index to a list of vertexs that belong to that bone
	public Dictionary<int, List<Vector3>> vertexByBone_01;

	// For display in selector window
	public int vertexCount;

	// Tells us if this data has been analyzed once before
	public bool hasInit = false;
}

#if UNITY_EDITOR

public class SelectorWindow : EditorWindow {
	public GenerateColliders gCollidersRef = null;                  // Pointer back to GenerateColliders script set when editor window is created so buttons can call appropriate functions

	private bool allBool = true;                                    // Represents weather the all toggle button is checked or unchecked
	private bool allBoolLast = true;                                // Last value of all toggle to detect changes

	private ColliderLevel colliderLevel;                            // Value the collider level dorp down is set to
	private SkinnedRendererVertexData[] srData;                     // Copy of srData from GenerateColliders script, used to remember which SkinnedRenderers are selected to include in collider generation
	private List<SkinnedRendererVertexData> srDataList;             // srData to pass back to gCollidersRef

	private void OnGUI () {
		GUILayout.Label ("Collider Level", EditorStyles.boldLabel);
		colliderLevel = (ColliderLevel) EditorGUILayout.EnumPopup ("", colliderLevel, GUILayout.Width (80));

		if (gCollidersRef != null) gCollidersRef.colliderLevel = colliderLevel;

		if (colliderLevel != ColliderLevel.None) {
			GUILayout.Label ("SkinnedMeshRenderer (to generate colliders from)", EditorStyles.boldLabel);
			allBool = EditorGUILayout.ToggleLeft ("  All", allBool);
			ToggleAllOptions ();

			if (srData != null) {
				srDataList = new List<SkinnedRendererVertexData> ();
				for (int i = 0; i < srData.Length; i++) {
					srData[i].includeInDetection = EditorGUILayout.ToggleLeft ("  " + srData[i].skinnedMeshRenderer.name, srData[i].includeInDetection);
					srDataList.Add (srData[i]);
				}
			}

			GUILayout.Space (20);
			if (GUILayout.Button ("Generate Colliders & Import") && gCollidersRef != null) {
				gCollidersRef.srvData.Clear ();
				gCollidersRef.srvData = srDataList;
				gCollidersRef.DoAnalyze ();
				Close ();
			}
		} else {
			GUILayout.Space (20);
			if (GUILayout.Button ("Import") && gCollidersRef != null) {
				gCollidersRef.DoAnalyze ();
				Close ();
			}
		}
	}

	public void SetGenerateColliderReference (GenerateColliders generateColliders) {
		gCollidersRef = generateColliders;
	}

	public void SetData (ref List<SkinnedRendererVertexData> newData) {
		srData = newData.ToArray ();
	}

	public void SetColliderLevel (ColliderLevel newColliderLevel) {
		colliderLevel = newColliderLevel;
	}

	private void ToggleAllOptions () {
		if (srData == null) return;

		// If the all toggle changes we change all SkinnedRenderer toggles to match
		if (allBool != allBoolLast) {
			for (int i = 0; i < srData.Length; i++) {
				srData[i].includeInDetection = allBool;
			}
			allBoolLast = allBool;
		}

		// If any SkinnedRenderer toggles are false then all should be false
		for (int i = 0; i < srData.Length; i++) {
			if (srData[i].includeInDetection == false) {
				allBool = false;
				allBoolLast = false;
				break;
			}
		}

		// If any SkinnedRenderer toggles are true then all should be true as well
		bool allActive = true;
		for (int i = 0; i < srData.Length; i++) {
			if (srData[i].includeInDetection == false) {
				allActive = false;
				break;
			}
		}

		if (allActive == true) {
			allBool = true;
			allBoolLast = true;
		}
	}
}

#else
// This is a placeholder to use during builds (unity editor classes can't exist during build so we can't derive from EditorWindow)
public class SelectorWindow{
	public GenerateColliders gCollidersRef = null;

	public void SetGenerateColliderReference (GenerateColliders generateColliders) {}
	public void SetData (ref List<SkinnedRendererVertexData> newData) {}
	public void SetColliderLevel (ColliderLevel newColliderLevel) {}
	private void ToggleAllOptions () {}
}
#endif

public class GenerateColliders : MonoBehaviour {
	public Animator animator;

	[Header ("Collider Generation")]
	[Tooltip ("Will set all body parts to this collider type before overrides and execptions are applied.")]
	public colliderGenerationType defaultBody = colliderGenerationType.humanoid;

	[Tooltip ("How much to overlap colliders between bones (Not implemented yet)")]
	public float colliderLengthOverlap = 0.1f;

	[Tooltip ("Will shrink or grow colliders generated by this multiplier.")]
	public float sizeMultiplier = 1f;

	[Tooltip ("The full body weight of this character. It will be divided into all the body parts proportionally.")]
	public float weight = 75f;

	[Tooltip ("This is a value between 0-1 that joint ranges are multiplied by to increase/decrease their range.")]
	public float jointRange = 1f;

	[Header ("Toggle this to see Vertex Points when you select colliders")]
	public bool DrawDebugPoints = true;

	[Header ("Toggle this to see bones")]
	public bool DrawDebugBones = true;

	[Header ("Bone Weight Layers")]
	[Tooltip ("Toggle which bone weight layers should be used.")]
	public bool Layer_01 = true;

	[Tooltip ("Toggle which bone weight layers should be used.")]
	public bool Layer_02 = false;

	[Tooltip ("Toggle which bone weight layers should be used.")]
	public bool Layer_03 = false;

	[Tooltip ("Toggle which bone weight layers should be used.")]
	public bool Layer_04 = false;

	[Header ("Collider Overrides")]
	[Tooltip ("Add Bone Transforms to this list that you want to force to be a Capsule Collider.")]
	public Transform[] capsuleTranforms;

	[Tooltip ("Add Bone Transforms to this list that you want to force to be a Sphere Collider.")]
	public Transform[] sphereTranforms;

	[Tooltip ("Add Bone Transforms to this list that you want to force to be a Sphere Collider.")]
	public Transform[] boxTranforms;

	[Tooltip ("Add Bone Transforms to this list to prevent Collider generation on them.")]
	public Transform[] excludeTranforms;

	[Header ("Reference")]
	[Tooltip ("Bones that have been generated by this script")]
	public List<GameObject> generatedColliders = new List<GameObject> ();

	[Tooltip ("Bones that existed prior to script running")]
	public List<Collider> existingColliders = new List<Collider> ();

	public ColliderLevel colliderLevel = ColliderLevel.Medium;
	public List<SkinnedRendererVertexData> srvData = new List<SkinnedRendererVertexData> ();

	#region FastLookups

	// ID lookups for easy checking
	private Dictionary<int, bool> capsuleLookup = new Dictionary<int, bool> ();

	private Dictionary<int, bool> sphereLookup = new Dictionary<int, bool> ();
	private Dictionary<int, bool> boxLookup = new Dictionary<int, bool> ();
	private Dictionary<int, bool> excludeLookup = new Dictionary<int, bool> ();

	private Dictionary<int, bool> feetLookup = new Dictionary<int, bool> ();
	private Dictionary<int, bool> legLookup = new Dictionary<int, bool> ();
	private Dictionary<int, fColliderType> humanoidLookup = new Dictionary<int, fColliderType> ();

	public Dictionary<int, Transform> keyBoneLookup = new Dictionary<int, Transform> ();

	public Transform av_Root;

	public Transform av_Hips;
	public Transform av_LeftUpperLeg;
	public Transform av_RightUpperLeg;
	public Transform av_LeftLowerLeg;
	public Transform av_RightLowerLeg;
	public Transform av_LeftFoot;
	public Transform av_RightFoot;
	public Transform av_Spine;
	public Transform av_Chest;
	public Transform av_Neck;
	public Transform av_Head;
	public Transform av_LeftShoulder;
	public Transform av_RightShoulder;
	public Transform av_LeftUpperArm;
	public Transform av_RightUpperArm;
	public Transform av_LeftLowerArm;
	public Transform av_RightLowerArm;
	public Transform av_LeftHand;
	public Transform av_RightHand;
	public Transform av_LeftToes;
	public Transform av_RightToes;
	public Transform av_LeftEye;
	public Transform av_RightEye;
	public Transform av_Jaw;
	public Transform av_LeftThumbProximal;
	public Transform av_LeftThumbIntermediate;
	public Transform av_LeftThumbDistal;
	public Transform av_LeftIndexProximal;
	public Transform av_LeftIndexIntermediate;
	public Transform av_LeftIndexDistal;
	public Transform av_LeftMiddleProximal;
	public Transform av_LeftMiddleIntermediate;
	public Transform av_LeftMiddleDistal;
	public Transform av_LeftRingProximal;
	public Transform av_LeftRingIntermediate;
	public Transform av_LeftRingDistal;
	public Transform av_LeftLittleProximal;
	public Transform av_LeftLittleIntermediate;
	public Transform av_LeftLittleDistal;
	public Transform av_RightThumbProximal;
	public Transform av_RightThumbIntermediate;
	public Transform av_RightThumbDistal;
	public Transform av_RightIndexProximal;
	public Transform av_RightIndexIntermediate;
	public Transform av_RightIndexDistal;
	public Transform av_RightMiddleProximal;
	public Transform av_RightMiddleIntermediate;
	public Transform av_RightMiddleDistal;
	public Transform av_RightRingProximal;
	public Transform av_RightRingIntermediate;
	public Transform av_RightRingDistal;
	public Transform av_RightLittleProximal;
	public Transform av_RightLittleIntermediate;
	public Transform av_RightLittleDistal;
	public Transform av_UpperChest;

	#endregion FastLookups

	private float maxLegRadius = 0f;
	private float statusBarProgress = 0f;
	private bool hasInit = false;

	private SkinnedRendererVertexData combinedVertexData;

#if UNITY_EDITOR

	private SelectorWindow selectorWindow;

#endif

	private MecanimBone topBone = null;

	private int baseIncrement = 0;
	private float lastIncrementTime = 0;

	private Dictionary<int, MecanimBone> mecanimBoneLookup = new Dictionary<int, MecanimBone> ();
	private Dictionary<int, HumanBodyBones> mecanimLookup = new Dictionary<int, HumanBodyBones> ();

	/// <summary>
	/// Cleanup old Colliders. Can be done manually using buttons on script, will be done before generation
	/// </summary>
	public void DoClearGeneratedColliders () {
		for (int i = 0; i < generatedColliders.Count; i++) {
			GameObject.DestroyImmediate (generatedColliders[i]);
		}

		generatedColliders.Clear ();

#if UNITY_EDITOR
		EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
#endif
	}

	/// <summary>
	/// Will use a T-Pose animation to force avatar into T-Pose.  Can be done manually using button on script, will be done before generation
	/// </summary>
	public void DoForceTpose () {
#if UNITY_EDITOR

		animator = GetComponent<Animator> ();

		if (animator == null)
			return;

		if (!animator.isHuman) {
			Debug.LogError ("Animator Needs to be Human.");
			return;
		}

		string tPosePath = "TPoseController";
		UnityEditor.Animations.AnimatorController controller = Resources.Load<UnityEditor.Animations.AnimatorController> (tPosePath);

		if (controller != null)
			animator.runtimeAnimatorController = controller;

		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		animator.Update (1f);

		EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
#endif
	}

	/// <summary>
	/// For debugging, will output the bind pose (scale and rotations)
	/// </summary>
	public void DoPrintBindPose () {
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		// Get all the skinned mesh renderers from the animator component of this game object
		SkinnedMeshRenderer[] AllSkinnedMeshRenderers;
		AllSkinnedMeshRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer> ();

		if (AllSkinnedMeshRenderers.Length > 0) {
			foreach (SkinnedMeshRenderer currSMR in AllSkinnedMeshRenderers) {
				// Ouput the bind pose data for each bone
				Debug.Log ("=========================" + currSMR.name + "=================================================================================");

				int i = 0;
				foreach (Matrix4x4 currBone in currSMR.sharedMesh.bindposes) {
					Debug.Log ("N: " + currSMR.bones[i].name);
					Debug.Log ("R: " + currBone.rotation.eulerAngles);
					Debug.Log ("S: " + currBone.lossyScale);
					i++;
				}
			}
		} else {
			Debug.LogError ("Error: No SkinnedMeshRenderer to print bind pose from");
			return;
		}
	}

	/// <summary>
	/// Opens the selector window that allows user to select collider generation level and SkinnedMeshRenderers to include in generation
	/// </summary>
	public void DoOpenSelector () {
		animator = GetComponent<Animator> ();
		if (animator == null) return;

		// Ensure model is in T-Pose
		DoForceTpose ();

#if UNITY_EDITOR

		// Create the window if it dosn't exist already
		if (selectorWindow == null) {
			selectorWindow = ScriptableObject.CreateInstance<SelectorWindow> ();
			selectorWindow.titleContent.text = "Import Options:";
			selectorWindow.name = "Import Options:";
		}

#endif

		// Create array of data srvData[] that will hold data for each SkinnedMeshRenderer
		// We only add a SkinnedMeshRenderer if it isn't already in srvData[], this ensures system dosn't re analyze same data repeatedly
		SkinnedMeshRenderer[] AllSkinnedMeshRenderers;
		AllSkinnedMeshRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer> ();

		if (AllSkinnedMeshRenderers.Length > 0) {
			foreach (SkinnedMeshRenderer currSMR in AllSkinnedMeshRenderers) {
				bool foundExisting = false;

				foreach (SkinnedRendererVertexData existingSRVD in srvData) {
					if (existingSRVD.skinnedMeshRenderer.GetInstanceID () == currSMR.GetInstanceID ()) {
						foundExisting = true;
						break;
					}
				}

				if (foundExisting == false) {
					SkinnedRendererVertexData newData = new SkinnedRendererVertexData ();
					newData.skinnedMeshRenderer = currSMR;
					newData.bones = currSMR.bones;
					newData.includeInDetection = true;
					newData.vertexByBone_01 = new Dictionary<int, List<Vector3>> ();
					srvData.Add (newData);
				}
			}
		} else {
			Debug.LogError ("Error: There are no SkinnedMeshRenderer components in the hierarchy.");
			return;
		}

#if UNITY_EDITOR

		// Populate window with data and display
		selectorWindow.SetGenerateColliderReference (this);
		selectorWindow.SetColliderLevel (colliderLevel);
		selectorWindow.SetData (ref srvData);
		selectorWindow.ShowUtility ();

		// Set initial dimentions to fit buttons and text without any rows
		float windowWidth = 400f;
		float windowHeight = 140f;

		// increase height to fit each skinned mesh renderer (toggle/name) combo
		windowHeight += (18 * srvData.Count);

		// Center window on screen
		selectorWindow.position = new Rect ((Screen.currentResolution.width / 2) - (windowWidth / 2), (Screen.currentResolution.height / 2) - (windowHeight / 2), windowWidth, windowHeight);

#endif
	}

	/// <summary>
	/// Clears out saved vertex data (srvData). Typically the system will save vertex data from each SkinnedMeshRenderer selected and analyzed, but you might want to clear.  Done manually using button on script
	/// </summary>
	public void ClearAnalyze () {
		animator = GetComponent<Animator> ();

		if (animator == null) return;

		DoForceTpose ();
		srvData.Clear ();
	}

	/// <summary>
	/// Analyze the vertex data for all SkinnedMeshRenderers selected in selector window.  Called when Import button pressed in Selector Window UI
	/// <br>Will clear lookups and generate colliders based off collider level selected after analysis is completed</br>
	/// </summary>
	public void DoAnalyze () {
		// First, if the selected collider level is none we just remove this component and quit
		if (colliderLevel == ColliderLevel.None) {
			DoClearGeneratedColliders ();
			DestroyImmediate (this);
			return;
		}

		// Clear lookup tables
		capsuleLookup.Clear ();
		sphereLookup.Clear ();
		excludeLookup.Clear ();

		// Cleanup Data
		feetLookup.Clear ();
		legLookup.Clear ();

		// Clear lookup Tables for overrides
		capsuleLookup.Clear ();
		sphereLookup.Clear ();
		boxLookup.Clear ();
		excludeLookup.Clear ();

		// Cleanup MecanimBone tree
		foreach (KeyValuePair<int, MecanimBone> currMecBone in mecanimBoneLookup) {
			currMecBone.Value.bone = null;
			currMecBone.Value.parentMecanimBone = null;
			currMecBone.Value.childrenMecanim.Clear ();
			currMecBone.Value.childrenNonMecanim.Clear ();
		}

		// Analyze data
		hasInit = false;

		BuildBoneVertexLookup ();
		BuildMecanimBones ();
		GenerateCollidersForBones ();
	}

	/// <summary>
	/// Build a vertex lookup for each bone by pulling data from each SkinnedMeshRenderer selected in Selector Window UI
	/// </summary>
	public void BuildBoneVertexLookup () {
		bool keepWeightZero = false;

		// Record the transforms for specific parts for easy lookup later
		SetQuickHumanoidLookups ();

		// Create lookup tables for grouping based off desired collider level, do every time in case the collision level changes between generates
		CreateMappingLookup ();

		// Make sure everyting has been setup correctly for this function to work
		if (srvData.Count != 0 && hasInit) return;

		animator = GetComponent<Animator> ();

		if (animator == null)
			return;

		if (!animator.isHuman) {
			Debug.LogError ("Animator Needs to be Human.");
			return;
		}

		// Clear old colliders we generated on last pass before generating new (manually created colliders won't be effected)
		DoClearGeneratedColliders ();

		// Now that old data is clear and we have quick lookups find the existing colliders
		FindExistingColliders ();

		// Make lookup tables for the overrides
		if (capsuleTranforms != null) {
			foreach (Transform currCapsule in capsuleTranforms) {
				if (capsuleLookup.ContainsKey (currCapsule.GetInstanceID ()) == false)
					capsuleLookup.Add (currCapsule.GetInstanceID (), true);
			}
		}

		if (sphereTranforms != null) {
			foreach (Transform currSphere in sphereTranforms) {
				if (sphereLookup.ContainsKey (currSphere.GetInstanceID ()) == false)
					sphereLookup.Add (currSphere.GetInstanceID (), true);
			}
		}

		if (boxTranforms != null) {
			foreach (Transform currBox in boxTranforms) {
				if (boxLookup.ContainsKey (currBox.GetInstanceID ()) == false)
					boxLookup.Add (currBox.GetInstanceID (), true);
			}
		}

		if (excludeTranforms != null) {
			foreach (Transform currExclude in excludeTranforms) {
				if (excludeLookup.ContainsKey (currExclude.GetInstanceID ()) == false)
					excludeLookup.Add (currExclude.GetInstanceID (), true);
			}
		}

		// Create array of data srData[] that will hold data for each SkinnedMeshRenderer
		// We convert to array so we can make changes
		SkinnedRendererVertexData[] dataArray;
		int dataArraySelected = srvData.Count;

		if (srvData.Count > 0) {
			dataArray = srvData.ToArray ();

			dataArraySelected = 0;
			foreach (SkinnedRendererVertexData srvd in srvData) {
				if (srvd.includeInDetection)
					dataArraySelected++;
			}
		} else {
			Debug.LogError ("Error: No Skinned Mesh Renderers were selected.");
			return;
		}

		srvData.Clear ();

		// The progress bar for vertex being analyzed is 70% of total, we divide it by number of meshes so each mesh gets a slice of that time (boneProgressTotal)
		// Seteup progres bar depending on how many Skinned Mesh Renderers were selected
		float setupProgressTotal = 1f;
		float boneProgressTotal = setupProgressTotal / dataArraySelected;
		int smrIndex = 0;

		// Go through each SkinnedMeshRenderer and collect vertex data for each bone
		// Each vertex can have up to 4 bones affecting it, each array vertexByBone_01 to vertexByBone_04 is a dictionary that maps the bone index to list of vertexes
		for (int i = 0; i < dataArray.Length; i++) {
			// We want to re add the ones we arn't processing so thier disabled state is remembered
			if (dataArray[i].includeInDetection == false) {
				srvData.Add (dataArray[i]);
				continue;
			}
			// Don't process more then once, users can hit the clear data button to force a re check of all verts
			if (dataArray[i].hasInit == true) {
				srvData.Add (dataArray[i]);
				continue;
			}

			SkinnedMeshRenderer currSkinnedMeshRenderer = dataArray[i].skinnedMeshRenderer;

			Mesh orgMesh = currSkinnedMeshRenderer.sharedMesh;
			BoneWeight[] boneWeights = orgMesh.boneWeights;

			Mesh targetMesh = new Mesh ();

			var storePos = currSkinnedMeshRenderer.transform.localPosition;
			var storeScale = currSkinnedMeshRenderer.transform.localScale;
			var storeRot = currSkinnedMeshRenderer.transform.localRotation;

			currSkinnedMeshRenderer.transform.localPosition = Vector3.zero;
			currSkinnedMeshRenderer.transform.localScale = Vector3.one;
			currSkinnedMeshRenderer.transform.localRotation = Quaternion.identity;
			currSkinnedMeshRenderer.BakeMesh (targetMesh);

			int currIndex = 0;
			foreach (BoneWeight currVertexForBone in boneWeights) {
				// statusBarProgress is the the progress bars current position for completed work, we add the current progress to it
				// (currIndex + 1 / boneWeights.Length gives) us the normalized progress of vertex process for current mesh
				// We multiply progress by boneProgressTotal then add offset
				float currProgress = statusBarProgress + (boneProgressTotal * (((float) currIndex + 1f) / (float) boneWeights.Length)) + (smrIndex * boneProgressTotal);
				int currProgressWhole = (int) currProgress;
#if UNITY_EDITOR
				// Update the progress bar when in unity editory, we only update every 5th vertex to avoid unnecessary UI update overhead
				if (currProgressWhole % 5 == 0)
					EditorUtility.DisplayProgressBar ("Generating Colliders: Analyzing", "" + currSkinnedMeshRenderer.name + " [ Vertex: " + (currIndex + 1) + " / " + boneWeights.Length + " ]", currProgress);
#endif
				// Get verts for first bone index
				int boneID_01 = dataArray[i].bones[currVertexForBone.boneIndex0].GetInstanceID ();

				if (MappingLookup.ContainsKey (boneID_01) == true && MappingLookup[boneID_01] != 0) {
					// This allows us to group bones.  MappingLookup is setup so they key is each unique bone and the value is the bone id it should mapped to
					// Eg) All bones in head would be keys, the values would be the id of head bone so all vertices are grouped as Head instead of individual Eye, Jaw, ect...
					boneID_01 = MappingLookup[boneID_01];

					// Only vertices that are effected by this bone are added, a zero weight means the bone is not effecting this vertex
					if (Layer_01 && (keepWeightZero == true || (keepWeightZero == false && currVertexForBone.weight0 != 0))) {
						// If this bone hasn't been added then add it to vertexByBone_01 with bone ID as key, otherwise add to value which is a list of vertices belonging to the bone
						if (dataArray[i].vertexByBone_01.ContainsKey (boneID_01) == true) {
							// Add Vertices to the bone
							dataArray[i].vertexByBone_01[boneID_01].Add (currSkinnedMeshRenderer.transform.TransformPoint (targetMesh.vertices[currIndex]));
						} else {
							// Create Key for this Bone
							dataArray[i].vertexByBone_01.Add (boneID_01, new List<Vector3> ());
						}
					}
				}

				currIndex++;
			}

			dataArray[i].hasInit = true;
			srvData.Add (dataArray[i]);

			currSkinnedMeshRenderer.transform.localPosition = storePos;
			currSkinnedMeshRenderer.transform.localScale = storeScale;
			currSkinnedMeshRenderer.transform.localRotation = storeRot;

			smrIndex++;
		}

		// Make combined SkinnedRendererData, the code above makes a set of SkinnedRendererData (srvData), one for each SkinnedRenderer (Hair, Body, Cloths..)
		// If we generate for each we can have multiple colliders for each bone, one for each SkinnedRenderer but typically we want to combind all the vertices and get one collider

		combinedVertexData = new SkinnedRendererVertexData ();
		combinedVertexData.vertexByBone_01 = new Dictionary<int, List<Vector3>> ();

		for (int i = 0; i < srvData.Count; i++) {
			// Go through all the bone adding its vertex data to the combinded data for Layer 1
			foreach (KeyValuePair<int, List<Vector3>> bone in srvData[i].vertexByBone_01) {
				if (combinedVertexData.vertexByBone_01.ContainsKey (bone.Key) == false) {
					combinedVertexData.vertexByBone_01.Add (bone.Key, new List<Vector3> ());
				}
				// bone.value is the list of vertex belonging to the bone for the current skinned renderer srData[i]
				foreach (Vector3 vertex in bone.Value) {
					combinedVertexData.vertexByBone_01[bone.Key].Add (vertex);
				}
			}
		}

		// Make a lookup for all feet transforms (Used to make sure box colliders on feet are don't get aligned to bone, stay axis aligned)
		SetupFeetLookup ();

		// Make a lookup for both legs without  feet (Used for Daz characters, keep rotation of legs as identity)
		SetupLegLookup ();

		// This creates a lookup table that maps bone transform ID's to collider types.  The collider types are the defaults we want for humanoids
		SetupHumanoidLookup ();

		// Preven leg chafe on colliders of Upper Legs
		maxLegRadius = (av_Hips.position.x - av_RightUpperLeg.position.x) * 0.85f;  // Calculate max and scale by 85% to give a bit more room
		maxLegRadius = Mathf.Abs (maxLegRadius);

		// Indicate completion and close progress bar
		hasInit = true;

#if UNITY_EDITOR

		EditorUtility.ClearProgressBar ();

#endif
	}

	/// <summary>
	/// A skeleton can have multiple bones between the Mecanim key bones so this function creates a tree of (MecanimBone) nodes
	/// <br>The tree matches structure of mecanim body with hip at root but has childrenMecanim and childrenNonMecanim for traversal</br>
	/// <br>This strucure is used to determin what direction the next bone is really pointing and simplifies generation of colliders</br>
	/// </summary>
	public void BuildMecanimBones () {
		// Ensure we have animator
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		// If we found animator then build the MecanimBone tree
		if (animator == null) return;

		mecanimBoneLookup.Clear ();

		// Create top node of tree and add it to full list of nodes (mecanimBoneLookup)
		topBone = new MecanimBone ();
		topBone.childrenMecanim = new List<MecanimBone> ();
		topBone.childrenNonMecanim = new List<Transform> ();
		mecanimBoneLookup.Add (animator.GetBoneTransform (HumanBodyBones.Hips).GetInstanceID (), topBone);

		// Create a lookup table
		BuildMecanimLookups ();

		// Recuse from top building out tree
		BuildMecanimBonesRecurse (ref topBone, animator.GetBoneTransform (HumanBodyBones.Hips));

		// Do leaf adjustments
		BuildLeafBonesAndBounds ();
	}

	/// <summary>
	/// During BuildMecanimBonesRecurse() childMecanimPoint is set for all bones that have children but leaf bones don't have children so we calculate them last
	/// <br>childMecanimPoint points in the dirction of next bone so we use it to rotate from that direction to x-axis then calculate a bounding box for veretices</br>
	/// </summary>
	public void BuildLeafBonesAndBounds () {
		// We have a tree of MecanimBone but here we want to process them all and order dosn't matter so we can just itterate through the full list (mecanimBoneLookup)
		foreach (KeyValuePair<int, MecanimBone> currMecBone in mecanimBoneLookup) {
			// Calculate leaf bone childMecanimPoint, done differently depending on bone
			HumanBodyBones currBoneType = currMecBone.Value.humanBodyBone;

			// For finger bones we use the same childMecanimPoint (direction) as its parent since the last finger is typically at same angle as all other fingers
			if (currBoneType == HumanBodyBones.LeftIndexDistal || currBoneType == HumanBodyBones.LeftLittleDistal || currBoneType == HumanBodyBones.LeftMiddleDistal || currBoneType == HumanBodyBones.LeftRingDistal || currBoneType == HumanBodyBones.LeftThumbDistal) {
				currMecBone.Value.childMecanimPoint = currMecBone.Value.parentMecanimBone.childMecanimPoint;
			}

			// For finger bones we use the same childMecanimPoint (direction) as its parent since the last finger is typically at same angle as all other fingers
			if (currBoneType == HumanBodyBones.RightIndexDistal || currBoneType == HumanBodyBones.RightLittleDistal || currBoneType == HumanBodyBones.RightMiddleDistal || currBoneType == HumanBodyBones.RightRingDistal || currBoneType == HumanBodyBones.RightThumbDistal) {
				currMecBone.Value.childMecanimPoint = currMecBone.Value.parentMecanimBone.childMecanimPoint;
			}

			// The head leaf should always point up since head should be aligned to y-axis
			if (currBoneType == HumanBodyBones.Head) {
				currMecBone.Value.childMecanimPoint = Vector3.up;
			}

			// In T-pose the feet should be facing forward so we make sure the feet are pointing pointing that direction
			if (currBoneType == HumanBodyBones.LeftFoot || currBoneType == HumanBodyBones.RightFoot) {
				if (currMecBone.Value.childrenMecanim.Count == 0) {
					currMecBone.Value.childMecanimPoint = Vector3.forward;      // This is the last bone, no toes so default it to facing forward
				} else {
					currMecBone.Value.childMecanimPoint.y = 0f;                 // Not all feet bones will be parallel to ground so we zero out the y to avoid rotating collider in way that isn't aligned with ground
				}
			}

			// Like fingers, toes should orient the same way as their parent
			if (currBoneType == HumanBodyBones.LeftToes || currBoneType == HumanBodyBones.RightToes) {
				currMecBone.Value.childMecanimPoint = currMecBone.Value.parentMecanimBone.childMecanimPoint;
			}

			// Orient vertices and figure out bounds
			Quaternion boneFixRot = GetBoneFix (currMecBone.Value.bone, currMecBone.Value.childMecanimPoint, currMecBone.Value.humanBodyBone);

			currMecBone.Value.maxBounds = new Vector3 (float.MinValue, float.MinValue, float.MinValue);
			currMecBone.Value.minBounds = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);

			// Make sure bone should be included
			if (combinedVertexData.vertexByBone_01.ContainsKey (currMecBone.Value.bone.transform.GetInstanceID ()) == false) continue;

			// This is to skip check for vertices being behind joint, we always want the vertices behind joint for feet
			bool allowVertexBehindBoneJoint = feetLookup.ContainsKey (currMecBone.Value.bone.transform.GetInstanceID ());

			// Go through each vertex and rotate it to the x-axis, we remove the bones transform position so offsets are from origin
			for (int i = 0; i < combinedVertexData.vertexByBone_01[currMecBone.Value.bone.transform.GetInstanceID ()].Count; i++) {
				Vector3 outPoint;
				outPoint = combinedVertexData.vertexByBone_01[currMecBone.Value.bone.transform.GetInstanceID ()][i];

				outPoint -= currMecBone.Value.bone.transform.position;
				outPoint = boneFixRot * outPoint;

				// Ignore points beyond bounds of bone joints
				if (allowVertexBehindBoneJoint == false && outPoint.x < 0) continue;                                                   // behind current bone joint
				if (allowVertexBehindBoneJoint == false && outPoint.x > currMecBone.Value.childMecanimPoint.magnitude) continue;       // past next bone joint

				// Update max bounding box point
				if (outPoint.x > currMecBone.Value.maxBounds.x) currMecBone.Value.maxBounds.x = outPoint.x;
				if (outPoint.y > currMecBone.Value.maxBounds.y) currMecBone.Value.maxBounds.y = outPoint.y;
				if (outPoint.z > currMecBone.Value.maxBounds.z) currMecBone.Value.maxBounds.z = outPoint.z;

				// Update min bounding box point
				if (outPoint.x < currMecBone.Value.minBounds.x) currMecBone.Value.minBounds.x = outPoint.x;
				if (outPoint.y < currMecBone.Value.minBounds.y) currMecBone.Value.minBounds.y = outPoint.y;
				if (outPoint.z < currMecBone.Value.minBounds.z) currMecBone.Value.minBounds.z = outPoint.z;
			}

			// Check if min/max bounds have been set, if any havn't been then just use half distance between bones as center and have collider height the size of bone lenght and radius 80% of length
			if (currMecBone.Value.maxBounds.x == float.MinValue ||
				currMecBone.Value.maxBounds.y == float.MinValue ||
				currMecBone.Value.maxBounds.z == float.MinValue ||
				currMecBone.Value.minBounds.x == float.MaxValue ||
				currMecBone.Value.minBounds.y == float.MaxValue ||
				currMecBone.Value.minBounds.z == float.MaxValue) {
				//Debug.Log (currMecBone.Value.bone.name + " : (" + currMecBone.Value.childMecanimPoint.x + "," + currMecBone.Value.childMecanimPoint.y + "," + currMecBone.Value.childMecanimPoint.z + ")");
				//Debug.Log (currMecBone.Value.bone.name + " : (" + currMecBone.Value.childMecanimPoint.magnitude);

				currMecBone.Value.minBounds = new Vector3 (0f, -currMecBone.Value.childMecanimPoint.magnitude * 0.45f, -currMecBone.Value.childMecanimPoint.magnitude * 0.45f);
				currMecBone.Value.maxBounds = new Vector3 (currMecBone.Value.childMecanimPoint.magnitude, currMecBone.Value.childMecanimPoint.magnitude * 0.45f, currMecBone.Value.childMecanimPoint.magnitude * 0.45f);
			}

			// Do some final cleanup of childMecanimPoint to make sure the lenght matches content
			// (childMecanimPoint) are set to unit values above for these parts so multiplication should keep point in bounds
			// Only really effects drawing of skeleton
			if (currBoneType == HumanBodyBones.Head) {
				currMecBone.Value.childMecanimPoint = currMecBone.Value.childMecanimPoint * currMecBone.Value.maxBounds.x;
			}

			if (currBoneType == HumanBodyBones.LeftFoot || currBoneType == HumanBodyBones.RightFoot) {
				if (currMecBone.Value.childrenMecanim.Count == 0) {
					currMecBone.Value.childMecanimPoint = currMecBone.Value.childMecanimPoint * currMecBone.Value.maxBounds.x;
				}
			}

			if (currBoneType == HumanBodyBones.LeftToes || currBoneType == HumanBodyBones.RightToes) {
				currMecBone.Value.childMecanimPoint = currMecBone.Value.childMecanimPoint * currMecBone.Value.maxBounds.x;
			}

			// Calculate the center of bounds
			currMecBone.Value.vertexCenter = currMecBone.Value.minBounds + ((currMecBone.Value.maxBounds - currMecBone.Value.minBounds) / 2f);

			if (currMecBone.Value.vertexCenter.x > (currMecBone.Value.childMecanimPoint.magnitude / 2f)) {
				currMecBone.Value.vertexCenter.x = (currMecBone.Value.childMecanimPoint.magnitude / 2f);
			}

			// All data is aligned to x-axis so rotate the center and bounds back to match the bone orietnation (How vertex data is stored)
			currMecBone.Value.vertexCenter = Quaternion.Inverse (boneFixRot) * currMecBone.Value.vertexCenter;
			currMecBone.Value.maxBounds = Quaternion.Inverse (boneFixRot) * currMecBone.Value.maxBounds;
			currMecBone.Value.minBounds = Quaternion.Inverse (boneFixRot) * currMecBone.Value.minBounds;

			currMecBone.Value.vertexCenter += currMecBone.Value.bone.transform.position;
			currMecBone.Value.maxBounds += currMecBone.Value.bone.transform.position;
			currMecBone.Value.minBounds += currMecBone.Value.bone.transform.position;
		}
	}

	/// <summary>
	/// Build (mecanimLookup) which is a dictonary of bone transform id keys, and values of HumanBodyBones type so we know what type of mechinim bone it is
	/// </summary>
	public void BuildMecanimLookups () {
		// Ensure we have animator
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		if (animator == null) return;

		mecanimLookup.Clear ();

		// Go through all bones on animator adding every mechanim bone to mecanimLookup where int is transform instance ID and HumanBodyBones is the bone type
		for (int i = 0; i < (int) HumanBodyBones.LastBone; i++) {
			if (animator.GetBoneTransform ((HumanBodyBones) i) != null) {
				mecanimLookup.Add (animator.GetBoneTransform ((HumanBodyBones) i).GetInstanceID (), (HumanBodyBones) i);
			}
		}
	}

	/// <summary>
	/// Builds the mecanimBoneLookup tree of MecanimBone nodes that group children into childrenMecanim and childrenNonMecanim.
	/// <br>Also records parent mecanim bone and caclulates initial childMecanimPoint (the direction and positon of next mecanim bone)</br>
	/// <br>This structure matches skeleton of model but jumps between mechanim bones for easier collider generation</br>
	/// </summary>
	public void BuildMecanimBonesRecurse (ref MecanimBone currMecBone, Transform currBone) {
		bool currIsMechanimBone = mecanimLookup.ContainsKey (currBone.GetInstanceID ());

		HumanBodyBones currBoneType = HumanBodyBones.LastBone;
		if (currIsMechanimBone) currBoneType = mecanimLookup[currBone.GetInstanceID ()];

		// mecanimLookup is a list of all valid mecanim bones on the avatar, if currBone is in that list its a Mecanim Bone
		if (currIsMechanimBone == true) {
			currMecBone.bone = currBone;
			currMecBone.humanBodyBone = mecanimLookup[currBone.GetInstanceID ()];
		}

		// process currBone's children
		for (int i = 0; i < currBone.childCount; i++) {
			bool childIsMechanimBone = mecanimLookup.ContainsKey (currBone.GetChild (i).GetInstanceID ());

			if (childIsMechanimBone == true) {
				// If the child is a mecanim bone then we create a new MecanimBone object since this is a tree of Mecanim Bones
				MecanimBone newChildBone = new MecanimBone ();
				newChildBone.childrenMecanim = new List<MecanimBone> ();
				newChildBone.childrenNonMecanim = new List<Transform> ();
				newChildBone.parentMecanimBone = currMecBone;
				mecanimBoneLookup.Add (currBone.GetChild (i).GetInstanceID (), newChildBone);

				// Continue processing this child recursivly, since we just created a new MecanimBone we pass that into recursive
				BuildMecanimBonesRecurse (ref newChildBone, currBone.GetChild (i));
				currMecBone.childrenMecanim.Add (newChildBone);
			} else {
				// This is not a mechanim bone so we add it to child list of MecanimBone passed in
				// MecanimBone passed in is the last valid mecanim bone up the tree of bones
				currMecBone.childrenNonMecanim.Add (currBone.GetChild (i));
				// continue processing the child recursivly but pass in the same MecanimBone passed into function since this isn't a mecanim bone
				BuildMecanimBonesRecurse (ref currMecBone, currBone.GetChild (i));
			}
		}

		// Find childMecanimPoint for special cases

		if (currBoneType == HumanBodyBones.Head) {
			// We need to find the next mecanim bone from chest to neck or head so we know what its direction is (childMecanimPoint)
			// Since the chest can be Spine, Chest, or UpperChest, and next mechanim bone could be neck or head we have to traverse from head
			// Since the head parent will always point towards chest directly we can traverse parents to find the first chest transform

			Transform spineNextTransform = currBone.parent;
			Transform spineNextTransformFound = currBone.parent;

			HumanBodyBones spineNextBoneType = HumanBodyBones.LastBone;
			bool spineNextIsMechanimBone = mecanimLookup.ContainsKey (spineNextTransform.GetInstanceID ());
			if (spineNextIsMechanimBone) spineNextBoneType = mecanimLookup[spineNextTransform.GetInstanceID ()];

			// Not all mecanim skeletons have neck, spineNextTransform starts as head and switches to neck if it exists skipping extra non mechanim bones
			while (!(spineNextBoneType == HumanBodyBones.UpperChest || spineNextBoneType == HumanBodyBones.Chest || spineNextBoneType == HumanBodyBones.Spine)) {
				if (mecanimLookup.ContainsKey (spineNextTransform.GetInstanceID ()) == true && mecanimLookup[spineNextTransform.GetInstanceID ()] == HumanBodyBones.Neck) {
					spineNextTransformFound = spineNextTransform;
					break;
				}

				spineNextTransform = spineNextTransform.parent;
				spineNextIsMechanimBone = mecanimLookup.ContainsKey (spineNextTransform.GetInstanceID ());
				if (spineNextIsMechanimBone) spineNextBoneType = mecanimLookup[spineNextTransform.GetInstanceID ()];
			}

			// Now we go through bone parents until we find the upper chest transform
			MecanimBone mecanimBone = currMecBone.parentMecanimBone;
			bool parentIsMechanimBone = mecanimLookup.ContainsKey (mecanimBone.bone.GetInstanceID ());

			HumanBodyBones parentBoneType = HumanBodyBones.LastBone;
			if (parentIsMechanimBone) parentBoneType = mecanimLookup[mecanimBone.bone.GetInstanceID ()];

			while (!(parentBoneType == HumanBodyBones.UpperChest || parentBoneType == HumanBodyBones.Chest || parentBoneType == HumanBodyBones.Spine)) {
				mecanimBone = mecanimBone.parentMecanimBone;
				parentIsMechanimBone = mecanimLookup.ContainsKey (mecanimBone.bone.GetInstanceID ());
				if (parentIsMechanimBone) parentBoneType = mecanimLookup[mecanimBone.bone.GetInstanceID ()];
			}

			// If we found a chest transform parentBoneType will be set to something other than LastBone
			if (parentBoneType != HumanBodyBones.LastBone) {
				mecanimBone.childMecanimPoint = spineNextTransformFound.position - mecanimBone.bone.position;
			}
		}

		if (currBoneType == HumanBodyBones.Hips) {
			// We check to top 3 spine bones to see which is the closest to neck so we can make hip bone extend to the last bone
			Transform highestSpineBone = av_Spine;

			if (highestSpineBone == null) {
				highestSpineBone = av_Chest;
			}
			if (highestSpineBone == null) {
				highestSpineBone = av_UpperChest;
			}

			if (mecanimBoneLookup.ContainsKey (highestSpineBone.GetInstanceID ()) == true) {
				currMecBone.childMecanimPoint = mecanimBoneLookup[highestSpineBone.GetInstanceID ()].bone.transform.position;
			}

			return;
		}

		if (currBoneType == HumanBodyBones.LeftHand) {
			// For hands we go through children adding the points for just fingers, not thumb.  We then divide by number of fingers to get average direction towards fingers
			Vector3 childPositionSum = Vector3.zero;
			int fingerCount = 0;
			for (int i = 0; i < currMecBone.childrenMecanim.Count; i++) {
				if (mecanimLookup.ContainsKey (currMecBone.childrenMecanim[i].bone.GetInstanceID ())) {
					if (mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.LeftIndexProximal ||
						mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.LeftLittleProximal ||
						mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.LeftMiddleProximal ||
						mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.LeftRingProximal) {
						childPositionSum += currMecBone.childrenMecanim[i].bone.position;
						fingerCount++;
					}
				}
			}
			if (fingerCount != 0) {
				Vector3 childPosition = childPositionSum / fingerCount;
				currMecBone.childMecanimPoint = childPosition - currBone.position;
			}

			return;
		}

		if (currBoneType == HumanBodyBones.RightHand) {
			// For hands we go through children adding the points for just fingers, not thumb.  We then divide by number of fingers to get average direction towards fingers
			Vector3 childPositionSum = Vector3.zero;
			int fingerCount = 0;
			for (int i = 0; i < currMecBone.childrenMecanim.Count; i++) {
				if (mecanimLookup.ContainsKey (currMecBone.childrenMecanim[i].bone.GetInstanceID ())) {
					if (mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.RightIndexProximal ||
						mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.RightLittleProximal ||
						mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.RightMiddleProximal ||
						mecanimLookup[currMecBone.childrenMecanim[i].bone.GetInstanceID ()] == HumanBodyBones.RightRingProximal) {
						childPositionSum += currMecBone.childrenMecanim[i].bone.position;
						fingerCount++;
					}
				}
			}
			if (fingerCount != 0) {
				Vector3 childPosition = childPositionSum / fingerCount;
				currMecBone.childMecanimPoint = childPosition - currBone.position;
			}

			return;
		}

		// Find childMecanimPoint for all others where child count would just be one
		if (currMecBone.childrenMecanim.Count == 1) {
			// One child means we can use it to calculate childMecanimPoint (Vector from curr bone to next mechamim in world space)
			currMecBone.childMecanimPoint = currMecBone.childrenMecanim[0].bone.position - currBone.position;
		}
	}

	/// <summary>
	/// Gives us a rotation from the boneDirection to an axis aligned direction
	/// <br>It will use the longest (X/Y/Z) of boneDirection to determin which axis to return a rotation to</br>
	/// </summary>
	public Quaternion GetBoneFix (Transform bone, Vector3 boneDirection, HumanBodyBones boneType) {
		// This function gives a rotation to align the bone to the positive X axis
		Quaternion boneFixRot = Quaternion.identity;

		Vector3 boneRotVec = new Vector3 (boneDirection.x, boneDirection.y, boneDirection.z);
		Vector3 boneRotVecAbs = new Vector3 (Math.Abs (boneDirection.x), Math.Abs (boneDirection.y), Math.Abs (boneDirection.z));

		// If the abs X axis is longest we remove Z rotation before aligning to X
		if (boneRotVecAbs.x > boneRotVecAbs.y && boneRotVecAbs.x > boneRotVecAbs.z) {
			boneRotVec.z = 0f;
			boneFixRot = Quaternion.FromToRotation (boneRotVec, Vector3.right);
		}

		// If the abs Y axis is longest we want to undo the Y before aligning to X
		if (boneRotVecAbs.y > boneRotVecAbs.x && boneRotVecAbs.y > boneRotVecAbs.z) {
			if (boneDirection.z < 0) {
				boneFixRot = Quaternion.FromToRotation (boneDirection, Vector3.forward);
				boneFixRot = Quaternion.Euler (-boneFixRot.eulerAngles.x, 0f, 0f) * Quaternion.FromToRotation (boneDirection, Vector3.right);
			} else {
				boneFixRot = Quaternion.FromToRotation (boneDirection, Vector3.forward);
				boneFixRot = Quaternion.Euler (boneFixRot.eulerAngles.x, 0f, 0f) * Quaternion.FromToRotation (boneDirection, Vector3.right);
			}
		}

		// If the abs Z axis is longest we remove X rotation before aligning to Z
		if (boneRotVecAbs.z > boneRotVecAbs.x && boneRotVecAbs.z > boneRotVecAbs.y) {
			boneRotVec.x = 0f;
			boneFixRot = Quaternion.FromToRotation (boneRotVec, Vector3.right);
		}

		return boneFixRot;
	}

	/// <summary>
	/// Use all the data collected and calculated to generate the colliders
	/// </summary>
	public void GenerateCollidersForBones () {
		// We itterate through all the bones in the mecanimBoneLookup generating a collider for each
		foreach (KeyValuePair<int, MecanimBone> currMecBone in mecanimBoneLookup) {
			// Ensure the bone isn't in exlude list.  Also check if it has been grouped, if it was grouped MappingKeyBones and MappingKeyBones would not contain the bone id
			if (excludeLookup.ContainsKey (currMecBone.Value.bone.GetInstanceID ()) == false && MappingKeyBones.ContainsKey (currMecBone.Value.bone.GetInstanceID ()) == true && MappingKeyBones[currMecBone.Value.bone.GetInstanceID ()] == true) {
				// Create GameObject that collider will be attached to
				GameObject newColliderObj = new GameObject (currMecBone.Value.bone.gameObject.name + "_Collider");
				newColliderObj.transform.position = currMecBone.Value.vertexCenter;
				newColliderObj.transform.parent = currMecBone.Value.bone;
				generatedColliders.Add (newColliderObj);

				// Determin largest axis

				// First move center and bounds from bone space to local space
				Vector3 minBounds = currMecBone.Value.minBounds - currMecBone.Value.bone.position;
				Vector3 maxBounds = currMecBone.Value.maxBounds - currMecBone.Value.bone.position;

				Quaternion boneFixRot = GetBoneFix (currMecBone.Value.bone, currMecBone.Value.childMecanimPoint, currMecBone.Value.humanBodyBone);
				minBounds = boneFixRot * minBounds;
				maxBounds = boneFixRot * maxBounds;

				Vector3 bounds = maxBounds - minBounds;
				float largestAxis = Mathf.Abs (bounds.x);
				fsBoneAxis largestBoneAxis = fsBoneAxis.x_axis;

				if (Mathf.Abs (bounds.y) > largestAxis) {
					largestAxis = Mathf.Abs (bounds.y);
					largestBoneAxis = fsBoneAxis.y_axis;
				}

				if (Mathf.Abs (bounds.z) > largestAxis) {
					largestAxis = Mathf.Abs (bounds.z);
					largestBoneAxis = fsBoneAxis.z_axis;
				}

				// Determin what type of collider to override with
				// There are 3 lists, if the current bones ID is in one of the lists then override and put a collider of desired type
				// If the id isn't in an override then we use the default setting
				if (capsuleLookup.ContainsKey (currMecBone.Value.bone.GetInstanceID ()) == true) {
					MakeCapsuleCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
				} else if (sphereLookup.ContainsKey (currMecBone.Value.bone.GetInstanceID ()) == true) {
					MakeSphereCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
				} else if (boxLookup.ContainsKey (currMecBone.Value.bone.GetInstanceID ()) == true) {
					MakeBoxCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
				} else {
					// The current bone doesn't have an override collider set so we will put the default on based off variable: colliderGeneration
					// This is set using a dropdown on the scrip (default is colliderGenerationType.humanoid)
					switch (defaultBody) {
						case colliderGenerationType.boxes:
							MakeBoxCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
							break;

						case colliderGenerationType.capsules:
							MakeCapsuleCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
							break;

						case colliderGenerationType.spheres:
							MakeSphereCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
							break;

						case colliderGenerationType.humanoid:
							if (humanoidLookup.ContainsKey (currMecBone.Value.bone.GetInstanceID ()) == true) {
								switch (humanoidLookup[currMecBone.Value.bone.GetInstanceID ()]) {
									case fColliderType.boxes:
										MakeBoxCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
										break;

									case fColliderType.capsules:
										MakeCapsuleCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
										break;

									case fColliderType.spheres:
										MakeSphereCollider (newColliderObj, currMecBone.Key, currMecBone.Value, largestBoneAxis);
										break;
								}
							}
							break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Creates a Capsule collider for the given MecanimBone and axis, adds it to parentObj passed in
	/// </summary>
	private void MakeCapsuleCollider (GameObject parentObj, int boneTransformID, MecanimBone currMecBone, fsBoneAxis correctAxis) {
		CapsuleCollider newCapsuleCollider = parentObj.AddComponent<CapsuleCollider> ();

		// First move center and bounds from bone space to local space
		Vector3 minBounds = currMecBone.minBounds - currMecBone.bone.position;
		Vector3 maxBounds = currMecBone.maxBounds - currMecBone.bone.position;

		// Rotate bounds to x-axis align
		Quaternion boneFixRot = GetBoneFix (currMecBone.bone, currMecBone.childMecanimPoint, currMecBone.humanBodyBone);
		minBounds = boneFixRot * minBounds;
		maxBounds = boneFixRot * maxBounds;

		// Create the capsule
		float axisHeight = maxBounds.x - minBounds.x;
		float radius1 = maxBounds.y - minBounds.y;
		float radius2 = maxBounds.z - minBounds.z;

		float radius = (radius1 + radius2) / 2f;        // We want radius to be between the two shortest bounds
		radius = radius / 2f;                           // Convert from diamiter to radius

		// If we didn't find the correct bounds remove collider, its not valid
		if (float.IsInfinity (axisHeight) || float.IsInfinity (radius)) {
			DestroyImmediate (parentObj);
			return;
		}

		switch (correctAxis) {
			case fsBoneAxis.x_axis:
				axisHeight = axisHeight * 0.95f;            // Helps prevent overlap of colliders by reducing axis height by a small amount

				// if radius is higher then half height then height gets longer to compensate, we avoid this by restricting radius by half height
				// Height is the more important variable since being too long can cause overlap which will cause physics problems during simulation
				if (radius > (axisHeight / 2f)) {
					radius = (axisHeight / 2f);
				}

				newCapsuleCollider.direction = 0;
				newCapsuleCollider.height = axisHeight;
				newCapsuleCollider.radius = radius;
				break;

			case fsBoneAxis.y_axis:
				axisHeight = maxBounds.y - minBounds.y;
				radius1 = maxBounds.x - minBounds.x;
				radius2 = maxBounds.z - minBounds.z;
				radius = (radius1 + radius2) / 2f;          // We want radius to be between the two shortest bounds
				radius = radius / 2f;                       // Convert from diamiter to radius

				axisHeight = axisHeight * 0.95f;            // Helps prevent overlap of colliders by reducing axis height by a small amount

				// if radius is higher then half height then height gets longer to compensate, we avoid this by restricting radius by half height
				// Height is the more important variable since being too long can cause overlap which will cause physics problems during simulation
				if (radius > (axisHeight / 2f)) {
					radius = (axisHeight / 2f);
				}

				newCapsuleCollider.direction = 1;
				newCapsuleCollider.height = axisHeight;
				newCapsuleCollider.radius = radius;
				break;

			case fsBoneAxis.z_axis:
				axisHeight = maxBounds.z - minBounds.z;
				radius1 = maxBounds.x - minBounds.x;
				radius2 = maxBounds.y - minBounds.y;
				radius = (radius1 + radius2) / 2f;          // We want radius to be between the two shortest bounds
				radius = radius / 2f;                       // Convert from diamiter to radius

				axisHeight = axisHeight * 0.95f;            // Helps prevent overlap of colliders by reducing axis height by a small amount

				// if radius is higher then half height then height gets longer to compensate, we avoid this by restricting radius by half height
				// Height is the more important variable since being too long can cause overlap which will cause physics problems during simulation
				if (radius > (axisHeight / 2f)) {
					radius = (axisHeight / 2f);
				}

				newCapsuleCollider.direction = 2;
				newCapsuleCollider.height = axisHeight;
				newCapsuleCollider.radius = radius;
				break;
		}

		// We don't want legs to be bigger than the maxLegRadius or they will clip each other
		if (currMecBone.bone.GetInstanceID () == av_LeftUpperLeg.GetInstanceID () || currMecBone.bone.GetInstanceID () == av_RightUpperLeg.GetInstanceID ()) {
			if (newCapsuleCollider.radius > maxLegRadius) {
				newCapsuleCollider.radius = maxLegRadius;
			}
		}

		newCapsuleCollider.transform.rotation = Quaternion.Inverse (boneFixRot);
	}

	/// <summary>
	/// Creates a Box collider for the given MecanimBone and axis, adds it to parentObj passed in
	/// </summary>
	private void MakeBoxCollider (GameObject parentObj, int boneTransformID, MecanimBone currMecBone, fsBoneAxis correctAxis) {
		BoxCollider newBoxCollider = parentObj.AddComponent<BoxCollider> ();

		// Fix scale issues so box is correct size given scale of avatar parts
		if (currMecBone.bone.transform.lossyScale.x >= 1) {
			parentObj.transform.localScale = Vector3.one;
		} else {
			parentObj.transform.localScale = new Vector3 (1f / currMecBone.bone.transform.lossyScale.x, 1f / currMecBone.bone.transform.lossyScale.y, 1f / currMecBone.bone.transform.lossyScale.z);
		}

		// First move center and bounds from bone space to local space
		Vector3 minBounds = currMecBone.minBounds - currMecBone.bone.position;
		Vector3 maxBounds = currMecBone.maxBounds - currMecBone.bone.position;

		// Rotate bounds to x-axis align
		Quaternion boneFixRot = GetBoneFix (currMecBone.bone, currMecBone.childMecanimPoint, currMecBone.humanBodyBone);
		minBounds = boneFixRot * minBounds;
		maxBounds = boneFixRot * maxBounds;

		// Calculate box size
		Vector3 boxSize = (maxBounds - minBounds) * sizeMultiplier;
		boxSize = boxSize * 0.9f;                                           // Boxes always a bit bigger then they need to be due to vetex data ouside bulk of body, this fudge factor tightens things up a bit
		boxSize.x = Mathf.Abs (boxSize.x);
		boxSize.y = Mathf.Abs (boxSize.y);
		boxSize.z = Mathf.Abs (boxSize.z);

		// If we didn't find the correct bounds remove collider, its not valid
		if (float.IsInfinity (boxSize.x) || float.IsInfinity (boxSize.x) || float.IsInfinity (boxSize.x)) {
			DestroyImmediate (parentObj);
			return;
		}

		if (feetLookup.ContainsKey (currMecBone.bone.GetInstanceID ()) == true) {
			if (feetLookup.ContainsKey (currMecBone.parentMecanimBone.bone.GetInstanceID ()) == true) {
				boxSize.y = boxSize.y * 0.8f;             // We fudge this a bit so the feet colliders arn't dragging on the floor or lower leg colliders
				newBoxCollider.center = new Vector3 (newBoxCollider.center.x + (boxSize.x * 0.2f), newBoxCollider.center.y + (boxSize.y * 0.2f / 2f), newBoxCollider.center.z);
			} else {
				// Check if the height of our foot box is higher then the ankle bone, if so we want to reduce it some
				if (boxSize.y > currMecBone.bone.position.y) {
					float diff = (boxSize.y - currMecBone.bone.position.y);
					boxSize.y = currMecBone.bone.position.y * 0.7f;             // We fudge this a bit so the feet colliders arn't dragging on the floor or lower leg colliders
					newBoxCollider.center = new Vector3 (newBoxCollider.center.x, -diff / 2f, newBoxCollider.center.z);
				}
			}
		}

		newBoxCollider.size = boxSize;
		newBoxCollider.transform.rotation = Quaternion.Inverse (boneFixRot);
	}

	/// <summary>
	/// Creates a Sphere collider for the given MecanimBone and axis, adds it to parentObj passed in
	/// <br>This will rarely be used but provided for completness</br>
	/// </summary>
	private void MakeSphereCollider (GameObject parentObj, int boneTransformID, MecanimBone currMecBone, fsBoneAxis correctAxis) {
		SphereCollider newSphereCollider = parentObj.AddComponent<SphereCollider> ();
		newSphereCollider.center = Vector3.zero;

		// First move center and bounds from bone space to local space
		Vector3 minBounds = currMecBone.minBounds - currMecBone.bone.position;
		Vector3 maxBounds = currMecBone.maxBounds - currMecBone.bone.position;

		// Rotate bounds to x-axis align
		Quaternion boneFixRot = GetBoneFix (currMecBone.bone, currMecBone.childMecanimPoint, currMecBone.humanBodyBone);
		minBounds = boneFixRot * minBounds;
		maxBounds = boneFixRot * maxBounds;

		// Calculate box size
		Vector3 boxSize = (maxBounds - minBounds) * sizeMultiplier;
		boxSize = boxSize * 0.9f;                                           // Boxes always a bit bigger then they need to be due to vetex data ouside bulk of body, this fudge factor tightens things up a bit
		boxSize.x = Mathf.Abs (boxSize.x);
		boxSize.y = Mathf.Abs (boxSize.y);
		boxSize.z = Mathf.Abs (boxSize.z);

		float radius = boxSize.x;

		if (boxSize.y < radius)
			radius = boxSize.z;

		if (boxSize.z < radius)
			radius = boxSize.z;

		// If we didn't find the correct bounds remove collider, its not valid
		if (float.IsInfinity (radius)) {
			DestroyImmediate (parentObj);
			return;
		}

		newSphereCollider.radius = Mathf.Abs (radius / 2F);
	}

	#region Setup

	public Dictionary<int, int> MappingLookup = new Dictionary<int, int> ();
	public Dictionary<int, bool> MappingKeyBones = new Dictionary<int, bool> ();

	public void CreateMappingLookup () {
		// Generate MappingLookup for all bones first, so if minial collider level is selected first it isn't missing anything if we move up to full colliders later
		CreateMappingLookupFull ();
		MappingLookup.Clear ();
		CreateMappingLookupRecurse (av_Hips, av_Hips.GetInstanceID ());

		// colliderLevel is set by SelectorWindow when changed in the drop down
		switch (colliderLevel) {
			case ColliderLevel.Full:
				CreateMappingLookupFull ();
				break;

			case ColliderLevel.Medium:
				CreateMappingLookupMedium ();
				break;

			case ColliderLevel.Minimal:
				CreateMappingLookupMinimal ();
				break;

			default:
			case ColliderLevel.None:
				// We don't have to do anything if no colliders will be generated
				break;
		}
	}

	public void AddBoneIfExists (Animator animator, HumanBodyBones targetBone, Dictionary<int, bool> mappingKeyBones, bool includeBone) {
		// This function will check if a bone exists before adding it to the lookup dictonary passed in (mappingKeyBoens) with the value (includeBone)
		Transform boneTransform = animator.GetBoneTransform (targetBone);
		if (boneTransform != null && mappingKeyBones != null && mappingKeyBones.ContainsKey (boneTransform.GetInstanceID ()) == false) {
			mappingKeyBones.Add (boneTransform.GetInstanceID (), includeBone);
		}
	}

	public void CreateMappingLookupFull () {
		MappingKeyBones.Clear ();

		// We add all key bones defined in function SetQuickHumanoidLookups () for full body minus a few bones
		foreach (KeyValuePair<int, Transform> currBone in keyBoneLookup) {
			if (av_LeftEye != null && currBone.Key == av_LeftEye.GetInstanceID ()) continue;
			if (av_RightEye != null && currBone.Key == av_RightEye.GetInstanceID ()) continue;
			if (av_Jaw != null && currBone.Key == av_Jaw.GetInstanceID ()) continue;

			MappingKeyBones.Add (currBone.Key, true);
		}

		if (av_LeftShoulder != null) MappingKeyBones.Remove (av_LeftShoulder.GetInstanceID ());
		if (av_RightShoulder != null) MappingKeyBones.Remove (av_RightShoulder.GetInstanceID ());
	}

	public void CreateMappingLookupMedium () {
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		MappingKeyBones.Clear ();

		// Add Hips
		MappingKeyBones.Add (av_Hips.GetInstanceID (), true);

		// Add chest, some skeletons won't have all spine bones so we check all 3 and record the first that isn't null
		Transform highestSpineBone = av_UpperChest;
		if (highestSpineBone == null) highestSpineBone = av_Chest;
		if (highestSpineBone == null) highestSpineBone = av_Spine;

		MappingKeyBones.Add (highestSpineBone.GetInstanceID (), true);

		// Add Head
		AddBoneIfExists (animator, HumanBodyBones.Head, MappingKeyBones, true);

		// Ignore Neck
		AddBoneIfExists (animator, HumanBodyBones.Neck, MappingKeyBones, false);

		// Add Lower Arms, Hands, and tips of index finger and thumb.  Ignore all other bones in arms
		AddBoneIfExists (animator, HumanBodyBones.RightUpperArm, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightLowerArm, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightHand, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightIndexDistal, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightThumbDistal, MappingKeyBones, true);

		AddBoneIfExists (animator, HumanBodyBones.RightIndexIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightIndexProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightThumbIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightThumbProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightMiddleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightMiddleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightMiddleProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightRingDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightRingIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightRingProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLittleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLittleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLittleProximal, MappingKeyBones, false);

		AddBoneIfExists (animator, HumanBodyBones.LeftUpperArm, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftLowerArm, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftHand, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftIndexDistal, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftThumbDistal, MappingKeyBones, true);

		AddBoneIfExists (animator, HumanBodyBones.LeftIndexIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftIndexProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftThumbIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftThumbProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftMiddleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftMiddleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftMiddleProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftRingDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftRingIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftRingProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLittleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLittleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLittleProximal, MappingKeyBones, false);

		// Remove everything below the ankles
		AddBoneIfExists (animator, HumanBodyBones.RightFoot, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightToes, MappingKeyBones, false);

		AddBoneIfExists (animator, HumanBodyBones.LeftFoot, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftToes, MappingKeyBones, false);

		// Keep Legs
		AddBoneIfExists (animator, HumanBodyBones.LeftUpperLeg, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftLowerLeg, MappingKeyBones, true);

		AddBoneIfExists (animator, HumanBodyBones.RightUpperLeg, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightLowerLeg, MappingKeyBones, true);
	}

	public void CreateMappingLookupMinimal () {
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		MappingKeyBones.Clear ();

		// Add Hips
		MappingKeyBones.Add (av_Hips.GetInstanceID (), true);

		// Add chest, some skeletons won't have all spine bones so we check all 3 and record the first that isn't null
		Transform highestSpineBone = av_UpperChest;
		if (highestSpineBone == null) highestSpineBone = av_Chest;
		if (highestSpineBone == null) highestSpineBone = av_Spine;

		MappingKeyBones.Add (highestSpineBone.GetInstanceID (), true);

		// Add Head
		AddBoneIfExists (animator, HumanBodyBones.Head, MappingKeyBones, false);

		// Ignore Neck
		AddBoneIfExists (animator, HumanBodyBones.Neck, MappingKeyBones, false);

		// Add Lower Arms, Hands, and tips of index finger and thumb.  Ignore all other bones in arms
		AddBoneIfExists (animator, HumanBodyBones.RightLowerArm, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightHand, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightIndexDistal, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.RightThumbDistal, MappingKeyBones, true);

		AddBoneIfExists (animator, HumanBodyBones.RightUpperArm, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightIndexIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightIndexProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightThumbIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightThumbProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightMiddleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightMiddleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightMiddleProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightRingDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightRingIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightRingProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLittleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLittleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLittleProximal, MappingKeyBones, false);

		AddBoneIfExists (animator, HumanBodyBones.LeftLowerArm, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftHand, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftIndexDistal, MappingKeyBones, true);
		AddBoneIfExists (animator, HumanBodyBones.LeftThumbDistal, MappingKeyBones, true);

		AddBoneIfExists (animator, HumanBodyBones.LeftUpperArm, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftIndexIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftIndexProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftThumbIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftThumbProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftMiddleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftMiddleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftMiddleProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftRingDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftRingIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftRingProximal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLittleDistal, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLittleIntermediate, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLittleProximal, MappingKeyBones, false);

		// Remove everything below the hips
		AddBoneIfExists (animator, HumanBodyBones.RightUpperLeg, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightLowerLeg, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightFoot, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.RightToes, MappingKeyBones, false);

		AddBoneIfExists (animator, HumanBodyBones.LeftUpperLeg, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftLowerLeg, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftFoot, MappingKeyBones, false);
		AddBoneIfExists (animator, HumanBodyBones.LeftToes, MappingKeyBones, false);
	}

	public void CreateMappingLookupRecurse (Transform currBone, int parentID) {
		// Mapping lookup tells us when to set a new parent
		// MappingLookup will have all key bones added for the collision level
		// All children of a key bone will have thier vertices added to parent
		if (MappingKeyBones.ContainsKey (currBone.GetInstanceID ())) {
			if (MappingKeyBones[currBone.GetInstanceID ()] == true) {
				// We want to add this bone and all its children until we hit another key bone
				parentID = currBone.GetInstanceID ();
				MappingLookup.Add (parentID, parentID);
			} else {
				// We want to exclude this bone and all its children until we hit another key bone
				parentID = 0;
				MappingLookup.Add (currBone.GetInstanceID (), parentID);
			}
		} else {
			//MappingLookup.Add (parentID, currBone.GetInstanceID ());
			MappingLookup.Add (currBone.GetInstanceID (), parentID);
		}

		for (int i = 0; i < currBone.childCount; i++) {
			CreateMappingLookupRecurse (currBone.GetChild (i), parentID);
		}
	}

	public void PostPuppetGenerationCleanup () {
		// Cleanup Colliders
		for (int i = 0; i < generatedColliders.Count; i++) {
			GameObject.DestroyImmediate (generatedColliders[i]);
		}

		generatedColliders.Clear ();
		existingColliders.Clear ();
	}

	private void SetupHumanoidLookup () {
		// The humanoid body type has specific colliders for each part
		// Make lookup tables for humanoid drop down
		SetupHumanoidRecurse (av_Hips, fColliderType.boxes);
		SetupHumanoidRecurse (av_LeftUpperLeg, fColliderType.capsules);
		SetupHumanoidRecurse (av_RightUpperLeg, fColliderType.capsules);

		SetupHumanoidRecurse (av_LeftFoot, fColliderType.boxes);
		SetupHumanoidRecurse (av_RightFoot, fColliderType.boxes);

		SetupHumanoidRecurse (av_LeftUpperArm, fColliderType.capsules);
		SetupHumanoidRecurse (av_RightUpperArm, fColliderType.capsules);

		SetHumanoidLookup (av_LeftHand, fColliderType.boxes);
		SetHumanoidLookup (av_RightHand, fColliderType.boxes);

		SetupHumanoidRecurse (av_Neck, fColliderType.capsules);
	}

	private void SetupHumanoidRecurse (Transform currBone, fColliderType colliderTypeToSetBoneTo) {
		if (currBone == null) return;

		SetHumanoidLookup (currBone, colliderTypeToSetBoneTo);

		for (int i = 0; i < currBone.childCount; i++) {
			SetupHumanoidRecurse (currBone.GetChild (i), colliderTypeToSetBoneTo);
		}
	}

	private void SetHumanoidLookup (Transform currBone, fColliderType colliderTypeToSetBoneTo) {
		if (currBone == null) return;

		if (humanoidLookup.ContainsKey (currBone.GetInstanceID ()) == true) {
			humanoidLookup[currBone.GetInstanceID ()] = colliderTypeToSetBoneTo;
		} else {
			humanoidLookup.Add (currBone.GetInstanceID (), colliderTypeToSetBoneTo);
		}
	}

	private void SetupLegLookup () {
		// Add both feet and recurse through thier children adding them as well
		if (av_LeftUpperLeg != null) {
			legLookup.Add (av_LeftUpperLeg.GetInstanceID (), true);
			SetupLegLookupRecurse (av_LeftUpperLeg);
		}
		if (av_RightUpperLeg != null) {
			legLookup.Add (av_RightUpperLeg.GetInstanceID (), true);
			SetupLegLookupRecurse (av_RightUpperLeg);
		}
	}

	private void SetupLegLookupRecurse (Transform currBone) {
		for (int i = 0; i < currBone.childCount; i++) {
			// We don't want to recurse through feet
			if (feetLookup.ContainsKey (currBone.GetChild (i).GetInstanceID ()) == true)
				return;

			// Not Feed so add then recurse
			if (legLookup.ContainsKey (currBone.GetChild (i).GetInstanceID ()) == false) {
				legLookup.Add (currBone.GetChild (i).GetInstanceID (), true);
			}

			SetupLegLookupRecurse (currBone.GetChild (i));
		}
	}

	private void SetupFeetLookup () {
		// Add both feet and recurse through thier children adding them as well
		if (av_LeftFoot != null) {
			feetLookup.Add (av_LeftFoot.GetInstanceID (), true);
			FeetLookupRecurse (av_LeftFoot);
		}
		if (av_RightFoot != null) {
			feetLookup.Add (av_RightFoot.GetInstanceID (), true);
			FeetLookupRecurse (av_RightFoot);
		}
	}

	private void FeetLookupRecurse (Transform currBone) {
		for (int i = 0; i < currBone.childCount; i++) {
			if (feetLookup.ContainsKey (currBone.GetChild (i).GetInstanceID ()) == false) {
				feetLookup.Add (currBone.GetChild (i).GetInstanceID (), true);
			}

			FeetLookupRecurse (currBone.GetChild (i));
		}
	}

	public void SetQuickHumanoidLookups () {
		// Ensure we have animator
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		// These are hard coded references to the various Transforms for spcific parts of the Mechanim body
		av_Root = animator.transform;

		av_Hips = animator.GetBoneTransform (HumanBodyBones.Hips);
		AddKeyBoneLookup (av_Hips);
		av_LeftUpperLeg = animator.GetBoneTransform (HumanBodyBones.LeftUpperLeg);
		AddKeyBoneLookup (av_LeftUpperLeg);
		av_RightUpperLeg = animator.GetBoneTransform (HumanBodyBones.RightUpperLeg);
		AddKeyBoneLookup (av_RightUpperLeg);
		av_LeftLowerLeg = animator.GetBoneTransform (HumanBodyBones.LeftLowerLeg);
		AddKeyBoneLookup (av_LeftLowerLeg);
		av_RightLowerLeg = animator.GetBoneTransform (HumanBodyBones.RightLowerLeg);
		AddKeyBoneLookup (av_RightLowerLeg);
		av_LeftFoot = animator.GetBoneTransform (HumanBodyBones.LeftFoot);
		AddKeyBoneLookup (av_LeftFoot);
		av_RightFoot = animator.GetBoneTransform (HumanBodyBones.RightFoot);
		AddKeyBoneLookup (av_RightFoot);
		av_Spine = animator.GetBoneTransform (HumanBodyBones.Spine);
		AddKeyBoneLookup (av_Spine);
		av_Chest = animator.GetBoneTransform (HumanBodyBones.Chest);
		AddKeyBoneLookup (av_Chest);
		av_Neck = animator.GetBoneTransform (HumanBodyBones.Neck);
		AddKeyBoneLookup (av_Neck);
		av_Head = animator.GetBoneTransform (HumanBodyBones.Head);
		AddKeyBoneLookup (av_Head);
		av_LeftShoulder = animator.GetBoneTransform (HumanBodyBones.LeftShoulder);
		AddKeyBoneLookup (av_LeftShoulder);
		av_RightShoulder = animator.GetBoneTransform (HumanBodyBones.RightShoulder);
		AddKeyBoneLookup (av_RightShoulder);
		av_LeftUpperArm = animator.GetBoneTransform (HumanBodyBones.LeftUpperArm);
		AddKeyBoneLookup (av_LeftUpperArm);
		av_RightUpperArm = animator.GetBoneTransform (HumanBodyBones.RightUpperArm);
		AddKeyBoneLookup (av_RightUpperArm);
		av_LeftLowerArm = animator.GetBoneTransform (HumanBodyBones.LeftLowerArm);
		AddKeyBoneLookup (av_LeftLowerArm);
		av_RightLowerArm = animator.GetBoneTransform (HumanBodyBones.RightLowerArm);
		AddKeyBoneLookup (av_RightLowerArm);
		av_LeftHand = animator.GetBoneTransform (HumanBodyBones.LeftHand);
		AddKeyBoneLookup (av_LeftHand);
		av_RightHand = animator.GetBoneTransform (HumanBodyBones.RightHand);
		AddKeyBoneLookup (av_RightHand);
		av_LeftToes = animator.GetBoneTransform (HumanBodyBones.LeftToes);
		AddKeyBoneLookup (av_LeftToes);
		av_RightToes = animator.GetBoneTransform (HumanBodyBones.RightToes);
		AddKeyBoneLookup (av_RightToes);
		av_LeftEye = animator.GetBoneTransform (HumanBodyBones.LeftEye);
		AddKeyBoneLookup (av_LeftEye);
		av_RightEye = animator.GetBoneTransform (HumanBodyBones.RightEye);
		AddKeyBoneLookup (av_RightEye);
		av_Jaw = animator.GetBoneTransform (HumanBodyBones.Jaw);
		AddKeyBoneLookup (av_Jaw);
		av_LeftThumbProximal = animator.GetBoneTransform (HumanBodyBones.LeftThumbProximal);
		AddKeyBoneLookup (av_LeftThumbProximal);
		av_LeftThumbIntermediate = animator.GetBoneTransform (HumanBodyBones.LeftThumbIntermediate);
		AddKeyBoneLookup (av_LeftThumbIntermediate);
		av_LeftThumbDistal = animator.GetBoneTransform (HumanBodyBones.LeftThumbDistal);
		AddKeyBoneLookup (av_LeftThumbDistal);
		av_LeftIndexProximal = animator.GetBoneTransform (HumanBodyBones.LeftIndexProximal);
		AddKeyBoneLookup (av_LeftIndexProximal);
		av_LeftIndexIntermediate = animator.GetBoneTransform (HumanBodyBones.LeftIndexIntermediate);
		AddKeyBoneLookup (av_LeftIndexIntermediate);
		av_LeftIndexDistal = animator.GetBoneTransform (HumanBodyBones.LeftIndexDistal);
		AddKeyBoneLookup (av_LeftIndexDistal);
		av_LeftMiddleProximal = animator.GetBoneTransform (HumanBodyBones.LeftMiddleProximal);
		AddKeyBoneLookup (av_LeftMiddleProximal);
		av_LeftMiddleIntermediate = animator.GetBoneTransform (HumanBodyBones.LeftMiddleIntermediate);
		AddKeyBoneLookup (av_LeftMiddleIntermediate);
		av_LeftMiddleDistal = animator.GetBoneTransform (HumanBodyBones.LeftMiddleDistal);
		AddKeyBoneLookup (av_LeftMiddleDistal);
		av_LeftRingProximal = animator.GetBoneTransform (HumanBodyBones.LeftRingProximal);
		AddKeyBoneLookup (av_LeftRingProximal);
		av_LeftRingIntermediate = animator.GetBoneTransform (HumanBodyBones.LeftRingIntermediate);
		AddKeyBoneLookup (av_LeftRingIntermediate);
		av_LeftRingDistal = animator.GetBoneTransform (HumanBodyBones.LeftRingDistal);
		AddKeyBoneLookup (av_LeftRingDistal);
		av_LeftLittleProximal = animator.GetBoneTransform (HumanBodyBones.LeftLittleProximal);
		AddKeyBoneLookup (av_LeftLittleProximal);
		av_LeftLittleIntermediate = animator.GetBoneTransform (HumanBodyBones.LeftLittleIntermediate);
		AddKeyBoneLookup (av_LeftLittleIntermediate);
		av_LeftLittleDistal = animator.GetBoneTransform (HumanBodyBones.LeftLittleDistal);
		AddKeyBoneLookup (av_LeftLittleDistal);
		av_RightThumbProximal = animator.GetBoneTransform (HumanBodyBones.RightThumbProximal);
		AddKeyBoneLookup (av_RightThumbProximal);
		av_RightThumbIntermediate = animator.GetBoneTransform (HumanBodyBones.RightThumbIntermediate);
		AddKeyBoneLookup (av_RightThumbIntermediate);
		av_RightThumbDistal = animator.GetBoneTransform (HumanBodyBones.RightThumbDistal);
		AddKeyBoneLookup (av_RightThumbDistal);
		av_RightIndexProximal = animator.GetBoneTransform (HumanBodyBones.RightIndexProximal);
		AddKeyBoneLookup (av_RightIndexProximal);
		av_RightIndexIntermediate = animator.GetBoneTransform (HumanBodyBones.RightIndexIntermediate);
		AddKeyBoneLookup (av_RightIndexIntermediate);
		av_RightIndexDistal = animator.GetBoneTransform (HumanBodyBones.RightIndexDistal);
		AddKeyBoneLookup (av_RightIndexDistal);
		av_RightMiddleProximal = animator.GetBoneTransform (HumanBodyBones.RightMiddleProximal);
		AddKeyBoneLookup (av_RightMiddleProximal);
		av_RightMiddleIntermediate = animator.GetBoneTransform (HumanBodyBones.RightMiddleIntermediate);
		AddKeyBoneLookup (av_RightMiddleIntermediate);
		av_RightMiddleDistal = animator.GetBoneTransform (HumanBodyBones.RightMiddleDistal);
		AddKeyBoneLookup (av_RightMiddleDistal);
		av_RightRingProximal = animator.GetBoneTransform (HumanBodyBones.RightRingProximal);
		AddKeyBoneLookup (av_RightRingProximal);
		av_RightRingIntermediate = animator.GetBoneTransform (HumanBodyBones.RightRingIntermediate);
		AddKeyBoneLookup (av_RightRingIntermediate);
		av_RightRingDistal = animator.GetBoneTransform (HumanBodyBones.RightRingDistal);
		AddKeyBoneLookup (av_RightRingDistal);
		av_RightLittleProximal = animator.GetBoneTransform (HumanBodyBones.RightLittleProximal);
		AddKeyBoneLookup (av_RightLittleProximal);
		av_RightLittleIntermediate = animator.GetBoneTransform (HumanBodyBones.RightLittleIntermediate);
		AddKeyBoneLookup (av_RightLittleIntermediate);
		av_RightLittleDistal = animator.GetBoneTransform (HumanBodyBones.RightLittleDistal);
		AddKeyBoneLookup (av_RightLittleDistal);
		av_UpperChest = animator.GetBoneTransform (HumanBodyBones.UpperChest);
		AddKeyBoneLookup (av_UpperChest);
	}

	private void AddKeyBoneLookup (Transform keyBone) {
		if (keyBone != null && keyBoneLookup.ContainsKey (keyBone.GetInstanceID ()) == false) {
			keyBoneLookup.Add (keyBone.GetInstanceID (), keyBone);
		}
	}

	private void FindExistingColliders () {
		// Clear old then start at hips recursivily looking for existing colliders
		existingColliders.Clear ();
		FindExistingCollidersRecurse (av_Hips);
	}

	private void FindExistingCollidersRecurse (Transform currBone) {
		if (currBone != null) {
			Collider[] boneExistingColliders = currBone.gameObject.GetComponents<Collider> ();

			foreach (Collider currCollider in boneExistingColliders) {
				MeshCollider currAsMeshCollider = currCollider as MeshCollider;

				if (currAsMeshCollider != null && currAsMeshCollider.convex == false) {
					Debug.LogError (currCollider.name + " has a mesh collider that does not have convex enabled.  Convex is needed as we are using colliders with rigid bodies.");
				}

				existingColliders.Add (currCollider);
			}

			for (int i = 0; i < currBone.childCount; i++) {
				FindExistingCollidersRecurse (currBone.GetChild (i));
			}
		}
	}

	#endregion Setup

	#region Editor

#if UNITY_EDITOR

	/// <summary>
	/// Will draw the MecanimBone tree in purple and vertices of selected bone as long as an analysis/generation has been done
	/// </summary>
	private void OnDrawGizmos () {
		DrawMecanimBones ();
		DrawSelectedBoneVertices (UnityEditor.Selection.activeGameObject);
	}

	/// <summary>
	/// Starts the recursive bone draw if topBone is valid
	/// </summary>
	public void DrawMecanimBones () {
		if (topBone != null)
			DrawMecanimBonesRecurse (topBone);
	}

	/// <summary>
	/// Recurse through MecanimBone tree drawing the skeleton in purple
	/// </summary>
	/// <param name="currMecBone"></param>
	public void DrawMecanimBonesRecurse (MecanimBone currMecBone) {
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine (currMecBone.bone.position, currMecBone.bone.position + currMecBone.childMecanimPoint);
		Gizmos.DrawSphere (currMecBone.bone.position, 0.005f);

		for (int i = 0; i < currMecBone.childrenMecanim.Count; i++) {
			DrawMecanimBonesRecurse (currMecBone.childrenMecanim[i]);
		}
	}

	public void DrawSelectedBoneVertices (GameObject selectedObject) {
		// Make sure we have something selected
		if (selectedObject == null) return;

		// Draw Debug, this shows the way the vertices are altered before detarmining collider bounds
		bool drawDebug = false;

		// Ensure the bone we are checking has vertices
		if (combinedVertexData == null || combinedVertexData.vertexByBone_01 == null || combinedVertexData.vertexByBone_01.ContainsKey (selectedObject.transform.GetInstanceID ()) == false)
			return;

		// Make sure we are drawing only Mechanim bones
		if (mecanimBoneLookup == null || mecanimBoneLookup.ContainsKey (selectedObject.transform.GetInstanceID ()) == false)
			return;

		MecanimBone currMecBone = mecanimBoneLookup[selectedObject.transform.GetInstanceID ()];
		Quaternion boneFixRot = GetBoneFix (selectedObject.transform, currMecBone.childMecanimPoint, currMecBone.humanBodyBone);

		// This sets up or increment value and baseIncrement so no more then 300 vertices are drawn at a time
		// Too many Gizmos.DrawSphere () calls bogs down the 3D preview in Unity
		// The code will change baseIncrement as time goes on so the 300 vertices you see will rotate through all possible vertices
		float incrementVertex = combinedVertexData.vertexByBone_01[selectedObject.transform.GetInstanceID ()].Count / 300f;
		if (incrementVertex < 1f) incrementVertex = 1f;

		int incrment = (int) incrementVertex;

		if (Time.realtimeSinceStartup > lastIncrementTime + 0.5f) {
			baseIncrement++;
			lastIncrementTime = Time.realtimeSinceStartup;
		}

		if (baseIncrement >= incrment) {
			baseIncrement = 0;
		}

		Vector3 centerPointOrg = Vector3.zero;

		// Useful for working with generation code, knowing where the next bone max point would be (enable when needed)
		if (drawDebug) {
			// Draw the spot where next joint is
			Gizmos.color = Color.white;
			Gizmos.DrawSphere (currMecBone.childMecanimPoint.magnitude * Vector3.right, 0.002f);
		}

		// Draw all the points
		for (int i = 0; i < combinedVertexData.vertexByBone_01[selectedObject.transform.GetInstanceID ()].Count; i = i + baseIncrement + incrment) {
			Vector3 outPoint;
			outPoint = combinedVertexData.vertexByBone_01[selectedObject.transform.GetInstanceID ()][i];

			Gizmos.color = Color.green;
			Gizmos.DrawSphere (outPoint, 0.002f);

			// This will draw the points axis aligned with zero origin (Location detection uses to determin bounds)
			if (drawDebug) {
				Gizmos.color = Color.magenta;
				outPoint -= selectedObject.transform.position;
				outPoint = boneFixRot * outPoint;
			}

			// Ignore points beyond bounds of bone joints (feet don't do this, use as needed)
			//if (outPoint.x < 0) continue;                                           // behind current bone joint
			//if (outPoint.x > currMecBone.childMecanimPoint.magnitude) continue;     // past next bone joint

			Gizmos.DrawSphere (outPoint, 0.002f);
		}

		// Draw vertex Center bone aligned
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere (currMecBone.vertexCenter, 0.002f);

		// Draw origin and center (enable if needed)
		if (drawDebug) {
			// Draw Center Dot
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere (centerPointOrg, 0.002f);
			// Draw vertex Center x-axis aligned
			Vector3 xVertCenter = currMecBone.vertexCenter - selectedObject.transform.position;
			xVertCenter = boneFixRot * xVertCenter;
			Gizmos.DrawSphere (xVertCenter, 0.002f);
		}
	}

#endif
}

#if UNITY_EDITOR

/// <summary>
/// Handles State's GUI, makes sure only one is active at a time
/// </summary>
[CustomEditor (typeof (GenerateColliders), true)]
[CanEditMultipleObjects]
[ExecuteInEditMode]
public class GenerateCollidersEditor : Editor {

	public override void OnInspectorGUI () {
		// Custom viewr so only desired variables and buttons are exposed

		GenerateColliders generateCollidersReferences = (GenerateColliders) target;
		SerializedObject so = new SerializedObject (target);

		SerializedProperty animator = so.FindProperty ("animator");
		EditorGUILayout.PropertyField (animator, true);

		SerializedProperty colliderGenerationDefault = so.FindProperty ("defaultBody");
		EditorGUILayout.PropertyField (colliderGenerationDefault, true);

		SerializedProperty colliderSizeMultiplier = so.FindProperty ("sizeMultiplier");
		EditorGUILayout.PropertyField (colliderSizeMultiplier, true);

		DrawOverrideFields (so);

		DrawReferenceFields (so);

		GUILayout.Space (10);

		if (GUILayout.Button ("Generate Colliders"))
			generateCollidersReferences.DoOpenSelector ();

		if (GUILayout.Button ("Clear Generated Colliders"))
			generateCollidersReferences.DoClearGeneratedColliders ();

		if (GUILayout.Button ("Force Tpose"))
			generateCollidersReferences.DoForceTpose ();

		if (GUILayout.Button ("Clear Model Data"))
			generateCollidersReferences.ClearAnalyze ();

		so.ApplyModifiedProperties ();

		GUILayout.Space (10);
	}

	private void DrawOverrideFields (SerializedObject so) {
		SerializedProperty capsuleTranforms = so.FindProperty ("capsuleTranforms");
		EditorGUILayout.PropertyField (capsuleTranforms, true);

		SerializedProperty sphereTranforms = so.FindProperty ("sphereTranforms");
		EditorGUILayout.PropertyField (sphereTranforms, true);

		SerializedProperty boxTranforms = so.FindProperty ("boxTranforms");
		EditorGUILayout.PropertyField (boxTranforms, true);

		SerializedProperty excludeTranforms = so.FindProperty ("excludeTranforms");
		EditorGUILayout.PropertyField (excludeTranforms, true);
	}

	private void DrawReferenceFields (SerializedObject so) {
		SerializedProperty generatedColliders = so.FindProperty ("generatedColliders");
		EditorGUILayout.PropertyField (generatedColliders, true);

		SerializedProperty existingColliders = so.FindProperty ("existingColliders");
		EditorGUILayout.PropertyField (existingColliders, true);
	}
}

#endif

#endregion Editor