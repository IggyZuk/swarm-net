using UnityEngine;
using System.Collections;

namespace Game
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField]
		private Transform _target;
		[SerializeField]
		private Vector3 _offset;
		[SerializeField]
		private float _speed;

		void FixedUpdate()
		{
			if(_target != null)
			{
				this.transform.position = Vector3.Lerp(this.transform.position, _target.position + _offset, _speed * Time.deltaTime);
			}
		}

		public void FollowTarget(Transform target)
		{
			_target = target;
		}
	}
}
