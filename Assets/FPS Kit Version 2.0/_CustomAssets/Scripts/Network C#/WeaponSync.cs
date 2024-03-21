//NSdesignGames @ 2012 - 2013
//FPS Kit | Version 2.0 + Multiplayer

//This script is used to controll third person weapons over network
// It receives messages from PlayerNetworkController.js

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]

public class WeaponSync : MonoBehaviour {
	//We have 4 possible types of weapons (MACHINE_GUN, GRENADE_LAUNCHER, SHOTGUN, KNIFE)
	//So we will have 4 functions to receive message from each of those types
	
	public Transform firePoint;
	
	//Assign this if we synchronize MACHINE_GUN or SHOTGUN or MELEE
	public GameObject bullet;
	public Renderer muzzleFlash;
	
	//Assign this if we synchronize GRENADE_LAUNCHER
	public Rigidbody projectile;
	
	public AudioClip fireAudio;
	
	void Awake(){
		if(muzzleFlash){
			muzzleFlash.GetComponent<Renderer>().enabled = false;	
		}
		GetComponent<AudioSource>().playOnAwake = false;
	}
	
	//MACHINE GUN type weapons sync ************************************************
	void syncMachineGun(float errorAngle){
		this.StopAllCoroutines();
		StartCoroutine(machineGunShot(errorAngle));	
	}
	
	IEnumerator machineGunShot(float errorAngle){
		Quaternion oldRotation = firePoint.rotation;
		firePoint.rotation = Quaternion.Euler(Random.insideUnitSphere * errorAngle) * firePoint.rotation;
		Instantiate (bullet, firePoint.position, firePoint.rotation);
		firePoint.rotation = oldRotation;
		//Play fire sound 
		GetComponent<AudioSource>().clip = fireAudio;
		GetComponent<AudioSource>().Play();
		//Muzzle flash
		if(muzzleFlash){
			muzzleFlash.GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(0.04f);
			muzzleFlash.GetComponent<Renderer>().enabled = false;
		}
	}
	//************************************************************************
	
	//SHOTGUN type weapons sync **************************************************
	void syncShotGun(float fractions){
		this.StopAllCoroutines();
		StartCoroutine(shotGunShot(fractions));	
	}
	
	IEnumerator shotGunShot(float fractions){
		for (int i = 0;i < (int)fractions; i++) {
			Quaternion oldRotation = firePoint.rotation;
			firePoint.rotation = Quaternion.Euler(Random.insideUnitSphere * 3) * firePoint.rotation;
			Instantiate (bullet, firePoint.position, firePoint.rotation);
			firePoint.rotation = oldRotation;
		}
		//Play fire sound 
		GetComponent<AudioSource>().clip = fireAudio;
		GetComponent<AudioSource>().Play();
		//Muzzle flash
		if(muzzleFlash){
			muzzleFlash.GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(0.04f);
			muzzleFlash.GetComponent<Renderer>().enabled = false;
		}
	}
	//************************************************************************
	
	//GRENADE LAUNCHER type weapon sync ******************************************
	void syncGrenadeLauncher(float initialSpeed){
		grenadeLauncherShot(initialSpeed);	
	}
	
	void grenadeLauncherShot(float initialSpeed){
		// create a new projectile, use the same position and rotation as the Launcher.
		Rigidbody instantiatedProjectile = Instantiate(projectile, firePoint.position, firePoint.rotation) as Rigidbody;
		// Give it an initial forward velocity. The direction is along the z-axis of the missile launcher's transform.
		//instantiatedProjectile.velocity = transform.TransformDirection(new Vector3 (initialSpeed, 0, 0));
		instantiatedProjectile.AddForce(instantiatedProjectile.transform.forward * initialSpeed*50);
		// Ignore collisions between the missile and the character controller
		Physics.IgnoreCollision(instantiatedProjectile.GetComponent<Collider>(), transform.root.GetComponent<Collider>());
		foreach(Collider c in transform.root.GetComponentsInChildren<Collider>()){
			Physics.IgnoreCollision(instantiatedProjectile.GetComponent<Collider>(), c);
		}
		
		GetComponent<AudioSource>().clip = fireAudio;
		GetComponent<AudioSource>().Play();
	}
	//************************************************************************
	
	//MELEE type sync ***********************************************************
	void syncKnife(float temp){
		knifeOneHit();	
	}
	
	void knifeOneHit(){
		GetComponent<AudioSource>().clip = fireAudio;
		GetComponent<AudioSource>().Play();
	}
	//************************************************************************
}
