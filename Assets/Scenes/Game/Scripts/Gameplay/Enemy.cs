using UnityEngine;
using System.Collections;

namespace Game
{
	public class Enemy : Photon.MonoBehaviour
	{
		[SerializeField]
		private float _moveSpeed;

		private Rigidbody _rigidbody;
		private Material _material;

		private int _life;
		private Transform _target;

		private Vector3 _correctPlayerPos;
		private Quaternion _correctPlayerRot;

		void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_material = GetComponentInChildren<MeshRenderer>().material;
		}

		public void Init(int life, int followPlayerWithViewID)
		{
			_life = life;

			// Let's scale the enemy according to the amount of life it has.
			this.transform.localScale = Vector3.one * (_life * 0.5f);

			// Let's make sure it's positioned right on the ground.
			Vector3 pos = this.transform.position;
			pos.y = this.transform.localScale.y * 0.5f;
			this.transform.position = pos;

			// Pick a random player to follow
			if(GameController.Instance.Players != null)
			{
				foreach(Player player in GameController.Instance.Players)
				{
					if(player.photonView.viewID == followPlayerWithViewID)
					{
						_target = player.transform;
						Debug.Log("FOLLOW: " + followPlayerWithViewID);
					}
				}
			}
		}

		void FixedUpdate()
		{
			if(this.photonView.isMine)
			{
				if(_target != null)
				{
					Vector3 direction = (_target.position - this.transform.position).normalized;
					_rigidbody.AddForce(direction.normalized * _moveSpeed, ForceMode.Impulse);

					// Let's rotate the enemy towards the velocity vector (but only if the velocity is more than zero).
					if(_rigidbody.velocity.magnitude > 0.1f) this.transform.forward = _rigidbody.velocity;
				}
			}
			else
			{
				this.transform.position = Vector3.Lerp(this.transform.position, _correctPlayerPos, Time.deltaTime * 10);
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _correctPlayerRot, Time.deltaTime * 20);
			}
		}

		[PunRPC]
		public void TakeDamage(int damage = 1, bool dropAmmo = false)
		{
			_life -= damage;

			if(_life <= 0)
			{
				if(PhotonNetwork.isMasterClient)
				{
					GameController.Instance.AddScore();
					PhotonNetwork.Destroy(this.gameObject);
				}
				Instantiate(GameController.Instance.Explosion, this.transform.position, Quaternion.identity);


				// Drop a random ammo
				if(dropAmmo)
				{
					Vector3 ammoDropPos = this.transform.position;
					ammoDropPos.y = 0.025f;

					if(PhotonNetwork.isMasterClient)
					{
						PhotonNetwork.Instantiate("Ammo", ammoDropPos, Quaternion.identity, 0);
					}
				}
			}
			else
			{
				StartCoroutine(Flash_Coroutine(0.25f));
			}

			if(this.photonView.isMine)
			{
				this.photonView.RPC("TakeDamage", PhotonTargets.OthersBuffered, damage, dropAmmo);
			}
		}

		// Fun little coroutine which flashes the object white.
		private IEnumerator Flash_Coroutine(float time)
		{
			float t = 0f;
			while(t < time)
			{
				t += Time.deltaTime;

				_material.SetColor("_AdditiveColor", Color.Lerp(Color.white, Color.black, t / time));

				yield return null;
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if(collision.gameObject.tag == "Player")
			{
				Player go = collision.gameObject.GetComponent<Player>();
				PhotonView netView = PhotonView.Get(go);
				if(netView.isMine)
				{
					collision.gameObject.GetComponent<Player>().TakeDamage(1, this.transform.forward);
				}
			}
		}

		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if(stream.isWriting)
			{
				stream.SendNext(this.transform.position);
				stream.SendNext(this.transform.rotation);

			}
			else
			{
				_correctPlayerPos = (Vector3)stream.ReceiveNext();
				_correctPlayerRot = (Quaternion)stream.ReceiveNext();
			}
		}
	}
}
