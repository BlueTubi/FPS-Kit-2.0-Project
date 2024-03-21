//NSdesignGames @ 2012 
//FPS Kit | Version 2.0 + Multiplayer

//Implementation of multiplayer chat inside rooms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerChat : Photon.MonoBehaviour{
	
    public static MultiplayerChat SP;
	
	public struct ChatData { 
	    public string name { get; set; } //Name of sender
	    public string text { get; set; } //Message text
		public Color color { get; set; } //Sender color
		public float timer { get; set; } //Remove message after certain time
		
	    public ChatData(string string1, string string2, Color color1, float timer1){
	       	name = string1; 
	        text = string2;
			color = color1;
			timer = timer1;
    	} 

	} 
	public List<ChatData> messages = new List<ChatData>(); 

    private int chatHeight = (int)300;
    private Vector2 scrollPos = Vector2.zero;
	[HideInInspector]
   	public string chatInput = "";
	[HideInInspector]
	public bool isChatting;
	public GUIStyle chatStyle;
	
	RoomMultiplayerMenu rmm;

    void Awake(){
        SP = this;
		rmm = gameObject.GetComponent<RoomMultiplayerMenu>();
    }
	
	void Update(){
		//Remove chat message after timer reach 0
		for(int i = 0; i < messages.Count; i++){
			ChatData tempData = messages[i];
			tempData.timer -= Time.deltaTime;	
			if(tempData.timer > 0){
				messages[i] = new ChatData(tempData.name, tempData.text, tempData.color, tempData.timer);
			}else{
				messages.RemoveAt(i);	
			}
		}
	}

    void OnGUI(){ 
        GUILayout.BeginArea(new Rect(5, Screen.height - chatHeight-50, Screen.width, chatHeight+10));
        
	        //Show scroll list of chat messages
	        scrollPos = GUILayout.BeginScrollView(scrollPos);
				GUI.color = Color.white;
	
				GUILayout.FlexibleSpace();
				
		        for (int i = 0; i < messages.Count; i++){
					GUILayout.BeginHorizontal("box", GUILayout.Width(10));
						GUI.color = messages[i].color;
		            	GUILayout.Label(messages[i].name, chatStyle);
						GUILayout.Space(5);
						GUI.color = Color.white;
						GUILayout.Label(messages[i].text, chatStyle);
					GUILayout.EndHorizontal();
		        }
	        GUILayout.EndScrollView();
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(5, Screen.height - 35, Screen.width, 30));
	        //Chat input
			if(isChatting){
				GUI.FocusControl("ChatField");
		        GUI.SetNextControlName("ChatField");
		        GUILayout.BeginHorizontal("box", GUILayout.Width(400)); 
					GUI.color = Color.red;
					GUILayout.Label("Say: ", chatStyle);
					GUILayout.Space(5);
					GUI.color = Color.white;
			    	chatInput = GUILayout.TextField(chatInput, chatStyle, GUILayout.Width(400));
		        GUILayout.EndHorizontal();
			}else{
	            GUI.FocusControl("");
			}
		GUILayout.EndArea();
		
		//Open Chat
      	if (Event.current.type == EventType.KeyDown && Event.current.character == 't' && !isChatting){ 
			isChatting = true;
		}
		//Send Chat
      	if (Event.current.type == EventType.KeyDown && Event.current.character == '\n'){   
			isChatting = false;
           	SendChat(PhotonTargets.All);
		}
    }

    void SendChat(PhotonTargets target){
        if (chatInput != ""){
			string tempChat =" " +  chatInput;
            photonView.RPC("SendChatMessage", target, tempChat, (string)PhotonNetwork.player.customProperties["TeamName"]);
            chatInput = "";
        }
    }
	
    [RPC]
    void SendChatMessage(string text, string teamName, PhotonMessageInfo info){
        AddMessage("  " + info.sender + ": ", text, teamName);
    }
	
    void AddMessage(string name, string text, string teamName){
		Color tempColor = new Color();
		if(teamName == rmm.team_1.teamName){
			tempColor = rmm.team_1_Color;
		}else{
			tempColor = rmm.team_2_Color;
		}
		
        SP.messages.Add(new ChatData(name, text, tempColor, 30));
		//Message count limit
        if (SP.messages.Count > 8)
            SP.messages.RemoveAt(0);
    }

    void OnLeftRoom(){
		messages.Clear();
        this.enabled = false;
    }

    void OnJoinedRoom(){
        this.enabled = true;
    }
    void OnCreatedRoom(){
        this.enabled = true;
    }
}
