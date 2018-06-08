using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Class used so that the UniNoteComponent is not drawn
    /// </summary>
    [CustomEditor(typeof(UniNoteComponent)), CanEditMultipleObjects]
    public class UniNoteComponentDrawer : Editor
    {
        public override void OnInspectorGUI() { }
    }
}