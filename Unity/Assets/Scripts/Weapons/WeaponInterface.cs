using UnityEngine;
using System.Collections;

/// <summary>
/// Weapon interface class that handles visual effects for weapons.
/// </summary>
public class WeaponInterface : MonoBehaviour {
	public ParticleSystem muzzleFlash;
	public ParticleSystem tracers;

	// Use this for initialization
	void Start () {
		if (muzzleFlash != null) muzzleFlash.Stop();
		if (tracers != null) tracers.Stop ();
	}

	public void Fire(Transform target, float t) {
		//transform.LookAt(target/*, transform.up*/);
		Fire (t);
	}

	public void Fire(float t) {
		if (muzzleFlash != null) {
			muzzleFlash.Play();
			StartCoroutine(StopParticles(muzzleFlash, t));
		}
		if (tracers != null) {
			tracers.Play();
			StartCoroutine(StopParticles(tracers, t));
		}
	}

	IEnumerator StopParticles(ParticleSystem ps, float t) {
		yield return new WaitForSeconds(t);

		ps.Stop();
	}
}
