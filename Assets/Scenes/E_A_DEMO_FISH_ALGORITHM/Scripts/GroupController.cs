using UnityEngine;
using System.Collections.Generic;

namespace E_A_DEMO_FISH_ALGORITHM.mono
{
    public class GroupController : MonoBehaviour
    {
        // private static List<GroupController> groups; //所有组

        private static GroupController group;

        // [Header("组中成员的层")] public LayerMask mask;
        // [Header("组中成员的ID")] public int groupID = 0;
        // [Header("组中成员始终保持的距离")] public float keepDis;
        // [Header("组中成员始终保持的距离的权重")] public float keepWeight;
        // [Header("多少距离算离得太近")] public float targetCloseDistance;
        // [Header("组中成员停止移动的距离")] public float stopDis;

        private readonly List<GroupMember> _members = new List<GroupMember>();

        public void Awake()
        {
            group = this;
        }

        public void GroupMemberRegister(GroupMember member)
        {
            _members.Add(member);
        }

        public List<GroupMember> GetSameGroupMembers()
        {
            return _members;
        }

        /// <summary>
        /// 得到成员属于哪个组
        /// </summary>
        /// <param name="index">成员ID</param>
        /// <returns></returns>
        public static GroupController GetGroup()
        {
            // if (groups == null)
            // {
            //     groups = new List<GroupController>(FindObjectsOfType(typeof(GroupController)) as GroupController[]);
            // }
            //
            // for (int i = 0; i < groups.Count; i++)
            // {
            //     if (groups[i].groupID == index)
            //     {
            //         return groups[i];
            //     }
            // }
            return group;

            throw new System.Exception("没有找到相同ID的组");
        }
    }
}
