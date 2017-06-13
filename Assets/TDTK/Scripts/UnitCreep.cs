using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public enum _CreepType{Default, Offense, Support}
	
	public class UnitCreep : Unit {
		
		[Header("Creep Setting")]
		public _CreepType type=_CreepType.Default;
		
		[HideInInspector] public int waveID=0;
		public bool flying=false;
		public bool stopToAttack=false;
		
		public float rotateSpd=12;
		public float moveSpeed=3;
		
		public bool rotateTowardsDestination=true;
		public bool rotateTowardsDestinationX=true;
		
		[Header("Player Lost Upon Creep Scored")]
		public int lifeCost=1;
		//public int scoreValue=1;
		
		[Header("Player Gain Upon Creep Destroyed")]
		public int lifeValue=0;
		public List<int> valueRscMin=new List<int>();
		public List<int> valueRscMax=new List<int>();
		public int valueEnergyGain=0;
		
		
		[Header("Spawn Upon Destroyed")]
		//public GameObject spawnUponDestroyed;
		public UnitCreep spawnUponDestroyed;
		public int spawnUponDestroyedCount=0;
		public float spawnUnitHPMultiplier=0.5f;
		
		private Vector3 pathDynamicOffset;
		public Vector3 GetPathDynamicOffset(){ return pathDynamicOffset; }
		
		
		[Header("Visual Effects")]
		public GameObject destinationEffObj;
		public bool autoDestroydestinationEff=true;
		public float destinationEffDuration=1;
		
		
		[HideInInspector] public PathTD path;
		[HideInInspector] public List<Vector3> subPath=new List<Vector3>();
		[HideInInspector] public int waypointID=1;
		[HideInInspector] public int subWaypointID=0;
		
		
		public override void Awake() {
			isCreep=true;
			
			if(!flying) gameObject.layer=TDTK.GetLayerCreep();
			else gameObject.layer=TDTK.GetLayerCreepF();
			
			//SetSubClass(this);
			
			base.Awake();
			
			if(thisObj.GetComponent<Collider>()==null){
				thisObj.AddComponent<SphereCollider>();
			}
		}
		
		public override void Start() {
			base.Start();
		}
		
		//parent unit is for unit which is spawned from destroyed unit
		public void Init(PathTD p, int ID, int wID, UnitCreep parentUnit=null){
			Init();
			
			path=p;
			instanceID=ID;
			waveID=wID;
			
			float dynamicX=Random.Range(-path.dynamicOffset, path.dynamicOffset);
			float dynamicZ=Random.Range(-path.dynamicOffset, path.dynamicOffset);
			pathDynamicOffset=new Vector3(dynamicX, 0, dynamicZ);
			thisT.position+=pathDynamicOffset;
			
			if(parentUnit==null){
				waypointID=1;
				subWaypointID=0;
				SetSubPath(path.GetWPSectionPath(waypointID));
			}
			else{
				//inherit stats and path from parent unit
				waypointID=parentUnit.waypointID;
				subWaypointID=parentUnit.subWaypointID;
				SetSubPath(parentUnit.subPath);
				
				defaultHP=parentUnit.defaultHP*parentUnit.spawnUnitHPMultiplier;
				HP=GetFullHP(); 	
				
				defaultShield=parentUnit.defaultShield*parentUnit.spawnUnitHPMultiplier;
				shield=GetFullShield();
			}
			
			distFromDestination=CalculateDistFromDestination();
			
			if(type==_CreepType.Offense){
				maskTarget=1<<TDTK.GetLayerTower();
				StartCoroutine(ScanForTargetRoutine());
				StartCoroutine(TurretRoutine());
			}
			if(type==_CreepType.Support){
				LayerMask mask1=1<<TDTK.GetLayerCreep();
				LayerMask mask2=1<<TDTK.GetLayerCreepF();
				maskTarget=mask1 | mask2;
				
				StartCoroutine(SupportRoutine());
			}
			
			int rscCount=ResourceManager.GetResourceCount();
			while(valueRscMin.Count<rscCount) valueRscMin.Add(0);
			while(valueRscMin.Count>rscCount) valueRscMin.RemoveAt(valueRscMin.Count-1);
			while(valueRscMax.Count<rscCount) valueRscMax.Add(0);
			while(valueRscMax.Count>rscCount) valueRscMax.RemoveAt(valueRscMax.Count-1);
			
			PlayAnimSpawn();
		}
		
		
		
		
		public override void Update() {
			base.Update();
		}
		
		public override void OnEnable(){
			SubPath.onPathChangedE += OnSubPathChanged;
		}
		public override void OnDisable(){
			SubPath.onPathChangedE -= OnSubPathChanged;
		}
		
		
		
		
		void OnSubPathChanged(SubPath platformSubPath){
			if(flying) return;
			if(platformSubPath.parentPath==path && platformSubPath.wpIDPlatform==waypointID){
				ResetSubPath(platformSubPath);
			}
		}
		private static Transform dummyT;
		void ResetSubPath(SubPath platformSubPath){
			if(dummyT==null) dummyT=new GameObject().transform;
			
			Quaternion rot=Quaternion.LookRotation(subPath[subWaypointID]-thisT.position);
			dummyT.rotation=rot;
			dummyT.position=thisT.position;
			
			Vector3 pos=dummyT.TransformPoint(0, 0, BuildManager.GetGridSize()/2);
			NodeTD startN=PathFinder.GetNearestNode(pos, platformSubPath.parentPlatform.GetNodeGraph());
			PathFinder.GetPath(startN, platformSubPath.endN, platformSubPath.parentPlatform.GetNodeGraph(), this.PathFinderCallback);
		}
		void PathFinderCallback(List<Vector3> pathList){
			SetSubPath(pathList);
			subWaypointID=0;
			distFromDestination=CalculateDistFromDestination();
		}
		void SetSubPath(List<Vector3> pathList){
			subPath=pathList;
			if(flying && subPath.Count>1){
				subPath=new List<Vector3>{ subPath[0], subPath[subPath.Count-1] };
				subWaypointID=Mathf.Min(subWaypointID, subPath.Count-1);
			}
		}
		
		public override void FixedUpdate(){
			base.FixedUpdate();
			
			if(!stunned && !destroyed){
				
				if(MoveToPoint(subPath[subWaypointID])){
					subWaypointID+=1;
					if(subWaypointID>=subPath.Count){
						subWaypointID=0;
						waypointID+=1;
						if(waypointID>=path.GetPathWPCount()){
							ReachDestination();
						}
						else{
							SetSubPath(path.GetWPSectionPath(waypointID));
						}
					}
				}
				
				/*
				if(!path.IsLinearPath()){
					if(MoveToPoint(subPath[subWaypointID])){
						subWaypointID+=1;
						if(subWaypointID>=subPath.Count){
							subWaypointID=0;
							waypointID+=1;
							if(waypointID>=path.GetPathWPCount()){
								ReachDestination();
							}
							else{
								subPath=path.GetWPSectionPath(waypointID);
							}
						}
					}
				}
				else{
					if(MoveToPoint(path.GetWaypointPos(waypointID))){//path.wpList[waypointID].position)){
						waypointID+=1;
						//~ if(waypointID>=path.wpList.Count){
						if(path.ReachEndOfPath(waypointID)){
							ReachDestination();
						}
					}
				}
				*/
			}
		}
		
		
		
		//function call to rotate and move toward a pecific point, return true when the point is reached
		//private bool moved=false;
		public bool MoveToPoint(Vector3 point){
			//moved=false;
			
			if(type==_CreepType.Offense && stopToAttack && target!=null) return false;
			
			//this is for dynamic waypoint, each unit creep have it's own offset pos
			//point+=dynamicOffset;
			point+=pathDynamicOffset;//+flightHeightOffset;
			
			float dist=Vector3.Distance(point, thisT.position);
			
			//if the unit have reached the point specified
			//~ if(dist<0.15f) return true;
			if(dist<0.005f) return true;
			
			//rotate towards destination
			if(rotateTowardsDestination && moveSpeed>0){
				if(rotateTowardsDestinationX){
					Quaternion wantedRot=Quaternion.LookRotation(point-thisT.position);
					thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, rotateSpd*Time.fixedDeltaTime);
				}
				else{
					Vector3 destPos=new Vector3(point.x, thisT.position.y, point.z);
					Quaternion wantedRot=Quaternion.LookRotation(destPos-thisT.position);
					thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, rotateSpd*Time.fixedDeltaTime);
				}
			}
			
			//move, with speed take distance into accrount so the unit wont over shoot
			Vector3 dir=(point-thisT.position).normalized;
			thisT.Translate(dir*Mathf.Min(dist, moveSpeed * slowMultiplier * Time.fixedDeltaTime), Space.World);
			
			distFromDestination-=(moveSpeed * slowMultiplier * Time.fixedDeltaTime);
			
			//moved=true;
			
			return false;
		}
		
		
		void LateUpdate(){
			//PlayAnimMove(moved ? moveSpeed * slowMultiplier : 0);
			PlayAnimMove(moveSpeed * slowMultiplier);
		}
		
		
		void ReachDestination(){
			//Debug.Log("ReachDestination");
			
			if(destinationEffObj!=null){
				if(!autoDestroydestinationEff) ObjectPoolManager.Spawn(destinationEffObj, thisT.position, thisT.rotation);
				else ObjectPoolManager.Spawn(destinationEffObj, thisT.position, thisT.rotation, destinationEffDuration);
			}
			
			GameControl.OnCreepReachDestination(this);
			SpawnManager.OnCreepReachDestination(this);
			TDTK.OnCreepDestination(this);
			
			if(path.loop){
				if(!path.IsLinearPath()){
					//if(onDestinationE!=null) onDestinationE(this);
					subWaypointID=0;
					waypointID=path.GetLoopPoint();
					SetSubPath(path.GetWPSectionPath(waypointID));
				}
				else{
					waypointID=path.GetLoopPoint();
				}
				return;
			}
			
			destroyed=true;
			
			//if(onDestinationE!=null) onDestinationE(this);
			
			//float delay=0;
			//if(aniInstance!=null){ delay=aniInstance.PlayDestination(); }
			
			StartCoroutine(_ReachDestination(PlayAnimDestination()));
		}
		IEnumerator _ReachDestination(float duration){
			yield return new WaitForSeconds(duration);
			ObjectPoolManager.Unspawn(thisObj);
		}
		
		
		public override void Destroyed(float delay=0){
			if(destroyed) return;
			
			destroyed=true;
			
			List<int> rscGain=new List<int>();
			for(int i=0; i<valueRscMin.Count; i++){
				rscGain.Add(Random.Range(valueRscMin[i], valueRscMax[i]));
			}
			ResourceManager.GainResource(rscGain, PerkManager.GetRscCreepKilled());
			
			AbilityManager.GainEnergy(valueEnergyGain+(int)PerkManager.GetEnergyWaveClearedModifier());
			if(lifeValue>0) GameControl.GainLife(lifeValue);
			
			if(spawnUponDestroyed!=null && spawnUponDestroyedCount>0){
				for(int i=0; i<spawnUponDestroyedCount; i++){
					Vector3 posOffset=new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
					GameObject obj=ObjectPoolManager.Spawn(spawnUponDestroyed.gameObject, thisT.position+posOffset, thisT.rotation);
					UnitCreep unit=obj.GetComponent<UnitCreep>();
					
					unit.waveID=waveID;
					int ID=SpawnManager.AddDestroyedSpawn(unit);
					unit.Init(path, ID, waveID, this);
				}
			}
			
			SpawnManager.OnUnitDestroyed(this);
			TDTK.OnUnitCreepDestroyed(this);
			
			base.Destroyed(PlayAnimDestroyed());
		}
		
		
		
		
		public float GetMoveSpeed(){ return moveSpeed * slowMultiplier; }
		
		private float distFromDestination=0;
		public override float GetDistFromDestination(){ return distFromDestination; }
		public float CalculateDistFromDestination(){
			float dist=Vector3.Distance(thisT.position, subPath[subWaypointID]);
			for(int i=subWaypointID+1; i<subPath.Count; i++){
				dist+=Vector3.Distance(subPath[i-1], subPath[i]);
			}
			dist+=path.GetPathDistance(waypointID+1);
			return dist;
		}
		
		
		
		/*
		//for unity-navmesh based navigation, not in used (reason: requird navmesh carving - pro only feature)
		private NavMeshAgent agent;
		private Vector3 tgtPos;
		public void InitNavMesh(PathTD p, int ID, int wID){
			Init();
			
			path=p;
			instanceID=ID;
			waveID=wID;
			
			tgtPos=path.wpList[path.wpList.Count-1].position;
			agent = thisObj.GetComponent<NavMeshAgent>();
			agent.SetDestination(tgtPos);
			
			StartCoroutine(CheckDestination());
		}
		IEnumerator CheckDestination(){
			while(!stunned && !dead){
				tgtPos.y=thisT.pos.y;
				if(Vector3.Distance(tgtPos, thisT.position)<0.5f) ReachDestination();
				yield return null;
			}
		}
		*/
		
		
		/*
		//check if the creep will turn at the next waypoint, return true if yes, false if otherwise
		bool TurningOnNextWaypoint(){
			Vector3 nextPoint=Vector3.zero;
			
			//if we are not at the end of current subpath
			if(subWaypointID<subPath.Count-1){
				//get next point inc current subpath
				nextPoint=subPath[subWaypointID+1];
			}
			else{
				if(waypointID<path.GetPathWPCount()-1){
					//get the first point of next subpath
					List<Vector3> nextSubPath=	path.GetWPSectionPath(waypointID+1);
					nextPoint=nextSubPath[0];
				}
				else return false;	//end of path, no more turning
			}
			
			//current moving dir
			Vector3 dir1=(subPath[subWaypointID]-thisT.position).normalized;
			//next waypoint moving dir
			Vector3 dir2=(nextSubPath[0]-subPath[subWaypointID]).normalized;
			
			if(dir1==dir2) return false;
			
			return true;
		}
		*/
	}
	
}
