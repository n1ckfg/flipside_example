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
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Flipside.Helpers;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

#endif

namespace Flipside.Avatars {

	public class Viseme {

		public Viseme (int blendShape, float weight) {
			this.blendShape = blendShape;
			this.weight = weight;
		}

		public int blendShape = -1;

		[Range (1, 100)]
		public float weight = 100;
	}

	public class AvatarModelReferences : MonoBehaviour {

		[Tooltip ("Disable VR mode for testing in the Unity editor")]
		public bool disableVRModeInEditor = false;

		[Space (10)]
		[Tooltip ("The name of your character")]
		public string characterName = "";

		[Tooltip ("Credits for this character (e.g., Creative Commons)")]
		public string attribution = "";

		[Tooltip ("Alternate thumbnail to use instead of the automatically generated thumbnail.")]
		[AssetPath.Attribute (typeof (Texture2D))]
		public string thumbnail = "";

		[Tooltip ("Height at which nametag should be displayed, relative to character's height.")]
		public float nameTagHeight = 0.35f;

		[Space (10)]
		[Tooltip ("The center eye transform, used to position the avatar's head")]
		public Transform centerEye;

		[Tooltip ("The eyes bones, used to rotate the eyes")]
		public List<Transform> eyes;

		[Tooltip ("Should this character's eyes ignore eye targets such as other characters?")]
		public bool ignoreEyeTargets = false;

		[Space (10)]
		[Tooltip ("The primary mesh that contains the blendshape information")]
		public SkinnedMeshRenderer mesh;

		[Tooltip ("Extra meshes that will use the same blendshapes. (For example, if teeth is a separate mesh from the rest of the face.)")]
		public SkinnedMeshRenderer[] additionalMeshes;

		[Tooltip ("The animator that controls the avatar's body")]
		public Animator animator;

		[Tooltip ("The animator that controls the avatar's expressions")]
		[HideInInspector]
		public Animator expressionAnimator;

		public enum ExpressionType {
			blendShapes,
			textures,
			simplifiedBlendShapes,
			animation,
			comboTextures,
			animationParameters
		}

		[Space (10)]
		public ExpressionType expressionType = ExpressionType.blendShapes;

		public float smoothing = 50f;

		public UnityEvent OnSitting;
		public UnityEvent OnStanding;

		//Blend Shape Mappings

		[HideInInspector]
		public string neutralShape = "-1";

		[HideInInspector]
		public string happyShape = "-1";

		[HideInInspector]
		public string sadShape = "-1";

		[HideInInspector]
		public string surprisedShape = "-1";

		[HideInInspector]
		public string angryShape = "-1";

		[HideInInspector]
		public string blinkLeftShape = "-1";

		[HideInInspector]
		public string blinkRightShape = "-1";

		[HideInInspector]
		public string blinkAllShape = "-1";

		//Simplified Shapes

		[HideInInspector]
		public string openMouthShape = "-1";

		//Viseme Shape Mappings

		[HideInInspector]
		public string aaahShape = "-1";

		[HideInInspector]
		public string eeeShape = "-1";

		[HideInInspector]
		public string iiShape = "-1";

		[HideInInspector]
		public string ohShape = "-1";

		[HideInInspector]
		public string ooohShape = "-1";

		[HideInInspector]
		public string fuhShape = "-1";

		[HideInInspector]
		public string mmmShape = "-1";

		[HideInInspector]
		public string luhShape = "-1";

		[HideInInspector]
		public string sssShape = "-1";

		//Facial Expression Thresholds

		[HideInInspector]
		public float happyThreshold = 1;

		[HideInInspector]
		public float sadThreshold = 1;

		[HideInInspector]
		public float surprisedThreshold = 1;

		[HideInInspector]
		public float angryThreshold = 1;

		[HideInInspector]
		public float openMouthThreshold = 1;

		//Facial Expression Textures

		[HideInInspector]
		public Texture neutralTexture;

		[HideInInspector]
		public Texture mmmTexture;

		[HideInInspector]
		public Texture fuhTexture;

		[HideInInspector]
		public Texture thTexture;

		[HideInInspector]
		public Texture ddTexture;

		[HideInInspector]
		public Texture kkTexture;

		[HideInInspector]
		public Texture chTexture;

		[HideInInspector]
		public Texture sssTexture;

		[HideInInspector]
		public Texture nnnTexture;

		[HideInInspector]
		public Texture rrrTexture;

		[HideInInspector]
		public Texture aaahTexture;

		[HideInInspector]
		public Texture eeeTexture;

		[HideInInspector]
		public Texture iiTexture;

		[HideInInspector]
		public Texture ohTexture;

		[HideInInspector]
		public Texture ooohTexture;

		[HideInInspector]
		public Transform jawBone;

		[HideInInspector]
		public Vector3 jawBoneClosedRotation;

		[HideInInspector]
		public Vector3 jawBoneOpenRotation;

		public enum BlendShapeIndices {
			neutral, happy, sad, surprised, angry,
			blinkLeft, blinkRight, blinkAll,
			aaah, eee, ii, oh, oooh, fuh, mmm, luh, sss
		}

		[System.Serializable]
		public class BlendShapeRangeList {
			public Vector2[] a;

			public BlendShapeRangeList (int length = 1) {
				a = new Vector2[length];
				for (int i = 0; i < length; i++) {
					a[i] = new Vector2 (0, 100);
				}
			}
		}

		public BlendShapeRangeList[] blendShapeRanges;

		public Transform[] leftWristTwistBones;
		public float[] leftWristTwistCrossfades;

		public Transform[] rightWristTwistBones;
		public float[] rightWristTwistCrossfades;

		[HideInInspector]
		public Vector3 bentElbowScale = new Vector3 (0.02f, 0.06f, 0.04f);

		[HideInInspector]
		[SerializeField]
		public ResourceUsageData resourceUsage;

#if UNITY_EDITOR

		private Flipside.Helpers.PlayerController player;

		public Flipside.Helpers.PlayerController GetPlayerController () {
			return player;
		}

		private void Awake () {
			var lobj = new GameObject ("Temp Lighting");
			lobj.transform.position = new Vector3 (0f, 5f, 0f);
			lobj.transform.eulerAngles = new Vector3 (120f, 0f, 0f);

			var light = lobj.AddComponent<Light> ();
			light.type = LightType.Directional;
			light.color = new Color (0.9f, 0.9f, 0.9f);

			Instantiate (Resources.Load<GameObject> ("AvatarFloor"));

			if (!disableVRModeInEditor) {
				GameObject controllerResource = Resources.Load ("PlayerController") as GameObject;
				GameObject playerInstance = GameObject.Instantiate (controllerResource);

				player = playerInstance.GetComponent<Flipside.Helpers.PlayerController> ();
				player.headCam.clearFlags = CameraClearFlags.Color;
				player.headCam.backgroundColor = new Color32 (200, 200, 200, 1);

				player.TeleportTo (new Vector3 (0f, 0f, 1.5f), new Vector3 (0f, 180f, 0f));

				player.leftHand.OnTriggerDown.AddListener (ToggleAnimation);
			} else {
				UnityEngine.XR.XRSettings.enabled = false;

				var cobj = new GameObject ("Main Camera");
				cobj.transform.position = new Vector3 (0f, 1.25f, 2.5f);
				cobj.transform.eulerAngles = new Vector3 (0f, 180f, 0f);
				var cam = cobj.AddComponent<Camera> ();
				cam.nearClipPlane = 0.1f;
				cam.clearFlags = CameraClearFlags.Color;
				cam.backgroundColor = new Color32 (200, 200, 200, 1);
			}

			Instantiate (Resources.Load<GameObject> ("ToggleAnimation"));
		}

