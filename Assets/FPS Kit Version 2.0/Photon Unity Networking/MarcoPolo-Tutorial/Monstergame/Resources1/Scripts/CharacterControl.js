// Require a character controller to be attached to the same game object
@script RequireComponent(CharacterController)

//All Animation Clip Params
public var idleAnimation : AnimationClip;
public var walkAnimation : AnimationClip;
public var runAnimation : AnimationClip;
public var jumpPoseAnimation : AnimationClip;
public var fallPoseAnimation : AnimationClip;

//Animation Clip Speed
public var jumpAnimationSpeed : float = 4;
public var fallAnimationSpeed : float = 0.1;
public var runAnimationSpeed : float = 1.5;
public var walkAnimationSpeed : float = 1.5;
public var idleAnimationSpeed : float = 0.5;

public var speed : float = 2; //Walk speed
public var runSpeed : float = 5.0;
public var jumpSpeed : float = 8.0;
public var gravity : float = 20.0;

private var controller : CharacterController;

//Move Params
private var f_verticalSpeed : float = 0.0;
private var f_moveSpeed : float = 0.0;
private var v3_moveDirection : Vector3 = Vector3.zero;

//Boolean
private var b_isRun : boolean;
private var b_isBackward : boolean;
private var b_isJumping : boolean;

//Rotate Params
private var q_currentRotation : Quaternion; //current rotation of the character
private var q_rot : Quaternion; //Rotate to left or right direction
private var f_rotateSpeed : float = 1.0; //Smooth speed of rotation

//Direction Params
private var v3_forward : Vector3; //Forward Direction of the character
private var v3_right : Vector3; //Right Direction of the character

private var c_collisionFlags : CollisionFlags; //Collision Flag return from Moving the character

//Create in air time
private var f_inAirTime : float = 0.0;
private var f_inAirStartTime : float = 0.0;
private var f_minAirTime : float = 0.15; // 0.15 sec.

//Using Awake to set up parameters before Initialize
public function Awake() : void {
	controller = GetComponent(CharacterController);
	b_isRun = false;
	b_isBackward = false;
	b_isJumping = false;
	f_moveSpeed = speed;
	c_collisionFlags = CollisionFlags.CollidedBelow;
	
	//Set warpMode for each aniamtion clip
	GetComponent.<Animation>()[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
	GetComponent.<Animation>()[fallPoseAnimation.name].wrapMode = WrapMode.ClampForever;
	GetComponent.<Animation>()[idleAnimation.name].wrapMode = WrapMode.Loop;
	GetComponent.<Animation>()[runAnimation.name].wrapMode = WrapMode.Loop;
	GetComponent.<Animation>()[walkAnimation.name].wrapMode = WrapMode.Loop;
}

public function Start() : void {
	f_inAirStartTime = Time.time;
}

public function Update() : void {
	//Get Main Camera Transfrom
	var cameraTransform = Camera.main.transform;
	
	//Get forward direction of the character
	v3_forward = cameraTransform.TransformDirection(Vector3.forward);
	v3_forward.y = 0; //Make sure that vertical direction equal zero
	// Right vector relative to the character
	// Always orthogonal to the forward direction vector
	v3_right = new Vector3(v3_forward.z, 0, -v3_forward.x); // -90 degree to the left from the forward direction
	//Get Horizontal move - rotation
	var f_hor : float = Input.GetAxis("Horizontal");
	//Get Vertical move - move forward or backward
	var f_ver : float = Input.GetAxis("Vertical");
	//If we are moving backward
	if (f_ver < 0) {
		b_isBackward = true;
	} else { 
		b_isBackward = false; 
	}
	//Get target direction
	var v3_targetDirection : Vector3 = (f_hor * v3_right) + (f_ver * v3_forward);
	//If the target direction is not zero - mean there is no button pressing
	if (v3_targetDirection != Vector3.zero) {
		//Rotate toward the target direction
		v3_moveDirection = Vector3.Slerp(v3_moveDirection, v3_targetDirection, f_rotateSpeed * Time.deltaTime);
		v3_moveDirection = v3_moveDirection.normalized; //Get only direction by normalize our target vector
	} else {
		v3_moveDirection = Vector3.zero;
	}
	
	//Checking if character is on the ground	
	if (!b_isJumping) {
		//Holding Shift to run
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			b_isRun = true;
			f_moveSpeed = runSpeed;
		} else {
			b_isRun = false;
			f_moveSpeed = speed;
		}  
        //Press Space to Jump
        if (Input.GetButton ("Jump")) {
            f_verticalSpeed = jumpSpeed;
            b_isJumping = true;
        }
	}

	// Apply gravity
	if (IsGrounded()) {
		f_verticalSpeed = 0.0; //if our character are grounded
		b_isJumping = false; //Checking if our character is in the air or not
		f_inAirTime = 0.0;
		f_inAirStartTime = Time.time;
	} else {
		f_verticalSpeed -= gravity * Time.deltaTime; //if our character in the air
		//Count Time
		f_inAirTime = Time.time - f_inAirStartTime;
	}

	// Calculate actual motion
	var v3_movement : Vector3 = (v3_moveDirection * f_moveSpeed) + Vector3 (0, f_verticalSpeed, 0); // Apply the vertical speed if character fall down
	v3_movement *= Time.deltaTime;
    
    // Move the controller
    c_collisionFlags = controller.Move(v3_movement);
    
    //Play animation
   	if (b_isJumping) {
		if (controller.velocity.y > 0 ) {
			GetComponent.<Animation>()[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
			GetComponent.<Animation>().CrossFade(jumpPoseAnimation.name, 0.1);
		} else {
			GetComponent.<Animation>()[fallPoseAnimation.name].speed = fallAnimationSpeed;
			GetComponent.<Animation>().CrossFade(fallPoseAnimation.name, 0.1);
		}
	} else {
		if (IsAir()) { // Fall down
			GetComponent.<Animation>()[fallPoseAnimation.name].speed = fallAnimationSpeed;
			GetComponent.<Animation>().CrossFade(fallPoseAnimation.name, 0.1);
		} else { //Not fall down
			 //If the character has no velocity or very close to 0 show idle animation
			if(controller.velocity.sqrMagnitude < 0.1) {
				GetComponent.<Animation>()[idleAnimation.name].speed = idleAnimationSpeed;
				GetComponent.<Animation>().CrossFade(idleAnimation.name, 0.1);
			} else { //Checking if the character walk or run
				if (b_isRun) {
					GetComponent.<Animation>()[runAnimation.name].speed = runAnimationSpeed;
					GetComponent.<Animation>().CrossFade(runAnimation.name, 0.1);
				} else {
					GetComponent.<Animation>()[walkAnimation.name].speed = walkAnimationSpeed;
					GetComponent.<Animation>().CrossFade(walkAnimation.name, 0.1);
				}
			}
		}
	}

	//Update rotation of the character
    if (v3_moveDirection != Vector3.zero) {
    	transform.rotation = Quaternion.LookRotation(v3_moveDirection);
    }
}

//Checking if the character hit the ground (collide Below)
public function IsGrounded () : boolean {
	return (c_collisionFlags & CollisionFlags.CollidedBelow);
}
//Geting if the character is jumping or not
public function IsJumping() : boolean {
	return b_isJumping;
}
//Checking if the character is in the air more than the minimun time 
//This function is to make sure that we are falling not walking down slope
public function IsAir() : boolean {
	return (f_inAirTime > f_minAirTime);
}
//Geting if the character is moving backward
public function IsMoveBackward() : boolean {
	return b_isBackward;
}
