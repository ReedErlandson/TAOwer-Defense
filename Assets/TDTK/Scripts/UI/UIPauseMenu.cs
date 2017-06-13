using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TDTK;

namespace TDTK {

	public class UIPauseMenu : MonoBehaviour {

		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIPauseMenu instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			//thisObj.SetActive(false);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			rectT.anchoredPosition=new Vector3(0, 99999, 0);
		}
		
		
		public void OnResumeButton(){
			UIMainControl.ResumeGame();
		}
		public void OnRestartButton(){
			GameControl.RestartScene();
		}
		public void OnOptionButton(){
			
		}
		public void OnMenuButton(){
			GameControl.LoadMainMenu();
		}
		
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			rectT.localPosition=new Vector3(0, 0, 0);
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
			rectT.localPosition=new Vector3(-5000, -5000, 0);
		}
		
	}

}