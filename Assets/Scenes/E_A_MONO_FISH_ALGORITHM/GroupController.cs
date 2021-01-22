using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 组的总控制器（挂载到组长身上，可以是玩家也可以是要跟随的物体）
/// </summary>
public class GroupController : MonoBehaviour
{
    private static List<GroupController> groups; //所有组

    [Header("组中成员的层")]
    public LayerMask mask;
    [Header("组中成员的ID")]
    public int groupID = 0;
    [Header("组中成员始终保持的距离")]
    public float keepDis;
    [Header("组中成员始终保持的距离的权重")]
    public float keepWeight;
    [Header("多少距离算离得太近")]
    public float targetCloseDistance;
    [Header("组中成员停止移动的距离")]
    public float stopDis;

    /// <summary>
    /// 得到成员属于哪个组
    /// </summary>
    /// <param name="index">成员ID</param>
    /// <returns></returns>
    public static GroupController GetGroup(int index)
    {
        if (groups == null)
        {
            groups = new List<GroupController>(FindObjectsOfType(typeof(GroupController)) as GroupController[]);
        }

        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i].groupID == index)
            {
                return groups[i];
            }
        }
        throw new System.Exception("没有找到相同ID的组");
    }
}
