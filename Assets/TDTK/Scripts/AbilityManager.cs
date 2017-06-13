using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {
	
	public class AbilityManager : MonoBehaviour {
		
		public List<int> unavailableIDList=new List<int>();	//ID list of perk unavailable for this level, modified in editor
		[HideInInspector] public List<int> availableIDList=new List<int>();	//ID list of perk available for this level, modified in editor
		
		public List<Ability> abilityList=new List<Ability>();	//actual ability list, filled in runtime based on availableIDList
		public static List<Ability> GetAbilityList(){ return instance.abilityList; }
		public static int GetAbilityCount(){ return instance.abilityList.Count; }
		
		public Transform defaultIndicator;		//generic indicator use for ability without any specific indicator
		
		//private bool inTargetSelectMode=false;
		private bool isSelectingTarget=false;
		public static bool IsSelectingTarget(){ return instance==null ? false : instance.isSelectingTarget; }
		
		//public int selectedAbilityID=-1;
		public Transform currentIndicator;		//active indicator in used
		
		public bool startWithFullEnergy=false;
		public bool onlyChargeOnSpawn=false;
		public float energyRate=2;
		public float fullEnergy=100;
		public float energy=0;
		
		
		private Transform thisT;
		private static AbilityManager instance;
		public static AbilityManager GetInstance(){ return instance; }
		public static bool IsOn(){ return instance==null ? false : true; }
		
		public void Init(){
			instance=this;
			thisT=transform;
			
			if(startWithFullEnergy) energy=fullEnergy;
			
			List<Ability> dbList=TDTK.GetAbilityDBList();	//AbilityDB.Load();
			
			availableIDList=new List<int>();
			abilityList=new List<Ability>();
			for(int i=0; i<dbList.Count; i++){
				if(dbList[i].disableInAbilityManager) continue;
				if(!unavailableIDList.Contains(dbList[i].ID)){
					abilityList.Add(dbList[i].Clone());
					availableIDList.Add(dbList[i].ID);
				}
			}
			
			List<Ability> newList=PerkManager.GetUnlockedAbilityList();
			for(int i=0; i<newList.Count; i++) abilityList.Add(newList[i].Clone());
			
			for(int i=0; i<abilityList.Count; i++) abilityList[i].Init();
			
			
			if(defaultIndicator!=null){
				defaultIndicator=(Transform)Instantiate(defaultIndicator);
				defaultIndicator.parent=thisT;
				defaultIndicator.gameObject.SetActive(false);
			}
			
			maskAOE=1<<TDTK.GetLayerPlatform();
			int terrainLayer=TDTK.GetLayerTerrain();
			if(terrainLayer>=0) maskAOE|=1<<terrainLayer;
		}
		
		
		public static void AddNewAbility(Ability ab){ if(instance!=null) instance._AddNewAbility(ab); }
		public void _AddNewAbility(Ability ab){ 
			for(int i=0; i<abilityList.Count; i++){ if(ab.ID==abilityList[i].ID) return; }
			
			Ability ability=ab.Clone();
			ability.Init();
			
			abilityList.Add(ability);
			TDTK.OnNewAbility(ability);
		}
		
		
		
		
		void FixedUpdate(){
			if((onlyChargeOnSpawn && GameControl.IsGameStarted()) || !onlyChargeOnSpawn){
				if(energy<GetEnergyFull()){
					float valueGained=Time.fixedDeltaTime*GetEnergyRate();
					energy+=valueGained;
					energy=Mathf.Min(energy, GetEnergyFull());
					
					if(energy==GetEnergyFull()) TDTK.OnEnergyFull();
				}
			}
		}
		
		
		
		private LayerMask maskAOE;
		IEnumerator SelectAbilityTargetRoutine(Ability ability, int pointerID=-1){
			yield return null;
			
			Vector3 cursorPos=Vector3.zero;
			Unit targetUnit=null;
			
			LayerMask mask=maskAOE;
			if(ability.singleUnitTargeting){
				if(ability.targetType==Ability._TargetType.Hybrid) mask|=1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerCreep();
				else if(ability.targetType==Ability._TargetType.Friendly) mask|=1<<TDTK.GetLayerTower();
				else if(ability.targetType==Ability._TargetType.Hostile) mask|=1<<TDTK.GetLayerCreep();
			}
			
			Transform indicator=ability.indicator;
			if(indicator==null){
				indicator=defaultIndicator;
				float scale=ability.singleUnitTargeting ? BuildManager.GetGridSize() : ability.GetAOERadius()*2;
				indicator.localScale=new Vector3(scale, scale, scale);
			}
			
			//TDTK.OnGameMessage("SelectAbilityTargetRoutine   "+pointerID);
			
			isSelectingTarget=true;
			TDTK.OnAbilityTargetSelectModeE(true);
			
			if(pointerID>=0){
				while(true){
					if(TDTK.IsTouchStarting(pointerID)) break;
					yield return null;
				}
			}
			
			bool cursorOnUI=true;
			
			while(isSelectingTarget){
				if(Input.GetKeyDown(KeyCode.Escape)) break;
				
				bool invalidCursor=false;
				bool invalidTarget=false;
				
				if(pointerID<0) cursorPos=Input.mousePosition;
				else cursorPos=TDTK.GetTouchPosition(pointerID);
				
				if(cursorPos.magnitude<0) invalidCursor=true;
				
				
				if(!invalidCursor && !cursorOnUI){
					Ray ray = Camera.main.ScreenPointToRay(cursorPos);
					RaycastHit hit;
					if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
						indicator.position=hit.point;
						
						targetUnit=null;
						
						if(ability.singleUnitTargeting){
							targetUnit=hit.transform.GetComponent<Unit>();
							if(targetUnit!=null) indicator.position=targetUnit.thisT.position;
							else invalidTarget=true;
						}
					}
				}
				
				indicator.gameObject.SetActive(!invalidCursor);
				
				if(pointerID==-1){
					if(Input.GetMouseButtonDown(0)){
						if(cursorOnUI) break;
						if(!invalidTarget) ActivateAbility(ability, indicator.position, targetUnit);
						else TDTK.OnGameMessage("Invalid target for ability");
						break;
					}
					if(Input.GetMouseButtonDown(1)) break;
				}
				else{
					if(TDTK.IsTouchEnding(pointerID)){
						//TDTK.OnGameMessage("SelectAbilityTargetRoutine   "+pointerID+"   "+UI.IsCursorOnUI(pointerID));
						if(cursorOnUI) break;
						if(!invalidTarget) ActivateAbility(ability, indicator.position, targetUnit);
						else TDTK.OnGameMessage("Invalid target for ability");
						break;
					}
				}
				
				//check in previous frame cause IsCursorOnUI wont return true if the touch is ending
				cursorOnUI=UI.IsCursorOnUI(pointerID);
				
				yield return null;
			}
			
			yield return null;
			
			indicator.gameObject.SetActive(false);
			
			isSelectingTarget=false;
			TDTK.OnAbilityTargetSelectModeE(false);
		}
		public static void ExitSelectingTargetMode(){
			instance.isSelectingTarget=false;
		}
		
		
		
		//called by ability button from UI, select an ability
		public static string SelectAbility(int ID, int pointerID=-1){ return instance._SelectAbility(ID, pointerID); }
		public string _SelectAbility(int ID, int pointerID=-1){
			Ability ability=abilityList[ID];
			
			Debug.Log(ability.name+"   "+ability.requireTargetSelection);
			
			string exception=ability.IsAvailable();
			if(exception!="") return exception;
			
			if(!ability.requireTargetSelection) ActivateAbility(ability);		//no target selection required, fire it away
			else{
				if(Input.touchCount==0) StartCoroutine(SelectAbilityTargetRoutine(ability, -1));
				else StartCoroutine(SelectAbilityTargetRoutine(ability, 0));
			}
				
			return "";
		}
		
		
		//called when an ability is fired, reduce the energy, start the cooldown and what not
		public void ActivateAbility(Ability ab, Vector3 pos=default(Vector3), Unit unit=null){
			energy-=ab.GetCost();
			ab.Activate(pos);
			
			//ab.CastEffectObject(pos);
			
			//CastAbility(ab, pos, unit);
			//if(ab.effectObj!=null)
			//	ObjectPoolManager.Spawn(ab.effectObj, pos, Quaternion.identity);
			
			if(ab.useDefaultEffect)
				StartCoroutine(ApplyAbilityEffect(ab, pos, unit));
			
			TDTK.OnAbilityActivated(ab);
		}
		
		
		
		//apply the ability effect, damage, stun, buff and so on 
		IEnumerator ApplyAbilityEffect(Ability ab, Vector3 pos, Unit tgtUnit=null){
			yield return new WaitForSeconds(ab.effectDelay);
			
			List<Unit> creepList=new List<Unit>();
			List<Unit> towerList=new List<Unit>();
			
			if(tgtUnit==null){
				LayerMask mask=1<<TDTK.GetLayerTower() | 1<<TDTK.GetLayerCreep() | 1<<TDTK.GetLayerCreepF();
				float radius=ab.requireTargetSelection ? ab.GetAOERadius() : Mathf.Infinity;
				Collider[] cols=Physics.OverlapSphere(pos, radius, mask);
				
				if(cols.Length>0){
					for(int i=0; i<cols.Length; i++){
						int layer=cols[i].gameObject.layer;
						
						if(layer==TDTK.GetLayerCreep() || layer==TDTK.GetLayerCreepF())
							creepList.Add(cols[i].gameObject.GetComponent<UnitCreep>());
						if(layer==TDTK.GetLayerTower())
							towerList.Add(cols[i].gameObject.GetComponent<UnitTower>());
					}
				}
			}
			else{
				creepList.Add(tgtUnit);
				towerList.Add(tgtUnit);
			}
				
			AbilityEffect eff=ab.GetActiveEffect();
			
			for(int n=0; n<creepList.Count; n++){
				if(eff.damageMax>0){
					creepList[n].ApplyDamage(Random.Range(eff.damageMin, eff.damageMax));
				}
				if(eff.stunChance>0 && eff.duration>0){
					if(Random.Range(0f, 1f)<eff.stunChance) creepList[n].ApplyStun(eff.duration);
				}
				if(eff.slow.IsValid()){
					creepList[n].ApplySlow(eff.slow);
				}
				if(eff.dot.GetTotalDamage()>0){
					creepList[n].ApplyDot(eff.dot);
				}
			}
			for(int n=0; n<towerList.Count; n++){
				if(eff.duration>0){
					if(eff.damageBuff>0){
						towerList[n].ABBuffDamage(eff.damageBuff, eff.duration);
					}
					if(eff.rangeBuff>0){
						towerList[n].ABBuffRange(eff.rangeBuff, eff.duration);
					}
					if(eff.cooldownBuff>0){
						towerList[n].ABBuffCooldown(eff.cooldownBuff, eff.duration);
					}
				}
				if(eff.HPGainMax>0){
					towerList[n].RestoreHP(Random.Range(eff.HPGainMin, eff.HPGainMax));
				}
			}
			
		}
		
		
		
		
		public static void GainEnergy(int value){ if(instance!=null) instance._GainEnergy(value); }
		public void _GainEnergy(int value){
			energy+=value;
			energy=Mathf.Min(energy, GetEnergyFull());
		}
		
		
		public static float GetAbilityCurrentCD(int index){ return instance.abilityList[index].currentCD; }
		
		public static float GetEnergyFull(){ return instance.fullEnergy+PerkManager.GetEnergyCapModifier(); }
		public static float GetEnergy(){ return instance.energy; }
		
		private float GetEnergyRate(){ return energyRate+PerkManager.GetEnergyRegenModifier(); }


		public static int GetAbilityIndex(Ability ability){
			for(int i=0; i<instance.abilityList.Count; i++){
				if(ability==instance.abilityList[i]) return i;
			}
			return -1;
		}
	}

	
	
	
	
	
	
	
}