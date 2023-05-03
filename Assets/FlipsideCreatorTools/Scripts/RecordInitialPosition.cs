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

namespace Flipside.Sets {
	//This will record the initial position of any object it's placed on, and then set that position on playback
	public class RecordInitialPosition : MonoBehaviour {

		/// <summary>
		/// Whether to auto change postion based on thresholds instead of calling ChangedPosition directly
		/// </summary>
		public bool autoChangePosition = true;

		/// <summary>
		/// Threshold at which a change in position is saved, measured in metres
		/// </summary>
		public float positionThreshold = 0.1f;

		/// <summary>
		/// Threshold at which a change in rotation is saved, measured in degrees
		/// </summary>
		public float rotationThreshold = 1f;

		/// <summary>
		/// If not auto-changing position, call this after position has been changed 
		/// </summary>
		public void ChangedPosition () {
		}
	}
}
