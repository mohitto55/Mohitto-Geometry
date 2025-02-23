using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Geometry.DCEL
{
    [Serializable]
    public enum GeometryOperationType
    {
        INTERSECTION,
        UNION,
        DIFFERENCE,
        XOR,
        NONE
    }
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
        [SerializeField] private GeometryOperationType operationType;
        public GeometryOperationType OperationType => operationType;

        [SerializeField] public List<TransformGroup> vertexs = new List<TransformGroup>();

        public TextMeshPro tmppropab;
        private HalfEdgeData EdgeData;
        private List<HalfEdgeDebugValue> _halfEdgeDebugValue = new List<HalfEdgeDebugValue>();
        
        private List<HalfEdgeVertex> vertices = new List<HalfEdgeVertex>();
        
        [SerializeReference, HideInInspector]
        private ListBehaviour<HalfEdgeVertex> vertexListCompoennt;
        Action<GenerateObject<HalfEdgeVertex, TextMeshPro>, HalfEdgeVertex, TextMeshPro> onUpdate;
        private GenerateObject<HalfEdgeVertex, TextMeshPro> generateObject;

        [SerializeField] private float monotoneDelay = 0.5f;
        [SerializeField] private float travelDelay = 0.1f;
        [SerializeField] private float travelWaitDelay = 0.1f;
        [SerializeField] private float triangulationDelay = 0.3f;
        [SerializeField] private bool vertexTypeOuter = true;
        
        [ShowInInspector]
        public int VertexCount
        {
            get
            {
                if (EdgeData == null)
                    return -1;
                return EdgeData.vertices.Count;
            }
        }
        
        [ShowInInspector]
        public int EdgeCount
        {
            get
            {
                if (EdgeData == null)
                    return -1;
                return EdgeData.edges.Count;
            }
        }
        [ShowInInspector]
        public int FaceCount
        {
            get
            {
                if (EdgeData == null)
                    return -1;
                return EdgeData.faces.Count;
            }
        }

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            InitPolygon();
            // HalfEdge edge = EdgeData.edges[0];
            // HalfEdgeVertex newVertex = new HalfEdgeVertex(edge.vertex.Coordinate + Vector2.up * 2 + Vector2.left * 2);
            //
            // EdgeData.vertices.Add(newVertex);
            // // Debug.Log("Ω√¿€" + EdgeData.edges[0]);
            // EdgeData.InsertEdge(edge, newVertex);
            //EdgeData.UpdateEdge(edge, newVertex, edge.prev.vertex);
            StartCoroutine(Monotone.Monotone.MonotoneTriangulation(EdgeData, _halfEdgeDebugValue, monotoneDelay, travelDelay, travelWaitDelay, triangulationDelay));
        }

        private void Update()
        {
            Mesh mesh = HalfEdgeUtility.GetMesh(EdgeData, operationType);
            _meshFilter.mesh = mesh;
        }

        public void InitPolygon()
        {
            EdgeData = new HalfEdgeData(MyMath.GetVector2ListFromTransform(vertexs[0].Transforms));
            for (int i = 1; i < vertexs.Count; i++)
            {
                EdgeData.AddPolygon(MyMath.GetVector2ListFromTransform(vertexs[i].Transforms));
            }
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                InitPolygon();
            }

            if (EdgeData == null)
                return;
            
            vertices = EdgeData.vertices;
            List<HalfEdge> edges = new List<HalfEdge>();
            edges = EdgeData.edges;
            List<HalfEdgeFace> faces = EdgeData.faces;
            for (int i = 0; i < edges.Count; i++)
            {
                HalfEdge edge = edges[i];
                HalfEdgeVertex vertex = edge.vertex;
                Gizmos.color = Color.white;
                Gizmos.DrawLine(vertex.Coordinate, edge.prev.vertex.Coordinate);
            }

            foreach (var face in faces)
            {
                List<HalfEdge> faceEdges = face.GetOuterEdges();
                foreach (var edge in faceEdges)
                {
                    HalfEdgeUtility.VertexHandleType type =
                        HalfEdgeUtility.DetermineType(edge.prev.vertex, edge.vertex, edge.next.vertex, vertexTypeOuter);
                    switch (type)
                    {
                        case HalfEdgeUtility.VertexHandleType.START:
                            Gizmos.color = Color.blue;
                            break;
                        case HalfEdgeUtility.VertexHandleType.END:
                            Gizmos.color = Color.red;
                
                            break;
                        case HalfEdgeUtility.VertexHandleType.REGULAR:
                            Gizmos.color = Color.green;
                
                            break;
                        case HalfEdgeUtility.VertexHandleType.SPLIT:
                            Gizmos.color = Color.magenta;
                
                            break;
                        case HalfEdgeUtility.VertexHandleType.MERGE:
                            Gizmos.color = Color.cyan;
                
                            break;
                    }
                    MyGizmos.DrawWireCircle(edge.vertex.Coordinate, 1, 30);
                }
            }

            foreach (var halfEdgeDebugValue in _halfEdgeDebugValue)
            {
                if(halfEdgeDebugValue.color == Color.white)
                    halfEdgeDebugValue.color = Color.red;
                Gizmos.color = halfEdgeDebugValue.color;
                MyGizmos.DrawWireCircle(halfEdgeDebugValue.value, 2, 30);
            }
            
            onUpdate = EventCallback;
            generateObject = new GenerateObject<HalfEdgeVertex, TextMeshPro>(tmppropab, onUpdate);
            
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
            if (vertex == null || pro == null || component == null)
                return;
            if (vertex.IncidentEdge != null)
            {
                Vector2 v1 = vertex.IncidentEdge.prev.vertex.Coordinate - vertex.Coordinate;
                Vector2 v2 = vertex.IncidentEdge.next.vertex.Coordinate - vertex.Coordinate;
                float angle = MyMath.SignedAngle(v1, v2);

                pro.transform.position = vertex.Coordinate;
                pro.color = Color.white;
                pro.text = "\nAngle : " + MyMath.SignedAngle(v1, v2) + "\n" +
                           vertex.Coordinate.ToString();
                component.prefab = tmppropab;
            }
            else
            {
                pro.transform.position = vertex.Coordinate;
                pro.color = Color.white;
                pro.text = "\n" +
                           vertex.Coordinate.ToString();
                component.prefab = tmppropab;
            }
        }
    }
}
