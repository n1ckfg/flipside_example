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

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Helpers {

	/// <summary>
	/// Shows a canvas on hover, otherwise hides it.
	/// </summary>
	public class ShowOnHover : MonoBehaviour {
		private Canvas canvas;
		private WaitForSeconds wfs;
		private Vector3 lastMousePosition;
		private float timer = 0f;
		private float delay = 1f;

		private void Awake () {
			canvas = GetComponent<Canvas> ();
			wfs = new WaitForSeconds (0.3f);
		}

		private void Update () {
#if UNITY_EDITOR
			if (lastMousePosition != Input.mousePosition && EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.ToString ().Trim () == "(UnityEditor.GameView)") {
				lastMousePosition = Input.mousePosition;
				canvas.enabled = true;
				timer = delay;
			}

			if (timer > 0f) {
				timer -= Time.deltaTime;
			}

			if (timer <= 0f) {
				canvas.enabled = false;
			}
#else
			canvas.enabled = false;
#endif
		}
	}
}