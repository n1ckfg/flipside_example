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
using UnityEngine.XR;
using Flipside.Sets;
using Unity.Labs.SuperScience;

namespace Flipside.Helpers {

	/// <summary>
	/// Tracks the position of a user's hand movement.
	/// </summary>
	public class HandController : MonoBehaviour {
		public XRNode hand;

		public UnityEvent OnTriggerDown;
		public UnityEvent OnTriggerUp;

		public UnityEvent OnGripDown;
		public UnityEvent OnGripUp;

		[SerializeField]
		private GameObject oculusController;

		[SerializeField]
		private GameObject steamVrController;

		public GameObject activeController {
			get {
				if (XRSettings.loadedDeviceName == "Oculus") {
					return oculusController;
				} else {
					return steamVrController;
				}
			}
		}

		private InputDevice device;

		private float currentTriggerValue = 0f;
		private float currentGripValue = 0f;
		private Vector2 primaryAxis = Vector2.zero;

		private float fireThreshold = 0.82f;
		private float grabThreshold = 0.75f;

		private List<PropElement> collidingWith = new List<PropElement> ();

		private PropElement interactingWith;

		private PhysicsTracker physicsTracker;

		private List<XRNodeState> nodeStates = new List<XRNodeState> ();
		private Vector3 tempPos;
		private Quaternion tempRot;
		private Rigidbody rb;

		private void Awake () {
			physicsTracker = new PhysicsTracker ();
			rb = GetComponent<Rigidbody> ();
		}

		private void OnEnable () {
			StartCoroutine (ConnectControllers ());

			physicsTracker.Reset (transform.position, transform.rotation, Vector3.zero, Vector3.zero);
		}

		private IEnumerator ConnectControllers () {
			oculusController.SetActive (false);
			steamVrController.SetActive (false);

			WaitForSeconds wfs = new WaitForSeconds (0.5f);
			List<InputDevice> devices = new List<InputDevice> ();

			while (!device.isValid) {
				Debug.Log ("Checking for controller on hand " + hand);
				InputDevices.GetDevicesAtXRNode (hand, devices);

				if (devices.Count > 0 && devices[0].isValid) {
					if (XRSettings.loadedDeviceName == "OpenVR") {
						steamVrController.SetActive (true);
					} else {
						oculusController.SetActive (true);
					}

					device = devices[0];
					Debug.LogFormat ("Found device {0} for hand {1}", device, hand);
					break;
				}

				yield return wfs;
			}
		}

		private void OnTriggerEnter (Collider other) {
			PropElement pe = other.GetComponent<PropElement> () ?? other.GetComponentInParent<PropElement> ();

			if (pe == null) return;
			if (collidingWith.Contains (pe)) return;

			collidingWith.Add (pe);
		}

		private void OnTriggerExit (Collider other) {
			PropElement pe = other.GetComponent<PropElement> () ?? other.GetComponentInParent<PropElement> ();

			if (pe == null) return;
			if (!collidingWith.Contains (pe)) return;

			collidingWith.Remove (pe);
		}

		private void Update () {
			if (device == null || !device.isValid) return;
			UpdateButtonStates ();
			UpdateInteractionState ();
			UpdateVelocityEstimates ();
		}

		private void FixedUpdate () {
			UpdateTransform ();
		}

		private void UpdateTransform () {
			InputTracking.GetNodeStates (nodeStates);

			foreach (XRNodeState state in nodeStates) {
				if (state.nodeType == hand) {
					state.TryGetPosition (out tempPos);
					state.TryGetRotation (out tempRot);
					rb.MovePosition (tempPos);
					rb.MoveRotation (tempRot);
					break;
				}
			}
		}