		private void Update () {
			if (Input.GetKeyDown (KeyCode.Space)) {
				ToggleAnimation ();
			}

			if (disableVRModeInEditor) return;

			Vector2 axisValue = player.leftHand.GetPrimaryAxisValue ();
			if (happyShape != "" && axisValue.y >= 0f) {
				var numbers = happyShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						mesh.SetBlendShapeWeight (index, axisValue.y * 100 * happyThreshold);
						ClearBlendShapeList (sadShape, BlendShapeIndices.sad);
					}
				}
			} else if (sadShape != "") {
				var numbers = sadShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						mesh.SetBlendShapeWeight (index, axisValue.y * -100 * sadThreshold);
						ClearBlendShapeList (happyShape, BlendShapeIndices.happy);
					}
				}
			}

			if (angryShape != "" && axisValue.x >= 0f) {
				var numbers = angryShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						mesh.SetBlendShapeWeight (index, axisValue.x * 100 * angryThreshold);
						ClearBlendShapeList (surprisedShape, BlendShapeIndices.surprised);
					}
				}
			} else if (surprisedShape != "") {
				var numbers = surprisedShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						mesh.SetBlendShapeWeight (index, axisValue.x * -100 * surprisedThreshold);
						ClearBlendShapeList (angryShape, BlendShapeIndices.angry);
					}
				}
			}
		}

		public void ToggleAnimation () {
			if (animator.runtimeAnimatorController == null) {
				animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController> ("SampleAnimation");
			} else {
				animator.runtimeAnimatorController = null;
				transform.position = Vector3.zero;
				transform.rotation = Quaternion.identity;
			}
		}

		public void OnDrawGizmos () {
			var bounds = GetBounds ();

			float height = bounds.max.y;
			float horizontalOffset = bounds.min.x * 1.1f;

			Gizmos.color = Color.white;
			GUIStyle guiStyle = new GUIStyle ();
			guiStyle.normal.textColor = Color.white;
			guiStyle.font = (Font) Resources.Load ("Quicksand-Medium", typeof (Font));

			Gizmos.DrawLine (new Vector3 (horizontalOffset, 0, 0), new Vector3 (horizontalOffset, height, 0));
			string meters = height.ToString ("0.00");
			float heightFeet = height * 3.28084f;
			string feet = ((int) heightFeet).ToString ("0");
			string inches = ((heightFeet - (int) heightFeet) * 12).ToString ("0");
			Handles.Label (new Vector3 (horizontalOffset, height, 0), "  " + meters + " meters\n  " + feet + " feet, " + inches + " inches", guiStyle);

			if (centerEye != null) {
				Gizmos.DrawWireSphere (centerEye.position, 0.025f);
				guiStyle.fontSize = 10;
				Handles.Label (centerEye.position + new Vector3 (0f, 0.03f, 0f), "   Center Eye", guiStyle);

				//draw nametag
				var nameTagpos = centerEye.position + Vector3.up * (centerEye.transform.position.y * nameTagHeight);
				Gizmos.DrawLine (nameTagpos - Vector3.right * 0.25f, nameTagpos + Vector3.right * 0.25f);
				guiStyle.alignment = TextAnchor.LowerCenter; //why does this not actually centre it? Or align with the bottom of the text?
				guiStyle.fontSize = 14;
				Handles.Label (nameTagpos, "Name Tag              ", guiStyle);
			}
		}

