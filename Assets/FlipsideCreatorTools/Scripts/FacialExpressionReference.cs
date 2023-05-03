/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
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

#if UNITY_EDITOR

using UnityEditor;
using Flipside.Helpers;

#endif

namespace Flipside.Avatars {

	/// <summary>
	/// Additional facial expressions that don't fit into AvatarModelReferences
	/// </summary>
	public class FacialExpressionReference : MonoBehaviour {
		public SkinnedMeshRenderer[] meshes;

		public MeshRenderer[] renderers;

		public Animator expressionAnimator;

		[Space (10)]
		public AvatarModelReferences.ExpressionType expressionType = AvatarModelReferences.ExpressionType.blendShapes;

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
		public Texture[] arbitraryTextures;

		public string[] animatorParameters;

		[HideInInspector]
		public bool includeBlinkOptions = false;

		[HideInInspector]
		public Transform jawBone;

		[HideInInspector]
		public Vector3 jawBoneClosedRotation;

		[HideInInspector]
		public Vector3 jawBoneOpenRotation;

		public enum BlendShapeIndices {
			neutral, happy, sad, surprised, angry,
			blinkLeft, blinkRight, blinkAll,
			aaah, eee, ii, oh, oooh, fuh, mmm, luh, sss,
		}

		public AvatarModelReferences.BlendShapeRangeList[] blendShapeRanges;

		public void ClearBlendShapes () {
			if (meshes == null || meshes.Length == 0)
				return;
			if (meshes[0] == null)
				return;
			switch (expressionType) {
				case AvatarModelReferences.ExpressionType.blendShapes:
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

				case AvatarModelReferences.ExpressionType.simplifiedBlendShapes:
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
		}

		private void ClearBlendShapeList (string list, BlendShapeIndices rangeIndex) {
			ClearBlendShapeList (list, (int) rangeIndex);
		}

		private Vector2 GetRange (int rangeIndex, int numbIndex) {
			if (blendShapeRanges == null)
				return new Vector2 (0, 100);
			if (blendShapeRanges.Length <= rangeIndex) {
				System.Array.Resize (ref blendShapeRanges, rangeIndex + 1);
				blendShapeRanges[rangeIndex] = new AvatarModelReferences.BlendShapeRangeList (numbIndex + 1);
				blendShapeRanges[rangeIndex].a[numbIndex] = new Vector2 (0, 100);
			}
			if (blendShapeRanges[rangeIndex] == null)
				blendShapeRanges[rangeIndex] = new AvatarModelReferences.BlendShapeRangeList (1);
			if (blendShapeRanges[rangeIndex].a == null) {
				blendShapeRanges[rangeIndex].a = new Vector2[1];
				blendShapeRanges[rangeIndex].a[0] = new Vector2 (0, 100);
			}
			if (blendShapeRanges[rangeIndex].a.Length <= numbIndex) {
				return new Vector2 (0, 100);
			}
			return blendShapeRanges[rangeIndex].a[numbIndex];
		}

		public AvatarModelReferences.BlendShapeRangeList GetRangeList (int rangeIndex) {
			if (blendShapeRanges == null)
				return null;
			while (blendShapeRanges.Length <= rangeIndex) {
				System.Array.Resize (ref blendShapeRanges, blendShapeRanges.Length + 1);
				blendShapeRanges[blendShapeRanges.Length - 1] = new AvatarModelReferences.BlendShapeRangeList (0);
			}
			if (blendShapeRanges[rangeIndex] == null)
				blendShapeRanges[rangeIndex] = new AvatarModelReferences.BlendShapeRangeList (0);
			if (blendShapeRanges[rangeIndex].a == null) {
				blendShapeRanges[rangeIndex].a = new Vector2[0];
			}
			return blendShapeRanges[rangeIndex];
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

		private void ClearSingleBlendShape (int blendShapeIndex, float newValue) {
			if (blendShapeIndex == -1)
				return;
			if (meshes != null) {
				foreach (var aMesh in meshes) {
					aMesh.SetBlendShapeWeight (blendShapeIndex, newValue);
				}
			}
		}

		public void ClearBlendShapesViaModel () {
			var avaModRef = GetComponent<AvatarModelReferences> ();
			if (avaModRef != null)
				avaModRef.ClearBlendShapes ();
		}

#if UNITY_EDITOR

		private Flipside.Helpers.PlayerController player;

		private void Update () {
			if (player == null) {
				AvatarModelReferences avr = GetComponent<AvatarModelReferences> ();
				if (avr == null) return;
				player = avr.GetPlayerController ();
			}

			Vector2 axisValue = player.leftHand.GetPrimaryAxisValue ();
			if (happyShape != "" && axisValue.y >= 0f) {
				var numbers = happyShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						foreach (var aMesh in meshes) {
							aMesh.SetBlendShapeWeight (index, axisValue.y * 100 * happyThreshold);
						}
						ClearBlendShapeList (sadShape, BlendShapeIndices.sad);
					}
				}
			} else if (sadShape != "") {
				var numbers = sadShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						foreach (var aMesh in meshes) {
							aMesh.SetBlendShapeWeight (index, axisValue.y * -100 * sadThreshold);
						}
						ClearBlendShapeList (happyShape, BlendShapeIndices.happy);
					}
				}
			}

