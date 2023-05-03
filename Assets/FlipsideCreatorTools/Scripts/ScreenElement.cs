/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;

namespace Flipside.Sets {

	/// <summary>
	/// Replaces the material on this object with the selected screen element in Flipside.
	/// </summary>
	[RequireComponent (typeof (Renderer))]
	public class ScreenElement : MonoBehaviour {

		private enum ScreenType {
			Slideshow,
			MainOutput,
			Desktop,
		}

		[SerializeField]
		private ScreenType screenType;

		[Tooltip ("An alternate or fallback image to display on this screen if there are no slides available")]
		[SerializeField]
		private Texture2D alternateImage;
	}
}