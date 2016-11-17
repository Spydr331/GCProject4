using UnityEngine;
using System.Collections;

public class GrappleMotor : MonoBehaviour {

	public CharacterMotor characterMotorReference;
	public RingMotor ringMotorReference;

	public GameObject enemyGameObject;
	public GameObject hitCheckGameObject;

	private Vector3 grabbedPosition;

	private bool isCharacter;
	public float distanceGrab = 2.6f;

	// States
	public bool grabbed = false;
	public bool grabbing = false;
	public bool suplexed = false;
	public bool suplexing = false;

	public Animator characterAnimator;

	public void hitRing() {
		ringMotorReference.BroadcastMessage ("HittheMat");
	}

	public void gotGrabbed() {
		grabbedPosition = transform.localPosition;
		grabbed = true;
		characterAnimator.SetBool ("GrappledBy", true);
	}

	public void grapplingStart() {
		characterAnimator.SetTrigger ("Grappling");
		grabbing = true;
	}

	public void grapplingEnd() {
		grabbing = false;
	}

	public void suplexingStart() {
		enemyGameObject.BroadcastMessage ("gotSuplexed");
		characterAnimator.SetBool("Grappled", false);
		characterAnimator.SetBool ("Suplexing", true);
		grabbing = false;
		suplexing = true;
	}

	public void suplexingEnd() {
		characterAnimator.SetBool("Suplexing", false);
		characterAnimator.SetBool("SuplexBy", false);
		suplexing = false;
		suplexed = false;
		if(isCharacter)
			cancelGrapple ();
	}

	public void gotSuplexed() {
		characterAnimator.SetBool("GrappledBy", false);
		grabbed = false;
		suplexed = true;
		characterAnimator.SetBool ("SuplexBy", true);
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
	}

	// Use this for initialization
	void Start () {
		// Set the start location when we first "spawn"
		if(enemyGameObject != null)
			transform.forward = enemyGameObject.transform.position - transform.position;

		isCharacter = characterMotorReference.isCharacter;
	}

	// Update is called once per frame
	void Update () {


		if (isCharacter) {
			if (Input.GetKeyDown (KeyCode.Q)) {
				if (!grabbing && !grabbed) {
					grapplingStart ();
				} else if (grabbed) {
					suplexingStart ();
				}
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

	void Grappling() {
		grabbed = true;

		characterMotorReference.speedRatio = 0.5f;

		// Put opponent in current players object
		enemyGameObject.transform.parent = transform;

		// Rotate and position opponent
		enemyGameObject.transform.forward = transform.position - enemyGameObject.transform.position;
		enemyGameObject.transform.position = transform.position + transform.forward * distanceGrab;

		// Set Animations
		Debug.Log("Grappleing");

		characterAnimator.SetBool("Grappled", true);
		enemyGameObject.BroadcastMessage ("gotGrabbed");



	}

	void freedFromGrapple() {
		characterAnimator.SetBool("GrappledBy", false);
	}

	void cancelGrapple() {
		grabbed = false;
		characterMotorReference.speedRatio = 1f;

		enemyGameObject.transform.parent = null;

		characterAnimator.SetBool("Grappled", false);
		enemyGameObject.BroadcastMessage ("freedFromGrapple");
	}
}
