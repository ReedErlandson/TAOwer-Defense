using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class SpawnEditorWindow : TDEditorWindow {
		
		private static SpawnManager instance;
		private static SpawnEditorWindow window;
		
		private bool configureProcedural=false;				//take consideration of spawn mode, (the master flag)
		private bool configureProceduralSetting=false;	//toggle by button
		
		private List<bool> waveFoldList=new List<bool>();
		
		
		public static void Init(SpawnManager smInstance=null) {
			// Get existing open window or if none, make a new one:
			window = (SpawnEditorWindow)EditorWindow.GetWindow(typeof (SpawnEditorWindow), false, "Spawn Manager Editor");
			window.minSize=new Vector2(500, 300);
			
			LoadDB();
			
			InitLabel();
			
			window.InitParameter();
			
			if(smInstance!=null) instance=smInstance;
		}
		
		private static string[] spawnLimitLabel;
		private static string[] spawnLimitTooltip;
		
		private static string[] spawnModeLabel;
		private static string[] spawnModeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(SpawnManager._SpawnLimit)).Length;
			spawnLimitLabel=new string[enumLength];
			spawnLimitTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				spawnLimitLabel[i]=((SpawnManager._SpawnLimit)i).ToString();
				if((SpawnManager._SpawnLimit)i==SpawnManager._SpawnLimit.Finite) spawnLimitTooltip[i]="Finite number of waves";
				if((SpawnManager._SpawnLimit)i==SpawnManager._SpawnLimit.Infinite) spawnLimitTooltip[i]="Infinite number of waves (for survival or endless mode)";
			}
			
			enumLength = Enum.GetValues(typeof(SpawnManager._SpawnMode)).Length;
			spawnModeLabel=new string[enumLength];
			spawnModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				spawnModeLabel[i]=((SpawnManager._SpawnMode)i).ToString();
				if((SpawnManager._SpawnMode)i==SpawnManager._SpawnMode.Continous) 
					spawnModeTooltip[i]="A new wave is spawn upon every wave duration countdown (with option to skip the timer)";
				if((SpawnManager._SpawnMode)i==SpawnManager._SpawnMode.WaveCleared) 
					spawnModeTooltip[i]="A new wave is spawned when the current wave is cleared (with option to spawn next wave in advance)";
				if((SpawnManager._SpawnMode)i==SpawnManager._SpawnMode.Round) 
					spawnModeTooltip[i]="Each wave is treated like a round. a new wave can only take place when the previous wave is cleared. Each round require initiation from user";
			}
		}
		
		private void InitParameter(){
			proceduralVariableWidth=spaceX+2*widthS-33;
			subWaveBoxWidth=width+42;
		}
		
		public override bool OnGUI() {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			if(instance==null && !GetSpawnManager()) return true;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(instance, "SpawnManager");
			
			UpdateProceduralUnitList();
			
			configureProcedural=instance.spawnLimit==SpawnManager._SpawnLimit.Infinite | configureProceduralSetting | instance.procedurallyGenerateWave;
			
			if(instance.spawnLimit==SpawnManager._SpawnLimit.Finite && !instance.procedurallyGenerateWave){
				string text=configureProceduralSetting ? "Wave List" : "Configuration";
				if(GUI.Button(new Rect(window.position.width-130, 5, 125, 25), text)){
					configureProceduralSetting=!configureProceduralSetting;
				}
				
				if(!configureProceduralSetting){
					GUI.color=new Color(0, 1, 1, 1);
					cont=new GUIContent("Auto Generate", "Procedurally generate all the waves\nCAUTION: overwirte all existing wave!");
					if(GUI.Button(new Rect(window.position.width-130, 35, 125, 25), cont)){
						for(int i=0; i<instance.waveList.Count; i++) instance.waveList[i]=instance.waveGenerator.Generate(i);
					}
					GUI.color=Color.white;
				}
			}
			
			if(GUI.Button(new Rect(window.position.width-130, 110, 125, 25), "Save")) SetDirtyTD();
			
			if(GUI.Button(new Rect(window.position.width-130, 80, 125, 25), "Creep Editor")){
				UnitCreepEditorWindow.Init();
			}
			
			
			float startX=5;	float startY=5;
			
			startY=DrawGeneralSetting(startX, startY)+10;
			
			if(!configureProcedural) EditorGUI.LabelField(new Rect(startX, startY, 2*width, height), "Spawn Info (wave list)", headerStyle);
			else EditorGUI.LabelField(new Rect(startX, startY, 2*width, height), "Procedural Wave Generation Parameters", headerStyle);	
				
			
			startY+=spaceY;
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX*2, window.position.height-(startY+5));
			Rect contentRect=new Rect(startX, startY, contentWidth-25, contentHeight);
			
			
			GUI.color=new Color(.8f, .8f, .8f, 1f);
			GUI.Box(visibleRect, "");
			GUI.color=Color.white;
			
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				if(configureProcedural){
					startY=DrawProceduralSetting(startX+=5, startY+=5)+10;
					
					contentWidth=proUnitItemWidth+10;
					contentHeight=startY-visibleRect.y;
				}
				else{
					startY=DrawSpawnInfo(startX+=5, startY+=5);
					
					maxSubWaveSize=1;
					for(int i=0; i<instance.waveList.Count; i++)
						maxSubWaveSize=Mathf.Max(instance.waveList[i].subWaveList.Count, maxSubWaveSize);
					
					contentWidth=(subWaveBoxWidth+10)*maxSubWaveSize+60;
					contentHeight=startY-visibleRect.y;
				}
			
			GUI.EndScrollView();
			
				
			if(GUI.changed) EditorUtility.SetDirty(instance);
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		private float proceduralVariableWidth=0;
		private float DrawProceduralVariable(float startX, float startY, ProceduralVariable variable, GUIContent cont=null){
			//startX+=2; 	startY+=2;
			spaceX-=35;
			
			if(cont==null) cont=new GUIContent("");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont); //startY-=1;
			
			cont=new GUIContent(" - Start Value:", "Override the value of default MoveSpeed set in CreepEditor. Only valid if value is set to >0");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			variable.startValue=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), variable.startValue);
			
			cont=new GUIContent(" - Increment:", "Override the value of default MoveSpeed set in CreepEditor. Only valid if value is set to >0");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			variable.incMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), variable.incMultiplier);
			
			cont=new GUIContent(" - Deviation:", "Override the value of default MoveSpeed set in CreepEditor. Only valid if value is set to >0");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			variable.devMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), variable.devMultiplier);
			
			cont=new GUIContent(" - Min/Max:", "Override the value of default MoveSpeed set in CreepEditor. Only valid if value is set to >0");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			variable.minValue=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), variable.minValue);
			variable.maxValue=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), variable.maxValue);
			
			spaceX+=35;
			
			return startY;
		}
		
		private bool showProceduralPathList=false;
		private float DrawProceduralSetting(float startX, float startY){
			WaveGenerator waveGen=instance.waveGenerator;
			
			float cachedY=startY;
			float cachedX=startX;
			
			cont=new GUIContent("Sub Wave Count:");
			startY=DrawProceduralVariable(startX, cachedY, waveGen.subWaveCount, cont);
			cont=new GUIContent("Total Creep Count:");
			startY=DrawProceduralVariable(startX+=proceduralVariableWidth+20, cachedY, waveGen.unitCount, cont);
			
			float alignY=startY+=spaceY;	startY=cachedY;
			startX+=proceduralVariableWidth+40;
			
			
			cont=new GUIContent("Wave interval:", "Time (in second) between two wave");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			if(instance.spawnMode==SpawnManager._SpawnMode.Continous) {
				waveGen.waveSpacingTimeMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), waveGen.waveSpacingTimeMin);
				waveGen.waveSpacingTimeMax=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), waveGen.waveSpacingTimeMax);
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			
			cont=new GUIContent("SimilarSubwave:", "Check to have identical subwave for each wave");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
			waveGen.similarSubWave=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), waveGen.similarSubWave);
			
			
			startY+=5;
			
			cont=new GUIContent("Utilise All Path:", "Check to have the generator to use all the path when possible, by assigning different path to each subwave (when there's more path than subwave)");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			waveGen.utiliseAllPath=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), waveGen.utiliseAllPath);
			
			showProceduralPathList=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, widthS, height), showProceduralPathList, "Path List "+(showProceduralPathList ? "" : "("+waveGen.pathList.Count+")"));
			if(showProceduralPathList){
				int count=waveGen.pathList.Count;
				count=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), count);
				while(count<waveGen.pathList.Count) waveGen.pathList.RemoveAt(waveGen.pathList.Count-1);
				while(count>waveGen.pathList.Count) waveGen.pathList.Add(null);
				
				for(int i=0; i<waveGen.pathList.Count; i++){
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
					waveGen.pathList[i]=(PathTD)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), waveGen.pathList[i], typeof(PathTD), true);
				}
				startY+=spaceY;
			}
			
			startX=cachedX;
			cachedY=rscDB.rscList.Count>2 ? Mathf.Max(startY, alignY) : alignY;
			
			while(waveGen.rscSettingList.Count<rscDB.rscList.Count) waveGen.rscSettingList.Add(new ProceduralVariable(0, 0));
			while(waveGen.rscSettingList.Count>rscDB.rscList.Count) waveGen.rscSettingList.RemoveAt(waveGen.rscSettingList.Count-1);
			
			for(int i=0; i<rscDB.rscList.Count; i++){
				startY=cachedY+10;
				TDEditor.DrawSprite(new Rect(startX, startY-2, 20, 20), rscDB.rscList[i].icon);
				cont=new GUIContent("      "+rscDB.rscList[i].name+":");
				startY=DrawProceduralVariable(startX, startY, waveGen.rscSettingList[i], cont);
				startX+=proceduralVariableWidth+20;
			}
			
			
			startX=cachedX;	startY+=spaceY;
			
			for(int i=0; i<waveGen.unitSettingList.Count; i++){
				ProceduralUnitSetting unitSetting=waveGen.unitSettingList[i];
				startY=DrawProceduralUnitSetting(startX, startY+12, unitSetting);
			}
			
			return startY;
		}
		
		private float proUnitItemWidth=0;
		private float proUnitItemHeightShow=0;
		private float proUnitItemHeightHide=0;
		private float DrawProceduralUnitSetting(float startX, float startY, ProceduralUnitSetting unitSetting){
			if(unitSetting.enabled) GUI.Box(new Rect(startX, startY, 185*4, proUnitItemHeightShow), "");
			else GUI.Box(new Rect(startX, startY, 185*4, proUnitItemHeightHide), "");
			
			startX+=5; 	startY+=5;
			
			float cachedX=startX;
			float cachedY=startY;
			
				unitSetting.unitC=unitSetting.unit.GetComponent<UnitCreep>();
			
				TDEditor.DrawSprite(new Rect(startX, startY, 30, 30), unitSetting.unitC.iconSprite);
				EditorGUI.LabelField(new Rect(startX+32, startY, width, height), unitSetting.unitC.unitName);
				
				cont=new GUIContent("enabled: ", "Check to enable unit in the procedural generation otherwise unit will not be considered at all");
				EditorGUI.LabelField(new Rect(startX+32, startY+spaceY-3, width, height), cont);
				unitSetting.enabled=EditorGUI.Toggle(new Rect(startX+32+60, startY+spaceY-3, width, height), unitSetting.enabled);
			
			if(!unitSetting.enabled){
				proUnitItemHeightHide=startY+40-cachedY;
				return startY+35;
			}
			
				cont=new GUIContent("Min Wave:", "The minimum wave in which the creep will start appear in");
				EditorGUI.LabelField(new Rect(startX+=185, startY+5, width, height), cont);
				unitSetting.minWave=EditorGUI.IntField(new Rect(startX+70, startY+5, 40, height), unitSetting.minWave);
				
				EditorGUI.LabelField(new Rect(cachedX, startY+=24, 185*4-10, height), "______________________________________________________________________________________________________________________");
			
			startY+=spaceY;		startX=cachedX;
			
				cont=new GUIContent("HitPoint (HP):");
				DrawProceduralVariable(startX, startY, unitSetting.HP, cont);
				
				cont=new GUIContent("Shield:");
				DrawProceduralVariable(startX+=proceduralVariableWidth+20, startY, unitSetting.shield, cont);
				
				cont=new GUIContent("Move Speed:");
				DrawProceduralVariable(startX+=proceduralVariableWidth+20, startY, unitSetting.speed, cont);
				
				cont=new GUIContent("Spawn Interval:");
				startY=DrawProceduralVariable(startX+=proceduralVariableWidth+20, startY, unitSetting.interval, cont);
			
			proUnitItemWidth=startX+proceduralVariableWidth+20;
			proUnitItemHeightShow=(startY+spaceY+8)-cachedY;
			
			return startY+spaceY;
		}
		
		
		
		private int maxSubWaveSize=1;
		private float DrawSpawnInfo(float startX, float startY){
			
			maxSubWaveSize=1;
			
			while(waveFoldList.Count<instance.waveList.Count) waveFoldList.Add(true);
			while(waveFoldList.Count>instance.waveList.Count) waveFoldList.RemoveAt(waveFoldList.Count-1);
			
			startY+=5;
			
			for(int i=0; i<instance.waveList.Count; i++){
				Wave wave=instance.waveList[i];
				
				if(deleteID==i){
					if(GUI.Button(new Rect(startX, startY, 60, 20), "Cancel")) deleteID=-1;
					
					GUI.color=Color.red;
					if(GUI.Button(new Rect(startX+65, startY, 20, 20), "X")){
						instance.waveList.RemoveAt(i);	i-=1;
						deleteID=-1;
					}
					GUI.color=Color.white;
				}
				else{
					cont=new GUIContent("X", "Delete wave");
					if(GUI.Button(new Rect(startX, startY, 20, 20), cont)) deleteID=i;
				}
				
				
				float offsetX=deleteID==i ? 60 : 0 ;
				waveFoldList[i]=EditorGUI.Foldout(new Rect(startX+25+offsetX, startY+3, width, height), waveFoldList[i], "wave "+i, foldoutStyle);
				if(!waveFoldList[i]){	//preview
					DrawSubWavePreview(startX+120, startY-5, wave);
				}
				else{						//details
					startX+=25;	startY+=3;
					
					cont=new GUIContent("SubWave Size: "+wave.subWaveList.Count, "Number of sub waves in the level");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(GUI.Button(new Rect(startX+spaceX, startY, widthS, height), "-1"))
						if(wave.subWaveList.Count>1) wave.subWaveList.RemoveAt(wave.subWaveList.Count-1);
					if(GUI.Button(new Rect(startX+spaceX+50, startY, widthS, height), "+1"))
						wave.subWaveList.Add(new SubWave());
					
					
					startY=DrawSubWave(startX, startY+spaceY+5, wave)+8;
					
					
					cont=new GUIContent("Time To Next Wave: ", "Time until next wave");
					EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
					if(instance.spawnMode==SpawnManager._SpawnMode.Continous) 
						wave.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), wave.duration);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					
					
					float reqDuration=wave.CalculateSpawnDuration();
					EditorGUI.LabelField(new Rect(startX+spaceX+50, startY, 500, height), "(Time to spawn all units: "+reqDuration.ToString("f1")+"s)");
					
					
					cont=new GUIContent("Resource Gain:", "The amount of resource player will gain when surviving the wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, width, height), cont);
					
					if(wave.rscGainList.Count<rscDB.rscList.Count) wave.rscGainList.Add(0);
					if(wave.rscGainList.Count>rscDB.rscList.Count) wave.rscGainList.RemoveAt(wave.rscGainList.Count-1);
					
					float cachedX=startX;	startX+=spaceX;
					for(int n=0; n<rscDB.rscList.Count; n++){
						TDEditor.DrawSprite(new Rect(startX, startY-2, 20, 20), rscDB.rscList[n].icon);
						wave.rscGainList[n]=EditorGUI.IntField(new Rect(startX+20, startY, widthS, height-2), wave.rscGainList[n]);
						startX+=75;
					}
					startX=cachedX;
					
					cont=new GUIContent("Life Gained: ", "The amount of life player will gain when surviving the wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					wave.lifeGain=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), wave.lifeGain);
					
					cont=new GUIContent("Energy Gained: ", "The amount of energy (for abilities) player will gain when surviving the wave");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					wave.energyGain=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), wave.energyGain);
					
					startX-=25;
				}
				
				startY+=spaceY*2;
			}
			
			return startY;
		}
		
		
		private void DrawSubWavePreview(float startX, float startY, Wave wave){
			for(int i=0; i<wave.subWaveList.Count; i++){
				SubWave subWave=wave.subWaveList[i];
				
				Sprite icon=subWave.unitC==null ? null : subWave.unitC.iconSprite;
				TDEditor.DrawSprite(new Rect(startX, startY, 30, 30), icon);
				EditorGUI.LabelField(new Rect(startX+33, startY+7, 30, 30), "x"+subWave.count);
				
				startX+=70;
			}
		}
		
		
		private float subWaveBoxHeight=0;
		private float subWaveBoxWidth=0;
		private float DrawSubWave(float startX, float startY, Wave wave){
			float cachedY=startY;	spaceX-=20;	
			
			for(int i=0; i<wave.subWaveList.Count; i++){
				SubWave subWave=wave.subWaveList[i];
				
				startY=cachedY;	
				
				GUI.Box(new Rect(startX, startY, subWaveBoxWidth, subWaveBoxHeight), "");
				
				startX+=5; startY+=5;
				
					if(subWave.unit!=null && subWave.unitC==null) subWave.unitC=subWave.unit.GetComponent<UnitCreep>();
					if(subWave.unitC!=null && subWave.unit!=subWave.unitC.gameObject) subWave.unit=subWave.unitC.gameObject;
					
					Sprite icon=subWave.unitC==null ? null : subWave.unitC.iconSprite;
					TDEditor.DrawSprite(new Rect(startX, startY, 30, 30), icon);
					
					int index=subWave.unitC!=null ? TDEditor.GetCreepIndex(subWave.unitC.prefabID) : 0 ;
					cont=new GUIContent("Creep Prefab:", "The creep prefab to be spawned");
					EditorGUI.LabelField(new Rect(startX+32, startY, width, height), cont);
					index=EditorGUI.Popup(new Rect(startX+32, startY+spaceY-4, width, height), index, creepLabel);
					if(index>0) subWave.unitC=creepDB.creepList[index-1];
					else subWave.unitC=null;
				
				startY+=35;
				
					cont=new GUIContent("Number of Unit:", "Number of unit to be spawned");
					EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
					subWave.count=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), subWave.count);

					cont=new GUIContent("Start Delay:", "Time delay before the first creep of this subwave start spawn");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					subWave.delay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.delay);
					
					cont=new GUIContent("Spawn Interval:", "The time interval in second between each single individual spawned");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					subWave.interval=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.interval);
					
					cont=new GUIContent("Alternate Path:", "The path to use for this subwave, if it's different from the default path. Optional and can be left blank");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					subWave.path=(PathTD)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, widthS+40, height), subWave.path, typeof(PathTD), true);
				
				startY+=5;
				
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Override:");
					
					GUI.color=subWave.overrideHP>=0 ? Color.white : Color.grey ;
					cont=new GUIContent(" - HP:", "Override the value of default HP set in CreepEditor. Only valid if value is set to >0");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					subWave.overrideHP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.overrideHP);
					
					GUI.color=subWave.overrideShield>=0 ? Color.white : Color.grey ;
					cont=new GUIContent(" - Shield:", "Override the value of default shield set in CreepEditor. Only valid if value is set to >0");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					subWave.overrideShield=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.overrideShield);
									
					GUI.color=subWave.overrideMoveSpd>=0 ? Color.white : Color.grey ;
					cont=new GUIContent(" - Move Speed:", "Override the value of default MoveSpeed set in CreepEditor. Only valid if value is set to >0");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					subWave.overrideMoveSpd=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), subWave.overrideMoveSpd);
				
				GUI.color=Color.white;
				
				startX+=subWaveBoxWidth+10;
				
				subWaveBoxHeight=startY+spaceY+5-cachedY;
			}
			
			spaceX+=20;
			
			return cachedY+subWaveBoxHeight;
		}
		
		
		private float DrawGeneralSetting(float startX, float startY){
			spaceX-=20;
			
				int spawnMode=(int)instance.spawnMode;
				cont=new GUIContent("Spawn Mode:", "Spawn mode in this level");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				contL=new GUIContent[spawnModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(spawnModeLabel[i], spawnModeTooltip[i]);
				spawnMode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), spawnMode, contL);
				instance.spawnMode=(SpawnManager._SpawnMode)spawnMode;
				
				if(instance.spawnMode!=SpawnManager._SpawnMode.Round){
					cont=new GUIContent(" - Allow Skip", "Allow player to skip ahead and spawn the next wave");
					instance.allowSkip=EditorGUI.ToggleLeft(new Rect(startX+spaceX+width+10, startY, width, 15), cont, instance.allowSkip);
				}
				
				int spawnLimit=(int)instance.spawnLimit;
				cont=new GUIContent("Spawn Limit:", "Spawn limit in this level. Infinite (endless mode) must use procedural wave generation");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				cont=new GUIContent("", "");
				contL=new GUIContent[spawnLimitLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(spawnLimitLabel[i], spawnLimitTooltip[i]);
				spawnLimit = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), cont, spawnLimit, contL);
				instance.spawnLimit=(SpawnManager._SpawnLimit)spawnLimit;
				
				if(instance.spawnLimit==SpawnManager._SpawnLimit.Infinite) configureProceduralSetting=false;
				
				cont=new GUIContent("Auto Start: ", "Check to have the spawning start on a fixed timer. Rather than waiting for player initiation");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				instance.autoStart=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), instance.autoStart);
				
				cont=new GUIContent(" - Start Timer: ", "The duration to wait in second before the spawning start");
				EditorGUI.LabelField(new Rect(startX+spaceX+20, startY, width, height), cont);
				if(!instance.autoStart) EditorGUI.LabelField(new Rect(startX+spaceX+110, startY, width, height), "-");
				else instance.autoStartDelay=EditorGUI.FloatField(new Rect(startX+spaceX+110, startY, widthS, height), instance.autoStartDelay);
			
			startY+=8;
				
				
				cont=new GUIContent("Auto Generate", "Check to have the SpawnManager automatically generate the wave in runtime as opposed to using preset data");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+50, height), cont);
				if(instance.spawnLimit==SpawnManager._SpawnLimit.Finite)
					instance.procedurallyGenerateWave=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), instance.procedurallyGenerateWave);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				
				cont=new GUIContent("Default Path:", "The primary path to be used. Every creep will follow this path unless an alternate path is specified in a sub-wave");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(instance.spawnLimit==SpawnManager._SpawnLimit.Finite)
					instance.defaultPath=(PathTD)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), instance.defaultPath, typeof(PathTD), true);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				
				cont=new GUIContent("Waves Size: "+instance.waveList.Count, "Number of waves in the level");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, 15), cont);
				if(instance.spawnLimit==SpawnManager._SpawnLimit.Finite){
					if(GUI.Button(new Rect(startX+spaceX, startY-1, widthS, 15), "-1"))
						if(instance.waveList.Count>1) instance.waveList.RemoveAt(instance.waveList.Count-1);
					if(GUI.Button(new Rect(startX+spaceX+50, startY-1, widthS, 15), "+1"))
						instance.waveList.Add(NewWave());
				}
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				
			spaceX+=20;
			
			return startY+spaceY;
		}
		
		
		
		private Wave NewWave(){
			Wave wave=new Wave();
			for(int i=0; i<rscDB.rscList.Count; i++) wave.rscGainList.Add(0);
			return wave;
		}
		
		private bool GetSpawnManager(){
			instance=(SpawnManager)FindObjectOfType(typeof(SpawnManager));
			return instance==null ? false : true ;
		}
		
		
		private int unitCount=0;
		private void UpdateProceduralUnitList(){
			if(unitCount==creepDB.creepList.Count) return;
			
			unitCount=creepDB.creepList.Count;
			
			List<ProceduralUnitSetting> unitSettingList=instance.waveGenerator.unitSettingList;
			List<ProceduralUnitSetting> newSettingList=new List<ProceduralUnitSetting>();
			for(int i=0; i<creepDB.creepList.Count; i++){
				bool match=false;
				for(int n=0; n<unitSettingList.Count; n++){
					if(unitSettingList[n].unit==creepDB.creepList[i].gameObject){
						newSettingList.Add(unitSettingList[n]);
						match=true;
						break;
					}
				}
				if(!match){
					ProceduralUnitSetting unitSetting=new ProceduralUnitSetting();
					unitSetting.unit=creepDB.creepList[i].gameObject;
					unitSetting.unitC=creepDB.creepList[i].gameObject.GetComponent<UnitCreep>();
					newSettingList.Add(unitSetting);
				}
			}
			instance.waveGenerator.unitSettingList=newSettingList;
		}
				
	}
}
