using UnityEngine;
using System.Collections;

namespace Game
{
	public class Projectile : Photon.MonoBehaviour
	{
		[SerializeField]
		private float _moveSpeed;
		[SerializeField]
		private float _totalLifeTime;

		private Rigidbody _rigidbody;

		private float _lifeTime;

		void Awake()
		{
			// Caching a rigid component is good.
			_rigidbody = GetComponent<Rigidbody>();
			_rigidbody.AddForce(this.transform.forward * _moveSpeed, ForceMode.Impulse);
		}

		void Update()
		{
			_lifeTime += Time.deltaTime;
			if(_lifeTime >= _totalLifeTime)
			{
				GameObject.Destroy(this.gameObject);
				Instantiate(GameController.Instance.Explosion, this.transform.position, Quaternion.identity);
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if(collision.gameObject.tag == "Enemy")
			{
				collision.gameObject.GetComponent<Enemy>().TakeDamage(1, Random.value < 0.5f);
				GameObject.Destroy(this.gameObject);
				Instantiate(GameController.Instance.Explosion, this.transform.position, Quaternion.identity);
			}
		}
	}
}
