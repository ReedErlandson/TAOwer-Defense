using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class SpawnManager : MonoBehaviour {
		
		public enum _SpawnMode{Continous, WaveCleared, Round}
		public _SpawnMode spawnMode;
		//public static _SpawnMode GetSpawnMode(){ return instance.spawnMode; }
		
		public enum _SpawnLimit{Finite, Infinite}
		public _SpawnLimit spawnLimit=_SpawnLimit.Finite;
		
		public bool allowSkip=false;
		
		public bool autoStart=false;
		public float autoStartDelay=5;
		public static bool AutoStart(){ return instance==null ? false : instance.autoStart; }
		public static float GetAutoStartDelay(){ return instance.autoStartDelay; }
		
		public bool procedurallyGenerateWave=false;	//used in finite mode, when enabled, all wave is generate procedurally
		
		public PathTD defaultPath;
		private int currentWaveID=-1;			//start at -1, switch to 0 as soon as first wave start, always indicate latest spawned wave's ID
		public bool spawning=false;
		
		public int activeUnitCount=0;	//for wave-cleared mode checking
		public int totalSpawnCount=0;	//for creep instanceID
		public int waveClearedCount=0; 	//for quick checking how many wave has been cleared
		
		public List<Wave> waveList=new List<Wave>();	//in endless mode, this is use to store temporary wave
		
		public WaveGenerator waveGenerator;
		
		public static SpawnManager instance;
		
		void Awake(){
			instance=this;
		}
		
		// Use this for initialization
		void Start () {
			if(defaultPath==null){
				Debug.Log("DefaultPath on SpawnManager not assigned, auto search for one");
				defaultPath=(PathTD)FindObjectOfType(typeof(PathTD));
			}
			
			if(spawnLimit==_SpawnLimit.Infinite || procedurallyGenerateWave){
				waveGenerator.Init();
				if(defaultPath!=null && waveGenerator.pathList.Count==0) waveGenerator.pathList.Add(defaultPath);
			}
			
			//waveGenerator.Generate(i);
			if(spawnLimit==_SpawnLimit.Finite && procedurallyGenerateWave){
				for(int i=0; i<waveList.Count; i++){
					waveList[i]=waveGenerator.Generate(i);
				}
			}
			
			if(spawnLimit==_SpawnLimit.Infinite) waveList=new List<Wave>();
			if(spawnLimit==_SpawnLimit.Finite){
				for(int i=0; i<waveList.Count; i++) waveList[i].waveID=i;
			}
			
			if(autoStart) StartCoroutine(AutoStartRoutine());
		}
		
		
		IEnumerator AutoStartRoutine(){
			//yield return new WaitForSeconds(autoStartDelay);
			while(autoStartDelay>0){
				if(IsSpawningStarted()) yield break;
				autoStartDelay-=Time.deltaTime;
				spawnCD=autoStartDelay;
				yield return null;
			}
			if(!IsSpawningStarted()) _Spawn();
		}
		
		
		
		
		public static void OnUnitDestroyed(UnitCreep unit){
			instance.OnUnitCleared(unit);
		}
		public static void OnCreepReachDestination(UnitCreep creep){
			if(!creep.IsDestroyed()) instance.OnUnitCleared(creep);
		}
		void OnUnitCleared(UnitCreep creep){
			if(GameControl.IsGameOver()) return;
			
			int waveID=creep.waveID;
			
			activeUnitCount-=1;
			
			Wave wave=null;
			if(spawnLimit==_SpawnLimit.Finite) wave=waveList[waveID];
			else if(spawnLimit==_SpawnLimit.Infinite){
				for(int i=0; i<waveList.Count; i++){
					if(waveList[i].waveID==waveID){
						wave=waveList[i];
						break;
					}
				}
				
				if(wave==null){
					Debug.Log("error!");
					return;
				}
			}
			
			
			wave.activeUnitCount-=1;
			if(wave.spawned && wave.activeUnitCount==0){
				wave.cleared=true;
				waveClearedCount+=1;
				
				TDTK.OnWaveCleared(waveID);
				Debug.Log("wave"+(waveID+1)+ " is cleared");
				
				ResourceManager.GainResource(wave.rscGainList, PerkManager.GetRscWaveCleared());
				GameControl.GainLife(wave.lifeGain+PerkManager.GetLifeWaveClearedModifier());
				AbilityManager.GainEnergy(wave.energyGain+(int)PerkManager.GetEnergyWaveClearedModifier());
				
				if(spawnLimit==_SpawnLimit.Infinite) waveList.Remove(wave);
				
				if(IsAllWaveCleared()){
					GameControl.GameOver(true);	//pass true to signify level won
				}
				else{
					if(spawnMode==_SpawnMode.Round) TDTK.OnEnableSpawn();// && onEnableSpawnE!=null) onEnableSpawnE();
				}
			}
			
			
			if(!IsAllWaveCleared() && activeUnitCount==0 && !spawning){
				if(spawnMode==_SpawnMode.WaveCleared) SpawnWaveFinite();
			}
			
		}
		
		
		//add a SpawnUponDestroyed creep to the wave
		public static int AddDestroyedSpawn(UnitCreep unit){ return instance._AddDestroyedSpawn(unit); }
		public int _AddDestroyedSpawn(UnitCreep unit){
			activeUnitCount+=1;
			
			if(spawnLimit==_SpawnLimit.Finite) waveList[unit.waveID].activeUnitCount+=1;
			else if(spawnLimit==_SpawnLimit.Infinite){
				for(int i=0; i<waveList.Count; i++){
					if(waveList[i].waveID==unit.waveID){
						waveList[i].activeUnitCount+=1;
						break;
					}
				}
			}
			
			return totalSpawnCount+=1;
		}
		
		
		
		//general call to spawn the next wave, if available
		public static void Spawn(){ instance._Spawn(); }
		public void _Spawn(){
			if(GameControl.IsGameOver()) return;
			
			if(IsSpawningStarted()){
				if(spawnMode==_SpawnMode.Round){
					if(spawnLimit==_SpawnLimit.Infinite){
						if(waveList.Count!=0) return;
					}
					else{
						if(!waveList[currentWaveID].cleared) return;
					}
				}
				else if(!allowSkip) return;
				
				spawnCD=SpawnWaveFinite();
				
				return;
			}
			
			if(spawnMode!=_SpawnMode.Continous) SpawnWaveFinite();
			else StartCoroutine(ContinousSpawnRoutine());
			
			GameControl.StartGame();
		}
		
		
		private float spawnCD=0;
		IEnumerator ContinousSpawnRoutine(){
			while(true){
				if(GameControl.IsGameOver()) yield break;
				
				spawnCD=SpawnWaveFinite();
				if(spawnLimit==_SpawnLimit.Finite && currentWaveID>=waveList.Count) break;
				
				yield return null;
				
				while(spawnCD>0){
					spawnCD-=Time.deltaTime;
					yield return null;
				}
			}
		}
		
		
		
		
		private float SpawnWaveFinite(){
			if(spawning) return 0;
			
			spawning=true;
			currentWaveID+=1;
			
			if(spawnLimit==_SpawnLimit.Finite && currentWaveID>=waveList.Count) return 0;
			
			Debug.Log("spawning wave"+(currentWaveID+1));
			TDTK.OnNewWave(currentWaveID+1);
			
			Wave wave=null;
			if(spawnLimit==_SpawnLimit.Finite) wave=waveList[currentWaveID];
			else if(spawnLimit==_SpawnLimit.Infinite){
				wave=waveGenerator.Generate(currentWaveID);
				wave.waveID=currentWaveID;
				waveList.Add(wave);
			}
			
			if(spawnMode==_SpawnMode.Continous){
				if((spawnLimit==_SpawnLimit.Finite && currentWaveID<waveList.Count-1) || spawnLimit==_SpawnLimit.Infinite){
					TDTK.OnSpawnTimer(wave.duration);
				}
			}
			
			for(int i=0; i<wave.subWaveList.Count; i++){
				StartCoroutine(SpawnSubWave(wave.subWaveList[i], wave));
			}
			
			return wave.duration;
		}
		IEnumerator SpawnSubWave(SubWave subWave, Wave parentWave){
			if(subWave.unit==null){
				Debug.LogWarning("No creep prefab has been assigned to sub-wave", this);
				yield break;
			}
			
			yield return new WaitForSeconds(subWave.delay);
			
			PathTD path=defaultPath;
			if(subWave.path!=null) path=subWave.path;
				
			Vector3 pos=path.GetSpawnPoint();
			Quaternion rot=path.GetSpawnDirection();
			
			int spawnCount=0;
			
			if(subWave.unitC==null) subWave.unitC=subWave.unit.GetComponent<UnitCreep>();
			
			while(spawnCount<subWave.count){
				if(subWave.unit==null){
					Debug.LogWarning("no creep has been assigned to subwave", this);
					break;
				}
				
				GameObject obj=ObjectPoolManager.Spawn(subWave.unit, pos, rot);
				UnitCreep unit=obj.GetComponent<UnitCreep>();
				
				if(subWave.overrideHP>0) unit.defaultHP=subWave.overrideHP;
				else unit.defaultHP=subWave.unitC.defaultHP;
					
				if(subWave.overrideShield>0) unit.defaultShield=subWave.overrideShield;
				else unit.defaultShield=subWave.unitC.defaultShield;
					
				if(subWave.overrideMoveSpd>0) unit.moveSpeed=subWave.overrideMoveSpd;
				else unit.moveSpeed=subWave.unitC.moveSpeed;
				
				unit.Init(path, totalSpawnCount, parentWave.waveID);
				
				totalSpawnCount+=1;
				activeUnitCount+=1;
				
				parentWave.activeUnitCount+=1;
				
				spawnCount+=1;
				if(spawnCount==subWave.count) break;
				
				yield return new WaitForSeconds(subWave.interval);
			}
			
			parentWave.subWaveSpawnedCount+=1;
			if(parentWave.subWaveSpawnedCount==parentWave.subWaveList.Count){
				parentWave.spawned=true;
				spawning=false;
				Debug.Log("wave "+(parentWave.waveID+1)+" has done spawning");
				
				yield return new WaitForSeconds(0.5f);
				
				if(currentWaveID<=waveList.Count-2 || spawnLimit==_SpawnLimit.Infinite){
					if((spawnMode==_SpawnMode.WaveCleared || spawnMode==_SpawnMode.Continous) && allowSkip) 
						TDTK.OnEnableSpawn();
				}
			}
		}
		
		
		
		
		public bool IsSpawningStarted(){
			return currentWaveID>=0 ? true : false;
		}
		
		public static bool IsAllWaveCleared(){
			if(instance.spawnLimit==_SpawnLimit.Infinite) return false;
			
			if(instance.waveClearedCount>=instance.waveList.Count) return true;
			return false;
		}
		
		public static int GetTotalWaveCount(){
			if(instance==null || instance.spawnLimit==_SpawnLimit.Infinite) return -1;
			return instance.waveList.Count;
		}
		
		
		public static float GetTimeToNextSpawn(){ return instance._GetTimeToNextSpawn(); }
		public float _GetTimeToNextSpawn(){
			if(spawnMode==_SpawnMode.Round || spawnMode==_SpawnMode.WaveCleared) return -1;
			if(spawnLimit==_SpawnLimit.Finite && currentWaveID>=waveList.Count-1) return -1;
			return spawnCD;
		}
		
		
		public static int GetCurrentWaveID(){ return instance==null ? 0 : instance.currentWaveID; }
		
		public static int GetActiveUnitCount(){ return instance.activeUnitCount; }
		
	}

}