#endif

		public void FindJawbone () {
			Transform[] children = transform.GetComponentsInChildren<Transform> ();

			if (jawBone == null) {
				foreach (var child in children) {
					if (child.name.ToLower ().Contains ("jaw")) {
						jawBone = child;
						CalcJawRotations ();
					}
				}
			}
		}

		public void CalcJawRotations () {
			CalcJawRotations (30f, Vector3.right);
		}

		public void CalcJawRotations (float degrees, Vector3 direction) {
			if (jawBone == null) return;
			jawBoneClosedRotation = jawBone.localEulerAngles;
			jawBoneOpenRotation = (Quaternion.Euler (direction * degrees) * jawBone.localRotation).eulerAngles;
		}

		public void FindEyes () {
			if (eyes == null || eyes.Count == 0) {
				Transform[] children = transform.GetComponentsInChildren<Transform> ();
				eyes = new List<Transform> ();
				foreach (var child in children) {
					string childname = child.name.ToLower ();
					if (childname.Contains ("eye")
						&& child.childCount == 0
						&& !childname.Contains ("lid")
						&& !childname.Contains ("lash")
						&& !childname.Contains ("glass")
						&& !childname.Contains ("wear")
						&& !childname.Contains ("center")
						&& !eyes.Contains (child)
						&& child.GetComponentInChildren<SkinnedMeshRenderer> () == null) {
						eyes.Add (child);
					}
				}
			}
		}

		public void FindEmptyReferences () {
			Transform[] children = transform.GetComponentsInChildren<Transform> ();

			if (centerEye == null) {
				foreach (var child in children) {
					if (child.name.ToLower ().Contains ("center") && child.name.ToLower ().Contains ("eye")) {
						centerEye = child;
					}
				}
			}

			FindEyes ();

			if (animator == null) {
				animator = GetComponentInChildren<Animator> ();
				if (animator == null) {
					animator = gameObject.AddComponent<Animator> ();
				}
			}

			CreateCenterEye ();

			SkinnedMeshRenderer[] meshs = GetComponentsInChildren<SkinnedMeshRenderer> ();

			if (mesh == null) {
				int blendShapes = -1; //finds mesh with most blend shapes
				foreach (var childMesh in meshs) {
					if (childMesh.sharedMesh.blendShapeCount > blendShapes) {
						blendShapes = childMesh.sharedMesh.blendShapeCount;
						mesh = childMesh;
					}
				}
			}

			foreach (var mesh in meshs) {
				mesh.updateWhenOffscreen = true;
			}

			foreach (var flareLayer in GetComponentsInChildren<FlareLayer> ()) {
				DestroyImmediate (flareLayer);
			}

			foreach (var audioListener in GetComponentsInChildren<AudioListener> ()) {
				DestroyImmediate (audioListener);
			}

			foreach (var camera in GetComponentsInChildren<Camera> ()) {
				DestroyImmediate (camera);
			}

			foreach (var light in GetComponentsInChildren<Light> ()) {
				DestroyImmediate (light);
			}

			EnforceHeight ();
		}

		public void CreateCenterEye () {
			if (!animator.isHuman) {
				return;
			}

			if (centerEye == null) {
				centerEye = new GameObject ("centereye").transform;
				centerEye.rotation = Quaternion.identity;
				centerEye.position = animator.GetBoneTransform (HumanBodyBones.Head).position + new Vector3 (0, 0.12f, 0.12f);
			}

			centerEye.parent = animator.GetBoneTransform (HumanBodyBones.Head);
		}

		public Bounds GetBounds () {
			var combinedBounds = new Bounds ();
			var renderers = GetComponentsInChildren<SkinnedMeshRenderer> ();
			foreach (var render in renderers) {
				combinedBounds.Encapsulate (render.bounds);
			}

			return combinedBounds;
		}

		public void EnforceHeight () {
			if (GetBounds ().size.y < .1f) {
				transform.localScale = Vector3.one * 100;
			}

			if (GetBounds ().size.y > 10f) {
				transform.localScale = Vector3.one * 0.01f;
			}

			Bounds bounds = GetBounds ();
			if (bounds.size.y < .5f ||
				bounds.size.y > 5f) {
				transform.localScale = Vector3.one * 1.8f / bounds.size.y * transform.localScale.y;
			}
		}

		private void ClearSingleBlendShape (int blendShapeIndex, float newValue) {
			if (blendShapeIndex == -1)
				return;
			mesh.SetBlendShapeWeight (blendShapeIndex, newValue);
			if (additionalMeshes != null) {
				foreach (var aMesh in additionalMeshes) {
					if (aMesh == null) continue;
					aMesh.SetBlendShapeWeight (blendShapeIndex, newValue);
				}
			}
		}

		private Vector2 GetRange (int rangeIndex, int numbIndex) {
			if (blendShapeRanges == null) {
				blendShapeRanges = new BlendShapeRangeList[rangeIndex + 1];
			}

			while (blendShapeRanges.Length <= rangeIndex) {
				System.Array.Resize (ref blendShapeRanges, blendShapeRanges.Length + 1);
				blendShapeRanges[blendShapeRanges.Length - 1] = new AvatarModelReferences.BlendShapeRangeList (0);
			}

			if (blendShapeRanges[rangeIndex] == null) {
				var bsl = new BlendShapeRangeList (numbIndex + 1);
				for (int i = 0; i < numbIndex; i++) {
					bsl.a[i] = new Vector2 (0, 100);
				}
				blendShapeRanges[rangeIndex] = bsl;
			}

			if (blendShapeRanges[rangeIndex].a.Length <= numbIndex) {
				Array.Resize (ref blendShapeRanges[rangeIndex].a, numbIndex + 1);
				blendShapeRanges[rangeIndex].a[numbIndex] = new Vector2 (0, 100);
			}

			if (blendShapeRanges[rangeIndex].a[numbIndex].x == 0 && blendShapeRanges[rangeIndex].a[numbIndex].y == 0) {
				blendShapeRanges[rangeIndex].a[numbIndex] = new Vector2 (0, 100);
			}

			return blendShapeRanges[rangeIndex].a[numbIndex];
		}

		private void ClearBlendShapeList (string list, BlendShapeIndices rangeIndex) {
			ClearBlendShapeList (list, (int) rangeIndex);
		}

		private void ClearBlendShapeList (string list, int rangeIndex) {
			var numbers = list.Split (',');
			for (int numbIndex = 0; numbIndex < numbers.Length; numbIndex++) {
				int index = -1;
				if (int.TryParse (numbers[numbIndex], out index)) {
					ClearSingleBlendShape (index, GetRange (rangeIndex, numbIndex).x);
				}
			}
		}

		public void ClearBlendShapes () {
			switch (expressionType) {
				case ExpressionType.blendShapes:
					ClearBlendShapeList (neutralShape, BlendShapeIndices.neutral);
					ClearBlendShapeList (angryShape, BlendShapeIndices.angry);
					ClearBlendShapeList (surprisedShape, BlendShapeIndices.surprised);
					ClearBlendShapeList (happyShape, BlendShapeIndices.happy);
					ClearBlendShapeList (sadShape, BlendShapeIndices.sad);
					ClearBlendShapeList (blinkLeftShape, BlendShapeIndices.blinkLeft);
					ClearBlendShapeList (blinkRightShape, BlendShapeIndices.blinkRight);
					ClearBlendShapeList (blinkAllShape, BlendShapeIndices.blinkAll);

					ClearBlendShapeList (aaahShape, BlendShapeIndices.aaah);
					ClearBlendShapeList (eeeShape, BlendShapeIndices.eee);
					ClearBlendShapeList (iiShape, BlendShapeIndices.ii);
					ClearBlendShapeList (fuhShape, BlendShapeIndices.fuh);
					ClearBlendShapeList (luhShape, BlendShapeIndices.luh);
					ClearBlendShapeList (mmmShape, BlendShapeIndices.mmm);
					ClearBlendShapeList (ohShape, BlendShapeIndices.oh);
					ClearBlendShapeList (ooohShape, BlendShapeIndices.oooh);
					ClearBlendShapeList (sssShape, BlendShapeIndices.sss);
					break;

				case ExpressionType.simplifiedBlendShapes:
					ClearBlendShapeList (neutralShape, BlendShapeIndices.neutral);
					ClearBlendShapeList (angryShape, BlendShapeIndices.angry);
					ClearBlendShapeList (surprisedShape, BlendShapeIndices.surprised);
					ClearBlendShapeList (happyShape, BlendShapeIndices.happy);
					ClearBlendShapeList (sadShape, BlendShapeIndices.sad);
					ClearBlendShapeList (blinkAllShape, BlendShapeIndices.blinkAll);
					ClearBlendShapeList (blinkLeftShape, BlendShapeIndices.blinkLeft);
					ClearBlendShapeList (blinkRightShape, BlendShapeIndices.blinkRight);
					ClearBlendShapeList (openMouthShape, BlendShapeIndices.aaah);
					break;
			}
			var expRefs = GetComponents<FacialExpressionReference> ();
			foreach (var expRef in expRefs) {
				expRef.ClearBlendShapes ();
			}
		}

		internal void CheckIfAdditionalMeshesFitMesh () {
			if (additionalMeshes == null || additionalMeshes.Length == 0)
				return;
			if (mesh == null)
				return;
			foreach (var aMesh in additionalMeshes) {
				if (aMesh != null && aMesh.sharedMesh.blendShapeCount != mesh.sharedMesh.blendShapeCount) {
					//then
				} else {
					//check that each blendShape in aMesh has a match in mesh
				}
			}
		}

		/// <summary>
		/// Find additional meshes with blend shapes
		/// </summary>
		internal void FindAdditionalMeshes () {
			//find all skinned mesh children with blend shapes
			var skinnedMeshChildren = GetComponentsInChildren<SkinnedMeshRenderer> ();
			var expressionSkins = skinnedMeshChildren.Where (c => c.sharedMesh != null && c.sharedMesh.blendShapeCount > 0).ToArray ();
			//make list of blend shape counts
			var shapeCounts = new Dictionary<int, List<SkinnedMeshRenderer>> ();
			foreach (var expS in expressionSkins) {
				var shapeCount = expS.sharedMesh.blendShapeCount;
				if (!shapeCounts.ContainsKey (shapeCount)) {
					shapeCounts.Add (shapeCount, new List<SkinnedMeshRenderer> ());
				}
				shapeCounts[shapeCount].Add (expS);
				Debug.LogFormat ("{1} has {0} blendshapes", shapeCount, expS.name);
			}

			if (shapeCounts.Count == 1) {
				//then they all have the same number of blend shapes, and we can probably use them together
				AssignMeshesToAvatarModelReferences (expressionSkins.ToList ());
			} else { //then we need to put some in additonal components.
					 //assign largest to AvatarModelReferences
				var allCounts = shapeCounts.ToList ().OrderBy (c => -c.Key).ToArray ();
				AssignMeshesToAvatarModelReferences (allCounts[0].Value.ToList ());
				//get a list of other FacialExpressionReferences
				var facialExpressionReferences = GetComponents<FacialExpressionReference> ();
				if (facialExpressionReferences.Length < allCounts.Length - 1) {
					//then add more
					for (int i = facialExpressionReferences.Length; i < allCounts.Length - 1; i++) {
						gameObject.AddComponent<FacialExpressionReference> ();
					}
					facialExpressionReferences = GetComponents<FacialExpressionReference> ();
				}
				for (int i = 1; i < allCounts.Length; i++) {
					//assign meshes to each of the FacialExpressionReferences
					facialExpressionReferences[i - 1].meshes = allCounts[i].Value.ToArray ();
				}
			}
		}

		private void AssignMeshesToAvatarModelReferences (List<SkinnedMeshRenderer> allSkins) {
			mesh = allSkins[0];
			allSkins.RemoveAt (0);
			additionalMeshes = allSkins.ToArray ();
		}

