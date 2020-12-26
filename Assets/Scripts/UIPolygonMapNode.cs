using System;
using System.Text;
using UnityEngine;

public class UIPolygonMapNode : MonoBehaviour
{
    private PolygonCollider2D _collider;
    
    // 测试用点
    public Transform pointer;
    
    private Vector2[] points;
    void Start () {
        _collider = gameObject.GetComponent<PolygonCollider2D>();
        if (null == _collider) {
            _collider = gameObject.AddComponent<PolygonCollider2D>();
        }
    }
    
    void Update()
    {
        Debug.LogError("====================");
        Debug.LogError(pointer.position);
        bool result = CheckPosition(pointer.position);
        Debug.LogError(result);
    }

    /// <summary>
    /// 判断当前点是否在碰撞体范围内
    /// </summary>
    /// <param name="Overlaps"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool CheckPosition(Vector2 p)
    {
        points = new Vector2[_collider.GetTotalPointCount()];
        for (int z = 0; z < _collider.GetTotalPointCount(); z++)
        {
            //本地坐标变为屏幕坐标
            points[z] = new Vector2(transform.position.x, transform.position.y) + _collider.points[z];
            Debug.LogError(points[z]);
        }
        
        int i, j, c = 0;
        for (i = 0, j = points.Length - 1; i < points.Length; j = i++) {

            if (((points[i].y > p.y) != (points[j].y > p.y)) && (p.x <
                                                                 (points[j].x - points[i].x) *
                                                                 (p.y - points[i].y) /
                                                                 (points[j].y - points[i].y) + points[i].x)) {
                c = 1 + c;
            }
        }

        if (c % 2 == 0) {
            return false;
        }
        else {
            return true;
        }
    }

    [ContextMenu("复制节点坐标到Excel")]
    public void CopyPolygonMapPointsPosition()
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (var i = 0; i < points.Length; i++) {
            stringBuilder.Append(points[i].x).Append("\t");
            stringBuilder.Append(points[i].y).Append("\t");
        }

        String result = stringBuilder.ToString();
        Debug.Log("result = " + result);
    }
}
