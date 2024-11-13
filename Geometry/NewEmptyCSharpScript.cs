using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class ListBehaviour<T>
{
    [SerializeReference] public List<T> items;
    [SerializeReference] public List<ListComponent<T>> components;
    [SerializeReference] public ListComponent<T> comp;

    public ListBehaviour(List<T> items, ListComponent<T> component)
    {
        comp = component;
        this.items = items;
        components = new List<ListComponent<T>>();
        GnerateItemComponent();
    }


    ~ListBehaviour()
    {
        for (int i = 0; i < components.Count; i++)
            components[i].DestroyComponent();
    }

    public void SetItems(List<T> changeItems)
    {
        this.items = changeItems;
    }

    public void SetComponent(ListComponent<T> changeComponent)
    {
        this.comp = changeComponent;
        // for(int i = 0; i < components.Count; i++)
        //     components[i].DestroyComponent();
        // components = new List<ListComponent<T>>();
    }

    public void ResetComponent()
    {
        for (int i = 0; i < components.Count; i++)
            components[i].DestroyComponent();
        components = new List<ListComponent<T>>();
    }

    public bool Update()
    {
        GnerateItemComponent();
        try
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (!components[i].Update(items[i]))
                    return false;
            }

            if (components.Count - items.Count > 0)
            {
                int overCount = components.Count - items.Count;
                int startIndex = components.Count - overCount;
                for (int i = startIndex; i < startIndex + overCount; i++)
                {
                    components[i].DestroyComponent();
                }

                components.RemoveRange(components.Count - overCount, overCount);
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    void GnerateItemComponent()
    {
        if (components == null)
        {
            components = new List<ListComponent<T>>();
        }

        while (items.Count > components.Count)
        {
            // 이건 하위 객체로 안됀다.
            // ListComponent<T> copy = new ListComponent<T>(comp);
            // components.Add(copy);
            // 이건 리플렉션 비용이 있다.
            // components.Add((ListComponent<T>)Activator.CreateInstance(comp.GetType(), comp));
            int count = components.Count;
            components.Add(comp.Factory(items[count]));
        }
    }
}

[System.Serializable]
public class ListComponent<T>
{
    public ListComponent()
    {
    }

    public ListComponent(ListComponent<T> copy)
    {
    }

    ~ListComponent()
    {
    }

    public virtual void Generate(T obj)
    {
    }

    public virtual bool Update(T obj)
    {
        return true;
    }

    public virtual void DestroyComponent()
    {
    }

    public virtual ListComponent<T> Factory(T responseItem)
    {
        return null;
    }
}

[System.Serializable]
public class GenerateObject<T, TObj> : ListComponent<T> where TObj : Object
{
    [SerializeReference] public TObj prefab;
    [SerializeReference] private TObj spawnedObject;
    [SerializeReference] private Action<TObj> tmproUpdate;
    [SerializeReference] public Action<GenerateObject<T, TObj>, T, TObj> OnUpdate;

    public GenerateObject(TObj objPrefab, Action<GenerateObject<T, TObj>, T, TObj> onUpdate = null)
    {
        this.prefab = objPrefab;
        if (onUpdate != null)
            this.OnUpdate += onUpdate;
    }

    public GenerateObject(GenerateObject<T, TObj> copy) : base(copy)
    {
        this.prefab = copy.prefab;
        this.OnUpdate = copy.OnUpdate;
    }

    ~GenerateObject()
    {
        if (spawnedObject)
        {
            GameObject.Destroy(spawnedObject);
        }
    }

    public override void Generate(T obj)
    {
        spawnedObject = GameObject.Instantiate(prefab);
    }

    public override bool Update(T obj)
    {
        if (!prefab)
            return false;
        if (OnUpdate == null)
            return false;
        try
        {
            if (OnUpdate != null)
            {
                OnUpdate.Invoke(this, obj, spawnedObject);
            }

            if (spawnedObject == null)
            {
                spawnedObject = GameObject.Instantiate(prefab);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Null obj");
            return false;
        }

        return true;
    }

    public override void DestroyComponent()
    {
        if (spawnedObject.GameObject())
        {
            GameObject.DestroyImmediate(spawnedObject.GameObject());
        }
    }
    public override ListComponent<T> Factory(T responseItem)
    {
        // UnityEngine.Object에서 파생된 여러 객체가 있는 경우 각각을 자산으로 저장해야 합니다( AssetDatabase.AddObjectToAsset 참조 ).
        // 물론 이것은 사용자 정의 클래스에는 적용되지 않습니다. 왜냐하면 사용자 정의 클래스는 자동으로 직렬화되기 때문입니다. 예를 들어, 다음은 그냥 작동합니다.
        // 아니요. Unity는 클래스가 MonoBehaviour 또는 ScriptableObject에서 파생된 경우에만 다형성을 적절히 저장할 수 있도록 허용합니다. 그 외에는 없습니다.
        // https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html

        // 직렬화 관련 문서 https://docs.unity3d.com/kr/2021.3/Manual/script-Serialization.html
        // https://mintchobab.tistory.com/39 나랑 같이 다형성 문제에 대해서 고민했다.
        //  UnityEngine.Object에서 파생되지 않은 커스텀 클래스의 경우 Unity는 커스텀 클래스를 참조하는 MonoBehaviour 또는 ScriptableObject의 직렬화된 데이터에 인스턴스 상태를 직접 포함합니다.
        // 이 작업은 인라인과 [SerializeReference]로 하는 두 가지 방식이 있습니다.
        GenerateObject<T, TObj> copy = (GenerateObject<T, TObj>)Activator.CreateInstance(this.GetType(), this);
        copy.Generate(responseItem);
        copy.OnUpdate = OnUpdate;
        copy.prefab = prefab;
        return copy;
    }
}