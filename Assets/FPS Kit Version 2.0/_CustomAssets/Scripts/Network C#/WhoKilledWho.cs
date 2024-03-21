//NSdesignGames @ 2012 - 2013
//FPS Kit | Version 2.0 + Multiplayer

//This script is used to show on screen who kileld who
//Script should be attached to object with tag "Network"

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WhoKilledWho : Photon.MonoBehaviour {
	
	public GUISkin guiSkin;
	
	public struct WhoKillWho { 
	    public string killer { get; set; } //Name of the player who killed other player
	    public string killed { get; set; } //Name of the player who got killed
		public string middleText { get; set; } //Text displayed in the middle
		public Color killerColor { get; set; } 
		public Color killedColor { get; set; }
		public float timer { get; set; } //Remove notification after certain time
		
	    public WhoKillWho(string string1, string string2, string string3, Color color1, Color color2, float timer1){
	       	killer = string1; 
	        killed = string2;
			middleText = string3;
			killerColor = color1;
			killedColor = color2;
			timer = timer1;
    	} 

	} 
	public List<WhoKillWho> whoKillWho = new List<WhoKillWho>();
	
	RoomMultiplayerMenu rmm;
	
	void Start(){
		rmm = gameObject.GetComponent<RoomMultiplayerMenu>();	
		if(PhotonNetwork.room != null){
			PlayerJoinedRoom();	
		}
	}
	
	void Update(){
		//Remove notification after timer reach 0
		for(int i = 0; i < whoKillWho.Count; i++){
			WhoKillWho tempWKW = whoKillWho[i];
			tempWKW.timer -= Time.deltaTime;	
			if(tempWKW.timer > 0){
				whoKillWho[i] = new WhoKillWho(tempWKW.killer, tempWKW.killed, tempWKW.middleText, tempWKW.killerColor, tempWKW.killedColor, tempWKW.timer);
			}else{
				whoKillWho.RemoveAt(i);	
			}
		}
	}
	
	void OnGUI(){
		GUI.skin = guiSkin;
        GUILayout.BeginArea(new Rect(Screen.width - Screen.width/3.5f, 85, Screen.width/3.5f, 400));
        //Show kill notificatin list
			foreach(WhoKillWho wkw in whoKillWho){
				GUI.color = new Color(1, 1, 1, 0.65f);
				GUILayout.BeginHorizontal("box");
					GUILayout.FlexibleSpace();
					//Killer
					GUI.color = wkw.killerColor;
					GUILayout.Label(wkw.killer);
					GUILayout.Space(5);
					//Middle Text
					GUI.color = new Color(1, 1, 1, 0.65f);
					GUILayout.Label(wkw.middleText);
					GUILayout.Space(5);
					//Killed
					GUI.color = wkw.killedColor;
					GUILayout.Label(wkw.killed);
					GUILayout.Space(10);
				GUILayout.EndHorizontal();
			}
		GUILayout.EndArea();
	}
	
	//Receive message and update List
	void AddKillNotification(string killed){
		photonView.RPC("networkAddMessage", PhotonTargets.All, PhotonNetwork.player.name, killed, "killed", (string)PhotonNetwork.player.customProperties["TeamName"]);
	}
	
	void PlayerJoinedRoom(){
		photonView.RPC("networkAddMessage", PhotonTargets.All, PhotonNetwork.player.name, "", "connected", "");
	}
	
	void PlayerLeftRoom(string playerName){
		photonView.RPC("networkAddMessage", PhotonTargets.All, playerName, "", "left", "");
	}
	
	void PlayerJoinedTeam(string teamName){
		photonView.RPC("networkAddMessage", PhotonTargets.All, PhotonNetwork.player.name, "", "joined " + teamName, "");
	}
	
	[RPC]
	void networkAddMessage(string killer, string killed, string middleText, string teamName){
		Color killerColor = new Color();
		Color killedColor = new Color();
		if(rmm.gameMode == "TDM"){
			if(teamName != ""){
				if(teamName == rmm.team_1.teamName){
					killerColor = rmm.team_1_Color;
					killedColor = rmm.team_2_Color;
				}else{
					killerColor = rmm.team_2_Color;
					killedColor = rmm.team_1_Color;
				}
			}else{
				killerColor = Color.white;
				killedColor = Color.white;
			}
		}else{
			killerColor = rmm.team_1_Color;
			killedColor = rmm.team_1_Color;
		}
		whoKillWho.Add(new WhoKillWho(killer, killed, middleText, killerColor, killedColor, 30));
		//Message count limit
        if (whoKillWho.Count > 5)
            whoKillWho.RemoveAt(0);
	}
	
    void OnLeftRoom(){
		whoKillWho.Clear();
        this.enabled = false;
    }

    void OnJoinedRoom(){
        this.enabled = true;
    }
    void OnCreatedRoom(){
        this.enabled = true;
    }
	
	void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer){
		//Some player just left, show notification to all players
		if(PhotonNetwork.isMasterClient){
			gameObject.SendMessage("PlayerLeftRoom", otherPlayer.name , SendMessageOptions.DontRequireReceiver);
		}
	}
}
