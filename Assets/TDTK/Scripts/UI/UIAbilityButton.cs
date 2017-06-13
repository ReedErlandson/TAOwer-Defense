using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIAbilityButton : MonoBehaviour {
		
		public Slider energyBar;
		public Text lbEnergy;
		
		public GameObject lbSelectTargetObj;
		
		
		[Header("Buttons")]
		public List<UIButton> buttonList=new List<UIButton>();	//all button list
		public GameObject butCancelObj;
		private CanvasGroup butCancelCanvasG;
		
		private BuildInfo buildInfo;
		
		[Header("Tooltip")]
		public GameObject tooltipObj;
		private CanvasGroup tooltipCanvasG;
		
		public Text lbTooltipName;
		public Text lbTooltipDesp;
		public UIObject tooltipRscItem;
		
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIAbilityButton instance;
		public static UIAbilityButton GetInstance(){ return instance; }
		
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			tooltipCanvasG=tooltipObj.GetComponent<CanvasGroup>();
			tooltipCanvasG.alpha=0;
			
			rectT.localPosition=new Vector3(0, 0, 0);
		}
		
		void Start(){
			if(!AbilityManager.IsOn() || AbilityManager.GetAbilityCount()==0){
				thisObj.SetActive(false);
				return;
			}
			
			List<Ability> abList=AbilityManager.GetAbilityList();
			
			for(int i=0; i<abList.Count; i++){
				if(i==0) buttonList[0].Init();
				else if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "AbilityButton"+(i+1)));
				
				buttonList[i].imgIcon.sprite=abList[i].icon;
				buttonList[i].imgHighlight.enabled=false;
				buttonList[i].label.text="";
				
				if(abList[i].usedRemained>0) buttonList[i].label.text=abList[i].usedRemained.ToString();
				//else buttonList[i].label.text="∞";
				
				if(UIMainControl.InTouchMode()) buttonList[i].SetCallback(null, null, this.OnAbilityButton, null);
				else buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnAbilityButton, null);
			}
			
			
			tooltipRscItem.Init();
			
			
			butCancelCanvasG=butCancelObj.AddComponent<CanvasGroup>(); 
			OnAbilitySelectingTarget(false);
			
			UIItemCallback cancelCallback=butCancelObj.AddComponent<UIItemCallback>();
			cancelCallback.SetDownCallback(this.OnCancelAbilityButton);
			//cancelCallback.SetUpCallback(up);
			
			tooltipObj.SetActive(false);
		}
		
		
		void OnEnable(){
			TDTK.onAbilityActivatedE += OnAbilityActivated;
			TDTK.onNewAbilityE += OnNewAbility;
			
			TDTK.onAbilitySelectingTargetE += OnAbilitySelectingTarget;
		}
		void OnDisable(){
			TDTK.onAbilityActivatedE -= OnAbilityActivated;
			TDTK.onNewAbilityE -= OnNewAbility;
			
			TDTK.onAbilitySelectingTargetE -= OnAbilitySelectingTarget;
		}
		
		
		void OnAbilitySelectingTarget(bool flag){
			butCancelCanvasG.alpha=flag ? 1 : 0;
			butCancelCanvasG.interactable=flag;
			
			if(lbSelectTargetObj!=null) lbSelectTargetObj.SetActive(flag);
		}
		
		
		void OnNewAbility(Ability ability){
			buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "AbilityButton"+(buttonList.Count+1)));
			buttonList[buttonList.Count-1].imgIcon.sprite=ability.icon;
			buttonList[buttonList.Count-1].label.text="";
			buttonList[buttonList.Count-1].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnAbilityButton, null);
			
			if(ability.usedRemained>0) buttonList[buttonList.Count-1].label.text=ability.usedRemained.ToString();
		}
		
		
		void OnAbilityActivated(Ability ability){
			StartCoroutine(ButtonCDRoutine(ability));
		}
		IEnumerator ButtonCDRoutine(Ability ability){
			int ID=AbilityManager.GetAbilityIndex(ability);
			
			if(ability.usedRemained>=0) buttonList[ID].label.text=ability.usedRemained.ToString();
			
			buttonList[ID].button.interactable=false;
		
			if(ability.usedRemained==0){ yield break; }
			
			
			while(true){
				string text="";
				float duration=ability.currentCD;
				if(duration<=0) break;
				
				if(duration>60) text=Mathf.Floor(duration/60).ToString("F0")+"m";
				else text=(Mathf.Ceil(duration)).ToString("F0")+"s";
				buttonList[ID].label.text=text;
				
				yield return new WaitForSeconds(0.05f);
			}
			
			buttonList[ID].label.text="";
			buttonList[ID].button.interactable=true;
		}
		
		
		void Update(){
			energyBar.value=AbilityManager.GetEnergy()/AbilityManager.GetEnergyFull();
			lbEnergy.text=AbilityManager.GetEnergy().ToString("f0")+"/"+AbilityManager.GetEnergyFull().ToString("f0");
		}
		
		
		private int currentButtonID=-1; //last touched button, for touch mode only
		public void OnAbilityButton(GameObject butObj, int pointerID=-1){
			int ID=GetButtonID(butObj);
			
			if(UIMainControl.InTouchMode()){
				if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
				if(currentButtonID!=ID){
					//OnAbilitySelectingTarget(true);	//to turn on cancel button
					butCancelCanvasG.alpha=1;
					butCancelCanvasG.interactable=true;
					
					currentButtonID=ID;
					buttonList[ID].imgHighlight.enabled=true;
					OnHoverButton(butObj);
					return;
				}
				ClearTouchModeButton();
			}
			
			string exception=AbilityManager.SelectAbility(ID, pointerID);
			if(exception!="") UIMessage.DisplayMessage(exception);
		}
		
		public void ClearTouchModeButton(){
			OnAbilitySelectingTarget(false);	//to turn off cancel button
			if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
			currentButtonID=-1;
			OnExitButton(null);
		}
		
		
		public void OnHoverButton(GameObject butObj){
			int ID=GetButtonID(butObj);
			ShowTooltip(AbilityManager.GetAbilityList()[ID]);
		}
		public void OnExitButton(GameObject butObj){
			HideTooltip();
		}
		
		private int GetButtonID(GameObject butObj){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		public void OnCancelAbilityButton(GameObject butObj=null, int pointerID=-1){
			AbilityManager.ExitSelectingTargetMode();
			
			ClearTouchModeButton();
		}
		
		
		void ShowTooltip(Ability ability){
			lbTooltipName.text=ability.name;
			lbTooltipDesp.text=ability.GetDesp();
			
			tooltipRscItem.label.text=ability.GetCost().ToString("f0");
			
			//~ List<int> cost=tower.GetCost();
			//~ for(int i=0; i<tooltipRscItemList.Count; i++){
				//~ tooltipRscItemList[i].label.text=cost[i].ToString();
			//~ }
			
			tooltipCanvasG.alpha=1;	tooltipObj.SetActive(true);
		}
		void HideTooltip(){
			tooltipObj.SetActive(false);
		}
		
		
		
		
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			if(!thisObj.activeInHierarchy) return;
			
			rectT.localPosition=new Vector3(0, 0, 0);
			UIMainControl.FadeIn(canvasGroup, 0.25f);
			//thisObj.SetActive(true);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			if(!thisObj.activeInHierarchy) return;
			
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			StartCoroutine(DelayHide());
			//thisObj.SetActive(false);
		}
		
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.25f);
			rectT.localPosition=new Vector3(-5000, -5000, 0);
		}
		
	}

}