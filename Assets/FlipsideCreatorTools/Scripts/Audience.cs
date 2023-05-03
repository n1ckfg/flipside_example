/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
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
using Flipside.Helpers;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	[Serializable]
	public class AudiencePosition {

		[Tooltip ("Name of this position to cut to while live streaming in Flipside Studio.")]
		public string name;

		[Tooltip ("Move audience together to this position.")]
		public Vector3 position;

		[Tooltip ("Move audience together to this rotation. Note: We only support Y rotation so we don't make viewers sick!")]
		public float yRotation;

		[Tooltip ("Move audience together to this scale.")]
		public float scale = 1f;

		public AudiencePosition () {
		}

		public AudiencePosition (string name, Vector3 pos, float yrot, float scale) {
			this.name = name;
			this.position = pos;
			this.yRotation = yrot;
			this.scale = scale;
		}
	}

	[Serializable]
	public class SeatPosition {

		[Tooltip ("Position of this audience member (sitting or standing).")]
		public Vector3 position;

		[Tooltip ("Rotation of this audience member (sitting or standing). Note: We only support Y rotation so we don't make viewers sick!")]
		public float yRotation;

		public SeatPosition () {
		}

		public SeatPosition (Vector3 pos, float yrot) {
			this.position = pos;
			this.yRotation = yrot;
		}
	}

	public enum AudiencePlacementMode {
		HeadPosition = 1,
		PlayArea = 2
	}

	[ExecuteInEditMode]
	public class Audience : MonoBehaviour {
		public static Audience Instance;

		[Tooltip ("Define audience positions here and use MoveTo() and FadeTo() to move the audience to them")]
		[SerializeField]
		public AudiencePosition[] positions = new AudiencePosition[1] {
			new AudiencePosition ("Position 1", new Vector3 (0f, 0f, 5f), 180f, 1f)
		};

		[Tooltip ("Define audience member placements within the audience component")]
		public SeatPosition[] seats = new SeatPosition[5] {
			new SeatPosition (Vector3.zero, 0f),
			new SeatPosition (new Vector3 (0.75f, 0f, 0.1f), 0.1f),
			new SeatPosition (new Vector3 (-0.75f, 0f, 0.1f), 0.1f),
			new SeatPosition (new Vector3 (1.5f, 0f, 0.25f), 0.25f),
			new SeatPosition (new Vector3 (-1.5f, 0f, 0.25f), 0.25f)
		};

		[Tooltip ("How to align audience members in their positions (head position is best for sitting, play area for standing experiences)")]
		public AudiencePlacementMode alignSeatsBy = AudiencePlacementMode.HeadPosition;

		[Tooltip ("Trigger events whenever the audience moves. Note: Fires for each step of a move and once for a fade.")]
		[SerializeField]
		public UnityEvent OnMoved = new UnityEvent ();

		[Tooltip ("Trigger events whenever the audience has finished moving to a new position.")]
		[SerializeField]
		public UnityEvent OnMoveCompleted = new UnityEvent ();

		[Tooltip ("Trigger events when EnableTeleportOnSet() is called.")]
		[SerializeField]
		public UnityEvent OnTeleportOnSetEnabled = new UnityEvent ();

		[Tooltip ("Trigger events when DisableTeleportOnSet() is called.")]
		[SerializeField]
		public UnityEvent OnTeleportOnSetDisabled = new UnityEvent ();

		private float movementSpeed = 1f;
		private float fadeSpeed = 0.3f;
		private float maxMovementSpeed = 4f;
		private float maxFadeSpeed = 1f;
		private bool canTeleportOnSet = false;
		private BlinkEffect blink;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS

		private void Awake () {
			Instance = this;
		}

#endif

		public bool IsRotationOkay () {
			if (transform.localEulerAngles.x != 0f || transform.localEulerAngles.z != 0f) {
				return false;
			}
			return true;
		}

		public void FixRotation () {
			transform.localEulerAngles = new Vector3 (0f, transform.localEulerAngles.y, 0f);
		}

		public void Update () {
			if (!IsRotationOkay ()) FixRotation ();

#if UNITY_EDITOR
			// Press Shift + A + # to teleport into audience seats
			bool shift = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));
			bool a = Input.GetKey (KeyCode.A);

			if (shift && a && Input.GetKey (KeyCode.Alpha1)) {
				TeleportIntoSeat (0);
			} else if (shift && a && Input.GetKey (KeyCode.Alpha2)) {
				TeleportIntoSeat (1);
			} else if (shift && a && Input.GetKey (KeyCode.Alpha3)) {
				TeleportIntoSeat (2);
			} else if (shift && a && Input.GetKey (KeyCode.Alpha4)) {
				TeleportIntoSeat (3);
			} else if (shift && a && Input.GetKey (KeyCode.Alpha5)) {
				TeleportIntoSeat (4);
			}
