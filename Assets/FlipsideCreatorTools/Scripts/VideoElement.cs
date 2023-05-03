/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	/// <summary>
	/// Creates functioning embedded video players in sets which can also be
	/// controlled by triggering the various public methods in buttons,
	/// colliders, and toggles.
	/// </summary>
	public class VideoElement : MonoBehaviour {

		[Tooltip ("Hard-coded list of videos")]
		public string[] videoList;

		[Tooltip ("Link to a text file with a list of videos in it")]
		public string videosLink;

		[Tooltip ("Whether the first video should play on start or wait for Play to be called")]
		public bool autoPlayFirstVideo = true;

		[Tooltip ("Whether the video playback should loop automatically")]
		public bool loop = true;

		[Tooltip ("Whether the video playback should loop the current video")]
		public bool loopSingle = false;

		public float audioVolume = 1f;

		public enum GammaConversion { NoConversion, GammaToLinear, LinearToGamma }

		[Tooltip ("Whether to convert colour of video between gamma and linear")]
		public GammaConversion gammaConversion = GammaConversion.NoConversion;

		private int current = 0;
		private bool playFirst = false;
		private VideoPlayer videoPlayer;
		private AudioSource audioSource;
		private FlipsideApi api;

		private void Awake () {
			videoPlayer = gameObject.AddComponent<VideoPlayer> ();
			audioSource = gameObject.AddComponent<AudioSource> ();

			videoPlayer.playOnAwake = false;
			audioSource.playOnAwake = false;

			audioSource.volume = audioVolume;

			videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
			videoPlayer.EnableAudioTrack (0, true);
			videoPlayer.SetTargetAudioSource (0, audioSource);

			videoPlayer.loopPointReached += HandleEndReached;
			videoPlayer.prepareCompleted += HandlePrepared;

			api = gameObject.AddComponent<FlipsideApi> ();
		}

		private void OnEnable () {
			if (videosLink != "") {
				api.ExpireCache (videosLink);
				api.DownloadFile (videosLink, LoadSlidesFromFile);
			} else if (videoList.Length > 0) {
				api.DownloadFile (videoList[current], LoadVideo);
			}
		}

		private void LoadSlidesFromFile (string path) {
			videoList = File.ReadAllLines (path);

			for (int i = 0; i < videoList.Length; i++) {
				if (i == 0) {
					api.DownloadFile (videoList[i], LoadVideo);
				} else {
					api.DownloadFile (videoList[i]);
				}
			}
		}

		private void LoadVideo (string path) {
			videoPlayer.isLooping = loopSingle;
			videoPlayer.url = path;
			videoPlayer.controlledAudioTrackCount = 1;
			videoPlayer.SetTargetAudioSource (0, audioSource);
			videoPlayer.Prepare ();
		}

		private void HandleEndReached (VideoPlayer vp) {
			if (!loop && !loopSingle) return;

			if (loopSingle || videoList.Length == 1) {
				videoPlayer.time = 0f;
				videoPlayer.Play ();
			} else {
				Next ();
			}
		}

		private void HandlePrepared (VideoPlayer vp) {
			if (current > 0 || autoPlayFirstVideo || playFirst) {
				videoPlayer.Play ();
				playFirst = false;
			}
		}

		public void Next () {
			if (videoList.Length == 0) return;

			current++;
			if (current >= videoList.Length) {
				current = 0;
			}

			if (current == 0) playFirst = true;

			api.DownloadFile (videoList[current], LoadVideo);
		}

		public void Previous () {
			if (videoList.Length == 0) return;

			current--;
			if (current < 0) {
				current = videoList.Length - 1;
			}

			if (current == 0) playFirst = true;

			api.DownloadFile (videoList[current], LoadVideo);
		}

		public void Pause () {
			videoPlayer.Pause ();
		}

		public void Play () {
			videoPlayer.Play ();
		}

		public void Play (int num) {
			if (videoList.Length == 0) return;
			if (num < 0 || num >= videoList.Length) return;

			current = num;
			api.DownloadFile (videoList[current], LoadVideo);
		}

		public void Last () {
			if (videoList.Length == 0) return;

			current = videoList.Length - 1;

			if (current == 0) playFirst = true;

			api.DownloadFile (videoList[current], LoadVideo);
		}

		public void First () {
			if (videoList.Length == 0) return;

			current = 0;
			api.DownloadFile (videoList[current], LoadVideo);
		}

		public void Volume (float val) {
			audioSource.volume = val;
		}

		public void SetPlaybackSpeed (float newSpeed) {
			if (newSpeed < 0) newSpeed = 0;
			if (newSpeed > 4) newSpeed = 4;
			videoPlayer.playbackSpeed = newSpeed;
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (VideoElement))]
	public class VideoElementEditor : Editor {
		private AudioSource audioSource;

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			if (!EditorApplication.isPlaying) return;

			VideoElement ve = (VideoElement) target;
			if (audioSource == null) audioSource = ve.GetComponent<AudioSource> ();

			GUILayout.Space (10f);

			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("Previous")) {
				ve.Previous ();
			}

			if (GUILayout.Button ("Next")) {
				ve.Next ();
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("Play")) {
				ve.Play ();
			}

			if (GUILayout.Button ("Pause")) {
				ve.Pause ();
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("Go To First")) {
				ve.First ();
			}

			if (GUILayout.Button ("Go To Last")) {
				ve.Last ();
			}

			GUILayout.EndHorizontal ();

			GUILayout.Label ("Volume");

			audioSource.volume = GUILayout.HorizontalSlider (audioSource.volume, 0f, 1f);
		}
	}

#endif
}
