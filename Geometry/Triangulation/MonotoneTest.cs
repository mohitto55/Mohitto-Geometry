using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Monotone
{
    public class MonotoneTest : MonoBehaviour
    {
        public List<Transform> vertexs = new List<Transform>();
        
        public List<HalfEdgeVertex> vertices;
        [SerializeReference]
        private ListBehaviour<HalfEdgeVertex> vertexListCompoennt;
        public TextMeshPro tmppropab;
        private HalfEdgeData EdgeData;
        private Monotone.HalfEdgeDebugValue _halfEdgeDebugValue = new Monotone.HalfEdgeDebugValue();
        private void Awake()
        {
            EdgeData = new HalfEdgeData(MyMath.GetVector2ListFromTransform(vertexs));
            StartCoroutine(Monotone.MonotoneTriangulation(EdgeData, _halfEdgeDebugValue));
        }

        void OnDrawGizmos()
        {
            if (EdgeData == null)
                return;
           
            // for (int i = 0; i < vertexs.Count; i++)
            // {
            //     Gizmos.DrawLine(vertexs[i].position, vertexs[(i + 1) % vertexs.Count].position);
            // }
            
            vertices = EdgeData.vertices;
            List<HalfEdge> edges = new List<HalfEdge>();
            edges = EdgeData.edges;

            for (int i = 0; i < edges.Count; i++)
            {
                HalfEdge edge = edges[i];
                HalfEdgeVertex vertex = edge.vertex;
                Gizmos.color = Color.white;
                Gizmos.DrawLine(vertex.Coordinate, edge.prev.vertex.Coordinate);
                switch (vertex.type)
                {
                    case HalfEdgeVertex.Vtype.START:
                        Gizmos.color = Color.blue;
                        break;
                    case HalfEdgeVertex.Vtype.END:
                        Gizmos.color = Color.red;

                        break;
                    case HalfEdgeVertex.Vtype.REGULAR:
                        Gizmos.color = Color.green;

                        break;
                    case HalfEdgeVertex.Vtype.SPLIT:
                        Gizmos.color = Color.magenta;

                        break;
                    case HalfEdgeVertex.Vtype.MERGE:
                        Gizmos.color = Color.cyan;

                        break;
                }
                MyGizmos.DrawWireCicle(vertex.Coordinate, 1, 30);
            }

            _halfEdgeDebugValue.color = Color.red;
            Gizmos.color = _halfEdgeDebugValue.color;
            MyGizmos.DrawWireCicle(_halfEdgeDebugValue.value, 2, 30);

            GenerateObject<HalfEdgeVertex, TextMeshPro> generateObject = new GenerateObject<HalfEdgeVertex, TextMeshPro>(tmppropab, EventCallback);
            if (vertexListCompoennt == null)
            {
                vertexListCompoennt = new ListBehaviour<HalfEdgeVertex>(vertices, generateObject);
            }

            vertexListCompoennt.SetItems(vertices);
            if (!vertexListCompoennt.Update())
            {
                vertexListCompoennt.ResetComponent();
                vertexListCompoennt.SetComponent(generateObject);
            }
        }

        void EventCallback(GenerateObject<HalfEdgeVertex, TextMeshPro> component, HalfEdgeVertex vertex, TextMeshPro pro)
        {
            Vector2 v1 = vertex.IncidentEdge.prev.vertex.Coordinate - vertex.Coordinate;
            Vector2 v2 = vertex.IncidentEdge.next.vertex.Coordinate - vertex.Coordinate;
            float angle =  MyMath.SignedAngle(v1, v2);
                    
            pro.transform.position = vertex.Coordinate;
            pro.color = Color.white;
            pro.text = vertex.type.ToString() + "\nAngle : " + MyMath.SignedAngle(v1, v2);
            component.prefab = tmppropab;
        }
    }
}
