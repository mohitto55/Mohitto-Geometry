using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Monotone
{
    [System.Serializable]
    public struct PolygonLine
    {
        public Transform p1;
        public Transform p2;
    }
    [System.Serializable]
    public class TransformGroup
    {
        public List<Transform> Transforms;
    }
    public class MonotoneTest : MonoBehaviour
    {
        
        [SerializeField] public List<TransformGroup> vertexs = new List<TransformGroup>();
        //public List<PolygonLine> _lines = new List<PolygonLine>();
        
        [SerializeReference]
        private ListBehaviour<HalfEdgeVertex> vertexListCompoennt;
        public TextMeshPro tmppropab;
        private HalfEdgeData EdgeData;
        private Monotone.HalfEdgeDebugValue _halfEdgeDebugValue = new Monotone.HalfEdgeDebugValue();

        [ReadOnly, ShowInInspector]
        private int VertexCount
        {
            get
            {
                if (EdgeData == null)
                    return -1;
                return EdgeData.vertices.Count;
            }
        }
        [ReadOnly, ShowInInspector]
        private int EdgeCount
        {
            get
            {
                if (EdgeData == null)
                    return -1;
                return EdgeData.edges.Count;
            }
        }
        [ReadOnly, ShowInInspector]
        private int FaceCount
        {
            get
            {
                if (EdgeData == null)
                    return -1;
                return EdgeData.faces.Count;
            }
        }
        private void Awake()
        {
            InitPolygon();
            StartCoroutine(Monotone.MonotoneTriangulation(EdgeData, _halfEdgeDebugValue));
        }

        public void InitPolygon()
        {
            EdgeData = new HalfEdgeData(MyMath.GetVector2ListFromTransform(vertexs[0].Transforms));
            //for (int i = 1; i < vertexs.Count; i++)
            //{
            //    EdgeData.AddPolygon(MyMath.GetVector2ListFromTransform(vertexs[1].Transforms));
            //}
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                InitPolygon();
            }

            if (EdgeData == null)
                return;
            
            List<HalfEdgeVertex> vertices = EdgeData.vertices;
            List<HalfEdge> edges = new List<HalfEdge>();
            edges = EdgeData.edges;

            for (int i = 0; i < edges.Count; i++)
            {
                HalfEdge edge = edges[i];
                HalfEdgeVertex vertex = edge.vertex;
                Gizmos.color = Color.white;
                Gizmos.DrawLine(vertex.Coordinate, edge.prev.vertex.Coordinate);
            }
            
            for (int i = 0; i < vertices.Count; i++)
            {
                HalfEdgeVertex vertex = vertices[i];

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
                // vertexListCompoennt.ResetComponent();
                // vertexListCompoennt.SetComponent(generateObject);
            }
        }

        void EventCallback(GenerateObject<HalfEdgeVertex, TextMeshPro> component, HalfEdgeVertex vertex, TextMeshPro pro)
        {
            if (vertex == null || pro == null || component == null)
                return;
            if (vertex.IncidentEdge != null)
            {
                Vector2 v1 = vertex.IncidentEdge.prev.vertex.Coordinate - vertex.Coordinate;
                Vector2 v2 = vertex.IncidentEdge.next.vertex.Coordinate - vertex.Coordinate;
                float angle = MyMath.SignedAngle(v1, v2);

                pro.transform.position = vertex.Coordinate;
                pro.color = Color.white;
                pro.text = vertex.type.ToString() + "\nAngle : " + MyMath.SignedAngle(v1, v2) + "\n" +
                           vertex.Coordinate.ToString();
                component.prefab = tmppropab;
            }
            else
            {
                pro.transform.position = vertex.Coordinate;
                pro.color = Color.white;
                pro.text = vertex.type.ToString() + "\n" +
                           vertex.Coordinate.ToString();
                component.prefab = tmppropab;
            }
        }
    }
}