#endif
		}

		public void TeleportIntoSeat (int seatNumber) {
#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS
			if (seatNumber < 0 || seatNumber >= seats.Length) return;

			var pos = seats[seatNumber];
			var r = transform.rotation * Quaternion.Euler (Vector3.up * pos.yRotation);
			var p = transform.TransformPoint (pos.position);

			var teleporter = Flipside.Helpers.PlayerController.Instance.rightHand.GetComponent<Flipside.Helpers.HandTeleporter> ();
			teleporter.TeleportTo (p, r, 1f);
			teleporter.SetAudienceSeatNumber (seatNumber);
#endif
		}

		public void UpdateTeleportPosition (HandTeleporter teleporter, int seatNumber) {
#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS
			if (seatNumber < 0 || seatNumber >= seats.Length) return;

			var pos = seats[seatNumber];
			var r = transform.rotation * Quaternion.Euler (Vector3.up * pos.yRotation);
			var p = transform.TransformPoint (pos.position);

			teleporter.TeleportTo (p, r, 1f);
#endif
		}

		public Vector3 GetSeatPosition (int seatNumber) {
			if (seatNumber < 0 || seatNumber >= seats.Length) return Vector3.zero;

			var pos = seats[seatNumber];
			return transform.TransformPoint (pos.position);
		}

		public void SetMovementSpeed (float speed) {
			if (speed == 0 || speed > maxMovementSpeed) {
				Debug.LogError ("Invalid movement speed, must be greater than 0 and less than " + maxMovementSpeed);
				return;
			}

			movementSpeed = speed;
		}

		public void SetFadeSpeed (float speed) {
			if (speed == 0 || speed > maxFadeSpeed) {
				Debug.LogError ("Invalid fade speed, must be greater than 0 and less than " + maxFadeSpeed);
				return;
			}

			fadeSpeed = speed;
		}

		public void MoveTo (int positionIndex = 0) {
			if (positionIndex < 0 || positionIndex >= positions.Length) {
				Debug.LogError ("Invalid audience position index");
				return;
			}

			if (positions[positionIndex] == null) {
				Debug.LogError ("Can't move audience to unassigned position index");
				return;
			}

			StopAllCoroutines ();
			StartCoroutine (DoMoveTo (positions[positionIndex]));
		}

		private IEnumerator DoMoveTo (AudiencePosition pos) {
			float duration = Vector3.Distance (transform.localPosition, pos.position) / movementSpeed;
			float time = 0f;

			Vector3 origPos = transform.localPosition;
			Quaternion origRot = transform.localRotation;
			float origScale = transform.localScale.x;

			// Correct for accidental zero scales
			if (pos.scale == 0) pos.scale = 1f;

			while (time < duration) {
				yield return null;
				time += Time.deltaTime;
				transform.localPosition = Vector3.Lerp (origPos, pos.position, (time / duration));
				transform.localRotation = Quaternion.Lerp (origRot, Quaternion.Euler (Vector3.up * pos.yRotation), (time / duration));
				transform.localScale = Vector3.one * Mathf.Lerp (origScale, pos.scale, (time / duration));

				OnMoved.Invoke ();
			}

			transform.localPosition = pos.position;
			transform.localRotation = Quaternion.Euler (Vector3.up * pos.yRotation);
			transform.localScale = Vector3.one * pos.scale;
			OnMoveCompleted.Invoke ();
		}

		public void FadeTo (int positionIndex = 0) {
			if (positionIndex < 0 || positionIndex >= positions.Length) {
				Debug.LogError ("Invalid audience position index");
				return;
			}

			if (positions[positionIndex] == null) {
				Debug.LogError ("Can't move audience to unassigned position index");
				return;
			}

			StopAllCoroutines ();
			StartCoroutine (DoFadeTo (positions[positionIndex]));
		}

		private IEnumerator DoFadeTo (AudiencePosition pos) {
			if (blink == null) blink = Flipside.Helpers.PlayerController.Instance.headCam.GetComponent<BlinkEffect> ();

			yield return blink.Blink (fadeSpeed);

			// Correct for accidental zero scales
			if (pos.scale == 0) pos.scale = 1f;

			transform.localPosition = pos.position;
			transform.localEulerAngles = Vector3.up * pos.yRotation;
			transform.localScale = Vector3.one * pos.scale;

			OnMoved.Invoke ();

			yield return blink.Unblink (fadeSpeed);

			OnMoveCompleted.Invoke ();
		}

		public void EnableTeleportOnSet () {
			canTeleportOnSet = true;
			OnTeleportOnSetEnabled.Invoke ();
		}

		public void DisableTeleportOnSet () {
			canTeleportOnSet = false;
			OnTeleportOnSetDisabled.Invoke ();
		}

		public bool GetTeleportOnSetState () {
			return canTeleportOnSet;
		}

