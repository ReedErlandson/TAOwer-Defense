using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDTK;

namespace TDTK {

	public class UIGameOver : MonoBehaviour {

		private GameObject thisObj;
		private CanvasGroup canvasGroup;
		private static UIGameOver instance;
		
		public Text lbTitle;
		
		public GameObject buttonNext;
		
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			thisObj.SetActive(false);
			thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		public void OnNextButton(){
			GameControl.LoadNextScene();
		}
		public void OnRestartButton(){
			GameControl.RestartScene();
		}
		public void OnMenuButton(){
			GameControl.LoadMainMenu();
		}
		
		
		public static void Show(bool won){ instance._Show(won); }
		public void _Show(bool won){
			if(won) lbTitle.text="Level Cleared!";
			else lbTitle.text="Level Lost";
			
			if(!(UIMainControl.AlwaysShowNextButton() || won) && buttonNext!=null) buttonNext.SetActive(false);
			
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
		}
		
	}

}