using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SGP_Util
{
    public class TileMapWindow : EditorWindow
    {
        private TileMap t;
        public bool isStart;
        private string path;
        private static readonly string basename = "BaseMap";
        private static readonly string basepath = "Base/" + basename + ".prefab";
        private GameObject[] prefabs;
        private int selectindex;
        private int count;
        private GameObject selectObject;

        private void LoadSaveData()
        {
            if (EditorPrefs.HasKey("SGP_TimeMap_LeftMove")) t.leftMove = (KeyCode) EditorPrefs.GetInt("SGP_TimeMap_LeftMove");
            if (EditorPrefs.HasKey("SGP_TimeMap_RightMove")) t.rightMove = (KeyCode) EditorPrefs.GetInt("SGP_TimeMap_RightMove");
            if (EditorPrefs.HasKey("SGP_TimeMap_Rotation")) t.rightMove = (KeyCode) EditorPrefs.GetInt("SGP_TimeMap_Rotation");
            if (EditorPrefs.HasKey("SGP_TimeMap_Active")) t.active = (KeyCode) EditorPrefs.GetInt("SGP_TimeMap_Active");
            if (EditorPrefs.HasKey("SGP_TimeMap_Delete")) t.delete = (KeyCode) EditorPrefs.GetInt("SGP_TimeMap_Delete");
        }

        private string lastLoadFolderName;

        public void OnGUI()
        {
            if (Application.isPlaying)
            {
                isStart = false;
                if (selectObject != null) selectObject = null;

                return;
            }


            if (t == null)
            {
                var tilemap = GameObject.Find(basename) as GameObject;
                if (tilemap == null)
                {
                    tilemap =  PrefabUtility.InstantiatePrefab(EditorResources.Load<GameObject>(path + basepath)) as GameObject;
                    Undo.IncrementCurrentGroup();
                    Undo.RegisterCreatedObjectUndo(tilemap, basename);
                    tilemap.name = basename;
                }

                t = tilemap.GetComponent<TileMap>();
                LoadSaveData();
            }

            if (t.cellSize <= 0 || t.sizeX < 1 || t.sizeY < 1)
            {
                EditorGUILayout.LabelField(
                    "Initialize failed, The following conditions must be met: cellSize > 0 , sizeX > 1 , size Y > 1");
                return;
            }

            EditorGUI.BeginChangeCheck();
            if (!isStart)
            {
                if (GUILayout.Button("StartBuild....stopped now")) isStart = true;
            }
            else
            {
                if (GUILayout.Button("StopBuild....working now")) isStart = false;
            }

            var subprefabpaths = AssetDatabase.FindAssets("t:prefab", new[] {path + t.loadFolderName});


            if (subprefabpaths.Length > 0 && (count != subprefabpaths.Length || lastLoadFolderName != t.loadFolderName||prefabs.Any(x=>x == null)))
            {
                lastLoadFolderName = t.loadFolderName;
                prefabs = new GameObject[subprefabpaths.Length];
                for (var i = 0; i < subprefabpaths.Length; i++)
                {
                    prefabs[i] =
                        AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(subprefabpaths[i]));
                    //  Debug.Log(prefabs[i].name);

                    if (i == 0)
                    {
                        selectindex = i;
                        SetSelectObject( PrefabUtility.InstantiatePrefab(prefabs[i]) as GameObject);
                        selectObject.SetActive(false);
                    }
                }

                count = subprefabpaths.Length;
            }


            var originleftmoave = t.leftMove;
            t.leftMove = (KeyCode) EditorGUILayout.EnumPopup("LeftMove", t.leftMove);
            if (t.leftMove != originleftmoave) EditorPrefs.SetInt("SGP_TimeMap_LeftMove", (int) t.leftMove);
            var originrightmoave = t.rightMove;
            t.rightMove = (KeyCode) EditorGUILayout.EnumPopup("RightMove", t.rightMove);
            if (t.rightMove != originrightmoave) EditorPrefs.SetInt("SGP_TimeMap_RightMove", (int) t.rightMove);
            var originrotation = t.rotation;
            t.rotation = (KeyCode) EditorGUILayout.EnumPopup("Rotation", t.rotation);
            if (t.rotation != originrotation) EditorPrefs.SetInt("SGP_TimeMap_Active", (int) t.rotation);
            var originactive = t.active;
            t.active = (KeyCode) EditorGUILayout.EnumPopup("Active", t.active);
            if (t.active != originactive) EditorPrefs.SetInt("SGP_TimeMap_Active", (int) t.active);
            var origindelete = t.delete;
            t.delete = (KeyCode) EditorGUILayout.EnumPopup("Delete", t.delete);
            if (t.delete != origindelete) EditorPrefs.SetInt("SGP_TimeMap_Delete", (int) t.delete);
            PreViewArea();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(t);
            //    Undo.IncrementCurrentGroup();
            //    Undo.RecordObject(t, "t");
            }
        }
        private Vector2 scrollpos;

        private void PreViewArea()
        {
            if (prefabs.Length > 0)
            {
                var paletteIcons = new List<GUIContent>();
                foreach (var prefab in prefabs)
                {
                    var texture = AssetPreview.GetAssetPreview(prefab);
                    paletteIcons.Add(new GUIContent(texture));
                }
                scrollpos = EditorGUILayout.BeginScrollView(scrollpos);
                var xCount = Screen.width / 150;
                if (xCount > prefabs.Length)
                    xCount = prefabs.Length;
                else if (xCount == 0) xCount = 1;
                var originselectindex = selectindex;
                selectindex = GUILayout.SelectionGrid(selectindex, paletteIcons.ToArray(), xCount);
                if (originselectindex != selectindex)
                {
                    ChangeSelectIndex("newobject0");
                }

                EditorGUILayout.EndScrollView();
            }
        }
         private void OnSceneGUI(SceneView sv)
        {
            if (Application.isPlaying)
                return;
            selectObjectView = false;
            if (isStart)
            {
                Vector3 mousePos = Event.current.mousePosition;
                mousePos.y = sv.camera.pixelHeight - mousePos.y;
                var ray = sv.camera.ScreenPointToRay(mousePos);
                RaycastHit hit;
             
                if (Physics.Raycast(ray, out hit, 10000, selectObjectLayerMask))
                {
                   
                    if (hit.transform == t.transform)
                    {
                        EditorApplication.ExecuteMenuItem("Window/General/Scene");
                        Handles.color = Color.red;
                        Handles.DrawWireDisc(hit.point, hit.normal, 0.1f);
                        //  Debug.Log(hit.point);
                        var pointx = (int) (hit.point.x / t.cellSize) * t.cellSize;
                        var pointz = (int) (hit.point.z / t.cellSize) * t.cellSize;
                        if (hit.point.x < 0) pointx -= t.cellSize;

                        if (hit.point.z < 0) pointz -= t.cellSize;

                        pointx += t.cellSize * 0.5f;
                        pointz += t.cellSize * 0.5f;

                        var newpoint = new Vector3(pointx, 0, pointz);
                        Handles.DrawWireCube(newpoint, new Vector3(t.cellSize, 0, t.cellSize));
                        selectObjectView = true;
                        selectObjectPosition = newpoint;

                        switch (Event.current.type)
                        {
                            case EventType.KeyDown:

                                if (Event.current.keyCode == t.leftMove)
                                {
                                   
                                    selectindex--;
                                    if (selectindex < 0) selectindex = prefabs.Length - 1;

                                    Repaint();
                                    ChangeSelectIndex("newobject1");
                                }
                                else if (Event.current.keyCode == t.rightMove)
                                {
                                    selectindex++;
                                    if (selectindex >= prefabs.Length) selectindex = 0;
                                    Repaint();
                                    ChangeSelectIndex("newobject2");
                                 
                                }
                                else if (Event.current.keyCode == t.active)
                                {
                                    if (selectObject != null)
                                    {
                                        Undo.IncrementCurrentGroup();
                                        var newobject =  PrefabUtility.InstantiatePrefab(prefabs[selectindex]) as GameObject;
                                        Undo.RegisterCreatedObjectUndo(newobject, "newobject3");
                                        newobject.transform.position = selectObjectPosition;
                                        var rot = newobject.transform.eulerAngles;
                                        rot.y += rotationindex * 90f;
                                        newobject.transform.eulerAngles = rot;
                                        newobject.SetActive(true);
                                        SetLayer(newobject, 0);
                                    }
                                }else if (Event.current.keyCode == t.rotation)
                                {
                                    if (selectObject != null)
                                    {
                                        rotationindex++;
                                        if (rotationindex > 3)
                                            rotationindex = 0;
                                        var rot = selectObject.transform.eulerAngles;
                                        rot.y += 90f;
                                        selectObject.transform.eulerAngles = rot;
                                    }
                                }

                                break;
                        }
                    }
                    else
                    {
                    
                        Selection.activeObject = hit.transform.root.gameObject;
                    
                        switch (Event.current.type)
                        {
                            case EventType.KeyDown:

                                if (Event.current.keyCode == t.delete)
                                {
                                    Undo.IncrementCurrentGroup();
                                    Undo.DestroyObjectImmediate(Selection.activeObject);
                                    DestroyImmediate(Selection.activeObject);
                                }

                                break;
                        }
                    }
                }
            }


            sv.Repaint();
        }

         void ChangeSelectIndex(string undokey)
         {
             if (selectObject != null)
             {
                 Undo.IncrementCurrentGroup();
                 Undo.DestroyObjectImmediate(selectObject);
                 DestroyImmediate(selectObject);
             }

             var newobject =  PrefabUtility.InstantiatePrefab(prefabs[selectindex]) as GameObject;
             SetSelectObject(newobject);
             Undo.RegisterCreatedObjectUndo(newobject, undokey);
         }

         private int rotationindex = 0;
        private void SetSelectObject(GameObject selectObject)
        {
            if(this.selectObject!=null)
                DestroyImmediate(this.selectObject);
            SetLayer(selectObject, selectObjectLayer);
            this.selectObject = selectObject;

            selectObjectLayerMask = ~(1 << selectObjectLayer);
            rotationindex = 0;
        }

        private LayerMask selectObjectLayerMask;
        private readonly int selectObjectLayer = 2;

        private void OnEnable()
        {
            Initialized();
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void Initialized()
        {
            EditorApplication.update += () =>
            {
                if (Application.isPlaying)
                    return;

                if (selectObject != null)
                {
                    selectObject.transform.position = selectObjectPosition;
                    selectObject.SetActive(selectObjectView);
                }
            };
            count = 0;
            var ms = MonoScript.FromScriptableObject(this);
            path = AssetDatabase.GetAssetPath(ms);
            path = path.Replace("Script/Editor/TileMapWindow.cs", "");
        }

        private void OnDisable()
        {
            if (selectObject != null)
                DestroyImmediate(selectObject);
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private static bool selectObjectView;
        private static Vector3 selectObjectPosition;

       

     

        private void SetLayer(GameObject origin, int layer)
        {
            origin.layer = layer;
            for (var i = 0; i < origin.transform.childCount; i++)
                SetLayer(origin.transform.GetChild(i).gameObject, layer);
        }

        [MenuItem("SGP_Pack/TileMap")]
        private static void Init()
        {
            var window = (TileMapWindow) GetWindow(typeof(TileMapWindow));
            window.Show();
        }
    }
}