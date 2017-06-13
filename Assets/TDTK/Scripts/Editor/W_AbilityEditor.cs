using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class AbilityEditorWindow : TDEditorWindow {
		
		private static AbilityEditorWindow window;
		
		public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (AbilityEditorWindow)EditorWindow.GetWindow(typeof (AbilityEditorWindow), false, "Ability Editor");
			window.minSize=new Vector2(400, 300);
			
			LoadDB();
			
			InitLabel();
			
			window.SetupCallback();
		}
		
		private static string[] targetTypeLabel;
		private static string[] targetTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(Ability._TargetType)).Length;
			targetTypeLabel=new string[enumLength];
			targetTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				targetTypeLabel[i]=((Ability._TargetType)i).ToString();
				if((Ability._TargetType)i==Ability._TargetType.Hostile) 	targetTypeTooltip[i]="Ability effect will only be applied to hostile units only";
				if((Ability._TargetType)i==Ability._TargetType.Friendly) targetTypeTooltip[i]="Ability effect will only be applied to friendly units only";
				if((Ability._TargetType)i==Ability._TargetType.Hybrid) 	targetTypeTooltip[i]="Ability effect will only be applied to both hostile and friendly units";
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
			
			List<Ability> abilityList=abilityDB.abilityList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(abilityDB, "AbilityDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTD();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(abilityList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawAbilityList(startX, startY, abilityList);	
			
			startX=v2.x+25;
			
			if(abilityList.Count==0) return true;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				//float cachedX=startX;
				v2=DrawAbilityConfigurator(startX, startY, abilityList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		
		
		private Vector2 DrawAbilityConfigurator(float startX, float startY, Ability ability){
			
			TDEditor.DrawSprite(new Rect(startX, startY, 60, 60), ability.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The ability name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/2, width, height), cont);
			ability.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), ability.name);
			
			cont=new GUIContent("Icon:", "The ability icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), ability.icon, typeof(Sprite), false);
			
			startX-=65;
			startY+=10+spaceY;	//cachedY=startY;
			
				startY=DrawGeneralSetting(startX, startY+spaceY, ability);
				
				startY=DrawVisualSetting(startX, startY+spaceY, ability);
			
				startY=DrawStatsSetting(startX, startY+spaceY, ability);
			
			startY+=10;
			
			
			cont=new GUIContent("Use Custom Description:", "Enable to add your own runtime description to this ability");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.useCustomDesp=EditorGUI.Toggle(new Rect(startX+spaceX+25, startY, 40, height), ability.useCustomDesp);
			
			if(ability.useCustomDesp){
				GUIStyle style=new GUIStyle("TextArea");
				style.wordWrap=true;
				//cont=new GUIContent(" - Description (to be used in runtime): ", "");
				//EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
				ability.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY, 270, 150), ability.desp, style);
				
				startY+=170;
			}
			
			return new Vector2(startX, startY+30);
		}
		
		
		
		private bool foldGeneral=true;
		private float DrawGeneralSetting(float startX, float startY, Ability ability){
			string text="General Setting "+(!foldGeneral ? "(show)" : "(hide)");
			foldGeneral=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldGeneral, text, foldoutStyle);
			if(foldGeneral){
				startX+=15;
				
				cont=new GUIContent("Disable in AbilityManager:", "When checked, ability won't appear on AbilityManager list won't be available to player by default\nThis is intended for ability that only available through unlocking perk");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+5, height), cont);
				ability.disableInAbilityManager=EditorGUI.Toggle(new Rect(startX+spaceX+33, startY, widthS, height), ability.disableInAbilityManager);
				
				startY+=5;
			
				cont=new GUIContent("Cost:", "The energy cost to launch the ability");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.cost=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), ability.cost);
				
				cont=new GUIContent("Cooldown:", "The cooldown in second for the ability before it can be used again after it's used");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.cooldown);
				
				startY+=5;
				
				string tooltip="Check if ability need a specific position or unit as target.";
				tooltip+="\nWhen checked, the user will need to select a position/unit before the ability can be cast.";
				tooltip+="\nIf left uncheck, the ability will be applied to all active units in game.";
				
				cont=new GUIContent("Require Targeting:", text);
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.requireTargetSelection=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), ability.requireTargetSelection);
				
				cont=new GUIContent(" - TargetSingleUnit:", "Check if the ability require a specific unit as a target. Otherwise the ability can be cast anywhere without a specific target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.requireTargetSelection) ability.singleUnitTargeting=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), ability.singleUnitTargeting);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				cont=new GUIContent(" - Target Type:", "Determine which type of unit the tower can target. Hostile for hostile unit. Friendly for friendly unit. Hybrid for both.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.singleUnitTargeting){
					int targetType=(int)ability.targetType;
					contL=new GUIContent[targetTypeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(targetTypeLabel[i], targetTypeTooltip[i]);
					targetType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width-20, height), new GUIContent(""), targetType, contL);
					ability.targetType=(Ability._TargetType)targetType;
				}
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width-20, height), "-");
				
				startX-=15;
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldVisual=true;
		private float DrawVisualSetting(float startX, float startY, Ability ability){
			string text="Visual Effect Setting"+(!foldVisual ? "(show)" : "(hide)");
			foldVisual=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldVisual, text, foldoutStyle);
			if(foldVisual){
				startX+=15;
				
				cont=new GUIContent("Select Indicator:", "(Optional) The cursor indicator that used to indicate the ability target position during target selection phase for the ability. If left unassigned, the default indicator specified in the AbilityManager will be used instead");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.requireTargetSelection) 
					ability.indicator=(Transform)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), ability.indicator, typeof(Transform), false);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				
				startY+=8;
				
				cont=new GUIContent("Cast Visual Effect:", "The visual effect object to be spawned at the target point (optional)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.effectObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), ability.effectObj, typeof(GameObject), false);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the visual effect object needs to be removed from the game");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.effectObj!=null) ability.destroyEffectObj=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), ability.destroyEffectObj);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.effectObj!=null && ability.destroyEffectObj) 
					ability.destroyEffectDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), ability.destroyEffectDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				startX-=15;
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldStats=true;
		private float DrawStatsSetting(float startX, float startY, Ability ability){
			string text="Effect and Stats Setting "+(!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldStats, text, foldoutStyle);
			if(foldStats){
				startX+=15;
				
				cont=new GUIContent("Use Default Effect:", "Check to use default built in ability effects. Alternative you can script your custom effect and have it spawn as the ability's EffectObject");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.useDefaultEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, 40, height), ability.useDefaultEffect);
				
				cont=new GUIContent(" - AOE Radius:", "The Area of Effective radius of the effect. Only target within the radius of the target position will be affected by the ability");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.requireTargetSelection) ability.aoeRadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.aoeRadius);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
				
				cont=new GUIContent(" - Effect Delay:", "The delay in second before the effect actually hit after the ability is cast. This is mostly used to sync-up the visual effect with the effects.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.effectDelay=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), ability.effectDelay);
				
				if(ability.useDefaultEffect){
					startY=DrawAbilityEffect(ability.effect, startX+15, startY+10);
				}
				
				startX-=15;
			}
			
			return startY+spaceY;
		}
		
		
		
		
		
		
		protected Vector2 DrawAbilityList(float startX, float startY, List<Ability> abilityList){
			List<Item> list=new List<Item>();
			for(int i=0; i<abilityList.Count; i++){
				Item item=new Item(abilityList[i].ID, abilityList[i].name, abilityList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		int NewItem(int cloneID=-1){
			Ability ability=null;
			if(cloneID==-1){
				ability=new Ability();
				ability.name="New Ability";
			}
			else{
				ability=abilityDB.abilityList[selectID].Clone();
			}
			ability.ID=GenerateNewID(abilityIDList);
			abilityIDList.Add(ability.ID);
			
			abilityDB.abilityList.Add(ability);
			
			UpdateLabel_Ability();
			
			return abilityDB.abilityList.Count-1;
		}
		void DeleteItem(){
			abilityIDList.Remove(abilityDB.abilityList[deleteID].ID);
			abilityDB.abilityList.RemoveAt(deleteID);
			
			UpdateLabel_Ability();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<abilityDB.abilityList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Ability ability=abilityDB.abilityList[selectID];
			abilityDB.abilityList[selectID]=abilityDB.abilityList[selectID+dir];
			abilityDB.abilityList[selectID+dir]=ability;
			selectID+=dir;
		}
		
		
		
		
		
		
		
		
		
		
		
		private float DrawAbilityEffect(AbilityEffect effect, float startX, float startY){
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Ability Effects", headerStyle);
			
			GUI.Box(new Rect(startX, startY+=spaceY, spaceX+100, 285), "");
			
			startX+=5;
			startY+=10;
			
				cont=new GUIContent("Duration:", "Duration of the effects. This is shared by all the effects that may have a duration (stun, dot, slot, buff)");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				effect.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.duration);
				
				effect.slow.duration=effect.duration;
				effect.dot.duration=effect.duration;
			
			startY+=10;
			
				cont=new GUIContent("Damage Min/Max:", "Damage to be done to target (creep only)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.damageMin);
				effect.damageMax=EditorGUI.FloatField(new Rect(startX+spaceX+42, startY, 40, height), effect.damageMax);
				
				cont=new GUIContent("HP-Gain Min/Max:", "HP to restored to target (tower only)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.HPGainMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.HPGainMin);
				effect.HPGainMax=EditorGUI.FloatField(new Rect(startX+spaceX+42, startY, 40, height), effect.HPGainMax);
			
			startY+=10;
			
				cont=new GUIContent("Stun Chance:", "Chance to stun target (creep only). Takes value from 0-1 with 0.3 being 30% to stun the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.stunChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.stunChance);
			
			startY+=10;
			
				cont=new GUIContent("Slow Multiplier:", "Slow speed multiplier to be applied to target (creep only). Takes value from 0-1 with with 0.7 being decrese default speed by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.slow.slowMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.slow.slowMultiplier);
			
			startY+=10;
			
				cont=new GUIContent("Dot:", "Damage over time to be applied to target (creep only)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=3;
				
				cont=new GUIContent(" - Interval:", "Duration between each tick. Damage is applied at each tick.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.dot.interval=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.dot.interval);
				
				cont=new GUIContent(" - Damage:", "Damage applied at each tick");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.dot.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.dot.value);
			
			startY+=10;
			
				cont=new GUIContent("Buff:", "Buffs to be applied to the target (tower only)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				
				cont=new GUIContent(" - Damage Buff:", "Damage buff multiplier. Takes value from 0 and above with 0.4 being increase damage by 40%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.damageBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.damageBuff);
			
				cont=new GUIContent(" - Range Buff:", "Range buff multiplier. Takes value from 0 and above with 0.3 being increase effective range by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.rangeBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.rangeBuff);
			
				cont=new GUIContent(" - Cooldown Buff:", "Cooldown buff multiplier. Takes value from 0 and above with 0.5 being decrease attack cooldown by 50%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.cooldownBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), effect.cooldownBuff);
			
			return startY;
		}
		
		
		
	}

}