#if UNITY_EDITOR

		public string PreviewButton (Viseme[] visemes, Vector2[] ranges) {
			ClearBlendShapes ();

			for (int i = 0; i < visemes.Length; i++) {
				var viseme = visemes[i];
				var rangeWeight = new Vector2 (0f, 100f);
				if (ranges != null && i < ranges.Length && ranges[i] != null)
					rangeWeight = ranges[i];
				mesh.SetBlendShapeWeight (viseme.blendShape, Mathf.Lerp (rangeWeight.x, rangeWeight.y, viseme.weight / 100f));
				if (additionalMeshes != null) {
					foreach (var aMesh in additionalMeshes) {
						if (aMesh == null) continue;
						aMesh.SetBlendShapeWeight (viseme.blendShape, Mathf.Lerp (rangeWeight.x, rangeWeight.y, viseme.weight / 100f));
					}
				}
			}

			GUI.backgroundColor = new Color (0.23f, 0.35f, 1f);
			Texture2D previewTex = Resources.Load<Texture2D> ("icon-preview");
			if (GUILayout.Button (previewTex, GUILayout.Width (26), GUILayout.Height (20))) {
				AvatarModelReferencesEditor.previewingFieldName = "";
				ClearBlendShapes ();
			}
			GUI.backgroundColor = Color.white;
			return AvatarModelReferencesEditor.previewingFieldName;
		}

		public void FindWristBones () {
			if (animator == null)
				animator = GetComponentInChildren<Animator> (true);
			if (animator == null)
				return;
			var leftArm = animator.GetBoneTransform (HumanBodyBones.LeftLowerArm);
			var leftHand = animator.GetBoneTransform (HumanBodyBones.LeftHand);
			if (leftArm == null)
				return;
			foreach (Transform twistBone in leftArm) {
				if (twistBone != leftHand) {
					leftWristTwistBones = new Transform[1];
					leftWristTwistBones[0] = twistBone;
					break;
				}
			}
			CheckLeftCrossFades ();

			var rightArm = animator.GetBoneTransform (HumanBodyBones.RightLowerArm);
			var rightHand = animator.GetBoneTransform (HumanBodyBones.RightHand);
			if (rightArm == null)
				return;
			foreach (Transform twistBone in rightArm) {
				if (twistBone != rightHand) {
					rightWristTwistBones = new Transform[1];
					rightWristTwistBones[0] = twistBone;
					break;
				}
			}
			CheckRightCrossFades ();
		}

		public void CheckLeftCrossFades () {
			var leftHand = animator.GetBoneTransform (HumanBodyBones.LeftHand);
			if (leftHand == null)
				return;
			if (leftWristTwistBones == null)
				return;
			var newCrossfades = new float[leftWristTwistBones.Length];
			for (int i = 0; i < newCrossfades.Length; i++) {
				if (leftWristTwistCrossfades != null && i < leftWristTwistCrossfades.Length) {
					newCrossfades[i] = leftWristTwistCrossfades[i];
				} else {
					newCrossfades[i] = CalcFadeValue (leftWristTwistBones[i], leftHand);
				}
			}
			leftWristTwistCrossfades = newCrossfades;
		}

		public void CheckRightCrossFades () {
			var rightHand = animator.GetBoneTransform (HumanBodyBones.RightHand);
			if (rightHand == null)
				return;
			if (rightWristTwistBones == null)
				return;
			var newCrossfades = new float[rightWristTwistBones.Length];
			for (int i = 0; i < newCrossfades.Length; i++) {
				if (rightWristTwistCrossfades != null && i < rightWristTwistCrossfades.Length) {
					newCrossfades[i] = rightWristTwistCrossfades[i];
				} else {
					newCrossfades[i] = CalcFadeValue (rightWristTwistBones[i], rightHand);
				}
			}
			rightWristTwistCrossfades = newCrossfades;
		}

		public float CalcFadeValue (Transform armSegment, Transform hand) {
			if (armSegment == null)
				return 0;
			var startLength = Vector3.Distance (armSegment.parent.position, armSegment.position);
			var totalLength = startLength + Vector3.Distance (hand.position, armSegment.position);
			return Mathf.InverseLerp (0f, totalLength, startLength);
		}

		public void CalcWristCrossfades () {
			var leftArm = animator.GetBoneTransform (HumanBodyBones.LeftLowerArm);
			var leftHand = animator.GetBoneTransform (HumanBodyBones.LeftHand);
			var rightArm = animator.GetBoneTransform (HumanBodyBones.RightLowerArm);
			var rightHand = animator.GetBoneTransform (HumanBodyBones.RightHand);
			if (rightArm == null || leftArm == null || leftHand == null || rightHand == null)
				return;
			leftWristTwistCrossfades = new float[leftWristTwistBones.Length];
			rightWristTwistCrossfades = new float[rightWristTwistBones.Length];
			for (int i = 0; i < leftWristTwistCrossfades.Length; i++) {
				leftWristTwistCrossfades[i] = CalcFadeValue (leftWristTwistBones[i], leftHand);
			}
			for (int i = 0; i < rightWristTwistCrossfades.Length; i++) {
				rightWristTwistCrossfades[i] = CalcFadeValue (rightWristTwistBones[i], rightHand);
			}
		}

#endif
	}

