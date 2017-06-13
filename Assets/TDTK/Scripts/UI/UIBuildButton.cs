using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIBuildButton : MonoBehaviour {
		
		[Header("Buttons")]
		public HorizontalLayoutGroup layoutGroup;
		
		//private Transform buttonParent;
		//private Vector3 buttonParentDefaultPos;
		
		public List<UIButton> buttonList=new List<UIButton>();	//all button list
		private List<UIButton> activeButtonList=new List<UIButton>();	//current active button list, subject to tower availability on platform
		
		public GameObject butCancelObj;
		private CanvasGroup butCancelCanvasG;
		
		private BuildInfo buildInfo;
		
		
		[Header("Tooltip")]
		public GameObject tooltipObj;
		private CanvasGroup tooltipCanvasG;
		
		public Text lbTooltipName;
		public Text lbTooltipDesp;
		public List<UIObject> tooltipRscItemList=new List<UIObject>();
		
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIBuildButton instance;
		public static UIBuildButton GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			tooltipCanvasG=tooltipObj.GetComponent<CanvasGroup>();
			tooltipCanvasG.alpha=0;
			
			rectT=thisObj.GetComponent<RectTransform>();
		}
		
		void Start(){
			if(BuildManager.GetInstance()==null) return;
			
			List<UnitTower> towerList=BuildManager.GetTowerList();
			for(int i=0; i<towerList.Count; i++){
				if(i==0) buttonList[0].Init();
				else if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "BuildButton"+(i+1)));
				
				buttonList[i].imgIcon.sprite=towerList[i].iconSprite;
				buttonList[i].imgHighlight.enabled=false;
				
				if(UIMainControl.InTouchMode()) buttonList[i].SetCallback(null, null, this.OnTowerButton, null);
				else buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnTowerButton, null);
			}
			
			if(!BuildManager.UseDragNDrop()){
				canvasGroup.alpha=0;
				rectT.localPosition=new Vector3(0, 99999, 0);
			}
			
			if(!BuildManager.UseDragNDrop() && UIMainControl.UsePieMenu()){
				layoutGroup.enabled=false;
				tooltipObj.transform.localPosition-=new Vector3(0, 60, 0);
			}
			else layoutGroup.enabled=true;
			
			
			List<Rsc> rscList=ResourceManager.GetResourceList();
			for(int i=0; i<rscList.Count; i++){
				if(i==0) tooltipRscItemList[0].Init();
				else tooltipRscItemList.Add(UIObject.Clone(tooltipRscItemList[0].rootObj, "Rsc"+(i+1)));
				
				tooltipRscItemList[i].imgRoot.sprite=rscList[i].icon;
				tooltipRscItemList[i].label.text=rscList[i].value.ToString();
			}
			
			if(!BuildManager.UseDragNDrop()) butCancelObj.SetActive(false);
			else{
				butCancelCanvasG=butCancelObj.AddComponent<CanvasGroup>();
				butCancelObj.transform.SetAsLastSibling();
				OnDragNDrop(false);
			}
			
			tooltipObj.SetActive(false);
		}
		
		void OnNewBuildableTower(UnitTower tower){
			buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "BuildButton"+(buttonList.Count+1)));
			buttonList[buttonList.Count-1].imgIcon.sprite=tower.iconSprite;
			buttonList[buttonList.Count-1].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnTowerButton, null);
			butCancelObj.transform.SetAsLastSibling();
		}
		
		
		void OnEnable(){
			TDTK.onNewBuildableTowerE += OnNewBuildableTower;
			TDTK.onDragNDropE += OnDragNDrop;
		}
		void OnDisable(){
			TDTK.onNewBuildableTowerE -= OnNewBuildableTower;
			TDTK.onDragNDropE -= OnDragNDrop;
		}
		
		
		void OnDragNDrop(bool flag){
			if(!BuildManager.UseDragNDrop()) return;
			butCancelCanvasG.alpha=flag ? 1 : 0;
			butCancelCanvasG.interactable=flag;
		}
		
		
		
		void Update(){
			if(BuildManager.UseDragNDrop() || !UIMainControl.UsePieMenu()) return;
			
			if(buildInfo==null) return;
			
			Vector3 screenPos = Camera.main.WorldToScreenPoint(buildInfo.position)*UIMainControl.GetScaleFactor();
			List<Vector3> pos=GetPieMenuPos(activeButtonList.Count, screenPos, 120, 50);
			
			for(int i=0; i<activeButtonList.Count; i++){
				if(i<pos.Count){
					activeButtonList[i].rectT.localPosition=pos[i];
				}
				else{
					activeButtonList[i].rectT.localPosition=new Vector3(0, 9999, 0);
				}
			}
		}
		
		
		private int currentButtonID=-1; //last touched button, for touch mode only
		public void OnTowerButton(GameObject butObj, int pointerID=-1){
			int ID=GetButtonID(butObj);
			
			if(UIMainControl.InTouchMode() && !BuildManager.UseDragNDrop()){
				if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
				if(currentButtonID!=ID){
					currentButtonID=ID;
					buttonList[ID].imgHighlight.enabled=true;
					OnHoverButton(butObj);
					return;
				}
				ClearTouchModeButton();
			}
			
			string exception=BuildManager.BuildTower(ID, buildInfo, pointerID);
			
			if(exception!=""){
				UIMessage.DisplayMessage(exception);
				return;
			}
			
			buildInfo=null;
			
			if(!BuildManager.UseDragNDrop()) Hide();
		}
		
		public void ClearTouchModeButton(){
			if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
			currentButtonID=-1;
			OnExitButton(null);
		}
		
		
		public void OnHoverButton(GameObject butObj){
			ShowTooltip(BuildManager.GetSampleTower(GetButtonID(butObj)));
			
			if(!BuildManager.UseDragNDrop() && buildInfo!=null)
				BuildManager.ShowSampleTower(GetButtonID(butObj), buildInfo);
		}
		public void OnExitButton(GameObject butObj){
			HideTooltip();
			
			if(!BuildManager.UseDragNDrop())
				BuildManager.ClearSampleTower();
		}
		
		int GetButtonID(GameObject butObj){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		
		public void OnCancelBuildButton(){
			BuildManager.ExitDragNDrop();
		}
		
		
		
		void ShowTooltip(UnitTower tower){
			lbTooltipName.text=tower.unitName;
			lbTooltipDesp.text=tower.GetDespGeneral();
			
			List<int> cost=tower.GetCost();
			for(int i=0; i<tooltipRscItemList.Count; i++){
				tooltipRscItemList[i].label.text=cost[i].ToString();
			}
			
			tooltipCanvasG.alpha=1;	tooltipObj.SetActive(true);
			//UIMainControl.FadeIn(tooltipCanvasG, 0.0f, tooltipObj);
		}
		void HideTooltip(){
			tooltipCanvasG.alpha=0;	tooltipObj.SetActive(false);
			//UIMainControl.FadeOut(tooltipCanvasG, 0.0f, tooltipObj);
		}
		
		
		void UpdateActiveBuildButtonList(){
			activeButtonList=new List<UIButton>();
			
			if(buildInfo==null) return;
			
			for(int i=0; i<buildInfo.availableTowerIDList.Count; i++){
				activeButtonList.Add(buttonList[buildInfo.availableTowerIDList[i]]);
			}
			
			for(int i=0; i<buttonList.Count; i++){
				if(activeButtonList.Contains(buttonList[i])) buttonList[i].rootObj.SetActive(true);
				else buttonList[i].rootObj.SetActive(false);
			}
			
			//if there's nothing to show, hide
			if(activeButtonList.Count==0) Hide();
		}
		
		
		
		
		public static void Show(BuildInfo bInfo=null){ instance._Show(bInfo); }
		public void _Show(BuildInfo bInfo=null){
			if(!BuildManager.UseDragNDrop()){
				if(bInfo!=null){
					ClearTouchModeButton();
					buildInfo=bInfo;//BuildManager.GetBuildInfo();
					UpdateActiveBuildButtonList();
				}
				else return;
			}
			
			rectT.localPosition=new Vector3(0, 0, 0);
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			if(UIMainControl.InTouchMode()) ClearTouchModeButton();
			
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.25f);
			rectT.localPosition=new Vector3(0, 99999, 0);
		}
		
		
		
		private Transform piePosDummyT;
		public List<Vector3> GetPieMenuPos(float num, Vector3 screenPos, float cutoff, int size){
			List<Vector3> points=new List<Vector3>();
			
			if(num==1){
				//points.Add(screenPos*UIMainControl.GetScaleFactor()+new Vector3(0, 50, 0));
				//return points;
				
				return new List<Vector3>{ screenPos+new Vector3(0, 50, 0) };
			}
			
			//if there's only two button to be displayed, then normal calculation doesnt apply
			if(num<=2){
				//~ points.Add(screenPos*UIMainControl.GetScaleFactor()+new Vector3(50, 10, 0));
				//~ points.Add(screenPos*UIMainControl.GetScaleFactor()+new Vector3(-50, 10, 0));
				//~ return points;
				
				return new List<Vector3>{ screenPos+new Vector3(50,10,0), screenPos+new Vector3(-50,10,0) };
			}
			
			
			//create a dummy transform which we will use to do the calculation
			if(piePosDummyT==null){
				piePosDummyT=new GameObject().transform;
				piePosDummyT.parent=transform;
				piePosDummyT.name="PiePosDummy";
			}
			
			int cutoffOffset=cutoff>0 ? 1:0;
			
			//calculate the spacing of angle and distance of button from center
			float spacing=(float)((360f-cutoff)/(num-cutoffOffset));
			//float dist=Mathf.Max((num+1)*10, 50);
			float dist=0.35f*num*size;//UIMainControl.GetScaleFactor();
			
			piePosDummyT.rotation=Quaternion.Euler(0, 0, cutoff/2);
			piePosDummyT.position=screenPos;//Vector3.zero;
			
			//rotate the dummy transform using the spacing interval, then sample the end point
			//these end point will be our button position
			for(int i=0; i<num; i++){
				points.Add(piePosDummyT.TransformPoint(new Vector3(0, -dist, 0)));
				piePosDummyT.Rotate(Vector3.forward*spacing);
			}
			
			return points;
		}
		
	}

}