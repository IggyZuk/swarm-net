using UnityEngine;
using System.Collections;

namespace Game
{
	public class Ammo : MonoBehaviour
	{
		void Update()
		{
			this.transform.rotation = Quaternion.AngleAxis(Time.time * 360f, Vector3.up);
		}
	}
}