#if UNITY_EDITOR

	/// <summary>
	/// Handles State's GUI, makes sure only one is active at a time
	/// </summary>
	[CustomEditor (typeof (AvatarModelReferences), true)]
	[CanEditMultipleObjects]
	[ExecuteInEditMode]
	public class AvatarModelReferencesEditor : Editor {
		public static string previewingFieldName = "";

		private static SerializedObject _so;

		public override void OnInspectorGUI () {
			AvatarModelReferences avatarModelReferences = (AvatarModelReferences) target;
			SerializedObject so = new SerializedObject (target);
			_so = so;

			if (previewingFieldName == null)
				previewingFieldName = "";
			string[] blendShapes = GetBlendShapeNames (avatarModelReferences.mesh != null ? avatarModelReferences.mesh.sharedMesh : null);

			if (avatarModelReferences.blendShapeRanges == null || avatarModelReferences.blendShapeRanges.Length == 0) {
				SetUpBlendShapeRanges (avatarModelReferences);
			}

			//check if additional meshes contain only matching blendshapenames
			//if they don't, add button to split it off into separate component

			GUILayout.Space (5);
			avatarModelReferences.disableVRModeInEditor = EditorGUILayout.Toggle (new GUIContent ("Disable VR Mode For Testing", "Disable VR mode for testing in the Unity editor"), avatarModelReferences.disableVRModeInEditor);
			GUILayout.Space (5);

			SerializedProperty characterName = so.FindProperty ("characterName");
			EditorGUILayout.PropertyField (characterName);

			SerializedProperty attribution = so.FindProperty ("attribution");
			EditorGUILayout.PropertyField (attribution);

			SerializedProperty thumbnail = so.FindProperty ("thumbnail");
			EditorGUILayout.PropertyField (thumbnail);

			SerializedProperty nameTagHeight = so.FindProperty ("nameTagHeight");
			EditorGUILayout.PropertyField (nameTagHeight);

			GUILayout.Space (10);

			avatarModelReferences.centerEye = EditorGUILayout.ObjectField (new GUIContent ("Center Eye", "The center eye transform, used to position the avatar's head"), avatarModelReferences.centerEye, typeof (Transform), true) as Transform;

			SerializedProperty eyes = so.FindProperty ("eyes");
			EditorGUILayout.PropertyField (eyes, true);
			if (avatarModelReferences.eyes == null || avatarModelReferences.eyes.Count == 0)
				if (GUILayout.Button ("Find Eyes"))
					avatarModelReferences.FindEyes ();

			avatarModelReferences.ignoreEyeTargets = EditorGUILayout.Toggle (new GUIContent ("Ignore Eye Targets", "Should this character's eyes ignore eye targets such as other characters?"), avatarModelReferences.ignoreEyeTargets);

			GUILayout.Space (10);
			SerializedProperty mesh = so.FindProperty ("mesh");

			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (mesh, true);
			if (EditorGUI.EndChangeCheck ()) {
				avatarModelReferences.CheckIfAdditionalMeshesFitMesh ();
			}
			if (GUILayout.Button ("Find Meshes with Blend Shapes"))
				avatarModelReferences.FindAdditionalMeshes ();

			if (avatarModelReferences.mesh != null) { //hide until first mesh is applied
				SerializedProperty additionalMeshes = so.FindProperty ("additionalMeshes");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (additionalMeshes, true);
				if (EditorGUI.EndChangeCheck ()) {
					avatarModelReferences.CheckIfAdditionalMeshesFitMesh ();
				}
			}

			SerializedProperty animator = so.FindProperty ("animator");
			EditorGUILayout.PropertyField (animator, true);

			SerializedProperty expressionType = so.FindProperty ("expressionType");
			EditorGUILayout.PropertyField (expressionType, true);

			GUILayout.Space (10);

			switch (avatarModelReferences.expressionType) {
				case AvatarModelReferences.ExpressionType.blendShapes:

					BasicBlendShapeEditor (avatarModelReferences, blendShapes);
					Vector2[] ranges = null;
					avatarModelReferences.aaahShape = BlendShapeSelector ("'AAAH'", avatarModelReferences.aaahShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.eeeShape = BlendShapeSelector ("'EEE'", avatarModelReferences.eeeShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.iiShape = BlendShapeSelector ("'II'", avatarModelReferences.iiShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.ohShape = BlendShapeSelector ("'OH'", avatarModelReferences.ohShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.fuhShape = BlendShapeSelector ("'FUH'", avatarModelReferences.fuhShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.mmmShape = BlendShapeSelector ("'MMM'", avatarModelReferences.mmmShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.luhShape = BlendShapeSelector ("'LUH'", avatarModelReferences.luhShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					avatarModelReferences.sssShape = BlendShapeSelector ("'SSS'", avatarModelReferences.sssShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

					break;

				case AvatarModelReferences.ExpressionType.simplifiedBlendShapes:

					BasicBlendShapeEditor (avatarModelReferences, blendShapes);
					ranges = null;
					avatarModelReferences.openMouthShape = BlendShapeSelector ("Open Mouth", avatarModelReferences.openMouthShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges, false, false, true, avatarModelReferences);

					GUILayout.BeginHorizontal ();
					GUILayout.Space (30);

					avatarModelReferences.openMouthThreshold = MinMaxSlider ("Open Mouth Threshold", avatarModelReferences.openMouthThreshold, 0, 1);

					GUILayout.EndHorizontal ();

					if (avatarModelReferences.jawBone == null) {
						//display "Find jaw bone" button that will try to find and assign a jaw bone to the avatar
						GUILayout.Space (10);
						GUILayout.BeginHorizontal ();

						EditorGUILayout.LabelField (new GUIContent ("Jaw Bone", "optional bone for opening and closing mouth"));
						avatarModelReferences.jawBone = EditorGUILayout.ObjectField (avatarModelReferences.jawBone, typeof (Transform), true) as Transform;
						if (GUILayout.Button ("Find Jawbone")) {
							avatarModelReferences.FindJawbone ();
						}

						GUILayout.EndHorizontal ();
					} else {
						//display jawBone without button and the two rotation values
						GUILayout.Space (10);
						EditorGUI.BeginChangeCheck ();
						GUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (new GUIContent ("Jaw Bone", "optional bone for opening and closing mouth"));
						avatarModelReferences.jawBone = EditorGUILayout.ObjectField (avatarModelReferences.jawBone, typeof (Transform), true) as Transform;
						GUILayout.EndHorizontal ();
						if (EditorGUI.EndChangeCheck ()) {
							avatarModelReferences.CalcJawRotations ();
						}

						avatarModelReferences.jawBoneClosedRotation = EditorGUILayout.Vector3Field (
							new GUIContent ("Closed Rotation ", "how the jaw should be rotated when fully closed"), avatarModelReferences.jawBoneClosedRotation, null);
						avatarModelReferences.jawBoneOpenRotation = EditorGUILayout.Vector3Field (
							new GUIContent ("Open Rotation ", "how the jaw should be rotated when fully open"), avatarModelReferences.jawBoneOpenRotation, null);
					}

					break;

				case AvatarModelReferences.ExpressionType.textures:
					avatarModelReferences.neutralTexture = (Texture) EditorGUILayout.ObjectField ("Neutral", avatarModelReferences.neutralTexture, typeof (Texture), false);
					avatarModelReferences.mmmTexture = (Texture) EditorGUILayout.ObjectField ("'MMM'", avatarModelReferences.mmmTexture, typeof (Texture), false);
					avatarModelReferences.fuhTexture = (Texture) EditorGUILayout.ObjectField ("'FUH'", avatarModelReferences.fuhTexture, typeof (Texture), false);
					avatarModelReferences.thTexture = (Texture) EditorGUILayout.ObjectField ("'TH'", avatarModelReferences.thTexture, typeof (Texture), false);
					avatarModelReferences.ddTexture = (Texture) EditorGUILayout.ObjectField ("'DD'", avatarModelReferences.ddTexture, typeof (Texture), false);
					avatarModelReferences.kkTexture = (Texture) EditorGUILayout.ObjectField ("'KK'", avatarModelReferences.kkTexture, typeof (Texture), false);
					avatarModelReferences.chTexture = (Texture) EditorGUILayout.ObjectField ("'CH'", avatarModelReferences.chTexture, typeof (Texture), false);
					avatarModelReferences.sssTexture = (Texture) EditorGUILayout.ObjectField ("'SSS'", avatarModelReferences.sssTexture, typeof (Texture), false);
					avatarModelReferences.nnnTexture = (Texture) EditorGUILayout.ObjectField ("'NNN'", avatarModelReferences.nnnTexture, typeof (Texture), false);
					avatarModelReferences.rrrTexture = (Texture) EditorGUILayout.ObjectField ("'RRR'", avatarModelReferences.rrrTexture, typeof (Texture), false);
					avatarModelReferences.aaahTexture = (Texture) EditorGUILayout.ObjectField ("'AAAH'", avatarModelReferences.aaahTexture, typeof (Texture), false);
					avatarModelReferences.eeeTexture = (Texture) EditorGUILayout.ObjectField ("'EEE'", avatarModelReferences.eeeTexture, typeof (Texture), false);
					avatarModelReferences.iiTexture = (Texture) EditorGUILayout.ObjectField ("'II'", avatarModelReferences.iiTexture, typeof (Texture), false);
					avatarModelReferences.ohTexture = (Texture) EditorGUILayout.ObjectField ("'OH'", avatarModelReferences.ohTexture, typeof (Texture), false);
					avatarModelReferences.ooohTexture = (Texture) EditorGUILayout.ObjectField ("'OOOH'", avatarModelReferences.ooohTexture, typeof (Texture), false);
					break;

				case AvatarModelReferences.ExpressionType.animation:
					avatarModelReferences.expressionAnimator = (Animator) EditorGUILayout.ObjectField ("Expression Animator", avatarModelReferences.expressionAnimator, typeof (Animator), true);
					if (avatarModelReferences.expressionAnimator == null && GUILayout.Button ("Setup Animator")) {
						SetupAnimatorExpressions (avatarModelReferences);
					}
					break;

				case AvatarModelReferences.ExpressionType.comboTextures:
					EditorGUILayout.LabelField ("Add a FacialExpressionReference component to handle the combotextures instead");
					break;

				case AvatarModelReferences.ExpressionType.animationParameters:
					EditorGUILayout.LabelField ("Add a FacialExpressionReference component to handle the animation parameters instead");
					break;
			}

			GUILayout.Space (5);

			if (GUILayout.Button ("Fix References")) {
				avatarModelReferences.FindEmptyReferences ();
			}

			if (avatarModelReferences.mesh == null && avatarModelReferences.expressionType == AvatarModelReferences.ExpressionType.blendShapes) {
				EditorGUILayout.TextArea ("Select a SkinnedMeshRenderer to setup your blend shapes.");
			}

			GUILayout.Space (5);

			GUILayout.Label ("Avatar Bent Elbow", EditorStyles.boldLabel);
			GUILayout.BeginVertical (EditorStyles.helpBox);
			GUILayout.Label ("Elbow can be configured to have bend between 0-15% for (X / Y / Z) ");
			GUILayout.Label ("0.0 = No bend when physical arms fully extended in given direction ");
			GUILayout.Label ("0.15 = Bent by 15% when physical arms stretched fully in given direction");
			GUILayout.EndVertical ();
			avatarModelReferences.bentElbowScale.x = MinMaxSlider ("Bent Elbow Scale X", avatarModelReferences.bentElbowScale.x, 0.0f, 0.15f);
			avatarModelReferences.bentElbowScale.y = MinMaxSlider ("Bent Elbow Scale Y", avatarModelReferences.bentElbowScale.y, 0.0f, 0.15f);
			avatarModelReferences.bentElbowScale.z = MinMaxSlider ("Bent Elbow Scale Z", avatarModelReferences.bentElbowScale.z, 0.0f, 0.15f);

			GUILayout.Space (5);

			DrawWristSettings (avatarModelReferences, so);

			DrawEvents (avatarModelReferences, so);

			DrawResourceUsage (avatarModelReferences, so);

			DrawDebugWindow (avatarModelReferences, so);

			so.ApplyModifiedProperties ();
		}

		private static void SetUpBlendShapeRanges (AvatarModelReferences avatarModelReferences) {
			int len = (int) AvatarModelReferences.BlendShapeIndices.sss;

			avatarModelReferences.blendShapeRanges = new AvatarModelReferences.BlendShapeRangeList[len];

			for (int i = 0; i < len; i++) {
				avatarModelReferences.blendShapeRanges[i] = new AvatarModelReferences.BlendShapeRangeList (1);
				avatarModelReferences.blendShapeRanges[i].a[0] = new Vector2 (0, 100);
			}

			_so.ApplyModifiedProperties ();
		}

		private void SetupAnimatorExpressions (AvatarModelReferences avatarModelReferences) {
			GameObject head = avatarModelReferences.animator.GetBoneTransform (HumanBodyBones.Head).gameObject;

			string newExpressionPath = SceneManager.GetActiveScene ().path;

			newExpressionPath = newExpressionPath.Substring (0, newExpressionPath.LastIndexOf ("/"));
			newExpressionPath += "/" + newExpressionPath.Substring (newExpressionPath.LastIndexOf ("/") + 1) + "ExpressionAnimator.controller";

			AssetDatabase.CopyAsset ("Assets/FlipsideCreatorTools/Resources/expressionAnimator.controller", newExpressionPath);

			Animator expressionAnimator = head.GetComponent<Animator> ();
			if (expressionAnimator == null) {
				expressionAnimator = head.AddComponent<Animator> ();
			}
			expressionAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController> (newExpressionPath);

			avatarModelReferences.expressionAnimator = expressionAnimator;

			FixNaming (expressionAnimator.transform);
		}

		private static void FixNaming (Transform parentTransform) {
			Debug.Log ("Fixing names");
			int children = parentTransform.childCount;
			for (int i = 0; i < children; ++i) {
				Transform child = parentTransform.GetChild (i);
				if (child.name.Contains ("/")) {
					child.name = child.name.Replace ('/', '-');
				}
				Debug.Log ("Fixing name to " + i);

				FixNaming (child);
			}
		}

		public static void BlendShapeSelectorPlusThreshold (string[] blendShapes, string name, ref string shape, ref float threshold, Func<Viseme[], Vector2[], string> PreviewButton) {
			Vector2[] ranges = null;
			shape = BlendShapeSelector (name, shape, blendShapes, PreviewButton, ref ranges);
			GUILayout.BeginHorizontal ();
			GUILayout.Space (30);
			threshold = MinMaxSlider (string.Format ("{0} Threshold", name), threshold, 0, 1);
			GUILayout.EndHorizontal ();
		}

		public void BasicBlendShapeEditor (AvatarModelReferences avatarModelReferences, string[] blendShapes) {
			BlendShapeSelectorPlusThreshold (blendShapes, "Happy", ref avatarModelReferences.happyShape, ref avatarModelReferences.happyThreshold, avatarModelReferences.PreviewButton);
			BlendShapeSelectorPlusThreshold (blendShapes, "Sad", ref avatarModelReferences.sadShape, ref avatarModelReferences.sadThreshold, avatarModelReferences.PreviewButton);
			BlendShapeSelectorPlusThreshold (blendShapes, "Surprised", ref avatarModelReferences.surprisedShape, ref avatarModelReferences.surprisedThreshold, avatarModelReferences.PreviewButton);
			BlendShapeSelectorPlusThreshold (blendShapes, "Angry", ref avatarModelReferences.angryShape, ref avatarModelReferences.angryThreshold, avatarModelReferences.PreviewButton);

			Vector2[] ranges = null;
			avatarModelReferences.blinkLeftShape = BlendShapeSelector ("Blink Left", avatarModelReferences.blinkLeftShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);
			avatarModelReferences.blinkRightShape = BlendShapeSelector ("Blink Right", avatarModelReferences.blinkRightShape, blendShapes, avatarModelReferences.PreviewButton, ref ranges);

			if (avatarModelReferences.blendShapeRanges[(int) AvatarModelReferences.BlendShapeIndices.blinkAll] == null) {
				SetUpBlendShapeRanges (avatarModelReferences);
			}
			avatarModelReferences.blinkAllShape = BlendShapeSelector ("Blink All", avatarModelReferences.blinkAllShape, blendShapes, avatarModelReferences.PreviewButton, ref avatarModelReferences.blendShapeRanges[(int) AvatarModelReferences.BlendShapeIndices.blinkAll].a, true, false, false, avatarModelReferences);
		}

		public static float MinMaxSlider (string fieldName, float value, float min, float max) {
			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (fieldName);
			float input = EditorGUILayout.Slider (value, min, max);
			GUILayout.EndHorizontal ();
			return input;
		}

		public static Vector2 MinMaxSlider (string fieldName, Vector2 value, float min, float max, bool showFirstSlider = true, bool showSecondSlider = true) {
			GUILayout.BeginHorizontal ();
			GUILayout.Space (30);
			EditorGUILayout.LabelField (fieldName, GUILayout.Width (90));
			Vector2 input = new Vector2 (0f, 100f);
			if (showFirstSlider) input.x = EditorGUILayout.Slider (value.x, min, max);
			if (showSecondSlider) input.y = EditorGUILayout.Slider (value.y, min, max);
			GUILayout.EndHorizontal ();
			return input;
		}

		private static void OpenJawbone (AvatarModelReferences avatarModelReferences) {
			if (avatarModelReferences.jawBone == null) return;
			avatarModelReferences.jawBone.transform.localEulerAngles = avatarModelReferences.jawBoneOpenRotation;
		}

		private static void OpenJawbone (FacialExpressionReference facialExpressionReference) {
			if (facialExpressionReference.jawBone == null) return;
			facialExpressionReference.jawBone.transform.localEulerAngles = facialExpressionReference.jawBoneOpenRotation;
		}

		private static void ResetJawbone (AvatarModelReferences avatarModelReferences) {
			if (avatarModelReferences.jawBone == null) return;
			avatarModelReferences.jawBone.transform.localEulerAngles = avatarModelReferences.jawBoneClosedRotation;
		}

		private static void ResetJawbone (FacialExpressionReference facialExpressionReference) {
			if (facialExpressionReference.jawBone == null) return;
			facialExpressionReference.jawBone.transform.localEulerAngles = facialExpressionReference.jawBoneClosedRotation;
		}

		public static string BlendShapeSelector (string fieldName, string selected, string[] blendShapeNames, Func<Viseme[], Vector2[], string> PreviewButton, ref Vector2[] ranges, bool showFirstSlider = true, bool showSecondSlider = true, bool controlJawBone = false, AvatarModelReferences avatarRefs = null, FacialExpressionReference faceRefs = null) {
			var visemes = GetMaskFromString (selected);

			GUILayout.Space (15);

			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (fieldName);

			Texture2D previewTex = Resources.Load<Texture2D> ("icon-preview");

			if (fieldName == previewingFieldName) {
				previewingFieldName = PreviewButton (visemes, ranges);
				if (controlJawBone && fieldName != previewingFieldName) {
					if (avatarRefs != null) ResetJawbone (avatarRefs);
					if (faceRefs != null) ResetJawbone (faceRefs);
				}
			} else {
				if (GUILayout.Button (previewTex, GUILayout.Width (26), GUILayout.Height (20))) {
					previewingFieldName = fieldName;
					if (controlJawBone) {
						if (avatarRefs != null) OpenJawbone (avatarRefs);
						if (faceRefs != null) OpenJawbone (faceRefs);
					}
				}
			}
			GUILayout.EndHorizontal ();

			for (int i = 0; i < visemes.Length; i++) {
				GUILayout.BeginHorizontal ();
				GUILayout.Space (30);
				visemes[i].blendShape = EditorGUILayout.Popup (visemes[i].blendShape, blendShapeNames);
				if (GUILayout.Button ("Delete")) {
					visemes[i].blendShape = -1;
				}
				GUILayout.EndHorizontal ();
				if (ranges != null) {
					if (ranges.Length <= i) {
						Array.Resize (ref ranges, visemes.Length);
						ranges[i] = new Vector2 (0, 100);
					}
					ranges[i] = MinMaxSlider ("Resting Value", ranges[i], 0, 100, showFirstSlider, showSecondSlider);

					if (fieldName != previewingFieldName) {
						if (avatarRefs != null) {
							if (avatarRefs.mesh != null) avatarRefs.mesh.SetBlendShapeWeight (visemes[i].blendShape, ranges[i][0]);
							if (avatarRefs.additionalMeshes != null) {
								foreach (var aMesh in avatarRefs.additionalMeshes) {
									if (aMesh != null) aMesh.SetBlendShapeWeight (visemes[i].blendShape, ranges[i][0]);
								}
							}
						}
						if (faceRefs != null) {
							foreach (var aMesh in faceRefs.meshes) {
								if (aMesh != null) aMesh.SetBlendShapeWeight (visemes[i].blendShape, ranges[i][0]);
							}
						}
					}
				}
			}

			GUILayout.BeginHorizontal ();
			GUILayout.Space (30);
			if (GUILayout.Button ("Add Blend Shape")) {
				Array.Resize (ref visemes, visemes.Length + 1);
				visemes[visemes.Length - 1] = new Viseme (0, 100);
				if (ranges != null) {
					Array.Resize (ref ranges, visemes.Length);
					ranges[ranges.Length - 1] = new Vector2 (0, 100);
				}
			}
			GUILayout.EndHorizontal ();
			return GetStringFromMask (visemes);
		}

		public static string FloatTextField (string fieldName, string value, Animator animator, FacialExpressionReference references) {
			return FloatTextField (fieldName, value, animator, references.ClearBlendShapes);
		}

		public static string FloatTextField (string fieldName, string value, Animator animator, AvatarModelReferences references) {
			return FloatTextField (fieldName, value, animator, references.ClearBlendShapes);
		}

		public static string FloatTextField (string fieldName, string value, Animator animator, Action ClearBlendShapes) {
			//string[] blendShapeNames, Func<Viseme[], Vector2[], string> PreviewButton, ref Vector2[] ranges, bool showFirstSlider = true, bool showSecondSlider = true, bool controlJawBone = false, AvatarModelReferences avatarRefs = null, FacialExpressionReference faceRefs = null) {
			//GUILayout.Space (5);
			var horizontalStyle = new GUIStyle ();
			horizontalStyle.fixedWidth = Screen.width * 0.2f;
			horizontalStyle.stretchWidth = false;
			horizontalStyle.padding = new RectOffset (0, 0, 0, 0);
			GUILayout.BeginHorizontal (horizontalStyle);
			//grey out if string is empty
			bool valueFound = false;
			GUIStyle labelStyle = new GUIStyle (EditorStyles.label);
			GUIStyle fieldStyle = new GUIStyle (EditorStyles.textField);

			if (value.Length == 0) {
				valueFound = true; //shouldn't bother showing red background if string is empty
				labelStyle.normal.textColor = Color.grey;
			} else {
				//check if animator contains a parameter of this name
				if (animator.parameters.Length == 0) {
					animator.Rebind ();
				}
				foreach (var par in animator.parameters) {
					if (par.name == value) {
						valueFound = true;
						break;
					}
				}
			}

			if (!valueFound)
				GUI.backgroundColor = new Color (1f, 0.5f, 0.5f); //flag it as no match

			EditorGUILayout.LabelField (fieldName, labelStyle, GUILayout.Width (Screen.width / 3f - 10));
			value = GUILayout.TextField (value, fieldStyle, GUILayout.Width (Screen.width / 3f - 10));

			//show preview button
			if (value.Length > 0 && valueFound) {
				Texture2D previewTex = Resources.Load<Texture2D> ("icon-preview");

				if (fieldName == previewingFieldName) {
					//previewingFieldName = PreviewButton (visemes, ranges);
					GUI.backgroundColor = new Color (0.23f, 0.35f, 1f);
					if (GUILayout.Button (previewTex, GUILayout.Width (26), GUILayout.Height (20))) {
						AvatarModelReferencesEditor.previewingFieldName = "";
						ClearBlendShapes ();
					}
					GUI.backgroundColor = Color.white;
				} else {
					if (GUILayout.Button (previewTex, GUILayout.Width (26), GUILayout.Height (20))) {
						previewingFieldName = fieldName;
					}
				}
			}

			GUI.backgroundColor = Color.white;

			GUILayout.EndHorizontal ();
			return value;
		}

		public static Viseme[] GetMaskFromString (string visemeText) {
			if (visemeText == null || visemeText == "-1" || visemeText == "") {
				return new Viseme[0];
			}

			string[] selectedBlendShapes = visemeText.Split (',');

			Viseme[] visemes = new Viseme[selectedBlendShapes.Length];

			for (int i = 0; i < visemes.Length; i++) {
				if (Int32.TryParse (selectedBlendShapes[i], out var result)) {
					visemes[i] = new Viseme (result, 100);
				} else {
					visemes[i] = new Viseme (0, 0);
				}
			}

			return visemes;
		}

		public static string GetStringFromMask (Viseme[] visemes) {
			if (visemes == null || visemes.Length == 0) {
				return "-1";
			}
			string result = "";

			for (int i = 0; i < visemes.Length; i++) {
				if (visemes[i].blendShape != -1) {
					if (result != "") result += ",";
					result += visemes[i].blendShape;
				}
			}
			return result;
		}

		internal static string[] GetBlendShapeNames (Mesh m) {
			string[] arr;
			if (m == null)
				return new string[] { };
			arr = new string[m.blendShapeCount];
			for (int i = 0; i < m.blendShapeCount; i++) {
				string s = m.GetBlendShapeName (i);
				arr[i] = s;
			}
			return arr;
		}

		private void DrawWristSettings (AvatarModelReferences component, SerializedObject so) {
			GUILayout.Space (5);
			GUILayout.Label ("Wrist Settings", EditorStyles.boldLabel);

			//show bones
			SerializedProperty leftBones = so.FindProperty ("leftWristTwistBones");
			SerializedProperty rightBones = so.FindProperty ("rightWristTwistBones");

			if (rightBones.arraySize == 0 && leftBones.arraySize == 0) {
				//if no entries
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (leftBones, true);
				EditorGUILayout.PropertyField (rightBones, true);
				if (EditorGUI.EndChangeCheck ()) {
					if (leftBones.arraySize > 0) {
						so.ApplyModifiedProperties ();
						component.CheckLeftCrossFades ();
					}
					if (rightBones.arraySize > 0) {
						so.ApplyModifiedProperties ();
						component.CheckRightCrossFades ();
					}
				}
				if (GUILayout.Button ("Find Wrist Bones")) {
					//search for bones
					component.FindWristBones ();
				}
			} else {
				EditorGUI.BeginChangeCheck ();
				SerializedProperty leftCrossfades = so.FindProperty ("leftWristTwistCrossfades");
				SerializedProperty rightCrossfades = so.FindProperty ("rightWristTwistCrossfades");
				EditorGUILayout.PropertyField (leftBones, true);
				EditorGUILayout.PropertyField (leftCrossfades, true);
				EditorGUILayout.PropertyField (rightBones, true);
				EditorGUILayout.PropertyField (rightCrossfades, true);
				if (EditorGUI.EndChangeCheck ()) {
					if (leftBones.arraySize > leftCrossfades.arraySize) {
						so.ApplyModifiedProperties ();
						component.CheckLeftCrossFades ();
					}
					if (rightBones.arraySize > rightCrossfades.arraySize) {
						so.ApplyModifiedProperties ();
						component.CheckRightCrossFades ();
					}
				}
				if (GUILayout.Button ("Calculate Cross Fades")) {
					component.CalcWristCrossfades ();
				}
			}

			/*left
			public Transform[] ;
		public float[] leftWristTwistCrossfades;

		public Transform[] rightWristTwistBones;
		public float[] rightWristTwistCrossfades;*/
			//find bones button if not yet assigned
		}

		private void DrawEvents (AvatarModelReferences component, SerializedObject so) {
			GUILayout.Space (5);
			GUILayout.Label ("Event Handlers", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField (so.FindProperty ("OnSitting"));
			EditorGUILayout.PropertyField (so.FindProperty ("OnStanding"));
		}

		private bool resourcesVisible = true;

		private void DrawResourceUsage (AvatarModelReferences component, SerializedObject so) {
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
			GUILayout.Label ("Skinned Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.skinnedMeshCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Windows.skinnedMeshCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Android.skinnedMeshCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.meshCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Windows.meshCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Android.meshCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Props", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.propCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Windows.propCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Android.propCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Lights", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.lightCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Windows.lightCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Android.lightCount.ToString ("##,#0"), rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.Space (6);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Vertices", leftHeader, col1Opts);
			GUILayout.EndHorizontal ();

			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Skinned Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.skinnedMeshVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
			GUILayout.Label ("-", rightAlign, col3Opts);
			GUILayout.Label ("-", rightAlign, col4Opts);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Meshes", leftAlign, col1Opts);
			GUILayout.Label (component.resourceUsage.meshVertexCount.ToString ("##,#0"), rightAlign, col2Opts);
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
			GUILayout.Label (ResourceUsageLimitAvatars.Windows.meshVertexCount.ToString ("##,#0"), rightAlign, col3Opts);
			GUILayout.Label (ResourceUsageLimitAvatars.Android.meshVertexCount.ToString ("##,#0"), rightAlign, col4Opts);
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

		private bool debugVisible = false;

		private void DrawDebugWindow (AvatarModelReferences component, SerializedObject so) {
			GUILayout.Space (5);

			debugVisible = EditorGUILayout.Foldout (debugVisible, "Debug Info");

			if (!debugVisible) return;

			GUILayout.BeginVertical (EditorStyles.helpBox);

			GUILayout.Label ("Blend Shape Ranges", EditorStyles.boldLabel);

			for (int i = 0; i < component.blendShapeRanges.Length; i++) {
				var ranges = component.blendShapeRanges[i];
				if (ranges == null) {
					GUILayout.Label (string.Format ("Expression {0} is null", i));
					continue;
				}
				if (ranges.a == null) {
					GUILayout.Label (string.Format ("Expression {0} list is null", i));
					continue;
				}
				for (int x = 0; x < ranges.a.Length; x++) {
					if (ranges.a[x] == null) {
						GUILayout.Label (string.Format ("Expression {0}.{1} is null", i, x));
						continue;
					}
					GUILayout.Label (string.Format ("Expression {0}.{1}: ({2}, {3})", i, x, ranges.a[x].x, ranges.a[x].y));
				}
			}

			GUILayout.Space (3);

			GUILayout.EndVertical ();
		}
	}

#endif
}