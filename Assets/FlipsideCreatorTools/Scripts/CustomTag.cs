/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
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

	/// <summary>
	/// A custom tag system for ColliderElement since Unity's tagging system is too
	/// limited for handling both Flipside's internal tagging and interactivity
	/// on custom sets from many sources.
	/// </summary>
	public class CustomTag : MonoBehaviour {

		[Tooltip ("Make sure this matches the Custom Tag value in ColliderElement")]
		public string tagName = "";

		public enum FollowPlayer { None, P1, P2, P3, P4, P5 }

		[Tooltip ("Tells ColliderElement to act like this tag is attached to the specified player")]
		public FollowPlayer triggerAsPlayer = FollowPlayer.None;
	}
}