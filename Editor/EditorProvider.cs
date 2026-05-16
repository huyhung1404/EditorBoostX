using UnityEditor;
using UnityEngine;
using VFavorites;

namespace EditorBoostX
{
    public static class EditorProvider
    {
        private static bool s_vFavoritesFoldout = true;
        private static bool s_vFavoritesDataFoldout;

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/Editor BoostX", SettingsScope.Project)
            {
                keywords = new[] { "Editor", "BoostX" },
                guiHandler = (_) =>
                {
                    var config = VFavoritesData.instance;
                    if (config == null) return;

                    var serializedObject = new SerializedObject(config);
                    serializedObject.Update();

                    EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(5, 5, 5, 5) });
                    EditorGUI.BeginChangeCheck();

                    DrawGUI(serializedObject);

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();

                        var saveMethod = typeof(ScriptableSingleton<VFavoritesData>).GetMethod("Save",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        saveMethod?.Invoke(config, new object[] { true });
                        EditorUtility.SetDirty(config);
                    }

                    EditorGUILayout.EndVertical();
                }
            };
            return provider;
        }

        private static void DrawGUI(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var headerRect = EditorGUILayout.GetControlRect(false, 26);
            var bgRect = new Rect(headerRect.x - 3, headerRect.y - 3, headerRect.width + 6, headerRect.height + 4);
            GUI.Box(bgRect, GUIContent.none, GUI.skin.box);

            var transparentFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            transparentFoldoutStyle.fontStyle = FontStyle.Bold;

            var foldoutRect = new Rect(headerRect.x + 4, headerRect.y, headerRect.width - 55, headerRect.height);
            s_vFavoritesFoldout = EditorGUI.Foldout(foldoutRect, s_vFavoritesFoldout, "Favorites", true, transparentFoldoutStyle);

            var toggleRect = new Rect(headerRect.xMax - 45, headerRect.y + 3, 45, 20);

            var isEnabled = !VFavoritesMenu.pluginDisabled;
            var newEnabled = DrawSwitchToggle(toggleRect, isEnabled);

            if (newEnabled != isEnabled)
            {
                VFavoritesMenu.pluginDisabled = !newEnabled;
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }

            if (s_vFavoritesFoldout && newEnabled)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(5);

                DrawVFavoritesSettings(serializedObject);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawVFavoritesSettings(SerializedObject serializedObject)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Shortcuts", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            VFavoritesMenu.pageScrollEnabled = EditorGUILayout.ToggleLeft("Scroll to change page", VFavoritesMenu.pageScrollEnabled);
            VFavoritesMenu.numberKeysEnabled = EditorGUILayout.ToggleLeft("1-9 keys to change page", VFavoritesMenu.numberKeysEnabled);
            VFavoritesMenu.arrowKeysEnabled = EditorGUILayout.ToggleLeft("Arrow keys to change page or selection", VFavoritesMenu.arrowKeysEnabled);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            VFavoritesMenu.fadeAnimationsEnabled = EditorGUILayout.ToggleLeft("Fade animations", VFavoritesMenu.fadeAnimationsEnabled);
            VFavoritesMenu.pageScrollAnimationEnabled = EditorGUILayout.ToggleLeft("Page scroll animation", VFavoritesMenu.pageScrollAnimationEnabled);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Open Trigger", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            string[] triggerOptions = { "Holding Alt", "Holding Alt and Shift", "Holding Ctrl/Cmd and Alt" };
            VFavoritesMenu.activeOnKeyCombination = EditorGUILayout.Popup("Key Combination", VFavoritesMenu.activeOnKeyCombination, triggerOptions);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            var dataHeaderRect = EditorGUILayout.GetControlRect(false, 18);
            var dataBgRect = new Rect(dataHeaderRect.x + 18, dataHeaderRect.y - 3, dataHeaderRect.width - 18, dataHeaderRect.height + 4);
            GUI.Box(dataBgRect, GUIContent.none, GUI.skin.box);

            var transparentFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            transparentFoldoutStyle.fontStyle = FontStyle.Bold;

            var dataFoldoutRect = new Rect(dataHeaderRect.x + 4, dataHeaderRect.y, dataHeaderRect.width, dataHeaderRect.height);
            s_vFavoritesDataFoldout = EditorGUI.Foldout(dataFoldoutRect, s_vFavoritesDataFoldout, "Internal Data", true, transparentFoldoutStyle);
            if (!s_vFavoritesDataFoldout) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(28);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var rowScaleProp = serializedObject.FindProperty("rowScale");
            if (rowScaleProp != null) EditorGUILayout.PropertyField(rowScaleProp);

            var pagesProp = serializedObject.FindProperty("pages");
            if (pagesProp != null)
            {
                EditorGUILayout.Space(5);
                DrawListWithToolbar(pagesProp, "Pages List");
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        private static void DrawListWithToolbar(SerializedProperty listProp, string title)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUILayout.LabelField($"{title}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.toolbarButton, GUILayout.Width(28)))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.toolbarButton, GUILayout.Width(28)))
            {
                if (listProp.arraySize > 0) listProp.DeleteArrayElementAtIndex(listProp.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elementProp = listProp.GetArrayElementAtIndex(i);
                
                if (elementProp.type == "Page") 
                {
                    DrawPageElement(elementProp, i);
                }
                else
                {
                    elementProp.isExpanded = EditorGUILayout.Foldout(elementProp.isExpanded, $"Item {i}", true);
                    
                    if (elementProp.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        
                        // 2. Tự động lặp qua tất cả các property con bên trong Item
                        var child = elementProp.Copy();
                        var end = child.GetEndProperty();
                        
                        if (child.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(child, end)) break;
                                EditorGUILayout.PropertyField(child, true);
                            } 
                            // false: Chỉ duyệt các biến cùng cấp, không đào sâu vào biến của biến
                            while (child.NextVisible(false)); 
                        }
                        
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawPageElement(SerializedProperty pageProp, int index)
        {
            var nameProp = pageProp.FindPropertyRelative("name");
            var itemsProp = pageProp.FindPropertyRelative("items");

            var pageName = nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue) ? nameProp.stringValue : $"Page {index}";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            pageProp.isExpanded = EditorGUILayout.Foldout(pageProp.isExpanded, pageName, true);
            
            if (!pageProp.isExpanded)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
                return;
            }

            EditorGUI.indentLevel++;
            if (nameProp != null) EditorGUILayout.PropertyField(nameProp);

            if (itemsProp != null)
            {
                EditorGUILayout.Space(2);
                DrawListWithToolbar(itemsProp, "Items");
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        public static bool DrawSwitchToggle(Rect rect, bool value)
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                e.Use();
            }

            if (e.type != EventType.Repaint) return value;
            var bgColor = value ? new Color(0.2f, 0.84f, 0.29f) : new Color(0.45f, 0.45f, 0.45f);

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, bgColor, 0, rect.height / 2f);

            const float padding = 2f;
            var knobSize = rect.height - padding * 2f;

            var knobX = value ? (rect.x + rect.width - knobSize - padding) : (rect.x + padding);
            var knobRect = new Rect(knobX, rect.y + padding, knobSize, knobSize);

            var shadowRect = new Rect(knobRect.x, knobRect.y + 1.5f, knobSize, knobSize);
            GUI.DrawTexture(shadowRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0f, 0f, 0f, 0.35f), 0, knobSize / 2f);
            GUI.DrawTexture(knobRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, knobSize / 2f);

            return value;
        }

        public static bool DrawSwitchToggle(bool value)
        {
            var rect = GUILayoutUtility.GetRect(50, 24);
            return DrawSwitchToggle(rect, value);
        }
    }
}