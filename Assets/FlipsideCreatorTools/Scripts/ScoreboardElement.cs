/**
 * Copyright (c) 2019 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Flipside.Sets {

	/// <summary>
	/// Displays a scoreboard wherever placed, and lets you reset scores
	/// and trigger events on points, winning, and reset.
	/// </summary>
	public class ScoreboardElement : MonoBehaviour {
		private static ScoreboardElement _instance;

		/// <summary>
		/// In-app, this returns the correct score for the number of players present.
		/// </summary>
		public int pointsToWin {
			get { return pointsToWin1P; }
			private set { pointsToWin1P = value; }
		}

		[Tooltip ("The score players must reach in order to win in 1-player mode")]
		public int pointsToWin1P = 10;

		[Tooltip ("The score players must reach in order to win in 2-player mode")]
		public int pointsToWin2P = 10;

		[Tooltip ("The score players must reach in order to win in 3-player mode")]
		public int pointsToWin3P = 10;

		[Tooltip ("The score players must reach in order to win in 4-player mode")]
		public int pointsToWin4P = 10;

		[Tooltip ("The score players must reach in order to win in 5-player mode")]
		public int pointsToWin5P = 10;

		[Tooltip ("Is scoring done cooperatively?")]
		public bool combineScoresToWin = false;

		[Tooltip ("Should scoring continue after a win is reached?")]
		public bool continueAfterWin = false;

		[Tooltip ("A TextMeshPro component to display the scores on")]
		public TextMeshPro displayScoresOn;

		[Space (10)]
		[Tooltip ("Initialize any game elements for player 1")]
		public UnityEvent OnInitializeP1 = new UnityEvent ();

		[Tooltip ("Initialize any game elements for player 2")]
		public UnityEvent OnInitializeP2 = new UnityEvent ();

		[Tooltip ("Initialize any game elements for player 3")]
		public UnityEvent OnInitializeP3 = new UnityEvent ();

		[Tooltip ("Initialize any game elements for player 4")]
		public UnityEvent OnInitializeP4 = new UnityEvent ();

		[Tooltip ("Initialize any game elements for player 5")]
		public UnityEvent OnInitializeP5 = new UnityEvent ();

		[Tooltip ("Points were gained by a player")]
		public UnityEvent OnPoints = new UnityEvent ();

		[Tooltip ("The game was won (note: only fires if TriggerGameEnd() is called when continueAfterWin=true")]
		public UnityEvent OnWin = new UnityEvent ();

		[Tooltip ("The game was lost (note: only fires if TriggerGameEnd() is called")]
		public UnityEvent OnLose = new UnityEvent ();

		[Tooltip ("The game was won but we are continuing to play til the end")]
		public UnityEvent OnWinReached = new UnityEvent ();

		[Tooltip ("The game was reset")]
		public UnityEvent OnReset = new UnityEvent ();

		[Serializable]
		public class CombinedScoreEvent : UnityEvent<float> { }

		[Tooltip ("Sends the percentage score (0-1) when the combined score changes")]
		public CombinedScoreEvent OnCombinedScore = new CombinedScoreEvent ();

		private static Dictionary<string, int> scores = new Dictionary<string, int> ();
		private static string winner = "";
		private int combinedScore = 0;

		private void Awake () {
			if (_instance != null) {
				Debug.LogError ("There must only be one ScoreboardElement in your scene.");
			}
			_instance = this;
		}

		private void OnEnable () {
			OnInitializeP1.Invoke ();
			//OnInitializeP2.Invoke ();
			UpdateDisplay ();
		}

		public void SetPointsToWin (int points) {
			_instance.pointsToWin = points;
		}

		public string GetWinner () {
			return winner;
		}

		public bool DidWin () {
			return (winner != "");
		}

		public Dictionary<string, int> GetScores () {
			return scores;
		}

		public static void UpdatePoints (string userId, int points) {
			if (_instance != null) _instance.AddPoints (userId, points);
		}

		/// <summary>
		/// Add team points in combined scoring mode.
		/// </summary>
		/// <param name="points">Points.</param>
		public void AddPoints (int points) {
			if (!continueAfterWin && winner != "") return; // Game is done, don't keep adding points

			if (!scores.ContainsKey ("Team")) {
				scores["Team"] = points;
			} else {
				scores["Team"] += points;
			}

			if (combineScoresToWin) {
				combinedScore += points;
				OnCombinedScore.Invoke (Mathf.Clamp ((float) combinedScore / (float) pointsToWin, 0f, 1f));
			}

			_instance.UpdateDisplay ();
			_instance.OnPoints.Invoke ();

			UpdateWinState ("Team");
		}

		/// <summary>
		/// Add points to a specific player.
		/// </summary>
		/// <param name="userId">User ID.</param>
		/// <param name="points">Points.</param>
		public void AddPoints (string userId, int points) {
			if (!continueAfterWin && winner != "") return; // Game is done, don't keep adding points

			if (!scores.ContainsKey (userId)) {
				scores[userId] = points;
			} else {
				scores[userId] += points;
			}

			if (combineScoresToWin) {
				combinedScore += points;
				OnCombinedScore.Invoke (Mathf.Clamp ((float) combinedScore / (float) pointsToWin, 0f, 1f));
			}

			_instance.UpdateDisplay ();
			_instance.OnPoints.Invoke ();

			UpdateWinState (userId);
		}

		private void UpdateWinState (string userId) {
			if (winner != "") return; // Already declared a winner

			if (combineScoresToWin) {
				if (combinedScore >= _instance.pointsToWin) {
					Debug.Log ("Game won!");
					winner = "Team";
					_instance.UpdateDisplay ();

					if (continueAfterWin) {
						_instance.OnWinReached.Invoke ();
					} else {
						_instance.OnWin.Invoke ();
					}
				}
			} else {
				if (scores[userId] >= _instance.pointsToWin) {
					Debug.Log ("Game won by " + userId);
					winner = userId;
					_instance.UpdateDisplay ();

					if (continueAfterWin) {
						_instance.OnWinReached.Invoke ();
					} else {
						_instance.OnWin.Invoke ();
					}
				}
			}
		}

		public void TriggerGameEnd () {
			if (winner != "") {
				Debug.Log ("Game won by " + winner);
				_instance.OnWin.Invoke ();
			} else {
				Debug.Log ("Game lost :(");
				_instance.OnLose.Invoke ();
			}
		}

		public void TriggerLose () {
			winner = "";
			Debug.Log ("Game lost :(");
			_instance.OnLose.Invoke ();
		}

		public void ResetScores () {
			Debug.Log ("Resetting scores");
			scores.Clear ();
			combinedScore = 0;
			winner = "";
			UpdateDisplay ();
			OnReset.Invoke ();
		}

		private string scoresString = "";
		private StringBuilder sb = new StringBuilder ();

		private void UpdateDisplay () {
			if (displayScoresOn == null) return;

			sb.Clear ();
			if (winner != "") {
				sb.AppendFormat ("<u>{0} wins!</u>\n", winner);
			} else {
				sb.Append ("<u>Scoreboard</u>\n");
			}

			foreach (var score in scores) {
				if (score.Key == "Team") continue; // Don't show "Team" since total is already shown
				sb.AppendFormat ("{0,-22} {1,5}\n", score.Key, score.Value);
			}

			if (combineScoresToWin) {
				sb.AppendFormat ("{0,-22} {1,5}", "Total", combinedScore);
			}

			scoresString = sb.ToString ();

			if (displayScoresOn != null) {
				displayScoresOn.text = scoresString;
			}

			//Debug.Log (scoresString);
		}
	}

#if UNITY_EDITOR

	[CustomEditor (typeof (ScoreboardElement))]
	public class ScoreboardElementEditor : Editor {

		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();

			ScoreboardElement sb = (ScoreboardElement) target;

			GUILayout.Space (10);

			if (GUILayout.Button ("Fire InitializeP1 Event")) {
				sb.OnInitializeP1.Invoke ();
			}

			if (GUILayout.Button ("Fire InitializeP2 Event")) {
				sb.OnInitializeP2.Invoke ();
			}

			if (GUILayout.Button ("Fire InitializeP3 Event")) {
				sb.OnInitializeP3.Invoke ();
			}

			if (GUILayout.Button ("Fire InitializeP4 Event")) {
				sb.OnInitializeP4.Invoke ();
			}

			if (GUILayout.Button ("Fire InitializeP5 Event")) {
				sb.OnInitializeP5.Invoke ();
			}

			if (GUILayout.Button ("Fire Points Event")) {
				sb.OnPoints.Invoke ();
			}

			if (GUILayout.Button ("Fire Win Event")) {
				sb.OnWin.Invoke ();
			}

			if (GUILayout.Button ("Fire WinReached Event")) {
				sb.OnWinReached.Invoke ();
			}

			if (GUILayout.Button ("Fire Lose Event")) {
				sb.OnLose.Invoke ();
			}

			if (GUILayout.Button ("Fire Reset Event")) {
				sb.OnReset.Invoke ();
			}
		}
	}

#endif
}