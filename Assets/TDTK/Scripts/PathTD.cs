using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	//a special struct contains all the info for a waypoint
	public class WPSection{
		public Transform waypointT;
		
		private List<Vector3> posList=new List<Vector3>();
		public List<Vector3> GetPosList(){ 
			if(!isPlatform) return new List<Vector3>{ waypointT.position };
			return posList;
		}
		public void SetPosList(List<Vector3> list){ posList=list; }
		public Vector3 GetStartPos(){
			if(isPlatform)
				return platform.GetSubPathPath(pathIDOnPlatform)[0];
			return posList[0];
		}
		public Vector3 GetEndPos(){ 
			if(isPlatform)
				return platform.GetSubPathPath(pathIDOnPlatform)[platform.GetSubPathPath(pathIDOnPlatform).Count-1];
			return posList[posList.Count-1];
		}
		
		
		public bool isPlatform=false;
		//followng memebers are only used when the section is on platform
		public PlatformTD platform;
		public int pathIDOnPlatform=0;	//in case there are more than 1 path crossing the platform, 
													//this ID will be used to identity which sub-path on the platform belong to which path
		//public bool subPathFound=false;	//indicate if the subpath on the platform has been found
		
		
		public WPSection(Transform wpT){ waypointT=wpT; }//	position=wpT.position; }
	}
	
	public class PathTD : MonoBehaviour {
		
		[HideInInspector] private bool isLinearPath=true;
		public bool IsLinearPath(){ return isLinearPath; }
		
		public List<Transform> wpList=new List<Transform>();
		public List<WPSection> wpSectionList=new List<WPSection>();	//construct from wpList, each waypoint has each own WPSection
		
		public bool createPathLine=true;
		
		public float dynamicOffset=1;
		
		public bool loop=false;
		public int loopPoint=0;		
		
		
		public void Init(){
			wpSectionList=new List<WPSection>();
			
			for(int i=0; i<wpList.Count; i++){
				Transform wpT=wpList[i];
				
				//check if this is a platform, BuildManager would have add the component and have them layered
				if(wpT!=null){
					WPSection section=new WPSection(wpT);
					
					if(wpT.gameObject.layer==TDTK.GetLayerPlatform()){
						section.isPlatform=true;
						section.platform=wpT.gameObject.GetComponent<PlatformTD>();
						section.pathIDOnPlatform=section.platform.AddSubPath(this, i, wpList[i-1], wpList[i+1]);
						
						if(isLinearPath) isLinearPath=false;
					}
					else{
						WPSubPath wpSubPath=wpT.gameObject.GetComponent<WPSubPath>();
						if(wpSubPath!=null) section.SetPosList( new List<Vector3>(wpSubPath.posList) );
						else section.SetPosList( new List<Vector3>{ wpT.position } );
					}
					
					wpSectionList.Add(section);
				}
				else{
					wpList.RemoveAt(i);
					i-=1;
				}
			}
			
			
			if(loop){
				loopPoint=Mathf.Min(wpList.Count-1, loopPoint); //looping must be 1 waypoint before the destination
			}
			
		}
		
		
		public List<Vector3> GetWPSectionPath(int ID){
			if(wpSectionList[ID].isPlatform){ 
				WPSection section=wpSectionList[ID];
				return section.platform.GetSubPathPath(section.pathIDOnPlatform);
			}
			
			return new List<Vector3>( wpSectionList[ID].GetPosList() );
		}
		
		
		public int GetPathWPCount(){ return wpList.Count; }
		public Vector3 GetSpawnPoint(){ return wpSectionList[0].GetStartPos(); }
		public Quaternion GetSpawnDirection(){ return wpSectionList[0].waypointT.rotation; }
		
		//public int GetPathWPCount(){ return !usePointList ? wpList.Count : pointList.Count; }
		//public Vector3 GetSpawnPoint(){ return !usePointList ? wpList[0].position : pointList[0]; }
		//public Quaternion GetSpawnDirection(){ return !usePointList ? wpList[0].rotation : Quaternion.LookRotation(pointList[1]-pointList[0]); }
		
		//called by UnitCreep when using linear path
		//public Vector3 GetWaypointPos(int ID){	return wpSectionList[ID].GetPosition(); }
		
		public bool ReachEndOfPath(int ID){	return ID>=wpSectionList.Count; }
		
		public int GetLoopPoint(){ return loopPoint; }
		
		
		public float GetPathDistance(int wpID=1){
			if(wpList.Count==0) return 0;
			
			float totalDistance=0;
			
			if(Application.isPlaying){
				Vector3 lastPoint=wpSectionList[wpID-1].GetEndPos();
				for(int i=wpID; i<wpSectionList.Count; i++){
					//List<Vector3> subPointList=null;
					//if(!wpSectionList[i].isPlatform) subPointList=wpSectionList[i].GetPosList();
					//else subPointList=GetWPSectionPath(i);
					List<Vector3> subPointList=GetWPSectionPath(i);
					
					totalDistance+=Vector3.Distance(lastPoint, subPointList[0]);
					for(int n=1; n<subPointList.Count; n++)
						totalDistance+=Vector3.Distance(subPointList[n-1], subPointList[n]);
					lastPoint=subPointList[subPointList.Count-1];
				}
			}
			else{
				for(int i=wpID; i<wpList.Count; i++) totalDistance+=Vector3.Distance(wpList[i-1].position, wpList[i].position);
			}
			
			return totalDistance;
		}
		
		
		
		
		
		void Start(){
			if(createPathLine) CreatePathLine();
		}
		void CreatePathLine(){
			
			Transform parentT=new GameObject().transform;
			parentT.position=transform.position;
			parentT.parent=transform;
			parentT.gameObject.name="PathLine";
			
			GameObject pathLine=(GameObject)Resources.Load("ScenePrefab/PathLine");
			GameObject pathPoint=(GameObject)Resources.Load("ScenePrefab/PathPoint");
			
			Vector3 startPoint=Vector3.zero;
			Vector3 endPoint=Vector3.zero;
			
			SubPath subP=null;
			
			for(int i=0; i<wpSectionList.Count; i++){
				WPSection wpSec=wpSectionList[i];
				if(!wpSec.isPlatform){
					List<Vector3> posList=wpSec.GetPosList();
					for(int n=0; n<posList.Count-1; n++){
						GameObject lineObj=(GameObject)Instantiate(pathLine, posList[n], Quaternion.identity);
						LineRenderer lineRen=lineObj.GetComponent<LineRenderer>();
						lineRen.SetPosition(0, posList[n]);
						lineRen.SetPosition(1, posList[n+1]);
						
						lineObj.transform.parent=parentT;
					}
					
					endPoint=wpSec.GetStartPos();
				}
				else{
					subP=wpSec.platform.GetSubPath(wpSec.pathIDOnPlatform);
					GameObject point1Obj=(GameObject)Instantiate(pathPoint, subP.startN.pos, Quaternion.identity);
					GameObject point2Obj=(GameObject)Instantiate(pathPoint, subP.endN.pos, Quaternion.identity);
					endPoint=subP.startN.pos;
					
					point1Obj.transform.parent=parentT;
					point2Obj.transform.parent=parentT;
				}
				
				if(i>0){
					GameObject lineObj=(GameObject)Instantiate(pathLine, startPoint, Quaternion.identity);
					LineRenderer lineRen=lineObj.GetComponent<LineRenderer>();
					lineRen.SetPosition(0, startPoint);
					lineRen.SetPosition(1, endPoint);
					
					lineObj.transform.parent=parentT;
				}
				
				if(wpSec.isPlatform) startPoint=subP.endN.pos;
				else startPoint=wpSec.GetEndPos();
			}
		}
		
		
		
		
		
		public bool showGizmo=true;
		public Color gizmoColor=Color.blue;
		void OnDrawGizmos(){
			if(showGizmo){
				Gizmos.color = gizmoColor;
				
				if(Application.isPlaying){
					List<Vector3> firstSubPath=GetWPSectionPath(0);
					for(int n=1; n<firstSubPath.Count; n++) Gizmos.DrawLine(firstSubPath[n-1], firstSubPath[n]);
					
					for(int i=1; i<wpSectionList.Count; i++){
						List<Vector3> subPathO=GetWPSectionPath(i-1);
						List<Vector3> subPath=GetWPSectionPath(i);
						
						Gizmos.DrawLine(subPathO[subPathO.Count-1], subPath[0]);
						for(int n=1; n<subPath.Count; n++){
							Gizmos.DrawLine(subPath[n-1], subPath[n]);
						}
					}
				}
				else{
					for(int i=0; i<wpList.Count-1; i++){
						if(wpList[i]!=null && wpList[i+1]!=null){
							WPSubPath wpSubPath1=wpList[i].gameObject.GetComponent<WPSubPath>();
							WPSubPath wpSubPath2=wpList[i+1].gameObject.GetComponent<WPSubPath>();
							
							if(wpSubPath1!=null && wpSubPath2!=null){
								for(int n=0; n<wpSubPath1.posList.Count-1; n++){
									Gizmos.DrawLine(wpSubPath1.posList[n], wpSubPath1.posList[n+1]);
								}
								Gizmos.DrawLine(wpSubPath1.posList[wpSubPath1.posList.Count-1], wpSubPath2.posList[0]);
							}
							else if(wpSubPath1!=null && wpSubPath2==null){
								for(int n=0; n<wpSubPath1.posList.Count-1; n++){
									Gizmos.DrawLine(wpSubPath1.posList[n], wpSubPath1.posList[n+1]);
								}
								Gizmos.DrawLine(wpSubPath1.posList[wpSubPath1.posList.Count-1], wpList[i+1].position);
							}
							else if(wpSubPath1==null && wpSubPath2!=null){
								Gizmos.DrawLine(wpList[i].position, wpSubPath2.posList[0]);
							}
							else Gizmos.DrawLine(wpList[i].position, wpList[i+1].position);
						}
					}
				}
			}
		}
		
	}
	
	
	
	
}



