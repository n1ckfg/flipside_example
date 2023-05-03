/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using Flipside.Sets;
using Flipside.Avatars;

namespace Flipside.Helpers {

	[Serializable]
	public struct ResourceNode {
		public string name;
		public int vertexCount;

		public ResourceNode (string name, int count) {
			this.name = name;
			this.vertexCount = count;
		}
	}

	public class ResourceNodeComparer : IComparer<ResourceNode> {

		public int Compare (ResourceNode x, ResourceNode y) {
			return (x.vertexCount - y.vertexCount) * -1;
		}
	}

	// Resource usage limits for sets
	public class ResourceUsageLimitSets {

		public static ResourceUsageData Windows {
			get {
				Init (); return _Windows;
			}
		}

		public static ResourceUsageData Android {
			get {
				Init (); return _Android;
			}
		}

		private static ResourceUsageData _Windows = new ResourceUsageData ();
		private static ResourceUsageData _Android = new ResourceUsageData ();
		private static bool initialized = false;

		private static void Init () {
			if (initialized) return;

			_Windows.lightCount = 3;
			_Windows.meshCount = 1000;
			_Windows.skinnedMeshCount = 0;
			_Windows.propCount = 50;
			_Windows.meshVertexCount = 250000;

			_Android.lightCount = 1;
			_Android.meshCount = 50;
			_Android.skinnedMeshCount = 0;
			_Android.propCount = 10;
			_Android.meshVertexCount = 100000;

			initialized = true;
		}

		public static bool WithinAndroidLimits (SetInfo setInfo) {
			/*if (setInfo.resourceUsage.lightCount > Android.lightCount) return false;
			if (setInfo.resourceUsage.meshCount > Android.meshCount) return false;
			if (setInfo.resourceUsage.skinnedMeshCount > Android.skinnedMeshCount) return false;
			if (setInfo.resourceUsage.propCount > Android.propCount) return false;
			if (setInfo.resourceUsage.vertexCount > Android.meshVertexCount) return false;*/

			// TODO: Verify additional set restrictions

			return true;
		}
	}

	// Resource usage limits for avatars
	public class ResourceUsageLimitAvatars {

		public static ResourceUsageData Windows {
			get {
				Init (); return _Windows;
			}
		}

		public static ResourceUsageData Android {
			get {
				Init (); return _Android;
			}
		}

		private static ResourceUsageData _Windows = new ResourceUsageData ();
		private static ResourceUsageData _Android = new ResourceUsageData ();
		private static bool initialized = false;

		private static void Init () {
			if (initialized) return;

			_Windows.lightCount = 1;
			_Windows.meshCount = 10;
			_Windows.skinnedMeshCount = 5;
			_Windows.propCount = 5;
			_Windows.meshVertexCount = 30000;

			_Android.lightCount = 0;
			_Android.meshCount = 2;
			_Android.skinnedMeshCount = 2;
			_Android.propCount = 2;
			_Android.meshVertexCount = 10000;

			initialized = true;
		}

		public static bool WithinAndroidLimits (AvatarModelReferences avatarInfo) {
			/*if (avatarInfo.resourceUsage.lightCount > Android.lightCount) return false;
			if (avatarInfo.resourceUsage.meshCount > Android.meshCount) return false;
			if (avatarInfo.resourceUsage.skinnedMeshCount > Android.skinnedMeshCount) return false;
			if (avatarInfo.resourceUsage.propCount > Android.propCount) return false;
			if (avatarInfo.resourceUsage.vertexCount > Android.meshVertexCount) return false;*/

			// TODO: Verify additional character restrictions

			return true;
		}
	}

	// Resource usage limits for prop kits
	public class ResourceUsageLimitPropKits {

		public static ResourceUsageData Windows {
			get {
				Init (); return _Windows;
			}
		}

		public static ResourceUsageData Android {
			get {
				Init (); return _Android;
			}
		}

		private static ResourceUsageData _Windows = new ResourceUsageData ();
		private static ResourceUsageData _Android = new ResourceUsageData ();
		private static bool initialized = false;

		private static void Init () {
			if (initialized) return;

			_Windows.lightCount = 1;
			_Windows.meshCount = 20;
			_Windows.skinnedMeshCount = 0;
			_Windows.propCount = 20;
			_Windows.meshVertexCount = 50000;

			_Android.lightCount = 0;
			_Android.meshCount = 10;
			_Android.skinnedMeshCount = 0;
			_Android.propCount = 10;
			_Android.meshVertexCount = 10000;

			initialized = true;
		}

		public static bool WithinAndroidLimits (PropKit propKit) {
			/*if (propKit.resourceUsage.lightCount > Android.lightCount) return false;
			if (propKit.resourceUsage.meshCount > Android.meshCount) return false;
			if (propKit.resourceUsage.skinnedMeshCount > Android.skinnedMeshCount) return false;
			if (propKit.resourceUsage.propCount > Android.propCount) return false;
			if (propKit.resourceUsage.vertexCount > Android.meshVertexCount) return false;*/

			// TODO: Verify additional character restrictions

			return true;
		}
	}

