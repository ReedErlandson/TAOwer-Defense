using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class PerkEditorWindow : TDEditorWindow {
		
		private static PerkEditorWindow window;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (PerkEditorWindow)EditorWindow.GetWindow(typeof (PerkEditorWindow), false, "Perk Editor");
			window.minSize=new Vector2(400, 300);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
		}

		private static string[] perkTypeLabel;
		private static string[] perkTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_PerkType)).Length;
			perkTypeLabel=new string[enumLength];
			perkTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				perkTypeLabel[i]=((_PerkType)i).ToString();
				
				if((_PerkType)i==_PerkType.NewTower) 	perkTypeTooltip[i]="unlock a new buildable tower";
				if((_PerkType)i==_PerkType.NewAbility) 	perkTypeTooltip[i]="unlock a new ability";
				if((_PerkType)i==_PerkType.NewFPSWeapon) 	perkTypeTooltip[i]="unlock a new weapon in fps mode";
				
				if((_PerkType)i==_PerkType.GainLife) 	perkTypeTooltip[i]="grant player more life immediately";
				if((_PerkType)i==_PerkType.LifeCap) 	perkTypeTooltip[i]="modify player's life capacity";
				if((_PerkType)i==_PerkType.LifeRegen) 	perkTypeTooltip[i]="increase/modify player's life regeneration";
				if((_PerkType)i==_PerkType.LifeWaveClearedBonus) perkTypeTooltip[i]="increase/modify the amount of life player gains from clearing a wave";
				
				if((_PerkType)i==_PerkType.GainRsc) 	perkTypeTooltip[i]="grant player more resources immediately";
				if((_PerkType)i==_PerkType.RscRegen) 	perkTypeTooltip[i]="increase/modify player resources generation";
				if((_PerkType)i==_PerkType.RscGain) 	perkTypeTooltip[i]="increase/modify the resource gain multiplier (applies whenever player gain resources)";
				if((_PerkType)i==_PerkType.RscCreepKilledGain) 		perkTypeTooltip[i]="increase/modify the resource player gained directly from destroying creeps";
				if((_PerkType)i==_PerkType.RscWaveClearedGain) 	perkTypeTooltip[i]="increase/modify the resource player gained directly from clearing a wave";
				if((_PerkType)i==_PerkType.RscResourceTowerGain) perkTypeTooltip[i]="increase/modify the resource player gained directly from a resource towers";
				
				if((_PerkType)i==_PerkType.Tower) perkTypeTooltip[i]="modify attributes/properties of all towers";
				if((_PerkType)i==_PerkType.TowerSpecific) perkTypeTooltip[i]="modify attributes/properties of a certain tower(s)";
				if((_PerkType)i==_PerkType.Ability) perkTypeTooltip[i]="modify attributes/properties of all abilities";
				if((_PerkType)i==_PerkType.AbilitySpecific) perkTypeTooltip[i]="modify attributes/properties of a certain ability(s)";
				if((_PerkType)i==_PerkType.FPSWeapon) perkTypeTooltip[i]="modify attributes/properties of all weapons in fps mode";
				if((_PerkType)i==_PerkType.FPSWeaponSpecific) perkTypeTooltip[i]="modify attributes/properties of a certain weapon(s) in fps mode";
				
				if((_PerkType)i==_PerkType.EnergyRegen) perkTypeTooltip[i]="increase/modify the energy regeneration rate for abilities system";
				if((_PerkType)i==_PerkType.EnergyIncreaseCap) perkTypeTooltip[i]="increase/modify the energy-pool capacity for abilities system";
				if((_PerkType)i==_PerkType.EnergyCreepKilledBonus) perkTypeTooltip[i]="increase/modify the energy player gained directly from destroying creeps";
				if((_PerkType)i==_PerkType.EnergyWaveClearedBonus) perkTypeTooltip[i]="increase/modify the energy player gained directly from clearing a wave";
			}
		}
		
		
		public void SetupCallback(){
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
		}
		
		
		public override bool OnGUI() {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<Perk> perkList=perkDB.perkList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(abilityDB, "AbilityDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTD();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(perkList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawPerkList(startX, startY, perkList);	
			
			startX=v2.x+25;
			
			if(perkList.Count==0) return true;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				startY=DrawPerkConfigurator(startX, startY, perkList[selectID]);
				contentWidth=360;
				contentHeight=startY-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		
		protected Vector2 DrawPerkList(float startX, float startY, List<Perk> perkList){
			List<Item> list=new List<Item>();
			for(int i=0; i<perkList.Count; i++){
				Item item=new Item(perkList[i].ID, perkList[i].name, perkList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		int NewItem(int cloneID=-1){
			Perk perk=null;
			if(cloneID==-1){
				perk=new Perk();
				perk.name="New Perk";
			}
			else{
				perk=perkDB.perkList[selectID].Clone();
			}
			perk.ID=GenerateNewID(perkIDList);
			perkIDList.Add(perk.ID);
			
			perkDB.perkList.Add(perk);
			
			UpdateLabel_Perk();
			
			return perkDB.perkList.Count-1;
		}
		void DeleteItem(){
			perkIDList.Remove(perkDB.perkList[deleteID].ID);
			perkDB.perkList.RemoveAt(deleteID);
			
			UpdateLabel_Perk();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<perkDB.perkList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Perk perk=perkDB.perkList[selectID];
			perkDB.perkList[selectID]=perkDB.perkList[selectID+dir];
			perkDB.perkList[selectID+dir]=perk;
			selectID+=dir;
		}
		
		
		
		
		
		
		
		
		
		private bool showTypeDesp=false;
		private float DrawPerkConfigurator(float startX, float startY, Perk perk){
			TDEditor.DrawSprite(new Rect(startX, startY, 60, 60), perk.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The perk name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/4, width, height), cont);
			perk.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), perk.name);
			
			cont=new GUIContent("Icon:", "The perk icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), perk.icon, typeof(Sprite), false);
			
			cont=new GUIContent("PerkID:", "The ID used to associate a perk item in perk menu to a perk when configuring perk menu manually");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.LabelField(new Rect(startX+spaceX-65, startY, width-5, height), perk.ID.ToString());
			
			startX-=65;
			startY+=15;//+spaceY-spaceY/2;
			
			//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Perk General Setting", headerStyle);
			
				if(showTypeDesp){
					EditorGUI.HelpBox(new Rect(startX, startY+=spaceY, width+spaceX, 40), perkTypeTooltip[(int)perk.type], MessageType.Info);
					startY+=45-height;
				}
			
				int type=(int)perk.type;
				cont=new GUIContent("Perk Type:", "What the perk does");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, headerStyle);
				contL=new GUIContent[perkTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(perkTypeLabel[i], perkTypeTooltip[i]);
				type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), new GUIContent(""), type, contL);
				perk.type=(_PerkType)type;
				
				showTypeDesp=EditorGUI.ToggleLeft(new Rect(startX+spaceX+width+2, startY, width, 20), "Show Description", showTypeDesp);
			
				
				
				
			startY+=spaceY;
			
			startY=DrawPerkGeneralSetting(startX, startY+spaceY, perk);
			
			startY=DrawPerkStats(startX, startY+spaceY, perk);
			
			
			//startY+=25;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Perk description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY, 400, 20), cont);
			perk.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), perk.desp, style);
			
			return startY+170;
		}
		
		
		
		private bool foldGeneral=true;
		private float DrawPerkGeneralSetting(float startX, float startY, Perk perk){
			string text="General Setting "+(!foldGeneral ? "(show)" : "(hide)");
			foldGeneral=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldGeneral, text, foldoutStyle);
			if(foldGeneral){
				startX+=15;
				
					cont=new GUIContent("Repeatable:", "Check if the ability can be repeatably purchase. For perk that offer straight, one off bonus such as life and resource");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.repeatable=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), perk.repeatable);
					
				startY+=5;
					
					cont=new GUIContent("Prerequisite Perk:", "Perks that needs to be purchased before this perk is unlocked and become available");
					EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont);
					
					for(int i=0; i<perk.prereq.Count+1; i++){
						int index=(i<perk.prereq.Count) ? TDEditor.GetPerkIndex(perk.prereq[i]) : 0;
						index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width, height), index, perkLabel);
						if(index>0){
							int perkID=perkDB.perkList[index-1].ID;
							if(perkID!=perk.ID && !perk.prereq.Contains(perkID)){
								if(i<perk.prereq.Count) perk.prereq[i]=perkID;
								else perk.prereq.Add(perkID);
							}
						}
						else if(i<perk.prereq.Count){ perk.prereq.RemoveAt(i); i-=1; }
					}
					
				startY+=5;
					
					cont=new GUIContent("Min level required:", "Minimum level to reach before the perk becoming available. (level are specified in GameControl of each scene)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.minLevel=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.minLevel);
					
					cont=new GUIContent("Min wave required:", "Minimum wave to reach before the perk becoming available");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.minWave=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.minWave);
					
					cont=new GUIContent("Min PerkPoint req:", "Minimum perk point to have before the perk becoming available");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.minPerkPoint=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.minPerkPoint);
					
				startY+=5;
					
					while(perk.cost.Count<rscDB.rscList.Count) perk.cost.Add(0);
					while(perk.cost.Count>rscDB.rscList.Count) perk.cost.RemoveAt(perk.cost.Count-1);
				
					cont=new GUIContent("Purchase Cost:", "The resource required to build/upgrade to this level");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, width, height), cont);
					int count=0;	startX+=spaceX;		float cachedX2=startX;
					for(int i=0; i<rscDB.rscList.Count; i++){
						TDEditor.DrawSprite(new Rect(startX, startY-1, 20, 20), rscDB.rscList[i].icon);
						perk.cost[i]=EditorGUI.IntField(new Rect(startX+20, startY, 40, height), perk.cost[i]);
						count+=1; 	startX+=75;
						if(count==2){ startY+=spaceY; startX=cachedX2; }
					}
					
			}
				
			return startY+spaceY;
		}
		
		
		
		private bool foldStats=true;
		float DrawPerkStats(float startX, float startY, Perk perk){
			string text="Perk Stats "+(!foldGeneral ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldStats, text, foldoutStyle);
			if(foldStats){
				startX+=15;
				
				_PerkType type=perk.type;
				
				if(type==_PerkType.NewTower){
					cont=new GUIContent("New Tower:", "The tower to add to game when the perk is unlocked");
					startY=DrawItemIDTower(startX, startY+spaceY, perk, cont);
				}
				else if(type==_PerkType.NewAbility){
					cont=new GUIContent("New Ability:", "The ability to add to game when the perk is unlocked");
					startY=DrawItemIDAbility(startX, startY+spaceY, perk, cont);
				}
				else if(type==_PerkType.NewFPSWeapon){
					cont=new GUIContent("New FPS Weapon:", "The fps weapon to add to game when the perk is unlocked");
					startY=DrawItemIDFPSWeapon(startX, startY+spaceY, perk, cont);
				}
				
				else if(type==_PerkType.GainLife){
					GUIContent cont1=new GUIContent(" - Min value:", "Minimum value");
					GUIContent cont2=new GUIContent(" - Max value:", "Maximum value");
					startY=DrawValueMinMax(startX, startY, perk, cont1, cont2);
				}
				else if(type==_PerkType.LifeCap){
					cont=new GUIContent(" - Increase Value:", "value used to modify the existing maximum life capacity");
					startY=DrawValue(startX, startY, perk, cont);
				}
				else if(type==_PerkType.LifeRegen){
					cont=new GUIContent(" - Increase Value:", "value used to modify the existing life regeneration rate");
					startY=DrawValue(startX, startY, perk, cont);
				}
				else if(type==_PerkType.LifeWaveClearedBonus){
					GUIContent cont1=new GUIContent(" - Min value:", "Minimum value");
					GUIContent cont2=new GUIContent(" - Max value:", "Maximum value");
					startY=DrawValueMinMax(startX, startY, perk, cont1, cont2);
				}
				
				else if(IsPerkTypeUsesRsc(type)){
					if(type==_PerkType.GainRsc) cont=new GUIContent(" - Gain:", "The resource to be gain upon purchasing this perk");
					else if(type==_PerkType.RscRegen) cont=new GUIContent(" - Rate modifier:", "The resource to be gain upon purchasing this perk");
					else if(type==_PerkType.RscGain) cont=new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
					else if(type==_PerkType.RscCreepKilledGain) cont=new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
					else if(type==_PerkType.RscWaveClearedGain) cont=new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
					else if(type==_PerkType.RscResourceTowerGain) cont=new GUIContent(" - Gain multiplier:", "The resource to be gain upon purchasing this perk");
					
					while(perk.valueRscList.Count<rscDB.rscList.Count) perk.valueRscList.Add(0);
					while(perk.valueRscList.Count<rscDB.rscList.Count) perk.valueRscList.RemoveAt(perk.valueRscList.Count-1);
					
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					int count=0;	startY+=spaceY; 	float cachedX=startX;
					for(int i=0; i<rscDB.rscList.Count; i++){
						TDEditor.DrawSprite(new Rect(startX+15, startY-1, 20, 20), rscDB.rscList[i].icon);
						perk.valueRscList[i]=EditorGUI.FloatField(new Rect(startX+35, startY, 40, height), perk.valueRscList[i]);
						count+=1; 	startX+=75;
						if(count==3){ startY+=spaceY; startX=cachedX; }
					}
					startX=cachedX;	startY+=spaceY;
				}
			
			
				if(type==_PerkType.Tower || type==_PerkType.TowerSpecific){
					if(type==_PerkType.TowerSpecific){
						cont=new GUIContent("Associated Towers:", "The towers that will gain the associated perk bonus");
						startY=DrawItemIDTower(startX, startY+spaceY, perk, cont, 5)+5-spaceY;	
					}
					startY=DrawTowerStat(startX, startY+spaceY, perk);
				}
				
				if(type==_PerkType.Ability || type==_PerkType.AbilitySpecific){
					if(type==_PerkType.AbilitySpecific){
						cont=new GUIContent("Associated Abilities:", "The abilities that will gain the associated perk bonus");
						startY=DrawItemIDAbility(startX, startY+spaceY, perk, cont, 5)+5-spaceY;	
					}
					startY=DrawAbilityStat(startX, startY+spaceY, perk);
				}
				
				if(type==_PerkType.FPSWeapon || type==_PerkType.FPSWeaponSpecific){
					if(type==_PerkType.FPSWeaponSpecific){
						cont=new GUIContent("Associated Weapons:", "The FPS weapon that will gain the associated perk bonus");
						startY=DrawItemIDFPSWeapon(startX, startY+spaceY, perk, cont, 5)+5-spaceY;	
					}
					startY=DrawFPSWeaponStat(startX, startY+spaceY, perk);
				}
				
				else if(type==_PerkType.EnergyRegen){
					cont=new GUIContent(" - Increase Value:", "value used to modify the existing energy regeneration rate");
					startY=DrawValue(startX, startY, perk, cont);
				}
				else if(type==_PerkType.EnergyIncreaseCap){
					cont=new GUIContent(" - Increase Value:", "value used to modify the existing maximum energy capacity");
					startY=DrawValue(startX, startY, perk, cont);
				}
				else if(type==_PerkType.EnergyCreepKilledBonus){
					GUIContent cont1=new GUIContent(" - Min value:", "Minimum value");
					GUIContent cont2=new GUIContent(" - Max value:", "Maximum value");
					startY=DrawValueMinMax(startX, startY, perk, cont1, cont2);
				}
				else if(type==_PerkType.EnergyWaveClearedBonus){
					GUIContent cont1=new GUIContent(" - Min value:", "Minimum value");
					GUIContent cont2=new GUIContent(" - Max value:", "Maximum value");
					startY=DrawValueMinMax(startX, startY, perk, cont1, cont2);
				}
			}
			else startY+=spaceY;
			
			return startY+spaceY;
		}
		
		
		private float DrawItemIDTower(float startX, float startY, Perk perk, GUIContent cont=null, int limit=1){
			return DrawItemID(startX, startY, perk, 0, cont, limit);
		}
		private float DrawItemIDAbility(float startX, float startY, Perk perk, GUIContent cont=null, int limit=1){
			return DrawItemID(startX, startY, perk, 1, cont, limit);
		}
		private float DrawItemIDFPSWeapon(float startX, float startY, Perk perk, GUIContent cont=null, int limit=1){
			return DrawItemID(startX, startY, perk, 2, cont, limit);
		}
		private float DrawItemID(float startX, float startY, Perk perk, int type=0, GUIContent cont=null, int limit=1){
			while(perk.itemIDList.Count>limit) perk.itemIDList.RemoveAt(perk.itemIDList.Count-1);
			
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			
			string[] labelList=null;
			if(type==0) labelList=towerLabel;
			else if(type==1) labelList=abilityLabel;
			else if(type==2) labelList=fpsWeaponLabel;
			
			for(int i=0; i<Mathf.Min(perk.itemIDList.Count+1, limit); i++){
				int index=0;
				if(type==0) index=i<perk.itemIDList.Count ? TDEditor.GetTowerIndex(perk.itemIDList[i]) : 0 ;
				else if(type==1) index=i<perk.itemIDList.Count ? TDEditor.GetAbilityIndex(perk.itemIDList[i]) : 0 ;
				else if(type==2) index=i<perk.itemIDList.Count ? TDEditor.GetFPSWeaponIndex(perk.itemIDList[i]) : 0 ;
				
				if(i>0) startY+=spaceY;
				EditorGUI.LabelField(new Rect(startX+spaceX-10, startY, width, height), "-");
				index = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, 15), index, labelList);
				
				if(index>0){
					int ID=0;
					if(type==0) ID=towerDB.towerList[index-1].prefabID;
					else if(type==1) ID=abilityDB.abilityList[index-1].ID;
					else if(type==2) ID=fpsWeaponDB.weaponList[index-1].prefabID;
					
					if(!perk.itemIDList.Contains(ID)){
						if(i<perk.itemIDList.Count) perk.itemIDList[i]=ID;
						else perk.itemIDList.Add(ID);
					}
				}
				else if(i<perk.itemIDList.Count){
					perk.itemIDList.RemoveAt(i);
					i-=1;
				}
			}
			
			return startY+spaceY;
		}
		
		
		float DrawValue(float startX, float startY, Perk perk, GUIContent cont=null){
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.value);
			
			return startY+spaceY;
		}
		
		float DrawValueMinMax(float startX, float startY, Perk perk, GUIContent cont1=null, GUIContent cont2=null){
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont1);
			perk.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.value);
				
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont2);
			perk.valueAlt=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), perk.valueAlt);
			
			return startY+spaceY;
		}
		
		
		private bool foldGeneralParameter=true;
		float DrawTowerStat(float startX, float startY, Perk perk){
			
			foldGeneralParameter=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldGeneralParameter, "Show General Stats", foldoutStyle);
			
			if(foldGeneralParameter){
				startX+=15;
				
				cont=new GUIContent("HP:", "HP multiplier of the tower. Takes value from 0 and above with 0.2 being 20% increment");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.HP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.HP);
				cont=new GUIContent("HP Regen:", "HP rgeneration multiplier of the tower. Takes value from 0 and above with 0.2 being 20% increment");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.HPRegen=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.HPRegen);
				cont=new GUIContent("HP Stagger:", "HP stagger duration multiplier of the tower. Takes value from 0-1 with 0.2 being 20% decrease in duration");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.HPStagger=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.HPStagger);
				
				cont=new GUIContent("Shield:", "Shield multiplier of the tower. Takes value from 0 and above with 0.2 being 20% increment");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.shield=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.shield);
				cont=new GUIContent("Shield Regen:", "Shield rgeneration multiplier of the tower. Takes value from 0 and above with 0.2 being 20% increment");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.shieldRegen=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.shieldRegen);
				cont=new GUIContent("Shield Stagger:", "Shield stagger duration multiplier of the tower. Takes value from 0-1 with 0.2 being 20% decrease in duration");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.shieldStagger=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.shieldStagger);
				
				cont=new GUIContent("Build Cost:", "Build cost multiplier of the tower. Takes value from 0-1 with 0.2 being 20% decrease in cost");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.buildCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.buildCost);
				cont=new GUIContent("Upgrade Cost:", "Upgrade cost multiplier of the tower. Takes value from 0-1 with 0.2 being 20% decrease in cost");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.upgradeCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.upgradeCost);
				
				startX-=15;
			}
			
			startY=DrawUnitStat(startX, startY+spaceY+5, perk.stats, false);
			
			return startY+spaceY;
		}
		
		
		
		float DrawAbilityStat(float startX, float startY, Perk perk){
			cont=new GUIContent("Cost:", "Multiplier to the ability energy cost. Takes value from 0-1 with 0.3 being decrease energy cost by 30%");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			perk.abCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abCost);
			cont=new GUIContent("Cooldown:", "Multiplier to the ability cooldown duration. Takes value from 0-1 with 0.3 being decrease cooldown duration by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.abCooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abCooldown);
			cont=new GUIContent("AOE Radius:", "Multiplier to the ability AOE radius. Takes value from 0 and above with 0.3 being increment of 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.abAOERadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abAOERadius);
			
			
			startY+=5;
			
			cont=new GUIContent("Duration:", "Duration multiplier. Takes value from 0 and above with 0.3 being increase duration by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.duration);
			perk.effects.dot.duration=perk.effects.duration;
			perk.effects.slow.duration=perk.effects.duration;

			startY+=5;
			
			cont=new GUIContent("Damage:", "Damage multiplier. Takes value from 0 and above with 0.3 being increase existing effect damage by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.damageMin);
			
			cont=new GUIContent("Stun Chance:", "stun chance modifier. Takes value from 0 and above with 0.3 being increase stun chance by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.stunChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.stunChance);
			
			startY+=5;
			
			cont=new GUIContent("Slow", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
			
			cont=new GUIContent("         - Multiplier:", "Move speed multiplier. Takes value from 0-1 with with 0.3 being decrese default speed by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.slow.slowMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.slow.slowMultiplier);
			
			
			startY+=5;
			
			cont=new GUIContent("Dot", "Damage over time");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
			
			cont=new GUIContent("        - Damage:", "Damage multiplier to DOT. Takes value from 0 and above with with 0.3 being increase the tick damage by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.dot.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.dot.value);
			
			startY+=5;
			
			cont=new GUIContent("DamageBuff:", "Damage buff modifer. Takes value from 0 and above with 0.3 being increase existing damage by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.damageBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.damageBuff);
			
			cont=new GUIContent("RangeBuff:", "Range buff modifer. Takes value from 0 and above with 0.3 being increase existing range by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.rangeBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.rangeBuff);
			
			cont=new GUIContent("CDBuff:", "Cooldown buff modifer. Takes value from 0 and above with 0.3 being reduce existing cooldown by 30%");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.cooldownBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.cooldownBuff);
			
			cont=new GUIContent("HPGain:", "HP Gain multiplier. Takes value from 0 and above with 0.3 being increase existing effect HP gain value by 30%.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.effects.HPGainMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.effects.HPGainMin);
			
			
			return startY+spaceY;
		}
		
		
		
		
		float DrawFPSWeaponStat(float startX, float startY, Perk perk){
			startY=DrawUnitStat(startX, startY, perk.stats, true);
			return startY-20;
		}
		
		private bool foldOffenseParameter=true;
		private bool foldSupportParameter=true;
		private bool foldRscParameter=true;
		float DrawUnitStat(float startX, float startY, UnitStat stats, bool isWeapon){
			if(!isWeapon) foldOffenseParameter=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldOffenseParameter, "Show Offensive Stats", foldoutStyle);
			if(isWeapon || foldOffenseParameter){
				if(!isWeapon){
					startX+=15;
					startY+=spaceY;
				}
					
					cont=new GUIContent("Damage:", "");
					EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
					stats.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.damageMin);
				
				startY+=5;	
				
					cont=new GUIContent("Cooldown:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.cooldown);
				
					if(isWeapon){
						cont=new GUIContent("Clip Size:", "");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						stats.clipSize=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.clipSize);
						
						cont=new GUIContent("Reload Duration:", "");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						stats.reloadDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.reloadDuration);
					}
					
				startY+=5;	
				
					if(!isWeapon){
						cont=new GUIContent("Range:", "");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						stats.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.range);
					}
				
					cont=new GUIContent("AOE Radius:", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.aoeRadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.aoeRadius);
				
				startY+=5;	
					
					if(!isWeapon){
						cont=new GUIContent("Hit Modifier:", "");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						stats.hit=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.hit);
						
						cont=new GUIContent("Dodge Modifier:", "");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					}
				
				startY+=5;	
				
					cont=new GUIContent("Stun", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
					
					cont=new GUIContent("        - Chance:", "Chance to stun the target in each successful attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.stun.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.stun.chance);
					
					cont=new GUIContent("        - Duration:", "The stun duration in second");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.stun.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.stun.duration);
				
				startY+=5;	
				
					cont=new GUIContent("Critical", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
					
					cont=new GUIContent("            - Chance:", "Chance to score critical hit in attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.crit.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.crit.chance);
					
					cont=new GUIContent("            - Multiplier:", "Damage multiplier for successful critical hit. Takes value from 0 and above with with 0.5 being 50% of normal damage as bonus");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.crit.dmgMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.crit.dmgMultiplier);
				
				startY+=5;	
				
					cont=new GUIContent("Slow", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
					
					cont=new GUIContent("         - Duration:", "The effect duration in second");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.slow.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.slow.duration);
					
					cont=new GUIContent("         - Multiplier:", "Move speed multiplier. Takes value from 0-1 with with 0.7 being decrese default speed by 30%");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.slow.slowMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.slow.slowMultiplier);
					
				startY+=5;	
					
					cont=new GUIContent("Dot", "Damage over time");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
					
					cont=new GUIContent("        - Duration:", "The effect duration in second");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.dot.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.dot.duration);
					
					cont=new GUIContent("        - Interval:", "Duration between each tick. Damage is applied at each tick.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.dot.interval=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.dot.interval);
					
					cont=new GUIContent("        - Damage:", "Damage applied at each tick");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.dot.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.dot.value);
					
				startY+=5;		
					
					cont=new GUIContent("InstantKill", "");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
					
					cont=new GUIContent("               - Chance:", "The chance to instant kill the target. Takes value from 0-1 with 0 being 0% and 1 being 100%");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.instantKill.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.instantKill.chance);
					
					cont=new GUIContent("       - HP Threshold:", "The HP threshold of the target in order for the instantKill to become valid. Take value from 0-1 with 0.3 being 30% of the fullHP.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.instantKill.HPThreshold=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.instantKill.HPThreshold);
				
				startY+=5;	
					
					cont=new GUIContent("Shield Break:", "The chance of the unit's attack to damage target's shield and disable shield regen permenantly\nTakes value from 0-1 with 0 being 0% and 1 being 100%");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.shieldBreak=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.shieldBreak);
					
					cont=new GUIContent("Shield Pierce:", "The chance of the unit's attack to bypass target's shield and damage HP directly\nTakes value from 0-1 with 0 being 0% and 1 being 100%");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					stats.shieldPierce=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.shieldPierce);
					
				startX-=15;
			}
			
			startY+=10;
			
			if(!isWeapon) foldSupportParameter=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), foldSupportParameter, "Show Support Stats", foldoutStyle);
			if(!isWeapon && foldSupportParameter){
				startX+=15;
				
				cont=new GUIContent("Buff:", "Note: Buffs from multple tower doesnt stack, however when there's difference in the buff strength, the stronger buff applies. A tower can gain maximum dmage buff from one source and maximum range buff from another");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("        - Damage:", "Damage buff multiplier. Takes value from 0 and above with 0.5 being 50% increase in damage");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.damageBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.damageBuff);
				
				cont=new GUIContent("        - Cooldown:", "Dooldown buff multiplier. Takes value from 0-1 with 0.2 being reduce cooldown by 20%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.cooldownBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.cooldownBuff);
				
				cont=new GUIContent("        - Range:", "Range buff multiplier. Takes value from 0 and above with 0.5 being 50% increase in range");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.rangeBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.rangeBuff);
				
				cont=new GUIContent("        - Critical:", "Critical hit chance buff modifier. Takes value from 0 and above with 0.25 being 25% increase in critical hit chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.criticalBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.criticalBuff);
				
				cont=new GUIContent("        - Hit:", "Hit chance buff modifier. Takes value from 0 and above with .2 being 20% increase in hit chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.hitBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.hitBuff);
				
				cont=new GUIContent("        - Dodge:", "Dodge chance buff modifier. Takes value from 0 and above with 0.15 being 15% increase in dodge chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.dodgeBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.dodgeBuff);
				
				cont=new GUIContent("        - HP Regen:", "HP Regeneration Buff. Takes value from 0 and above with 2 being gain 2HP second ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stats.buff.regenHP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stats.buff.regenHP);
				
				startX-=15;
			}
			
			startY+=10;
			
			if(!isWeapon) foldRscParameter=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), foldRscParameter, "Show RscGain", foldoutStyle);
			if(!isWeapon && foldRscParameter){
				startX+=15;
				
				if(stats.rscGain.Count!=rscDB.rscList.Count){
					while(stats.rscGain.Count>rscDB.rscList.Count) stats.rscGain.RemoveAt(stats.rscGain.Count-1);
					while(stats.rscGain.Count<rscDB.rscList.Count) stats.rscGain.Add(0);
				}
				cont=new GUIContent("Resource Gain:", "The resource gain by unit at each cooldown interval\nOnly applicable to ResourceTower");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				int count=0;	startY+=spaceY; 	float cachedX=startX;
				for(int i=0; i<rscDB.rscList.Count; i++){
					TDEditor.DrawSprite(new Rect(startX+10, startY-1, 20, 20), rscDB.rscList[i].icon);
					stats.rscGain[i]=EditorGUI.IntField(new Rect(startX+30, startY, widthS, height), stats.rscGain[i]);
					count+=1; 	startX+=65;
					if(count==3){ startY+=spaceY; startX=cachedX; }
				}
				
				startX-=15;
			}
			
			return startY+spaceY;
		}
		
		
		bool IsPerkTypeUsesRsc(_PerkType type){
			if(type==_PerkType.GainRsc) return true;
			else if(type==_PerkType.RscRegen) return true;
			else if(type==_PerkType.RscGain) return true;
			else if(type==_PerkType.RscCreepKilledGain) return true;
			else if(type==_PerkType.RscWaveClearedGain) return true;
			else if(type==_PerkType.RscResourceTowerGain) return true;
			return false;
		}
		
		
		
	}

}