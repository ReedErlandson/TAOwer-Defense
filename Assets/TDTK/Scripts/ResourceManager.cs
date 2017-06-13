using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {
	
	
	public class ResourceManager : MonoBehaviour {
		
		
		public bool carryFromLastScene=false;
		private static List<int> lastLevelValueList=new List<int>();
		private static List<int> initialValueList=new List<int>();
		
		public bool enableRscGen=false;
		public List<float> rscGenRateList=new List<float>();
		
		public List<int> startingValueList=new List<int>();
		
		[HideInInspector] 
		public List<Rsc> rscList=new List<Rsc>();
		
		public static ResourceManager instance;
		
		
		
		public void Init(){	//to match the rscList with the DB
			instance=this;
			
			rscList=ResourceDB.LoadClone();
			
			if(carryFromLastScene){
				for(int i=0; i<lastLevelValueList.Count; i++)
					rscList[i].value=lastLevelValueList[i];
			}
			else{
				for(int i=0; i<rscList.Count; i++){
					if(i<startingValueList.Count) rscList[i].value=startingValueList[i];
				}
			}
			
			for(int i=0; i<rscList.Count; i++) initialValueList.Add(rscList[i].value);
			
			if(enableRscGen) StartCoroutine(RscGenRoutine());
		}
		
		
		public static int GetResourceCount(){ return instance.rscList.Count; }
		public static List<Rsc> GetResourceList(){ return instance.rscList; }
		
		
		public static void OnGameOver(bool flag){ instance._OnGameOver(flag); }
		public void _OnGameOver(bool flag){
			if(!flag) return;
			
			lastLevelValueList=new List<int>();
			for(int i=0; i<rscList.Count; i++) lastLevelValueList.Add(rscList[i].value);
		}
		public static void OnRestartLevel(){
			lastLevelValueList=new List<int>(initialValueList);
		}
		
		
		
		IEnumerator RscGenRoutine(){
			List<float> temp=new List<float>();
			for(int i=0; i<rscList.Count; i++) temp.Add(0);
			
			while(true){
				yield return new WaitForSeconds(1);
				
				List<float> perkRegenRate=PerkManager.GetRscRegen();
				List<int> valueList=new List<int>();
				
				bool increased=false;
				
				for(int i=0; i<rscList.Count; i++){
					temp[i]+=rscGenRateList[i]+perkRegenRate[i];
					
					valueList.Add(0);
					
					if(temp[i]>=1){ 
						while(temp[i]>=1){
							valueList[i]+=1;
							temp[i]-=1;
						}
						increased=true;
					}
				}
				
				if(increased) GainResource(valueList);
			}
		}
		
		
		
		public static int HasSufficientResource(List<int> rscL){ return instance._HasSufficientResource(rscL); }
		public int _HasSufficientResource(List<int> rscL){
			if(rscList.Count!=rscL.Count){
				Debug.Log("error, resource number doesnt match!    "+rscList.Count+"     "+rscL.Count);
				return -99;
			}
			for(int i=0; i<rscList.Count; i++){
				if(rscList[i].value<rscL[i]) return i;
			}
			return -1;
		}
		
		public static void SpendResource(List<int> rscL){ instance._GainResource(rscL, null, false, -1); }
		public static void GainResource(List<int> rscL, List<float> mulL=null, bool useMul=true){ instance._GainResource(rscL, mulL, useMul); }
		public void _GainResource(List<int> rscL, List<float> mulL=null, bool useMul=true, float sign=1f){
			if(rscList.Count!=rscL.Count) return;
			
			//if this is gain, apply perks multiplier
			if(sign==1 && useMul){
				List<float> multiplierL=PerkManager.GetRscGain();
				
				if(mulL!=null){
					for(int i=0; i<multiplierL.Count; i++) multiplierL[i]+=mulL[i];
				}
				
				for(int i=0; i<multiplierL.Count; i++) rscL[i]=(int)((float)rscL[i]*(1f+multiplierL[i]));
			}
			
			if(sign==-1){
				for(int i=0; i<rscL.Count; i++) rscL[i]=(int)(rscL[i]*sign);
			}
			
			for(int i=0; i<rscList.Count; i++){
				rscList[i].value=(int)Mathf.Max(0, rscList[i].value+rscL[i]);
			}
			
			//if(onRscChangedE!=null) onRscChangedE(rscL);
			TDTK.OnResource(rscL);
		}
		
		
		
		public static void SetResourceValue(List<int> rscL){ instance._SetResourceValue(rscL); } 
		public void _SetResourceValue(List<int> rscL){
			if(rscList.Count!=rscL.Count){
				Debug.Log("error, resource number doesnt match!");
				return;
			}

			for(int i=0; i<rscList.Count; i++){
				rscList[i].value=(int)Mathf.Max(0, rscL[i]);
			}
		}
		
		
		
		
		//not in use at the moment
		public static void NewSceneNotification(){
			//resourcesA=resourceManager.resources;
		}
		public static void ResetCummulatedResource(){
			//for(int i=0; i<resourcesA.Length; i++){
			//	resourcesA[i].value=0;
			//}
		}
		
		
		
		
	}

}
