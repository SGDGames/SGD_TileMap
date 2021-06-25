using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SGP_Util
{
    public class TileMap : MonoBehaviour
    {
        public string loadFolderName = "";

        // public Object TargetFolder;
        /// <summary>
        /// tile sell size
        /// </summary>
        public float cellSize;

        /// <summary>
        /// horizental count
        /// </summary>
        public int sizeX;

        /// <summary>
        /// vertical count
        /// </summary>
        public int sizeY;


        [HideInInspector] public KeyCode leftMove = KeyCode.A;
        [HideInInspector] public KeyCode rightMove = KeyCode.S;
        [HideInInspector] public KeyCode rotation = KeyCode.R;
        [HideInInspector] public KeyCode active = KeyCode.LeftShift;
        [HideInInspector] public KeyCode delete = KeyCode.D;


        public void UpdateGrid()
        {
            var material = GetComponent<MeshRenderer>().sharedMaterial;
            transform.localScale = new Vector3(cellSize * sizeX * 0.1f, 1, cellSize * sizeY * 0.1f);
            material.SetTextureScale("_BaseMap", new Vector2(sizeX, sizeY));
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(transform);
            EditorUtility.SetDirty(material);
#endif
        }
    }
}