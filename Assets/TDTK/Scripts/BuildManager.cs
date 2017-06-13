using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {
	
	public enum _BuildMode{PointNBuild, DragNDrop};
	
	public class BuildManager : MonoBehaviour {
		
		public _BuildMode buildMode=_BuildMode.PointNBuild;
		public static bool UseDragNDrop(){ return instance.buildMode==_BuildMode.PointNBuild ? false : true; }
		
		
		public float gridSize=1.5f;
		public static float GetGridSize(){ return instance.gridSize; }
		
		
		[Tooltip("When checked, player cannot build tower when there are active creep in the scene")]
		public bool disableBuildWhenInPlay=false;
		
		
		public bool autoAdjustTextureToGrid=true;
		private List<PlatformTD> buildPlatforms=new List<PlatformTD>();
		
		//prefabID of tower available in this level
		public List<int> unavailableTowerIDList=new List<int>();
		[HideInInspector] public List<int> availableTowerIDList=new List<int>();
		
		//only used in runtime, filled up using info from availableTowerIDList
		private List<UnitTower> towerList=new List<UnitTower>();
		
		
		private int towerCount=0;
		public static int GetTowerCount(){ return instance.towerCount; }
		
		
		private static BuildManager instance;
		public static BuildManager GetInstance(){ return instance; }
		
		public void Init(){
			instance=this;
			
			gridSize=Mathf.Max(0.25f, gridSize);
			
			InitTower();
			InitPlatform();
		}
		
		
		void Start(){
			SetupLayerMask();
			InitiateSampleTowerList();
		}
		
		public void InitTower(){
			List<UnitTower> towerListDB=TDTK.GetTowerDBList();	//TowerDB.Load();
			
			availableTowerIDList=new List<int>();
			towerList=new List<UnitTower>();
			for(int i=0; i<towerListDB.Count; i++){
				if(towerListDB[i]==null) continue;
				if(towerListDB[i].disableInBuildManager) continue;
				if(unavailableTowerIDList.Contains(towerListDB[i].prefabID)) continue;
				//if(availableTowerIDList.Contains(towerListDB[i].prefabID)) 
					
				towerList.Add(towerListDB[i]);
				availableTowerIDList.Add(towerListDB[i].prefabID);
			}
			
			List<UnitTower> newList=PerkManager.GetUnlockedTowerList();
			for(int i=0; i<newList.Count; i++) towerList.Add(newList[i]);
		}
		

		// Use this for initialization
		void InitPlatform(){
			//~ if(autoSearchForPlatform){
				buildPlatforms=new List<PlatformTD>();
				PlatformTD[] platList = FindObjectsOfType(typeof(PlatformTD)) as PlatformTD[];
				for(int i=0; i<platList.Length; i++) buildPlatforms.Add(platList[i]);
			//~ }
			
			for(int i=0; i<buildPlatforms.Count; i++){
				buildPlatforms[i].Init(gridSize, autoAdjustTextureToGrid, towerList);
			}
		}
		
		
		//setup layerMask used in detecting platform and build point
		public LayerMask maskPlatform;
		public LayerMask maskAll;
		public LayerMask maskIndicator;
		private void SetupLayerMask(){
			//layerMask for platform only
			maskPlatform=1<<TDTK.GetLayerPlatform();
			//layerMask for detect all collider within buildPoint
			maskAll=1<<TDTK.GetLayerPlatform();
			int terrainLayer=TDTK.GetLayerTerrain();
			if(terrainLayer>=0) maskAll|=1<<terrainLayer;
			
			maskIndicator=1<<TDTK.GetLayerPlatform() | 1<<TDTK.GetLayerTerrain() | 1<<TDTK.GetLayerTower();
			maskIndicator|=1<<TDTK.GetLayerCreep() | 1<<TDTK.GetLayerCreepF() | 1<<TDTK.GetLayerShootObject();
		}
		
		
		public static void AddNewTower(UnitTower newTower){ if(instance!=null) instance._AddNewTower(newTower); }
		public void _AddNewTower(UnitTower newTower){
			if(towerList.Contains(newTower)) return;
			towerList.Add(newTower);
			availableTowerIDList.Add(newTower.prefabID);
			
			AddNewSampleTower(newTower);
			
			for(int i=0; i<buildPlatforms.Count; i++){
				buildPlatforms[i].availableTowerIDList.Add(newTower.prefabID);
			}
			
			TDTK.OnNewBuildableTower(newTower);
		}
		
		
		//called from IndicatorControl to set the tile indicator to a tile point
		public static void SetTileIndicator(Vector3 cursor){ instance._SetTileIndicator(cursor); }
		public void _SetTileIndicator(Vector3 cursor){
			Ray ray = Camera.main.ScreenPointToRay(cursor);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
				for(int i=0; i<buildPlatforms.Count; i++){
					if(hit.transform!=buildPlatforms[i].thisT) continue;
					Vector3 tilePos=GetTilePos(buildPlatforms[i], hit.point);
					
					Collider[] cols=Physics.OverlapSphere(tilePos, gridSize/2*0.9f, ~maskIndicator);
					if(cols.Length==0) IndicatorControl.SetIndicatorCursor(tilePos, buildPlatforms[i].thisT.rotation);
					else break;
					
					return;
				}
			}
			IndicatorControl.ClearIndicatorCursor();
		}
		
		
		public static Vector3 GetTilePos(PlatformTD platform, Vector3 hitPos){ return instance._GetTilePos(platform, hitPos); }
		public Vector3 _GetTilePos(PlatformTD platform, Vector3 hitPos){
			Vector3 v=hitPos-platform.thisT.position;	//get the vector from platform origin to hit point
			
			//transform the vector to the platform local space, so we know the (x, y)
			v=Quaternion.Euler(0, -platform.thisT.rotation.eulerAngles.y, 0) * v;	
			
			//check the size of the platform for odd/even columen and then set the offset in corresponding axis
			float osX=platform.size.x%2==0 ? gridSize/2 : 0;
			float osZ=platform.size.y%2==0 ? gridSize/2 : 0;
			
			//calculate the x and z position (this is the relative position in platform local space to the platform origin)
			float x=Mathf.Round((osX+v.x)/gridSize)*gridSize-osX;
			float z=Mathf.Round((osZ+v.z)/gridSize)*gridSize-osZ;
			
			//transform the calculated position to world space
			return platform.thisT.position+platform.thisT.TransformDirection(new Vector3(x, 0, z));
		}
		
		
		
		
		
		public static BuildInfo CheckBuildPoint(Vector3 pointer, int towerID=-1){ 
			return instance._CheckBuildPoint(pointer, towerID);
		}
		public BuildInfo _CheckBuildPoint(Vector3 pointer, int towerID){ 
			BuildInfo buildInfo=new BuildInfo();
			
			if(disableBuildWhenInPlay && SpawnManager.GetActiveUnitCount()>0){
				buildInfo.status=_TileStatus.NotInBuildPhase;
				return buildInfo;
			}
			
			Camera mainCam=Camera.main;
			if(mainCam!=null){
				Ray ray = mainCam.ScreenPointToRay(pointer);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
					
					for(int i=0; i<buildPlatforms.Count; i++){
						if(hit.transform==buildPlatforms[i].thisT){
							PlatformTD platform=buildPlatforms[i];
							
							//calculating the build center point base on the input position
							Vector3 pos=GetTilePos(platform, hit.point);
							
							buildInfo.position=pos;
							buildInfo.platform=platform;
							
							//checking if tower can be built on the platform, for dragNdrop mode
							if(towerID>=0 && !platform.availableTowerIDList.Contains(towerID)) 
								buildInfo.status=_TileStatus.Unavailable;
							
							if(buildInfo.status==_TileStatus.Available){
								//check if the position is blocked, by any other obstabcle other than the baseplane itself
								Collider[] cols=Physics.OverlapSphere(pos, gridSize/2*0.9f, ~maskAll);
								if(cols.Length>0) buildInfo.status=_TileStatus.NoPlatform;
								else buildInfo.status=_TileStatus.Available;
								
								if(buildInfo.status==_TileStatus.Available){
									//check if the platform is walkable, if so, check if building on the point wont block all possible path
									if(platform.IsWalkable()){
										if(platform.CheckForBlock(pos)){
											buildInfo.status=_TileStatus.Blocked;
										}
									}
									
									//map platform availableTowerIDList (which is the towers' prefabID) to the list elements' ID in towerList
									buildInfo.availableTowerIDList=new List<int>();
									for(int m=0; m<platform.availableTowerIDList.Count; m++){
										for(int n=0; n<towerList.Count; n++){
											if(platform.availableTowerIDList[m]==towerList[n].prefabID){
												buildInfo.availableTowerIDList.Add(n);
												break;
											}
										}
									}
								}
								
							}
							
							break;
						}
					}

				}
				else buildInfo.status=_TileStatus.NoPlatform;
			}
			else buildInfo.status=_TileStatus.NoPlatform;
			
			//reverse block status for mine 
			if(buildInfo.status==_TileStatus.Blocked){
				//for drag n drop mode
				if(towerID>=0 && GetTowerPrefab(towerID).type==_TowerType.Mine) buildInfo.status=_TileStatus.Available;
				if(towerID<0){
					bool gotMineInList=false;
					for(int i=0; i<buildInfo.availableTowerIDList.Count; i++){
						if(towerList[buildInfo.availableTowerIDList[i]].type==_TowerType.Mine) gotMineInList=true;
						else{
							buildInfo.availableTowerIDList.RemoveAt(i);
							i-=1;
						}
					}
					if(gotMineInList) buildInfo.status=_TileStatus.Available;
				}
			}
			
			
			if(!UseDragNDrop()){ //for PointNClick
				if(buildInfo.status!=_TileStatus.Available) IndicatorControl.ClearBuildTileIndicator();
				else IndicatorControl.SetBuildTileIndicator(buildInfo);	
			}
			
			return buildInfo;
		}
		
		
		
		
		//called when a tower building is initated in DragNDrop, use the sample tower as the model and set it in DragNDrop mode
		public string StartDragNDrop(int ID, int pointerID=-1){
			UnitTower sampleTower=GetSampleTower(ID);
			
			if(sampleTower.type==_TowerType.Resource && !GameControl.IsGameStarted()){
				return "Cant Build Tower before spawn start"; 
			}
			
			IndicatorControl.SetDragNDropPhase(true);
			
			List<int> cost=sampleTower.GetCost();
			int suffCost=ResourceManager.HasSufficientResource(cost);
			if(suffCost==-1){
				sampleTower.thisT.position=new Vector3(9999, 9999, 9999);
				sampleTower.thisObj.SetActive(true);
				
				UnitTower towerInstance=sampleTower;
				
				towerInstance.StartCoroutine(towerInstance.DragNDropRoutine(pointerID));
				
				return "";
			}
			
			return "Insufficient Resource   "+suffCost;
		}
		public static bool InDragNDrop(){ return UseDragNDrop() && UnitTower.InDragNDrop(); }
		public static void ExitDragNDrop(){ UnitTower.InDragNDrop(); }
		
		
		//called by any external component to build tower, 
		public static string BuildTower(int ID, BuildInfo bInfo, int pointerID=-1){
			if(UseDragNDrop()) return instance.StartDragNDrop(ID, pointerID);
			else{
				ClearSampleTower();
				return _BuildTower(instance.towerList[ID], bInfo);
			}
		}
		public static string _BuildTower(UnitTower tower, BuildInfo bInfo){	//called from UnitTower.DragNDropRoutine
			if(bInfo==null){
				if(!UseDragNDrop()) return "Select a Build Point First";
				else return "Invalid build position";
			}
			
			if(bInfo.status!=_TileStatus.Available) return "Invalid build position";
			
			//dont allow building of resource tower before game started
			if(tower.type==_TowerType.Resource && !GameControl.IsGameStarted()){
				return "Cant Build Tower before spawn start"; 
			}
			
			UnitTower sampleTower=GetSampleTower(tower);
			
			//check if there are sufficient resource
			List<int> cost=sampleTower.GetCost();
			int suffCost=ResourceManager.HasSufficientResource(cost);
			if(suffCost==-1){
				ResourceManager.SpendResource(cost);
				
				GameObject towerObj=(GameObject)Instantiate(tower.gameObject, bInfo.position, bInfo.platform.thisT.rotation);
				UnitTower towerInstance=towerObj.GetComponent<UnitTower>();
				towerInstance.InitTower(instance.towerCount+=1);
				towerInstance.Build();
				
				//if(bInfo.platform!=null) towerObj.transform.parent=bInfo.platform.transform;
				
				//register the tower to the platform
				if(bInfo.platform!=null) bInfo.platform.BuildTower(bInfo.position, towerInstance);
				
				//clear the build info and indicator for build manager
				//ClearBuildIndicator();
				IndicatorControl.ClearBuildTileIndicator();
				
				return "";
			}
			
			return "Insufficient Resource";
		}
		
		
		public static void PreBuildTower(UnitTower tower){
			PlatformTD platform=null;
			LayerMask mask=1<<TDTK.GetLayerPlatform();
			Collider[] cols=Physics.OverlapSphere(tower.thisT.position, GetGridSize(), mask);
			if(cols.Length>0) platform=cols[0].gameObject.GetComponent<PlatformTD>();
			
			
			if(platform!=null){
				Vector3 buildPos=GetTilePos(platform, tower.thisT.position);
				tower.thisT.position=buildPos;
				tower.thisT.rotation=platform.thisT.rotation;
				platform.BuildTower(buildPos, tower);
			}
			else Debug.Log("no platform found for pre-placed tower");
			
			tower.InitTower(instance.towerCount+=1);
		}
		
		
		
		
		
		
		
		private List<UnitTower> sampleTowerList=new List<UnitTower>();
		private int currentSampleID=-1;
		public void InitiateSampleTowerList(){
			sampleTowerList=new List<UnitTower>();
			for(int i=0; i<towerList.Count; i++){
				UnitTower towerInstance=CreateSampleTower(towerList[i]);
				sampleTowerList.Add(towerInstance);
			}
		}
		public void AddNewSampleTower(UnitTower newTower){
			UnitTower towerInstance=CreateSampleTower(newTower);
			sampleTowerList.Add(towerInstance);
		}
		public UnitTower CreateSampleTower(UnitTower towerPrefab){
			GameObject towerObj=(GameObject)Instantiate(towerPrefab.gameObject);
			
			towerObj.transform.parent=transform;
			if(towerObj.GetComponent<Collider>()!=null) Destroy(towerObj.GetComponent<Collider>());
			Utility.DestroyColliderRecursively(towerObj.transform);
			
			towerObj.SetActive(false);
			
			UnitTower towerInstance=towerObj.GetComponent<UnitTower>();
			towerInstance.SetAsSampleTower(towerPrefab);
			
			return towerInstance;
		}
		
		public static UnitTower GetSampleTower(int ID){ return instance.sampleTowerList[ID]; }
		public static UnitTower GetSampleTower(UnitTower tower){
			for(int i=0; i<instance.sampleTowerList.Count; i++){
				if(instance.sampleTowerList[i].prefabID==tower.prefabID) return instance.sampleTowerList[i];
			}
			return null;
		}
		
		public static void ShowSampleTower(int ID, BuildInfo buildInfo){ instance._ShowSampleTowerList(ID, buildInfo); }
		public void _ShowSampleTowerList(int ID, BuildInfo buildInfo){
			if(currentSampleID==ID || buildInfo==null) return;
			
			if(currentSampleID>=0) ClearSampleTower();
			
			currentSampleID=ID;
			sampleTowerList[ID].thisT.position=buildInfo.position;
			sampleTowerList[ID].thisT.rotation=buildInfo.platform.transform.rotation;
			
			GameControl.SelectTower(sampleTowerList[ID]);
			
			sampleTowerList[ID].thisObj.SetActive(true);
		
		}
		
		public static void ClearSampleTower(){ instance._ClearSampleTower(); }
		public void _ClearSampleTower(){
			if(currentSampleID<0) return;
			sampleTowerList[currentSampleID].thisObj.SetActive(false);
			currentSampleID=-1;
		}
		
		
		
		
		
		
		
		public static int GetTowerListCount(){ return (instance==null) ? 0 : instance.towerList.Count; }
		public static List<UnitTower> GetTowerList(){ return (instance==null) ? new List<UnitTower>() : instance.towerList; }
		public static UnitTower GetTowerPrefab(int ID){
			foreach(UnitTower tower in instance.towerList){
				if(tower.prefabID==ID) return tower;
			}
			return null;
		}
		
	}
	

}

