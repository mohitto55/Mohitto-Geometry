using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spring : MonoBehaviour
{
    
    public class SpringNode
    {
        public float mass;
        public Vector2 position;
        public Vector2 beforePosition;
        public Vector2 velocity;
        public Vector2 beforeVelocity;
        public bool isLock;
    }
    public float gravity = -9.8f;
    public float k = 7; // 강도
    public float damping = 10; // 감쇠계수
    public int NodeCount = 2;
    public float TimeStep = 1;
    public float mass = 30;
    public float StartLength = 30;
    public float maxLength = 30;
    public Vector2 Wind = Vector2.zero;
    public List<SpringNode> Nodes;

    private void Awake()
    {
        Nodes = new List<SpringNode>();
        for (int i = 0; i < NodeCount; i++)
        {
            SpringNode node = new SpringNode();
            node.position = new Vector2(StartLength * (i), StartLength * (i));
            node.mass = mass;
            Nodes.Add(node);
        }

        if (Nodes.Count > 0)
        {
            SpringNode node = Nodes[0];
            node.isLock = true;
            Nodes[0] = node;
        }
    }

    private void Update()
    {
        Part5MultiSpringMassGuage();
    }

    void Part2SpringMassGuage()
    {
        // var springForceY = -k*(positionY - anchorY);
        // var forceY = springForceY + mass * gravity;
        // var accelerationY = forceY/mass;
        // velocityY = velocityY + accelerationY * Time.deltaTime;
        // positionY = positionY + velocityY;
    }

    void Part2SpringMassGuageGizmo()
    {
        // Gizmos.color = Color.green;
        // Vector2 center = new Vector2(anchorX - 5, anchorY - 5);
        // MyGizmos.DrawWireCicle(center, 5, 30);
        // Gizmos.DrawLine(center, new Vector2(anchorX - 5, positionY));
        // MyGizmos.DrawWireCicle(new Vector2(anchorX - 5, positionY), 5, 30);   
    }

    void Part3Damping()
    {
        // var springForceY = -k*(positionY - anchorY);
        // var dampingForceY = damping * velocityY;
        // var forceY = springForceY + mass * gravity - dampingForceY;
        // var accelerationY = forceY/mass;
        // velocityY = velocityY + accelerationY * Time.deltaTime;
        // positionY = positionY + velocityY;
    }
    
    void Part42DSpringMassGuage()
    {
        // var springForceX = -k*(positionX - anchorY);
        // var springForceY = -k*(positionY - anchorY);
        //
        // var dampingForceX = damping * velocityX;
        // var dampingForceY = damping * velocityY;
        //
        // var forceX = springForceX - dampingForceX;
        // var forceY = springForceY + mass * gravity - dampingForceY;
        //
        // var accelerationX = forceX/mass;
        // var accelerationY = forceY/mass;
        //
        // velocityX = velocityX + accelerationX * Time.deltaTime;
        // velocityY = velocityY + accelerationY * Time.deltaTime;
        //
        // positionX = positionX + velocityX;
        // positionY = positionY + velocityY;
    }
    
    void Part42DSpringMassGuageGizmo()
    {
        // Gizmos.color = Color.green;
        // Vector2 center = new Vector2(anchorX, anchorY);
        // MyGizmos.DrawWireCicle(center, 5, 30);
        // Gizmos.DrawLine(center, new Vector2(positionX, positionY));
        // MyGizmos.DrawWireCicle(new Vector2(positionX, positionY), 5, 30);   
    }

void Part5MultiSpringMassGuage()
    {
        SpringNode NoneNode = new SpringNode();
        for (int i = 0; i < NodeCount; i++)
        {
            SpringNode prevNode = i > 0 ? Nodes[i - 1] : Nodes[i];  // 첫 노드는 자기 자신과 연결됨 (고정된 anchor로 처리할 수도 있음)
            SpringNode curNode = Nodes[i];
            SpringNode nextNode = i < Nodes.Count-1 ? Nodes[i+1] : Nodes[i];  // 마지막 노드는 자기 자신과 연결

            if (curNode.isLock)
                continue;

            // 질량 1의 스프링 힘 계산 (이전 노드와의 상호작용)
            Vector2 mass1SpringForce = new Vector2(-k * (curNode.position.x - prevNode.position.x), -k * (curNode.position.y - prevNode.position.y));
            Vector2 mass1DampingForce = new Vector2(damping * curNode.velocity.x, damping * curNode.velocity.y);

            // 질량 2의 스프링 힘 계산 (다음 노드와의 상호작용)
            Vector2 mass2SpringForce = new Vector2(-k * (nextNode.position.x - curNode.position.x), -k * (nextNode.position.y - curNode.position.y));
            Vector2 mass2DampingForce = new Vector2(damping * nextNode.velocity.x, damping * nextNode.velocity.y);

            // 질량 1에 작용하는 힘 (질량 2와의 상호작용 포함)
            Vector2 mass1Force = new Vector2(
                mass1SpringForce.x - mass1DampingForce.x - mass2SpringForce.x + mass2DampingForce.x,
                mass1SpringForce.y + curNode.mass * gravity - mass1DampingForce.y - mass2SpringForce.y + mass2DampingForce.y
            );
            
            Vector2 mass2Force = new Vector2(
                mass2SpringForce.x - mass2DampingForce.x,
                mass2SpringForce.y + nextNode.mass * gravity - mass2DampingForce.y
            );

            // 가속도 계산
            Vector2 mass1Acceleration = mass1Force / curNode.mass;
            Vector2 mass2Acceleration = mass2Force / nextNode.mass;

            Vector2 WindForce = Wind * TimeStep * Time.deltaTime;
            // 속도 및 위치 업데이트
            curNode.velocity += mass1Acceleration * Time.deltaTime * TimeStep + WindForce;
            curNode.position += curNode.velocity;
            
            nextNode.velocity += mass2Acceleration * Time.deltaTime * TimeStep + WindForce;
            nextNode.position += nextNode.velocity;
            

            Debug.Log(Nodes[i].position);
        }
    }

    void Part5MultiSpringMassGuageGizmo()
    {
        Gizmos.color = Color.green;
        if (Nodes != null)
        {
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                MyGizmos.DrawWireCicle(Nodes[i].position, 5, 30);
                Gizmos.DrawLine(Nodes[i].position, Nodes[i + 1].position);
            }

            if (Nodes.Count > 0)
                MyGizmos.DrawWireCicle(Nodes[Nodes.Count - 1].position, 5, 30);
        }
    }
    void OnDrawGizmos()
    {
        Part5MultiSpringMassGuageGizmo();
    }
}
