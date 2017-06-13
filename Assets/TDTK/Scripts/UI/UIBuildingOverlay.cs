using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIBuildingOverlay : MonoBehaviour {

		public List<Slider> buildingBarList=new List<Slider>();
		
		private static UIBuildingOverlay instance;
		public static UIBuildingOverlay GetInstance(){ return instance; }
		
		
		void Awake() {
			instance=this;
			
			for(int i=0; i<20; i++){
				if(i>0){
					GameObject newObj=UI.Clone(buildingBarList[0].gameObject);
					buildingBarList.Add(newObj.GetComponent<Slider>());
				}
				buildingBarList[i].gameObject.SetActive(false);
			}
		}
		
		
		void OnEnable(){
			TDTK.onTowerConstructingE += OnShowBar;
			//TDTK.onTowerConstructedE += OnUnitDamaged;
			TDTK.onTowerUpgradingE += OnShowBar;
			//TDTK.onTowerUpgradedE += OnUnitDamaged;
		}
		void OnDisable(){
			TDTK.onTowerConstructingE -= OnShowBar;
			//TDTK.onTowerConstructedE += OnUnitDamaged;
			TDTK.onTowerUpgradingE -= OnShowBar;
			//TDTK.onTowerUpgradedE += OnUnitDamaged;
		}
		
		
		public void OnShowBar(UnitTower tower){ StartCoroutine(BuildingBarRoutine(tower)); }
		
		public IEnumerator BuildingBarRoutine(UnitTower tower){
			Slider bar=buildingBarList[GetUnusedBuildingBarIndex()];
			Transform barT=bar.transform;
			bar.gameObject.SetActive(true);
			
			while(tower!=null && tower.IsInConstruction()){
				bar.value=tower.GetBuildProgress();
				
				Vector3 screenPos = Camera.main.WorldToScreenPoint(tower.thisT.position+new Vector3(0, 0, 0));
				barT.localPosition=(screenPos+new Vector3(0, -20, 0))*UIMainControl.GetScaleFactor();
				
				yield return null;
			}
			
			bar.gameObject.SetActive(false);
		}
		
		
		private int GetUnusedBuildingBarIndex(){
			for(int i=0; i<buildingBarList.Count; i++){
				if(buildingBarList[i].gameObject.activeInHierarchy) continue;
				return i;
			}
			
			GameObject newObj=UI.Clone(buildingBarList[0].gameObject);
			buildingBarList.Add(newObj.GetComponent<Slider>());
			return buildingBarList.Count-1;
		}
		
	}

}