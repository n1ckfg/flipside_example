/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
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

namespace Flipside.Avatars {

	public class CharacterCreatorImport : IImport {

		public bool CanAutoSetup (AvatarModelReferences avatarModelReferences) {
			foreach (Transform trans in avatarModelReferences.transform) {
				if (trans.name.StartsWith ("CC_")) return true;
			}
			return false;
		}

		public void Setup (AvatarModelReferences avatarModelReferences) {
			avatarModelReferences.expressionType = AvatarModelReferences.ExpressionType.simplifiedBlendShapes;

			string[] blendShapes = GetBlendShapeNames (avatarModelReferences.mesh != null ? avatarModelReferences.mesh.sharedMesh : null);

			string happy = "";
			string sad = "";
			string surprised = "";
			string angry = "";
			string blinkLeft = "";
			string blinkRight = "";
			string blinkAll = "";
			string openMouth = "";

			for (int i = 0; i < blendShapes.Length; i++) {
				switch (blendShapes[i]) {
					case "Brow_Raise_Inner_L":
					case "Brow_Raise_Inner_R":
						sad += (sad == "") ? i.ToString () : "," + i.ToString ();
						happy += (happy == "") ? i.ToString () : "," + i.ToString ();
						break;

					case "Mouth_Frown":
						sad += (sad == "") ? i.ToString () : "," + i.ToString ();
						break;

					case "Mouth_Smile":
						happy += (happy == "") ? i.ToString () : "," + i.ToString ();
						break;

					case "Eye_Wide_L":
					case "Eye_Wide_R":
					case "Brow_Raise_Outer_L":
					case "Brow_Raise_Outer_R":
					case "Mouth_Lips_Part":
						surprised += (surprised == "") ? i.ToString () : "," + i.ToString ();
						break;

					case "Eye_Squint_L":
					case "Eye_Squint_R":
					case "Brow_Drop_L":
					case "Brow_Drop_R":
					case "Nose_Scrunch":
						angry += (angry == "") ? i.ToString () : "," + i.ToString ();
						break;

					case "Eye_Blink_L":
						blinkLeft = i.ToString ();
						break;

					case "Eye_Blink_R":
						blinkRight = i.ToString ();
						break;

					case "Eye_Blink":
						blinkAll = i.ToString ();
						break;

					case "Merged_Open_Mouth":
						openMouth = i.ToString ();
						break;
				}
			}

			avatarModelReferences.happyShape = happy;
			avatarModelReferences.sadShape = sad;
			avatarModelReferences.surprisedShape = surprised;
			avatarModelReferences.angryShape = angry;
			avatarModelReferences.blinkLeftShape = blinkLeft;
			avatarModelReferences.blinkRightShape = blinkRight;
			avatarModelReferences.blinkAllShape = blinkAll;
			avatarModelReferences.openMouthShape = openMouth;

			// Fix jaw so mouth doesn't hang open and teeth move with the mouth
			Transform jawRoot = FindRecursive (avatarModelReferences.transform, "CC_Base_JawRoot");
			Transform upperJaw = FindRecursive (avatarModelReferences.transform, "CC_Base_UpperJaw");

			if (jawRoot != null && upperJaw != null) {
				jawRoot.SetParent (upperJaw, true);
			}
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

		internal static Transform FindRecursive (Transform parent, string childName) {
			foreach (Transform child in parent) {
				if (child.name == childName) {
					return child;
				}
				Transform found = FindRecursive (child, childName);
				if (found != null) {
					return found;
				}
			}
			return null;
		}
	}
}