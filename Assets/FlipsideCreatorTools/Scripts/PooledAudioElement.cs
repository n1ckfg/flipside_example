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

namespace Flipside.Sets {

	/// <summary>
	/// Positions an audio playback location on set that when triggered
	/// uses Flipside Studio's audio source pool so you can manage a
	/// larger number of audio sources without hitting Unity's limits.
	/// </summary>
	public class PooledAudioElement : MonoBehaviour {
		public AudioClip[] audioClips;

		public int autoPlayClip = -1;

		public bool loop = false;

		public bool reserve = false;

		[Range (0f, 1f)]
		public float volume = 1f;

		public AudioSource overrideAudioSource;

		[Space (10)]
		[Header ("Spatialization Settings")]
		public bool spatialized = true;

		public float gain = 0f;

		[Tooltip ("Range (0.0 - 1000000.0 meters")]
		public float near = 0.25f;

		[Tooltip ("Range (0.0 - 1000000.0 meters")]
		public float far = 100f;

		[Tooltip ("Range (0.0 - 1000.0 meters")]
		public float volumetricRadius = 0f;

		[Tooltip ("(-60.0 - 20.0 decibels)")]
		public float reverbSendLevel = 0f;

		[Serializable]
		public class VolumeChangeEvent : UnityEvent<float> { }

		public VolumeChangeEvent OnVolumeChange = new VolumeChangeEvent ();

		public UnityEvent OnVolumeOff = new UnityEvent ();

		#region Audio Pool

		private static PooledAudioElement Instance;
		private static AudioSource[] audioSources = new AudioSource[12];

		private AudioSource audioSource {
			get {
				if (overrideAudioSource != null) {
					return overrideAudioSource;
				}

				if (_audioSource == null || oneShot) { // One-shots always grab fresh from the pool
					_audioSource = GetAudioSourceFromPool ();
				}
				return _audioSource;
			}
		}

		private AudioSource _audioSource;
		private bool oneShot = false; // Is playing one-shots?

		private void Awake () {
			if (Instance == null) {
				Instance = this;
				InitializeAudioManager ();
			}

			if (overrideAudioSource) {
				CopyAudioSettings (overrideAudioSource);
			}

			if (autoPlayClip != -1) {
				Play (autoPlayClip);
			}
		}

		private void Update () {
			if (_audioSource != null && _audioSource.isPlaying) {
				_audioSource.transform.position = transform.position;
			}
		}

		private void InitializeAudioManager () {
			GameObject audioPool = new GameObject ("Audio Pool");
			for (int i = 0; i < audioSources.Length; i++) {
				GameObject pooledAudioSource = new GameObject ("Pooled Audio Source " + i);
				pooledAudioSource.transform.SetParent (audioPool.transform);
				audioSources[i] = pooledAudioSource.AddComponent<AudioSource> ();
			}
		}

		// For simulating in editor
		private AudioSource GetAudioSourceFromPool () {
			for (int i = 0; i < audioSources.Length; i++) {
				var src = audioSources[i];
				if (src == null) continue; // Not assigned yet
				if (src.isPlaying) continue; // In use
				return CopyAudioSettings (src);
			}

			return CopyAudioSettings (audioSources[0]);
		}

		private AudioSource CopyAudioSettings (AudioSource src) {
			src.volume = volume;
			src.loop = loop;
			src.transform.position = transform.position;
			src.transform.rotation = transform.rotation;
			src.spatialize = spatialized;
			src.spatialBlend = (spatialized) ? 1f : 0f;

			// NOTE: Spatialization requires Oculus spatializer, not simulated in editor

			return src;
		}

		#endregion Audio Pool

		public void Play () {
			Play (0);
		}

		public void Play (int index) {
			if (index < 0 || index >= audioClips.Length) return;
			if (audioClips[index] == null) return;

			oneShot = false;
			audioSource.clip = audioClips[index];
			audioSource.Play ();
		}

		public void Play (AudioClip clip) {
			oneShot = false;
			audioSource.clip = clip;
			audioSource.Play ();
		}

		public void PlayOneShot () {
			PlayOneShot (0);
		}

		public void PlayOneShot (int index) {
			if (index < 0 || index >= audioClips.Length) return;
			if (audioClips[index] == null) return;

			oneShot = true;
			audioSource.PlayOneShot (audioClips[index]);
		}

		public void PlayOneShot (AudioClip clip) {
			oneShot = true;
			audioSource.PlayOneShot (clip);
		}

		public void Pause () {
			audioSource.Pause ();
		}

		public void UnPause () {
			audioSource.UnPause ();
		}

		public void Stop () {
			audioSource.Stop ();
		}

		public void SetVolume (float volume) {
			this.volume = volume;
			audioSource.volume = volume;

			if (audioSource.volume == 0f) {
				OnVolumeOff.Invoke ();
			} else {
				OnVolumeChange.Invoke (audioSource.volume);
			}
		}

		public void SubtractVolume (float sub) {
			this.volume -= sub;
			audioSource.volume -= sub;

			if (audioSource.volume <= 0f) {
				OnVolumeOff.Invoke ();
			} else {
				OnVolumeChange.Invoke (audioSource.volume);
			}
		}

		public void AddVolume (float add) {
			this.volume += add;
			audioSource.volume += add;

			OnVolumeChange.Invoke (audioSource.volume);
		}
	}
}