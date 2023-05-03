/**
 * Copyright (c) 2018 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

namespace Flipside.Avatars {

	internal interface IImport {

		bool CanAutoSetup (AvatarModelReferences avatarModelReferences);

		void Setup (AvatarModelReferences avatarModelReferences);
	}
}