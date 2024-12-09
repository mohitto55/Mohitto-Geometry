using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;


/// <summary>
/// 유니온 파인드
/// </summary>
/// <typeparam name="T"></typeparam>
public class DisjointSet<T>{
    List<int> parent, rank, setSize;
    Dictionary<T, int> itemIdTable;
    
    public DisjointSet(int n)
    {
        itemIdTable = new Dictionary<T, int>();
        parent = new List<int>();
        rank = new List<int>(n);
        setSize = new List<int>();
        for(int i =0; i < n;i++){
            parent.Add(i);
            rank.Add(1);
            setSize.Add(1);
        }
    }
    /// <summary>
    /// 부모 노드를 찾는 함수
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public int FindParent(int n){
        if(parent[n] == n) return n;
        return parent[n] = FindParent(parent[n]);
    }
    /// <summary>
    /// 두 부모 노드를 합치는 함수
    /// u,v의 부모를 찾고 더 작은 값쪽으로 부모를 합쳐준다.
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    public void Merge(int u, int v){
        u = FindParent(u); v = FindParent(v);
        if(u == v){ return;}
        if(rank[u] > rank[v]){
            int temp = u;
            u = v;
            v = temp;
        }
        parent[u] = v;
        if(rank[u] == rank[v]) ++rank[v];
        setSize[v] += setSize[u];
    }

    /// <summary>
    /// 같은 부모를 가지는지 확인
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public bool SameParent(int u, int v)
    {
        u = FindParent(u); v = FindParent(v);
        if (u == v) return true;
        return false;
    }
    
    public int AddItem(T item){
        if(!itemIdTable.ContainsKey(item)){
            itemIdTable.Add(item, itemIdTable.Count);
            return itemIdTable.Count -1;
        }
        else{
            return itemIdTable[item];
        }
    }
    
    public int GetItemId(T item){
        if(!itemIdTable.ContainsKey(item)){
            itemIdTable.Add(item, itemIdTable.Count);
            return itemIdTable.Count -1;
        }
        else{
            return itemIdTable[item];
        }
    }
    public override string ToString()
    {
        string str = "";
        for (int i = 0; i < parent.Count; i++)
        {
            str += "(" + i + ") Parent : " + parent[i] + " Rank : " + rank[i] + " Size : " + setSize[i] + "\n";
        }
        return str;
    }

    public IEnumerable<List<int>> GetConnectedNodes()
    {
        Dictionary<int, List<int>> idTable = new Dictionary<int, List<int>>();
        for (int i = 0; i < parent.Count; i++)
        {
            int parent = FindParent(i);
            if (!idTable.ContainsKey(parent))
            {
                idTable.Add(parent, new List<int>());
            }
            idTable[parent].Add(i);
        }
        return idTable.Values;
    }
    
    public IEnumerable<List<T>> GetConnectedItems()
    {
        Dictionary<int, List<T>> itemTable = new Dictionary<int, List<T>>();
        foreach (KeyValuePair<T, int> kv in itemIdTable)
        {
            T item = kv.Key;
            int parent = FindParent(kv.Value);
            if (!itemTable.ContainsKey(parent))
            {
                itemTable.Add(parent, new List<T>());
            }
            itemTable[parent].Add(item);
        }
        return itemTable.Values;
    }
};

