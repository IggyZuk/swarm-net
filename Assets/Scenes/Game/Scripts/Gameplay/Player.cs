using UnityEngine;
using System.Collections;

namespace Game
{
	public class Player : Photon.MonoBehaviour
	{
		[SerializeField]
		private Weapon _weapon;
		[SerializeField]
		private float _moveSpeed;
		[SerializeField]
		private int _totalLife;

		private Rigidbody _rigidbody;
		private Material _material;

		private int _life;
		private bool _isInvincible;

		private Vector3 _correctPlayerPos;
		private Quaternion _correctPlayerRot;
		private Quaternion _correctWeaponRot;

		void Awake()
		{
			// Caching a rigid component is good.
			_rigidbody = GetComponent<Rigidbody>();
			_material = GetComponentInChildren<MeshRenderer>().material; // This might be bad (player holds a weapon)

			_life = _totalLife;
			_isInvincible = false;

			if(this.photonView.isMine)
			{
				GameController.Instance.Camera.FollowTarget(this.transform);

				if(PhotonNetwork.isMasterClient == false)
				{
					this.transform.position += Vector3.right * PhotonNetwork.playerList.Length;
					if(PhotonNetwork.playerList.Length == 2) SetColor(new Vector3(0.1f, 0f, 1f));
					else if(PhotonNetwork.playerList.Length == 3) SetColor(new Vector3(0.5f, 0.4f, 0.1f));
					else if(PhotonNetwork.playerList.Length == 4) SetColor(new Vector3(0f, 0.6f, 1f));
				}
			}
		}

		void Start()
		{
			GameController.Instance.UpdateLives(_life);
		}

		void FixedUpdate()
		{
			if(this.photonView.isMine)
			{
				if(GameController.Instance.CurrentState == GameController.State.Playing)
				{
					// Pick a normalized direction from the inputs.
					Vector3 direction = Vector3.zero;

					if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
					{
						direction += Vector3.forward;
					}
					else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
					{
						direction += Vector3.back;
					}
					if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
					{
						direction += Vector3.left;
					}
					else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
					{
						direction += Vector3.right;
					}

					_rigidbody.AddForce(direction.normalized * _moveSpeed, ForceMode.Impulse);

					// Let's rotate the player towards the velocity vector (but only if the velocity is more than zero).
					if(_rigidbody.velocity.magnitude > 0.1f) this.transform.forward = _rigidbody.velocity;
				}
			}
			else
			{
				this.transform.position = Vector3.Lerp(this.transform.position, _correctPlayerPos, Time.deltaTime * 10);
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _correctPlayerRot, Time.deltaTime * 20);
				_weapon.transform.rotation = Quaternion.Lerp(_weapon.transform.rotation, _correctWeaponRot, Time.deltaTime * 20);
			}
		}

		void Update()
		{
			if(this.photonView.isMine)
			{
				if(Input.GetMouseButton(0))
				{
					_weapon.Shoot();
				}
			}
		}

		[PunRPC]
		public void TakeDamage(int damage, Vector3 dir)
		{
			// Nullify all damage if the player is invincible.
			if(_isInvincible == false)
			{
				_life -= damage;

				if(_life <= 0)
				{
					GameObject.Destroy(this.gameObject);
					Instantiate(GameController.Instance.Explosion, this.transform.position, Quaternion.identity);

					GameController.Instance.GameOver();
				}
				else
				{
					_rigidbody.AddForce(dir * 25f, ForceMode.Impulse);
					StartCoroutine(Flash_Coroutine(2f));

				}
			}

			if(this.photonView.isMine)
			{
				GameController.Instance.UpdateLives(_life);
				this.photonView.RPC("TakeDamage", PhotonTargets.OthersBuffered, damage, dir);
			}
		}

		private IEnumerator Flash_Coroutine(float time)
		{
			_isInvincible = true;

			bool isBlinkOn = false;

			float t = 0f;
			float tBlink = 0f;

			Color originalColor = _material.GetColor("_Color");

			_material.SetColor("_AdditiveColor", Color.white * 0.25f);

			while(t < time)
			{
				t += Time.deltaTime;
				tBlink += Time.deltaTime;

				if(tBlink > 0.025f)
				{
					tBlink = 0f;
					isBlinkOn = !isBlinkOn;
					_material.SetColor("_Color", isBlinkOn ? Color.red : Color.white);
				}

				yield return null;
			}

			_material.SetColor("_Color", originalColor);
			_material.SetColor("_AdditiveColor", Color.black);

			_isInvincible = false;
		}

		void OnTriggerEnter(Collider collider)
		{
			if(this.photonView.isMine)
			{
				if(collider.tag == "Ammo")
				{
					TouchedAmmo(PhotonView.Get(collider.gameObject).viewID, Random.Range(10, 50));
				}
			}
		}

		[PunRPC]
		private void TouchedAmmo(int viewID, int ammo)
		{
			_weapon.AddAmmo(ammo);

			if(PhotonNetwork.isMasterClient)
			{
				PhotonNetwork.Destroy(PhotonView.Find(viewID));
			}

			if(this.photonView.isMine)
			{
				this.photonView.RPC("TouchedAmmo", PhotonTargets.OthersBuffered, viewID, ammo);
			}
		}

		[PunRPC]
		private void SetColor(Vector3 color)
		{
			_material.color = new Color(color.x, color.y, color.z, 1f);
			if(this.photonView.isMine)
			{
				this.photonView.RPC("SetColor", PhotonTargets.OthersBuffered, color);
			}
		}

		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if(stream.isWriting)
			{
				stream.SendNext(this.transform.position);
				stream.SendNext(this.transform.rotation);
				stream.SendNext(_weapon.transform.rotation);

			}
			else
			{
				_correctPlayerPos = (Vector3)stream.ReceiveNext();
				_correctPlayerRot = (Quaternion)stream.ReceiveNext();
				_correctWeaponRot = (Quaternion)stream.ReceiveNext();
			}
		}
	}
}
