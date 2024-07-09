using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RealityFlow.Scripting.Editor
{
    [CustomEditor(typeof(AssemblyReferenceAsset))]
    public class AssemblyReferenceAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AssemblyReferenceAsset asset = target as AssemblyReferenceAsset;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUIStyle style = new(EditorStyles.largeLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                GUILayout.Label("Assembly Info", style);

                Rect area = GUILayoutUtility.GetLastRect();
                area.y += EditorGUIUtility.singleLineHeight + 5;
                area.height = 2;

                EditorGUI.DrawRect(area, new Color(0.2f, 0.2f, 0.2f, 0.4f));
                GUILayout.Space(10);

                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("assemblyName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("assemblyPath"));

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel("Last Write Time");
                        EditorGUILayout.TextField((asset.IsValid == true) ? asset.LastWriteTime.ToString() : string.Empty);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();

            int widthStretch = 310;

            GUILayout.Space(10);
            if (Screen.width > widthStretch)
                GUILayout.BeginHorizontal();

            if (GUILayout.Button("Select Loaded Assembly", GUILayout.Height(30)) == true)
            {
                GenericMenu menu = new();

                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string menuName = asm.FullName;

                    if (menuName.StartsWith("Unity") == true)
                        menuName = "Unity Assemblies/" + menuName;
                    else if (menuName.StartsWith("System") == true)
                        menuName = "System Assemblies/" + menuName;

                    menu.AddItem(
                        new GUIContent(menuName), 
                        false, 
                        (object value) =>
                        {
                            Assembly selectedAsm = (Assembly)value;

                            if (string.IsNullOrEmpty(selectedAsm.Location) == true || File.Exists(selectedAsm.Location) == false)
                            {
                                Debug.LogError("The selected assembly could not be referenced because its source location could not be determined. Please add the assembly using the full path!");
                                return;
                            }

                            string path = selectedAsm.Location;

                            string relativePath = path.Replace('\\', '/');
                            relativePath = FileUtil.GetProjectRelativePath(relativePath);

                            if (string.IsNullOrEmpty(relativePath) == false && File.Exists(relativePath) == true)
                                path = relativePath;

                            asset.UpdateAssemblyReference(path, selectedAsm.FullName);

                            EditorUtility.SetDirty(asset);
                        }, 
                        asm
                    );
                }

                menu.ShowAsContext();
            }

            if (Screen.width > widthStretch)
                GUILayout.EndHorizontal();

            if (asset.IsValid == false)
            {
                EditorGUILayout.HelpBox("The assembly reference is not valid. Select a valid assembly path to reference", MessageType.Warning);
            }
            else if (File.Exists(asset.AssemblyPath) == false)
            {
                EditorGUILayout.HelpBox("The assembly path does not exists. Referencing will still work but any changes to the assembly will not be detected! Consider selecting a valid assembly path", MessageType.Warning);
            }
        }
    }
}