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

namespace Flipside.Avatars {

	public class FuseImport : IImport {

		public bool CanAutoSetup (AvatarModelReferences avatarModelReferences) {
			foreach (Transform trans in avatarModelReferences.transform) {
				if (trans.name.StartsWith ("mixamorig:")) return true;
			}
			return false;
		}

		public void Setup (AvatarModelReferences avatarModelReferences) {
			avatarModelReferences.expressionType = AvatarModelReferences.ExpressionType.simplifiedBlendShapes;
			avatarModelReferences.happyShape = "41,42";
			avatarModelReferences.sadShape = "14,15";
			avatarModelReferences.surprisedShape = "8,9";
			avatarModelReferences.angryShape = "2,3";
			avatarModelReferences.blinkLeftShape = "0";
			avatarModelReferences.blinkRightShape = "1";
			avatarModelReferences.blinkAllShape = "0,1";
			avatarModelReferences.openMouthShape = "35";
		}
	}
}