using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIUnitOverlayManager : MonoBehaviour {
		
		public List<Unit> unitList=new List<Unit>();
		public List<UIUnitOverlay> unitOverlayList=new List<UIUnitOverlay>();
		
		private static UIUnitOverlayManager instance;
		public static UIUnitOverlayManager GetInstance(){ return instance; }
		
		
		void Awake() {
			instance=this;
			
			for(int i=0; i<20; i++){
				if(i>0){
					GameObject newObj=UI.Clone(unitOverlayList[0].gameObject);
					unitOverlayList.Add(newObj.GetComponent<UIUnitOverlay>());
				}
				unitOverlayList[i].gameObject.SetActive(false);
			}
		}
		
		void Start(){
			if(!UIMainControl.EnableHPOverlay()) gameObject.SetActive(false);
		}
		
		
		void OnEnable(){
			TDTK.onUnitDamagedE += OnUnitDamaged;
			
			//~ TDTK.onNewUnitE += NewUnit;
			//~ TDTK.onCreepDestroyedE += OnUnitDestroyed;
			//~ TDTK.onTowerDestroyedE += OnUnitDestroyed;
		}
		void OnDisable(){
			TDTK.onUnitDamagedE += OnUnitDamaged;
			
			//~ TDTK.onNewUnitE -= NewUnit;
			//~ TDTK.onCreepDestroyedE -= OnUnitDestroyed;
			//~ TDTK.onTowerDestroyedE -= OnUnitDestroyed;
		}
		
		
		public static void OnUnitDamaged(Unit unit){ instance._OnUnitDamaged(unit); }
		public void _OnUnitDamaged(Unit unit){
			if(!UIMainControl.EnableHPOverlay()) return;
			
			if(unitList.Contains(unit)) return;
			
			unitList.Add(unit);
			
			int index=GetUnusedUnitOverlayIndex();
			
			unitOverlayList[index].SetUnit(unit);
			unitOverlayList[index].gameObject.SetActive(true);
		}
		
		private int GetUnusedUnitOverlayIndex(){
			for(int i=0; i<unitOverlayList.Count; i++){
				if(unitOverlayList[i].unit!=null) continue;
				return i;
			}
			
			GameObject newObj=UI.Clone(unitOverlayList[0].gameObject);
			unitOverlayList.Add(newObj.GetComponent<UIUnitOverlay>());
			return unitOverlayList.Count-1;
		}
		
		public static void RemoveUnit(Unit unit){
			instance.unitList.Remove(unit);
		}
		
	}

}