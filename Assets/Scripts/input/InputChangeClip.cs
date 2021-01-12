using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class InputChangeClip : MonoBehaviour
{
    private static InputChangeClip _instance;

    public static InputChangeClip Instance => _instance;

    public void Awake()
    {
        _instance = gameObject.GetComponent<InputChangeClip>();
    }

    public Text MenuText;

    private Entity _entity;

    public void RegisterInputEntity(Entity entity)
    {
        _entity = entity;
        Debug.Log("注册成功");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 尝试按下C键后修改 MyFirstClip_PlayClipComponent 中的Clip值
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
    }
}
