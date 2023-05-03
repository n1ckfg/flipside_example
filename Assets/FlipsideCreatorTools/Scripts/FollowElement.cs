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
using Flipside.Helpers;

namespace Flipside.Sets {

	/// <summary>
	/// Causes an object to follow the movement of another object, such as the
	/// user's hands.
	///
	/// To change the object's position relative to the hand, assign FollowElement
	/// to a parent game object and adjust the position of the model on the
	/// child object.
	///
	/// Note: Will clone to appear on the hands of all connected users.
	/// </summary>
	public class FollowElement : MonoBehaviour {

		public enum FollowObject { LeftHand, RightHand, Head, Custom }

		[Tooltip ("The object that this object should follow.")]
		public FollowObject follow;

		[Tooltip ("If follow is set to custom, assign the custom transform here.")]
		public Transform customFollow;

		public enum FollowPlayer { P1, P2, P3, P4, P5, LocalPlayer }

		[Tooltip ("Follow a specific player's hands or head.")]
		public FollowPlayer followPlayer = FollowPlayer.LocalPlayer;

		[Tooltip ("Should this object match the scale of the target object?")]
		public bool scaleWithTarget = true;

		[Tooltip ("Smoothing makes the follow element lerp over the specified number of steps.")]
		public int smoothing = 0;

		private Transform target;

		private Vector3[] positionHistory;
		private Quaternion[] rotationHistory;
		private int currentPosition = 0;
		private int positionsCounted = 0;

		private void Awake () {
			ResetHistory ();
		}

		private void OnEnable () {
			ResetHistory ();
		}

		private void ResetHistory () {
			if (smoothing > 0) {
				positionHistory = new Vector3[smoothing];
				rotationHistory = new Quaternion[smoothing];
				currentPosition = 0;
				positionsCounted = 0;
			}
		}

		private void GetTarget () {
			if (target != null) return;

			switch (follow) {
				case FollowObject.LeftHand:
					if (Flipside.Helpers.PlayerController.Instance == null) return;
					target = Flipside.Helpers.PlayerController.Instance.leftHand.activeController.transform;
					break;

				case FollowObject.RightHand:
					if (Flipside.Helpers.PlayerController.Instance == null) return;
					target = Flipside.Helpers.PlayerController.Instance.rightHand.activeController.transform;
					break;

				case FollowObject.Head:
					if (Flipside.Helpers.PlayerController.Instance == null) return;
					target = Flipside.Helpers.PlayerController.Instance.headCam.transform;
					break;

				case FollowObject.Custom:
					target = customFollow;
					break;
			}
		}

		private void FollowTarget () {
			if (target == null) return;

			if (smoothing <= 0) {
				transform.position = target.position;
				transform.rotation = target.rotation;
			} else {
				transform.position = GetAveragedPosition ();
				transform.rotation = GetAveragedRotation ();
			}

			if (scaleWithTarget) transform.localScale = target.localScale;
		}

		private void FixedUpdate () {
			GetTarget ();
			UpdateHistory ();
			FollowTarget ();
		}

		private void UpdateHistory () {
			if (target == null) return;
			if (smoothing <= 0) return;

			positionHistory[currentPosition] = target.position;
			rotationHistory[currentPosition] = target.rotation;

			positionsCounted++;
			currentPosition++;
			if (currentPosition >= smoothing) {
				currentPosition = 0;
			}
		}

		private Vector3 GetAveragedPosition () {
			if (positionsCounted < smoothing) return target.position;

			Vector3 average = Vector3.zero;
			int goalCount = smoothing - 1;
			int counter = 0;
			int index = currentPosition;

			while (counter < goalCount) {
				average += positionHistory[index];
				index--;
				if (index < 0) {
					index += smoothing;
				}
				counter++;
			}

			return average / (goalCount * 1f);
		}

		private Quaternion GetAveragedRotation () {
			if (positionsCounted < smoothing) return target.rotation;

			Quaternion average = new Quaternion (0f, 0f, 0f, 0f);
			float amount = 0;
			int counter = 0;
			int goalCount = smoothing - 1;
			int index = currentPosition;

			while (counter < goalCount) {
				Quaternion quat = rotationHistory[index];
				amount++;
				index--;
				if (index < 0) {
					index += smoothing;
				}
				average = Quaternion.Slerp (average, quat, 1f / amount);
				counter++;
			}

			return average;
		}
	}
}