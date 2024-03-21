//NSdesignGames @ 2012
//FPS Kit | Version 2.0

#pragma strict
var turnOn : boolean;
var flashLight : Light;
var OnOffAudio : AudioClip;

function Start () {
	if(turnOn){
		flashLight.enabled = true;
	}else{
		flashLight.enabled = false;
	}
}

function Update () {
	//Flash light input
	if(Input.GetKeyDown(KeyCode.G)){
		turnOn = !turnOn;
		flashLightOnOff();
	}
}

function flashLightOnOff(){
	//Play flash light On/Off sound
	GetComponent.<AudioSource>().clip = OnOffAudio;
	GetComponent.<AudioSource>().Play();
	//Activate flash light
	if(turnOn){
		flashLight.enabled = true;
	}else{
		flashLight.enabled = false;
	}
}
