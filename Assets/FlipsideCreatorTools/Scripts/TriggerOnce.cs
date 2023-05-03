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

namespace Flipside.Sets {

	/// <summary>
	/// Triggers an event only when a boolean is set. Use TriggerEvent()
	/// to trigger the OnTriggered event once and set TriggerEnabled
	/// to false so subsequent calls to TriggerEvent() will be ignored.
	/// </summary>
	public class TriggerOnce : MonoBehaviour {
		public UnityEvent OnTriggered = new UnityEvent ();

		/// <summary>
		/// Is the trigger currently enabled?
		/// </summary>
		public bool triggerEnabled = true;

		/// <summary>
		/// Enable the trigger so the event will fire again on the next
		/// call to TriggerEvent().
		/// </summary>
		public void EnableTrigger () {
			triggerEnabled = true;
		}

		/// <summary>
		/// Disable the trigger, stopping the event from firing.
		/// </summary>
		public void DisableTrigger () {
			triggerEnabled = false;
		}

		/// <summary>
		/// Trigger the event and set TriggerEnabled to false to prevent
		/// subsequent calls from triggering.
		/// </summary>
		public void TriggerEvent () {
			if (!triggerEnabled) return;
			triggerEnabled = false;
			OnTriggered.Invoke ();
		}

		/// <summary>
		/// Trigger the event, but keep TriggerEnabled true.
		/// </summary>
		public void TriggerAndKeepEnabled () {
			if (!triggerEnabled) return;
			OnTriggered.Invoke ();
		}
	}
}