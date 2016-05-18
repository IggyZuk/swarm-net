using UnityEngine;
using System.Collections;

namespace Game
{
	public class Weapon : Photon.MonoBehaviour
	{
		[SerializeField]
		private GameObject _projectile;

		[SerializeField]
		private int _totalAmmo;
		[SerializeField]
		private float _recoilInSeconds;

		private int _ammo;
		private float _recoil;

		void Awake()
		{
			_ammo = _totalAmmo;
		}

		void Update()
		{
			if(this.photonView.isMine)
			{
				Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit floorHit;

				if(Physics.Raycast(camRay, out floorHit, 100f))
				{
					Vector3 weaponToEnemy = floorHit.point - this.transform.position;
					weaponToEnemy.y = 0f;
					this.transform.forward = weaponToEnemy;
				}

				_recoil += Time.deltaTime;
			}
		}

		public void Shoot()
		{
			if(_ammo > 0)
			{
				if(_recoil >= _recoilInSeconds)
				{
					_ammo--;
					_recoil = 0;

					AddProjectile(this.transform.position, this.transform.forward);

					GameController.Instance.UpdateAmmo(_ammo);
				}
			}
		}

		[PunRPC]
		public void AddProjectile(Vector3 shootPos, Vector3 shootDir)
		{
			Instantiate(_projectile, shootPos + shootDir, Quaternion.LookRotation(shootDir + Vector3.up * 0.1f));
			Instantiate(GameController.Instance.Flash, shootPos + shootDir, Quaternion.identity);

			if(this.photonView.isMine)
			{
				this.photonView.RPC("AddProjectile", PhotonTargets.OthersBuffered, shootPos, shootDir);
			}
		}

		public void AddAmmo(int amount)
		{
			_ammo += amount;

			if(this.photonView.isMine)
			{
				GameController.Instance.UpdateAmmo(_ammo);
			}
		}
	}
}
