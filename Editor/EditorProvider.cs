using UnityEditor;
using UnityEngine;
using VFavorites;

namespace EditorBoostX
{
    public static class EditorProvider
    {
        private static bool s_vFavoritesFoldout = true;
        private static bool s_vFoldersFoldout = true;
        private static bool s_vFavoritesDataFoldout;
        private static bool s_vFoldersDataFoldout;
        private static bool s_vFoldersPaletteFoldout;
        private static Editor s_paletteEditorInstance; 

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/Editor BoostX", SettingsScope.Project)
            {
                keywords = new[] { "Editor", "BoostX" },
                guiHandler = (_) =>
                {
                    EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(5, 5, 5, 5) });
                    DrawFavorites();
                    DrawFolders();
                    EditorGUILayout.EndVertical();
                },
                deactivateHandler = () =>
                {
                    if (s_paletteEditorInstance == null) return;
                    Object.DestroyImmediate(s_paletteEditorInstance);
                    s_paletteEditorInstance = null;
                }
            };
            return provider;
        }

        private static void DrawFavorites()
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

                DrawVFavoritesSettings();

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawVFavoritesSettings()
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
            s_vFavoritesDataFoldout = EditorGUI.Foldout(dataFoldoutRect, s_vFavoritesDataFoldout, "Data", true, transparentFoldoutStyle);
            if (!s_vFavoritesDataFoldout) return;

            var config = VFavoritesData.instance;
            if (config == null) return;

            var serializedObject = new SerializedObject(config);
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

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

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                VFavoritesData.instance.Save();
                EditorUtility.SetDirty(config);
            }
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
            for (var i = 0; i < listProp.arraySize; i++)
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
                        var child = elementProp.Copy();
                        var end = child.GetEndProperty();

                        if (child.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(child, end)) break;
                                EditorGUILayout.PropertyField(child, true);
                            } while (child.NextVisible(false));
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

        private static void DrawFolders()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var headerRect = EditorGUILayout.GetControlRect(false, 26);
            var bgRect = new Rect(headerRect.x - 3, headerRect.y - 3, headerRect.width + 6, headerRect.height + 4);
            GUI.Box(bgRect, GUIContent.none, GUI.skin.box);

            var transparentFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

            var foldoutRect = new Rect(headerRect.x + 4, headerRect.y, headerRect.width - 55, headerRect.height);
            s_vFoldersFoldout = EditorGUI.Foldout(foldoutRect, s_vFoldersFoldout, "Folders", true, transparentFoldoutStyle);

            var toggleRect = new Rect(headerRect.xMax - 45, headerRect.y + 3, 45, 20);

            var isEnabled = !VFolders.VFoldersMenu.pluginDisabled;
            var newEnabled = DrawSwitchToggle(toggleRect, isEnabled);

            if (newEnabled != isEnabled)
            {
                VFolders.VFoldersMenu.pluginDisabled = !newEnabled;
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }

            if (s_vFoldersFoldout && newEnabled)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(5);

                DrawVFoldersSettings();

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawVFoldersSettings()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            var nav = EditorGUILayout.ToggleLeft("Navigation bar", VFolders.VFoldersMenu.navigationBarEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.navigationBarEnabled = nav;
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var twoLine = EditorGUILayout.ToggleLeft("Two-line names", VFolders.VFoldersMenu.twoLineNamesEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.twoLineNamesEnabled = twoLine;
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var autoIcons = EditorGUILayout.ToggleLeft("Automatic icons", VFolders.VFoldersMenu.autoIconsEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.autoIconsEnabled = autoIcons;
                VFolders.VFolders.folderInfoCache.Clear();
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var hierarchy = EditorGUILayout.ToggleLeft("Hierarchy lines", VFolders.VFoldersMenu.hierarchyLinesEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.hierarchyLinesEnabled = hierarchy;
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var zebra = EditorGUILayout.ToggleLeft("Zebra striping", VFolders.VFoldersMenu.zebraStripingEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.zebraStripingEnabled = zebra;
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var minimap = EditorGUILayout.ToggleLeft("Content minimap", VFolders.VFoldersMenu.contentMinimapEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.contentMinimapEnabled = minimap;
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var bgColors = EditorGUILayout.ToggleLeft("Background colors", VFolders.VFoldersMenu.backgroundColorsEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.backgroundColorsEnabled = bgColors;
                EditorApplication.RepaintProjectWindow();
            }

            EditorGUI.BeginChangeCheck();
            var minimal = EditorGUILayout.ToggleLeft("Minimal mode", VFolders.VFoldersMenu.minimalModeEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                VFolders.VFoldersMenu.minimalModeEnabled = minimal;
                EditorApplication.RepaintProjectWindow();
            }

#if UNITY_EDITOR_OSX
            EditorGUI.BeginChangeCheck();
            var foldersFirst = EditorGUILayout.ToggleLeft("Sort folders first", VFolders.VFoldersMenu.foldersFirstEnabled);
            if (EditorGUI.EndChangeCheck()) 
            { 
                VFolders.VFoldersMenu.foldersFirstEnabled = foldersFirst; 
                EditorApplication.RepaintProjectWindow(); 
                if (!foldersFirst) UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(); 
            }
#endif
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Shortcuts", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            VFolders.VFoldersMenu.toggleExpandedEnabled = EditorGUILayout.ToggleLeft("E to expand ∕ collapse folder", VFolders.VFoldersMenu.toggleExpandedEnabled);
            VFolders.VFoldersMenu.collapseEverythingElseEnabled = EditorGUILayout.ToggleLeft("Shift-E to isolate folder", VFolders.VFoldersMenu.collapseEverythingElseEnabled);
            VFolders.VFoldersMenu.collapseEverythingEnabled =
                EditorGUILayout.ToggleLeft("Ctrl-Shift-E to collapse all folders", VFolders.VFoldersMenu.collapseEverythingEnabled);

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);

            var dataHeaderRect = EditorGUILayout.GetControlRect(false, 18);
            var dataBgRect = new Rect(dataHeaderRect.x + 18, dataHeaderRect.y - 3, dataHeaderRect.width - 18, dataHeaderRect.height + 4);
            GUI.Box(dataBgRect, GUIContent.none, GUI.skin.box);

            var transparentFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            var dataFoldoutRect = new Rect(dataHeaderRect.x + 4, dataHeaderRect.y, dataHeaderRect.width, dataHeaderRect.height);
            s_vFoldersDataFoldout = EditorGUI.Foldout(dataFoldoutRect, s_vFoldersDataFoldout, "Data", true, transparentFoldoutStyle);
            
            DrawVFoldersDataContent();

            DrawVFoldersPalette(transparentFoldoutStyle);
        }

        private static void DrawVFoldersDataContent()
        {
            if (!s_vFoldersDataFoldout) return;

            var config = VFolders.VFoldersData.instance;
            if (config == null) return;

            var serializedObject = new SerializedObject(config);
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(28);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var textStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
            var isTeamMode = VFolders.VFoldersData.storeDataInMetaFiles;

            EditorGUI.BeginDisabledGroup(true);
            if (!isTeamMode)
            {
                EditorGUILayout.LabelField("This file stores data about which icons and colors are assigned to folders, along with bookmarks from navigation bar.", textStyle);
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("If there are multiple people working on the project, it's better to store icon and color data in .meta files of folders to avoid merge conflicts. To do that, use the action button below.", textStyle);
            }
            else
            {
                EditorGUILayout.LabelField("Icon and color data is currently stored in folders .meta files of folders, and this file only contains bookmarks from navigation bar.", textStyle);
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("If you want all data to be stored in this file, use the action button below to revert Team Mode.", textStyle);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(8);

            if (!isTeamMode)
            {
                if (GUILayout.Button("Enable Team Mode", GUILayout.Height(24)))
                {
                    var option = EditorUtility.DisplayDialogComplex("Licensing notice",
                        "To use vFolders 2 within a team, licenses must be purchased for each individual user as per the Asset Store EULA.\n\n Sharing one license across the team is illegal and considered piracy.",
                        "Acknowledge", null, null);
                    
                    if (option == 0) VFolders.VFoldersData.storeDataInMetaFiles = true;
                }
            }
            else
            {
                if (GUILayout.Button("Disable Team Mode", GUILayout.Height(24)))
                {
                    VFolders.VFoldersData.storeDataInMetaFiles = false;
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                VFolders.VFoldersData.instance.Save();
                EditorUtility.SetDirty(config);
            }
        }

        private static void DrawVFoldersPalette(GUIStyle transparentFoldoutStyle)
        {
            EditorGUILayout.Space(10);
            var paletteHeaderRect = EditorGUILayout.GetControlRect(false, 18);
            var paletteBgRect = new Rect(paletteHeaderRect.x + 18, paletteHeaderRect.y - 3, paletteHeaderRect.width - 18, paletteHeaderRect.height + 4);
            GUI.Box(paletteBgRect, GUIContent.none, GUI.skin.box);

            var dataFoldoutRect = new Rect(paletteHeaderRect.x + 4, paletteHeaderRect.y, paletteHeaderRect.width, paletteHeaderRect.height);
            s_vFoldersPaletteFoldout = EditorGUI.Foldout(dataFoldoutRect, s_vFoldersPaletteFoldout, "Palette Data", true, transparentFoldoutStyle);
            
            if (!s_vFoldersPaletteFoldout) return;

            var config = VFolders.VFoldersPalette.instance;
            if (config == null) return;
            
       
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (s_paletteEditorInstance == null || s_paletteEditorInstance.target != config)
                s_paletteEditorInstance = Editor.CreateEditor(config, typeof(VFolders.VFoldersPaletteEditor));

            if (s_paletteEditorInstance != null)
            {
                var oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                s_paletteEditorInstance.OnInspectorGUI();
                
                EditorGUI.indentLevel = oldIndent;
            }

            EditorGUILayout.EndVertical();
            VFolders.VFoldersPalette.instance.Save();
        }
    }
}