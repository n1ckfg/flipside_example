/**
 * Copyright (c) 2021 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Flipside.Avatars;
using Flipside.Sets;

namespace Flipside {

	public class ApiClient {

		[Serializable]
		public class Response {
			public bool success = true;
			public string error = "";
		}

		[Serializable]
		public class Token {
			public int user_id;
			public string token;
			public string api_key;
		}

		[Serializable]
		public class TokenResponse : Response {
			public Token data = new Token ();
		}

		[Serializable]
		public class PublishResponse : Response {
			public int data = -1;
		}

		public delegate void ErrorDelegate (string error);

		public static ErrorDelegate OnError;

		public delegate void ProgressDelegate (float progress);

		public static ProgressDelegate OnProgress;

		private static string authEndpoint = "https://www.flipsidexr.com/flipside/api/";
		private static string hmacPrefix = "www.flipsidexr.com/flipside/api/";
		private static string hmacToken = "";
		private static string hmacSecret = "";
		private static int creatorID = 0;
		private static int requestTimeout = 30;

		private static System.Text.Encoding enc = System.Text.Encoding.GetEncoding ("ISO-8859-1");

		private static Dictionary<UnityWebRequest, Action<UnityWebRequest>> requests = new Dictionary<UnityWebRequest, Action<UnityWebRequest>> ();
		private static List<UnityWebRequest> remove = new List<UnityWebRequest> ();

		private static bool initialized = false;

		public static void FetchToken (string username, string password, Action<int> callback) {
			Init ();

			string suffix = "request_token";

			WWWForm form = new WWWForm ();
			form.AddField ("username", username);
			form.AddField ("password", password);

			var req = PostRequestForSuffix (suffix, form);

			requests[req] = (UnityWebRequest www) => {
				TokenResponse res;

				try {
					res = JsonUtility.FromJson<TokenResponse> (www.downloadHandler.text);
				} catch (Exception e) {
					if (OnError != null) OnError ("JSON parse error: " + e.Message + ", body: " + www.downloadHandler.text);
					return;
				}

				if (!res.success) {
					if (OnError != null) OnError (res.error);
				} else {
					creatorID = res.data.user_id;
					hmacToken = res.data.token;
					hmacSecret = res.data.api_key;

					File.WriteAllText (Application.persistentDataPath + "/.fsid", creatorID.ToString ());
					File.WriteAllText (Application.persistentDataPath + "/.token", hmacToken + ":" + hmacSecret);

					callback (res.data.user_id);
				}
			};

			req.SendWebRequest ();
		}

		public static int GetCreatorID () {
			Init ();

			return creatorID;
		}

		private static T GetComponentOnRootElement<T> (Scene scene, bool logErrors = true) where T : MonoBehaviour {
			if (scene.rootCount == 0) {
				if (logErrors) Debug.LogError ("The scene is empty. There should be at least one object in the scene.");
				return null;
			}

			var root = scene.GetRootGameObjects ()[0];
			var obj = root.GetComponent<T> ();

			if (obj == null) {
				if (logErrors) Debug.LogError ("The component was not found on root scene object.");
				return null;
			}

			return obj;
		}

		/// <summary>
		/// Has the scene changed since the last asset bundle was generated for it?
		/// </summary>
		public static bool HasSceneChanges () {
			Scene scene = SceneManager.GetActiveScene ();
			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;
			string bundle_windows = Application.dataPath + "/../AssetBundles/" + assetBundleName;

			if (File.Exists (bundle_windows)) {
				// Compare bundle and scene file write times
				FileInfo bi = new FileInfo (bundle_windows);
				FileInfo fi = new FileInfo (scene.path);

				if (DateTime.Compare (fi.LastWriteTime, bi.LastWriteTime) > 0) {
					return true;
				}

				return false;
			}

			return true;
		}

		/// <summary>
		/// Has a new version of the current scene's asset bundle been generated since we last marked it
		/// in their player prefs?
		/// </summary>
		public static bool HasNewBundle () {
			Scene scene = SceneManager.GetActiveScene ();

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;
			string key = "published:" + assetBundleName;
			string bundle_windows = Application.dataPath + "/../AssetBundles/" + assetBundleName;

			if (PlayerPrefs.HasKey (key)) {
				string published = PlayerPrefs.GetString (key);
				DateTime pubDate = DateTime.Parse (published, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

				// Compare bundle file date to pubDate
				FileInfo fi = new FileInfo (bundle_windows);

				if (DateTime.Compare (fi.LastWriteTime, pubDate) != 0) {
					return true;
				}

				return false;
			}

			return File.Exists (bundle_windows);
		}

		/// <summary>
		/// Publish a character. Callback receives the character ID.
		/// </summary>
		public static void PublishAvatar (Action<int> callback) {
			Init ();

			if (creatorID == 0) {
				if (OnError != null) OnError ("Flipside creator ID not set. Please log in and try again.");
				return;
			}

			if (!HasToken ()) {
				if (OnError != null) OnError ("No publishing token acquired. Please log in and try again.");
				return;
			}

			Scene scene = SceneManager.GetActiveScene ();
			AvatarModelReferences avatarInfo = GetComponentOnRootElement<AvatarModelReferences> (scene);

			if (avatarInfo == null) {
				if (OnError != null) OnError ("AvatarModelReferences component not found on root scene object.");
				return;
			}

			if (avatarInfo.name.Trim (' ') == "") {
				if (OnError != null) OnError ("Please add a character name in the AvatarModelReferences component.");
				return;
			}

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;
			string bundle_windows = Application.dataPath + "/../AssetBundles/" + assetBundleName;
			string bundle_android = Application.dataPath + "/../AssetBundles/" + assetBundleName + "_android";
			string thumbnail = (avatarInfo.thumbnail != "" && File.Exists (avatarInfo.thumbnail))
				? avatarInfo.thumbnail
				: Application.dataPath + "/../AssetBundles/" + assetBundleName + ".png";

			string suffix = "avatar";

			WWWForm form = new WWWForm ();

			form.AddField ("creator", creatorID);
			form.AddField ("name", avatarInfo.characterName);
			form.AddField ("attribution", avatarInfo.attribution);
			form.AddField ("bundle_name", assetBundleName);

			string hash = CalculateHash ("POST", suffix, form);

			if (File.Exists (bundle_windows)) form.AddBinaryData ("bundle_windows2019", File.ReadAllBytes (bundle_windows), assetBundleName, "application/octet-stream");
			if (File.Exists (bundle_android)) form.AddBinaryData ("bundle_android", File.ReadAllBytes (bundle_android), assetBundleName + "_android", "application/octet-stream");
			if (File.Exists (thumbnail)) form.AddBinaryData ("thumbnail", File.ReadAllBytes (thumbnail), Path.GetFileName (thumbnail), "application/octet-stream");

			var req = PostRequestForSuffix (suffix, form, hash);
			req.timeout = 0;

			requests[req] = (UnityWebRequest www) => {
				PublishResponse res;

				try {
					res = JsonUtility.FromJson<PublishResponse> (www.downloadHandler.text);
				} catch (Exception e) {
					if (OnError != null) OnError ("JSON parse error: " + e.Message + ", body: " + www.downloadHandler.text);
					return;
				}

				if (!res.success) {
					if (OnError != null) OnError (res.error);
				} else {
					FileInfo fi = new FileInfo (bundle_windows);
					PlayerPrefs.SetString ("published:" + assetBundleName, fi.LastWriteTime.ToString ("O"));
					callback (res.data);
				}
			};

			req.SendWebRequest ();
		}

		/// <summary>
		/// Publish a set. Callback receives the set ID.
		/// </summary>
		public static void PublishSet (Action<int> callback) {
			Init ();

			if (creatorID == 0) {
				if (OnError != null) OnError ("Flipside creator ID not set. Please log in and try again.");
				return;
			}

			if (!HasToken ()) {
				if (OnError != null) OnError ("No publishing token acquired. Please log in and try again.");
			}

			Scene scene = SceneManager.GetActiveScene ();
			SetInfo setInfo = GetComponentOnRootElement<SetInfo> (scene);

			if (setInfo == null) {
				if (OnError != null) OnError ("SetInfo component not found on root scene object.");
				return;
			}

			if (setInfo.name.Trim (' ') == "") {
				if (OnError != null) OnError ("Please add a set name in the SetInfo component.");
				return;
			}

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;
			string bundle_windows = Application.dataPath + "/../AssetBundles/" + assetBundleName;
			string bundle_android = Application.dataPath + "/../AssetBundles/" + assetBundleName + "_android";
			string thumbnail = (setInfo.thumbnail != "" && File.Exists (setInfo.thumbnail))
				? setInfo.thumbnail
				: Application.dataPath + "/../AssetBundles/" + assetBundleName + ".png";

			string suffix = "set";

			WWWForm form = new WWWForm ();

			form.AddField ("creator", creatorID);
			form.AddField ("name", setInfo.setName);
			form.AddField ("attribution", setInfo.attribution);
			form.AddField ("bundle_name", assetBundleName);

			string hash = CalculateHash ("POST", suffix, form);

			if (File.Exists (bundle_windows)) form.AddBinaryData ("bundle_windows2019", File.ReadAllBytes (bundle_windows), assetBundleName, "application/octet-stream");
			if (File.Exists (bundle_android)) form.AddBinaryData ("bundle_android", File.ReadAllBytes (bundle_android), assetBundleName + "_android", "application/octet-stream");
			if (File.Exists (thumbnail)) form.AddBinaryData ("thumbnail", File.ReadAllBytes (thumbnail), Path.GetFileName (thumbnail), "application/octet-stream");

			var req = PostRequestForSuffix (suffix, form, hash);
			req.timeout = 0;

			requests[req] = (UnityWebRequest www) => {
				PublishResponse res;

				try {
					res = JsonUtility.FromJson<PublishResponse> (www.downloadHandler.text);
				} catch (Exception e) {
					if (OnError != null) OnError ("JSON parse error: " + e.Message + ", body: " + www.downloadHandler.text);
					return;
				}

				if (!res.success) {
					if (OnError != null) OnError (res.error);
				} else {
					FileInfo fi = new FileInfo (bundle_windows);
					PlayerPrefs.SetString ("published:" + assetBundleName, fi.LastWriteTime.ToString ("O"));
					callback (res.data);
				}
			};

			req.SendWebRequest ();
		}

		/// <summary>
		/// Publish a set. Callback receives the set ID.
		/// </summary>
		public static void PublishPropKit (Action<int> callback) {
			Init ();

			if (creatorID == 0) {
				if (OnError != null) OnError ("Flipside creator ID not set. Please log in and try again.");
				return;
			}

			if (!HasToken ()) {
				if (OnError != null) OnError ("No publishing token acquired. Please log in and try again.");
			}

			Scene scene = SceneManager.GetActiveScene ();
			PropKit propKit = GetComponentOnRootElement<PropKit> (scene);

			if (propKit == null) {
				if (OnError != null) OnError ("SetInfo component not found on root scene object.");
				return;
			}

			if (propKit.name.Trim (' ') == "") {
				if (OnError != null) OnError ("Please add a set name in the SetInfo component.");
				return;
			}

			string assetBundleName = AssetImporter.GetAtPath (scene.path).assetBundleName;
			string bundle_windows = Application.dataPath + "/../AssetBundles/" + assetBundleName;
			string bundle_android = Application.dataPath + "/../AssetBundles/" + assetBundleName + "_android";
			string thumbnail = (propKit.thumbnail != "" && File.Exists (propKit.thumbnail))
				? propKit.thumbnail
				: Application.dataPath + "/../AssetBundles/" + assetBundleName + ".png";
			string props = JsonUtility.ToJson (propKit.propList);

			string suffix = "propkit";

			WWWForm form = new WWWForm ();

			form.AddField ("creator", creatorID);
			form.AddField ("name", propKit.kitName);
			form.AddField ("attribution", propKit.attribution);
			form.AddField ("bundle_name", assetBundleName);

			// Build list of prop names, one per line
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < propKit.propList.Length; i++) {
				PropInfo prop = propKit.propList[i];

				sb.Append (prop.displayName.Replace ('[', ' ').Replace (']', ' ').Replace ('"', '\''));
				sb.Append ("\n");
			}
			form.AddField ("props", sb.ToString ());

			string hash = CalculateHash ("POST", suffix, form);

			if (File.Exists (bundle_windows)) form.AddBinaryData ("bundle_windows2019", File.ReadAllBytes (bundle_windows), assetBundleName, "application/octet-stream");
			if (File.Exists (bundle_android)) form.AddBinaryData ("bundle_android", File.ReadAllBytes (bundle_android), assetBundleName + "_android", "application/octet-stream");
			if (File.Exists (thumbnail)) form.AddBinaryData ("thumbnail", File.ReadAllBytes (thumbnail), Path.GetFileName (thumbnail), "application/octet-stream");

			// Append each prop's thumbnail, if available. Uses prop
			// index to keep track of which they belong to
			for (int i = 0; i < propKit.propList.Length; i++) {
				PropInfo prop = propKit.propList[i];

				string propThumb = (prop.thumbnail != "" && File.Exists (prop.thumbnail))
						? prop.thumbnail
						: Application.dataPath + "/../AssetBundles/" + assetBundleName + "." + i + ".png";

				if (File.Exists (propThumb)) {
					form.AddBinaryData (string.Format ("prop_{0}", i), File.ReadAllBytes (propThumb), Path.GetFileName (propThumb), "application/octet-stream");
				}
			}

			var req = PostRequestForSuffix (suffix, form, hash);
			req.timeout = 0;

			requests[req] = (UnityWebRequest www) => {
				PublishResponse res;

				try {
					res = JsonUtility.FromJson<PublishResponse> (www.downloadHandler.text);
				} catch (Exception e) {
					if (OnError != null) OnError ("JSON parse error: " + e.Message + ", body: " + www.downloadHandler.text);
					return;
				}

				if (res == null) {
					if (OnError != null) OnError ("Unknown response from server.");
				} else if (!res.success) {
					if (OnError != null) OnError (res.error);
				} else {
					FileInfo fi = new FileInfo (bundle_windows);
					PlayerPrefs.SetString ("published:" + assetBundleName, fi.LastWriteTime.ToString ("O"));
					callback (res.data);
				}
			};

			req.SendWebRequest ();
		}

		private static void EditorUpdate () {
			remove.Clear ();

			foreach (var req in requests) {
				var www = req.Key;

				if (!www.isDone) {
					if (OnProgress != null) OnProgress (www.uploadProgress);
					continue;
				};

				if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
					if (OnError != null) OnError ("Network error. Check your internet connection and try again.");
					remove.Add (www);
					continue;
				}

				try {
					req.Value (www);
				} catch (Exception e) {
					if (OnError != null) OnError (e.Message);
				}
				remove.Add (www);
			}

			foreach (UnityWebRequest www in remove) {
				requests.Remove (www);
			}
		}

		private static void Init () {
			if (initialized) return;

			EditorApplication.update += EditorUpdate;

			string fsid = Application.persistentDataPath + "/.fsid";

			if (File.Exists (fsid)) {
				string data = File.ReadAllText (fsid);
				Int32.TryParse (data, out creatorID);
			}

			string file = Application.persistentDataPath + "/.token";

			if (File.Exists (file)) {
				string data = File.ReadAllText (file);
				string[] list = data.Split (':');
				hmacToken = list[0];
				hmacSecret = list[1];
			}

			initialized = true;
		}

		public static bool HasToken () {
			Init ();

			return (creatorID != 0 && hmacToken != "");
		}

		public static void ClearToken () {
			string fsid = Application.persistentDataPath + "/.fsid";

			if (File.Exists (fsid)) {
				File.Delete (fsid);
			}

			string file = Application.persistentDataPath + "/.token";

			if (File.Exists (file)) {
				File.Delete (file);
			}

			hmacToken = "";
			hmacSecret = "";
			creatorID = 0;
		}

		private static UnityWebRequest PostRequestForSuffix (string suffix, WWWForm form, string hash = "") {
			if (hash == "") hash = CalculateHash ("POST", suffix, form);
			UnityWebRequest www = UnityWebRequest.Post (authEndpoint + suffix, form);
			www.SetRequestHeader ("Authorization", Authify (hash));
			www.timeout = requestTimeout;
			return www;
		}

		private static string CalculateHash (string method, string uri, WWWForm formData = null) {
			string data = method + hmacPrefix + uri;
			string hash = "";

			if (method != "GET" && formData != null) {
				data += enc.GetString (formData.data).Replace ("%20", "+").Replace ("%28", "(").Replace ("%29", ")");
			}

			data = data.ToLower ();

			HMACSHA256 hmac = new HMACSHA256 (enc.GetBytes (hmacSecret));
			byte[] bytes = hmac.ComputeHash (enc.GetBytes (data));

			for (int i = 0; i < bytes.Length; i++) {
				hash += bytes[i].ToString ("x2"); // hex
			}

			return hash;
		}

		private static string CalculateHash (UnityWebRequest www, string uri, WWWForm formData = null) {
			return CalculateHash (www.method, uri, formData);
		}

		private static string Authify (string user, string pass) {
			return "Basic " + Convert.ToBase64String (enc.GetBytes (user + ":" + pass));
		}

		private static string Authify (string pass) {
			return Authify (hmacToken, pass);
		}

		private static string AuthorizationHeader (UnityWebRequest www, string uri, WWWForm formData = null) {
			return Authify (hmacToken, CalculateHash (www, uri, formData));
		}
	}
}