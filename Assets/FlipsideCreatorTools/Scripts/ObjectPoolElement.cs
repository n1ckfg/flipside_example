/**
 * Copyright (c) 2020 The Campfire Union Inc - All Rights Reserved.
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
	/// Object pools help improve performance by not recreating things in a scene
	/// that may come and go. Common examples are pools of bullets or other projectiles
	/// in games, or particle bursts.
	/// 
	/// This component provides a simple object pool you can use in your custom sets.
	/// 
	/// Usage:
	/// 
	/// 1. Create an empty game object and add this component to it.
	/// 2. Create a prefab that you want to make a pool of.
	/// 3. Drag the prefab into the objectPrefab property in the inspector.
	/// 4. In the Unity event that should make an instance of your prefab appear,
	///    drag this component and set it to call InstantiateAndEnableAt(transform)
	///    and provide it with a Transform that says where to place it in the scene.
	/// 
	/// To return an object to the pool, call GameObject.SetActive(false) on it
	/// from another event, for example when its ColliderElement component hits
	/// something.
	/// 
	/// You can also listen for objects being instantiated using the
	/// OnObjectInstantiated event.
	/// </summary>
	public class ObjectPoolElement : MonoBehaviour {
		public int poolSize = 20;

		public GameObject objectPrefab;

		[Tooltip ("Whether to repurpose pooled objects that may still be in use")]
		public bool recycleOnLimitReached = true;

		[Tooltip ("Instantiate the pool under this object if not null, otherwise instantiate them at the root level.")]
		public GameObject parent = null;

		public class ObjectInstantiatedEvent : UnityEvent<GameObject> { }

		public ObjectInstantiatedEvent OnObjectInstantiated = new ObjectInstantiatedEvent ();

		private GameObject[] pool;

		private int lastUsed = -1;

		private void Awake () {
			pool = new GameObject[poolSize];

			GameObject inst;

			for (int i = 0; i < poolSize; i++) {
				inst = (GameObject) Instantiate (objectPrefab);
				inst.SetActive (false);
				pool[i] = inst;
			}
		}

		/// <summary>
		/// Instantiate an object at the specified Transform location
		/// and enable it.
		/// </summary>
		/// <param name="location">Transform.</param>
		public void InstantiateAndEnableAt (Transform location) {
			GameObject obj = null;

			for (int i = 0; i < pool.Length; i++) {
				if (!pool[i].activeInHierarchy) {
					obj = pool[i];
					lastUsed = i;
					break;
				}
			}

			if (obj == null) {
				if (recycleOnLimitReached) {
					lastUsed++;
					if (lastUsed >= poolSize) lastUsed = 0;
					obj = pool[lastUsed];
					if (obj.activeInHierarchy) obj.SetActive (false);
				} else {
					return;
				}
			}

			obj.transform.position = location.position;
			obj.transform.rotation = location.rotation;
			if (parent != null)
				obj.transform.SetParent (parent.transform, true);
			
			obj.SetActive (true);

			OnObjectInstantiated.Invoke (obj);
		}
	}
}