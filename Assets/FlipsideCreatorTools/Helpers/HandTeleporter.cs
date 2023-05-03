/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
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
using UnityEngine.AI;
using UnityEngine.XR;
using Flipside.Sets;

namespace Flipside.Helpers {

	public class HandTeleporter : MonoBehaviour {
		public PlayerController player;

		private VRTeleporter teleporter;

		private float threshold = 0.02f;

		private float targetRange = 1f;

		private bool teleporterActive = false;

		private TeleportTarget currentTarget = null;
		private int currentAudienceSeat = -1;
		private Transform highlightedTransform = null;
		private int highlightedSeat = -1;
		private GameObject targetHighlighter;
		private static List<Transform> targets = new List<Transform> ();

		private void Awake () {
			GameObject teleporterResource = Resources.Load ("Teleporter") as GameObject;
			GameObject teleporterInstance = GameObject.Instantiate (teleporterResource, transform);
			teleporter = teleporterInstance.GetComponent<VRTeleporter> ();

			teleporter.bodyTransform = player.trackingSpace.transform;

			GameObject targetHighlighterResource = Resources.Load ("TeleportHighlighter") as GameObject;
			targetHighlighter = GameObject.Instantiate (targetHighlighterResource);
			targetHighlighter.SetActive (false);

			if (XRSettings.loadedDeviceName == "OpenVR") {
				teleporter.angle = 0f;
			}
		}

		private void OnEnable () {
			player.rightHand.OnTriggerDown.AddListener (HandleTriggerDown);
		}

		private void OnDisable () {
			player.rightHand.OnTriggerDown.RemoveListener (HandleTriggerDown);
		}

		private void Update () {
			Vector2 axisValue = player.rightHand.GetPrimaryAxisValue ();
			if (!teleporterActive && Mathf.Abs (axisValue.x) >= threshold && Mathf.Abs (axisValue.y) >= threshold) {
				teleporterActive = true;
				teleporter.ToggleDisplay (teleporterActive);
			} else if (teleporterActive && axisValue.x == 0f && axisValue.y == 0f) {
				teleporterActive = false;
				teleporter.ToggleDisplay (teleporterActive);
			}

			if (teleporterActive) {
				teleporter.SetRotationFromAxisValues (axisValue);

				// Check if we should highlight a TeleportElement
				bool targetActive = false;

				foreach (var trans in targets) {
					if (TargetInRange (trans)) {
						HighlightTarget (trans.position);
						highlightedTransform = trans;
						highlightedSeat = -1;
						targetActive = true;
						break;
					}
				}

				// Check if we should highlight an audience seat position
				if (!targetActive && Audience.Instance != null) {
					for (int i = 0; i < Audience.Instance.seats.Length; i++) {
						Vector3 pos = Audience.Instance.GetSeatPosition (i);
						if (pos != null && TargetInRange (pos)) {
							HighlightTarget (pos);
							highlightedTransform = null;
							highlightedSeat = i;
							targetActive = true;
							break;
						}
					}
				}

				if (!targetActive) {
					highlightedTransform = null;
					highlightedSeat = -1;
					HideTargetHighlight ();
				}
			}
		}

		public static void RegisterTeleportTarget (Transform target) {
			if (!targets.Contains (target)) {
				targets.Add (target);
			}
		}

		public static void UnregisterTeleportTarget (Transform target) {
			if (targets.Contains (target)) {
				targets.Remove (target);
			}
		}

		private bool TargetInRange (Transform trans) {
			return TargetInRange (trans.position);
		}

		private bool TargetInRange (Vector3 pos) {
			return (teleporter.positionMarker.transform.InverseTransformPoint (pos).magnitude < targetRange);
		}

		private void HighlightTarget (Vector3 pos) {
			targetHighlighter.transform.position = pos;
			targetHighlighter.SetActive (true);
		}

		private void HideTargetHighlight () {
			targetHighlighter.SetActive (false);
		}

		private void HandleTriggerDown () {
			if (!teleporterActive) return;

			if (currentTarget != null) {
				currentTarget.leavingEvent.Invoke ();
				currentTarget = null;
			}

			currentAudienceSeat = -1;

			if (highlightedTransform != null) {
				TeleportTo (highlightedTransform);
				highlightedTransform = null;
			} else if (highlightedSeat != -1) {
				Audience.Instance.TeleportIntoSeat (highlightedSeat);
			} else {
				teleporter.Teleport ();
			}
		}

		public void TeleportTo (Transform target) {
			player.trackingSpace.transform.position = target.position;
			player.trackingSpace.transform.rotation = target.rotation;
			player.trackingSpace.transform.localScale = target.localScale;

			if (currentTarget != null) {
				currentTarget.leavingEvent.Invoke ();
			}

			currentAudienceSeat = -1;
			currentTarget = target.GetComponent<TeleportTarget> ();

			if (currentTarget != null) {
				currentTarget.teleportEvent.Invoke ();
			}
		}

		public void TeleportTo (Vector3 position, Quaternion rotation, float scale) {
			player.trackingSpace.transform.position = position;
			player.trackingSpace.transform.rotation = rotation;
			player.trackingSpace.transform.localScale = Vector3.one * scale;
		}

		public void SetAudienceSeatNumber (int seatNumber) {
			currentAudienceSeat = seatNumber;
			Audience.Instance.OnMoved.AddListener (HandleAudienceMovement);
		}

		public void UnsetAudienceSeatNumber () {
			currentAudienceSeat = -1;
			Audience.Instance.OnMoved.RemoveListener (HandleAudienceMovement);
		}

		private void HandleAudienceMovement () {
			if (currentAudienceSeat != -1) {
				Audience.Instance.UpdateTeleportPosition (this, currentAudienceSeat);
			}
		}
	}
}