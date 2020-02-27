using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores colors units use for their hp bar / outline colors
/// </summary>
[CreateAssetMenu(fileName = "DefaultColors", menuName = "ScriptableObjects/DefaultColorsScriptableObject", order = 1)]
public class DefaultColors : ScriptableObject
{
    [Header("HP Bars")]
    public Color ownHP;
    public Color allyChampHP;
    public Color allyStructureHP;
    public Color allyMinionHP;
    public Color enemyChampHP;
    public Color enemyStructureHP;
    public Color enemyMinionHP;

    [Header("Hover Outlines")]
    public Color ownOutline;
    public Color allyOutline;
    public Color enemyOutline;
}
