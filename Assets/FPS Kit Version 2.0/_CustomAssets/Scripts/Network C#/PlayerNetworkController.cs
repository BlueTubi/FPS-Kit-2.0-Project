//NSdesignGames @ 2012 - 2013
//FPS Kit | Version 2.0 + Multiplayer

//This script control all the actions that done on remote player

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerNetworkController : Photon.MonoBehaviour {
	
	//Player location sync and other *******************************************
	
	//Temporary variable
	[HideInInspector]
	public string playerName;
	//This is target fro HeadLookControllerc.cs
	public Transform lookTarget;
	
	CharacterController cc;
	RoomMultiplayerMenu rmm;
	GameObject currentActiveWeapon;
	//***************************************************************
	
	//Player animation sync ***********************************************
	
	
	public Transform thirdPersonWeapons; //GameObject which contain all third person weapons as child objects
	//Assign following Armature bones for separated animation blending
	public Transform shoulderR;
	public Transform shoulderL;
	public GameObject animationSyncHelper; //Use this object to receive animations that were sent from CharacterAnimation.js
	public GameObject animationForHands;
	public GameObject activeWeapon; //This gameObject.name tell which third person weapon is active now
	public GameObject animationType; //Receive animation type from CharacterAnimation.js
	
	//Define animation speed for each type
	[System.Serializable]
	public class AnimationSpeed{
		public float walkSpeed = 1;
		public float runSpeed = 1;
		public float crouchSpeed = 1;
		public float proneSpeed = 1;
	}
	
	public AnimationSpeed animationSpeed;
	
	//We store animation names that were given MixedTransform, So we add Mixed Transform only once for each needed animation
	List<string> mixedAnimations = new List<string>();
	
	
	string MovementAnimation;
	string currentBlendedAnimation; 	//Copy received blended animation
	string currentWeaponName; 
		
    private string currentAnimation = ""; 
	private string blendedAnimation = ""; 
	private string prevWeap = "";
	//***************************************************************
	
	public DrawPlayerName dwn;
	public PlayerDamage pd;
	public HeadLookController headLookController;
	
	public List<GameObject> remoteObjectsToDeactivate;
	public List<MonoBehaviour> remoteScriptsToDeactivate;
	public List<GameObject> localObjectsToDeactivate;
	public List<MonoBehaviour> localScriptsToDeactivate;
	
	// Use this for initialization
	void Awake () {
		if(!photonView.isMine){
			for(int i = 0; i<remoteObjectsToDeactivate.Count;i++){
				remoteObjectsToDeactivate[i].SetActive(false);	
			}
			for(int a = 0; a<remoteScriptsToDeactivate.Count;a++){
				//remoteScriptsToDeactivate[a].enabled = false;
				Destroy(remoteScriptsToDeactivate[a]);
			}
			//gameObject.layer = 0;
			gameObject.tag = "Remote";
			if(lookTarget.gameObject.activeSelf == false){
				lookTarget.gameObject.SetActive(true);	
			}
		}else{
			for(int b = 0; b<localObjectsToDeactivate.Count;b++){
				localObjectsToDeactivate[b].SetActive(false);	
			}
			for(int c = 0; c<localScriptsToDeactivate.Count;c++){
				//localScriptsToDeactivate[c].enabled = false;
				Destroy(localScriptsToDeactivate[c]);
			}
			Destroy(headLookController);
		}
		rmm = GameObject.FindWithTag("Network").GetComponent<RoomMultiplayerMenu>();
	}
	
	//Smooth player movement
 	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.isWriting){
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
			stream.SendNext(gameObject.name);
			stream.SendNext(lookTarget.position);
			stream.SendNext(lookTarget.rotation);
			stream.SendNext((string)PhotonNetwork.player.customProperties["TeamName"]);
			//Player animation sync
            stream.SendNext(animationSyncHelper.name);
			stream.SendNext(animationForHands.name);
			stream.SendNext(activeWeapon.name);
			stream.SendNext(animationType.name);
        }else{
            //Network player, receive data
            //controllerScript._characterState = (CharacterState)(int)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
			PlayerName = (string)stream.ReceiveNext();
			lookTargetPos = (Vector3)stream.ReceiveNext();
		 	lookTargetRot = (Quaternion)stream.ReceiveNext();
			playerTeam = (string)stream.ReceiveNext();
			//Player animation sync
            currentAnimation = (string)stream.ReceiveNext();
			blendedAnimation = (string)stream.ReceiveNext();
			currentWeaponName = (string)stream.ReceiveNext();
			animType = (string)stream.ReceiveNext();
        }
    }

    private Vector3 correctPlayerPos = new Vector3(0, -100, 0); //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this
	private string PlayerName = "";
	private Vector3 lookTargetPos = Vector3.zero;
	private Quaternion lookTargetRot = Quaternion.identity;
	private string playerTeam = "";
	public string animType = "";

    void Update(){
        if (!photonView.isMine){
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 8);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 8);
			lookTarget.position = Vector3.Lerp(lookTarget.position, lookTargetPos, Time.deltaTime * 8);
			lookTarget.rotation = lookTargetRot;
			
			if(gameObject.name != PlayerName){
				gameObject.name = PlayerName;
			}
			
			animationSyncHelper.name = currentAnimation;
			animationForHands.name = blendedAnimation;
			activeWeapon.name = currentWeaponName;
			
			
			//Check room game mode
			if(rmm.gameMode == "TDM"){
				//Check if this player from our team (if so, disable hit damage)
				if(playerTeam == (string)PhotonNetwork.player.customProperties["TeamName"]){
					pd.disableDamage = true;
					dwn.enabled = true;
				}else{
					//But if player from other team, do nto display name
					pd.disableDamage = false;
					dwn.enabled = false;
				}
			}else{
				dwn.enabled = false;
			}
			
			//Player animation sync ***************************************************************************************
			
			//Base movement animations
			if(MovementAnimation != currentAnimation){
				MovementAnimation = currentAnimation;
				
				//Set animation speed
				if(animType == "Walking"){
					headLookController.GetComponent<Animation>()[MovementAnimation].speed = animationSpeed.walkSpeed;
				}
				if(animType == "Running"){
					headLookController.GetComponent<Animation>()[MovementAnimation].speed = animationSpeed.runSpeed;
				}
				if(animType == "Crouch"){
					headLookController.GetComponent<Animation>()[MovementAnimation].speed = animationSpeed.crouchSpeed;
				}
				if(animType == "Prone"){
					headLookController.GetComponent<Animation>()[MovementAnimation].speed = animationSpeed.proneSpeed;
				}
				
				//Set animation layer and wrap mode
				if(headLookController.GetComponent<Animation>()[MovementAnimation] != null){
					headLookController.GetComponent<Animation>()[MovementAnimation].layer = 1;
					headLookController.GetComponent<Animation>()[MovementAnimation].wrapMode = WrapMode.Loop;
				}
			}
			if(headLookController.GetComponent<Animation>()[MovementAnimation] != null){
				headLookController.GetComponent<Animation>().CrossFade(MovementAnimation);
			}
			
			//Blended weapon animations (Pistol Idle, Knife Idle etc.)
			//If blended animation == "Null" mean that we dont need mixing animation at the moment, so disable them
			if(currentBlendedAnimation != blendedAnimation){
				currentBlendedAnimation = blendedAnimation;
				if(!mixedAnimations.Contains(currentBlendedAnimation) && currentBlendedAnimation != "Null" && headLookController.GetComponent<Animation>()[currentBlendedAnimation] != null){
			   		headLookController.GetComponent<Animation>()[currentBlendedAnimation].layer = 4;
					headLookController.GetComponent<Animation>()[currentBlendedAnimation].wrapMode = WrapMode.Loop;
					headLookController.GetComponent<Animation>()[currentBlendedAnimation].AddMixingTransform(shoulderR);
					headLookController.GetComponent<Animation>()[currentBlendedAnimation].AddMixingTransform(shoulderL);	
					mixedAnimations.Add (currentBlendedAnimation);
				}
			}
			if(currentBlendedAnimation != "Null" && headLookController.GetComponent<Animation>()[currentBlendedAnimation] != null){
				headLookController.GetComponent<Animation>().Play(currentBlendedAnimation); 
			}
			//**********************************************************************************************************
			
			//Change third person weapon 
			if(prevWeap != currentWeaponName){
				for(int i = 0; i < thirdPersonWeapons.childCount; i++){
					if(thirdPersonWeapons.GetChild(i).name != currentWeaponName){
						thirdPersonWeapons.GetChild(i).gameObject.SetActive(false);
					}else{
						thirdPersonWeapons.GetChild(i).gameObject.SetActive(true);
						currentActiveWeapon = thirdPersonWeapons.GetChild(i).gameObject;
					}
				}
				prevWeap = currentWeaponName;
			}
        }
    }
	
	public void ReDeactivatePlayerObjects(){
		if(photonView.isMine){
			for(int b = 0; b<localObjectsToDeactivate.Count;b++){
				localObjectsToDeactivate[b].SetActive(false);	
			}
			for(int c = 0; c<localScriptsToDeactivate.Count;c++){
				//localScriptsToDeactivate[c].enabled = false;
				Destroy(localScriptsToDeactivate[c]);
			}
		}
	}
	
	//Use this to sync weapons over network
	void syncMachineGun(float errorAngle){
		photonView.RPC ("SyncWeaponsRPC", PhotonTargets.Others, "syncMachineGun", errorAngle);
	}
	
	void syncShotGun(int fractions){
		photonView.RPC ("SyncWeaponsRPC", PhotonTargets.Others, "syncShotGun", (float)fractions);
	}
	
	void syncGrenadeLauncher(float initialSpeed){
		photonView.RPC ("SyncWeaponsRPC", PhotonTargets.Others, "syncGrenadeLauncher", initialSpeed);
	}
	
	void syncKnife(){
		photonView.RPC ("SyncWeaponsRPC", PhotonTargets.Others, "syncKnife", 0.0f);
	}
	
	[RPC] 
	void SyncWeaponsRPC(string functionName, float Value){
		if(currentActiveWeapon){
			currentActiveWeapon.SendMessage(functionName, Value, SendMessageOptions.DontRequireReceiver);
		}
	}
}
