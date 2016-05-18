using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Game
{
	public class GameController : Photon.MonoBehaviour
	{
		public static GameController Instance { get; private set; }

		[Header("References")]
		public List<Player> Players;
		public CameraController Camera;
		public GameObject Ground;

		[Header("Prefabs")]
		public GameObject Enemy;
		public GameObject Ammo;
		public GameObject Explosion;
		public GameObject Flash;

		[Header("Text")]
		public Text ScoreText;
		public Text AmmoText;
		public Text LivesText;
		public Text FinalScoreText;
		public Text RestartText;

		public EnemySystem EnemySystem { get; private set; }

		public enum State
		{
			WaitingToStart,
			Playing,
			GameOver
		}

		public State CurrentState { get; private set; }

		private int _totalScore = 0;

		void Awake()
		{
			Debug.Assert(Instance == null, "There can only be one instance of GameController!");
			Instance = this;

			EnemySystem = this.gameObject.AddComponent<EnemySystem>();

			// Just in case we'll disable the final text.
			ScoreText.enabled = false;
			AmmoText.enabled = false;
			LivesText.enabled = false;
			FinalScoreText.enabled = false;

			RestartText.text = "waiting for other player";

			UpdateScore();

			CurrentState = State.WaitingToStart;
		}

		void Update()
		{
			if(CurrentState == State.GameOver)
			{
				if(PhotonNetwork.connected == false)
				{
					if(Input.GetKeyDown(KeyCode.R))
					{
						Scene scene = SceneManager.GetActiveScene();
						SceneManager.LoadScene(scene.name);
					}
				}
			}
		}

		public void StartGame()
		{
			CurrentState = State.Playing;

			ScoreText.enabled = true;
			AmmoText.enabled = true;
			LivesText.enabled = true;

			RestartText.enabled = false;

			GameObject playerGo = PhotonNetwork.Instantiate("Player", new Vector3(0f, 0.5f, 0f), Quaternion.identity, 0) as GameObject;
			this.photonView.RPC("PlayerAdded", PhotonTargets.AllBuffered, playerGo.GetComponent<Player>().photonView.viewID);
		}

		[PunRPC]
		public void PlayerAdded(int viewID)
		{
			PhotonView newView = PhotonView.Find(viewID);
			Player player = newView.GetComponent<Player>();
			Players.Add(player);
		}

		public void GameOver()
		{
			CurrentState = State.GameOver;

			ScoreText.enabled = false;
			AmmoText.enabled = false;
			LivesText.enabled = false;

			FinalScoreText.enabled = true;
			RestartText.enabled = true;

			FinalScoreText.text = string.Format("final score - {0}", _totalScore);
			RestartText.text = "press 'r' to restart";

			StartCoroutine(Disconnect_Routine());
		}

		private IEnumerator Disconnect_Routine()
		{
			yield return new WaitForSeconds(0.2f);
			PhotonNetwork.Disconnect();
		}

		[PunRPC]
		public void AddScore()
		{
			_totalScore++;
			UpdateScore();

			if(this.photonView.isMine)
			{
				this.photonView.RPC("AddScore", PhotonTargets.OthersBuffered);
			}
		}

		public void UpdateScore()
		{
			ScoreText.text = string.Format("{0}", _totalScore);
		}

		public void UpdateAmmo(int ammo)
		{
			AmmoText.text = string.Format("ammo - {0}", ammo);
		}

		public void UpdateLives(int lives)
		{
			LivesText.text = string.Format("lives - {0}", lives);
		}
	}
}
