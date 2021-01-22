using UnityEngine;

namespace Scenes.E_A_MONO_FISH_ALGORITHM
{
    public class FishSpawner : MonoBehaviour
    {
        [Header("鱼类制体")]
        public GameObject spawnerFish;
        [Header("根节点")]
        public GameObject spawnerRoot;
        [Header("生成范围")]
        public float GenerateRange = 10;
        [Header("生成数量")]
        public int GenerateCount = 10;
        [Header("随机移动速度")]
        public float FishMinSpeed = 40;
        public float FishMaxSpeed = 55;

        public void Start()
        {
            if (null == spawnerRoot || null == spawnerFish)
            {
                return;
            }

            for (int i = 0; i < GenerateCount; i++)
            {
                GameObject newFish = Instantiate(spawnerFish, spawnerRoot.transform, true);
                newFish.transform.name = "Fish_" + i;
                newFish.transform.position = new Vector3(Random.Range(-GenerateRange, GenerateRange),
                    Random.Range(-GenerateRange, GenerateRange),
                    Random.Range(-GenerateRange, GenerateRange));
                newFish.GetComponent<GroupMember>().moveSpeed = Random.Range(FishMinSpeed, FishMaxSpeed);
                newFish.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            }
        }
    }
}
