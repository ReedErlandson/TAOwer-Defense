using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class TDUnitEditorWindow : TDEditorWindow {
		
		protected static string[] targetModeLabel;
		protected static string[] targetModeTooltip;
		
		
		private bool foldHitPoint=true;
		protected float DrawUnitBasicStats(float startX, float startY, Unit unit){
				TDEditor.DrawSprite(new Rect(startX, startY, 60, 60), unit.iconSprite);
			
			startX+=65;
			
				cont=new GUIContent("Name:", "The unit name to be displayed in game");
				EditorGUI.LabelField(new Rect(startX, startY+=5, width, height), cont);
				unit.unitName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width, height), unit.unitName);
				
				cont=new GUIContent("Icon:", "The ability icon to be displayed in game, must be a sprite");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.iconSprite=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width, height), unit.iconSprite, typeof(Sprite), false);
				
				cont=new GUIContent("Prefab:", "The prefab object of the unit\nClick this to highlight it in the ProjectTab");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width, height), unit.gameObject, typeof(GameObject), false);
			
			startX-=65;
			startY+=spaceY*2;	
			
			string text="HitPoint and Shield "+(!foldHitPoint ? "(show)" : "(hide)");
			foldHitPoint=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldHitPoint, text, foldoutStyle);
			if(foldHitPoint){
			
				float cachedY=startY+=spaceY;	startX+=15;	
				
					cont=new GUIContent("HitPoint(HP):", "The unit default's HitPoint.\nThis is the base value to be modified by various in game bonus.");
					EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
					unit.defaultHP=EditorGUI.FloatField(new Rect(startX+80, startY, widthS, height), unit.defaultHP);
					
					cont=new GUIContent(" - Regen:", "HP regeneration rate. The amount of HP to be regenerated per second");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.HPRegenRate=EditorGUI.FloatField(new Rect(startX+80, startY, widthS, height), unit.HPRegenRate);
					
					cont=new GUIContent(" - Stagger:", "HP regeneration stagger duration(in second). The duration which the HP regen will be stopped when a unit is hit. Once the duration is passed the HP will start regenerating again");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.HPRegenRate>0) unit.HPStaggerDuration=EditorGUI.FloatField(new Rect(startX+80, startY, widthS, height), unit.HPStaggerDuration);
					else EditorGUI.LabelField(new Rect(startX+80, startY, 40, height), new GUIContent("-", ""));
				
				startX+=140;		startY=cachedY; 	
				
					cont=new GUIContent("Shield:", "The unit default's Shield. Shield can act as a regenerative damage absorber. A unit only be damaged once its shield has been depleted.\nThis is the base value to be modified by various in game bonus.");
					EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
					unit.defaultShield=EditorGUI.FloatField(new Rect(startX+70, startY, widthS, height), unit.defaultShield);
					
					cont=new GUIContent(" - Regen:", "Shield regeneration rate. The amount of shield to be regenerated per second");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.defaultShield>0) unit.shieldRegenRate=EditorGUI.FloatField(new Rect(startX+70, startY, widthS, height), unit.shieldRegenRate);
					else EditorGUI.LabelField(new Rect(startX+70, startY, 40, height), new GUIContent("-", ""));
					
					cont=new GUIContent(" - Stagger:", "Shield regeneration stagger duration(in second). The duration which the shield regen will be stopped when a unit is hit. Once the duration is passed the shield will start regenerating again");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.defaultShield>0 && unit.shieldRegenRate>0)
						unit.shieldStaggerDuration=EditorGUI.FloatField(new Rect(startX+70, startY, widthS, height), unit.shieldStaggerDuration);
					else EditorGUI.LabelField(new Rect(startX+70, startY, 40, height), new GUIContent("-", ""));
				
			}
			
			return startY+spaceY;
		}
		
		private bool foldDefensive=true;
		protected float DrawUnitDefensiveStats(float startX, float startY, Unit unit){
			//EditorGUI.LabelField(new Rect(startX, startY, width, height), "Defensive Setting", headerStyle);
			string text="Defensive Setting "+(!foldDefensive ? "(show)" : "(hide)");
			foldDefensive=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldDefensive, text, foldoutStyle);
			if(foldDefensive){
				startX+=15;
					
					cont=new GUIContent("Armor Type:", "The armor type of the unit\nArmor type can be configured in Damage Armor Table Editor");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.armorType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.armorType, armorTypeLabel);
				
					cont=new GUIContent("Dodge Chance:", "Take value from 0-1. 0 being 0% and 1 being 100%. Final value are subject to target's hitChance. Assume two attackers with 1 hitChance and .8 hitChance and the dodgeChance set to .2, the chances to dodge attack from each attacker are 20% and 40% respectively.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.dodgeChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.dodgeChance);
				
				startY+=8;
				
					int objID=GetObjectIDFromHList(unit.targetPoint, objHList);
					cont=new GUIContent("TargetPoint:", "The transform object which indicate the center point of the unit\nThis would be the point where the shootObject and effect will be aiming at");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.targetPoint = (objHList[objID]==null) ? null : objHList[objID].transform;
					
					cont=new GUIContent("Hit Threshold:", "The range from the targetPoint where a shootObject is considered reached the target");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.hitThreshold=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.hitThreshold);
				
				startY+=8;
				
					cont=new GUIContent("Immuned to Crit:", "Check if the unit is immuned to critical hit");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.immuneToCrit=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.immuneToCrit);
					
					cont=new GUIContent("Immuned to Slow:", "Check if the unit is immuned to slow");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.immuneToSlow=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.immuneToSlow);
					
					cont=new GUIContent("Immuned to Stun:", "Check if the unit is immuned to stun");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.immuneToStun=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.immuneToStun);
				
				startY+=8;
				
					cont=new GUIContent("Destroyed Effect:", "The effect object to be spawned when the unit is destroyed\nThis is entirely optional");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.destroyedEffObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.destroyedEffObj, typeof(GameObject), false);
					
					cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.destroyedEffObj!=null) unit.autoDestroyEff=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.autoDestroyEff);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
					
					cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.destroyedEffObj!=null && unit.autoDestroyEff) 
						unit.destroyEffDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.destroyEffDuration);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
			}
			
			return startY+spaceY;
		}
		
		
		
		private bool foldOffensive=true;
		protected float DrawUnitOffensiveStats(float startX, float startY, Unit unit, bool isTower=true){
			
			//EditorGUI.LabelField(new Rect(startX, startY, width, height), "Offensive Setting", headerStyle);
			string text="Offensive Setting "+(!foldOffensive ? "(show)" : "(hide)");
			foldOffensive=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldOffensive, text, foldoutStyle);
			if(foldOffensive){
				startX+=15;
				
					int objID;
				
					UnitTower tower=isTower ? unit.gameObject.GetComponent<UnitTower>() : null;
					UnitCreep creep=!isTower ? unit.gameObject.GetComponent<UnitCreep>() : null;
				
					//add stop to attack for creep
					if(!isTower){
						cont=new GUIContent("Stop To Attack:", "Check to have the creep stop moving when there's target to attack");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						creep.stopToAttack=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), creep.stopToAttack);
					}
				
					//add target mode for tower
					if(isTower){
						int targetMode=(int)tower.targetMode;
						cont=new GUIContent("Target Mode:", "Determine which type of unit the tower can target");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						contL=new GUIContent[targetModeLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(targetModeLabel[i], targetModeTooltip[i]);
						targetMode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), targetMode, contL);
						tower.targetMode=(_TargetMode)targetMode;
					}
				
					cont=new GUIContent("Damage Type:", "The damage type of the unit\nDamage type can be configured in Damage Armor Table Editor");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.damageType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.damageType, damageTypeLabel);
				
				startY+=8;
				
					cont=new GUIContent("ShootPoint:", "The transform which indicate the position where the shootObject will be fired from (Optional)\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the unit transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the shootObject starting orientation.\n");
					shootPointFoldout=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), shootPointFoldout, cont);
					int shootPointCount=unit.shootPoints.Count;
					shootPointCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), shootPointCount);
					
					if(shootPointCount!=unit.shootPoints.Count){
						while(unit.shootPoints.Count<shootPointCount) unit.shootPoints.Add(null);
						while(unit.shootPoints.Count>shootPointCount) unit.shootPoints.RemoveAt(unit.shootPoints.Count-1);
					}
						
					if(shootPointFoldout){
						for(int i=0; i<unit.shootPoints.Count; i++){
							objID=GetObjectIDFromHList(unit.shootPoints[i], objHList);
							EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
							objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
							unit.shootPoints[i] = (objHList[objID]==null) ? null : objHList[objID].transform;
						}
					}
					
					cont=new GUIContent("Shots delay Between ShootPoint:", "Delay in second between shot fired at each shootPoint. When set to zero all shootPoint fire simulteneously");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+60, height), cont);
					if(unit.shootPoints.Count>1) unit.delayBetweenShootPoint=EditorGUI.FloatField(new Rect(startX+spaceX+90, startY-1, widthS, height-1), unit.delayBetweenShootPoint);
					else EditorGUI.LabelField(new Rect(startX+spaceX+90, startY-1, widthS, height-1), new GUIContent("-", ""));
					
				startY+=8;	
					
					objID=GetObjectIDFromHList(unit.turretObject, objHList);
					cont=new GUIContent("TurretObject:", "The object under unit's hierarchy which is used to aim toward target (Optional). When left unassigned, no aiming will be done.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.turretObject = (objHList[objID]==null) ? null : objHList[objID].transform;
					
					objID=GetObjectIDFromHList(unit.barrelObject, objHList);
					cont=new GUIContent("BarrelObject:", "The object under unit's hierarchy which is used to aim toward target (Optional). This is only required if the unit barrel and turret rotates independently on different axis");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.barrelObject = (objHList[objID]==null) ? null : objHList[objID].transform;
				
				startY+=8;
				
					cont=new GUIContent("Aim Rotate In x-axis:", "Check if the unit turret/barrel can rotate in x-axis (elevation)");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.rotateTurretAimInXAxis=EditorGUI.Toggle(new Rect(startX+spaceX+20, startY, widthS, height), unit.rotateTurretAimInXAxis);
					
					cont=new GUIContent("Directional Targeting:", "Check if the unit should only target hostile unit from a fixed direction");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.directionalTargeting=EditorGUI.Toggle(new Rect(startX+spaceX+20, startY, widthS, height), unit.directionalTargeting);
				
					cont=new GUIContent("- FOV:", "Field-Of-View of the directional targeting");
					EditorGUI.LabelField(new Rect(startX+spaceX+60, startY, width, height), cont);
					if(unit.directionalTargeting) unit.dirScanFOV=EditorGUI.FloatField(new Rect(startX+spaceX+110, startY, widthS, height), unit.dirScanFOV);
					else EditorGUI.LabelField(new Rect(startX+spaceX+110, startY, widthS, height), "-");
					
					if(isTower){
						cont=new GUIContent("- Angle:", "The y-axis angle in clock-wise (from transform local space) which the directional targeting will be aim towards\n0: +ve z-axis\n90: +ve x-axis\n180: -ve z-axis\n270: -ve x-axis");
						EditorGUI.LabelField(new Rect(startX+spaceX+60, startY+=spaceY, width, height), cont);
						if(unit.directionalTargeting) unit.dirScanAngle=EditorGUI.FloatField(new Rect(startX+spaceX+110, startY, widthS, height), unit.dirScanAngle);
						else EditorGUI.LabelField(new Rect(startX+spaceX+110, startY, widthS, height), "-");
					}
					
					//add weapon type for tower
					if(isTower){
						cont=new GUIContent("Disable FPS:", "Check to disable fps mode for this particular tower");
						GUI.Label(new Rect(startX, startY+=spaceY+5, width, height), cont);
						tower.disableFPS=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), tower.disableFPS);
						
						cont=new GUIContent("FPS Weapon:", "Weapon tied to this tower when using FPS mode");
						GUI.Label(new Rect(startX, startY+=spaceY, width, height), cont);
						
						if(!tower.disableFPS){
							int weaponID=TDEditor.GetFPSWeaponIndex(tower.FPSWeaponID);
							weaponID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weaponID, fpsWeaponLabel);
							if(weaponID==0)  tower.FPSWeaponID=-1;
							else tower.FPSWeaponID=fpsWeaponDB.weaponList[weaponID-1].prefabID;
						}
						else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					}
				
			}			
			
			return startY+spaceY;
		}
		
		
		
		
		
		
		
		
		
		
		private float statContentHeight=0;
		protected float DrawUnitStats(float startX, float startY, UnitStat stat, Unit unit, bool isTower=true){
			
			GUI.Box(new Rect(startX, startY, spaceX+2*widthS+10, statContentHeight), "");
			
			float cachedY=startY;
			startX+=5;	startY+=8;		spaceX+=8;	widthS-=5;	//width-=10;
			
			if(isTower){
				startY=DrawUnitStatsTower(startX, startY, stat)+13;
				if(unit.gameObject.GetComponent<UnitTower>().type==_TowerType.Block){
					startX-=5;	startY+=5;	spaceX-=8;	widthS+=5;	//width+=10;
					statContentHeight=startY-cachedY;
					return startY;
				}
			}
			
				startY=DrawShootEffectObject(startX, startY, stat, unit, isTower);
				
			startY+=5;	
				
				cont=new GUIContent("Damage(Min/Max):", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.damageMin);
				stat.damageMax=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), stat.damageMax);
				
				cont=new GUIContent("Cooldown:", "Duration between each attack");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.cooldown);
				
				cont=new GUIContent("Hit Chance:", "Take value from 0-1. 0 being 0% and 1 being 100%. Final value are subject to target's dodgeChance. Assume two targets with 0 dodgeChance and .2 dodgeChance and the hitChance set to 1, the unit will always hits the target and have 20% chance to miss the second target.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.hit=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.hit);
				
			startY+=5;	
				
				cont=new GUIContent("Range:", "Effect range of the unit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.range);
				
				cont=new GUIContent("AOE Radius:", "Area-of-Effective radius. When the shootObject hits it's target, any other hostile unit within the area from the impact position will suffer the same target as the target.\nSet value to >0 to enable. ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.aoeRadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.aoeRadius);
			
			startY+=5;
			
				cont=new GUIContent("Stun", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("        - Chance:", "Chance to stun the target in each successful attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.stun.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.stun.chance);
				
				cont=new GUIContent("        - Duration:", "The stun duration in second");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.stun.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.stun.duration);
				
			startY+=5;	
				
				cont=new GUIContent("Critical", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("            - Chance:", "Chance to score critical hit in attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.crit.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.crit.chance);
				
				cont=new GUIContent("            - Multiplier:", "Damage multiplier for successful critical hit. Takes value from 0 and above with with 0.5 being 50% of normal damage as bonus");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.crit.chance>0) stat.crit.dmgMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.crit.dmgMultiplier);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
			startY+=5;
				
				cont=new GUIContent("Slow", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("         - Duration:", "The effect duration in second");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.slow.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.slow.duration);
				
				cont=new GUIContent("         - Multiplier:", "Move speed multiplier. Takes value from 0-1 with with 0.7 being decrese default speed by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.slow.duration>0) stat.slow.slowMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.slow.slowMultiplier);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
			startY+=5;
				
				cont=new GUIContent("Dot", "Damage over time");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("        - Duration:", "The effect duration in second");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.dot.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.dot.duration);
				
				cont=new GUIContent("        - Interval:", "Duration between each tick. Damage is applied at each tick.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.dot.duration>0) stat.dot.interval=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.dot.interval);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
				stat.dot.interval=Mathf.Clamp(stat.dot.interval, 0, stat.dot.duration);
				
				cont=new GUIContent("        - Damage:", "Damage applied at each tick");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.dot.duration>0) stat.dot.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.dot.value);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
			startY+=5;
				
				cont=new GUIContent("InstantKill", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("                - Chance:", "The chance to instant kill the target. Takes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.instantKill.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.instantKill.chance);
				
				cont=new GUIContent("        - HP Threshold:", "The HP threshold of the target in order for the instantKill to become valid. Take value from 0-1 with 0.3 being 30% of the fullHP.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.instantKill.chance>0) stat.instantKill.HPThreshold=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.instantKill.HPThreshold);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			
			startY+=5;			
				
				cont=new GUIContent("Damage Shield Only:", "When checked, unit will only inflict shield damage");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.damageShieldOnly=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), stat.damageShieldOnly);
				
				cont=new GUIContent("Shield Break:", "The chance of the unit's attack to damage target's shield and disable shield regen permenantly\nTakes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.shieldBreak=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.shieldBreak);
				
				cont=new GUIContent("Shield Pierce:", "The chance of the unit's attack to bypass target's shield and damage HP directly\nTakes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.shieldPierce=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.shieldPierce);
				
			
			startX-=5;	startY+=5;	spaceX-=8;	widthS+=5;	
			statContentHeight=startY+spaceY-cachedY;
			
			return startY+spaceY;
		}
		
		
		protected float DrawUnitStatsSupport(float startX, float startY, UnitStat stat, Unit unit, bool isTower=true){
			GUI.Box(new Rect(startX, startY, spaceX+2*widthS+10, statContentHeight), "");
			
			float cachedY=startY;
			startX+=5;	startY+=8;		spaceX+=8;	widthS-=5;	
			
			if(isTower){
				startY=DrawUnitStatsTower(startX, startY, stat)+13;
			}
			
			startY=DrawShootEffectObject(startX, startY, stat, unit, isTower);
			
			//startY+=5;
				
				cont=new GUIContent(" - Effect Cooldown:", "Duration between each effect-object spawn");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, width, height), cont);
				if(stat.effectObject!=null) stat.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.cooldown);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			
			startY+=8;
			
				cont=new GUIContent("Range:", "Effect range of the unit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.range=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.range);
			
			startY+=5;
				
				cont=new GUIContent("Buff:", "Note: Buffs from multple tower doesnt stack, however when there's difference in the buff strength, the stronger buff applies. A tower can gain maximum dmage buff from one source and maximum range buff from another");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("        - Damage:", "Damage buff multiplier. Takes value from 0 and above with 0.5 being 50% increase in damage");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.damageBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.damageBuff);
				
				cont=new GUIContent("        - Cooldown:", "Dooldown buff multiplier. Takes value from 0-1 with 0.2 being reduce cooldown by 20%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.cooldownBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.cooldownBuff);
				
				cont=new GUIContent("        - Range:", "Range buff multiplier. Takes value from 0 and above with 0.5 being 50% increase in range");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.rangeBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.rangeBuff);
				
				cont=new GUIContent("        - Critical:", "Critical hit chance buff modifier. Takes value from 0 and above with 0.25 being 25% increase in critical hit chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.criticalBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.criticalBuff);
				
				cont=new GUIContent("        - Hit:", "Hit chance buff modifier. Takes value from 0 and above with .2 being 20% increase in hit chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.hitBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.hitBuff);
				
				cont=new GUIContent("        - Dodge:", "Dodge chance buff modifier. Takes value from 0 and above with 0.15 being 15% increase in dodge chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.dodgeBuff=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.dodgeBuff);
				
				cont=new GUIContent("        - HP Regen:", "HP Regeneration Buff. Takes value from 0 and above with 2 being gain 2HP second ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.buff.regenHP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buff.regenHP);
			
			startX-=5;	startY+=5;	spaceX-=8;	widthS+=5;	//width+=10;
			statContentHeight=startY+spaceY-cachedY;
			
			return startY+spaceY;
		}
		
		
		protected float DrawUnitStatsResource(float startX, float startY, UnitStat stat, Unit unit, bool isTower=true){
			GUI.Box(new Rect(startX, startY, spaceX+2*widthS+10, statContentHeight), "");
			
			float cachedY=startY;
			startX+=5;	startY+=8;		spaceX+=8;	widthS-=5;	//width-=10;
			
			if(isTower){
				startY=DrawUnitStatsTower(startX, startY, stat)+8;
			}
			
				startY=DrawShootEffectObject(startX, startY+5, stat, unit, isTower);
			
			startY+=5;
				
				cont=new GUIContent("Cooldown:", "Duration between each attack");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.cooldown);
			
			startY+=5;
			
				if(stat.rscGain.Count!=rscDB.rscList.Count){
					while(stat.rscGain.Count>rscDB.rscList.Count) stat.rscGain.RemoveAt(stat.rscGain.Count-1);
					while(stat.rscGain.Count<rscDB.rscList.Count) stat.rscGain.Add(0);
				}
				
				cont=new GUIContent("Resource Gain:", "The resource gain by unit at each cooldown interval\nOnly applicable to ResourceTower");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				int count=0;	startY+=spaceY; 	float cachedX=startX;
				for(int i=0; i<rscDB.rscList.Count; i++){
					TDEditor.DrawSprite(new Rect(startX+10, startY-1, 20, 20), rscDB.rscList[i].icon);
					stat.rscGain[i]=EditorGUI.IntField(new Rect(startX+30, startY, widthS, height), stat.rscGain[i]);
					count+=1; 	startX+=65;
					if(count==3){ startY+=spaceY; startX=cachedX; }
				}
				startX=cachedX;	startY+=5;
			
			startX-=5;	startY+=5;	spaceX-=8;	widthS+=5;	//width+=10;
			statContentHeight=startY+spaceY-cachedY;
			
			return startY+spaceY;
		}
		
		
		protected float DrawUnitStatsTower(float startX, float startY, UnitStat stat){
				cont=new GUIContent("Construct Duration:", "The time in second it takes to construct (if this is the first level)/upgrade (if this is not the first level)");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				stat.buildDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.buildDuration);
				
				cont=new GUIContent("Deconstruct Duration:", "The time in second it takes to deconstruct if the unit is in this level");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.unBuildDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.unBuildDuration);
			
			startY+=5;	
			
				if(stat.cost.Count!=rscDB.rscList.Count){
					while(stat.cost.Count>rscDB.rscList.Count) stat.cost.RemoveAt(stat.cost.Count-1);
					while(stat.cost.Count<rscDB.rscList.Count) stat.cost.Add(0);
				}
				
				cont=new GUIContent("Build/Upgrade Cost:", "The resource required to build/upgrade to this level");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				int count=0;	startY+=spaceY; 	float cachedX=startX;
				for(int i=0; i<rscDB.rscList.Count; i++){
					TDEditor.DrawSprite(new Rect(startX+10, startY-1, 20, 20), rscDB.rscList[i].icon);
					stat.cost[i]=EditorGUI.IntField(new Rect(startX+30, startY, widthS, height), stat.cost[i]);
					count+=1; 	startX+=65;
					if(count==3){ startY+=spaceY+3; startX=cachedX; }
				}
				//startX=cachedX;
			
			return startY+spaceY;
		}
		
		
		protected float DrawShootEffectObject(float startX, float startY, UnitStat stat, Unit unit, bool isTower=true){
			bool showShootObj=false;	
			
			if(isTower){
				UnitTower tower=isTower ? unit.gameObject.GetComponent<UnitTower>() : null;
				if(tower.type==_TowerType.Turret) showShootObj=true;
			}
			else{
				UnitCreep creep=!isTower ? unit.gameObject.GetComponent<UnitCreep>() : null;
				if(creep.type==_CreepType.Offense) showShootObj=true;
			}
			
			if(showShootObj){
				cont=new GUIContent("ShootObject:", "The shoot-object used by the unit.\nUnit that intended to shoot at the target will not function correctly if this is left unassigned.");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				stat.shootObject=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX-50, startY, 3*widthS+15, height), stat.shootObject, typeof(ShootObject), false);
			}
			else{
				cont=new GUIContent("EffectObject:", "The effect-object used by the unit.\nSpawned on shoot-point at every cooldown");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				stat.effectObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX-50, startY, 3*widthS+15, height), stat.effectObject, typeof(GameObject), false);
			
				cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.effectObject!=null) stat.autoDestroyEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), stat.autoDestroyEffect);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(stat.effectObject!=null && stat.autoDestroyEffect) 
					stat.effectDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), stat.effectDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
			}
			
			return startY+5;
		}
		
	}
	
}