	[Serializable]
	public class ResourceUsageData {
		public int lightCount = 0;
		public int meshCount = 0;
		public int skinnedMeshCount = 0;
		public int propCount = 0;
		public int meshVertexCount = 0;
		public int skinnedMeshVertexCount = 0;
		public int propVertexCount = 0;

		public ResourceNode[] lights = new ResourceNode[0];
		public ResourceNode[] meshes = new ResourceNode[0];
		public ResourceNode[] skinnedMeshes = new ResourceNode[0];
		public ResourceNode[] props = new ResourceNode[0];

		public int vertexCount {
			get { return meshVertexCount + skinnedMeshVertexCount + propVertexCount; }
		}

		public bool initialized = false;

		public void UpdateInfo (GameObject target) {
			GetLightCount (target);
			GetVertexCount (target);
			GetPropCount (target);
			initialized = true;
		}

		public void UpdateInfo (PropKit propKit) {
			Debug.Log ("UpdateInfo(PropKit)");
			GetLightCount (propKit.gameObject);
			GetVertexCount (propKit.gameObject);
			GetPropCount (propKit.gameObject);

			int lights = lightCount;
			int meshes = meshCount;
			int skinnedMeshes = skinnedMeshCount;
			int meshVertices = meshVertexCount;
			int skinnedMeshVertices = skinnedMeshVertexCount;
			int propVertices = propVertexCount;

			for (int i = 0; i < propKit.propList.Length; i++) {
				if (propKit.propList[i] == null) continue;

				PropElement propElement = propKit.propList[i].propElement;

				if (!propElement.transform.IsChildOf (propKit.transform)) {
					// Not in scene
					GetLightCount (propElement.gameObject);
					GetVertexCount (propElement.gameObject);
					GetPropCount (propElement.gameObject);

					lights += lightCount;
					meshes += meshCount;
					skinnedMeshes += skinnedMeshCount;
					meshVertices += meshVertexCount;
					skinnedMeshVertices += skinnedMeshVertexCount;
					propVertices += propVertexCount;
				}
			}

			propCount = propKit.propList.Length;

			lightCount = lights;
			meshCount = meshes;
			skinnedMeshCount = skinnedMeshes;
			meshVertexCount = 0; // Counted in propVertexCount
			skinnedMeshVertexCount = skinnedMeshVertices;
			propVertexCount = propVertices;

			initialized = true;
		}

		private void GetLightCount (GameObject target) {
			int count = 0;

			Light[] lightsInScene = target.GetComponentsInChildren<Light> (true);

			List<ResourceNode> resources = new List<ResourceNode> ();

			foreach (Light light in lightsInScene) {
				if (light.bakingOutput.isBaked == false) {
					count++;

					resources.Add (new ResourceNode ("[Scene] " + light.gameObject.name, (int) light.range));
				}
			}

			lights = resources.ToArray ();

			lightCount = count;
		}

		private void GetVertexCount (GameObject target) {
			meshVertexCount = 0;
			skinnedMeshVertexCount = 0;

			MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter> (true);

			List<ResourceNode> resources = new List<ResourceNode> ();

			foreach (MeshFilter mf in meshFilters) {
				if (mf.gameObject.GetComponentInParent<PropElement> () != null) continue;

				if (mf.sharedMesh != null) {
					meshVertexCount += mf.sharedMesh.vertexCount;

					resources.Add (new ResourceNode ("[Mesh] " + mf.gameObject.name, mf.sharedMesh.vertexCount));
				}
			}

			meshes = resources.ToArray ();

			SkinnedMeshRenderer[] skinnedMeshRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer> (true);

			resources.Clear ();

			foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
				if (renderer.sharedMesh != null) {
					skinnedMeshVertexCount += renderer.sharedMesh.vertexCount;

					resources.Add (new ResourceNode ("[Skinned Mesh] " + renderer.gameObject.name, renderer.sharedMesh.vertexCount));
				}
			}

			skinnedMeshes = resources.ToArray ();

			meshCount = meshFilters.Length;
			skinnedMeshCount = skinnedMeshRenderers.Length;
		}

		private void GetPropCount (GameObject target) {
			PropElement[] propElements = target.GetComponentsInChildren<PropElement> (true);
			propCount = propElements.Length;
			propVertexCount = 0;

			List<ResourceNode> resources = new List<ResourceNode> ();

			foreach (PropElement propElement in propElements) {
				int vertexCount = 0;

				MeshFilter[] mfs = propElement.GetComponentsInChildren<MeshFilter> (true);

				foreach (MeshFilter mf in mfs) {
					if (mf.sharedMesh != null) {
						vertexCount += mf.sharedMesh.vertexCount;
					}
				}

				resources.Add (new ResourceNode ("[PropElement] " + propElement.gameObject.name, vertexCount));

				propVertexCount += vertexCount;
			}

			props = resources.ToArray ();
		}
	}
}