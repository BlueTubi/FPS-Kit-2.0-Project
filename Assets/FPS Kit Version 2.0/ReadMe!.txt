NSdesignGames @ 2012 - 2013

Thank you for purchasing FPS Kit | Version 2.0

Documentations, Tutorials, Previews and Other information could be find in official Unity Community thread:

http://forum.unity3d.com/threads/153337-FPS-Kit-Version-2.0

Changelog:

	Version 1.5 (18.06.2013)
		NOTE: If you using previous version of Kit make sure to create a Back Up copy of your project before importing
		To change team name and color open RoomMultiplayerMenu.cs and scroll down till line 99. Edit lines and save.
	
		- Now example soldier is fully animated (40 animations)
		- Animation speed could be tweaked at PlayerNetworkController.cs
		- Added team colors when displaying chat messages or kill board
		- Now all messages automatically dissapear after certain period
		- Player Name is saved in PlayerPrefs so no need to retype it each time
		- Added notification when player connect/leave room and join team
		- Fixed chat bug

	Version 1.4 (02.05.2013)
		NOTE: This version is not compatible with previous.
		Before importing to project with previous version of FPS Kit 2.0 make sure to create a backup copy of your project.
		
		- Updated Photon Cloud plugin to most recent (v1.20)
		- Rewrited some multiplayer scripts to allow easy adding multiple maps for selection
		- Added Room creation menu with name, max players, game mode and map selection
		- Added team selection
		- Added round limit (When time ends round is automatically restarted)
		- 2 Game modes (Deathmatch and TeamDeathmatch)
		- Added scoreboard with Kills, Deaths and Ping for each player
		- Optimized many scripts and added many new explanation comments inside multiplayer scripts
		- Merget PlayerLocationSync.cs and AnimationSync.cs into one script PlayerNetworkController.cs
		- Reduced nuber of PhotonViews used for network player from 10 to 1

	Version 1.3 (22.11.2012)
		- Unity 4 support:

	Version 1.2c - Multiplayer (28.10.2012)
		- Multiplayer support based on Photon network
	 		> Create rooms 
	 		> Join rooms
	 		> Position sync
	 		> Animation sync
	 		> Hit box based damage system (+ hit marks) 
	 		> Shot sync 
	 		> Scoreboard (Player list + ping) 
	 		> Displaying player name above it (+ player HP)
	 		> Player spawn system
	 		>Multiplayer chat;
 			>Kill reporter example;
	 	- Bug fixes
	 		
	Version 1.1 (15.10.2012)
 		- Added Dynamic crosshair
 		- Added Smooth Slow Motion effect 
		- Fixed Weapon Script custom editor errors
	 	- Fixed bug when unchecking Recoil in Weapon Script do not stop recoil effect
	 	
	Version 1.0 (11.10.2012)
	 	- Initial release
