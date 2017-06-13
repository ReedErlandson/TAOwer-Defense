using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	public class FPSWeaponEditorWindow : TDEditorWindow {
		
		private static FPSWeaponEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (FPSWeaponEditorWindow)EditorWindow.GetWindow(typeof (FPSWeaponEditorWindow), false, "FPS Weapon Editor");
			window.minSize=new Vector2(400, 300);
			
			LoadDB();
			
			//if(prefabID>=0) window.selectID=TDSEditor.GetWeaponIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		

		public void SetupCallback(){
			selectCallback=this.SelectItem;
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
			
			SelectItem();
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<FPSWeapon> weaponList=fpsWeaponDB.weaponList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(fpsWeaponDB, "weaponDB");
			if(weaponList.Count>0) Undo.RecordObject(weaponList[selectID], "weapon");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTD();
			
			//if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			//if(abilityList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Weapon:");
			FPSWeapon newWeapon=null;
			newWeapon=(FPSWeapon)EditorGUI.ObjectField(new Rect(115, 7, 150, 17), newWeapon, typeof(FPSWeapon), false);
			if(newWeapon!=null) Select(NewItem(newWeapon));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawWeaponList(startX, startY, weaponList);	
			startX=v2.x+25;
			
			if(weaponList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				//float cachedX=startX;
				v2=DrawWeaponConfigurator(startX, startY, weaponList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTD();
			
			return true;
		}
		
		
		
		
		private bool foldStats=true;
		Vector2 DrawWeaponConfigurator(float startX, float startY, FPSWeapon weapon){
			TDEditor.DrawSprite(new Rect(startX, startY, 60, 60), weapon.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The ability name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=5, width, height), cont);
			weapon.weaponName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), weapon.weaponName);
			
			cont=new GUIContent("Icon:", "The ability icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			weapon.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), weapon.icon, typeof(Sprite), false);
			
			cont=new GUIContent("Prefab:", "The prefab object of the unit\nClick this to highlight it in the ProjectTab");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), weapon.gameObject, typeof(GameObject), false);
			
			startX-=65;
			startY+=spaceY;	//cachedY=startY;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Basic Setting", headerStyle);
			startX+=15;
			
				cont=new GUIContent("Damage Type:", "The damage type of the weapon\nDamage type can be configured in Damage Armor Table Editor");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				weapon.damageType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), weapon.damageType, damageTypeLabel);
				
				cont=new GUIContent("Recoil:", "The recoil force of the weapon");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				weapon.recoil=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), weapon.recoil);
				
				cont=new GUIContent("ShootPoint:", "The transform which indicate the position where the shootObject will be fired from (Optional)\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the unit transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the shootObject starting orientation.\n");
				shootPointFoldout=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), shootPointFoldout, cont);
				int shootPointCount=weapon.shootPoints.Count;
				shootPointCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), shootPointCount);
				
				if(shootPointCount!=weapon.shootPoints.Count){
					while(weapon.shootPoints.Count<shootPointCount) weapon.shootPoints.Add(null);
					while(weapon.shootPoints.Count>shootPointCount) weapon.shootPoints.RemoveAt(weapon.shootPoints.Count-1);
				}
					
				if(shootPointFoldout){
					for(int i=0; i<weapon.shootPoints.Count; i++){
						int objID=GetObjectIDFromHList(weapon.shootPoints[i], objHList);
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
						objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
						weapon.shootPoints[i] = (objHList[objID]==null) ? null : objHList[objID].transform;
					}
				}
				
				startY+=10;
				
				cont=new GUIContent("ShootObject:", "The shootObject used by the unit.\nUnit that intended to shoot at the target will not function correctly if this is left unassigned.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				weapon.stats[0].shootObject=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), weapon.stats[0].shootObject, typeof(ShootObject), false);
				
				startY+=5;
				
				cont=new GUIContent("ReloadSound:", "The audio-clip to be played when the weapon reloads (optional)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				weapon.reloadSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), weapon.reloadSound, typeof(AudioClip), false);
				
			
			startX-=15;
			startY+=35;
			
			if(weapon.stats.Count==0) weapon.stats.Add(new UnitStat());
			
			string text="Weapon Stats "+(!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldStats, text, foldoutStyle);
			if(foldStats){
				startX+=15;
				
				startY=DrawWeaponStat(weapon.stats[0], startX+15, startY+=spaceY);
				
				//for(int i=0; i<weapon.stats.Count; i++){
				//	startY=DrawWeaponStat(weapon.stats[i], startX+15, startY+=spaceY);
				//}
				
				startX-=15;
			}
			
			
			startY+=25;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Weapon description (to be used in runtime): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			weapon.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 150), weapon.desp, style);
			
			
			return new Vector2(startX, startY+170);
		}
		
		
		
		
		
		
		private float statContentHeight=0;
		private float DrawWeaponStat(UnitStat stat, float startX, float startY){
			
			float width=150;
			float fWidth=35;
			float spaceX=130;
			float height=18;
			float spaceY=height+2;
			
			//startY-=spaceY;
			
			GUI.Box(new Rect(startX, startY, 230, statContentHeight-startY), "");
			
			startX+=10;	startY+=10;
			
				//~ cont=new GUIContent("ShootObject:", "The shootObject used by the unit.\nUnit that intended to shoot at the target will not function correctly if this is left unassigned.");
				//~ EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				//~ stat.shootObject=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX-50, startY, 4*fWidth-10, height), stat.shootObject, typeof(ShootObject), false);
				
			
			//~ startY+=10;
				
			
				cont=new GUIContent("Damage(Min/Max):", "");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				stat.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.damageMin);
				stat.damageMax=EditorGUI.FloatField(new Rect(startX+spaceX+fWidth+2, startY, fWidth, height), stat.damageMax);
				
				cont=new GUIContent("Cooldown:", "Duration between each attack");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.cooldown=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.cooldown);
				
				
				cont=new GUIContent("Clip Size:", "The amount of attack the unit can do before the unit needs to reload\nWhen set to -1 the unit will never need any reload");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.clipSize=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.clipSize);
				stat.clipSize=Mathf.Round(stat.clipSize);
				
				cont=new GUIContent("Reload Duration:", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.reloadDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.reloadDuration);
				
				
			startY+=10;
				
				
				cont=new GUIContent("AOE Radius:", "Area-of-Effective radius. When the shootObject hits it's target, any other hostile unit within the area from the impact position will suffer the same target as the target.\nSet value to >0 to enable. ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.aoeRadius=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.aoeRadius);
				
				
				
				cont=new GUIContent("Stun", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("        - Chance:", "Chance to stun the target in each successful attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.stun.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.stun.chance);
				
				cont=new GUIContent("        - Duration:", "The stun duration in second");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.stun.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.stun.duration);
				
				
				
				cont=new GUIContent("Critical", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("            - Chance:", "Chance to score critical hit in attack. Takes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.crit.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.crit.chance);
				
				cont=new GUIContent("            - Multiplier:", "Damage multiplier for successful critical hit. Takes value from 0 and above with with 0.5 being 50% of normal damage as bonus");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.crit.dmgMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.crit.dmgMultiplier);
				
				
				
				cont=new GUIContent("Slow", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("         - Duration:", "The effect duration in second");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.slow.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.slow.duration);
				
				cont=new GUIContent("         - Multiplier:", "Move speed multiplier. Takes value from 0-1 with with 0.7 being decrese default speed by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.slow.slowMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.slow.slowMultiplier);
				
				
				
				cont=new GUIContent("Dot", "Damage over time");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("        - Duration:", "The effect duration in second");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.dot.duration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.dot.duration);
				
				cont=new GUIContent("        - Interval:", "Duration between each tick. Damage is applied at each tick.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.dot.interval=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.dot.interval);
				
				cont=new GUIContent("        - Damage:", "Damage applied at each tick");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.dot.value=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.dot.value);
				
				
				
				cont=new GUIContent("InstantKill", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);	startY-=spaceY;
				
				cont=new GUIContent("                - Chance:", "The chance to instant kill the target. Takes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.instantKill.chance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.instantKill.chance);
				
				cont=new GUIContent("        - HP Threshold:", "The HP threshold of the target in order for the instantKill to become valid. Take value from 0-1 with 0.3 being 30% of the fullHP.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.instantKill.HPThreshold=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.instantKill.HPThreshold);
				
				
			startY+=10;
				
				
				cont=new GUIContent("Damage Shield Only:", "When checked, unit will only inflict shield damage");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.damageShieldOnly=EditorGUI.Toggle(new Rect(startX+spaceX, startY, fWidth, height), stat.damageShieldOnly);
				
				cont=new GUIContent("Shield Break:", "The chance of the unit's attack to damage target's shield and disable shield regen permenantly\nTakes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.shieldBreak=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.shieldBreak);
				
				cont=new GUIContent("Shield Pierce:", "The chance of the unit's attack to bypass target's shield and damage HP directly\nTakes value from 0-1 with 0 being 0% and 1 being 100%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				stat.shieldPierce=EditorGUI.FloatField(new Rect(startX+spaceX, startY, fWidth, height), stat.shieldPierce);
			
			
			statContentHeight=startY+spaceY+5;
			
			return startY;
		}

		
		

		
		
		
		
		protected Vector2 DrawWeaponList(float startX, float startY, List<FPSWeapon> weaponList){
			List<Item> list=new List<Item>();
			for(int i=0; i<weaponList.Count; i++){
				Item item=new Item(weaponList[i].prefabID, weaponList[i].weaponName, weaponList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(FPSWeapon weapon){ return window._NewItem(weapon); }
		int _NewItem(FPSWeapon weapon){
			if(fpsWeaponDB.weaponList.Contains(weapon)) return selectID;
			
			weapon.prefabID=GenerateNewID(fpsWeaponIDList);
			fpsWeaponIDList.Add(weapon.prefabID);
			
			fpsWeaponDB.weaponList.Add(weapon);
			
			UpdateLabel_FPSWeapon();
			
			return fpsWeaponDB.weaponList.Count-1;
		}
		void DeleteItem(){
			fpsWeaponIDList.Remove(fpsWeaponDB.weaponList[deleteID].prefabID);
			fpsWeaponDB.weaponList.RemoveAt(deleteID);
			
			UpdateLabel_FPSWeapon();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<fpsWeaponDB.weaponList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			FPSWeapon weapon=fpsWeaponDB.weaponList[selectID];
			fpsWeaponDB.weaponList[selectID]=fpsWeaponDB.weaponList[selectID+dir];
			fpsWeaponDB.weaponList[selectID+dir]=weapon;
			selectID+=dir;
		}
		
		void SelectItem(){ 
			if(fpsWeaponDB.weaponList.Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, fpsWeaponDB.weaponList.Count-1);
			UpdateObjectHierarchyList(fpsWeaponDB.weaponList[selectID].gameObject);
		}
	}

}