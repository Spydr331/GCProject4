using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {

	public bool isCharacter = false;
	
	public GameObject enemyGameObject;
	public GameObject hitCheckGameObject;

	public Vector3 grabbedPosition;
	
	public float rotationSpeed = 2f;
	public float distanceGrab = 2.6f;

	// States
	private bool grabbed = false;
	private bool grabbing = false;

	public Animator characterAnimator;

	// set the move speed for the character in the inspector
	public float moveSpeed = 10;

	// set the fall speed for the character in the inspector
	public float fallSpeed = 30;

	// Check how far down we should check to register the floor to see if we are grounded. 
	public float groundedDistanceCheck = 1.2f;

	// A refrence of the CharacterController component attached to this player
	public CharacterController playerController;

	// A varible to hold the start location
	private Vector3 startPosition;
	private float speedRatio = 1;

	// the momentum for the jump
	public float momentum = 0;

	public void gotGrabbed() {
		grabbedPosition = transform.localPosition;
		grabbed = true;
		characterAnimator.SetBool ("GrappledBy", true);
	}

	public void grapplingStart() {
		grabbing = true;
	}

	public void grapplingEnd() {
		grabbing = false;
	}

	public void CheckForGrapple() {
		Vector3 grabDirectionRay = hitCheckGameObject.transform.TransformDirection (Vector3.forward);

		RaycastHit hit;

		if (Physics.Raycast (hitCheckGameObject.transform.position, grabDirectionRay, out hit)) {
			if (hit.distance < distanceGrab && hit.transform.CompareTag("Wrestler")) {
				enemyGameObject = hit.transform.gameObject;
				Grappling ();
			} else {
				Debug.Log ("TOO FAR");
			}
		}

		//Physics.Raycast(grabRay, 


	}

	// Use this for initialization
	void Start () {
		// Set the start location when we first "spawn"
		if(enemyGameObject != null)
			transform.forward = enemyGameObject.transform.position - transform.position;
		startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		// if momentum is greater than 1, reduce the momentum over time using a lerp method. Else, set it to 0;
		if(momentum>1)
			momentum = Mathf.Lerp(momentum,0,0.5f *Time.deltaTime);
		else
			momentum = 0;

		// output the result of IsGrounded to the log
		//Debug.Log(isGrounded);

		// If we are not grounded, trigger the fall Method
		if(!isGrounded)
			Fall ();

		if (isCharacter) {
			// Prevent movement when grappling
			if (!grabbing) {
				Move ();
			}

			// if we hit P on the keyboard, reset the player location to the start location
			if(Input.GetKeyDown(KeyCode.P)){
				transform.position = startPosition;
			}

			if (Input.GetKeyDown (KeyCode.Q) && !grabbing && !grabbed) {
				characterAnimator.SetTrigger ("Grappling");
				grapplingStart ();
			}

			if (Input.GetKeyDown (KeyCode.E) && grabbed) {
				cancelGrapple ();
			}
		} else {
			if (grabbed) {
				transform.localPosition = grabbedPosition;
			}
		}

	}

	// Check if the ControllerCollider Hit anything
	void OnControllerColliderHit(ControllerColliderHit hit){
		
		// If the object we hit has a rigidbody, add a force to that rigidbody 
		if(hit.rigidbody != null){
			Vector3 forceDirection = hit.transform.position - transform.position;
			hit.rigidbody.AddForce(forceDirection *100);
		}
	}
		
	void Move(){
		// Create a varible to store player input
		Vector3 inputDirection;

		// Set inputDirecion x acording to the Horizontal Axis of the input manage
		inputDirection.x = Input.GetAxis("Horizontal");

		// Set y to 0 to make sure the character does not move up and down
		inputDirection.y = 0;

		// Set InputDirection Z acording to keys defined by the input manager's "Vertical" axis
		inputDirection.z = Input.GetAxis("Vertical");

		//Make sure that the speed is the same for every computer (independent of cpu speed) by multiplying it by the time past over last frame.
		inputDirection *= Time.deltaTime;

		// Ajust speed acording to the movement speed we set up in the inspector
		inputDirection *= (moveSpeed * speedRatio);

		// transform.position would have us dictate the position of the transform
		//transform.position += inputDirection;

		// while CharacterController.Move allows us to take colliders into consideration
		transform.Translate(inputDirection, Space.World);

		if (enemyGameObject != null) {
			// Rotate character toward enemy
			Vector3 targetDirection = enemyGameObject.transform.position - transform.position;

			// :ock rotation of the body to the correct Axis
			targetDirection.y = 0;
			transform.forward = Vector3.RotateTowards (transform.forward, targetDirection, rotationSpeed * Time.deltaTime, 0);
		}
	}

	void Grappling() {
		grabbed = true;

		speedRatio = 0.5f;

		// Put opponent in current players object
		enemyGameObject.transform.parent = transform;

		// Rotate and position opponent
		enemyGameObject.transform.forward = transform.position - enemyGameObject.transform.position;
		enemyGameObject.transform.position = transform.position + transform.forward * distanceGrab;

		// Set Animations
		characterAnimator.SetBool("Grappled", true);
		enemyGameObject.BroadcastMessage ("gotGrabbed");



	}

	void freedFromGrapple() {
		characterAnimator.SetBool("GrappledBy", false);
		grabbed = false;
	}

	void cancelGrapple() {
		grabbed = false;
		speedRatio = 1f;

		enemyGameObject.transform.parent = null;

		characterAnimator.SetBool("Grappled", false);
		enemyGameObject.BroadcastMessage ("freedFromGrapple");
	}

	// Fall Method to make the character fall!
	void Fall(){
		// If falling, set parent to null, this way if they fall off a platform, the platform's movment will no longer manipulate the players position in air
		//transform.parent = null;

		Vector3 upMovment = Vector3.up * momentum * Time.deltaTime;
		// apply fall via Move
		playerController.Move(Vector3.down *Time.deltaTime*fallSpeed  + upMovment);
	}

	// Check if the character is grounded. returns of type boolean
	bool isGrounded{

		// Get is used to get a value from a varible. Set is used to set one, if the varibale does not have a "Set" feature, it is read only
		get{
			// create a ray which we will check against
			Ray theRay = new Ray(transform.position,Vector3.down);

			// Draw ray allows us to visualize the ray in the editor. Its "Direction" portion works differntly from the direction in a raycast ray. 
			//It's limit is set by the length of the direction
			//Debug.DrawRay(transform.position,Vector3.down*groundedDistanceCheck);

			// create a varible of type bool to see if we hit something. 
			bool didHitSomething;

			// Store the raycasthit info returned by the raycast function.
			RaycastHit hit; 

			// Set the bool acording to rather ot not we hit something. 
			// In this cast the limit of the ray has to be sepcified on its own
			didHitSomething = Physics.Raycast(theRay,out hit,groundedDistanceCheck);

			// if we hit something, set it as our parent object ot inherid its motion
			if(hit.transform!= null)
			{
				//transform.parent = hit.transform;
			}

			// return the bool. 
			return didHitSomething;
		}

	}
}
