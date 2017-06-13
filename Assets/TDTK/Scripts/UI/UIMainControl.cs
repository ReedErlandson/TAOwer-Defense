using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;

using TDTK;

namespace TDTK {
	
	public class UIMainControl : MonoBehaviour {
		
		[Space(5)]
		[Tooltip("Check to make the force the UI into Perk Menu only.\nIntended for inter-level scene to unlock new perk in for design which involve persistent perk unlock.")]
		public bool perkMenuOnly=false;
		
		[Space(5)]
		[Tooltip("Check to enable touch mode (optional mode intend for touch input)\n\nwhen using touch mode, build and ability button wont be trigger immediately as soon as they are click.\n\nInstead the first click will only bring up the tooltip, a second click will then confirm the button click")]
		public bool touchMode=false;
		public static bool InTouchMode(){ return instance.touchMode; }
		
		[Space(5)]
		[Tooltip("Check to use pie-menu arrangement for the build-buttons.\nOnly applicable when using PointNBuild Mode")]
		public bool usePieBuildMenu=false;
		public static bool UsePieMenu(){ return instance.usePieBuildMenu; }
		
		[Space(5)]
		[Tooltip("Check to show hit-point overlay on top of each unit")]
		public bool enableHPOverlay=true;
		public static bool EnableHPOverlay(){ return instance.enableHPOverlay; }
		
		[Tooltip("Check to show damage overlay of each attack when hitting a target")]
		public bool enableTextOverlay=true;
		public static bool EnableTextOverlay(){ return instance.enableTextOverlay; }
		
		[Space(5)]
		[Tooltip("Check to have the game over menu show the 'next level button' only whent the level is won")]
		public bool alwaysShowNextButton=true;
		public static bool AlwaysShowNextButton(){ return instance.alwaysShowNextButton; }
		
		[Space(10)]
		[Tooltip("The blur image effect component on the main ui camera (optional)")]
		public BlurOptimized uiBlurEffect;
		
		[Space(10)]
		[Tooltip("Check to have the camera auto center on build point or selected tower\nOnly available when using default CameraControl")]
		public bool autoCenterCamera=false;
		
		[Space(10)]
		[Tooltip("Check to disable auto scale up of UIElement when the screen resolution exceed reference resolution specified in CanvasScaler/nRecommended to have this set to false when building for mobile")]
		public bool limitScale=true;
		
		[Tooltip("The CanvasScaler components of all the canvas. Required to have the floating UI elements appear in the right screen position")]
		public List<CanvasScaler> scalerList=new List<CanvasScaler>();
		public static float GetScaleFactor(){ 
			if(instance.scalerList.Count==0) return 1;
			
			if(instance.scalerList[0].uiScaleMode==CanvasScaler.ScaleMode.ConstantPixelSize) 
				return 1f/instance.scalerList[0].scaleFactor;
			if(instance.scalerList[0].uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize) 
				return (float)instance.scalerList[0].referenceResolution.x/(float)Screen.width;
			
			return 1;
		}
		
		
		private bool enableInput=true;		//for whatever reason the input needs to be stopped
		public static void EnableInput(){ instance.enableInput=true; }
		public static void DisableInput(){ instance.enableInput=false; }
		
		
		private static UIMainControl instance;
		
		void Awake(){
			instance=this;
		}
		
		void Start(){
			if(limitScale){
				for(int i=0; i<scalerList.Count; i++){
					if(Screen.width>=scalerList[i].referenceResolution.x) instance.scalerList[i].uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
					else instance.scalerList[i].uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
				}
			}
			
			if(perkMenuOnly){
				UIHUD.GetInstance().gameObject.SetActive(false);
				UIBuildButton.GetInstance().gameObject.SetActive(false);
				UIAbilityButton.GetInstance().gameObject.SetActive(false);
				UITowerView.GetInstance().gameObject.SetActive(false);
				
				UIPerkMenu.DisableCloseButton();
				OnPerkMenu();
			}
		}
		
		
		void Update(){
			if(Input.GetKeyDown(KeyCode.Escape)){
				if(BuildManager.UseDragNDrop() && BuildManager.InDragNDrop()) return;
				if(!UITowerView.IsOn() && !AbilityManager.IsSelectingTarget() && !UIPerkMenu.IsOn() && !UIFPS.IsOn()) 
					TogglePause();
			}
			
			//keyboard for cursor based input start here
			if(!enableInput) return;
			
			if(Input.touchCount>1) return;
			if(UI.IsCursorOnUI(0) || UI.IsCursorOnUI()) return;
			
			if(Input.GetMouseButtonDown(0)){
				OnCursorDown(Input.mousePosition);
			}
		}
		
		
		void OnCursorDown(Vector3 cursorPos){
			UnitTower tower=GameControl.Select(cursorPos);
			GameControl.SelectTower(tower);
			
			if(tower!=null){
				if(!BuildManager.UseDragNDrop()) UIBuildButton.Hide();
				if(autoCenterCamera) CameraControl.SetPosition(tower.thisT.position);
				
				UITowerView.Show(tower);
			}
			else{
				UITowerView.Hide();
				
				if(!BuildManager.UseDragNDrop()){
					BuildInfo buildInfo=BuildManager.CheckBuildPoint(cursorPos);
					UIBuildButton.Show(buildInfo);
					if(buildInfo.status==_TileStatus.Available && autoCenterCamera)
						CameraControl.SetPosition(buildInfo.position);
				}
			}
		}
		
		
		public static void ClearSelectedTower(){	//for CameraControl to clear tower when panning the camera (using touch control)
			GameControl.SelectTower();
			UITowerView.Hide();
		}
		
		
		void OnEnable() {
			TDTK.onGameOverE += OnGameOver;
			TDTK.onFPSModeE += OnFPSMode;
		}
		void OnDisable() {
			TDTK.onGameOverE -= OnGameOver;
			TDTK.onFPSModeE -= OnFPSMode;
		}
		
