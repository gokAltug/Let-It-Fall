using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace GALTOOLS
{
    public class LetItFall : EditorWindow
    {
        List<GameObject> allObjects;
        List<Renderer> allRenderers;
        List<Collider> activeColliders;
        List<Collider> inactiveColliders;
        GameObject parent;
        float offset;
        Bounds totalBound;
        Texture2D buttonImage;
        GUIContent buttonGUIContent;

        [MenuItem("GAL/Let It Fall")]
        static void ShowLetItFallWindow()
        {
            GetWindowWithRect(typeof(LetItFall), new Rect(0, 0, 150, 150), true);
        }

        void DropSelectedObjectToGround()
        {
            allObjects.Clear();
            allRenderers.Clear();
            activeColliders.Clear();
            inactiveColliders.Clear();
            var selectedObject = Selection.activeGameObject;
            FindAllTheObjects(selectedObject);
            FindAllTheRenderers();
            DisableAndTrackAllObjectsColliders();
            FindTheCenterOfBounds();
            CalculateTheTotalBoundingBox(selectedObject.transform.position);
            Undo.RecordObject(selectedObject.transform, "Undo Move");
            CreateEmptyAndMakeParent(selectedObject);
            parent.transform.position = new Vector3(parent.transform.position.x, FindGround(parent.transform).y + CalculateOffSetAmount(parent.transform) + offset, parent.transform.position.z);
            RestoreAllColliders();
            ClearParent(selectedObject.transform);
        }

        void CreateEmptyAndMakeParent(GameObject selectedGameObject)
        {
            parent = new GameObject("Parent");
            parent.transform.position = totalBound.center;
            selectedGameObject.transform.parent = parent.transform;
        }
        float CalculateOffSetAmount(Transform parent)
        {
            float ParentAndBoundsMinDistance = parent.transform.position.y - totalBound.min.y;
            return ParentAndBoundsMinDistance;
        }
        void CalculateTheTotalBoundingBox(Vector3 selectedObjectPosition)
        {
            totalBound = new Bounds(FindTheCenterOfBounds(), Vector3.zero);

            foreach (var item in allRenderers)
            {
                totalBound.Encapsulate(item.bounds);
            }
        }

        Vector3 FindTheCenterOfBounds()
        {
            var bound = new Bounds(allRenderers[0].transform.position,Vector3.zero);
            foreach (var item in allRenderers)
            {
                bound.Encapsulate(item.transform.position);
            }
            return bound.center;
        }
        Vector3 FindGround(Transform parent)
        {
            RaycastHit hit;
            Physics.BoxCast(parent.transform.position, new Vector3(totalBound.size.x / 2, totalBound.size.y / 2, totalBound.size.z / 2), Vector3.down, out hit);
            return hit.point;
        }

        void FindAllTheObjects(GameObject selectedObject)
        {
            allObjects.Add(selectedObject);
            var allTransforms = selectedObject.transform.GetComponentsInChildren<Transform>();
            foreach (var item in allTransforms)
            {
                if(item != null)
                {
                    allObjects.Add(item.gameObject);
                }
            }
        }

        void FindAllTheRenderers()
        {
            foreach (var item in allObjects)
            {
                if (item.TryGetComponent<Renderer>(out Renderer itemRenderer))
                {
                    allRenderers.Add(itemRenderer);
                }
            }
        }

        void DisableAndTrackAllObjectsColliders()
        {
            foreach (var item in allObjects)
            {
                if (item.TryGetComponent<Collider>(out Collider collider))
                {
                    if (collider.enabled == true)
                    {
                        activeColliders.Add(collider);
                    }
                    else
                    {
                        inactiveColliders.Add(collider);
                    }
                }
            }

            foreach (var item in allObjects)
            {
                if (item.TryGetComponent<Collider>(out Collider collider))
                {
                    collider.enabled = false;
                }
            }
        }

        void RestoreAllColliders()
        {
            foreach (var item in activeColliders)
            {
                if (item != null)
                    item.enabled = true;
            }
            foreach (var item in inactiveColliders)
            {
                if (item != null)
                    item.enabled = false;
            }
        }

        void ClearParent(Transform selectedObject)
        {
            selectedObject.parent = null;
            DestroyImmediate(parent);
        }

        private void OnEnable()
        {
            allObjects = new List<GameObject>();
            allRenderers = new List<Renderer>();
            activeColliders = new List<Collider>();
            inactiveColliders = new List<Collider>();
            buttonImage = Resources.Load<Texture2D>("LetItFallIcon");
            buttonGUIContent = new GUIContent(buttonImage);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(buttonGUIContent, GUILayout.Width(100), GUILayout.Height(100)))
            {
                foreach (var item in Selection.gameObjects)
                {
                    Selection.activeGameObject = item;
                    DropSelectedObjectToGround();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Drop Offset", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            offset = EditorGUILayout.Slider(offset, -10f, 10f);
            EditorGUILayout.EndHorizontal();
        }
    }
}

