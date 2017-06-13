using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class IndicatorControl : MonoBehaviour {
		
		public bool disableCursorIndicator=false;
		
		
		[Header("Indicator Prefabs")]
		public Transform indicatorSelected;
		public Transform indicatorCursor;
		
		private GameObject indicatorSelectedObj;
		private GameObject indicatorCursorObj;
		
		private Renderer indicatorSelectedRenderer;
		
		private UnitTower currentTower;
		
		
		public Transform rangeIndicator;
		public Transform rangeIndicatorCone;
		private GameObject rangeIndicatorObj;
		private GameObject rangeIndicatorConeObj;

		private static IndicatorControl instance;
		private Transform thisT;
		
		public void Init(){
			instance=this;
			thisT=transform;
			
			float gridSize=BuildManager.GetGridSize();
			
			if(indicatorSelected!=null){
				indicatorSelected=(Transform)Instantiate(indicatorSelected);
				indicatorSelected.localScale=new Vector3(gridSize, gridSize, gridSize);
				indicatorSelected.parent=thisT;
				indicatorSelected.name="TileIndicator_Selected";
				
				indicatorSelectedRenderer=indicatorSelected.GetChild(0).GetComponent<Renderer>();
				
				indicatorSelectedObj=indicatorSelected.gameObject;
				indicatorSelectedObj.SetActive(false);
			}
			
			if(indicatorCursor!=null){
				indicatorCursor=(Transform)Instantiate(indicatorCursor);
				indicatorCursor.localScale=new Vector3(gridSize, gridSize, gridSize);
				indicatorCursor.parent=thisT;
				indicatorCursor.name="TileIndicator_Cursor";
				
				indicatorCursorObj=indicatorCursor.gameObject;
				indicatorCursorObj.SetActive(false);
			}
			
			
			if(rangeIndicator!=null){
				rangeIndicator=(Transform)Instantiate(rangeIndicator);
				rangeIndicator.parent=thisT;
				rangeIndicatorObj=rangeIndicator.gameObject;
			}
			if(rangeIndicatorCone!=null){
				rangeIndicatorCone=(Transform)Instantiate(rangeIndicatorCone);
				rangeIndicatorCone.parent=thisT;
				rangeIndicatorConeObj=rangeIndicatorCone.gameObject;
			}
			
			_ClearRangeIndicator();
			
			
			//~ #if !UNITY_STANDALONE_WIN && UNITY_STANDALONE_LINUX && UNITY_STANDALONE && UNITY_WEBPLAYER
				//~ disableCursorIndicator=true;
			//~ #endif
		}
		
		
		
		
		void Update(){
			if(FPSControl.IsInFPSMode()) return;
			if(AbilityManager.IsSelectingTarget()) return;
			
			//if(!disableCursorIndicator) 
				BuildManager.SetTileIndicator(Input.mousePosition);
		}
		
		
		private bool inDragNDropPhase=false;
		public static void SetDragNDropPhase(bool flag){
			instance.inDragNDropPhase=flag;
			instance.indicatorSelectedObj.SetActive(false);
		}
		
		public static void SetIndicatorCursor(Vector3 pos, Quaternion rot){ 
			if(instance.inDragNDropPhase) return;
			
			instance.indicatorCursor.position=pos;
			instance.indicatorCursor.rotation=rot;
			instance.indicatorCursorObj.SetActive(true);
		}
		public static void ClearIndicatorCursor(){
			instance.indicatorCursorObj.SetActive(false);
		}
		
		
		public static void SetBuildTileIndicator(BuildInfo buildInfo){ instance._SetBuildTileIndicator(buildInfo); }
		public void _SetBuildTileIndicator(BuildInfo buildInfo){
			if(buildInfo.status==_TileStatus.NoPlatform){
				indicatorSelectedObj.SetActive(false);
				return;
			}
			
			if(buildInfo.status==_TileStatus.Available)
				indicatorSelectedRenderer.material.SetColor("_Color", new Color(0, 1, 0, 1));
			else
				indicatorSelectedRenderer.material.SetColor("_Color", new Color(1, 0, 0, 1));
			
			indicatorSelected.position=buildInfo.position;
			if(buildInfo.platform!=null) indicatorSelected.rotation=buildInfo.platform.thisT.rotation;
			indicatorSelectedObj.SetActive(true);
		}
		public static void ClearBuildTileIndicator(){
			instance.indicatorSelectedObj.SetActive(false);
		}
		
		
		public static void ShowTowerRangeIndicator(UnitTower tower){ instance._ShowTowerRangeIndicator(tower); }
		public void _ShowTowerRangeIndicator(UnitTower tower){
			_ClearRangeIndicator();
			
			if(tower.type==_TowerType.Block || tower.type==_TowerType.Resource) return;
			
			_ClearRangeIndicator();
			
			currentTower=tower;
			
			indicatorSelectedRenderer.material.SetColor("_Color", new Color(0, 1, 0, 1));
			indicatorSelected.position=tower.thisT.position;
			indicatorSelected.rotation=tower.thisT.rotation;
			indicatorSelectedObj.SetActive(true);
			
			float range=tower.GetRange();
			Transform indicatorT=!tower.directionalTargeting ? rangeIndicator : rangeIndicatorCone;
			
			if(indicatorT!=null){
				indicatorT.position=tower.thisT.position;
				indicatorT.localScale=new Vector3(2*range, 1, 2*range);
				indicatorT.parent=tower.thisT;
				
				if(tower.directionalTargeting) indicatorT.localRotation=Quaternion.identity*Quaternion.Euler(0, tower.dirScanAngle, 0);
				
				indicatorT.gameObject.SetActive(true);
			}
		}
		
		public static void ClearRangeIndicator(){ instance._ClearRangeIndicator(); }
		public void _ClearRangeIndicator(){
			currentTower=null;
			
			indicatorSelectedObj.SetActive(false);
			
			rangeIndicatorObj.SetActive(false);
			rangeIndicatorConeObj.SetActive(false);
			
			rangeIndicator.parent=thisT;
			rangeIndicatorCone.parent=thisT;
		}
		
		
		public static void TowerScanAngleChanged(UnitTower tower){
			instance.rangeIndicatorCone.localRotation=tower.thisT.localRotation*Quaternion.Euler(0, tower.dirScanAngle, 0);
		}
		
		//called from UnitTower whenever a tower instance is destroyed/sold
		public static void TowerRemoved(UnitTower tower){
			if(instance.currentTower==tower) ClearRangeIndicator();
		}
		
	}

}