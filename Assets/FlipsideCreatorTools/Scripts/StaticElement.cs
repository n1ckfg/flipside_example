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

	public enum StaticElementType {
		Floor,
		Wall,
		Ceiling,
		Seat
	}

	public class StaticElement : MonoBehaviour {
		public StaticElementType type = StaticElementType.Wall;
	}
}