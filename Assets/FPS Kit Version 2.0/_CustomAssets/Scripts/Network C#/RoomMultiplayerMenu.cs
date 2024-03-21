//NSdesignGames @ 2012 - 2013
//FPS Kit | Version 2.0 + Multiplayer

//This script handle logic inside room

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomMultiplayerMenu : Photon.MonoBehaviour {

	public GUISkin guiSkin;
	public GameObject playerPrefab;
	public Texture2D blackScreen;
	public GameObject roomCamera;

	
	//Setup available teams (This include team name and spawn points for each)
	[System.Serializable]
	public class AllTeams{
		[HideInInspector]
		public string teamName;
		public List<Transform> spawnPoints;
	}
	
	public AllTeams team_1;
	public AllTeams team_2;
	
	[HideInInspector]
	public Color team_1_Color = Color.cyan;
	[HideInInspector]
	public Color team_2_Color = Color.yellow;
	
	[HideInInspector]
	public bool isPaused;
	GameObject enableHelper;
	
	bool playerList;
	Resolution[] resolutions;
	string[] QualityNames;
	int resolutionIndex = 3;
	Vector2 scroll;
	Vector2 scroll2;
	Vector2 scroll3;
	
	//Our player spawned
	GameObject Player;
	
	int fadeDir = 0;
	float fadeValue;
	
	//Update ping every 5 seconds
	float updateRate = 5;
	float nextUpdateTime = 0;
	
	List<PhotonPlayer> allPlayers = new List<PhotonPlayer>();
	
	//Join all spawn points for Dm mode
	List<Transform> allSpawnPoints = new List<Transform>();
	
	//Store round duration
	int roundDuration;
	float referenceTime;
	//Round countdown
	float currentRoundTime;
	[HideInInspector]
	public string gameMode;
	
	bool roundEnded = false;
	
	int team1Score = 0;
	int team2Score = 0;
	
	//Display this when round over
	string finalText = "";
	
	//Save first player name for Dm mode
	string leadingPlayer;
	
	//Sort players by kills
	private static int SortPlayers(PhotonPlayer A, PhotonPlayer B){
		if(B.customProperties["Kills"] != null && A.customProperties["Kills"] != null){
        	return (int)B.customProperties["Kills"] - (int)A.customProperties["Kills"];
		}else{
			return 0;	
		}
    }
	
	// Use this for initialization
	void Awake () {
		PhotonNetwork.isMessageQueueRunning = true;
		isPaused = true;
		resolutions = Screen.resolutions;
		resolutionIndex = (resolutions.Length-1)/2;
		QualityNames = QualitySettings.names;
		playerList = true;
		enableHelper = GameObject.FindWithTag("EnableHelper").gameObject;
		
		//Setup team names ********************************
		team_1.teamName = "Team A";
		team_2.teamName = "Team B";
		//Set player colors *********************************
		team_1_Color = Color.cyan;
		team_2_Color = Color.yellow;
		//*********************************************
		
		//Get round time
		roundDuration = (int)PhotonNetwork.room.customProperties["RoundDuration"];
		gameMode = (string)PhotonNetwork.room.customProperties["GameMode"];
		
		//Setup all player properties
		Hashtable setPlayerTeam = new Hashtable() {{"TeamName", "Spectators"}};
		PhotonNetwork.player.SetCustomProperties(setPlayerTeam);
		
		Hashtable setPlayerKills = new Hashtable() {{"Kills", 0}};
		PhotonNetwork.player.SetCustomProperties(setPlayerKills);
		
		Hashtable setPlayerDeaths = new Hashtable() {{"Deaths", 0}};
		PhotonNetwork.player.SetCustomProperties(setPlayerDeaths);
		

		//if WeaponSync master client, store reference tiem for others
		if(PhotonNetwork.isMasterClient){
			referenceTime = (float)PhotonNetwork.time;
			//And store in room properties given reference time
			Hashtable setReferenceTime = new Hashtable() {{"RefTime", referenceTime}};
			PhotonNetwork.room.SetCustomProperties(setReferenceTime);
					
			//Setup team scores
			Hashtable setTeam1Score = new Hashtable() {{"Team1Score", 0}};
			PhotonNetwork.room.SetCustomProperties(setTeam1Score);
			
			Hashtable setTeam2Score = new Hashtable() {{"Team2Score", 0}};
			PhotonNetwork.room.SetCustomProperties(setTeam2Score);
		}else{
			//Get saved reference time
			referenceTime = (float)PhotonNetwork.room.customProperties["RefTime"];
		}
		
		//Join all spawn points in one list for DM mode
		allSpawnPoints.Clear();
		foreach(Transform point in team_1.spawnPoints){
			allSpawnPoints.Add(point);
		}
		foreach(Transform point in team_2.spawnPoints){
			allSpawnPoints.Add(point);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Tab)){
			isPaused = !isPaused;
		}

		if(Input.GetKeyDown(KeyCode.P)){
			Screen.fullScreen = !Screen.fullScreen;
			if(!Screen.fullScreen){
				Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, true);
			}
		}
		
		if(isPaused){
			if(enableHelper.activeSelf == true){
				enableHelper.SetActive(false);
			}	
			Screen.lockCursor = false;
		}else{
			if(enableHelper.activeSelf == false){
				enableHelper.SetActive(true);
			}
			Screen.lockCursor = true;
		}
		
				
		if (Time.time - updateRate > nextUpdateTime){	
			nextUpdateTime = Time.time - Time.deltaTime;
		}
		
		//Send player ping every 5 seconds
		while(nextUpdateTime < Time.time){
			Hashtable setPlayerPing = new Hashtable() {{"Ping", PhotonNetwork.GetPing()}};
			PhotonNetwork.player.SetCustomProperties(setPlayerPing);
			nextUpdateTime += updateRate;
		}
	}
	
	void FixedUpdate(){
		if(isPaused || gameMode == "DM"){
			allPlayers.Clear();
			foreach(PhotonPlayer player in PhotonNetwork.playerList){
				allPlayers.Add (player);
			}
			
			if(allPlayers != null){
				allPlayers.Sort(SortPlayers);
				leadingPlayer = allPlayers[0].name;
			}
		}
		
		//Track round countdown timer
		float tempRoundTime = roundDuration - ((float)PhotonNetwork.time - referenceTime);
		if(tempRoundTime > 0){
			currentRoundTime = tempRoundTime;
		}else{
			currentRoundTime = 0;
			if(!roundEnded){
				StartCoroutine(RoundEnded());
				isPaused = false;
				roundEnded = true;
			}
		}
		
		//Get team scores
		if(PhotonNetwork.room != null){
			team1Score = (int)PhotonNetwork.room.customProperties["Team1Score"];
			team2Score = (int)PhotonNetwork.room.customProperties["Team2Score"];
		}
	}
	
	IEnumerator RoundEnded(){
		gameMode = (string)PhotonNetwork.room.customProperties["GameMode"];
		if(gameMode == "TDM"){
			if(team1Score == team2Score){
				finalText = "Draw... Restarting";
			}
			if(team1Score > team2Score){
				finalText = team_1.teamName + " Won... Restarting";
			}
			if(team1Score < team2Score){
				finalText = team_2.teamName + " Won... Restarting";
			}
		}else{
			finalText = leadingPlayer + " is Winner... Restarting";
		}
		//Call this when round time is over and move players to lobby
		yield return new WaitForSeconds(5);
		StartCoroutine(Restart());
	}
	
	//Restart round **************************************************************************
	IEnumerator Restart(){
		//Reset player properties
		Hashtable setPlayerKills = new Hashtable() {{"Kills", 0}};
		PhotonNetwork.player.SetCustomProperties(setPlayerKills);
		
		Hashtable setPlayerDeaths = new Hashtable() {{"Deaths", 0}};
		PhotonNetwork.player.SetCustomProperties(setPlayerDeaths);
		

		//if WeaponSync master client, store reference tiem for others
		if(PhotonNetwork.isMasterClient){
			referenceTime = (float)PhotonNetwork.time;
			//And store in room properties given reference time
			Hashtable setReferenceTime = new Hashtable() {{"RefTime", referenceTime}};
			PhotonNetwork.room.SetCustomProperties(setReferenceTime);
					
			//Reset team scores
			Hashtable setTeam1Score = new Hashtable() {{"Team1Score", 0}};
			PhotonNetwork.room.SetCustomProperties(setTeam1Score);
			
			Hashtable setTeam2Score = new Hashtable() {{"Team2Score", 0}};
			PhotonNetwork.room.SetCustomProperties(setTeam2Score);
		}else{
			//Get saved reference time
			while(referenceTime == (float)PhotonNetwork.room.customProperties["RefTime"]){
				yield return null;
			}
			referenceTime = (float)PhotonNetwork.room.customProperties["RefTime"];
		}
		
		if(Player){
			PhotonNetwork.Destroy(Player);
		}
		
		yield return new WaitForSeconds(0.2f);
		
		if((string)PhotonNetwork.player.customProperties["TeamName"] != "Spectators"){
			SpawnPlayer((string)PhotonNetwork.player.customProperties["TeamName"]);	
		}
		roundEnded = false;
	}
	//***********************************************************************************
		
	void OnGUI(){
		GUI.skin = guiSkin;
	 	GUI.Label(new Rect(Screen.width-190, Screen.height-80, 190, 20), " Tab - pause menu");
		
		GUI.color = new Color(1, 1, 1, 0.7f);
	    if(isPaused){
			GUI.color = new Color(1, 1, 1, 0.7f);
	    	GUI.Window (0, new Rect (Screen.width/2 - 250, Screen.height/2 - 210, 500, 500), MainMenu, "Room: " + PhotonNetwork.room.name + " | Game Mode: " + gameMode);
	    }
		
		//Display round countdown timer
		float roundedRestSeconds = Mathf.CeilToInt(currentRoundTime);
		int displaySeconds = Mathf.FloorToInt(roundedRestSeconds % 60);
		int displayMinutes = Mathf.FloorToInt((roundedRestSeconds / 60)%60);
    	string niceTime = string.Format("{0:00}:{1:00}", displayMinutes, displaySeconds);

		GUI.Box(new Rect(Screen.width/2 - 50, 45, 100, 30), niceTime);
		
		//Display Team Scores
		if(gameMode == "TDM"){
			GUILayout.BeginArea(new Rect(Screen.width/3 - 100, 45, 200, 30));
				GUILayout.BeginHorizontal("box", GUILayout.Width(200), GUILayout.Height(30));
					GUI.color = team_1_Color;
					GUILayout.Label(team_1.teamName + ":");
					GUILayout.Space(5);
					GUI.color = Color.white;
					GUILayout.Label(team1Score.ToString());
				GUILayout.EndHorizontal();
			GUILayout.EndArea();
			
			GUILayout.BeginArea(new Rect(Screen.width - Screen.width/3 - 100, 45, 200, 30));
				GUILayout.BeginHorizontal("box", GUILayout.Width(200), GUILayout.Height(30));
					GUI.color = team_2_Color;
					GUILayout.Label(team_2.teamName + ":");
					GUILayout.Space(5);
					GUI.color = Color.white;
					GUILayout.Label(team2Score.ToString());
				GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}else{
			GUI.color = Color.white;
			GUI.Box(new Rect(Screen.width/2 - 150, 80, 300, 30), "Leading Player: " + leadingPlayer);
		}
		
		GUI.color = Color.white;
		
		if(roundEnded){
			GUI.Box(new Rect(Screen.width/2 - 200, Screen.height/2 - 100, 400, 30), finalText);	
		}
		
		//Fade black screen when returning to Lobby
		FadeScreen();
	}
	
	void MainMenu (int windowID) { 
		GUI.FocusWindow(windowID);
		GUILayout.Space (10); 
		
		GUILayout.BeginHorizontal();
			Resolutions();
			QualityWindow();
			
			GUI.color = Color.white;
		
	 		GUILayout.Space (15); 
		
			GUILayout.BeginVertical();
				//Select Team to Join
				if(gameMode == "TDM"){
					//Team 1
					if((string)PhotonNetwork.player.customProperties["TeamName"] == team_1.teamName 
					|| (!Player && (string)PhotonNetwork.player.customProperties["TeamName"] != "Spectators" || roundEnded)){
						GUI.enabled = false;
					}else{
						GUI.enabled = true;
					}
					if(GUILayout.Button(team_1.teamName)){
						//Kill current player if exist
						if(!Player){
							SpawnPlayer(team_1.teamName);
						}else{
							SwapTeams(team_1.teamName);
						}
						isPaused = false;
						//Player joined team, notify everyone
						gameObject.SendMessage("PlayerJoinedTeam", team_1.teamName, SendMessageOptions.DontRequireReceiver);
					}
			
					//Team 2
					if((string)PhotonNetwork.player.customProperties["TeamName"] == team_2.teamName 
					|| (!Player && (string)PhotonNetwork.player.customProperties["TeamName"] != "Spectators" || roundEnded)){
						GUI.enabled = false;
					}else{
						GUI.enabled = true;
					}
					if(GUILayout.Button(team_2.teamName)){
						//Kill current player if exist
						if(!Player){
							SpawnPlayer(team_2.teamName);
						}else{
							SwapTeams(team_2.teamName);
						}
						isPaused = false;
						//Player joined team, notify everyone
						gameObject.SendMessage("PlayerJoinedTeam", team_2.teamName, SendMessageOptions.DontRequireReceiver);
					}
				}else{
					//Join game with Deathmatch mode
					if(Player || (string)PhotonNetwork.player.customProperties["TeamName"] != "Spectators"){
						GUI.enabled = false;
					}else{
						GUI.enabled = true;
					}
					if(GUILayout.Button("Join")){
						//Kill current player if exist
						if(!Player){
							SpawnPlayer(team_1.teamName);
						}else{
							SwapTeams(team_1.teamName);
						}
						isPaused = false;
						//Player joined team, notify everyone
						gameObject.SendMessage("PlayerJoinedTeam", "battle", SendMessageOptions.DontRequireReceiver);
					}
				}
		
				GUI.enabled = true;
		
				if(Player){
					if(GUILayout.Button("Resume")){
						isPaused = false;
					}
				}

				if(GUILayout.Button("Leave Room")){
					LeaveRoom();
				}
			GUILayout.EndVertical();
 		GUILayout.EndHorizontal();
		
	    GUILayout.Space (10);
		GUILayout.BeginHorizontal();
			if(playerList){
				GUI.color = new Color(0, 20, 0, 0.6f);
			}else{
				GUI.color = Color.white;
			}
			if(GUILayout.Button("Player List", GUILayout.Width(150), GUILayout.Height(25))){
				playerList = true;
			}
			if(!playerList){
				GUI.color = new Color(0, 20, 0, 0.6f);
			}else{
				GUI.color = Color.white;
			}
			if(GUILayout.Button("Controls", GUILayout.Width(150), GUILayout.Height(25))){
				playerList = false;
			}
		GUILayout.EndHorizontal();
				
	    GUILayout.Space (5);
	    GUI.color = Color.white;
	    scroll3 = GUILayout.BeginScrollView(scroll3, GUILayout.Width(480), GUILayout.Height(300));
			if(!playerList){
				//Show controls
		    	GUI.color = new Color(20, 20,0, 0.6f);
			    GUILayout.Label("Tab - Pause Menu"); 
			    GUILayout.Label("P - Fullscreen"); 
				GUILayout.Label("T - Chat / Enter - send"); 
			    GUILayout.Label("C - crouch");
			    GUILayout.Label("Left Ctrl - prone"); 
			    GUILayout.Label("LMB - fire"); 
			    GUILayout.Label("RMB - aim");
			    GUILayout.Label("F - weapon pick up");
			    GUILayout.Label("R - reload");
			    GUILayout.Label("Left Shift - run");
			    GUILayout.Label("Space - jump");
			    GUILayout.Label("1/2 - weapon change");
			    GUILayout.Label("While selected STW-25 press G for flashlight");
			}else{
				//Show player list***
				GUI.color = new Color(1,1,1,0.8f);
			
				if(gameMode == "TDM"){
					//Display Team 1 **************************************************************************
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUI.color = team_1_Color;
						GUILayout.Label(team_1.teamName);
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
			
					foreach(PhotonPlayer player in allPlayers){
						if((string)player.customProperties["TeamName"] == team_1.teamName){
							if(PhotonNetwork.player.name == player.name){
								GUI.color = Color.yellow;
							}else{
								GUI.color = Color.white;
							}
							GUILayout.BeginHorizontal("box");{
								GUILayout.Label(player.name, GUILayout.Width(150));
								GUILayout.Label("Kills: " + ((int)player.customProperties["Kills"]).ToString(), GUILayout.Width(115));
								GUILayout.Label("Deaths: " + ((int)player.customProperties["Deaths"]).ToString(), GUILayout.Width(115));
								GUILayout.FlexibleSpace();
								if(player.customProperties["Ping"] != null){
									GUILayout.Label("Ping: " + ((int)player.customProperties["Ping"]).ToString());
								}
							GUILayout.EndHorizontal();}
						}
					}
					//*************************************************************************************
			
					//Display Team 2 **************************************************************************
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUI.color = team_2_Color;
						GUILayout.Label(team_2.teamName);
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
			
					foreach(PhotonPlayer player in allPlayers){
						if((string)player.customProperties["TeamName"] == team_2.teamName){
							if(PhotonNetwork.player.name == player.name){
								GUI.color = Color.yellow;
							}else{
								GUI.color = Color.white;
							}
							GUILayout.BeginHorizontal("box");{
								GUILayout.Label(player.name, GUILayout.Width(150));
								GUILayout.Label("Kills: " + ((int)player.customProperties["Kills"]).ToString(), GUILayout.Width(115));
								GUILayout.Label("Deaths: " + ((int)player.customProperties["Deaths"]).ToString(), GUILayout.Width(115));
								GUILayout.FlexibleSpace();
								if(player.customProperties["Ping"] != null){
									GUILayout.Label("Ping: " + ((int)player.customProperties["Ping"]).ToString());
								}
							GUILayout.EndHorizontal();}
						}
					}
					//*************************************************************************************
				}else{
					//If game mode is Deathmatch, display all players **************************************************
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUI.color = Color.green;
						GUILayout.Label("All Players");
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
			
					foreach(PhotonPlayer player in allPlayers){
						if((string)player.customProperties["TeamName"] != "Spectators"){
							if(PhotonNetwork.player.name == player.name){
								GUI.color = Color.yellow;
							}else{
								GUI.color = Color.white;
							}
							GUILayout.BeginHorizontal("box");{
								GUILayout.Label(player.name, GUILayout.Width(150));
								GUILayout.Label("Kills: " + ((int)player.customProperties["Kills"]).ToString(), GUILayout.Width(115));
								GUILayout.Label("Deaths: " + ((int)player.customProperties["Deaths"]).ToString(), GUILayout.Width(115));
								GUILayout.FlexibleSpace();
								if(player.customProperties["Ping"] != null){
									GUILayout.Label("Ping: " + ((int)player.customProperties["Ping"]).ToString());
								}
							GUILayout.EndHorizontal();}
						}
					}
				}
				//Display Spctators ****************************************************************************
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUI.color = Color.green;
					GUILayout.Label("- Spectators -");
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
		
				foreach(PhotonPlayer player in allPlayers){
					if((string)player.customProperties["TeamName"] == "Spectators"){
						if(PhotonNetwork.player.name == player.name){
							GUI.color = Color.yellow;
						}else{
							GUI.color = Color.white;
						}
						GUILayout.BeginHorizontal("box");{
							GUILayout.Label(player.name);
							GUILayout.FlexibleSpace();
							if(player.customProperties["Ping"] != null){
								GUILayout.Label("Ping: " + ((int)player.customProperties["Ping"]).ToString());
							}
						GUILayout.EndHorizontal();}
					}
				}
				//*************************************************************************************
			}
	    GUILayout.EndScrollView();
	}
	
	void  Resolutions(){
		GUILayout.BeginVertical();
		GUILayout.Label("Resolution");
			scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(140), GUILayout.Height(100));
			GUILayout.BeginVertical();
				for(int a = 0; a < resolutions.Length; a++){
					if(resolutions[a].width == Screen.width && resolutions[a].height == Screen.height){
						GUI.color = new Color(0, 20, 20, 0.6f);
					}else{
						GUI.color = new Color(20, 20, 20, 0.6f);
					}
					if(GUILayout.Button(resolutions[a].width + " x " + resolutions[a].height)){
						resolutionIndex = a;
						if(Screen.fullScreen){
							Screen.SetResolution (resolutions[resolutionIndex].width,resolutions[resolutionIndex].height, true);	
						}
					}
				}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}
	
	void QualityWindow(){
		//GUILayout.Space (10);
		GUILayout.BeginVertical();
		GUI.color = Color.white;
		GUILayout.Label("Quality");
			scroll2 = GUILayout.BeginScrollView(scroll2, GUILayout.Width(140), GUILayout.Height(100));
			GUILayout.BeginVertical();
			 	for (var i = 0; i < QualityNames.Length; i++){
			 		if(QualityNames[i] == QualityNames[QualitySettings.GetQualityLevel ()]){
			 			GUI.color = new Color(0, 20, 20, 0.6f);
			 		}else{
			 			GUI.color = new Color(20, 20, 20, 0.6f);
			 		}
			        if (GUILayout.Button (QualityNames[i]))
			            QualitySettings.SetQualityLevel (i, true);
		    	}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}
	
	void FadeScreen(){
		if(fadeDir == 1){
			fadeValue += fadeDir * 15 * Time.deltaTime;	
			fadeValue = Mathf.Clamp01(fadeValue);	

			GUI.color = new Color(1,1,1,fadeValue);
			GUI.DrawTexture(new Rect(0,0, Screen.width, Screen.height), blackScreen);	
					
			GUI.color = Color.white;
			GUI.Label(new Rect(Screen.width/2 - 75, Screen.height/2 - 15, 150, 30), "Loading...");
		}
	}
	
	void SpawnPlayer(string teamName){
		if(Player){
			PhotonNetwork.Destroy(Player);
		}
		enableHelper.SetActive(true);
		Hashtable setPlayerTeam = new Hashtable() {{"TeamName", teamName}};
		PhotonNetwork.player.SetCustomProperties(setPlayerTeam);
		//Spawn our player
		int temp;
		if(teamName == team_1.teamName){
			if(gameMode == "TDM"){
				temp = Random.Range(0, team_1.spawnPoints.Count);
				Player = PhotonNetwork.Instantiate(playerPrefab.name, team_1.spawnPoints[temp].position, team_1.spawnPoints[temp].rotation, 0);
				Player.name = PhotonNetwork.player.name;
			}else{
				//Spawn player in DM mode
				temp = Random.Range(0, allSpawnPoints.Count);
				Player = PhotonNetwork.Instantiate(playerPrefab.name, allSpawnPoints[temp].position, allSpawnPoints[temp].rotation, 0);
				Player.name = PhotonNetwork.player.name;
			}
		}else{
			temp = Random.Range(0, team_2.spawnPoints.Count);
			Player = PhotonNetwork.Instantiate(playerPrefab.name, team_2.spawnPoints[temp].position, team_2.spawnPoints[temp].rotation, 0);
			Player.name = PhotonNetwork.player.name;
		}
		roomCamera.SetActive(false);
	}
	
	void SwapTeams(string teamName){
		Hashtable setPlayerTeam = new Hashtable() {{"TeamName", teamName}};
		PhotonNetwork.player.SetCustomProperties(setPlayerTeam);	
		Player.SendMessage("SwapTeams");
	}
	
	void LeaveRoom(){
		if(PhotonNetwork.connected){
			PhotonNetwork.LeaveRoom();
		}
	}
	
	IEnumerator LoadMap(string sceneName){
		PhotonNetwork.isMessageQueueRunning = false;
		fadeDir = 1;
		yield return new WaitForSeconds(1);

        Application.backgroundLoadingPriority = ThreadPriority.High;
		AsyncOperation async = Application.LoadLevelAsync(sceneName);     
		yield return async; 
		Debug.Log("Loading complete");  
	}
	
	void OnDisconnectedFromPhoton(){	
		print ("Disconnected from Photon");
		//Something wrong with connection - go to main menu
		isPaused = false;
		roomCamera.SetActive(true);
		StartCoroutine(LoadMap("MainMenu"));
	}
	
	void OnLeftRoom(){
		isPaused = false;
		roomCamera.SetActive(true);
		StartCoroutine(LoadMap("MainMenu"));
	}
}
