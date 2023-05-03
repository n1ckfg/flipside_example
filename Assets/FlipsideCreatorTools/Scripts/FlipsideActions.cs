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
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	/// <summary>
	/// Triggers internal actions within Flipside in custom sets.
	/// </summary>
	public class FlipsideActions : MonoBehaviour {

		[Tooltip ("Trigger events before the set has been shown to users.")]
		public UnityEvent OnPreload = new UnityEvent ();

		[Tooltip ("Trigger events timed to the set being shown to users.")]
		public UnityEvent OnDisplay = new UnityEvent ();

		[Tooltip ("Trigger events timed to the set being hidden from users.")]
		public UnityEvent OnHide = new UnityEvent ();

		[Tooltip ("Trigger events at the start of a Flipside recording.")]
		public UnityEvent OnStart = new UnityEvent ();

		[Tooltip ("Trigger events at a Flipside recording being paused.")]
		public UnityEvent OnPause = new UnityEvent ();

		[Tooltip ("Trigger events at a Flipside recording being resumed.")]
		public UnityEvent OnResume = new UnityEvent ();

		[Tooltip ("Trigger events at a Flipside recording being stopped.")]
		public UnityEvent OnStop = new UnityEvent ();

		[Tooltip ("Trigger events at a Flipside recording being rewound.")]
		public UnityEvent OnRewind = new UnityEvent ();

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private void Awake () {
			Debug.Log ("Invoking OnPreload event");
			OnPreload.Invoke ();
		}

		private void OnEnable () {
			Debug.Log ("Invoking OnEnable event");
			OnDisplay.Invoke ();
		}

		private void OnDisable () {
			Debug.Log ("Invoking OnHide event");
			OnHide.Invoke ();
		}

		private void OnDestroy () {
			Debug.Log ("Invoking OnStop event");
			OnStop.Invoke ();
		}

		private bool playing = false;
		private bool started = false;

#endif

#if UNITY_EDITOR

		private void Update () {
			if (Input.GetKeyDown (KeyCode.Space)) {
				UpdatePlaybackState ();
			}

			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				UpdateRewindState ();
			}
		}

		private void UpdatePlaybackState () {
			if (!playing && !started) {
				Debug.Log ("Invoking OnStart event");
				OnStart.Invoke ();
				started = true;
				playing = true;
			} else if (!playing && started) {
				Debug.Log ("Invoking OnResume event");
				OnResume.Invoke ();
				playing = true;
			} else {
				Debug.Log ("Invoking OnPause event");
				OnPause.Invoke ();
				playing = false;
			}
		}

		private void UpdateRewindState () {
			Debug.Log ("Invoking OnRewind event");
			OnRewind.Invoke ();

			if (playing) {
				Debug.Log ("Invoking OnStart event");
				OnStart.Invoke ();
			} else {
				started = false;
			}
		}

