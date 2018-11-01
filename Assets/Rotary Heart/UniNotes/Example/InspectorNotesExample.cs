using RotaryHeart.Lib.UniNotes;
using UnityEngine;

public class InspectorNotesExample : MonoBehaviour
{
#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

    [SerializeField, UniNote("WarningNoteId", "This note is set as an attribute")]
    private string value;

    [SerializeField]
    private UniNote note = new UniNote() { note = "This note has been written insde this script. Note that you can even select what type of icon you want to show on your note." };

    [SerializeField]
    private UniNote note1 = new UniNote() { note = "" };
    [SerializeField]
    private UniNote note2 = new UniNote() { note = "" };

    [SerializeField]
    private float lastTime = 0f;
    [SerializeField]
    private int lastFrameCount = 0;
    [SerializeField]
    private Color color = Color.white;
    [SerializeField]
    private string sFPS = "";

    [Divider("header", "subtitle"), SerializeField]
    private string m_id;
    [SerializeField]
    private string m_name;
    [SerializeField]
    private string m_race;

    [Divider("header"), SerializeField]
    private Transform m_target;

    [SerializeField]
    private float m_movSpeed = 3;
    [SerializeField]
    private Vector3 m_offset = Vector3.zero;

    [Range(0f, 20f), SerializeField]
    private float m_verticalSesitivity = 1.5f;
    [Range(0f, 20f), SerializeField]
    private float m_horizontalSensitivity = 1.5f;
    [SerializeField]
    private float m_turnSmoothing = 0.0f;
    [SerializeField]
    private float m_tiltMax = 75f;
    [SerializeField]
    private float m_tiltMin = 45f;
    [SerializeField]
    private bool m_lockCursor = false;
    [SerializeField]
    private bool m_verticalAutoReturn = false;

    private bool m_characterPreview;

    [Divider, SerializeField]
    private GameObject m_objectToSpawn;
    [SerializeField]
    private Vector3 m_spawnOffset;
    [SerializeField]
    private Vector3 m_startingPoint;
    [SerializeField]
    private Vector3 m_amount;

#pragma warning restore 0168 // variable declared but not used.
#pragma warning restore 0219 // variable assigned but not used.
#pragma warning restore 0414 // private field assigned but not used.
}