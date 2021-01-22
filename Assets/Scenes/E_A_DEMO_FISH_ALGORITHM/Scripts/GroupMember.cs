using System.Collections.Generic;
using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.mono
{
    public class GroupMember : MonoBehaviour
    {
        [Header("成员保持距离")]
        private float keepDisSquare;

        private GroupController myGroup;//当前成员的GroupController组件
        //速度和移动相关参数
        private float targetSpeed;
        private float speed;
        private float currentSpeed;
        private Vector3 myMovement;

        [Header("移动速度")]
        public float moveSpeed;

        public FishSpawnerData spawnerData;

        private void Start()
        {
            myGroup = GroupController.GetGroup();
            myGroup.GroupMemberRegister(this);
            keepDisSquare = spawnerData.keepDis * spawnerData.keepDis;
        }

        void Update()
        {
            if (null == myGroup)
            {
                return;
            }

            Vector3 dis = myGroup.transform.position - transform.position;
            Vector3 dir = dis.normalized;

            //重新计算目的地距离权重
            if (dis.magnitude < spawnerData.targetCloseDistance)
            {
                dir *= dis.magnitude / spawnerData.targetCloseDistance;
            }
            dir += GetAroundMemberInfo();       //获取周围组的移动

            //计算移动速度
            if ((myGroup.transform.position - transform.position).magnitude < spawnerData.stopDis)
            {
                targetSpeed = 0;
            }
            else
            {
                targetSpeed = moveSpeed;
            }
            speed = Mathf.Lerp(speed, targetSpeed, 2 * Time.deltaTime);

            //————————————————————移动
            transform.right = -dir;
            Move(dir, speed);
        }

        /// <summary>
        /// 得到周围成员的信息
        /// </summary>
        /// <returns></returns>
        private Vector3 GetAroundMemberInfo()
        {
            // 此处使用碰撞体，修改为位置判断试试
            // Collider[] c = Physics.OverlapSphere(transform.position, myGroup.keepDis, myGroup.mask);
            // Collider2D[] c = Physics2D.OverlapCircleAll(transform.position, myGroup.keepDis, myGroup.mask); //获取周围成员

            Vector3 v1 = Vector3.zero;
            Vector3 v2 = Vector3.zero;

            foreach (GroupMember otherMember in myGroup.GetSameGroupMembers())
            {
                if (otherMember == this)
                    continue;

                if ((otherMember.transform.position - transform.position).sqrMagnitude < keepDisSquare)
                {
                    var dis = transform.position - otherMember.transform.position;
                    v1 += dis.normalized * (1 - dis.magnitude / spawnerData.keepDis); // 查看与周围成员的距离
                    v2 += otherMember.myMovement;                                     // 查看周围成员移动方向
                    // Debug.DrawLine(transform.position, otherMember.transform.position, Color.yellow);
                }
            }

            return v1.normalized * spawnerData.keepWeight + v2.normalized;//添加权重因素
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="_dir">方向</param>
        /// <param name="_speed">速度</param>
        private void Move(Vector3 _dir, float _speed)
        {
            Vector3 finialDirection = _dir.normalized;
            float finialSpeed = _speed, finialRotate = 0;
            float rotateDir = Vector3.Dot(finialDirection, transform.right);
            float forwardDir = Vector3.Dot(finialDirection, transform.forward);

            if (forwardDir < 0)
            {
                rotateDir = Mathf.Sign(rotateDir);
            }
            if (forwardDir < -0.2f)
            {
                finialSpeed = Mathf.Lerp(currentSpeed, -_speed * 8, 4 * Time.deltaTime);
            }

            //——————————防抖
            if (forwardDir < 0.98f)
            {
                finialRotate = Mathf.Clamp(rotateDir * 180, -spawnerData.rotateSpeed, spawnerData.rotateSpeed);
            }

            finialSpeed *= Mathf.Clamp01(_dir.magnitude);
            finialSpeed *= Mathf.Clamp01(1 - Mathf.Abs(rotateDir) * 0.8f);

            transform.Translate(Vector3.left * finialSpeed * Time.deltaTime);
            transform.Rotate(Vector3.forward * finialRotate * Time.deltaTime);

            currentSpeed = finialSpeed;
            myMovement = _dir * finialSpeed;
        }
    }
}
