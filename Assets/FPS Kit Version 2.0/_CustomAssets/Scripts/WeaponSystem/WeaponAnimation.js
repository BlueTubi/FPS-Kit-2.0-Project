//NSdesignGames @ 2012
//FPS Kit | Version 2.0

#pragma strict
//This script is used to control main weapon animations
//Should be attached to the object that contain weapon/hand animation
var Idle : String = "Idle";
var Reload : String = "Reload";
var Shoot : String = "Fire";
var TakeIn : String = "TakeIn";
var TakeOut : String = "TakeOut";
var FireAnimationSpeed : float = 1;
var TakeInOutSpeed : float = 1;
	
private var PlayThis : String;

private var motor : FPScontroller;
private var player : GameObject;

function Awake () {
	GetComponent.<Animation>().Play(Idle);
	GetComponent.<Animation>().wrapMode = WrapMode.Once;
}

function Fire(){
	GetComponent.<Animation>().Rewind(Shoot);
	GetComponent.<Animation>()[Shoot].speed = FireAnimationSpeed;
	GetComponent.<Animation>().Play(Shoot);
}

function Reloading(reloadTime : float) {
	GetComponent.<Animation>().Stop(Reload);
	GetComponent.<Animation>()[Reload].speed = (GetComponent.<Animation>()[Reload].clip.length/reloadTime);
	GetComponent.<Animation>().Rewind(Reload);
	GetComponent.<Animation>().Play(Reload);
}

function takeIn(){
	GetComponent.<Animation>().Rewind(TakeIn);
	GetComponent.<Animation>()[TakeIn].speed = TakeInOutSpeed;
	GetComponent.<Animation>()[TakeIn].time = 0;
	GetComponent.<Animation>().Play(TakeIn);
}

function takeOut(){
	GetComponent.<Animation>().Rewind(TakeOut);
	GetComponent.<Animation>()[TakeOut].speed = TakeInOutSpeed;
	GetComponent.<Animation>()[TakeOut].time = 0;
	GetComponent.<Animation>().Play(TakeOut);
}

@script AddComponentMenu ("FPS system/Weapon System/WeaponAnimation")