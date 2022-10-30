using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_Triangulation : MonoBehaviour
{
    List<Vector2> pointList=new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //삼각형 클레스
    public class Triangle
    {
        public int point_A;
        public int point_B;
        public int point_C;

        public Triangle(int A,int B,int C)
        {
            this.point_A=A;
            this.point_B=B;
            this.point_C=C;
        }
        
    }

    //외심원 클레스
    public class circumcircle{
        public Vector2 center;

        public float sqrRadius;
        
        public Vector2[] points=new Vector2[3];

        public Vector2[] perpendicularBisector=new Vector2[3];

        public circumcircle(Vector2[] points)
        {
            
            this.points=points;

            float A=points[1].x-points[0].x;
            float B=points[1].y-points[0].y;

            float C=points[2].x-points[0].x;
            float D=points[2].y-points[0].y;

            float E=A*(points[0].x+points[1].x)+B*(points[0].y+points[1].y);
            float F=C*(points[0].x+points[2].x)+D*(points[0].y+points[2].y);

            float G=2*(A*(points[2].y-points[1].y)-B*(points[2].x-points[1].x));

            //외심 지정
            center.x=(D*E-B*F)/G;
            center.y=(A*F-C*E)/G;


            //sqrRadius지정
            sqrRadius=Mathf.Pow(center.x-points[0].x,2)+Mathf.Pow(center.y-points[0].y,2);

            //중점 지점
            perpendicularBisector[0]=(points[1]+points[2])/2;//0
            perpendicularBisector[1]=(points[2]+points[0])/2;//1
            perpendicularBisector[2]=(points[0]+points[1])/2;//2

        }
    }

    /*버텍스가 속한 삼각형을 리스트에서 검색하는 함수*/
    public int findVtxTriangle(int vtx,List<Triangle> triangleList)
    {
        for(int i=0;i<triangleList.Count;i++)
        {
            if(vtxInTriangle(vtx,triangleList[i]))
                return i;
        }
        return -1;
    }
    
    //버텍스가 삼각형에 포함되있는지 확인하는 함수
    public bool vtxInTriangle(int vtx,Triangle triangle)
    {
        float dis=(((pointList[vtx]-pointList[triangle.point_A])*10000).normalized+((pointList[vtx]-pointList[triangle.point_B])*10000).normalized+((pointList[vtx]-pointList[triangle.point_C])*10000).normalized).magnitude;
        return (dis<=1.05);
    }

    //삼각분할 함수
    public List<Triangle> triangulation(int vtx,Triangle triangle)
    {
        List<Triangle> tris=new List<Triangle>();
        tris.Add(new Triangle(triangle.point_A,triangle.point_B,vtx));
        tris.Add(new Triangle(triangle.point_A,triangle.point_C,vtx));
        tris.Add(new Triangle(triangle.point_B,triangle.point_C,vtx));
        return tris;
    }

    /*두점을 공유하는 삼각형을 리스트에서 검색하는 함수*/
    public int findNeighborTriangle(Triangle triangle,List<Triangle> triangleList)
    {
        for(int i=0;i<triangleList.Count;i++)
        {
            if(triangle.point_A==triangleList[i].point_A||triangle.point_A==triangleList[i].point_B||triangle.point_A==triangleList[i].point_C)                    
                if(triangle.point_B==triangleList[i].point_A||triangle.point_B==triangleList[i].point_B||triangle.point_B==triangleList[i].point_C)
                    return i;
                if(triangle.point_C==triangleList[i].point_A||triangle.point_C==triangleList[i].point_B||triangle.point_C==triangleList[i].point_C)
                    return i;

            if(triangle.point_B==triangleList[i].point_A||triangle.point_B==triangleList[i].point_B||triangle.point_B==triangleList[i].point_C)                    
                if(triangle.point_C==triangleList[i].point_A||triangle.point_C==triangleList[i].point_B||triangle.point_C==triangleList[i].point_C)
                    return i;
        }
        return -1;
    }

    public List<Triangle> delaunayTriangulation() {

        List<Triangle> triangleList=new List<Triangle>();


        /*Super Triangle생성*/
        triangleList.Add(new Triangle(0,1,2));
        /*정점[0]로 삼각분할을 수행하여 삼각형 목록에 등록*/
        triangleList=triangulation(3,triangleList[0]);

        /*정점[1]부터 하나 씩 추가*/
        for(int i=4;i<pointList.Count;i++)
        {
            /*정점[i]이 포함된 삼각형[triId]을 검색*/
            int triId=findVtxTriangle(i,triangleList);

            /*삼각분할 수행*/
            List<Triangle> tris=triangulation(i,triangleList[triId]);

            /*삼각형[triId]제거*/
            triangleList.RemoveAt(triId);

            for(int j=1;j<tris.Count;j++)
            {
                //인접삼각형 검색
                int ntrid=findNeighborTriangle(tris[j],triangleList);
                if(ntrid>-1)
                {
                    //외접원 생성
                    List<int> vtxList=new List<int>();
                    
                    vtxList.Add(tris[j].point_C);
                    vtxList.Add(tris[j].point_A);
                    vtxList.Add(tris[j].point_B);

                    if(!vtxList.Contains(triangleList[ntrid].point_A))
                        vtxList.Add(triangleList[ntrid].point_A);
                    if(!vtxList.Contains(triangleList[ntrid].point_B))
                        vtxList.Add(triangleList[ntrid].point_B);
                    if(!vtxList.Contains(triangleList[ntrid].point_C))
                        vtxList.Add(triangleList[ntrid].point_C);


                    Vector2[] tvtx={pointList[vtxList[0]],pointList[vtxList[1]],pointList[vtxList[2]]};

                    circumcircle Circumcircle=new circumcircle(tvtx);
                    /*두 점과 임이의 한점이 삼각분할 조건에 만족하는지 확인*/
                    if((Circumcircle.center-pointList[vtxList[3]]).sqrMagnitude<Circumcircle.sqrRadius)
                    {

                        /*tris[j]삼각형 오버라이드*/
                        tris[j]=new Triangle(vtxList[1],vtxList[3],vtxList[0]);

                        /*ntrid삼각형 오버라이드*/
                        triangleList[ntrid]=new Triangle(vtxList[2],vtxList[3],vtxList[0]);

                    }
                }
            }

            /*삼각분할된 삼각형 등록*/
            triangleList.AddRange(tris);
        }

        for(int i=0;i<triangleList.Count;i++)
        {
            if(triangleList[i].point_A<3||triangleList[i].point_B<3||triangleList[i].point_C<3)
            {
                triangleList.RemoveAt(i);
                i--;
            }
        }

        return triangleList;
    }
}
