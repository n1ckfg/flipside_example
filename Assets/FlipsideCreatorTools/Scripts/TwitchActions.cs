using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	[Serializable]
	public class TwitchCommand {

		[Tooltip ("Define your custom command string, e.g., 'ding' so users can type '!ding' to trigger the associated event.")]
		public string command;

		public UnityEvent OnCommand;
	}

	public class TwitchActions : MonoBehaviour {

		[SerializeField]
		public TwitchCommand[] commands;

		private void Start () {
			// Ensure they're all lowercase and missing the '/' prefix
			for (int i = 0; i < commands.Length; i++) {
				TwitchCommand command = commands[i];
				string cmd = command.command.ToLower ();
				if (cmd.StartsWith ("!")) {
					cmd = cmd.Substring (1);
				}
				commands[i].command = cmd;
			}
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (TwitchActions))]
	public class TwitchActionsEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			TwitchActions ta = (TwitchActions) target;

			if (ta.commands == null) return;

			for (int i = 0; i < ta.commands.Length; i++) {
				if (GUILayout.Button ("Fire !" + ta.commands[i].command + " Command")) {
					ta.commands[i].OnCommand.Invoke ();
				}
			}
		}
	}

#endif
}