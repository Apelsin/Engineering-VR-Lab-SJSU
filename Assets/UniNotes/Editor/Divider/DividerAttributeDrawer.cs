using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    [CustomPropertyDrawer(typeof(DividerAttribute))]
    public class DividerAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            DividerAttribute att = attribute as DividerAttribute;

            float headerHeight = 10.0f;

            //Calculate the height depending on the data that we draw
            if (!string.IsNullOrEmpty(att.Header))
            {
                headerHeight += 30.0f;
            }

            if (!string.IsNullOrEmpty(att.Subtitle))
            {
                headerHeight += 10.0f;
            }

            return base.GetPropertyHeight(prop, label) + headerHeight;
        }

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            DividerAttribute att = attribute as DividerAttribute;

            float headerHeight = 10.0f;

            //Get the used heights before drawing the drfault property
            if (!string.IsNullOrEmpty(att.Header))
            {
                headerHeight += 30.0f;
            }

            if (!string.IsNullOrEmpty(att.Subtitle))
            {
                headerHeight += 10.0f;
            }

            //Draw the default property
            rect.y += headerHeight;
            EditorGUI.PropertyField(rect, prop, label, true);
            rect.y -= headerHeight;

            //Draw the header
            if (!string.IsNullOrEmpty(att.Header))
            {
                GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontSize = 15;
                headerStyle.fontStyle = FontStyle.Bold;

                EditorGUI.LabelField(rect, att.Header, headerStyle);

                rect.y += 20.0f;
            }

            //Draw the subtitle
            if (!string.IsNullOrEmpty(att.Subtitle))
            {
                GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
                subtitleStyle.fontSize = 10;
                subtitleStyle.fontStyle = FontStyle.Italic;

                EditorGUI.LabelField(rect, att.Subtitle, subtitleStyle);

                rect.y += 17.0f;
            }

            //Draw the divider
            if (Event.current.type == EventType.Repaint)
            {
                rect.height = 1.0f;

                GUI.skin.box.Draw(rect, GUIContent.none, 0);
            }
        }
    }
}