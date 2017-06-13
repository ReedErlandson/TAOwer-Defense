using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public enum _GameState{Play, Pause, Over}
	
	[RequireComponent (typeof (ResourceManager))]
	[RequireComponent (typeof (DamageTable))]


	public class GameControl : MonoBehaviour {
		
		private bool gameStarted=false;
		public static void StartGame(){ instance.gameStarted=true; }	//when from SpawnManager when spawn
		public static bool IsGameStarted(){ return instance.gameStarted; }
		
		public _GameState gameState=_GameState.Play;
		public static _GameState GetGameState(){ return instance.gameState; }
		public static bool IsGameOver(){ return instance.gameState==_GameState.Over ? true : false; }
		public static bool IsGamePlaying(){ return instance.gameState==_GameState.Play ? true : false; }
		public static bool IsGamePaused(){ return instance.gameState==_GameState.Pause ? true : false; }
		
		
		public float ffSpeed=2.5f;
		public static void FastForwardOn(){ FastForward(true); }
		public static void FastForwardOff(){ FastForward(false); }
		public static void FastForward(bool flag){
			Time.timeScale=flag ? instance.ffSpeed : 1;
			instance.fastforward=flag;
			TDTK.OnFastForward(flag);
		}
		private bool fastforward=false;
		public static bool IsFastForwardOn(){ return instance.fastforward; }
		
		
		
		public bool playerWon=false;
		public static bool HasPlayerWon(){ return instance.playerWon; }
		
		
		public int levelID=1;	//the level progression (as in lvl1, lvl2, lvl3 and so on), user defined, use to verify perk availability
										//also useful if you want to save player progresssion usinhg player pref (which level is unlocked)
		public static int GetLevelID(){ return instance.levelID; }
		
		
		public bool capLife=false;
		public int playerLifeCap=0;
		public int playerLife=10;
		public static int GetPlayerLife(){ return instance.playerLife;	}
		public static int GetPlayerLifeCap(){	return instance.capLife ? instance.playerLifeCap+PerkManager.GetLifeCapModifier() : -1;	}
		
		public bool enableLifeGen=false;
		public float lifeRegenRate=0.1f;
		
		public float sellTowerRefundRatio=0.5f;
		
		public bool resetTargetAfterShoot=true;
		public static bool ResetTargetAfterShoot(){ return instance.resetTargetAfterShoot; }

		
		
		public string nextScene="";
		public string mainMenu="";
		public static void LoadNextScene(){ Load(instance.nextScene); }
		public static void LoadMainMenu(){ Load(instance.mainMenu); }
		public static void Load(string levelName){
			if(levelName==""){
				Debug.LogWarning("Trying to load unspecificed scene", instance);
				return;
			}
			Time.timeScale=1;
			
			#if UNITY_5_3_OR_NEWER
				SceneManager.LoadScene(levelName);
			#else
				Application.LoadLevel(levelName);
			#endif
		}
		public static void RestartScene(){
			ResourceManager.OnRestartLevel();
			Time.timeScale=1;
			
			#if UNITY_5_3_OR_NEWER
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			#else
				Application.LoadLevel(Application.loadedLevelName);
			#endif
		}

		
		
		//public bool loadAudioManager=false;
		
		private float timeStep=0.015f;
		
		public static GameControl instance;
		public Transform thisT;

		void Awake(){
			Time.fixedDeltaTime = timeStep;
			
			instance=this;
			thisT=transform;
			
			ObjectPoolManager.Init();
			
			NodeGenerator nodeGenerator = (NodeGenerator)FindObjectOfType(typeof(NodeGenerator));
			if(nodeGenerator!=null) nodeGenerator.Awake();
			PathFinder pathFinder = (PathFinder)FindObjectOfType(typeof(PathFinder));
			if(pathFinder!=null) pathFinder.Awake();
			
			PathTD[] paths = FindObjectsOfType(typeof(PathTD)) as PathTD[];
			if(paths.Length>0){ for(int i=0; i<paths.Length; i++) paths[i].Init(); }
			
			TDTK.InitDB();
			
			ResourceManager rscManager = (ResourceManager)FindObjectOfType(typeof(ResourceManager));
			if(rscManager!=null) rscManager.Init();
			
			PerkManager perkManager = (PerkManager)FindObjectOfType(typeof(PerkManager));
			if(perkManager!=null) perkManager.Init();
			
			BuildManager buildManager = (BuildManager)FindObjectOfType(typeof(BuildManager));
			if(buildManager!=null) buildManager.Init();
			
			AbilityManager abilityManager = (AbilityManager)FindObjectOfType(typeof(AbilityManager));
			if(abilityManager!=null) abilityManager.Init();
			
			FPSControl fpsControl = (FPSControl)FindObjectOfType(typeof(FPSControl));
			if(fpsControl!=null) fpsControl.Init();
			
			IndicatorControl indicatorControl = (IndicatorControl)FindObjectOfType(typeof(IndicatorControl));
			if(indicatorControl!=null) indicatorControl.Init();
			
			
			//if(loadAudioManager){
			//	Instantiate(Resources.Load("AudioManager", typeof(GameObject)));
			//}
			
			Time.timeScale=1;
		}


		// Use this for initialization
		void Start () {
			UnitTower[] towers = FindObjectsOfType(typeof(UnitTower)) as UnitTower[];
			for(int i=0; i<towers.Length; i++) BuildManager.PreBuildTower(towers[i]);
			
			//ignore collision between shootObject so they dont hit each other
			int soLayer=TDTK.GetLayerShootObject();
			Physics.IgnoreLayerCollision(soLayer, soLayer, true); 
			
			//playerLife=playerLifeCap;
			if(capLife) playerLife=Mathf.Min(playerLife, GetPlayerLifeCap());
			
			if(enableLifeGen) StartCoroutine(LifeRegenRoutine());
		}
		
		
		
		
		public static void OnCreepReachDestination(UnitCreep unit){ instance._OnCreepReachDestination(unit); }
		public void _OnCreepReachDestination(UnitCreep unit){
			playerLife=Mathf.Max(0, playerLife-unit.lifeCost);
			
			TDTK.OnLife(-unit.lifeCost);
			
			if(playerLife<=0) GameOver();
		}
		
		public static void GameOver(bool won=false){
			if(instance.gameState==_GameState.Over) return;
			
			instance.playerWon=won;
			
			ResourceManager.OnGameOver(won);	//to record current resource
			
			instance.gameState=_GameState.Over;
			TDTK.OnGameOver(won);
		}
		
		
		
		IEnumerator LifeRegenRoutine(){
			float temp=0;
			while(true){
				yield return new WaitForSeconds(1);
				temp+=lifeRegenRate+PerkManager.GetLifeRegenModifier();
				int value=0;
				while(temp>=1){
					value+=1;
					temp-=1;
				}
				if(value>0) _GainLife(value);
			}
		}
		
		public static void GainLife(int value){ instance._GainLife(value); }
		public void _GainLife(int value){
			playerLife+=value;
			if(capLife) playerLife=Mathf.Min(playerLife, GetPlayerLifeCap());
			//if(onLifeE!=null) onLifeE(value);
			TDTK.OnLife(value);
		}
		
		
		
		
		
		
		public UnitTower selectedTower;
		public static UnitTower GetSelectedTower(){ return instance.selectedTower; }
		
		public static UnitTower Select(Vector3 pointer){
			LayerMask mask=1<<TDTK.GetLayerTower();
			Ray ray = Camera.main.ScreenPointToRay(pointer);
			RaycastHit hit;
			if(!Physics.Raycast(ray, out hit, Mathf.Infinity, mask)) return null;
			
			return hit.transform.GetComponent<UnitTower>();
		}
		
		
		public static void ClearSelectedTower(){ SelectTower(null); }
		public static void SelectTower(UnitTower tower=null){ instance._SelectTower(tower); }
		public void _SelectTower(UnitTower tower=null){
			if(tower==null) IndicatorControl.ClearRangeIndicator();
			else IndicatorControl.ShowTowerRangeIndicator(tower);
		}
		
		public static void TowerScanAngleChanged(UnitTower tower){
			IndicatorControl.TowerScanAngleChanged(tower);
		}
		
		
		
		
		public static void PauseGame(){
			FastForwardOff();
			
			instance.gameState=_GameState.Pause;
			Time.timeScale=0;
		}
		public static void ResumeGame(){
			instance.gameState=_GameState.Play;
			Time.timeScale=1;
		}
		
		
		public static float GetSellTowerRefundRatio(){
			return instance.sellTowerRefundRatio;
		}
		
		
	}

}