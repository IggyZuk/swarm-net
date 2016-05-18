using UnityEngine;
using System.Collections;

namespace Game
{
	public class EnemySystem : Photon.MonoBehaviour
	{
		private float _timeToSpawnAnotherEnemy = 2f;
		private float _time = 0f;

		void Update()
		{
			if(PhotonNetwork.isMasterClient)
			{
				if(GameController.Instance.CurrentState == GameController.State.Playing)
				{
					_time += Time.deltaTime;

					if(_time >= _timeToSpawnAnotherEnemy)
					{
						_time = 0f;

						GameObject go = PhotonNetwork.Instantiate("Enemy", new Vector3(Random.Range(-20f, 20f), 0f, Random.Range(-20f, 20f)), Quaternion.identity, 0) as GameObject;
						SpawnNewEnemy(go.GetPhotonView().viewID, Random.Range(1, 8));
					}
				}
			}
		}

		[PunRPC]
		private void SpawnNewEnemy(int viewID, int life)
		{
			PhotonView netView = PhotonView.Find(viewID);
			Enemy enemy = netView.GetComponent<Enemy>();
			Debug.Log(GameController.Instance.Players.Count);
			enemy.Init(life, GameController.Instance.Players[Random.Range(0, GameController.Instance.Players.Count)].photonView.viewID);

			if(this.photonView.isMine)
			{
				this.photonView.RPC("SpawnNewEnemy", PhotonTargets.OthersBuffered, viewID, life);
			}
		}
	}
}