		public void OnGameOver(bool won){
			StartCoroutine(GameOverDelay(won));
		}
		IEnumerator GameOverDelay(bool won){
			yield return StartCoroutine(WaitForRealSeconds(.1f));
			CameraControl.FadeBlur(uiBlurEffect, 0, 2);
			CameraControl.TurnBlurOn();
			UIGameOver.Show(won);
		}
		
		public static void OnFPSMode(bool flag){
			if(flag){
				UIBuildButton.Hide();
				UIAbilityButton.Hide();
				UIFPS.Show();
			}
			else{
				UIBuildButton.Show();
				UIAbilityButton.Show();
				UIFPS.Hide();
			}
		}
		
		
		
		public static void OnPerkMenu(){ instance._OnPerkMenu(); }
		public void _OnPerkMenu(){
			UITowerView.Hide();
			
			CameraControl.FadeBlur(uiBlurEffect, 0, 2);
			CameraControl.TurnBlurOn();
			GameControl.PauseGame();
			UIPerkMenu.Show();
			
			Time.timeScale=0;
		}
		public static void ClosePerkMenu(){ instance.StartCoroutine(instance._ClosePerkMenu()); }
		IEnumerator _ClosePerkMenu(){
			CameraControl.FadeBlur(uiBlurEffect, 2, 0);
			CameraControl.TurnBlurOff();
			GameControl.ResumeGame();
			UIPerkMenu.Hide();
			
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
			Time.timeScale=1;
		}
		
		
		
		public static void TogglePause(){ instance._TogglePause(); }
		public void _TogglePause(){
			if(GameControl.IsGamePlaying()) PauseGame();
			else if(GameControl.IsGamePaused()) ResumeGame();
		}
		
		public static void PauseGame(){ instance._PauseGame(); }
		public void _PauseGame(){
			Debug.Log("_PauseGame");
			CameraControl.FadeBlur(uiBlurEffect, 0, 2);
			CameraControl.TurnBlurOn();
			GameControl.PauseGame();
			UIPauseMenu.Show();
			
			//Time.timeScale=0;
		}
		public static void ResumeGame(){ instance.StartCoroutine(instance._ResumeGame()); }
		IEnumerator _ResumeGame(){
			Debug.Log("_ResumeGame");
			CameraControl.FadeBlur(uiBlurEffect, 2, 0);
			CameraControl.TurnBlurOff();
			GameControl.ResumeGame();
			UIPauseMenu.Hide();
			
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
			//Time.timeScale=1;
		}
		
		
		public static IEnumerator WaitForRealSeconds(float time){
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time) yield return null;
		}
		
		
		public static void FadeOut(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			instance.StartCoroutine(instance._FadeOut(canvasGroup, 1f/duration, obj));
		}
		IEnumerator _FadeOut(CanvasGroup canvasGroup, float timeMul, GameObject obj){
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(1f, 0f, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=0f;
			
			if(obj!=null) obj.SetActive(false);
		}
		public static void FadeIn(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			instance.StartCoroutine(instance._FadeIn(canvasGroup, 1f/duration, obj)); 
		}
		IEnumerator _FadeIn(CanvasGroup canvasGroup, float timeMul, GameObject obj){
			if(obj!=null) obj.SetActive(true);
			
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(0f, 1f, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=1f;
		}
		
		
	}

}