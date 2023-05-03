/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;

namespace Flipside.Avatars {
	
	/// <summary>
	/// Causes eyes to target an object when it's within view.
	/// </summary>
	public class EyeTarget : MonoBehaviour {

		/// <summary>
		/// Whether this object will register itself on start
		/// </summary>
		public bool registerOnStart = true;

		/// <summary>
		/// Limit of how many degrees from forward this object with catch gaze
		/// </summary>
		public float angleLimit = 30f;

		/// <summary>
		/// Weight on angle. Since closest angle is used, then lower weights get higher priority over higher weights
		/// </summary>
		public float angleWeight = 1f;
		
		public void RegisterThis () {
			Debug.Log ("Registered EyeTarget");
		}
		
		public void RemoveThis () {
			Debug.Log ("Removed EyeTarget");
		}
		
		public void RegisterForTime (float time) {
			Debug.LogFormat ("Registered EyeTarget for {0} seconds", time);
		}
	}
}
