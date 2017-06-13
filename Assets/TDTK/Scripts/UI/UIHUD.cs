using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class UIHUD : MonoBehaviour{

		public Text txtLife;
		public Text txtWave;
		
		public List<UIObject> rscItemList=new List<UIObject>();
		
		public Text txtTimer;
		public UIButton butSpawn;
		private Vector3 butSpawnDefaultPos;
		
		public UIButton butFF;
		
		public GameObject butPerkMenuObj;
		
		private static UIHUD instance;
		public static UIHUD GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
		}
		
		
		// Use this for initialization
		void Start () {
			List<Rsc> rscList=ResourceManager.GetResourceList();
			for(int i=0; i<rscList.Count; i++){
				if(i==0) rscItemList[0].Init();
				else rscItemList.Add(UIObject.Clone(rscItemList[0].rootObj, "Rsc"+(i+1)));
				
				rscItemList[i].imgRoot.sprite=rscList[i].icon;
				rscItemList[i].label.text=rscList[i].value.ToString();
			}
			
			txtTimer.text="";
			butSpawn.Init();
			butSpawnDefaultPos=butSpawn.rectT.localPosition;
			
			butFF.Init();
			
			butPerkMenuObj.SetActive(PerkManager.IsOn());
			
			OnLife(0);
			OnNewWave(0);
			OnEnableSpawn();
		}
		
		void OnEnable(){
			TDTK.onLifeE += OnLife;
			TDTK.onFastForwardE += OnFastForward;
			
			TDTK.onNewWaveE += OnNewWave;
			TDTK.onEnableSpawnE += OnEnableSpawn;
			
			TDTK.onResourceE += OnResourceChanged;
		}
		void OnDisable(){
			TDTK.onLifeE -= OnLife;
			TDTK.onFastForwardE -= OnFastForward;
			
			TDTK.onNewWaveE -= OnNewWave;
			TDTK.onEnableSpawnE -= OnEnableSpawn;
			
			TDTK.onResourceE -= OnResourceChanged;
		}
		
		
		void OnLife(int changedvalue){
			int cap=GameControl.GetPlayerLifeCap();
			string text=(cap>0) ? "/"+GameControl.GetPlayerLifeCap() : "" ;
			txtLife.text=GameControl.GetPlayerLife()+text;
		}
		
		void OnResourceChanged(List<int> valueChangedList){
			List<Rsc> rscList=ResourceManager.GetResourceList();
			for(int i=0; i<rscItemList.Count; i++) rscItemList[i].label.text=rscList[i].value.ToString();
		}
		
		void OnNewWave(int waveID){
			int totalWaveCount=SpawnManager.GetTotalWaveCount();
			string text=totalWaveCount>0 ? "/"+totalWaveCount : "";
			txtWave.text=waveID+text;
			
			butSpawn.rectT.localPosition=new Vector3(0, 99999, 0);
			if(waveID>0) butSpawn.label.text="Next Wave";
		}
		
		void OnEnableSpawn(){
			butSpawn.rectT.localPosition=butSpawnDefaultPos;
			butSpawn.rootObj.SetActive(true);
		}
		
		
		public void OnSpawnButton(){
			SpawnManager.Spawn();
			butSpawn.rectT.localPosition=new Vector3(0, 99999, 0);
			butSpawn.label.text="Next Wave";
		}
		
		
		public void OnFFButton(){ GameControl.FastForward(!GameControl.IsFastForwardOn()); }
		public void OnFastForward(bool flag){ butFF.imgIcon.enabled=!flag; }
		
		
		public void OnPerkButton(){
			UIMainControl.OnPerkMenu();
		}
		
		
		public void OnMenuButton(){
			UIMainControl.TogglePause();
		}
		
		
		// Update is called once per frame
		void Update () {
			float timeToNextSpawn=SpawnManager.GetTimeToNextSpawn();
			if(timeToNextSpawn>0){
				if(timeToNextSpawn<60) txtTimer.text="Next Wave in "+timeToNextSpawn.ToString("f1")+"s";
				else txtTimer.text="Next Wave in "+(Mathf.Floor(timeToNextSpawn/60)).ToString("f0")+"m";
			}
			else txtTimer.text="";
		}
		
	}

}