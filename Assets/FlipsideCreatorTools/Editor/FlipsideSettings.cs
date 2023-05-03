using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu (fileName = "Assets/FlipsideCreatorTools/FlipsideSettings.asset", menuName = "Flipside Settings")]
public class FlipsideSettings : ScriptableObject {

	/// <summary>
	/// Increment this when we release new Creator Tools updates.
	/// </summary
	public static readonly string creatorToolsVersion = "2023.1.3-stable";

	/// <summary>
	/// Update this when we upgrade Unity versions.
	/// </summary>
	public static readonly string currentUnityVersion = "2020.3.";

	/// <summary>
	/// Update this when we upgrade Unity versions.
	/// </summary>
	public static readonly string fullUnityVersion = "2020.3.36f1";
}
