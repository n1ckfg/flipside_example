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
	/// Targets can be hit by BulletElement and ThrowableElement objects.
	/// </summary>
	public class TargetElement : MonoBehaviour {

		[Tooltip ("Check to override the addPointsOnTargetHit value on BulletElement and ThrowableElement with the value below")]
		public bool overridePointsOnHit = false;

		public int addPointsOnHit = 1;

		[Space (10)]
		public UnityEvent OnHit = new UnityEvent ();

		public void Hit (string player, int points) {
			Debug.Log ("Target hit by " + player);
			ScoreboardElement.UpdatePoints (player, (overridePointsOnHit) ? addPointsOnHit : points);
			OnHit.Invoke ();
		}
	}
}