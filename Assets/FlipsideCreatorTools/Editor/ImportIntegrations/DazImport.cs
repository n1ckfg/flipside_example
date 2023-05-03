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

	public class DazImport : IImport {

		public bool CanAutoSetup (AvatarModelReferences avatarModelReferences) {
			foreach (Transform trans in avatarModelReferences.transform) {
				if (trans.name.StartsWith ("Genesis8")) return true;
			}
			return false;
		}

		private string SetOrAppend (string orig, string add) {
			if (orig == "" || orig == "-1") return add;
			return string.Format ("{0},{1}", orig, add);
		}

		public void Setup (AvatarModelReferences avatarModelReferences) {
			avatarModelReferences.expressionType = AvatarModelReferences.ExpressionType.blendShapes;

			string[] blendShapes = GetBlendShapeNames (avatarModelReferences.mesh != null ? avatarModelReferences.mesh.sharedMesh : null);

			for (int i = 0; i < blendShapes.Length; i++) {
				string bs = blendShapes[i];

				if (avatarModelReferences.surprisedShape == "-1" && bs.EndsWith ("Surprised_HD")) {
					avatarModelReferences.surprisedShape = i.ToString ();
				} else if (avatarModelReferences.happyShape == "-1" && bs.EndsWith ("SmileOpenFullFace_HD")) {
					avatarModelReferences.happyShape = i.ToString ();
				} else if (avatarModelReferences.sadShape == "-1" && bs.EndsWith ("Frown_HD")) {
					avatarModelReferences.sadShape = i.ToString ();
				} else if (avatarModelReferences.angryShape == "-1" && bs.EndsWith ("Angry_HD")) {
					avatarModelReferences.angryShape = i.ToString ();
				} else if (avatarModelReferences.blinkLeftShape == "-1" && bs.EndsWith ("EyesClosedL")) {
					avatarModelReferences.blinkLeftShape = i.ToString ();
				} else if (avatarModelReferences.blinkRightShape == "-1" && bs.EndsWith ("EyesClosedR")) {
					avatarModelReferences.blinkRightShape = i.ToString ();
				} else if (avatarModelReferences.blinkAllShape == "-1" && bs.EndsWith ("EyesClosed")) {
					avatarModelReferences.blinkAllShape = i.ToString ();
				} else if (bs.EndsWith ("vAA")) {
					avatarModelReferences.aaahShape = SetOrAppend (avatarModelReferences.aaahShape, i.ToString ());
				} else if (bs.EndsWith ("vIY")) {
					avatarModelReferences.iiShape = SetOrAppend (avatarModelReferences.iiShape, i.ToString ());
				} else if (bs.EndsWith ("vEE")) {
					avatarModelReferences.eeeShape = SetOrAppend (avatarModelReferences.eeeShape, i.ToString ());
				} else if (bs.EndsWith ("vM")) {
					avatarModelReferences.mmmShape = SetOrAppend (avatarModelReferences.mmmShape, i.ToString ());
				} else if (bs.EndsWith ("vF")) {
					avatarModelReferences.fuhShape = SetOrAppend (avatarModelReferences.fuhShape, i.ToString ());
				} else if (bs.EndsWith ("vL")) {
					avatarModelReferences.luhShape = SetOrAppend (avatarModelReferences.luhShape, i.ToString ());
				} else if (bs.EndsWith ("vOW")) {
					avatarModelReferences.ohShape = SetOrAppend (avatarModelReferences.ohShape, i.ToString ());
				} else if (bs.EndsWith ("vUW")) {
					avatarModelReferences.ooohShape = SetOrAppend (avatarModelReferences.ooohShape, i.ToString ());
				} else if (bs.EndsWith ("vS")) {
					avatarModelReferences.sssShape = SetOrAppend (avatarModelReferences.sssShape, i.ToString ());
				}
			}

			// Make sure the correct main mesh is assigned
			Transform main = FindRecursive (avatarModelReferences.transform, "Genesis8Male.Shape");
			if (main == null) {
				main = FindRecursive (avatarModelReferences.transform, "Genesis8Female.Shape");
			}
			if (main != null) {
				SkinnedMeshRenderer mainMesh = main.GetComponent<SkinnedMeshRenderer> ();
				if (mainMesh != null) {
					avatarModelReferences.mesh = mainMesh;
				}
			}

			// Find eyelashes and add them as an additional mesh
			Transform eyelashes = EndsWithRecursive (avatarModelReferences.transform, "Eyelashes.Shape");

			if (eyelashes != null) {
				SkinnedMeshRenderer eyelashesMesh = eyelashes.GetComponent<SkinnedMeshRenderer> ();
				if (eyelashesMesh != null) {
					avatarModelReferences.additionalMeshes = new SkinnedMeshRenderer[] { eyelashesMesh };
				}
			}

			// Fix jaw so mouth doesn't hang open and teeth move with the mouth
			Transform lowerJaw = FindRecursive (avatarModelReferences.transform, "lowerJaw");
			Transform upperFaceRig = FindRecursive (avatarModelReferences.transform, "upperFaceRig");

			if (lowerJaw != null && upperFaceRig != null) {
				lowerJaw.SetParent (upperFaceRig, false);
				lowerJaw.transform.localRotation = Quaternion.identity;
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

		internal static Transform EndsWithRecursive (Transform parent, string endsWith) {
			foreach (Transform child in parent) {
				if (child.name.EndsWith (endsWith)) {
					return child;
				}
				Transform found = EndsWithRecursive (child, endsWith);
				if (found != null) {
					return found;
				}
			}
			return null;
		}
	}
}