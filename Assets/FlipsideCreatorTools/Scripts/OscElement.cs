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
using UnityEngine;
using UnityEngine.Events;

#if EXTOSC

using extOSC;

#endif

#if UNITY_EDITOR

using UnityEditor;
using Flipside.Helpers;

#endif

namespace Flipside.Sets {

	[Serializable]
	public class OscIntEvent : UnityEvent<int> { }

	[Serializable]
	public class OscFloatEvent : UnityEvent<float> { }

	[Serializable]
	public class OscLongEvent : UnityEvent<long> { }

	[Serializable]
	public class OscDoubleEvent : UnityEvent<double> { }

	[Serializable]
	public class OscBooleanEvent : UnityEvent<bool> { }

	[Serializable]
	public class OscStringEvent : UnityEvent<string> { }

	[Serializable]
	public class OscCharEvent : UnityEvent<char> { }

	/// <summary>
	/// Connects events in sets to OSC messages so OSC-compatible controllers can trigger
	/// effects within sets.
	/// </summary>
	public class OscElement : MonoBehaviour {
		public string messageAddress = "/message/address";

		[Tooltip ("Whether to broadcast events over multiplayer. Leave this unchecked if used in combo with FlipsideActions, which already broadcast themselves.")]
		public bool broadcast = false;

		//public int valueIndex = 0;

		[SerializeField]
		public OscIntEvent OnIntValue;

		[SerializeField]
		public OscFloatEvent OnFloatValue;

		[SerializeField]
		public OscLongEvent OnLongValue;

		[SerializeField]
		public OscDoubleEvent OnDoubleValue;

		[SerializeField]
		public OscBooleanEvent OnBooleanValue;

		[SerializeField]
		public OscStringEvent OnStringValue;

		[SerializeField]
		public OscCharEvent OnCharValue;

#if EXTOSC

		private static OSCReceiver receiver = null;
		private OSCBind bind;

#endif

		private float floatValue;
		private int intValue;
		private long longValue;
		private double doubleValue;
		private bool booleanValue;
		private string stringValue;
		private char charValue;

		private void Awake () {
			DontDestroyOnLoad (gameObject);
		}

#if EXTOSC

		private void OnEnable () {
			if (receiver == null) {
				CreateReceiver ();
			}

			bind = receiver.Bind (messageAddress, HandleMessageReceived);
		}

		private void OnDisable () {
			receiver.Unbind (bind);
		}

		private void OnDestroy () {
			if (receiver != null) {
				try {
					Destroy (receiver.gameObject);
				} catch { }
			}
		}

		private void CreateReceiver () {
			GameObject receiverObject = new GameObject ("OSCReceiver");
			DontDestroyOnLoad (receiverObject);

			receiver = receiverObject.AddComponent<OSCReceiver> ();
			receiver.LocalPort = 10001;
			receiver.AutoConnect = true;
			receiver.CloseOnPause = false;
			receiver.Connect ();
		}

		private void HandleMessageReceived (OSCMessage message) {
			//Debug.Log ("Message received: " + message.Address);

			if (OnIntValue != null) {
				if (message.ToInt (out intValue)) {
					OnIntValue.Invoke (intValue);
				}
			}

			if (OnFloatValue != null) {
				if (message.ToFloat (out floatValue)) {
					OnFloatValue.Invoke (floatValue);
				}
			}

			if (OnLongValue != null) {
				if (message.ToLong (out longValue)) {
					OnLongValue.Invoke (longValue);
				}
			}

			if (OnDoubleValue != null) {
				if (message.ToDouble (out doubleValue)) {
					OnDoubleValue.Invoke (doubleValue);
				}
			}

			if (OnBooleanValue != null) {
				if (message.ToBool (out booleanValue)) {
					OnBooleanValue.Invoke (booleanValue);
				}
			}

			if (OnStringValue != null) {
				if (message.ToString (out stringValue)) {
					OnStringValue.Invoke (stringValue);
				}
			}

			if (OnCharValue != null) {
				if (message.ToChar (out charValue)) {
					OnCharValue.Invoke (charValue);
				}
			}
		}

#endif

		#region Test Methods

		public void TestIntReceived (int val) {
			Debug.Log ("Received: " + val);
		}

		public void TestFloatReceived (float val) {
			Debug.Log ("Received: " + val);
		}

		public void TestLongValue (long val) {
			Debug.Log ("Received: " + val);
		}

		public void TestDoubleValue (double val) {
			Debug.Log ("Received: " + val);
		}

		public void TestBooleanValue (bool val) {
			Debug.Log ("Received: " + val);
		}

		public void TestStringValue (string val) {
			Debug.Log ("Received: " + val);
		}

		public void TestCharValue (char val) {
			Debug.Log ("Received: " + val);
		}

		#endregion Test Methods
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (OscElement))]
	public class OscElementEditor : Editor {
		private string ip = "";

		public override void OnInspectorGUI () {
			if (ip == "") {
				ip = NetworkInfo.IPAddress ();
			}

			GUIStyle infoStyle = EditorStyles.label;
			infoStyle.wordWrap = true;

			GUILayout.Space (10);

#if !EXTOSC

			GUILayout.Label ("Install extOSC to test OSC inputs in play mode in the Unity editor.", infoStyle);
			if (GUILayout.Button ("Visit extOSC on GitHub")) {
				Application.OpenURL ("https://github.com/Iam1337/extOSC");
			}
			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));

#endif
			GUILayout.Label ("To test in editor, press play and connect to " + ip + " on port 10001", infoStyle);
			EditorGUILayoutUtility.HorizontalLine (new Vector2 (3f, 3f));
			GUILayout.Space (10);

			base.OnInspectorGUI ();
		}
	}

#endif
}