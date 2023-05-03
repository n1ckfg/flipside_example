/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Flipside.Avatars;
using Flipside.Sets;

namespace Flipside.Helpers {
	public class UpdateResourceUsage : UnityEditor.AssetModificationProcessor {
		static string[] OnWillSaveAssets (string[] paths) {
			Scene scene = SceneManager.GetActiveScene ();
			GameObject[] gameObjects = scene.GetRootGameObjects ();

			if (gameObjects.Length == 0) return paths;

			GameObject root = gameObjects[0];

			AvatarModelReferences avatar = root.GetComponent<AvatarModelReferences> ();
			if (avatar != null) {
				if (avatar.resourceUsage == null) {
					avatar.resourceUsage = new ResourceUsageData ();
				}
				avatar.resourceUsage.UpdateInfo (root);
			}

			SetInfo set = root.GetComponent<SetInfo> ();
			if (set != null) {
				if (set.resourceUsage == null) {
					set.resourceUsage = new ResourceUsageData ();
				}
				set.resourceUsage.UpdateInfo (root);
			}

			return paths;
		}
	}
}