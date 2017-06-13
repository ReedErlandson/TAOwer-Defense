using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class TDTK : MonoBehaviour {
		
		public static int GetLayerCreep(){ return 31; }
		public static int GetLayerCreepF(){ return 30; }
		public static int GetLayerTower(){ return 29; }
		public static int GetLayerShootObject(){ return 28; }
		public static int GetLayerPlatform(){ return 27; }
		
		public static int GetLayerTerrain(){ return 26; }
		public static int GetLayerUI(){ return 5; }	//layer5 is named UI by Unity's default
		
		
		
		
		
		public delegate void GameMessageHandler(string msg); 
		public static event GameMessageHandler onGameMessageE;
		public static void OnGameMessage(string msg){ if(onGameMessageE!=null) onGameMessageE(msg); }
		
		
		//from GameControl
		public delegate void LifeHandler(int value);
		public static event LifeHandler onLifeE;								//call when player's life has changed
		public static void OnLife(int valueChanged){ if(onLifeE!=null) onLifeE(valueChanged); }
		
		public delegate void FastForwardHandler(bool ff);
		public static event FastForwardHandler onFastForwardE;			//call when toggle fastforward
		public static void OnFastForward(bool ff){ if(onFastForwardE!=null) onFastForwardE(ff); }
		
		public delegate void GameOverHandler(bool playerWon);
		public static event GameOverHandler onGameOverE;			//call when game is over
		public static void OnGameOver(bool playerWon){ if(onGameOverE!=null) onGameOverE(playerWon); }
		
		
		
		//from SpawnManager
		public delegate void NewWaveHandler(int waveID);
		public static event NewWaveHandler onNewWaveE;				//inidcate a new wave has start spawning
		public static void OnNewWave(int waveID){ if(onNewWaveE!=null) onNewWaveE(waveID); }
		
		public delegate void WaveClearedHandler(int waveID);
		public static event WaveClearedHandler onWaveClearedE;		//indicate a wave has been cleared
		public static void OnWaveCleared(int waveID){ if(onWaveClearedE!=null) onWaveClearedE(waveID); }
		
		public delegate void EnableSpawnHandler();
		public static event EnableSpawnHandler onEnableSpawnE;	//call to indicate SpawnManager is ready to spawn next wave (when no longer in the process of actively spawning a wave)
		public static void OnEnableSpawn(){ if(onEnableSpawnE!=null) onEnableSpawnE(); }
		
		public delegate void SpawnTimerHandler(float time);
		public static event SpawnTimerHandler onSpawnTimerE;		//call to indicate how long until next spawn
		public static void OnSpawnTimer(float time){ if(onSpawnTimerE!=null) onSpawnTimerE(time); }
		
		
		
		//From ResourceManager
		public delegate void ResourceHandler(List<int> changedValueList);
		public static event ResourceHandler onResourceE;				//call when the values on resource list are changed
		public static void OnResource(List<int> valueChangedList){ if(onResourceE!=null) onResourceE(valueChangedList); }
		
		
		
		//From BuildManager
		public delegate void DragNDropHandler(bool flag);
		public static event DragNDropHandler onDragNDropE;			//when enter/exit dragNdrop phase
		public static void OnDragNDrop(bool flag){ if(onDragNDropE!=null) onDragNDropE(flag); }
		
		public delegate void NewBuildableTowerHandler(UnitTower tower);
		public static event NewBuildableTowerHandler onNewBuildableTowerE;			//add new tower to build list
		public static void OnNewBuildableTower(UnitTower tower){ if(onNewBuildableTowerE!=null) onNewBuildableTowerE(tower); }
		
		
		
		//From Unit
		public delegate void NewUnitHandler(Unit unit);
		public static event NewUnitHandler onNewUnitE;		//called when a unit is damaged
		public static void OnNewUnit(Unit unit){ if(onNewUnitE!=null) onNewUnitE(unit); }
		
		public delegate void UnitDamagedHandler(Unit unit);
		public static event UnitDamagedHandler onUnitDamagedE;		//called when a unit is damaged
		public static void OnUnitDamaged(Unit unit){ if(onUnitDamagedE!=null) onUnitDamagedE(unit); }
		
		
		
		//From UnitCreep
		public delegate void CreepDestroyedHandler(UnitCreep creep);
		public static event CreepDestroyedHandler onCreepDestroyedE;		//indicate the creep has been destroyed
		public static void OnUnitCreepDestroyed(UnitCreep unit){ if(onCreepDestroyedE!=null) onCreepDestroyedE(unit); }
		
		public delegate void CreepDestinationHandler(UnitCreep creep);
		public static event CreepDestinationHandler onCreepDestinationE;	//indicate the creep has reach its destination
		public static void OnCreepDestination(UnitCreep creep){ if(onCreepDestinationE!=null) onCreepDestinationE(creep); }
		
		
		
		//From UnitTower
		public delegate void TowerConstructingHandler(UnitTower tower);
		public static event TowerConstructingHandler onTowerConstructingE;	//called when the tower start constructing 
		public static void OnTowerConstructing(UnitTower tower){ if(onTowerConstructingE!=null) onTowerConstructingE(tower); }
		
		public delegate void TowerConstructedHandler(UnitTower tower);
		public static event TowerConstructedHandler onTowerConstructedE; //called when the tower finished constructing 
		public static void OnTowerConstructed(UnitTower tower){ if(onTowerConstructedE!=null) onTowerConstructedE(tower); }
		
		public delegate void TowerUpgradingHandler(UnitTower tower);
		public static event TowerUpgradingHandler onTowerUpgradingE;	//called when the tower start upgrading 
		public static void OnTowerUpgrading(UnitTower tower){ if(onTowerUpgradingE!=null) onTowerUpgradingE(tower); }
		
		public delegate void TowerUpgradedHandler(UnitTower tower);
		public static event TowerUpgradedHandler onTowerUpgradedE;			//called when tower has been upgraded
		public static void OnTowerUpgraded(UnitTower tower){ if(onTowerUpgradedE!=null) onTowerUpgradedE(tower); }
		
		public delegate void TowerSoldHandler(UnitTower tower);
		public static event TowerSoldHandler onTowerSoldE;						//called when tower has been sold
		public static void OnTowerSold(UnitTower tower){ if(onTowerSoldE!=null) onTowerSoldE(tower); }
		
		public delegate void TowerDestroyedHandler(UnitTower tower);
		public static event TowerDestroyedHandler onTowerDestroyedE;		//called when tower has been destroyed
		public static void OnUnitTowerDestroyed(UnitTower unit){ if(onTowerDestroyedE!=null) onTowerDestroyedE(unit); }
		

		
		//From AbilityManager
		public delegate void NewAbilityHandler(Ability ability);
		public static event NewAbilityHandler onNewAbilityE;					//called when new ability is added
		public static void OnNewAbility(Ability ability){ if(onNewAbilityE!=null) onNewAbilityE(ability); }
		
		public delegate void AbilityActivatedHandler(Ability ability);
		public static event AbilityActivatedHandler onAbilityActivatedE;		//called when an ability is activated
		public static void OnAbilityActivated(Ability ability){ if(onAbilityActivatedE!=null) onAbilityActivatedE(ability); }
		
		public delegate void AbilityTargetSelectModeHandler(bool flag);
		public static event AbilityTargetSelectModeHandler onAbilitySelectingTargetE;	//call when target select for ability is switch on/off (true/false)
		public static void OnAbilityTargetSelectModeE(bool flag){ if(onAbilitySelectingTargetE!=null) onAbilitySelectingTargetE(flag); }
		
		public delegate void AbilityReadyHandler(Ability ability);
		public static event AbilityReadyHandler onAbilityReadyE;					//called when an ability is ready
		public static void OnAbilityReady(Ability ability){ if(onAbilityReadyE!=null) onAbilityReadyE(ability); }
		
		public delegate void EnergyFullHandler();
		public static event EnergyFullHandler onEnergyFullE;					//called when energy is fully filled
		public static void OnEnergyFull(){ if(onEnergyFullE!=null) onEnergyFullE(); }
		
		
		
		//From FPSControl
		public delegate void FPSModeHandler(bool flag);
		public static event FPSModeHandler onFPSModeE;		//called when enter/exit (true/false) FPSMode
		public static void OnFPSMode(bool flag){ if(onFPSModeE!=null) onFPSModeE(flag); }
		
		public delegate void FPSShootHandler();
		public static event FPSShootHandler onFPSShootE;		//called when FPS fire a shot
		public static void OnFPSShoot(){ if(onFPSShootE!=null) onFPSShootE(); }
		
		public delegate void FPSReloadHandler(bool flag);
		public static event FPSReloadHandler onFPSReloadE;				//called when FPS weapon start/complete reloading (true/false)
		public static void OnFPSReload(bool flag){ if(onFPSReloadE!=null) onFPSReloadE(flag); }
		
		public delegate void FPSSwitchWeaponHandler();
		public static event FPSSwitchWeaponHandler onFPSSwitchWeaponE;		//called when FPS mode switch weapon
		public static void OnFPSSwitchWeapon(){ if(onFPSSwitchWeaponE!=null) onFPSSwitchWeaponE(); }
		
		public delegate void OnFPSSwitchCamHandler();
		public static event OnFPSSwitchCamHandler onFPSSwitchCameraE;
		public static void OnFPSSwitchCamera(){ if(onFPSSwitchCameraE!=null) onFPSSwitchCameraE(); }
		
		
		
		public delegate void PerkPurchasedHandler(Perk perk);
		public static event PerkPurchasedHandler onPerkPurchasedE;		//called when a perk is purchased 
		public static void OnPerkPurchased(Perk perk){ if(onPerkPurchasedE!=null) onPerkPurchasedE(perk); }
		
		public delegate void PerkPointHandler();
		public static event PerkPointHandler onPerkPointE;					//called when perk point is changed
		public static void OnPerkPoint(){ if(onPerkPointE!=null) onPerkPointE(); }
		
		
		
		
		private static List<UnitTower> towerDBList=new List<UnitTower>();
		private static List<Ability> abilityDBList=new List<Ability>();
		private static List<Perk> perkDBList=new List<Perk>();
		private static List<FPSWeapon> fpsWeaponDBList=new List<FPSWeapon>();
		
		public static List<UnitTower> GetTowerDBList(){ return new List<UnitTower>( towerDBList ); }
		public static List<Ability> GetAbilityDBList(){ return new List<Ability>( abilityDBList ); }
		public static List<Perk> GetPerkDBList(){ return new List<Perk>( perkDBList ); }
		public static List<FPSWeapon> GetFpsWeaponDBList(){ return new List<FPSWeapon>( fpsWeaponDBList ); }
		
		public static UnitTower GetDBTower(int prefabID){
			for(int i=0; i<towerDBList.Count; i++){
				if(towerDBList[i].prefabID==prefabID) return towerDBList[i];
			}
			return null;
		}
		public static Ability GetDBAbility(int prefabID){
			for(int i=0; i<abilityDBList.Count; i++){
				if(abilityDBList[i].ID==prefabID) return abilityDBList[i];
			}
			return null;
		}
		public static Perk GetDBPerk(int prefabID){
			for(int i=0; i<perkDBList.Count; i++){
				if(perkDBList[i].ID==prefabID) return perkDBList[i];
			}
			return null;
		}
		public static FPSWeapon GetDBFpsWeapon(int prefabID){
			for(int i=0; i<fpsWeaponDBList.Count; i++){
				if(fpsWeaponDBList[i].prefabID==prefabID) return fpsWeaponDBList[i];
			}
			return null;
		}
		
		private static bool init=false;
		public static void InitDB(){
			if(init) return;
			init=true;
			
			towerDBList=TowerDB.Load();
			abilityDBList=AbilityDB.Load();
			perkDBList=PerkDB.Load();
			fpsWeaponDBList=FPSWeaponDB.Load();
		}
		
		
		
		public static List<Unit> GetUnitInRange(Vector3 pos, float range, LayerMask mask){
			return GetUnitInRange(pos, range, 0, mask);
		}
		public static List<Unit> GetUnitInRange(Vector3 pos, float range, float minRange, LayerMask mask){
			List<Unit> unitList=new List<Unit>();
			
			Collider[] cols=Physics.OverlapSphere(pos, range, mask);
			for(int i=0; i<cols.Length; i++){
				Unit unit=cols[i].GetComponent<Unit>();
				if(unit==null) continue;
				if(unit.IsDestroyed()) continue;
				if(Vector3.Distance(pos, unit.thisT.position)>range) continue;
				
				unitList.Add(unit);
			}
			
			if(minRange>0){
				for(int i=0; i<unitList.Count; i++){
					if(Vector3.Distance(pos, unitList[i].thisT.position)>minRange) continue;
					unitList.RemoveAt(i);	i-=1;
				}
			}
			
			return unitList;
		}
		
		
		
		
		
		public static Vector3 GetTouchPosition(int pointerID){
			for(int i=0; i<Input.touchCount; i++){
				if(Input.touches[i].fingerId==pointerID) return Input.touches[i].position;
			}
			return new Vector3(0, -50, 0);
		}
		
		public static bool IsTouchStarting(int pointerID){
			for(int i=0; i<Input.touchCount; i++){
				if(Input.touches[i].fingerId==pointerID){
					if(Input.touches[i].phase==TouchPhase.Began) return true;
				}
			}
			return false;
		}
		public static bool IsTouchEnding(int pointerID){
			for(int i=0; i<Input.touchCount; i++){
				if(Input.touches[i].fingerId==pointerID){
					if(Input.touches[i].phase==TouchPhase.Ended) return true;
				}
			}
			return false;
		}
		
		
		
	}

}
