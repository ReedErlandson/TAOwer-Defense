using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class AbilityIndicator : MonoBehaviour {

	public List<ParticleSystem> particleList=new List<ParticleSystem>();
	
	
	void OnEnable(){
		StartCoroutine(DelayedEnable());
	}
	IEnumerator DelayedEnable(){
		yield return null;
		for(int i=0; i<particleList.Count; i++){
			particleList[i].Clear();
			particleList[i].startSize=transform.localScale.x*1.75f;
			particleList[i].Play();
		}
	}
	
	void OnDisable(){
		for(int i=0; i<particleList.Count; i++){
			particleList[i].Stop();
		}
	}
	
}