		private void UpdateButtonStates () {
			float triggerValue;
			if (device.TryGetFeatureValue (CommonUsages.trigger, out triggerValue)) {
				if (currentTriggerValue < fireThreshold && triggerValue >= fireThreshold) {
					OnTriggerDown.Invoke ();
				} else if (currentTriggerValue >= fireThreshold && triggerValue < fireThreshold) {
					OnTriggerUp.Invoke ();
				}
				currentTriggerValue = triggerValue;
			}

			float gripValue;
			if (device.TryGetFeatureValue (CommonUsages.grip, out gripValue)) {
				if (currentGripValue < grabThreshold && gripValue >= grabThreshold) {
					OnGripDown.Invoke ();
				} else if (currentGripValue >= grabThreshold && gripValue < grabThreshold) {
					OnGripUp.Invoke ();
				}
				currentGripValue = gripValue;
			}

			Vector2 axisValue;
			if (device.TryGetFeatureValue (CommonUsages.primary2DAxis, out axisValue)) {
				primaryAxis = axisValue;
			}
		}

		private void UpdateInteractionState () {
			if (interactingWith == null && currentGripValue >= grabThreshold && collidingWith.Count > 0) {
				GrabObject ();
			}

			if (interactingWith != null && currentGripValue < grabThreshold) {
				ReleaseObject ();
			}
		}

		private void GrabObject () {
			PropElement closestElement = null;
			float closestDistance = float.MaxValue;

			foreach (PropElement pe in collidingWith) {
				if (pe == null) continue;

				float distance = Vector3.Distance (pe.transform.position, transform.position);

				if (distance < closestDistance) {
					closestElement = pe;
					closestDistance = distance;
				}
			}

			if (closestElement == null) return;

			interactingWith = closestElement;
#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS
			interactingWith.AttachToHand (this);
#endif
		}

		private void ReleaseObject () {
			if (interactingWith == null) return;

#if UNITY_EDITOR || FLIPSIDE_CREATOR_TOOLS
			interactingWith.DetachHand ();
			interactingWith.ApplyForce (physicsTracker.Velocity, physicsTracker.AngularVelocity);
#endif
			interactingWith = null;
		}

		private void UpdateVelocityEstimates () {
			if (interactingWith != null) {
				physicsTracker.Update (interactingWith.transform.position, interactingWith.transform.rotation, Time.smoothDeltaTime);
			} else {
				physicsTracker.Update (transform.position, transform.rotation, Time.smoothDeltaTime);
			}
		}

		public void HideController () {
			foreach (Transform child in transform) {
				child.gameObject.SetActive (false);
			}
		}

		public void ShowController () {
			foreach (Transform child in transform) {
				child.gameObject.SetActive (true);
			}
		}

		/// <summary>
		/// Is the trigger currently held down?
		/// </summary>
		/// <returns></returns>
		public bool GetTrigger () {
			return (currentTriggerValue > 0.95f);
		}

		/// <summary>
		/// Get how much the trigger is currently pressed.
		/// </summary>
		public float GetTriggerValue () {
			return currentTriggerValue;
		}

		/// <summary>
		/// Is the grip currently held down?
		/// </summary>
		public bool GetGrip () {
			return (currentGripValue > 0.95f);
		}

		/// <summary>
		/// Get how much the grip is currently pressed.
		/// </summary>
		public float GetGripValue () {
			return currentGripValue;
		}

		/// <summary>
		/// Get the value of the primary axis (e.g., joystick, touchpad, or trackpad).
		/// </summary>
		public Vector2 GetPrimaryAxisValue () {
			return primaryAxis;
		}

		/// <summary>
		/// Triggers a short haptic pulse at the specified amplitude.
		/// </summary>
		/// <param name="amplitude">Amplitude.</param>
		public void TriggerHaptics (float amplitude) {
			HapticCapabilities capabilities;
			if (device.TryGetHapticCapabilities (out capabilities)) {
				if (capabilities.supportsImpulse) {
					uint channel = 0;
					float duration = 0.1f;
					device.SendHapticImpulse (channel, amplitude, duration);
				}
			}
		}

		/// <summary>
		/// Get the attached XR input device.
		/// </summary>
		public InputDevice GetInputDevice () {
			return device;
		}
	}
}