using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TDTK;

namespace TDTK {

	[CustomEditor(typeof(PerkManager))]
	public class PerkManagerEditor : TDEditorInspector {

		private static PerkManager instance;
		
		private bool showPerkList=true;
		
		void Awake(){
			instance = (PerkManager)target;
			LoadDB();
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "BuildManager");
			
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Enable Save", "Check to enable auto saving of any progress made for future game session");
				instance.enableSave=EditorGUILayout.Toggle(cont, instance.enableSave);
				
				cont=new GUIContent("Enable Load", "Check to load from existing saved progress. This will override any preset purchased setting for this level");
				instance.enableLoad=EditorGUILayout.Toggle(cont, instance.enableLoad);
				
				
				cont=new GUIContent("Clear Saved Progress", "Remove all the save");
				if(GUILayout.Button(cont)) instance.ClearProgress();
			
			
			
			EditorGUILayout.Space();
			
			
				DrawPerkList();
			
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			
				if(GUILayout.Button("Open PerkEditor Window")) PerkEditorWindow.Init();
			
			
			EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
		
		void DrawPerkList(){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showPerkList=EditorGUILayout.Foldout(showPerkList, "Show Perk List");
			EditorGUILayout.EndHorizontal();
			
			if(showPerkList){
				
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("EnableAll") && !Application.isPlaying){
					instance.unavailableIDList=new List<int>();
				}
				if(GUILayout.Button("DisableAll") && !Application.isPlaying){
					instance.purchasedIDList=new List<int>();
					instance.unavailableIDList=new List<int>();
					for(int i=0; i<perkDB.perkList.Count; i++) instance.unavailableIDList.Add(perkDB.perkList[i].ID);
				}
				EditorGUILayout.EndHorizontal();
				
				
				for(int i=0; i<perkDB.perkList.Count; i++){
					Perk perk=perkDB.perkList[i];
					
					GUILayout.BeginHorizontal();
						
						GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
						Rect rect=GUILayoutUtility.GetLastRect();
						TDEditor.DrawSprite(rect, perk.icon, perk.desp, false);
						
						GUILayout.BeginVertical();
							EditorGUILayout.Space();
							GUILayout.Label(perk.name, GUILayout.ExpandWidth(false));
					
							GUILayout.BeginHorizontal();
								
								EditorGUI.BeginChangeCheck();
								bool flag=!instance.unavailableIDList.Contains(perk.ID) ? true : false;
								EditorGUILayout.LabelField(new GUIContent(" - enabled: ", "check to enable the perk in this level"), GUILayout.Width(70));
								flag=EditorGUILayout.Toggle(flag);
					
								if(!Application.isPlaying && EditorGUI.EndChangeCheck()){
									if(!flag && !instance.unavailableIDList.Contains(perk.ID))
										instance.unavailableIDList.Add(perk.ID);
									else if(flag) instance.unavailableIDList.Remove(perk.ID);
								}
								
								if(!instance.unavailableIDList.Contains(perk.ID)){
									flag=instance.purchasedIDList.Contains(perk.ID);
									EditorGUILayout.LabelField(new GUIContent(" - purchased: ", "check to set the perk as purchased right from the start"), GUILayout.Width(70));
									flag=EditorGUILayout.Toggle(flag);
									
									if(!Application.isPlaying){
										if(flag) instance.purchasedIDList.Add(perk.ID);
										else instance.purchasedIDList.Remove(perk.ID);
									}
								}
								
							GUILayout.EndHorizontal();
							
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
				}
			
			}
		}
	}

}