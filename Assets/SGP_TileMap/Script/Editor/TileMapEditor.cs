using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SGP_Util
{
    [CustomEditor(typeof(TileMap))]
    public class TileMapEditor : Editor
    {
        private TileMap t;

        private void OnEnable()
        {
            t = target as TileMap;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Active")) t.UpdateGrid();
        }
    }
}