#endif

		public void ShowNextSlide () {
			Debug.Log ("SHOW NEXT SLIDE");
		}

		public void ShowPreviousSlide () {
			Debug.Log ("SHOW PREVIOUS SLIDE");
		}

		public void ShowFirstSlide () {
			Debug.Log ("SHOW FIRST SLIDE");
		}

		public void ShowLastSlide () {
			Debug.Log ("SHOW LAST SLIDE");
		}

		public void ShowNextTeleprompterPage () {
			Debug.Log ("SHOW NEXT PAGE");
		}

		public void ShowPreviousTeleprompterPage () {
			Debug.Log ("SHOW PREVIOUS PAGE");
		}

		public void ShowFirstTeleprompterPage () {
			Debug.Log ("SHOW FIRST PAGE");
		}

		public void ShowLastTeleprompterPage () {
			Debug.Log ("SHOW LAST PAGE");
		}

		public void StartTeleprompterAutoscroll () {
			Debug.Log ("START AUTOSCROLL");
		}

		public void StopTeleprompterAutoscroll () {
			Debug.Log ("STOP AUTOSCROLL");
		}

		public void ToggleTeleprompterAutoscroll () {
			Debug.Log ("TOGGLE AUTOSCROLL");
		}

		public void CutToCamera (int num) {
			Debug.Log ("CUT TO CAMERA: " + num);
		}

		public void MoveToCamera (int num) {
			Debug.Log ("MOVE TO CAMERA: " + num);
		}

		public void CutToPOV (int num) {
			Debug.Log ("CUT TO POV: " + num);
		}

		/// <summary>
		/// 0 = Cut
		/// 1 = Move
		/// </summary>
		public void SetCameraMode (int mode) {
			Debug.Log ("CHANGE CAMERA MODE TO: " + mode);
		}

		public void TransitionToCamera (int num) {
			Debug.Log ("TRANSITION TO CAMERA: " + num);
		}

		public void SetCameraSpeed (float speed) {
			Debug.Log ("SETTING CAMERA SPEED TO: " + speed);
		}

		public void ChangeSet (int id) {
			Debug.Log ("CHANGE SET TO: " + id);
		}

		public void PreloadSet (int id) {
			Debug.Log ("PRELOAD SET: " + id);
		}

		public void ResetProps () {
			Debug.Log ("RESETTING PROPS");
		}

		public void MirrorDesktop (int val) {
			Debug.Log ("MIRROR DESKTOP: " + val);
		}

		public void SetAvatar (int id) {
			Debug.Log ("SET AVATAR: " + id);
		}

		public void TeleportUser (Transform target) {
			var teleporter = Flipside.Helpers.PlayerController.Instance.rightHand.GetComponent<Flipside.Helpers.HandTeleporter> ();
			teleporter.TeleportTo (target);
		}

		public void TriggerLeftHandHaptics (float val = 0.3f) {
			Flipside.Helpers.PlayerController.Instance.leftHand.TriggerHaptics (val);
		}

		public void TriggerRightHandHaptics (float val = 0.3f) {
			Flipside.Helpers.PlayerController.Instance.rightHand.TriggerHaptics (val);
		}

		public void MuteUser (string userId) {
			Debug.Log ("MUTE USER: " + userId);
		}

		public void UnMuteUser (string userId) {
			Debug.Log ("UNMUTE USER: " + userId);
		}

		public void MuteUserToLiveStream (string userId) {
			Debug.Log ("MUTE USER TO LIVE STREAM: " + userId);
		}

		public void UnMuteUserToLiveStream (string userId) {
			Debug.Log ("UNMUTE USER TO LIVE STREAM: " + userId);
		}

		/*public void PlayRecording (int id) {
			Debug.Log ("PLAY RECORDING: " + id);
		}

		public void StartRecording () {
			Debug.Log ("START RECORDING");
		}

		public void StopRecording () {
			Debug.Log ("STOP RECORDING");
		}*/

		public void StartLiveStreaming () {
			Debug.Log ("START LIVE STREAMING");
		}

		public void StopLiveStreaming () {
			Debug.Log ("STOP LIVE STREAMING");
		}

		public void ShowOverlay (string link) {
			Debug.Log ("SHOW OVERLAY: " + link);
		}

		public void HideOverlay () {
			Debug.Log ("HIDE OVERLAY");
		}

		public void EnsurePreloaded (string link) {
			Debug.Log ("ENSURE PRELOADED: " + link);
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (FlipsideActions))]
	public class FlipsideActionsEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			FlipsideActions fa = (FlipsideActions) target;

			GUILayout.Space (10);

			if (GUILayout.Button ("Fire Preload Event")) {
				fa.OnPreload.Invoke ();
			}

			if (GUILayout.Button ("Fire Display Event")) {
				fa.OnDisplay.Invoke ();
			}

			if (GUILayout.Button ("Fire Hide Event")) {
				fa.OnHide.Invoke ();
			}

			if (GUILayout.Button ("Fire Start Event")) {
				fa.OnStart.Invoke ();
			}

			if (GUILayout.Button ("Fire Pause Event")) {
				fa.OnPause.Invoke ();
			}

			if (GUILayout.Button ("Fire Resume Event")) {
				fa.OnResume.Invoke ();
			}

			if (GUILayout.Button ("Fire Stop Event")) {
				fa.OnStop.Invoke ();
			}

			if (GUILayout.Button ("Fire Rewind Event")) {
				fa.OnRewind.Invoke ();
			}
		}
	}

#endif
}