/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Flipside {

	public class FlipsideApi : MonoBehaviour {

		[Serializable]
		public class ErrorResponse {
			public bool success = false;
			public string data = "";
		}

		private string error = "";

		public bool IsError () {
			return (error != "");
		}

		public string Error () {
			return error;
		}

		public void ExpireCache (string file) {
			PlayerPrefs.SetInt ("file:" + file, 0);

			string cachePath = CachePath (file);

			if (File.Exists (cachePath)) File.Delete (cachePath);
		}

		public void DownloadFile (string file, Action<string> callback = null) {
			StartCoroutine (DoDownloadFile (file, callback));
		}

		private IEnumerator DoDownloadFile (string file, Action<string> callback = null) {
			Debug.Log ("Downloading file: " + file);

			if (file.IndexOf ("http://") != 0 && file.IndexOf ("https://") != 0) {
				Debug.Log ("Not a link: " + file);
				if (callback != null) callback (file);
				yield break;
			}

			if (file.Contains ("www.dropbox.com")) {
				file = ConvertDropboxLink (file);
			} else if (file.Contains ("drive.google.com")) {
				file = ConvertGoogleDriveLink (file);
			}

			if (IsDownloaded (file)) {
				Debug.Log ("Already downloaded: " + file);
				if (callback != null) callback (CachePath (file));
				yield break;
			}

			string fileKey = "file:" + file;

			PlayerPrefs.SetInt (fileKey, 0);

			using (UnityWebRequest www = UnityWebRequest.Get (file)) {
				www.SendWebRequest ();

				WaitForEndOfFrame wait = new WaitForEndOfFrame ();

				prevDownloadProgress = 0f;

				while (!www.isDone) {
					if (www.downloadProgress > prevDownloadProgress + 0.01f) {
						Debug.Log (string.Format ("Downloading {0} - {1}%", file, Mathf.RoundToInt (www.downloadProgress * 100f)));
						prevDownloadProgress = www.downloadProgress;
					}

					yield return wait;
				}

				if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
					Debug.LogError ("Download error: " + www.error);
					error = www.error;
					yield break;
				}

				string cachePath = CachePath (file);

				if (File.Exists (cachePath)) {
					File.Delete (cachePath);
				}

				FileStream fs = File.Create (cachePath);
				fs.Write (www.downloadHandler.data, 0, www.downloadHandler.data.Length);
				fs.Close ();

				PlayerPrefs.SetInt (fileKey, 1);

				if (callback != null) callback (cachePath);
			}
		}

		private IEnumerator LoadTexture (string file, int width, int height, TextureFormat fmt, Action<Texture2D> callback) {
			string cacheLink = string.Format ("file:///{0}/{1}", Application.dataPath.Replace ("/Assets", ""), CachePath (file).Replace (" ", "%20"));
			Debug.Log ("Loading texture: " + cacheLink);

			using (UnityWebRequest www = UnityWebRequestTexture.GetTexture (cacheLink, true)) {
				www.timeout = 30;
				yield return www.SendWebRequest ();

				if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
					Debug.LogError ("Texture error: " + www.error);
					error = www.error;
					yield break;
				}

				Texture2D tex = (www.downloadHandler as DownloadHandlerTexture).texture;
				tex.wrapMode = TextureWrapMode.Clamp;

				callback.Invoke (tex);
			}
		}

		private float prevDownloadProgress = 0f;

		private bool IsDownloaded (string file) {
			if (file.IndexOf ("http://") == 0 || file.IndexOf ("https://") == 0) {
				string cachePath = CachePath (file);

				if (File.Exists (cachePath)) {
					if (PlayerPrefs.HasKey ("file:" + file)) {
						return (PlayerPrefs.GetInt ("file:" + file) == 1);
					}
					// Incomplete
				}
				// Incomplete or doesn't exist
				return false;
			}
			// Local file
			return true;
		}

		public string CachePath (string file) {
			string path = "AssetBundles/DownloadCache";

			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}

			int qmark = file.LastIndexOf ('?');
			if (qmark > 0) {
				file = file.Substring (0, qmark);
			}

			file = file.Replace ("\\", "/").Replace (":", "-").Replace ("&", "-").Replace ("=", "-").Replace ("%", "-").Replace (";", "-").Replace (" ", "-");

			int index = file.LastIndexOf ('/');

			string folder = file;

			if (index > -1) {
				folder = folder.Substring (0, index);
				file = file.Substring (index + 1);
			}

			folder = folder.Replace ("/", "-");

			path = string.Format ("{0}/{1}", path, folder);

			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}

			return string.Format ("{0}/{1}", path, file);
		}

		private string ConvertDropboxLink (string slide) {
			return StripParameters (slide).Replace ("www.dropbox.com", "dl.dropboxusercontent.com");
		}

		private string ConvertGoogleDriveLink (string slide) {
			if (slide.Contains ("&export=download")) {
				return slide;
			} else if (slide.Contains ("/open?id=")) {
				var parts = slide.Split ('=');

				return string.Format ("https://drive.google.com/uc?authuser=0&id={0}&export=download", parts[parts.Length - 1]);
			} else {
				var parts = slide.Split ('/');

				return string.Format ("https://drive.google.com/uc?authuser=0&id={0}&export=download", parts[parts.Length - 2]);
			}
		}

		private string StripParameters (string slide) {
			int qmark = slide.LastIndexOf ('?');

			return (qmark > 0)
				? slide.Substring (0, qmark)
				: slide;
		}
	}
}