#if UNITY_EDITOR

		private GUIStyle guiStyle;

		public void OnDrawGizmos () {
			Gizmos.color = Color.white;

			if (guiStyle == null) {
				guiStyle = new GUIStyle ();
				guiStyle.normal.textColor = Color.white;
				guiStyle.font = (Font) Resources.Load ("Quicksand-Medium", typeof (Font));
			}

			if (positions.Length == 0) {
				var pos = new AudiencePosition ();
				pos.position = transform.position;
				pos.yRotation = transform.eulerAngles.y;
				pos.scale = transform.lossyScale.x;
				var scale = (pos.scale > 0f) ? pos.scale : 1f;

				Gizmos.matrix = Matrix4x4.TRS (pos.position + (Vector3.up * scale), Quaternion.Euler (Vector3.up * pos.yRotation), Vector3.one * scale);
				Gizmos.DrawFrustum (Vector3.zero, 60, 2f, 0.01f, 1.777f);

				guiStyle.normal.textColor = Color.white;
				Handles.Label (pos.position + (Vector3.up * scale), "Audience Position 0", guiStyle);
			}

			for (int i = 0; i < positions.Length; i++) {
				var pos = positions[i];
				var scale = (pos.scale > 0f) ? pos.scale : 1f;

				Gizmos.matrix = Matrix4x4.TRS (pos.position + (Vector3.up * scale), Quaternion.Euler (Vector3.up * pos.yRotation), Vector3.one * scale);
				Gizmos.DrawFrustum (Vector3.zero, 60, 2f, 0.01f, 1.777f);

				guiStyle.normal.textColor = Color.white;
				Handles.Label (pos.position + (Vector3.up * scale), "Audience Position " + i, guiStyle);
			}

			for (int i = 0; i < seats.Length; i++) {
				var pos = seats[i];
				var r = transform.rotation * Quaternion.Euler (Vector3.up * pos.yRotation);
				var p = transform.TransformPoint (pos.position);

				Gizmos.matrix = Matrix4x4.TRS (p, r, Vector3.one);
				Gizmos.DrawWireSphere (Vector3.zero + (Vector3.up * 1.5f), 0.2f);
				Gizmos.DrawWireCube (Vector3.zero + (Vector3.up * 0.65f), new Vector3 (0.15f, 1.3f, 0.15f));
			}
		}

#endif
	}
}