/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Flipside.Helpers {

	/// <summary>
	/// Provides references to the various parts of the player.
	/// </summary>
	public class PlayerController : MonoBehaviour {
		public static PlayerController Instance;
		public GameObject trackingSpace;
		public Camera headCam;
		public HandController leftHand;
		public HandController rightHand;

		private List<XRNodeState> nodeStates = new List<XRNodeState> ();
		private Vector3 tempPos;
		private Quaternion tempRot;

		private void Awake () {
			Instance = this;

			// Prevent jittery hand movement
			Time.fixedDeltaTime = 0.0111111111f;
		}

		private void OnEnable () {
			InitializeTrackingMode ();
		}

		private void Update () {
			UpdateTransform ();
			UpdateKeyboardInputs ();
		}

		private static void InitializeTrackingMode () {
			List<XRInputSubsystem> subsystems = new List<XRInputSubsystem> ();
			SubsystemManager.GetInstances<XRInputSubsystem> (subsystems);

			for (int i = 0; i < subsystems.Count; i++) {
				subsystems[i].TrySetTrackingOriginMode (TrackingOriginModeFlags.Floor);
			}
		}

		private void UpdateTransform () {
			InputTracking.GetNodeStates (nodeStates);

			foreach (XRNodeState state in nodeStates) {
				switch (state.nodeType) {
					case XRNode.Head:
						state.TryGetPosition (out tempPos);
						state.TryGetRotation (out tempRot);
						headCam.transform.position = tempPos;
						headCam.transform.rotation = tempRot;
						break;
				}
			}
		}

		private void UpdateKeyboardInputs () {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				TeleportTo (transform.position + transform.forward * 0.5f, transform.eulerAngles);
			}

			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				TeleportTo (transform.position + transform.forward * -0.5f, transform.eulerAngles);
			}

			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
					TeleportTo (transform.position, transform.eulerAngles + new Vector3 (0f, -45f, 0f));
				} else {
					transform.Translate (Vector3.left * 0.5f, Space.Self);
				}
			}

			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
					TeleportTo (transform.position, transform.eulerAngles + new Vector3 (0f, 45f, 0f));
				} else {
					transform.Translate (Vector3.right * 0.5f, Space.Self);
				}
			}
		}

		/// <summary>
		/// Teleport the entire player controller to a new position and rotation.
		/// </summary>
		/// <param name="pos">Position.</param>
		/// <param name="euler">Euler angle of rotation.</param>
		public void TeleportTo (Vector3 pos, Vector3 euler) {
			transform.position = pos;
			transform.eulerAngles = euler;
		}
	}
}