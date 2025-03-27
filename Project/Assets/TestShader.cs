using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Image))]
public class GridHighlighter : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    
    [Header("Appearance")]
    public Color highlightColor = Color.white;
    public Color borderColor = Color.black;
    [Range(0f, 0.1f)]
    public float borderSize = 0.05f;
    
    private Image image;
    private Material material;
    private List<Vector2Int> highlightPositions = new List<Vector2Int>();
    
    void Awake()
    {
        image = GetComponent<Image>();
        // 创建材质实例以避免影响其他对象
        material = new Material(Shader.Find("UI/InventoryGridHighlight"));
        image.material = material;
        
        // 设置初始属性
        UpdateShaderProperties();
    }
    
    void UpdateShaderProperties()
    {
        material.SetInt("_GridWidth", gridWidth);
        material.SetInt("_GridHeight", gridHeight);
        material.SetColor("_HighlightColor", highlightColor);
        material.SetColor("_BorderColor", borderColor);
        material.SetFloat("_BorderSize", borderSize);
        
        // 设置高亮位置
        material.SetInt("_CellCount", Mathf.Min(highlightPositions.Count, 64));
        
        // 将所有位置传递给shader
        for (int i = 0; i < highlightPositions.Count && i < 64; i++)
        {
            material.SetVector("_CellPositions" + "[" + i + "]", 
                new Vector2(highlightPositions[i].x, highlightPositions[i].y));
        }
    }
    
    // 调用此方法设置哪些单元格应该被高亮
    public void SetHighlightCells(Vector2Int[] positions)
    {
        highlightPositions.Clear();
        highlightPositions.AddRange(positions);
        UpdateShaderProperties();
    }
    
    // 示例：使用不规则形状
    public void SetCustomShape(params Vector2Int[] positions)
    {
        SetHighlightCells(positions);
    }

    [Button]
    public void Test()
    {
        // 设置网格尺寸（如果需要）

// 设置不规则形状
        Vector2Int[] customShape = new Vector2Int[] {
            new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1), new Vector2Int(4, 1),
            new Vector2Int(1, 2), new Vector2Int(2, 2)
        };
        SetCustomShape(customShape);
    }
}