			if (angryShape != "" && axisValue.x >= 0f) {
				var numbers = angryShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						foreach (var aMesh in meshes) {
							aMesh.SetBlendShapeWeight (index, axisValue.x * 100 * angryThreshold);
						}
						ClearBlendShapeList (surprisedShape, BlendShapeIndices.surprised);
					}
				}
			} else if (surprisedShape != "") {
				var numbers = surprisedShape.Split (',');
				foreach (var numb in numbers) {
					int index = -1;
					if (int.TryParse (numb, out index) && index != -1) {
						foreach (var aMesh in meshes) {
							aMesh.SetBlendShapeWeight (index, axisValue.x * -100 * surprisedThreshold);
						}
						ClearBlendShapeList (angryShape, BlendShapeIndices.angry);
					}
				}
			}
		}

		public string PreviewButton (Viseme[] visemes, Vector2[] ranges) {
			foreach (var viseme in visemes) {
				foreach (var aMesh in meshes) {
					aMesh.SetBlendShapeWeight (viseme.blendShape, viseme.weight);
				}
			}

			GUI.backgroundColor = new Color (0.23f, 0.35f, 1f);

			Texture2D previewTex = Resources.Load<Texture2D> ("icon-preview");
			if (GUILayout.Button (previewTex, GUILayout.Width (26), GUILayout.Height (20))) {
				AvatarModelReferencesEditor.previewingFieldName = "";
				ClearBlendShapesViaModel ();
			}

			GUI.backgroundColor = Color.white;
			return AvatarModelReferencesEditor.previewingFieldName;
		}

#endif
	}

