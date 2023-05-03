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

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Helpers {

	/// <summary>
	/// Attach to an object to set its rotations in editor to specific quaternion values.
	/// </summary>
	public class RotationFixer : MonoBehaviour {
		public float x;
		public float y;
		public float z;
		public float w;
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (RotationFixer))]
	public class RotationFixerEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			if (GUILayout.Button ("Fix Rotations")) {
				RotationFixer rot = (RotationFixer) target;

				Quaternion q = new Quaternion (rot.x, rot.y, rot.z, rot.w);

				rot.transform.localRotation = q;
			}
		}
	}

#endif
}