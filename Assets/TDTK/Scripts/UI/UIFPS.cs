using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDTK;

namespace TDTK {

	public class UIFPS : MonoBehaviour {

		public bool touchMode=false;
		public GameObject buttonGroupObj;
		public GameObject buttonFireObj;
		public UIButton buttonExit;
		
		[Space(10)]
		public Text lbReloading;
		public UIObject fpsItem;
		
		public GameObject recticleObj;
		public RectTransform recticleSpreadRectT;
		private Vector2 recticleSpreadDefaultSize;
		
		public Image imgReloadProgress;
		private GameObject reloadProgressObj;
		
		
		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIFPS instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			thisObj.SetActive(false);
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
			
			fpsItem.Init();
			recticleSpreadDefaultSize=recticleSpreadRectT.sizeDelta;
			
			reloadProgressObj=imgReloadProgress.transform.parent.gameObject;
			reloadProgressObj.SetActive(false);
			
			UIItemCallback itemCallback=buttonFireObj.AddComponent<UIItemCallback>();
			itemCallback.SetDownCallback(this.OnFireButtonDown);
			itemCallback.SetUpCallback(this.OnFireButtonUp);
			
			buttonExit.Init();
		}
		
		
		void Start () {
			buttonGroupObj.SetActive(touchMode);
		}
		
		
		void Update () {
			float value=FPSControl.GetRecoilModifier();
			value=Mathf.Min(value*20, 250);
			recticleSpreadRectT.sizeDelta=recticleSpreadDefaultSize+new Vector2(value, value)*2;
			
			
			if(reloading) imgReloadProgress.fillAmount=FPSControl.GetReloadProgress();
			
			
			if(!FPSControl.EnableInput()) return;
			
			if(Input.GetKeyDown(KeyCode.R)) FPSControl.Reload();
			
			if(Input.GetKeyDown(KeyCode.Q)) FPSControl.SelectPrevWeapon();
			if(Input.GetKeyDown(KeyCode.E)) FPSControl.SelectNextWeapon();
			
			if(Input.GetAxisRaw("Mouse ScrollWheel")!=0 && scrollCD<=0){
				if(Input.GetAxis("Mouse ScrollWheel")>0) FPSControl.SelectPrevWeapon();
				else FPSControl.SelectNextWeapon();
				scrollCD=0.15f;
			}
			scrollCD-=Time.deltaTime;
			
			if(!touchMode && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))){
				FPSControl.Fire();
			}
			
			if(touchMode && touchFiring){
				FPSControl.Fire();
			}
			
			if(IsOn() && Input.GetKeyDown(KeyCode.Escape)){
				FPSControl.Hide();
			}
		}
		private float scrollCD=0;
		
		
		void OnEnable(){
			TDTK.onFPSShootE += OnShoot;
			TDTK.onFPSReloadE += OnReload;
			TDTK.onFPSSwitchWeaponE += OnSwitchWeapon;
		}
		void OnDisable(){
			TDTK.onFPSShootE -= OnShoot;
			TDTK.onFPSReloadE -= OnReload;
			TDTK.onFPSSwitchWeaponE -= OnSwitchWeapon;
		}
		
		
		void OnShoot(){
			UpdateAmmoCount();
		}
		
		
		private bool reloading=false;
		void OnReload(bool flag){
			reloading=flag;
			if(reloading) StartCoroutine(ReloadRoutine());
			else UpdateAmmoCount();
		}
		IEnumerator ReloadRoutine(){
			recticleObj.SetActive(false);
			lbReloading.text="Reloading";
			int count=0;
			
			reloadProgressObj.SetActive(true);
			
			while(reloading){
				string text="";
				for(int i=0; i<count; i++) text+=".";
				
				lbReloading.text=text+"Reloading"+text;
				
				count+=1;
				if(count==4) count=0;
				yield return new WaitForSeconds(0.25f);
			}
			
			reloadProgressObj.SetActive(false);
			
			lbReloading.text="";
			recticleObj.SetActive(true);
		}
		
		
		void OnSwitchWeapon(){
			UpdateAmmoCount();
			reloading=false;
			
			fpsItem.imgRoot.sprite=FPSControl.GetCurrentWeaponIcon();
		}
		
		
		void UpdateAmmoCount(){
			int total=FPSControl.GetTotalAmmoCount();
			int current=FPSControl.GetCurrentAmmoCount();
			fpsItem.label.text=current+"/"+total;
		}
		
		
		private bool touchFiring=false;
		void OnFireButtonDown(GameObject butObj, int pointerID){
			OnFireButton(null);
			touchFiring=true;
		}
		void OnFireButtonUp(GameObject butObj, int pointerID){
			touchFiring=false;
		}
		
		public void OnFireButton(GameObject butObj){ FPSControl.Fire(); }
		
		public void OnReloadButton(){ FPSControl.Reload(); }
		public void OnPrevWeaponButton(){ FPSControl.SelectPrevWeapon(); }
		public void OnNextWeaponButton(){ FPSControl.SelectNextWeapon(); }
		
		public void OnExitFPSButton(){ FPSControl.Hide(); }
		
		
		
		private bool isOn=false;
		public static bool IsOn(){ return instance==null ? false : instance.isOn; }
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			OnSwitchWeapon();
			lbReloading.text="";
			
			isOn=true;
			
			buttonExit.SetActive(true);
			
			UIMainControl.DisableInput();
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			isOn=false;
			
			UIMainControl.EnableInput();
			UIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
		}
		
	}

}