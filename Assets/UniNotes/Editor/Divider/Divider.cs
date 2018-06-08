using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    public abstract class Divider
    {
        /// <summary>
        /// Uses UnityEditor.EditorGUILayout to draw the divider
        /// </summary>
        public static class EditorGUILayout
        {
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(float height = 18)
            {
                Divider(GUIContent.none, GUIContent.none, GUI.skin.label, height);
            }

            #region Polymorphism
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(GUIContent header, float height = 18)
            {
                Divider(header, GUIContent.none, GUI.skin.label, height);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="style">Style to use for the labels</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(GUIContent header, GUIStyle style, float height = 18)
            {
                Divider(header, GUIContent.none, style, height);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(GUIContent header, GUIContent subTitle, float height = 18)
            {
                Divider(header, subTitle, GUI.skin.label, height);
            }

            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(string header, float height = 18)
            {
                Divider(header, string.Empty, GUI.skin.label, height);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="style">Style to use for the labels</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(string header, GUIStyle style, float height = 18)
            {
                Divider(header, string.Empty, style, height);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(string header, string subTitle, float height = 18)
            {
                Divider(header, subTitle, GUI.skin.label, height);
            }
            #endregion

            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <param name="style">Style to use for the labels</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(GUIContent header, GUIContent subTitle, GUIStyle style, float height = 18)
            {
                UnityEditor.EditorGUILayout.Space();

                if (height < 18)
                    height = 18;

                if (!string.IsNullOrEmpty(header.text))
                {
                    GUIStyle headerStyle = new GUIStyle(style);
                    headerStyle.fontSize = (int)(height - 3);
                    headerStyle.fontStyle = FontStyle.Bold;

                    UnityEditor.EditorGUILayout.LabelField(header, headerStyle, GUILayout.Height(height + 3));
                }

                if (!string.IsNullOrEmpty(subTitle.text))
                {
                    GUIStyle subtitleStyle = new GUIStyle(style);
                    subtitleStyle.fontSize = (int)(height - 8);
                    subtitleStyle.fontStyle = FontStyle.Italic;

                    UnityEditor.EditorGUILayout.LabelField(subTitle, subtitleStyle, GUILayout.Height(height - 2));
                }

                Rect elementPos = UnityEditor.EditorGUILayout.GetControlRect(false, 2);
                EditorExtensions.DrawRect(elementPos, Color.white * 0.65f);
            }

            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <param name="style">Style to use for the labels</param>
            /// <param name="height">The height that the divider will use</param>
            public static void Divider(string header, string subTitle, GUIStyle style, float height = 18)
            {
                UnityEditor.EditorGUILayout.Space();

                if (height < 18)
                    height = 18;

                if (!string.IsNullOrEmpty(header))
                {
                    GUIStyle headerStyle = new GUIStyle(style);
                    headerStyle.fontSize = (int)(height - 3);
                    headerStyle.fontStyle = FontStyle.Bold;

                    UnityEditor.EditorGUILayout.LabelField(header, headerStyle, GUILayout.Height(height + 3));
                }

                if (!string.IsNullOrEmpty(subTitle))
                {
                    GUIStyle subtitleStyle = new GUIStyle(style);
                    subtitleStyle.fontSize = (int)(height - 8);
                    subtitleStyle.fontStyle = FontStyle.Italic;

                    UnityEditor.EditorGUILayout.LabelField(subTitle, subtitleStyle, GUILayout.Height(height - 2));
                }

                Rect elementPos = UnityEditor.EditorGUILayout.GetControlRect(false, 2);
                EditorExtensions.DrawRect(elementPos, Color.white * 0.65f);
            }
        }

        /// <summary>
        /// Uses UnityEditor.EditorGUI to draw the divider
        /// </summary>
        public static class EditorGUI
        {
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position)
            {
                return Divider(position, GUIContent.none, GUIContent.none, GUI.skin.label);
            }

            #region Polymorphism
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, GUIContent header)
            {
                return Divider(position, header, GUIContent.none, GUI.skin.label);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <param name="style">Style to use for the labels</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, GUIContent header, GUIStyle style)
            {
                return Divider(position, header, GUIContent.none, style);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, GUIContent header, GUIContent subTitle)
            {
                return Divider(position, header, subTitle, GUI.skin.label);
            }

            /// <summary>
            /// Draws a divider without subtitle
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, string header)
            {
                return Divider(position, header, string.Empty, GUI.skin.label);
            }
            /// <summary>
            /// Draws a divider without subtitle
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <param name="style">Style to use for the labels</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, string header, GUIStyle style)
            {
                return Divider(position, header, string.Empty, style);
            }
            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, string header, string subTitle)
            {
                return Divider(position, header, subTitle, GUI.skin.label);
            }
            #endregion

            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <param name="style">Style to use for the labels</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, GUIContent header, GUIContent subTitle, GUIStyle style)
            {
                Rect elementPos = position;

                elementPos.y += 10;

                if (elementPos.height < 18)
                    elementPos.height = 18;

                Rect nextElement = elementPos;
                nextElement.height = UnityEditor.EditorGUIUtility.singleLineHeight;

                if (!string.IsNullOrEmpty(header.text))
                {
                    GUIStyle headerStyle = new GUIStyle(style);
                    headerStyle.fontSize = (int)(elementPos.height - 3);
                    headerStyle.fontStyle = FontStyle.Bold;

                    elementPos.height += 3;

                    UnityEditor.EditorGUI.LabelField(elementPos, header, headerStyle);
                    elementPos.y += elementPos.height + 5;
                }

                if (!string.IsNullOrEmpty(subTitle.text))
                {
                    GUIStyle subtitleStyle = new GUIStyle(style);
                    subtitleStyle.fontSize = (int)(elementPos.height - 8);
                    subtitleStyle.fontStyle = FontStyle.Italic;

                    elementPos.height = elementPos.height - 2;

                    UnityEditor.EditorGUI.LabelField(elementPos, subTitle, subtitleStyle);
                    elementPos.y += elementPos.height + 2;
                }

                elementPos.height = 2;

                nextElement.y = elementPos.y + elementPos.height;

                EditorExtensions.DrawRect(elementPos, Color.white * 0.65f);

                return nextElement;
            }

            /// <summary>
            /// Draws a divider
            /// </summary>
            /// <param name="position">Rectangle on the screen to draw</param>
            /// <param name="header">The header</param>
            /// <param name="subTitle">The subtitle</param>
            /// <param name="style">Style to use for the labels</param>
            /// <returns>Rectangle of the next element to draw</returns>
            public static Rect Divider(Rect position, string header, string subTitle, GUIStyle style)
            {
                Rect elementPos = position;

                elementPos.y += 10;

                if (elementPos.height < 18)
                    elementPos.height = 18;

                Rect nextElement = elementPos;
                nextElement.height = UnityEditor.EditorGUIUtility.singleLineHeight;

                if (!string.IsNullOrEmpty(header))
                {
                    GUIStyle headerStyle = new GUIStyle(style);
                    headerStyle.fontSize = (int)(elementPos.height - 3);
                    headerStyle.fontStyle = FontStyle.Bold;

                    elementPos.height += 3;

                    UnityEditor.EditorGUI.LabelField(elementPos, header, headerStyle);
                    elementPos.y += elementPos.height + 5;
                }

                if (!string.IsNullOrEmpty(subTitle))
                {
                    GUIStyle subtitleStyle = new GUIStyle(style);
                    subtitleStyle.fontSize = (int)(elementPos.height - 8);
                    subtitleStyle.fontStyle = FontStyle.Italic;

                    elementPos.height = elementPos.height - 2;

                    UnityEditor.EditorGUI.LabelField(elementPos, subTitle, subtitleStyle);
                    elementPos.y += elementPos.height + 2;
                }

                elementPos.height = 2;

                nextElement.y = elementPos.y + elementPos.height;

                EditorExtensions.DrawRect(elementPos, Color.white * 0.65f);

                return nextElement;
            }
        }
    }
}