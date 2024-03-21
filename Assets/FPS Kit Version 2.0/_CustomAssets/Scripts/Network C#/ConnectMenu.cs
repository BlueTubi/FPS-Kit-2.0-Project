//NSdesignGames @ 2012 - 2013
//FPS Kit | Version 2.0 + Multiplayer

//Implementation of Photon Networking system
//This script handle connection to rooms

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectMenu : Photon.MonoBehaviour  {
	
	public GUISkin guiSKin;
	//Use this for loading splash screen
	public Texture blackScreen;
	public Texture top;
	public Texture bottom;
	
	//Defne Round duration in seconds
	public int roundDuration = 600;
	
	//Room settings
	List<int> maxPlayersOptions = new List<int>();
	
	//Setup available maps
	[System.Serializable]
	public class AllMaps{
		public string mapName;
		public Texture2D mapPreview;
	}
	public List<AllMaps> allMaps;
	
	//Room/Player names
	string newRoomName;
	string playerName;
	int maxPlayers;
	int selectedMap;
	string gameMode;
	
	Vector2 scroll;
	Vector2 mapScroll;
	
	//Fade black screen
	float fadeValue = new float();
	int fadeDir;
	
	//Save info about all Photon Rooms
	RoomInfo[] allRooms;
	bool createRoom = false;
	bool connectingToRoom = false;
	
	// Use this for initialization
	void Start () {
		PhotonNetwork.isMessageQueueRunning = true;
		Screen.lockCursor = false;
		allRooms = PhotonNetwork.GetRoomList();
		newRoomName = "Room Name " + Random.Range(111, 999);
		playerName = "Player " + Random.Range(111, 999);
		
		//Here we setup possible numbers for max players in one room
		maxPlayersOptions.Add(4);
		maxPlayersOptions.Add(8);
		maxPlayersOptions.Add(12);
		maxPlayersOptions.Add(16);
		maxPlayers = maxPlayersOptions[2];
		selectedMap = 0;
		
		//Prevent roundDuration being less than 1
		if(roundDuration == 0){
			roundDuration = 600;	
		}
		gameMode = "TDM";
		
		//Load player name if it was saved
		if(PlayerPrefs.HasKey("PlayerName")){
			playerName = PlayerPrefs.GetString("PlayerName");	
		}
	}
	
	void Update(){
		//Try to reconnect every 3 seconds
		float updateRate = 3;
		float nextUpdateTime = 0;
		//Do not try connect every frame, but using small intervals (To avoid lag while failed to connect)
		if(!PhotonNetwork.connected){
			if (Time.time - updateRate > nextUpdateTime){
				nextUpdateTime = Time.time - Time.deltaTime;
			}
			// Keep firing until we used up the fire time
			while(nextUpdateTime < Time.time){
				PhotonNetwork.ConnectUsingSettings("v0.0.1");
				nextUpdateTime += updateRate;
			}
		}
		
		//FInd all existing rooms automatically
		if(PhotonNetwork.connected && allRooms.Length != PhotonNetwork.GetRoomList().Length){
			allRooms = PhotonNetwork.GetRoomList();
		}
	}
	
	// Update is called once per frame
	void OnGUI () {
		GUI.skin = guiSKin;
		
		GUI.color = Color.white;
		GUI.depth = -2;
		//If we havnt connected to Photon Cloud do not allow player to join/create room 
		if(!PhotonNetwork.connected){
			GUI.enabled = false;	
		}else{
			GUI.enabled = true;
		}
		
		//Other GUI
		GUI.color = new Color(1,1,1, 0.9f);
		GUI.DrawTexture(new Rect(0 ,0, top.width, top.height), top, ScaleMode.ScaleToFit);
		GUI.DrawTexture(new Rect(Screen.width - bottom.width , Screen.height - bottom.height, bottom.width, bottom.height), bottom, ScaleMode.ScaleToFit);
		GUI.color = Color.white;
		
		//Main Menu
		GUILayout.BeginArea(new Rect (Screen.width/2 - 250, Screen.height/2 - 150, 500, 330), "FPS Kit 2.0 | Multiplayer Preview v2.7", GUI.skin.GetStyle("window"));
			ShowConnectMenu();
		GUILayout.EndArea();
		
		if(!PhotonNetwork.connected){
			GUI.color = Color.white;
			GUI.Box (new Rect(Screen.width/2 - 75, Screen.height/2 - 15, 150, 30), "Connecting...");
		}
		
		//Fade black screen when connecting to Room
		FadeScreen();
	}
	
	void ShowConnectMenu(){
		GUILayout.Space (10);	
		if(!createRoom){
			//DIsplay all available rooms and Join selected room
			scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(480), GUILayout.Height(225));{
				foreach(RoomInfo room in allRooms){
					if(allRooms.Length > 0){
						GUILayout.BeginHorizontal("box");
							//Room name
							GUILayout.Label(room.name, GUILayout.Width(150));
							//Map name
							GUILayout.Label((string)room.customProperties["MapName"], GUILayout.Width(135));
							//Player count
							GUILayout.Label(room.playerCount + "/" + room.maxPlayers, GUILayout.Width(60));
							GUILayout.FlexibleSpace();
							if(GUILayout.Button("Join Room", GUILayout.Width(100))){
								//Join a room
								PhotonNetwork.JoinRoom(room.name);
								PhotonNetwork.playerName = playerName;
								connectingToRoom = true;
								
								//Save player name
								CheckPlayerNameAndRoom();
								PlayerPrefs.SetString("PlayerName", playerName);
							}
						GUILayout.EndHorizontal();
					}
				}
				if(allRooms.Length == 0){
					GUILayout.Label("No rooms created...");
				}
			GUILayout.EndScrollView();}
			
			GUILayout.Space(5);
			
			//Choose player name
			GUILayout.BeginHorizontal();
				GUILayout.Label("Player Name: ");
				playerName = GUILayout.TextField (playerName, 15, GUILayout.Height(25));
			GUILayout.EndHorizontal();
			
			//Go to room creation menu
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Create Room", GUILayout.Width(130), GUILayout.Height(25))){
					createRoom = true;
					//Save player name
					CheckPlayerNameAndRoom();
					PlayerPrefs.SetString("PlayerName", playerName);
				}
			GUILayout.EndHorizontal();
		}else{
			//Create new room***************************************************************************
			
			//Room name
			GUILayout.BeginHorizontal();
				GUILayout.Label("Room Name: ", GUILayout.Width(130));
				newRoomName = GUILayout.TextField (newRoomName, 15, GUILayout.Height(25));
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5);
			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Max Players: ", GUILayout.Width(130));
				for(int i = 0; i < maxPlayersOptions.Count; i++){
					if(maxPlayers == maxPlayersOptions[i]){
						//Selected number is green
						GUI.color = Color.green;
					}else{
						GUI.color = Color.white;
					}
					if(GUILayout.Button(maxPlayersOptions[i].ToString(), GUILayout.Width(27), GUILayout.Height(25))){
						maxPlayers = maxPlayersOptions[i];
					}
				}
				GUI.color = Color.white;
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5);
			
			//Select Game Mode *****************************************************************
			GUILayout.BeginHorizontal();
				GUILayout.Label("Game Mode: ", GUILayout.Width(130));
			
				if(gameMode == "TDM"){
					GUI.color = Color.green;
				}
				if(GUILayout.Button("TeamDeathmatch", GUILayout.Width(140), GUILayout.Height(25))){
					gameMode = "TDM";
				}
			
				GUI.color = Color.white;
			
				if(gameMode == "DM"){
					GUI.color = Color.green;
				}
				if(GUILayout.Button("Deathmatch", GUILayout.Width(140), GUILayout.Height(25))){
					gameMode = "DM";
				}
			GUILayout.EndHorizontal();
			//*******************************************************************************
			GUI.color = Color.white;
			
			GUILayout.Space(5);
			
			GUILayout.BeginHorizontal();
				mapScroll = GUILayout.BeginScrollView(mapScroll, false, true, GUILayout.Width(240), GUILayout.Height(160));
					for(int i = 0; i < allMaps.Count; i++){
						if(selectedMap == i){
							//Selected map is green
							GUI.color = Color.green;
						}else{
							GUI.color = Color.white;
						}
						if(GUILayout.Button(allMaps[i].mapName, GUILayout.Height(25))){
							selectedMap = i;
						}
					}
					GUI.color = Color.white;
				GUILayout.EndScrollView();
				GUILayout.Space(10);
			
				//Show map preview image
				if(allMaps[selectedMap].mapPreview != null){
					GUILayout.Label(allMaps[selectedMap].mapPreview, GUILayout.Width(230), GUILayout.Height(160));
				}
			GUILayout.EndHorizontal();
			
			GUILayout.FlexibleSpace();
			 
			
			GUILayout.BeginHorizontal();
				//Return to Lobby
				if(GUILayout.Button("Main Menu", GUILayout.Width(130), GUILayout.Height(25))){
					createRoom = false;
				}
			
				GUILayout.FlexibleSpace();
			
				//Create our room **********************************************************
				if(GUILayout.Button("Continue", GUILayout.Width(130), GUILayout.Height(25))){
					CheckPlayerNameAndRoom();
					PhotonNetwork.player.name = playerName;
					Hashtable setMapName = new Hashtable(); 
					setMapName["MapName"] = allMaps[selectedMap].mapName;
					setMapName["RoundDuration"] = roundDuration;
					setMapName["GameMode"] = gameMode;
		            string[] exposedProps = new string[3];
		            exposedProps[0] = "MapName";
					exposedProps[1] = "RoundDuration";
					exposedProps[2] = "GameMode";
					//Create new Room
					PhotonNetwork.CreateRoom(newRoomName, true, true, maxPlayers, setMapName, exposedProps);
				}
			GUILayout.EndHorizontal();
			//****************************************************************************************
		}
	}
	
	void FadeScreen(){
		if(connectingToRoom){
			//Show loadign screen while we connection to room and loading scene
			fadeDir = 1;
			fadeValue += fadeDir * 15 * Time.deltaTime;	
			fadeValue = Mathf.Clamp01(fadeValue);	

			GUI.color = new Color(1,1,1,fadeValue);
			GUI.DrawTexture(new Rect(0,0, Screen.width, Screen.height), blackScreen);	
					
			GUI.color = Color.white;
			GUI.Label(new Rect(Screen.width/2 - 75, Screen.height/2 - 15, 150, 30), "Loading...");
		}
	}
	
	IEnumerator LoadMap(string sceneName){
		connectingToRoom = true;
		PhotonNetwork.isMessageQueueRunning = false;
		fadeDir = 1;
		yield return new WaitForSeconds(1);

        Application.backgroundLoadingPriority = ThreadPriority.High;
		AsyncOperation async = Application.LoadLevelAsync(sceneName);     
		yield return async; 
		Debug.Log("Loading complete");  
	}
	
	void CheckPlayerNameAndRoom(){
		//Player Name can't be empty
		string tempName = playerName.Replace(" ", "");
		if(tempName == ""){
			playerName = "Player " + Random.Range(111, 999);
		}
		//Room Name can't be empty
		string tempRoomName = newRoomName.Replace(" ", "");
		if(tempRoomName == ""){
			newRoomName = "Room Name " + Random.Range(111, 999);
		}
	}
	
	//Functions that are called on certan Photon action (Connected, Disconencted etc.) ************************************************************
	void OnJoinedRoom(){
		print ("Joined room: " + newRoomName);
		connectingToRoom = true;
		//We joined room, load respective map
		StartCoroutine(LoadMap((string)PhotonNetwork.room.customProperties["MapName"]));
	}
	
	void OnJoinedLobby(){
		print ("Joined master server");
	}
	
	void OnLeftRoom(){
		connectingToRoom = false;
	}
	
	void OnPhotonJoinRoomFailed(){
		print ("Failed on connecting to room");
		connectingToRoom = false;
	}
	
	void OnPhotonCreateRoomFailed(){
		print ("Failed on creating room");
		connectingToRoom = false;
	}
	
	void OnPhotonPlayerConnected(){
		print ("Player connected");	
	}
	
	void OnConnectedToPhoton(){
		print ("We connected to Photon Cloud");
		if(PhotonNetwork.room != null){
			PhotonNetwork.LeaveRoom();
		}
		connectingToRoom = false;

	}
	
	void OnDisconnectedFromPhoton(){	
		print ("We disconencted from Photon Cloud");
	}
	//*********************************************************************************************************************
}
