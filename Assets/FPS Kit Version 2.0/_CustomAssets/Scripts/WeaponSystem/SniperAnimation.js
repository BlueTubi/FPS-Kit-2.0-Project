//NSdesignGames @ 2012
//FPS Kit | Version 2.0

#pragma strict
//This script is used to control main weapon animations
//Should be attached to the object that contain weapon/hand animation
//Note this is Modifired version of WeaponAnimation.js and its adapted to work with per bullet reload animation
var Idle : String = "Idle";
var ReloadBegin : String = "Reload_1_3";
var ReloadMiddle : String = "Reload_2_3";
var ReloadEnd: String = "Reload_3_3";
var Shoot : String = "Fire";
var TakeIn : String = "TakeIn";
var TakeOut : String = "TakeOut";
var FireAnimationSpeed : float = 1;
var TakeInOutSpeed : float = 1;
var ReloadMiddleRepeat : float = 4;
	
private var PlayThis : String;

private var motor : FPScontroller;
private var player : GameObject;

function Awake () {
	GetComponent.<Animation>().Play(Idle);
	GetComponent.<Animation>()[Idle].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()[ReloadBegin].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()[ReloadMiddle].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()[ReloadEnd].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()[Shoot].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()[TakeIn].wrapMode = WrapMode.Once;
	GetComponent.<Animation>()[TakeOut].wrapMode = WrapMode.Once;
}

function Fire(){
	GetComponent.<Animation>().Rewind(Shoot);
	GetComponent.<Animation>()[Shoot].speed = FireAnimationSpeed;
	GetComponent.<Animation>().Play(Shoot);
}

function Reloading(reloadTime : float) {
	var totalLength = GetComponent.<Animation>()[ReloadBegin].clip.length + GetComponent.<Animation>()[ReloadMiddle].clip.length*ReloadMiddleRepeat + GetComponent.<Animation>()[ReloadEnd].clip.length;
	
	var newReload1 : AnimationState = GetComponent.<Animation>().CrossFadeQueued(ReloadBegin);
	newReload1.speed = (totalLength/reloadTime)/2;
	//4 is number of bullets to reload
	for(var i : int = 0; i < ReloadMiddleRepeat; i++){
 		var newReload2 : AnimationState = GetComponent.<Animation>().CrossFadeQueued(ReloadMiddle);
		newReload2.speed = (totalLength/reloadTime)/1.4;
	}
	var newReload3 : AnimationState = GetComponent.<Animation>().CrossFadeQueued(ReloadEnd);
	newReload3.speed = (totalLength/reloadTime)/2;
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

@script AddComponentMenu ("FPS system/Weapon System/SniperAnimation")