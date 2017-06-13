using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[System.Serializable]
	public class WaveGenerator{
		
		public bool similarSubWave=false;
		
		public float waveSpacingTimeMin=5;
		public float waveSpacingTimeMax=5;
		
		public ProceduralVariable subWaveCount=new ProceduralVariable(1, 4);
		
		public ProceduralVariable unitCount=new ProceduralVariable(5, 50);
		
		public List<PathTD> pathList=new List<PathTD>();
		public bool utiliseAllPath=true;
		
		public List<ProceduralVariable> rscSettingList=new List<ProceduralVariable>();
		
		public List<ProceduralUnitSetting> unitSettingList=new List<ProceduralUnitSetting>();
		
		public void CheckPathList(){
			for(int i=0; i<pathList.Count; i++){
				if(pathList[i]==null){
					pathList.RemoveAt(i);
					i-=1;
				}
			}
		}
		
		public void Init(){
			int rscCount=ResourceManager.GetResourceCount();
			while(rscSettingList.Count<rscCount) rscSettingList.Add(new ProceduralVariable(0, 0));
			while(rscSettingList.Count>rscCount) rscSettingList.RemoveAt(rscSettingList.Count-1);
			
			CheckPathList();
		}
		
		public Wave Generate(int waveID){
			if(pathList.Count==0){
				Debug.Log("no path at all");
				return null;
			}
			
			Wave wave=new Wave();
			wave.waveID=waveID;
			
			waveID+=1;
			
			int _subWaveCount=Mathf.Max(1, (int)subWaveCount.GetValueAtWave(waveID));
			int totalUnitCount=(int)unitCount.GetValueAtWave(waveID);
			
			_subWaveCount=Mathf.Min(totalUnitCount, _subWaveCount);
			
			//filter thru all the units, only use the one that meets the wave requirement (>minWave)
			List<ProceduralUnitSetting> availableUnitList=new List<ProceduralUnitSetting>();
			int nearestAvailableID=0; 	float currentNearestValue=Mathf.Infinity;
			for(int i=0; i<unitSettingList.Count; i++){
				if(!unitSettingList[i].enabled) continue;
				if(unitSettingList[i].minWave<=waveID) availableUnitList.Add(unitSettingList[i]);
				//while we are at it, check which unit has the lowest wave requirement, just in case
				if(availableUnitList.Count==0 && unitSettingList[i].minWave<currentNearestValue){
					currentNearestValue=unitSettingList[i].minWave;
					nearestAvailableID=i;
				}
			}
			//if no unit available, simply uses the one with lowest requirement
			if(availableUnitList.Count==0) availableUnitList.Add(unitSettingList[nearestAvailableID]);
			
			wave.subWaveList=new List<SubWave>();
			
			if(similarSubWave){
				wave.subWaveList.Add(GenerateSubWave(waveID, 0, availableUnitList));
				for(int i=1; i<_subWaveCount; i++) wave.subWaveList.Add(wave.subWaveList[0].Clone());
			}
			else{
				for(int i=0; i<_subWaveCount; i++){
					wave.subWaveList.Add(GenerateSubWave(waveID, i, availableUnitList));
				}
			}
			
			
			//we are going to just iterate thru the pathlist and assign them to each subwave. 
			//So here we introduce an offset so it doesnt always start from the first path in the list
			int startingPathID=Random.Range(0, pathList.Count);
			List<int> usedPathIdx=new List<int>();
			for(int i=0; i<wave.subWaveList.Count; i++){
				//iterate through the path, randomly skip one
				int pathID=startingPathID+(Random.Range(0f, 1f)>0.75f ? 1 : 0);
				while(pathID>=pathList.Count) pathID-=pathList.Count;
				if(utiliseAllPath){
					if(usedPathIdx.Count<pathList.Count && usedPathIdx.Contains(pathID)){
						pathID+=1;
						if(pathID>=pathList.Count) pathID=0;
					}
				}
				wave.subWaveList[i].path=pathList[pathID];
				usedPathIdx.Add(pathID);
			}
			
			
			//fill up the unit count
			if(similarSubWave){
				int count=(int)Mathf.Floor(totalUnitCount/wave.subWaveList.Count);
				for(int i=0; i<wave.subWaveList.Count; i++) wave.subWaveList[i].count=count;
			}
			else{
				int remainingUnitCount=totalUnitCount;
				while(remainingUnitCount>0){
					for(int i=0; i<_subWaveCount; i++){
						if(wave.subWaveList[i].count==0){
							wave.subWaveList[i].count=1;
							remainingUnitCount-=1;
						}
						else{
							int rand=Random.Range(0, 3);
							rand=Mathf.Min(rand, remainingUnitCount);
							wave.subWaveList[i].count+=rand;
							remainingUnitCount-=rand;
						}
					}
				}
			}
			
			wave.duration=wave.CalculateSpawnDuration()+Random.Range(waveSpacingTimeMin, waveSpacingTimeMax);
			
			//get the slowest moving unit and the longest path so we know which subwave is going to take the longest to finish
			float longestDuration=0;
			for(int i=0; i<_subWaveCount; i++){
				//float pathDist=wave.subWaveList[i].path.GetPathDistance();
				float pathDist=wave.subWaveList[i].path==null ? 0 : wave.subWaveList[i].path.GetPathDistance();
				float moveSpeed=wave.subWaveList[i].overrideMoveSpd;
				float duration=pathDist/moveSpeed;
				if(duration>longestDuration) longestDuration=duration;
			}
			//add the longest to the existing duration
			wave.duration+=longestDuration*Random.Range(0.5f, 0.8f);
			
			for(int i=0; i<rscSettingList.Count; i++){
				wave.rscGainList.Add((int)rscSettingList[i].GetValueAtWave(waveID));
			}
			
			return wave;
		}
		
		
		SubWave GenerateSubWave(int waveID, int subWaveID, List<ProceduralUnitSetting> availableUnitList){
			SubWave subWave=new SubWave();
			
			int unitID=Random.Range(0, availableUnitList.Count);
			ProceduralUnitSetting unitSetting=availableUnitList[unitID];
			
			subWave.unit=unitSetting.unit.gameObject;
			subWave.count=0;
			
			subWave.overrideHP=unitSetting.HP.GetValueAtWave(waveID);
			subWave.overrideShield=unitSetting.shield.GetValueAtWave(waveID);
			subWave.overrideMoveSpd=unitSetting.speed.GetValueAtWave(waveID);
			
			//limit the minimum interval to 0.25f
			subWave.interval=Mathf.Max(0.25f, unitSetting.interval.GetValueAtWave(waveID));
			
			subWave.delay=subWaveID*Random.Range(1f, 3f);
			
			return subWave;
		}
	}
	
	

	
	
	[System.Serializable]
	public class ProceduralUnitSetting{
		public GameObject unit;
		public UnitCreep unitC;
		
		public bool enabled=true;
		public int minWave=0;	//minimum wave to appear
		
		public ProceduralVariable HP=new ProceduralVariable(1, 50);		//HP
		public ProceduralVariable shield=new ProceduralVariable(0, 25);	//shield
		public ProceduralVariable speed=new ProceduralVariable(1, 6);	//speed
		public ProceduralVariable interval=new ProceduralVariable(1, 6);	//spawn interval
	}
	
	[System.Serializable]
	public class ProceduralVariable{
		public float startValue=5;		//C as in linear equation y=mx+C		x being the wave number, start at x=0
		public float incMultiplier=1f;		//M as in linear equation y=Mx+c		
		public float devMultiplier=0.2f;	//20% deviation, deviation multiplier applied after y in y=mx+c is calcualted
		public float minValue=1;
		public float maxValue=50;
		
		public ProceduralVariable(){}
		public ProceduralVariable(float val1, float val2){
			startValue=val1;
			maxValue=val2;
		}
		
		//linear increament, y=mx+c
		public float GetValueAtWave(int waveID){
			float value=(incMultiplier*waveID+startValue)*(1f+Random.Range(-devMultiplier, devMultiplier));
			return Mathf.Clamp(value, minValue, maxValue);
		}
		
		//non-linear, compounding increment, experimental, not in use
		//public float currentValue=5;		//also act as starting value
		//public float incMultiplier=1f;		//inc by 10% each iteration
		//public float devMultiplier=0.2f;	//10% deviation each iteration
		//currentValue=currentValue*(incMultiplier+Random.Range(-devMultiplier, devMultiplier));
	}
	
	
}