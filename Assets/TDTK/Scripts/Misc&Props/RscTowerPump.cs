using UnityEngine;
using System.Collections;

public class RscTowerPump : MonoBehaviour {

	public float speed=5;
	public float mag=1;
	
	private Vector3 origin;
	
	private Transform thisT;
	
	// Use this for initialization
	void Start () {
		thisT=transform;
		origin=thisT.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
	
		thisT.localPosition=origin-new Vector3(0, mag*Mathf.Abs(Mathf.Sin(Time.time*speed)), 0);
		
	}
}
