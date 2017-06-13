using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIPerkMenu : MonoBehaviour {
		
		//public bool demoMenu=false;
		
		public bool manuallySetupItem=false;
		
		[Space(10)]
		public List<UIPerkItem> perkItemList=new List<UIPerkItem>();
		private int selectID=0;
		
		[Space(10)]
		public Text lbPerkName;
		public Text lbPerkDesp;
		public Text lbPerkReq;
		public List<UIObject> rscItemList=new List<UIObject>();
		private GameObject rscRootObj;
		
		[Space(10)]
		public Text lbPerkPoint;
		public UIButton butPurchase;
		public GameObject butCloseObj;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIPerkMenu instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
		}
		
		void Start(){
			if(!manuallySetupItem){
				List<Perk> perkList=PerkManager.GetPerkList();
				for(int i=0; i<perkList.Count; i++){
					if(i==0) perkItemList[0].Init();
					else if(i>0) perkItemList.Add(UIPerkItem.Clone(perkItemList[0].rootObj, "PerkButton"+(i+1)));
					
					perkItemList[i].imgIcon.sprite=perkList[i].icon;
					perkItemList[i].perkID=perkList[i].ID;
					perkItemList[i].selectHighlight.SetActive(i==0);
					
					perkItemList[i].SetCallback(null, null, this.OnPerkItem, null);
				}
				
				UpdateContentRectSize();
			}
			else{
				for(int i=0; i<perkItemList.Count; i++){
					perkItemList[i].Init();
					perkItemList[i].selectHighlight.SetActive(i==0);
					perkItemList[i].SetCallback(null, null, this.OnPerkItem, null);
				}
			}
			
			
			List<Rsc> rscList=ResourceManager.GetResourceList();
			for(int i=0; i<rscList.Count; i++){
				if(i==0) rscItemList[0].Init();
				else rscItemList.Add(UIObject.Clone(rscItemList[0].rootObj, "Rsc"+(i+1)));
				
				rscItemList[i].imgRoot.sprite=rscList[i].icon;
				rscItemList[i].label.text="";
			}
			rscRootObj=rscItemList[0].rectT.parent.gameObject;
			
			butPurchase.Init();
			
			UpdatePerkItemList();
			UpdateDisplay();
			
			//thisObj.SetActive(false);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			rectT.localPosition=new Vector3(0, 99999, 0);
		}
		
		
		public GridLayoutGroup layoutGroup;
		private void UpdateContentRectSize(){
			int rowCount=(int)Mathf.Ceil(perkItemList.Count/(float)layoutGroup.constraintCount);
			float size=rowCount*layoutGroup.cellSize.y+rowCount*layoutGroup.spacing.y+layoutGroup.padding.top;
			
			RectTransform contentRect=layoutGroup.gameObject.GetComponent<RectTransform>();
			contentRect.sizeDelta=new Vector2(contentRect.sizeDelta.x, size);
		}
		
		
		void Update(){
			if(IsOn() && Input.GetKeyDown(KeyCode.Escape)) OnCloseButton();
		}
		
		
		public void OnPerkItem(GameObject butObj, int pointerID){
			int ID=GetButtonID(butObj);
			
			perkItemList[selectID].selectHighlight.SetActive(false);
			
			selectID=ID;
			
			perkItemList[selectID].selectHighlight.SetActive(true);
			UpdateDisplay();
		}
		
		int GetButtonID(GameObject butObj){
			for(int i=0; i<perkItemList.Count; i++){
				if(perkItemList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		
		void UpdateDisplay(){
			lbPerkPoint.text="Points: "+PerkManager.GetPerkPoint();
			
			Perk perk=PerkManager.GetPerk(perkItemList[selectID].perkID);
			
			lbPerkName.text=perk.name;
			lbPerkDesp.text=perk.desp;
			
			if(perk.purchased){
				lbPerkReq.text="";
				rscRootObj.SetActive(false);
				
				butPurchase.label.text="Purchased";
				butPurchase.button.interactable=false;
				return;
			}
			
			butPurchase.label.text="Purchase";
			
			string text=perk.IsAvailable();
			if(text==""){
				List<int> cost=perk.GetCost();
				for(int i=0; i<rscItemList.Count; i++) rscItemList[i].label.text=cost[i].ToString();
				
				lbPerkReq.text="";
				rscRootObj.SetActive(true);
				butPurchase.button.interactable=true;
			}
			else{
				lbPerkReq.text=text;
				rscRootObj.SetActive(false);
				butPurchase.button.interactable=false;
			}
		}
		
		void UpdatePerkItemList(){
			for(int i=0; i<perkItemList.Count; i++){
				bool purchased=PerkManager.IsPerkPurchased(perkItemList[i].perkID);
				bool available=PerkManager.IsPerkAvailable(perkItemList[i].perkID)=="";
				perkItemList[i].purchasedHighlight.SetActive(purchased);
				perkItemList[i].unavailableHighlight.SetActive(!(purchased || available));
				if(perkItemList[i].connector!=null) perkItemList[i].connector.SetActive(purchased);
			}
		}
		
		
		
		public void OnPurchaseButton(){
			//Perk perk=PerkManager.GetPerk(perkItemList[selectID].perkID);
			
			string text=PerkManager.PurchasePerk(perkItemList[selectID].perkID);
				
			if(text!=""){
				UIMessage.DisplayMessage(text);
				return;
			}
			
			UpdatePerkItemList();
			
			UpdateDisplay();
		}
		
		
		public void OnCloseButton(){
			UIMainControl.ClosePerkMenu();
		}
		public static void DisableCloseButton(){
			instance.butCloseObj.SetActive(false);
		}
		
		
		private bool isOn=false;
		public static bool IsOn(){ return instance==null ? false : instance.isOn; }
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			UpdatePerkItemList();
			UpdateDisplay();
			
			isOn=true;
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			rectT.localPosition=Vector3.zero;
			UIMainControl.FadeIn(canvasGroup, 0.25f);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.25f);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			isOn=false;
			rectT.localPosition=new Vector3(-5000, -5000, 0);
		}
		
	}

}