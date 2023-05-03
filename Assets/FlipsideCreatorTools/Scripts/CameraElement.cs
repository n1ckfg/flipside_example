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
using UnityEngine.Events;

namespace Flipside.Sets {
	
	/// <summary>
	/// Trigger Unity events when this camera is activated or deactivated.
	/// </summary>
	public class CameraElement : MonoBehaviour {

		//[Tooltip ("Keep the camera synced to this object's movement. Automatically set to true if under a PropElement or on a Cinemachine camera")]
		[HideInInspector]
		public bool trackThisObject = true;

		public UnityEvent OnCameraActivated = new UnityEvent ();

		public UnityEvent OnCameraDeactivated = new UnityEvent ();
	}
}
