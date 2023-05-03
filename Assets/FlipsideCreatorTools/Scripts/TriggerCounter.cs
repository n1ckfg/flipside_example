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
	/// Triggers an event after a counter reaches a certain count.
	/// Fires an OnCounterReached event when Increment() has been
	/// called enough times for it to reach the target count value.
	/// </summary>
	public class TriggerCounter : MonoBehaviour {
		public UnityEvent OnCounterReached = new UnityEvent ();

		public int counter = 0;

		public int targetCount = 5;

		public void Increment () {
			Increment (1);
		}

		public void Increment (int incr = 1) {
			counter += incr;

			if (counter == targetCount) {
				OnCounterReached.Invoke ();
			}
		}

		public void SetTargetCount (int count) {
			targetCount = count;
		}

		public void ResetCounter () {
			counter = 0;
		}
	}
}