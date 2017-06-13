using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDTK;

namespace TDTK {

	public class UIUnitOverlay : MonoBehaviour {
		
		[HideInInspector] public Unit unit;
		
		public Slider sliderHP;
		public Slider sliderShield;
		
		//private Vector2 fullSize;
		
		private GameObject thisObj;
		private RectTransform rectT;
		//private Image image;
		
		void Awake() {
			thisObj=gameObject;
			rectT=gameObject.GetComponent<RectTransform>();
			//image=gameObject.GetComponent<Image>();
			
			//fullSize=rectT.sizeDelta;
		}
		
		// Update is called once per frame
		void LateUpdate () {
			if(unit==null){
				if(thisObj.activeInHierarchy) thisObj.SetActive(false);
				return;
			}
			
			
			if(unit.IsDestroyed() || (unit.HP>=unit.GetFullHP() && unit.shield>=unit.GetFullShield())){
				UIUnitOverlayManager.RemoveUnit(unit);
				unit=null;
				thisObj.SetActive(false);
				return;
			}
			
			
			if(!thisObj.activeInHierarchy) return;
			
			Vector3 screenPos=Camera.main.WorldToScreenPoint(unit.thisT.position+new Vector3(0, 1, 0));
			screenPos.z=0;
			rectT.localPosition=screenPos*UIMainControl.GetScaleFactor(); 
			
			float fullHP=unit.GetFullHP();
			sliderHP.value=fullHP<=0 ? 0: (unit.HP/fullHP);
			
			if(sliderShield.gameObject.activeInHierarchy){
				float fullShield=unit.GetFullShield();
				sliderShield.value=fullShield<=0 ? 0: (unit.shield/fullShield);
			}
		}
		
		
		public void SetUnit(Unit tgtUnit){
			unit=tgtUnit;
			sliderShield.gameObject.SetActive(unit.GetFullShield()<=0 ? false : true);
		}
		
	}

}