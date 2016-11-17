using UnityEngine;
using System.Collections;

public class RingMotor : MonoBehaviour {
	public Animator matAnimator;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void HittheMat() {
		Debug.Log ("Hit");
		matAnimator.SetTrigger ("HitMat");
	}
}
