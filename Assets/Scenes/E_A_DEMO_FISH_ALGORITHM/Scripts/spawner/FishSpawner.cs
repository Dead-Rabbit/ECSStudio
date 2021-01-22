using UnityEngine;

namespace E_A_DEMO_FISH_ALGORITHM.mono
{
    public class FishSpawner : MonoBehaviour
    {
        [Header("鱼类制体")]
        public GameObject spawnerFish;
        [Header("根节点")]
        public GameObject spawnerRoot;
        [Header("生成范围")]
        public float GenerateRange = 10;

        public int GenerateCount = 100;
        public FishSpawnerData spawnerData;

        public void Start()
        {
            if (null == spawnerRoot || null == spawnerFish || null == spawnerData)
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
                newFish.GetComponent<GroupMember>().spawnerData = spawnerData;
                newFish.GetComponent<GroupMember>().moveSpeed = Random.Range(spawnerData.FishMinSpeed, spawnerData.FishMaxSpeed);
                newFish.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            }
        }
    }
}
