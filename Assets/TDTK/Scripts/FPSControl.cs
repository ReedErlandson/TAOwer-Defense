using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class FPSControl : MonoBehaviour {

		
		public int recoilMode=1;
		
		public float aimSensitivity = 2f;
		
		//prefabID of tower unavailable int this level
		public List<int> unavailableIDList=new List<int>();
		[HideInInspector] public List<int> availableIDList=new List<int>();
		public static bool IsIDAvailable(int ID){ return !instance.unavailableIDList.Contains(ID); }
		
		
		//only used in runtime, filled up using info from unavailableIDList
		public List<FPSWeapon> weaponList=new List<FPSWeapon>();
		public int currentWeaponID=0;
		private FPSWeapon currentWeapon;
		
		public Transform weaponPivot;
		public Transform cameraPivot;
		public Transform camT;
		private Camera camFPS;
		private Camera camMain;
		
		private bool isInFPSMode=false;
		public static bool IsInFPSMode(){ return instance==null ? false : instance.isInFPSMode; }
		public static bool ActiveInScene(){ return instance==null ? false : true; }
		
		
		public static bool EnableInput(){ 
			return (!instance.camFPS.enabled || instance.lerping) ? false : true;
		}
		
		
		private GameObject thisObj;
		private Transform thisT;
		private static FPSControl instance;
		
		
		public void Init(){
			instance=this;
			thisObj=gameObject;
			thisT=transform;
			
			camMain=Camera.main;
			camFPS=camT.GetComponent<Camera>();
			
			recoilMode=Mathf.Clamp(recoilMode, 0, 2);
			if(recoilMode==2) weaponPivot.parent=camT;
			
			List<FPSWeapon> dbList=TDTK.GetFpsWeaponDBList();	//FPSWeaponDB.Load();
			
			availableIDList=new List<int>();
			weaponList=new List<FPSWeapon>();
			for(int i=0; i<dbList.Count; i++){
				if(dbList[i].disableInFPSControl) continue;
				if(!unavailableIDList.Contains(dbList[i].prefabID)){
					weaponList.Add(dbList[i]);
					availableIDList.Add(dbList[i].prefabID);
				}
			}
			
			List<FPSWeapon> newList=PerkManager.GetUnlockedWeaponList();
			for(int i=0; i<newList.Count; i++) weaponList.Add(newList[i]);
		}
		
		void Start(){
			//weapon initiation goes here, wait for PerkManager to initiate the perk
			if(weaponList.Count>0){
				for(int i=0; i<weaponList.Count; i++){
					Transform weapT=CreateWeaponInstance(weaponList[i].transform);
					weaponList[i]=weapT.GetComponent<FPSWeapon>();
				}
				
				currentWeapon=weaponList[currentWeaponID];
				currentWeapon.gameObject.SetActive(true);
			}
			
			thisObj.SetActive(false);
		}
		
		
		
		
		public static void AddNewWeapon(FPSWeapon weaponPrefab){ 
			if(instance!=null) instance._AddNewWeapon(weaponPrefab);
		}
		public void _AddNewWeapon(FPSWeapon weaponPrefab){
			for(int i=0; i<weaponList.Count; i++){ if(weaponList[i].prefabID==weaponPrefab.prefabID) return; }
			
			Transform weapT=CreateWeaponInstance(weaponPrefab.transform);
			weaponList.Add(weapT.GetComponent<FPSWeapon>());
		}
		
		Transform CreateWeaponInstance(Transform weapPrefabT){
			Transform weapT=(Transform)Instantiate(weapPrefabT);
			weapT.parent=weaponPivot;
			weapT.localPosition=Vector3.zero;
			weapT.localRotation=Quaternion.identity;
			weapT.gameObject.SetActive(false);
			return weapT;
		}
		
		
		
		
		public static void SelectNextWeapon(){ instance.SelectWeapon(1); }
		public static void SelectPrevWeapon(){ instance.SelectWeapon(-1); }
		public void SelectWeapon(int val){
			if(useTowerWeapon) return;
			
			currentWeapon.gameObject.SetActive(false);
			currentWeaponID+=val;
			if(currentWeaponID<0) currentWeaponID=weaponList.Count-1;
			else if(currentWeaponID>=weaponList.Count) currentWeaponID=0;
			
			currentWeapon=weaponList[currentWeaponID];
			currentWeapon.gameObject.SetActive(true);
			
			//if(onSwitchWeaponE!=null) onSwitchWeaponE();
			TDTK.OnFPSSwitchWeapon();
		}
		
		
		
		// Update is called once per frame
		void Update () {
			
			if(!camFPS.enabled || lerping) return;
			
			
			float x=cameraPivot.rotation.eulerAngles.x;
			float y=cameraPivot.rotation.eulerAngles.y;
			
			//make sure x is between -180 to 180 so we can clamp it propery later
			if(x>180) x-=360;
			
			//calculate the x and y rotation
			Quaternion rotationY=Quaternion.Euler(0, y, 0)*Quaternion.Euler(0, Input.GetAxis("Mouse X")*aimSensitivity, 0);
			Quaternion rotationX=Quaternion.Euler(Mathf.Clamp(x-Input.GetAxis("Mouse Y")*aimSensitivity, -70, 70), 0, 0);
			cameraPivot.rotation=rotationY*rotationX;
			
			
			weaponPivot.localRotation=Quaternion.Lerp(weaponPivot.localRotation, Quaternion.identity, Time.deltaTime*3);
			recoilModifier*=(1-Time.deltaTime*2);
		}
		
		
		
		
		
		
		
		
		public static void Fire(){ instance._Fire(); }
		public void _Fire(){
			//if(!weapon.ReadyToFire()) return;
			
			if(currentWeapon.Shoot()){
			
				AttackInstance attInstance=new AttackInstance();
				attInstance.srcWeapon=currentWeapon;
				
				for(int i=0; i<currentWeapon.shootPoints.Count; i++){
					Transform shootP=currentWeapon.shootPoints[i];
					Transform shootObjT=(Transform)Instantiate(currentWeapon.GetShootObject(), shootP.position, shootP.rotation);
					shootObjT.GetComponent<ShootObject>().ShootFPS(attInstance, shootP);
				}
				
				Recoil(currentWeapon.recoil);
				
				//if(onFPSShootE!=null) onFPSShootE();
				TDTK.OnFPSShoot();
				
			}
		}
		
		
		public static void Reload(){
			instance.currentWeapon.Reload();
		}
		public static float GetReloadProgress(){
			return instance.currentWeapon.GetReloadProgress();
		}
		
		public static void ReloadComplete(FPSWeapon weap){
			if(instance.currentWeapon==weap){
				//if(onFPSReloadE!=null) onFPSReloadE(false);
				TDTK.OnFPSReload(false);
			}
		}
		public static void StartReload(FPSWeapon weap){
			if(instance.currentWeapon==weap){
				//if(onFPSReloadE!=null) onFPSReloadE(true);
				TDTK.OnFPSReload(true);
			}
		}
		
		
		
		public Vector3 recoilDir;
		private bool recoiling=false;
		private float recoilModifier=0;
		public static float GetRecoilModifier(){ return instance.recoilModifier; }
		
		public void Recoil(float magnitude=1){
			if(recoilMode!=1 && recoilMode!=2) return;
			
			shakeMagnitude=magnitude*0.1f;
			if(!shaking) StartCoroutine(RecoilShakeRoutine());
				
			if(recoilMode==1){
				recoilModifier+=magnitude;
				
				float x=Random.Range(recoilModifier*0.5f, recoilModifier) * (Random.Range(0f, 1f)>=0.5f ? -1f : 1f);
				float y=-Random.Range(recoilModifier*0.5f, recoilModifier) * (Random.Range(0f, 1f)>=0.5f ? -1f : 1f);
				
				recoilDir=new Vector3(x, y, 0)*magnitude*5;
				if(!recoiling) StartCoroutine(RecoilRoutine());
			}
			if(recoilMode==2){
				float x=Random.Range(0.3f, 1.0f);
				float y=Random.Range(-0.3f, 0.3f);
				
				recoilDir=new Vector3(-x, y, 0)*magnitude*30;
				if(!recoiling) StartCoroutine(RecoilRoutine());
			}
			
		}
		
		IEnumerator RecoilRoutine(){
			recoiling=true;
			while(recoilDir.magnitude>0.01f){
				yield return null;
				if(recoilMode==1) weaponPivot.Rotate(recoilDir*Time.deltaTime*2);
				if(recoilMode==2) camT.Rotate(recoilDir*Time.deltaTime);
				recoilDir*=(1-Time.deltaTime*20);
			}
			recoiling=false;
		}
		
		private bool shaking=false;
		private float shakeMagnitude=0;
		IEnumerator RecoilShakeRoutine(){
			shaking=true;
			while(shakeMagnitude>0){
				float x=Random.Range(-shakeMagnitude, shakeMagnitude);
				float y=Random.Range(-shakeMagnitude, shakeMagnitude);
				float z=Random.Range(-1.5f*shakeMagnitude, 0);
				
				camT.localPosition=new Vector3(x, y, z)*1.5f;
				
				shakeMagnitude-=Time.deltaTime*.5f;
				
				yield return null;
			}
			shaking=false;
		}
		
		
		
		
		public bool useTowerWeapon=false;
		
		private UnitTower anchorTower;
		public void SetAnchorTower(UnitTower tower){
			if(!useTowerWeapon) return;
			
			anchorTower=tower;
			if(currentWeapon!=null) currentWeapon.gameObject.SetActive(false);
			currentWeapon=null;
			
			if(anchorTower.FPSWeaponID<0) return;
			
			for(int i=0; i<weaponList.Count; i++){
				if(weaponList[i].prefabID==tower.FPSWeaponID){
					currentWeapon=weaponList[i];
					currentWeapon.gameObject.SetActive(true);
				}
			}
		}
		
		
		
		
		
		public static int GetTotalAmmoCount(){ return instance.currentWeapon.GetClipSize(); }
		public static int GetCurrentAmmoCount(){ return instance.currentWeapon.GetCurrentAmmo(); }
		public static Sprite GetCurrentWeaponIcon(){ return instance.currentWeapon.icon; }
		public static FPSWeapon GetCurrentWeapon(){ return instance.currentWeapon; }
		
		public static bool UseTowerWeapon(){ return instance.useTowerWeapon; }
		
		
		
		
		
		
		public static void Hide(){ if(instance!=null) instance._Hide(); }
		public void _Hide(){
			Cursor.visible=true;
			
			isInFPSMode=false;
			
			//Screen.lockCursor=true;
			Cursor.visible=true;
			
			//if(onFPSModeE!=null) onFPSModeE(isInFPSMode);
			TDTK.OnFPSMode(isInFPSMode);
			
			StartCoroutine(_LerpToMainCam());
		}
		public static void Show(UnitTower tower){ if(instance!=null) instance._Show(tower); }
		public void _Show(UnitTower tower){
			if(weaponList.Count==0){
				TDTK.OnGameMessage("No available weapon");
				return;
			}
			if(useTowerWeapon && tower.FPSWeaponID<0){
				TDTK.OnGameMessage("Tower doesn't have a weapon");
				return;
			}
			
			SetAnchorTower(tower);
			
			thisT.position=tower.thisT.position+new Vector3(0, 5, 0);
			thisT.rotation=Camera.main.transform.rotation;
			
			//Screen.lockCursor=false;
			Cursor.visible=false;
			
			isInFPSMode=true;
			
			//if(onFPSModeE!=null) onFPSModeE(isInFPSMode);
			TDTK.OnFPSMode(isInFPSMode);
			thisObj.SetActive(isInFPSMode);
			
			StartCoroutine(_LerpToView(thisT.position));
		}
		
		
		private bool lerping=false;
		IEnumerator _LerpToView(Vector3 targetPos){
			lerping=true;
			camFPS.enabled=true;
			
			camFPS.gameObject.tag="MainCamera";
			camMain.gameObject.tag="Untagged";
			
			camFPS.gameObject.GetComponent<AudioListener>().enabled=true;
			camMain.gameObject.GetComponent<AudioListener>().enabled=false;
			
			//if(onFPSCameraE!=null) onFPSCameraE();
			TDTK.OnFPSSwitchCamera();
			
			float targetFOV=camFPS.fieldOfView;
			
			cameraPivot.rotation=camMain.transform.rotation;
			
			camT.position=camMain.transform.position;
			camFPS.fieldOfView=camMain.fieldOfView;
			
			Vector3 startingPos=camT.position;
			float startingFOV=camFPS.fieldOfView;
			
			float duration=0;
			while(duration<1f){
				camT.position=Vector3.Lerp(startingPos, targetPos, duration);
				camFPS.fieldOfView=Mathf.Lerp(startingFOV, targetFOV, duration);
				duration+=Time.deltaTime*1;
				yield return null;
			}
			
			camT.position=targetPos;
			camFPS.fieldOfView=targetFOV;
			lerping=false;
		}
		IEnumerator _LerpToMainCam(){
			lerping=true;
			Vector3 targetPos=camMain.transform.position;
			Quaternion targetRot=camMain.transform.rotation;
			float targetFOV=camMain.fieldOfView;
			
			Vector3 startingPos=camT.position;
			Quaternion startingRot=camT.rotation;
			float startingFOV=camFPS.fieldOfView;
			
			float duration=0;
			while(duration<1f){
				camT.position=Vector3.Lerp(startingPos, targetPos, duration);
				camT.rotation=Quaternion.Lerp(startingRot, targetRot, duration);
				camFPS.fieldOfView=Mathf.Lerp(startingFOV, targetFOV, duration);
				duration+=Time.deltaTime*1;
				yield return null;
			}
			
			camT.position=targetPos;
			camT.rotation=targetRot;
			camFPS.fieldOfView=targetFOV;
			
			camFPS.gameObject.tag="Untagged";
			camMain.gameObject.tag="MainCamera";
			
			camFPS.gameObject.GetComponent<AudioListener>().enabled=false;
			camMain.gameObject.GetComponent<AudioListener>().enabled=true;
			
			//if(onFPSCameraE!=null) onFPSCameraE();
			TDTK.OnFPSSwitchCamera();
			
			camFPS.enabled=false;
			lerping=false;
			
			camT.localRotation=Quaternion.identity;
			
			thisObj.SetActive(isInFPSMode);
		}
	}

}