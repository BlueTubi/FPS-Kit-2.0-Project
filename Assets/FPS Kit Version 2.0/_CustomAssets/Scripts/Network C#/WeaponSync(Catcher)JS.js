//NSdesignGames @ 2012 
//FPS Kit | Version 2.0 + Multiplayer

//This script is used to catch fire messages from WeaponScript.js
//Then pass those messages to WeaponSync.cs
//Should be attached to every wepaon with WeaponScript.js

#pragma strict

private var playerRoot : Transform;
private var weapScript : WeaponScript;

function Awake(){
	weapScript = gameObject.GetComponent.<WeaponScript>();
	playerRoot = transform.root;
}

function Fire(){
	if(!weapScript){
		Debug.LogError("WeaponScript.js should be attached to same gameObject");
	}
	if(weapScript.GunType == weapScript.gunType.MACHINE_GUN){
		playerRoot.SendMessage("syncMachineGun", weapScript.errorAngle);
	}
	if(weapScript.GunType == weapScript.gunType.SHOTGUN){
		playerRoot.SendMessage("syncShotGun", weapScript.ShotGun.fractions);
	}
	if(weapScript.GunType == weapScript.gunType.GRENADE_LAUNCHER){
		playerRoot.SendMessage("syncGrenadeLauncher", weapScript.grenadeLauncher.initialSpeed);
	}
	if(weapScript.GunType == weapScript.gunType.KNIFE){
		playerRoot.SendMessage("syncKnife");
	}
}