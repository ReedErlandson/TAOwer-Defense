using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TDTK;

namespace TDTK{
	
	[CustomEditor(typeof(Unit))]
	[CanEditMultipleObjects]
	public class I_UnitEditor : TDEditorInspector {

		//private static Unit instance;
		void Awake(){
			//instance = (Unit)target;
			//LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("This is the base class for Tower and Creep which on it's own doesn't do anything\n\nUse UnitTower for tower and UnitCreep for creep instead", MessageType.Info);
			
			DefaultInspector();
		}
		
	}

}