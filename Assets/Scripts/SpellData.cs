using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Spell", menuName = "Arcanum Draw/Spell Data", order = 1)]
public class SpellData : ScriptableObject
{
    [Header("Basic Properties")]
    public string spellName = "New Spell";
    public string spellID = "new_spell";
    
    [Header("Game Properties")]
    public float manaCost = 10f;
    public float cooldownTime = 1f;
    public GameObject spellEffectPrefab;
    
    [Header("Gesture Template")]
    [Tooltip("Pre-processed normalized points defining the ideal gesture shape")]
    public List<Vector2> gestureTemplate = new List<Vector2>();
    
    [Header("Recognition Settings")]
    [Range(0f, 1f)]
    [Tooltip("How strictly to match (lower = stricter, 0.0-1.0)")]
    public float recognitionTolerance = 0.25f;
    
    [Tooltip("If true, gesture can be drawn at any rotation")]
    public bool allowRotation = false;
    
    [Tooltip("For multi-stroke gestures, does order matter?")]
    public bool enforceStrokeOrder = false;
    
    [Header("Speed Constraints")]
    public bool enforceSpeed = false;
    [Tooltip("X = min speed, Y = max speed (units per second)")]
    public Vector2 expectedSpeedRange = new Vector2(5f, 15f);
    
    [Header("Direction Constraints")]
    public bool enforceDirection = false;
    public GestureDirection expectedDirection = GestureDirection.None;
    
    [Header("Visual Feedback")]
    public Sprite uiIcon;
    public Sprite gestureHintSprite;
    public Color highlightColor = Color.cyan;
    public float highlightDuration = 0.5f;
}

public enum GestureDirection
{
    None,
    Clockwise,
    CounterClockwise,
    Up,
    Down,
    Left,
    Right
}
