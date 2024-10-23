using System;
using UnityEngine;


public class AVLNode<T>
{
    public T key;
    public AVLNode<T> parent;
    public AVLNode<T> left, right;
    public T item;
    public int height;

    public AVLNode()
    {
    }
    public AVLNode(T key)
    {
        this.key = key;
        this.height = 1;
        left = null;
        right = null;
    }
}
// 좌우 높이차가 최대 1인트리
// 왼쪽과 오른쪽의 차가 1초과이면 밸런스가 꺠진것을 알 수 있다.
public class AVLTree<T> where T : IComparable<T>
{

    public AVLNode<T> rootNode;

    public void InsertNewNode(T key)
    {
        AVLNode<T> newNode = new AVLNode<T>(key);
        if (rootNode == null)
        {
            rootNode = newNode;
            return;
        }
        Insert(newNode, key);
    }
    public AVLNode<T> Insert(AVLNode<T> node, T key)
    {
        if (node == null)
            return new AVLNode<T>(key);
        if (node.key.CompareTo(key) > 0)
        {
            node.left = Insert(node.left, key);
            if(node.left != null)
            node.left.parent = node;
        }
        else
        {
            node.right = Insert(node.right, key);
            if(node.right != null)
            node.right.parent = node;
        }

        UpdateHeight(node);
        return Balance(node);
    }

    // 삭제할 노드를 찾는다
    // BST 삭제와 같은 방법으로 노드를 삭제한다.
    // 자식이 없으면 삭제하고 자식이 1개면 삭제후 자식의 자식으로 연결
    // 자식이 2개면 오른쪽 값의 가장 왼쪽값과 교환 후 삭제
    // 트리 밸런스를 잡는다.
    public AVLNode<T> Remove(AVLNode<T> node, T key)
    {
        if (node == null)
            return null;
        if (node.key.CompareTo(key) > 0)
        {
            node.left = Remove(node.left, key);
            if(node.left != null)
                node.left.parent = node;
        }
        else if (node.key.CompareTo(key) <= 0)
        {
            node.right = Remove(node.right, key);
            if(node.right != null)
                node.right.parent = node;
        }
        else
        {
            if (node.left == null && node.right == null)
            {
                // node 삭제
                return null;
            }

            if (node.left == null)
            {
                AVLNode<T> right = node.right;
                return right;
            }

            if (node.right == null)
            {
                AVLNode<T> left = node.left;
                return left;
            }
            // 양쪽 자식이 모두 살아있으면 오른쪽 자식에서 가장 왼쪽 자식과 현재 위치를 교환한다.
            AVLNode<T> successor = node.right;
            while (successor.left != null)
            {
                successor = successor.left;
            }

            node.key = successor.key;
            node.right = Remove(node.right, successor.key);
            if(node.right != null)
                node.right.parent = node;
            
        }
        UpdateHeight(node);
        return Balance(node);
    }

    public AVLNode<T> Search(AVLNode<T> node, T key)
    {
        if (node == null || node.key.CompareTo(key) == 0)
        {
            return node;
        }

        if (node.key.CompareTo(key) > 0)
        {
            return Search(node.left, key);
        }
        else
        {
            return Search(node.right, key);
        }
    }
    public AVLNode<T> NewNode(T key)
    {
        AVLNode<T> temp = new AVLNode<T>();
        temp.key = key;
        temp.height = 1;
        return temp;
    }
    
    public AVLNode<T> GetLeft(AVLNode<T> node)
    {
        return null;
    }
    public AVLNode<T> GetRight(AVLNode<T> node)
    {
        return null;
    }

    private int GetHeight(AVLNode<T> node)
    {
        if (node == null)
            return 0;
        return node.height;
    }

    // 왼쪽, 오른쪽 서브트리의 높이의 차로 확인가능
    // 양수면 왼쪽이 크고 음수면 오른쪽이 크다.
    private int GetBalance(AVLNode<T> node)
    {
        if (node == null)
            return 0;
        return GetHeight(node.left) - GetHeight(node.right);
    }

    private void UpdateHeight(AVLNode<T> node)
    {
        node.height = 1 + Mathf.Max(GetHeight(node.left), GetHeight(node.right));
    }

    // 오른쪽으로 기울어져서 왼쪽으로 노드를 옮긴다.
    private AVLNode<T> RotateRight(AVLNode<T> node)
    {
        AVLNode<T> left = node.left;
        node.left = left.right;
        node.left.parent = node;
        left.right = node;
        left.right.parent = left;
        UpdateHeight(node);
        UpdateHeight(left);
        return left;
    }
    
    // 왼쪽으로 기울어져서 오른쪽으로 노드를 옮긴다.
    private AVLNode<T> RotateLeft(AVLNode<T> node)
    {
        AVLNode<T> right = node.right;
        node.right = right.left;
        node.right.parent = node;
        right.left = node;
        right.left.parent = right;
        UpdateHeight(node);
        UpdateHeight(right);
        return right;
    }

    private AVLNode<T> Balance(AVLNode<T> node)
    {
        int balanceFactor = GetBalance(node);
        if (balanceFactor > 1)
        {
            if (GetBalance(node.left) < 0)
            {
                node.left = RotateLeft(node.left);
                node.left.parent = node;
            }
            return RotateRight(node);
        }
        else if (balanceFactor < -1)
        {
            if (GetBalance(node.right) > 0)
            {
                node.right = RotateRight(node.right);
                node.right.parent = node;
            }

            return RotateLeft(node);
        }

        return node;
    }
    
}
