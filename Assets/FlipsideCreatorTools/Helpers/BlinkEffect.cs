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

namespace Flipside.Helpers {

	public class BlinkEffect : MonoBehaviour {
		private bool drawOverlay = false;
		private Material material;
		private Color color = new Color (0.01f, 0.01f, 0.01f, 1.0f);
		private WaitForEndOfFrame wait = new WaitForEndOfFrame ();

		private void Awake () {
			material = new Material (Shader.Find ("Other/Unlit Transparent Color"));
			material.color = color;
		}

		private void OnPostRender () {
			if (drawOverlay) {
				material.SetPass (0);
				GL.PushMatrix ();
				GL.LoadOrtho ();
				GL.Color (material.color);
				GL.Begin (GL.QUADS);
				GL.Vertex3 (0f, 0f, -12f);
				GL.Vertex3 (0f, 1f, -12f);
				GL.Vertex3 (1f, 1f, -12f);
				GL.Vertex3 (1f, 0f, -12f);
				GL.End ();
				GL.PopMatrix ();
			}
		}

		public Coroutine Blink (float duration = 0.2f) {
			StopAllCoroutines ();
			return StartCoroutine (DoBlink (duration));
		}

		public Coroutine Unblink (float duration = 0.2f) {
			StopAllCoroutines ();
			return StartCoroutine (DoUnblink (duration));
		}

		public void CancelBlink () {
			StopAllCoroutines ();
			drawOverlay = false;
		}

		private IEnumerator DoBlink (float duration) {
			drawOverlay = true;
			color.a = 0f;
			material.color = color;
			float time = 0f;

			while (time < duration) {
				yield return wait;
				time += Time.deltaTime;
				color.a = Mathf.Clamp01 (time / duration);
				material.color = color;
			}
		}

		private IEnumerator DoUnblink (float duration) {
			drawOverlay = true;
			color.a = 1f;
			material.color = color;
			float time = 0f;

			while (time < duration) {
				yield return wait;
				time += Time.deltaTime;
				color.a = 1f - Mathf.Clamp01 (time / duration);
				material.color = color;
			}

			drawOverlay = false;
		}
	}
}