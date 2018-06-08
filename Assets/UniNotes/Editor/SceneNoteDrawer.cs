using RotaryHeart.Lib.ProjectPreferences;
using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Class used to handle the draw of the notes on the scene window
    /// </summary>
    public abstract class SceneNoteDrawer
    {
        //Background color of the notes window
        static Color mainBackground = Color.white * (41f / 255f);
        //Color of the GameObject owner of the note
        static Color owner = Color.white * (82f / 255f);
        //Color of the note
        static Color note = Color.white * (56f / 255f);

        //Saves the scroll position
        static Vector2 scrollPosition = Vector2.zero;

        //Are the ntoes visible?
        static bool visible = true;

        /// <summary>
        /// Function that draws the UI for the notes
        /// </summary>
        /// <param name="goNotes">Array of the notes to draw (index 0 is parent)</param>
        public static void DrawUI(UniNoteComponent[] goNotes)
        {
            //Check if we need to draw the hide/show button
            if (ProjectPrefs.GetBool(Constants.SECTION, Constants.SCENE_NOTES_BTN_ENABLED, true))
                DrawButton();

            //If the notes are not visible ignore
            if (!ProjectPrefs.GetBool(Constants.SECTION, Constants.SCENE_NOTES_HIDDEN, false))
            {
                return;
            }

            //Be sure that the comments are initialized
            foreach (var comment in goNotes)
            {
                if (comment.myNote == null)
                    return;
            }

            //Adjust the alpha for the colors
            mainBackground.a = 1;
            owner.a = 1;
            note.a = 1;

            //Gets the saved size of the rect, calculates the height depending on how many notes are added and calculates the anchored position
            Rect areaRect = new Rect(0, 0, ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_WIDTH, 400),
                ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_HEIGHT, 100));

            areaRect.height = CalculateHeight(goNotes, areaRect);

            switch ((EditorExtensions.Anchor)ProjectPrefs.GetInt(Constants.SECTION, Constants.SCENE_NOTES_ANCHOR, 4))
            {
                case EditorExtensions.Anchor.Top:
                    areaRect.x = Camera.current.pixelWidth / 2f - areaRect.width / 2f;
                    break;

                case EditorExtensions.Anchor.TopRight:
                    areaRect.x = Camera.current.pixelWidth - areaRect.width;
                    break;

                case EditorExtensions.Anchor.Right:
                    areaRect.x = Camera.current.pixelWidth - areaRect.width;
                    areaRect.y = Camera.current.pixelHeight / 2f - areaRect.height / 2f;
                    break;

                case EditorExtensions.Anchor.BottomRight:
                    areaRect.x = Camera.current.pixelWidth - areaRect.width;
                    areaRect.y = Camera.current.pixelHeight - areaRect.height;
                    break;

                case EditorExtensions.Anchor.Bottom:
                    areaRect.x = Camera.current.pixelWidth / 2f - areaRect.width / 2f;
                    areaRect.y = Camera.current.pixelHeight - areaRect.height;
                    break;

                case EditorExtensions.Anchor.BottomLeft:
                    areaRect.y = Camera.current.pixelHeight - areaRect.height;
                    break;

                case EditorExtensions.Anchor.Left:
                    areaRect.y = Camera.current.pixelHeight / 2f - areaRect.height / 2f;
                    break;
            }

            //Draw background rect
            EditorExtensions.DrawRect(areaRect, mainBackground);
            //Begin the actual UI data here
            GUILayout.BeginArea(areaRect);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical();

            //Draw all, including children
            if (ProjectPrefs.GetBool(Constants.SECTION, Constants.SCENE_NOTES_VIEW_CHILDREN, false))
            {
                foreach (var goComment in goNotes)
                {
                    DrawNote(goComment);
                }
            }
            //Draw only the note from the selected Object
            else
            {
                DrawNote(goNotes[0]);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Function that draws the Hide/Show button
        /// </summary>
        private static void DrawButton()
        {
            //Gets the saved size of the rect and calculates the anchored position
            Rect btnRect = new Rect(0, 0, ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_BTN_WIDTH, 30),
                ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_BTN_HEIGHT, 30));

            switch ((EditorExtensions.Anchor)ProjectPrefs.GetInt(Constants.SECTION, Constants.SCENE_NOTES_BTN_ANCHOR, 5))
            {
                case EditorExtensions.Anchor.Top:
                    btnRect.x = Camera.current.pixelWidth / 2f - btnRect.width / 2f;
                    break;

                case EditorExtensions.Anchor.TopRight:
                    btnRect.x = Camera.current.pixelWidth - btnRect.width;
                    break;

                case EditorExtensions.Anchor.Right:
                    btnRect.x = Camera.current.pixelWidth - btnRect.width;
                    btnRect.y = Camera.current.pixelHeight / 2f - btnRect.height / 2f;
                    break;

                case EditorExtensions.Anchor.BottomRight:
                    btnRect.x = Camera.current.pixelWidth - btnRect.width;
                    btnRect.y = Camera.current.pixelHeight - btnRect.height;
                    break;

                case EditorExtensions.Anchor.Bottom:
                    btnRect.x = Camera.current.pixelWidth / 2f - btnRect.width / 2f;
                    btnRect.y = Camera.current.pixelHeight - btnRect.height;
                    break;

                case EditorExtensions.Anchor.BottomLeft:
                    btnRect.y = Camera.current.pixelHeight - btnRect.height;
                    break;

                case EditorExtensions.Anchor.Left:
                    btnRect.y = Camera.current.pixelHeight / 2f - btnRect.height / 2f;
                    break;
            }

            //Draw an invisible button
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor *= 0;
            if (GUI.Button(btnRect, new GUIContent("", "Show/Hide Notes")))
            {
                visible = !visible;
                ProjectPrefs.SetBool(Constants.SECTION, Constants.SCENE_NOTES_HIDDEN, visible);
            }
            GUI.backgroundColor = prevColor;

            //Default icon size
            Vector2 defaultSize = EditorGUIUtility.GetIconSize();
            //Resize the icon so that is more visible
            EditorGUIUtility.SetIconSize(new Vector2(btnRect.width, btnRect.height));
            GUIContent iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
            //Draw the icon
            GUI.Label(btnRect, iconContent);
            //Reset the icon size
            EditorGUIUtility.SetIconSize(defaultSize);
        }

        /// <summary>
        /// Function used to calculate the height of the notes window. This prevents the window from being bigger than thenote displayed
        /// </summary>
        /// <param name="notes">The notes array</param>
        /// <param name="areaRect">The area rectangle</param>
        /// <returns></returns>
        private static float CalculateHeight(UniNoteComponent[] notes, Rect areaRect)
        {
            //Iterate all the notes and calculate the size of each one of them
            float height = 0;
            foreach (var comment in notes)
            {
                height += EditorGUIUtility.singleLineHeight + EditorStyles.label.CalcHeight(new GUIContent(comment.myNote.note), areaRect.width) + 10;

                if (height > areaRect.height)
                    return areaRect.height;
            }

            return height;
        }

        /// <summary>
        /// Draws a note
        /// </summary>
        /// <param name="goNote">The note to draw</param>
        private static void DrawNote(UniNoteComponent goNote)
        {
            //The transform that owns this note
            Transform transform = goNote.transform;

            //Draw the name of the transfrom
            Rect nameRect = EditorGUILayout.GetControlRect();
            EditorExtensions.DrawRect(nameRect, owner);
            EditorGUI.LabelField(nameRect, transform.name);

            //Draw the actual note data
            Rect contentRect = EditorGUILayout.BeginVertical();
            EditorExtensions.DrawRect(contentRect, note);

            GUIStyle style = EditorStyles.label;
            style.wordWrap = true;
            style.richText = true;

            EditorGUILayout.LabelField(goNote.myNote.note, style);
            EditorGUILayout.EndVertical();

            //Add an invisible button so that the child is selected if the note is clicked
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor *= 0;
            if (GUI.Button(new Rect(nameRect.x, nameRect.y, nameRect.width, nameRect.height + contentRect.height), ""))
            {
                EditorGUIUtility.PingObject(transform);

                if (Selection.activeTransform != transform)
                    Selection.activeTransform = transform;
                else
                {
                    var currentlActive = Selection.activeGameObject;
                    Selection.activeGameObject = transform.gameObject;
                    SceneView.lastActiveSceneView.FrameSelected();
                    Selection.activeGameObject = currentlActive;
                }
            }
            GUI.backgroundColor = prevColor;

        }
    }
}