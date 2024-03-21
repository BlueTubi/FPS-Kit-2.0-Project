//NSdesignGames @ 2012 - 2013
//FPS Kit | Version 2.0 + Multiplayer

//This script is used to controll player health

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDamage : Photon.MonoBehaviour {
	
	public GUISkin guiSKin;
	//Player health
	public float hp = 100;
	public GameObject ragdoll;
	public Texture2D bloodyScreen;
	public Texture2D hitMarkTexture;
	//Hitboxes and damage properties for each
	[System.Serializable]
	public class HitBoxes { 
	    public Collider box /*{ get; set; } */;
	    public float damage /*{ get; set; }*/;
		
	    public HitBoxes(Collider box1, float damage1){
			box = box1;
			damage = damage1;
    	} 

	} 
	public List<HitBoxes> hitBoxes = new List<HitBoxes>(); 
	
	[HideInInspector]
	public float currentHp;
	Quaternion camRot;
	Quaternion camDefaultRotation;
	//Fade hit mark
	float fadeValue;
	//Fade bloody screen
	float fadeValueB;
	[HideInInspector]
	public bool disableDamage = false;
		
	bool weKilled;
	
	RoomMultiplayerMenu rmm;
	
	void Awake(){
		currentHp = hp;
		if(!photonView.isMine){
			for(int i = 0; i < hitBoxes.Count;i++){
				hitBoxes[i].box.gameObject.AddComponent<HitBox>();
				hitBoxes[i].box.gameObject.GetComponent<HitBox>().maxDamage = hitBoxes[i].damage;
				hitBoxes[i].box.gameObject.GetComponent<HitBox>().playerDamage = this;
				hitBoxes[i].box.isTrigger = false;
			}
		}else{
			camDefaultRotation = Camera.main.transform.localRotation;
			for(int a = 0; a < hitBoxes.Count;a++){
				//We dont need our hit boxes, destroy them
				Destroy (hitBoxes[a].box.GetComponent<Collider>());
			}
			hitBoxes.Clear();
		}
		rmm = GameObject.FindWithTag("Network").GetComponent<RoomMultiplayerMenu>();
	}
	
	void Update(){
		fadeValue = Mathf.Lerp(fadeValue, 0, Time.deltaTime*2);	
		fadeValueB = Mathf.Lerp(fadeValueB, 0, Time.deltaTime*2);
		//Do camera shake effect
		if(Camera.main)
			Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, camRot, Time.deltaTime * 15); 
	}
	
	//This is a dmaage our remote player received from Hit Boxes
	public void TotalDamage(float damage){
		if(disableDamage)
			return;
		fadeValue = 2;
	 	photonView.RPC("DoDamage", PhotonTargets.All, damage, PhotonNetwork.player);
	}
	
	[RPC]
	//This is damage sent fro remote player instance to our local
	void DoDamage(float damage, PhotonPlayer player){
		if(weKilled)
			return;
		if(currentHp > 0 && photonView.isMine){
			this.StopAllCoroutines();
		 	StartCoroutine(doCameraShake());
		}
		
		fadeValueB = 2;
		currentHp -= damage;
		
		//We got killed
		if(currentHp < 0){
			//Deactivate all child meshes
			for(int i = 0; i < transform.childCount; i++){
				transform.GetChild(i).gameObject.SetActive(false);	
			}
			
			//Spawn ragdoll
			GameObject temp;
			temp = Instantiate(ragdoll, transform.position, transform.rotation) as GameObject;
			
			if(!photonView.isMine){
				temp.SendMessage("clearCamera");	
				
				if(PhotonNetwork.player == player){
					//Send death notification message to script WhoKilledWho.cs
					GameObject.FindWithTag("Network").SendMessage("AddKillNotification", gameObject.name, SendMessageOptions.DontRequireReceiver);
					//Add 1 kill for our player
					int totalKIlls = (int)PhotonNetwork.player.customProperties["Kills"];
					totalKIlls ++;
					Hashtable setPlayerKills = new Hashtable() {{"Kills", totalKIlls}};
					PhotonNetwork.player.SetCustomProperties(setPlayerKills);
					
					//Add team score
					int teamScore = new int();
					if((string)PhotonNetwork.player.customProperties["TeamName"] == rmm.team_1.teamName){
						teamScore = (int)PhotonNetwork.room.customProperties["Team1Score"];
						teamScore ++;
						Hashtable setTeam1Score = new Hashtable() {{"Team1Score", teamScore}};
						PhotonNetwork.room.SetCustomProperties(setTeam1Score);
					}
					if((string)PhotonNetwork.player.customProperties["TeamName"] == rmm.team_2.teamName){
						teamScore = (int)PhotonNetwork.room.customProperties["Team2Score"];
						teamScore ++;
						Hashtable setTeam2Score = new Hashtable() {{"Team2Score", teamScore}};
						PhotonNetwork.room.SetCustomProperties(setTeam2Score);
					}
				}
			}else{
				//print ("We got killed");
				temp.SendMessage("RespawnAfter");
				//We was killed, add 1 to deaths
				int totalDeaths = (int)PhotonNetwork.player.customProperties["Deaths"];
				totalDeaths ++;
				Hashtable setPlayerDeaths = new Hashtable() {{"Deaths", totalDeaths}};
				PhotonNetwork.player.SetCustomProperties(setPlayerDeaths);
				//Destroy our player
				StartCoroutine(DestroyPlayer(0.2f));
			}
			currentHp = 0;
			weKilled = true;
		}
	}
	
	IEnumerator DestroyPlayer(float delay){
		yield return new WaitForSeconds(delay);	
		PhotonNetwork.Destroy(gameObject);
	}
	
	//Destroy player if we change teams
	void SwapTeams(){
		photonView.RPC("DoSwapTeams", PhotonTargets.All);
	}
	
	[RPC]
	void DoSwapTeams(){
			GameObject temp;
			temp = Instantiate(ragdoll, transform.position, transform.rotation) as GameObject;	
			if(photonView.isMine){
				temp.SendMessage("RespawnAfter");
				StartCoroutine(DestroyPlayer(0));
			}else{
				temp.SendMessage("clearCamera");	
			}
	}
	
	void OnGUI(){
		//Display HP for our player only
		if(photonView.isMine){
			GUI.skin = guiSKin;
			GUI.color = new Color(1,1,1,0.9f);
			GUI.depth = 10;
			GUI.color = new Color(1,1,1,fadeValueB);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bloodyScreen, ScaleMode.StretchToFill );
			GUI.color = new Color(1,1,1,0.9f);
			//Display player hp
			GUI.Box (new Rect (Screen.width - 220,Screen.height - 55,100,45), "HP | " + (int)currentHp);
		}else{
			GUI.color = new Color(1,1,1, fadeValue);
			GUI.DrawTexture(new Rect(Screen.width/2 - 13, Screen.height/2 - 13, 26, 26), hitMarkTexture, ScaleMode.StretchToFill );	
		}
	}
	
	IEnumerator doCameraShake(){
		//Change shake amount here (Currently its 10)
		camRot = Quaternion.Euler (Random.Range(-10, 10), Random.Range(-10, 10), 0);
		yield return new WaitForSeconds(0.1f);
		camRot = camDefaultRotation;
	}
}
