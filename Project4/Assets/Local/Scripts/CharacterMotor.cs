using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {

	public GrappleMotor grappleMotorReference;

	// A refrence of the CharacterController component attached to this player
	public CharacterController playerController;

	public bool isCharacter = false;


	// A varible to hold the start location
	private Vector3 startPosition;

	// set the move speed for the character in the inspector
	public float moveSpeed = 10;
	public float rotationSpeed = 2f;

	// set the fall speed for the character in the inspector
	public float fallSpeed = 30;
	public float speedRatio = 1;

	// the momentum for the fall
	public float momentum = 0;

	// Check how far down we should check to register the floor to see if we are grounded. 
	public float groundedDistanceCheck = 1.2f;

	void Start () {
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
		Debug.Log(isGrounded);

		// If we are not grounded, trigger the fall Method
		if(!isGrounded)
			Fall ();
		

		if (isCharacter) {
			// Prevent movement when grappling
			if (!grappleMotorReference.grabbing) {
				Move ();
			}
		}

		// if we hit P on the keyboard, reset the player location to the start location
		if (Input.GetKeyDown (KeyCode.P)) {
			transform.position = startPosition;
		}


	}

	// Fall Method to make the character fall!
	void Fall(){
		// If falling, set parent to null, this way if they fall off a platform, the platform's movment will no longer manipulate the players position in air
		//transform.parent = null;

		Vector3 upMovment = Vector3.up * momentum * Time.deltaTime;
		// apply fall via Move
		playerController.Move(Vector3.down *Time.deltaTime*fallSpeed  + upMovment);
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

		if (grappleMotorReference.enemyGameObject != null && !grappleMotorReference.grabbed) {
			// Rotate character toward enemy
			Vector3 targetDirection = grappleMotorReference.enemyGameObject.transform.position - transform.position;

			// Lock rotation of the body to the correct Axis
			targetDirection.y = 0;
			transform.forward = Vector3.RotateTowards (transform.forward, targetDirection, rotationSpeed * Time.deltaTime, 0);
		}
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
