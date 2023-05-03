/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
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
	/// Triggers an event at the specified interval as long as the component is enabled.
	/// </summary>
	public class TriggerEvery : MonoBehaviour {

		public enum TriggerOn {
			Frame,
			Seconds,
			SecondsRealtime
		}

		public TriggerOn triggerOn = TriggerOn.Frame;

		public float seconds = 0f;

		public UnityEvent OnTrigger = new UnityEvent ();

		private WaitForSeconds wfs;
		private WaitForSecondsRealtime wfsr;

		private void Awake () {
			wfs = new WaitForSeconds (seconds);
			wfsr = new WaitForSecondsRealtime (seconds);
		}

		private void OnEnable () {
			StopAllCoroutines ();
			StartCoroutine (InnerLoop ());
		}

		private void OnDisable () {
			StopAllCoroutines ();
		}

		private IEnumerator InnerLoop () {
			while (true) {
				switch (triggerOn) {
					case TriggerOn.Seconds:
						yield return wfs;
						break;

					case TriggerOn.SecondsRealtime:
						yield return wfsr;
						break;

					case TriggerOn.Frame:
					default:
						yield return null;
						break;
				}

				OnTrigger.Invoke ();
			}
		}
	}
}