using UnityEngine;
using System.Collections;

using TDTK;

namespace TDTK {

	public class UnitAnimation : MonoBehaviour {
		
		public Animator animator;
		
		[Tooltip("Check to reset the animator transform position when the unit is spawned\nDisable this if the animator is on the root-object of the prefab")]
		public bool resetAnimatorPosition=true;
		
		[Header("Commons")]
		public AnimationClip clipIdle;
		public AnimationClip clipHit;
		public AnimationClip clipDestroyed;
		
		public AnimationClip clipAttack;
		public float attackDelay;
		
		[Header("For Creeps")]
		public AnimationClip clipMove;
		public AnimationClip clipSpawn;
		public AnimationClip clipDestination;
		
		[Header("For Towers")]
		public AnimationClip clipConstruct;
		public AnimationClip clipDeconstruct;
		
		
		private Vector3 defaultPos;
		private Quaternion defaultRot;
		
		void Awake(){
			defaultPos=animator.transform.localPosition;
			defaultRot=animator.transform.localRotation;
		
			if(animator==null){
				Debug.LogWarning("Animator component is not assigned for UnitAnimation", this);
				return;
			}
			
			AnimatorOverrideController overrideController = new AnimatorOverrideController();
			overrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
			
			overrideController["DummyIdle"] = clipIdle;
			overrideController["DummyHit"] = clipHit;
			overrideController["DummyAttack"] = clipAttack;
			
			overrideController["DummyMove"] = clipMove;
			overrideController["DummySpawn"] = clipSpawn;
			overrideController["DummyDestroyed"] = clipDestroyed;
			overrideController["DummyDestination"] = clipDestination;
			
			overrideController["DummyConstruct"] = clipConstruct;
			overrideController["DummyDeconstruct"] = clipDeconstruct;
			
			animator.runtimeAnimatorController = overrideController;
			
			//animator.applyRootMotion=true;
			
			Unit unit=gameObject.GetComponent<Unit>();
			unit.SetUnitAnimation(this);
		}
		
		
		/*
		//for debug
		void Update(){
			AnimatorClipInfo[] clipInfo=animator.GetCurrentAnimatorClipInfo(0);
			Debug.Log(clipInfo[0].clip.name);
		}
		*/
		
		
		void OnEnable(){
			if(animator==null) return;
			animator.SetBool("Destroyed", false);
			animator.SetBool("Destination", false);
			
			if(resetAnimatorPosition){
				animator.transform.localPosition=defaultPos;
				animator.transform.localRotation=defaultRot;
			}
			
			animator.Rebind();
		}
		
		
		
		public void PlayMove(float speed){
			animator.SetFloat("Speed", speed);
		}
		
		public void PlaySpawn(){
			if(clipSpawn!=null) animator.SetTrigger("Spawn");
		}
		public void PlayHit(){
			if(clipHit!=null) animator.SetTrigger("Hit");
		}
		public float PlayDestroyed(){
			if(clipDestroyed!=null) animator.SetBool("Destroyed", true);
			return clipDestroyed!=null ? clipDestroyed.length : 0 ;
		}
		public float PlayDestination(){
			if(clipDestination!=null) animator.SetBool("Destination", true);
			return clipDestination!=null ? clipDestination.length : 0 ;
		}
			
		public void PlayConstruct(){
			if(clipConstruct!=null) animator.SetTrigger("Construct");
		}
		public void PlayDeconstruct(){
			if(clipDeconstruct!=null) animator.SetTrigger("Deconstruct");
		}
		
		public float PlayAttack(){
			if(clipAttack!=null) animator.SetTrigger("Attack");
			return attackDelay;
		}
		
	}

}