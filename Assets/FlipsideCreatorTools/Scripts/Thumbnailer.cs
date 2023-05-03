/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using Flipside.Avatars;
using Flipside.Sets;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Flipside {

	public class Thumbnailer {

		static public void TakeThumbnail (string bundlePath, string assetBundleName, int propIndex = -1) {
			Scene scene = SceneManager.GetActiveScene ();
			var cameraContainer = new GameObject ("Camera Container");
			var camera = cameraContainer.AddComponent<Camera> ();
			camera.cullingMask &= ~(1 << LayerMask.NameToLayer ("UI"));

			// Setup background for characters
			GameObject bg = GameObject.CreatePrimitive (PrimitiveType.Plane);
			bg.name = "Background";
			bg.transform.position = new Vector3 (0f, 0f, -20f);
			bg.transform.eulerAngles = new Vector3 (90f, 0f, 0f);
			bg.transform.localScale = new Vector3 (3f, 3f, 3f);

			Material bgMat = new Material (Shader.Find ("Unlit/Color"));
			bgMat.SetColor ("_Color", new Color32 (200, 200, 200, 1));
			MeshRenderer bgMeshRenderer = bg.GetComponent<MeshRenderer> ();
			bgMeshRenderer.material = bgMat;

			bg.SetActive (false);

			var audience = scene.GetRootGameObjects ()[0].GetComponentInChildren<Audience> ();
			if (audience != null) {
				var firstCam = scene.GetRootGameObjects ()[0].GetComponentInChildren<Camera> (true);
				if (firstCam != null) {
					cameraContainer.transform.position = firstCam.transform.position;
					cameraContainer.transform.rotation = firstCam.transform.rotation;
				} else {
					cameraContainer.transform.parent = audience.transform;
					cameraContainer.transform.localPosition = new Vector3 (0f, 1.5f, 0f);
					cameraContainer.transform.localRotation = Quaternion.identity;
				}
			}

			var avatar = scene.GetRootGameObjects ()[0].GetComponentInChildren<AvatarModelReferences> ();
			if (avatar != null) {
				cameraContainer.transform.parent = avatar.centerEye.transform;
				cameraContainer.transform.localPosition = new Vector3 (0f, 0f, 3.75f);
				cameraContainer.transform.localRotation = Quaternion.Euler (0f, 180f, 0f);

				camera.fieldOfView = 24;
				camera.nearClipPlane = 0.1f;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = new Color32 (200, 200, 200, 1);

				bg.SetActive (true);
			}

			PropKit propkit = scene.GetRootGameObjects ()[0].GetComponentInChildren<PropKit> ();
			GameObject propInstance = null;
			GameObject propLight = null;
			if (propkit != null) {
				cameraContainer.transform.position = new Vector3 (-0.25f, 0f, 0.9f);
				cameraContainer.transform.eulerAngles = new Vector3 (0f, 165f, 0f);

				camera.fieldOfView = 24;
				camera.nearClipPlane = 0.1f;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = new Color32 (200, 200, 200, 1);

				bg.SetActive (true);

#if UNITY_EDITOR

				propInstance = propkit.ShowProp ((propIndex == -1) ? 0 : propIndex);
				propLight = propkit.CreatePropLight ();

#endif

				if (propInstance != null) {
					propInstance.transform.position = Vector3.zero;
					propInstance.transform.rotation = Quaternion.identity;
				}
			}

			int resWidth = 500;
			int resHeight = 400;

			RenderTexture renderTex = new RenderTexture (500, 400, 32);

			camera.targetTexture = renderTex;
			Texture2D screenShot = new Texture2D (resWidth, resHeight, TextureFormat.ARGB32, false);
			camera.Render ();
			RenderTexture.active = renderTex;
			screenShot.ReadPixels (new Rect (0f, 0f, resWidth, resHeight), 0, 0);

			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			GameObject.DestroyImmediate (renderTex);
			GameObject.DestroyImmediate (cameraContainer);
			GameObject.DestroyImmediate (bg);
			if (propInstance != null) {
				GameObject.DestroyImmediate (propInstance);
			}
			if (propLight != null) {
				GameObject.DestroyImmediate (propLight);
			}

			byte[] bytes = screenShot.EncodeToPNG ();

			string filename = bundlePath + "/" + assetBundleName + ".png";

			if (propIndex != -1) {
				filename = bundlePath + "/" + assetBundleName + "." + propIndex + ".png";
			}

			if (File.Exists (filename)) {
				File.Delete (filename);
			}

			File.WriteAllBytes (filename, bytes);
		}

		static public void TakePropThumbnails (string bundlePath, string assetBundleName) {
			Scene scene = SceneManager.GetActiveScene ();

			PropKit propkit = scene.GetRootGameObjects ()[0].GetComponentInChildren<PropKit> ();
			if (propkit == null) return;

			for (int i = 0; i < propkit.propList.Length; i++) {
				TakeThumbnail (bundlePath, assetBundleName, i);
			}
		}
	}
}