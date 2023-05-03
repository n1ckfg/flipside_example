/**
 * Copyright (c) 2016 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using System.Text;
using System.Collections;

namespace Flipside {

	/// <summary>
	/// These are extensions to the Transform object.
	/// </summary>
	public static class TransformExtensions {

		/// <summary>
		/// Set the layer recursively for all children.
		/// </summary>
		/// <param name="trans">Transform.</param>
		/// <param name="layer">Layer.</param>
		/// <param name="skipLayer">Skip objects on this layer.</param>
		public static void SetLayer (this Transform trans, int layer, int skipLayer = -1) {
			if (trans == null)
				return;
			if (skipLayer == -1 || trans.gameObject.layer != skipLayer) {
				trans.gameObject.layer = layer;
			}

			foreach (Transform child in trans) {
				child.SetLayer (layer, skipLayer);
			}
		}

		/// <summary>
		/// Destroy all children of the transform.
		/// </summary>
		/// <param name="trans">Transform.</param>
		public static void Clear (this Transform trans) {
			foreach (Transform child in trans) {
				GameObject.Destroy (child.gameObject);
			}
		}

		/// <summary>
		/// Get the full path to this object in the hierarchy, which
		/// can then be used in GameObject.Find() to retrieve that
		/// object at a later time. Basically, a poor man's object ID.
		///
		/// Note: See Social.Utils.Tagger for a memoized wrapper around
		/// this method, including methods to rename and re-parent game
		/// objects and auto-updating the memoized versions.
		/// </summary>
		/// <param name="trans">Transform.</param>
		/// <returns>Path.</returns>
		public static string GetPath (this Transform trans) {
			var sb = new StringBuilder ();
			sb.Append (trans.name);

			while (trans.parent != null) {
				trans = trans.parent;
				sb.Insert (0, '/');
				sb.Insert (0, trans.name);
			}

			return sb.ToString ();
		}

		public static void SetLocalPositionAndRotation (this Transform trans, Vector3 pos, Quaternion rot) {
			//Debug.LogFormat ("{0}.SetLocalPositionAndRotation(({1}, {2}, {3}), {4})", trans.GetPath (), pos.x, pos.y, pos.z, rot);
			trans.localPosition = pos;
			trans.localRotation = rot;
		}

#if UNITY_EDITOR

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy Path")]
		public static void CopyPath (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.GetPath ();
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy Local Position")]
		public static void CopyLocalPosition (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.localPosition.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy World Position")]
		public static void CopyWorldPosition (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.position.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy Local Euler")]
		public static void CopyLocalEuler (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.localEulerAngles.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy World Euler")]
		public static void CopyWorldEuler (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.eulerAngles.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy Local Quaternion")]
		public static void CopyLocalQuaternion (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.localRotation.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy World Quaternion")]
		public static void CopyWorldQuaternion (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.rotation.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy Local Scale")]
		public static void CopyLocalScale (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.localScale.ToString ("F6");
		}

		[UnityEditor.MenuItem ("CONTEXT/Transform/Copy World Scale")]
		public static void CopyWorldScale (UnityEditor.MenuCommand cmd) {
			Transform trans = (Transform) cmd.context;
			UnityEditor.EditorGUIUtility.systemCopyBuffer = trans.lossyScale.ToString ("F6");
		}

#endif
	}
}