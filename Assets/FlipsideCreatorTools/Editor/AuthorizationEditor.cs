/**
 * Copyright (c) 2021 The Campfire Union Inc - All Rights Reserved.
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
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Flipside {

	public class AuthorizationEditor : EditorWindow {
		private static AuthorizationEditor window;

		private string username = "";
		private string password = "";
		private static bool requestPending = false;
		private static bool callbacksRegistered = false;
		private static string msg = "";

		[MenuItem ("Flipside Creator Tools/Sign Into Flipside Account", false, 3)]
		public static void ShowSignInWindow () {
			if (!callbacksRegistered) {
				ApiClient.OnError += ErrorCallback;
				callbacksRegistered = true;
			}

			window = (AuthorizationEditor) EditorWindow.GetWindow (typeof (AuthorizationEditor), true, "Sign Into Flipside Account");
			window.Show ();
		}

		private void OnInspectorUpdate () {
			Repaint ();
		}

		private void OnGUI () {
			if (!ApiClient.HasToken ()) {
				ShowLoginForm ();
			} else {
				ShowLogOutPage ();
			}
		}

		private void ShowLoginForm () {
			GUILayout.Label ("Email Address:");

			username = GUILayout.TextField (username);

			GUILayout.Space (2);

			GUILayout.Label ("Password:");

			password = GUILayout.PasswordField (password, '*');

			GUILayout.Space (5);

			if (!requestPending) {
				if (GUILayout.Button ("Sign In")) {
					if (username == "") {
						msg = "Please enter your email address.";
					} else if (password == "") {
						msg = "Please enter your password.";
					} else {
						msg = "";
						requestPending = true;
						ApiClient.FetchToken (username, password, FetchTokenCallback);
					}
				}
			} else {
				if (GUILayout.Button ("Authenticating...")) {
					// Do nothing
				}
			}

			GUILayout.Space (5);

			if (msg != "") {
				GUILayout.Label (msg);
			}
		}

		private static void ErrorCallback (string error) {
			if (requestPending) {
				msg = error;
				requestPending = false;
			}
		}

		private void FetchTokenCallback (int creatorID) {
			requestPending = false;
		}

		private void ShowLogOutPage () {
			int creatorID = ApiClient.GetCreatorID ();

			GUILayout.Space (5);

			GUILayout.Label ("Creator ID: " + creatorID);

			GUILayout.Space (5);

			if (GUILayout.Button ("Sign Out")) {
				SignOut ();
			}
		}

		public static void SignOut () {
			ApiClient.ClearToken ();
		}
	}
}