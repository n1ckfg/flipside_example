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

namespace Flipside.Avatars {

	public class WolfImport : IImport {

		public bool CanAutoSetup (AvatarModelReferences avatarModelReferences) {
			var head = avatarModelReferences.transform.Find ("head_object");

			if (head == null) return false;
			if (head.GetComponent<SkinnedMeshRenderer> () == null) return false;
			if (head.GetComponent<SkinnedMeshRenderer> ().sharedMesh.blendShapeCount != 51) return false;

			return true;
		}

		public void Setup (AvatarModelReferences avatarModelReferences) {
			Debug.Log ("Importing wolf3d character");
			SetBlendShapes (avatarModelReferences);
			LinkEyesToHead (avatarModelReferences);
			SetupAnimator (avatarModelReferences);
		}

		private void SetBlendShapes (AvatarModelReferences avatarModelReferences) {
			avatarModelReferences.expressionType = AvatarModelReferences.ExpressionType.simplifiedBlendShapes;
		}

		private void LinkEyesToHead (AvatarModelReferences avatarModelReferences) {
			avatarModelReferences.eyes.Clear ();
			var head = avatarModelReferences.transform.Find ("head_object");

			var eyeReflection1 = head.transform.Find ("eye_reflection");
			var eye1 = eyeReflection1.transform.Find ("eye");
			SetupEye (eye1, avatarModelReferences);
			UnityEngine.Object.DestroyImmediate (eyeReflection1.gameObject);

			var eyeReflection2 = head.transform.Find ("eye_reflection.001");
			var eye2 = eyeReflection2.transform.Find ("eye.001");
			SetupEye (eye2, avatarModelReferences);
			UnityEngine.Object.DestroyImmediate (eyeReflection2.gameObject);

			head.parent = avatarModelReferences.centerEye.parent;
		}

		private void SetupEye (Transform eyeTransform, AvatarModelReferences avatarModelReferences) {
			Transform eyeContainer = SetupEyeContainer (eyeTransform, avatarModelReferences.centerEye.parent);
			FixEyeMesh (eyeTransform);
			avatarModelReferences.eyes.Add (eyeContainer);
		}

		private static Transform SetupEyeContainer (Transform eyeTransform, Transform newEyeParent) {
			Transform eyeContainer = new GameObject ("eyeContainer").transform;
			eyeContainer.position = eyeTransform.position;
			eyeContainer.parent = newEyeParent;
			eyeTransform.parent = eyeContainer;
			return eyeContainer;
		}

		private static SkinnedMeshRenderer FixEyeMesh (Transform eyeTransform) {
			SkinnedMeshRenderer oldRenderer = eyeTransform.GetComponent<SkinnedMeshRenderer> ();
			MeshRenderer newRenderer = eyeTransform.gameObject.AddComponent<MeshRenderer> ();
			MeshFilter newMesh = eyeTransform.gameObject.AddComponent<MeshFilter> ();

			newMesh.sharedMesh = oldRenderer.sharedMesh;
			newRenderer.sharedMaterials = oldRenderer.sharedMaterials;

			UnityEngine.Object.DestroyImmediate (oldRenderer);
			return oldRenderer;
		}

		private void SetupAnimator (AvatarModelReferences avatarModelReferences) {
			avatarModelReferences.expressionType = AvatarModelReferences.ExpressionType.animation;

			Animator expressionAnimator = avatarModelReferences.centerEye.parent.gameObject.AddComponent<Animator> ();

			expressionAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController> ("Assets/FlipsideCreatorTools/Resources/Wolf3D/Wolf3DExpressionTemplate.controller");

			avatarModelReferences.expressionAnimator = expressionAnimator;
		}
	}
}