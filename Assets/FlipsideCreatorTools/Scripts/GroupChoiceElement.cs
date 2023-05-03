/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	[Serializable]
	public class GroupChoiceOption {
		public string option = "";
		public UnityEvent OnChosen = new UnityEvent ();
	}

	/// <summary>
	/// Provides a means of letting the group of users present vote on an outcome.
	/// In simulation, the outcome is triggered immediately, but running in Flipside,
	/// the choice will be determined by which choice received the most votes.
	/// </summary>
	public class GroupChoiceElement : MonoBehaviour {
		[Tooltip ("How many seconds should users have to make a choice? Timer starts when the component is enabled.")]
		public float timeout = 10f;

		[Tooltip ("Which option should be chosen if no choice is made before the timeout?")]
		public int chooseOnTimeout = 1;

		[Tooltip ("For an even number of votes, should the first option automatically be chosen so building the ability to re-vote isn't necessary?")]
		public bool chooseFirstOnTie = true;

		[Tooltip ("The child of this object that contains the user interface of the chooser.")]
		public GameObject groupChoiceInterface;

		[SerializeField]
		public GroupChoiceOption[] options;

		[Space (10)]

		public UnityEvent OnChoiceMade = new UnityEvent ();
		public UnityEvent OnTie = new UnityEvent ();
		public TimerEvent OnTimer = new TimerEvent ();
		public UnityEvent OnResetVotes = new UnityEvent ();

		[Serializable]
		public class TimerEvent : UnityEvent<float> { }

		private bool choiceMade = false;

		private void OnEnable () {
			if (groupChoiceInterface != null) {
				groupChoiceInterface.SetActive (true);
			}
			StartCoroutine (Timer ());
		}

		private void OnDisable () {
			StopAllCoroutines ();
		}

		private IEnumerator Timer () {
			float timeRemaining = timeout;

			while (timeRemaining > 0f) {
				yield return null;

				timeRemaining -= Time.deltaTime;

				if (timeRemaining > 0f && !choiceMade) {
					OnTimer.Invoke (Mathf.Clamp01 (timeRemaining / timeout));
				} else {
					break;
				}
			}

			if (!choiceMade) {
				ChooseOption (chooseOnTimeout);
			}
		}

		public void ResetVotes () {
			OnResetVotes.Invoke ();
			choiceMade = false;

			if (enabled && gameObject.activeInHierarchy) {
				StopAllCoroutines ();
				StartCoroutine (Timer ());
			}

			if (groupChoiceInterface != null) {
				groupChoiceInterface.SetActive (true);
			}
		}

		public void ChooseOption (int num) {
			if (num < 0) return;
			if (num >= options.Length) return;
			if (options[num] == null) return;

			OnChoiceMade.Invoke ();
			options[num].OnChosen.Invoke ();
			choiceMade = true;
			if (groupChoiceInterface != null) {
				groupChoiceInterface.SetActive (false);
			}
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (GroupChoiceElement))]
	public class GroupChoiceElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			GroupChoiceElement gc = (GroupChoiceElement) target;

			GUILayout.Space (10);

			int i = 0;
			foreach (var option in gc.options) {
				if (GUILayout.Button ("Choose Option " + (i + 1).ToString ())) {
					gc.ChooseOption (i);
				}
				i++;
			}

			if (GUILayout.Button ("Fire Tie Event")) {
				gc.OnTie.Invoke ();
			}

			if (GUILayout.Button ("Reset Votes")) {
				gc.ResetVotes ();
			}
		}
	}

#endif
}