#if UNITY_EDITOR

	/// <summary>
	///
	/// </summary>
	[CustomEditor (typeof (FacialExpressionReference), true)]
	[CanEditMultipleObjects]
	[ExecuteInEditMode]
	public class FacialExpressionReferenceEditor : Editor {

		public override void OnInspectorGUI () {
			FacialExpressionReference references = (FacialExpressionReference) target;

			if (references.blendShapeRanges == null || references.blendShapeRanges.Length == 0) {
				SetUpBlendShapeRanges (references);
			}

			SerializedObject so = new SerializedObject (target);
			if ((references.meshes == null || references.meshes.Length == 0) &&
				(references.renderers == null || references.renderers.Length == 0) &&
				(references.expressionAnimator == null)) {
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("Please assign a renderer to one of the lists below.");
				GUILayout.Space (10);
				SerializedProperty _meshes = so.FindProperty ("meshes");
				EditorGUILayout.PropertyField (_meshes, new GUIContent ("Skinned Mesh Renderers"), true);
				SerializedProperty _renderers = so.FindProperty ("renderers");
				EditorGUILayout.PropertyField (_renderers, new GUIContent ("Mesh Renderers"), true);
				SerializedProperty _animator = so.FindProperty ("expressionAnimator");
				EditorGUILayout.PropertyField (_animator, true);
				so.ApplyModifiedProperties ();
				return;
			}
			string[] blendShapes = new string[0];
			if (references.meshes.Length > 0)
				blendShapes = AvatarModelReferencesEditor.GetBlendShapeNames (references.meshes[0] != null ? references.meshes[0].sharedMesh : null);

			SerializedProperty meshes = so.FindProperty ("meshes");
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (meshes, new GUIContent ("Skinned Mesh Renderers"), true);
			if (EditorGUI.EndChangeCheck ()) {
			}

			SerializedProperty renderers = so.FindProperty ("renderers");
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (renderers, new GUIContent ("Mesh Renderers"), true);
			if (EditorGUI.EndChangeCheck ()) {
			}

			SerializedProperty expressionType = so.FindProperty ("expressionType");
			EditorGUILayout.PropertyField (expressionType, true);

			//switch based on expressionType
			so.ApplyModifiedProperties ();

			GUILayout.Space (10);

			switch (references.expressionType) {
				case AvatarModelReferences.ExpressionType.blendShapes:
					BasicBlendShapeEditor (references, blendShapes);
					Vector2[] ranges = null;

					references.aaahShape = AvatarModelReferencesEditor.BlendShapeSelector ("'AAAH'", references.aaahShape, blendShapes, references.PreviewButton, ref ranges);

					references.eeeShape = AvatarModelReferencesEditor.BlendShapeSelector ("'EEE'", references.eeeShape, blendShapes, references.PreviewButton, ref ranges);

					references.iiShape = AvatarModelReferencesEditor.BlendShapeSelector ("'II'", references.iiShape, blendShapes, references.PreviewButton, ref ranges);

					references.ohShape = AvatarModelReferencesEditor.BlendShapeSelector ("'OH'", references.ohShape, blendShapes, references.PreviewButton, ref ranges);

					references.fuhShape = AvatarModelReferencesEditor.BlendShapeSelector ("'FUH'", references.fuhShape, blendShapes, references.PreviewButton, ref ranges);

					references.mmmShape = AvatarModelReferencesEditor.BlendShapeSelector ("'MMM'", references.mmmShape, blendShapes, references.PreviewButton, ref ranges);

					references.luhShape = AvatarModelReferencesEditor.BlendShapeSelector ("'LUH'", references.luhShape, blendShapes, references.PreviewButton, ref ranges);

					references.sssShape = AvatarModelReferencesEditor.BlendShapeSelector ("'SSS'", references.sssShape, blendShapes, references.PreviewButton, ref ranges);

					break;

				case AvatarModelReferences.ExpressionType.simplifiedBlendShapes:

					BasicBlendShapeEditor (references, blendShapes);

					ranges = null;
					references.openMouthShape = AvatarModelReferencesEditor.BlendShapeSelector ("Open Mouth", references.openMouthShape, blendShapes, references.PreviewButton, ref ranges, false, false, true, null, references);

					GUILayout.BeginHorizontal ();
					GUILayout.Space (30);
					references.openMouthThreshold = AvatarModelReferencesEditor.MinMaxSlider ("Open Mouth Threshold", references.openMouthThreshold, 0, 1);
					GUILayout.EndHorizontal ();

					break;

				case AvatarModelReferences.ExpressionType.textures:

					references.neutralTexture = (Texture) EditorGUILayout.ObjectField ("Neutral", references.neutralTexture, typeof (Texture), false);
					references.mmmTexture = (Texture) EditorGUILayout.ObjectField ("'MMM'", references.mmmTexture, typeof (Texture), false);
					references.fuhTexture = (Texture) EditorGUILayout.ObjectField ("'FUH'", references.fuhTexture, typeof (Texture), false);
					references.thTexture = (Texture) EditorGUILayout.ObjectField ("'TH'", references.thTexture, typeof (Texture), false);
					references.ddTexture = (Texture) EditorGUILayout.ObjectField ("'DD'", references.ddTexture, typeof (Texture), false);
					references.kkTexture = (Texture) EditorGUILayout.ObjectField ("'KK'", references.kkTexture, typeof (Texture), false);
					references.chTexture = (Texture) EditorGUILayout.ObjectField ("'CH'", references.chTexture, typeof (Texture), false);
					references.sssTexture = (Texture) EditorGUILayout.ObjectField ("'SSS'", references.sssTexture, typeof (Texture), false);
					references.nnnTexture = (Texture) EditorGUILayout.ObjectField ("'NNN'", references.nnnTexture, typeof (Texture), false);
					references.rrrTexture = (Texture) EditorGUILayout.ObjectField ("'RRR'", references.rrrTexture, typeof (Texture), false);
					references.aaahTexture = (Texture) EditorGUILayout.ObjectField ("'AAAH'", references.aaahTexture, typeof (Texture), false);
					references.eeeTexture = (Texture) EditorGUILayout.ObjectField ("'EEE'", references.eeeTexture, typeof (Texture), false);
					references.iiTexture = (Texture) EditorGUILayout.ObjectField ("'II'", references.iiTexture, typeof (Texture), false);
					references.ohTexture = (Texture) EditorGUILayout.ObjectField ("'OH'", references.ohTexture, typeof (Texture), false);
					references.ooohTexture = (Texture) EditorGUILayout.ObjectField ("'OOOH'", references.ooohTexture, typeof (Texture), false);

					so.ApplyModifiedProperties ();
					break;

				case AvatarModelReferences.ExpressionType.animation:
					EditorGUILayout.LabelField ("Animation-based is not yet fully supported");
					/*references.expressionAnimator = (Animator) EditorGUILayout.ObjectField ("Expression Animator", references.expressionAnimator, typeof (Animator), true);
					if (references.expressionAnimator == null && GUILayout.Button ("Setup Animator")) {
						SetupAnimatorExpressions (references);
					}*/
					break;

				case AvatarModelReferences.ExpressionType.comboTextures:

					var _blinking = so.FindProperty ("includeBlinkOptions");
					_blinking.boolValue = EditorGUILayout.Toggle ("Use blinking textures", _blinking.boolValue);
					if (_blinking.boolValue == true) {
						references.neutralTexture = (Texture) EditorGUILayout.ObjectField ("Neutral, Closed Mouth", references.neutralTexture, typeof (Texture), false);
						references.mmmTexture = (Texture) EditorGUILayout.ObjectField ("Neutral, Open Mouth", references.mmmTexture, typeof (Texture), false);
						references.thTexture = (Texture) EditorGUILayout.ObjectField ("Happy, Closed Mouth", references.thTexture, typeof (Texture), false);
						references.fuhTexture = (Texture) EditorGUILayout.ObjectField ("Happy, Open Mouth", references.fuhTexture, typeof (Texture), false);
						references.kkTexture = (Texture) EditorGUILayout.ObjectField ("Sad, Closed Mouth", references.kkTexture, typeof (Texture), false);
						references.ddTexture = (Texture) EditorGUILayout.ObjectField ("Sad, Open Mouth", references.ddTexture, typeof (Texture), false);
						references.sssTexture = (Texture) EditorGUILayout.ObjectField ("Surprised, Closed Mouth", references.sssTexture, typeof (Texture), false);
						references.chTexture = (Texture) EditorGUILayout.ObjectField ("Surprised, Open Mouth", references.chTexture, typeof (Texture), false);
						references.rrrTexture = (Texture) EditorGUILayout.ObjectField ("Angry, Closed Mouth", references.rrrTexture, typeof (Texture), false);
						references.nnnTexture = (Texture) EditorGUILayout.ObjectField ("Angry, Open Mouth", references.nnnTexture, typeof (Texture), false);

						references.aaahTexture = (Texture) EditorGUILayout.ObjectField ("Neutral, Closed Mouth, Blinking", references.aaahTexture, typeof (Texture), false);
						references.eeeTexture = (Texture) EditorGUILayout.ObjectField ("Neutral, Open Mouth, Blinking", references.eeeTexture, typeof (Texture), false);
						references.iiTexture = (Texture) EditorGUILayout.ObjectField ("Happy, Closed Mouth, Blinking", references.iiTexture, typeof (Texture), false);
						references.ohTexture = (Texture) EditorGUILayout.ObjectField ("Happy, Open Mouth, Blinking", references.ohTexture, typeof (Texture), false);
						references.ooohTexture = (Texture) EditorGUILayout.ObjectField ("Sad, Closed Mouth, Blinking", references.ooohTexture, typeof (Texture), false);

						if (references.arbitraryTextures == null || references.arbitraryTextures.Length != 5) {
							references.arbitraryTextures = new Texture[5];
						}
						references.arbitraryTextures[0] = (Texture) EditorGUILayout.ObjectField ("Sad, Open Mouth, Blinking", references.arbitraryTextures[0], typeof (Texture), false);
						references.arbitraryTextures[1] = (Texture) EditorGUILayout.ObjectField ("Surprised, Closed Mouth, Blinking", references.arbitraryTextures[1], typeof (Texture), false);
						references.arbitraryTextures[2] = (Texture) EditorGUILayout.ObjectField ("Surprised, Open Mouth, Blinking", references.arbitraryTextures[2], typeof (Texture), false);
						references.arbitraryTextures[3] = (Texture) EditorGUILayout.ObjectField ("Angry, Closed Mouth, Blinking", references.arbitraryTextures[3], typeof (Texture), false);
						references.arbitraryTextures[4] = (Texture) EditorGUILayout.ObjectField ("Angry, Open Mouth, Blinking", references.arbitraryTextures[4], typeof (Texture), false);
					} else {
						references.neutralTexture = (Texture) EditorGUILayout.ObjectField ("Neutral, Closed Mouth", references.neutralTexture, typeof (Texture), false);
						references.mmmTexture = (Texture) EditorGUILayout.ObjectField ("Neutral, Open Mouth", references.mmmTexture, typeof (Texture), false);
						references.thTexture = (Texture) EditorGUILayout.ObjectField ("Happy, Closed Mouth", references.thTexture, typeof (Texture), false);
						references.fuhTexture = (Texture) EditorGUILayout.ObjectField ("Happy, Open Mouth", references.fuhTexture, typeof (Texture), false);
						references.kkTexture = (Texture) EditorGUILayout.ObjectField ("Sad, Closed Mouth", references.kkTexture, typeof (Texture), false);
						references.ddTexture = (Texture) EditorGUILayout.ObjectField ("Sad, Open Mouth", references.ddTexture, typeof (Texture), false);
						references.sssTexture = (Texture) EditorGUILayout.ObjectField ("Surprised, Closed Mouth", references.sssTexture, typeof (Texture), false);
						references.chTexture = (Texture) EditorGUILayout.ObjectField ("Surprised, Open Mouth", references.chTexture, typeof (Texture), false);
						references.rrrTexture = (Texture) EditorGUILayout.ObjectField ("Angry, Closed Mouth", references.rrrTexture, typeof (Texture), false);
						references.nnnTexture = (Texture) EditorGUILayout.ObjectField ("Angry, Open Mouth", references.nnnTexture, typeof (Texture), false);
					}

					so.ApplyModifiedProperties ();
					break;

				case AvatarModelReferences.ExpressionType.animationParameters:

					SerializedProperty _animator = so.FindProperty ("expressionAnimator");
					EditorGUILayout.PropertyField (_animator, true);

					if (references.animatorParameters == null || references.animatorParameters.Length < 17) {
						references.animatorParameters = new string[17] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
					}

					if (references.expressionAnimator == null) {
						references.expressionAnimator = references.GetComponent<Animator> ();
						so.ApplyModifiedProperties ();
					}

					if (references.expressionAnimator != null) {
						GUILayout.Space (5);
						EditorGUILayout.LabelField ("Animation Parameters", EditorStyles.boldLabel);
						references.animatorParameters[0] = AvatarModelReferencesEditor.FloatTextField ("Neutral", references.animatorParameters[0], references.expressionAnimator, references);
						references.animatorParameters[1] = AvatarModelReferencesEditor.FloatTextField ("Happy", references.animatorParameters[1], references.expressionAnimator, references);
						references.animatorParameters[2] = AvatarModelReferencesEditor.FloatTextField ("sad", references.animatorParameters[2], references.expressionAnimator, references);
						references.animatorParameters[3] = AvatarModelReferencesEditor.FloatTextField ("surprised", references.animatorParameters[3], references.expressionAnimator, references);
						references.animatorParameters[4] = AvatarModelReferencesEditor.FloatTextField ("angry", references.animatorParameters[4], references.expressionAnimator, references);
						//references.animatorParameters[5] = AvatarModelReferencesEditor.FloatTextField ("blinkLeft", references.animatorParameters[5], references.expressionAnimator, references);
						//references.animatorParameters[6] = AvatarModelReferencesEditor.FloatTextField ("blinkRight", references.animatorParameters[6], references.expressionAnimator, references);
						references.animatorParameters[7] = AvatarModelReferencesEditor.FloatTextField ("blinkAll", references.animatorParameters[7], references.expressionAnimator, references);
						references.animatorParameters[8] = AvatarModelReferencesEditor.FloatTextField ("aaah", references.animatorParameters[8], references.expressionAnimator, references);
						references.animatorParameters[9] = AvatarModelReferencesEditor.FloatTextField ("eee", references.animatorParameters[9], references.expressionAnimator, references);
						references.animatorParameters[10] = AvatarModelReferencesEditor.FloatTextField ("ii", references.animatorParameters[10], references.expressionAnimator, references);
						references.animatorParameters[11] = AvatarModelReferencesEditor.FloatTextField ("oh", references.animatorParameters[11], references.expressionAnimator, references);
						references.animatorParameters[12] = AvatarModelReferencesEditor.FloatTextField ("oooh", references.animatorParameters[12], references.expressionAnimator, references);
						references.animatorParameters[13] = AvatarModelReferencesEditor.FloatTextField ("fuh", references.animatorParameters[13], references.expressionAnimator, references);
						references.animatorParameters[14] = AvatarModelReferencesEditor.FloatTextField ("mmm", references.animatorParameters[14], references.expressionAnimator, references);
						references.animatorParameters[15] = AvatarModelReferencesEditor.FloatTextField ("luh", references.animatorParameters[15], references.expressionAnimator, references);
						references.animatorParameters[16] = AvatarModelReferencesEditor.FloatTextField ("sss", references.animatorParameters[16], references.expressionAnimator, references);
					}
					so.ApplyModifiedProperties ();
					break;
			}

			GUILayout.Space (5);

			DrawDebugWindow (references, so);
		}

		private static void SetUpBlendShapeRanges (FacialExpressionReference references) {
			int len = (int) AvatarModelReferences.BlendShapeIndices.sss;

			references.blendShapeRanges = new AvatarModelReferences.BlendShapeRangeList[len];

			for (int i = 0; i < len; i++) {
				references.blendShapeRanges[i] = new AvatarModelReferences.BlendShapeRangeList (1);
				references.blendShapeRanges[i].a[0] = new Vector2 (0, 100);
			}
		}

		private void BasicBlendShapeEditor (FacialExpressionReference references, string[] blendShapes) {
			AvatarModelReferencesEditor.BlendShapeSelectorPlusThreshold (blendShapes, "Happy", ref references.happyShape, ref references.happyThreshold, references.PreviewButton);
			AvatarModelReferencesEditor.BlendShapeSelectorPlusThreshold (blendShapes, "Sad", ref references.sadShape, ref references.sadThreshold, references.PreviewButton);
			AvatarModelReferencesEditor.BlendShapeSelectorPlusThreshold (blendShapes, "Surprised", ref references.surprisedShape, ref references.surprisedThreshold, references.PreviewButton);
			AvatarModelReferencesEditor.BlendShapeSelectorPlusThreshold (blendShapes, "Angry", ref references.angryShape, ref references.angryThreshold, references.PreviewButton);
			Vector2[] ranges = null;
			references.blinkLeftShape = AvatarModelReferencesEditor.BlendShapeSelector ("Blink Left", references.blinkLeftShape, blendShapes, references.PreviewButton, ref ranges);
			references.blinkRightShape = AvatarModelReferencesEditor.BlendShapeSelector ("Blink Right", references.blinkRightShape, blendShapes, references.PreviewButton, ref ranges);

			if (references.blendShapeRanges[(int) AvatarModelReferences.BlendShapeIndices.blinkAll] == null) {
				SetUpBlendShapeRanges (references);
			}

			references.blinkAllShape = AvatarModelReferencesEditor.BlendShapeSelector ("Blink All", references.blinkAllShape, blendShapes, references.PreviewButton, ref references.GetRangeList ((int) AvatarModelReferences.BlendShapeIndices.blinkAll).a, true, false, false, null, references);
		}

		private bool debugVisible = false;

		private void DrawDebugWindow (FacialExpressionReference component, SerializedObject so) {
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
