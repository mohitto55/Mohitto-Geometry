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
            // �̰� ���� ��ü�� �ȉ´�.
            // ListComponent<T> copy = new ListComponent<T>(comp);
            // components.Add(copy);
            // �̰� ���÷��� ����� �ִ�.
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
        // UnityEngine.Object���� �Ļ��� ���� ��ü�� �ִ� ��� ������ �ڻ����� �����ؾ� �մϴ�( AssetDatabase.AddObjectToAsset ���� ).
        // ���� �̰��� ����� ���� Ŭ�������� ������� �ʽ��ϴ�. �ֳ��ϸ� ����� ���� Ŭ������ �ڵ����� ����ȭ�Ǳ� �����Դϴ�. ���� ���, ������ �׳� �۵��մϴ�.
        // �ƴϿ�. Unity�� Ŭ������ MonoBehaviour �Ǵ� ScriptableObject���� �Ļ��� ��쿡�� �������� ������ ������ �� �ֵ��� ����մϴ�. �� �ܿ��� �����ϴ�.
        // https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceValue.html

        // ����ȭ ���� ���� https://docs.unity3d.com/kr/2021.3/Manual/script-Serialization.html
        // https://mintchobab.tistory.com/39 ���� ���� ������ ������ ���ؼ� ����ߴ�.
        //  UnityEngine.Object���� �Ļ����� ���� Ŀ���� Ŭ������ ��� Unity�� Ŀ���� Ŭ������ �����ϴ� MonoBehaviour �Ǵ� ScriptableObject�� ����ȭ�� �����Ϳ� �ν��Ͻ� ���¸� ���� �����մϴ�.
        // �� �۾��� �ζ��ΰ� [SerializeReference]�� �ϴ� �� ���� ����� �ֽ��ϴ�.
        GenerateObject<T, TObj> copy = (GenerateObject<T, TObj>)Activator.CreateInstance(this.GetType(), this);
        copy.Generate(responseItem);
        copy.OnUpdate = OnUpdate;
        copy.prefab = prefab;
        return copy;
    }
}