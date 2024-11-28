using System.Collections.Generic;
using UnityEngine;


public class DisjointSet<T>{
    List<int> parent, rank, setSize;
    Dictionary<T, int> itemDic;
    public DisjointSet(int n)
    {
        itemDic = new Dictionary<T, int>();
        parent = new List<int>();
        rank = new List<int>();
        setSize = new List<int>();
        for(int i =0;i < parent.Count;i++){
            parent[i] = i;
        }
    }
    public int Find(int n){
        if(parent[n] == n) return n;
        return parent[n] = Find(parent[n]);
    }
    public void Merge(int u, int v){
        u = Find(u); v = Find(v);
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
    public int GetItemId(T item){
        if(!itemDic.ContainsKey(item)){
            itemDic.Add(item, itemDic.Count);
            return itemDic.Count -1;
        }
        else{
            return itemDic[item];
        }
    }
};

