/**
 * Copyright (c) 2021 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using TMPro;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	public class TeleprompterElement : MonoBehaviour {
		public TextMeshPro displayText;

		[Tooltip ("If disabled, the text won't auto-scroll in Flipside Studio but will instead update the text as each line advances.")]
		public bool autoScroll = true;

		[Tooltip ("Override the auto-calculated line height with a custom value here, or leave it at 0 to auto-calculate in Flipside Studio.")]
		public float lineHeight = 0f;

		private string sampleScript = "This is a sample teleprompter script to\nserve as a preview in the Unity editor.\n\nThis will automatically be replaced by\nyour real teleprompter script in\nFlipside Studio.\n\nThe teleprompter displays 40 characters\nper line and 10 lines on screen at one\ntime.";

		private void Reset () {
			Setup ();
		}

		private void OnEnable () {
			Setup ();
		}

		public void Setup () {
			if (displayText == null) {
				displayText = GetComponent<TextMeshPro> ();

				if (displayText == null) {
					displayText = GetComponentInChildren<TextMeshPro> (true);

					if (displayText == null) {
						Debug.LogError ("TeleprompterElement couldn't find a TextMeshPro component.");
						return;
					}
				}
			}

			displayText.font = Resources.Load ("Fonts/Anonymous Pro SDF") as TMP_FontAsset;
			displayText.richText = true;
			displayText.enableWordWrapping = false;
			displayText.overflowMode = TextOverflowModes.Truncate;
			displayText.text = sampleScript;
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (TeleprompterElement))]
	public class TeleprompterElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			TeleprompterElement te = (TeleprompterElement) target;

			if (GUILayout.Button ("Setup")) {
				te.Setup ();
			}
		}
	}

#endif
}