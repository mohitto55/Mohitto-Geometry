using System.Collections.Generic;
using System.Drawing.Drawing2D;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;

public static class SplineUtility
{
	
	/// <summary>
	/// spline과 point의 가장 가까운 spline의 한 지점을 반환합니다.
	/// </summary>
	/// <param name="spline"></param>
	/// <param name="point"></param>
	/// <returns></returns>
	public static SplinePoint PointProjection(ISpline spline, Vector2 point)
	{
		float minDst = float.MaxValue;
		int minIndex = -1;

		Vector2[] onPoints = spline.CalculateEvenlySpacedPoints();

		for (int i = 0;i < onPoints.Length; i++)
		{
			float dst = Vector2.Distance(onPoints[i], point);
			if (dst < minDst)
			{
				minDst = dst;
				minIndex = i;
			}
		}

		SplinePoint splinePoint = new SplinePoint();
		
		if (minIndex >= 0)
		{
			splinePoint.p = onPoints[minIndex];
			splinePoint.t = (float)minIndex / (float)onPoints.Length;
		}
		else
		{
			splinePoint.p = point;
		}
		return splinePoint;
	}
	
	public static List<Vector2> IntersectionCurve(CubicBezier spline1, CubicBezier spline2)
	{
		Vector2 P01 = spline1.P0;
		Vector2 P11 = spline1.P1;
		Vector2 P21 = spline1.P2;
		Vector2 P31 = spline1.P3;

		Vector2 P02 = spline2.P0;
		Vector2 P12 = spline2.P1;
		Vector2 P22 = spline2.P2;
		Vector2 P32 = spline2.P3;
		
		Vector2[] bezier1Poly = ToMonomial(P01, P11, P21, P31);
		Vector2[] bezier2Poly = ToMonomial(P02, P12, P22, P32);
		
		Matrix<double> B1 = Matrix<double>.Build.DenseOfArray(new double[2, 1]
		{
			{ bezier1Poly[0].x + bezier1Poly[1].x + bezier1Poly[2].x + bezier1Poly[3].x },
			{ bezier1Poly[0].y + bezier1Poly[1].y + bezier1Poly[2].y + bezier1Poly[3].y },
		});
		
		Matrix<double> C = Matrix<double>.Build.DenseOfArray(new double[2, 4]
		{
			{ bezier1Poly[0].x, bezier1Poly[1].x, bezier1Poly[2].x, bezier1Poly[3].x },
			{ bezier1Poly[0].y, bezier1Poly[1].y, bezier1Poly[2].y, bezier1Poly[3].y },
		});
		
		Matrix<double> D = Matrix<double>.Build.DenseOfArray(new double[2, 4]
		{
			{ bezier2Poly[0].x, bezier2Poly[1].x, bezier2Poly[2].x, bezier2Poly[3].x },
			{ bezier2Poly[0].y, bezier2Poly[1].y, bezier2Poly[2].y, bezier2Poly[3].y },
		});
		
		List<double> commonRoots = Sylvester.GetIntersectionPoints(C, D);
		// if (commonRoots.Count > 0)
		// {
		// 	Vector2 intersectPoint = spline1.GetPoint((float)commonRoots[0]);
		// 	Debug.Log("공통 근 : " + commonRoots[0]);
		// 	Debug.Log("충돌 지점 : " + intersectPoint);
		// }

		List<Vector2> intersectPoints = new List<Vector2>();
		for (int i = 0; i < commonRoots.Count; i++)
		{
			Vector2 intersectPoint = spline1.GetPoint((float)commonRoots[i]);
			intersectPoints.Add(intersectPoint);
		}

		return intersectPoints;
	}

	public static Vector2[] ToMonomial(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		// Matrix4x4 M = new Matrix4x4(
		// 	new Vector4(1, 0, 0, 0),
		// 	new Vector4(-3, 3, 0, 0),
		// 	new Vector4(3, -6, 3, 0),
		// 	new Vector4(-1, 3, -3, 1)).transpose;
		// Matrix4x4 P = new Matrix4x4(
		// 	new Vector4(p0.x, p0.y, 0, 0),
		// 	new Vector4(p1.x, p1.y, 0, 0),
		// 	new Vector4(p2.x, p2.y, 0, 0),
		// 	new Vector4(p3.x, p3.y, 0, 0)).transpose;
		// Matrix4x4 MP = M * P;
		//
		// Vector2 a = new Vector2(MP.m00, MP.m01);
		// Vector2 b = new Vector2(MP.m10, MP.m11);
		// Vector2 c = new Vector2(MP.m20, MP.m21);
		// Vector2 d = new Vector2(MP.m30, MP.m31);

		Vector2 A = p0;
		Vector2 B = -3 * p0 + 3 * p1;
		Vector2 C = 3 * p0 - 6 * p1 + 3 * p2;
		Vector2 D = -p0 + 3 * p1 - 3 * p2 + p3;
		return new Vector2[] { A, B, C, D };
	}
	

	public static bool CheckIntersection(double[] f, double[] g)
	{
		var S = Sylvester.GetSylvesterMatrix(f, g);
		double determinant = S.Determinant();
		return Mathf.Abs((float)determinant) < 1e-6; // determinant가 0이면 교차함
	}
	
	
	
	public static double[] TTable = new double[24]
	{
		-0.0640568928626056260850430826247450385909,
		0.0640568928626056260850430826247450385909,
		-0.1911188674736163091586398207570696318404,
		0.1911188674736163091586398207570696318404,
		-0.3150426796961633743867932913198102407864,
		0.3150426796961633743867932913198102407864,
		-0.4337935076260451384870842319133497124524,
		0.4337935076260451384870842319133497124524,
		-0.5454214713888395356583756172183723700107,
		0.5454214713888395356583756172183723700107,
		-0.6480936519369755692524957869107476266696,
		0.6480936519369755692524957869107476266696,
		-0.7401241915785543642438281030999784255232,
		0.7401241915785543642438281030999784255232,
		-0.8200019859739029219539498726697452080761,
		0.8200019859739029219539498726697452080761,
		-0.8864155270044010342131543419821967550873,
		0.8864155270044010342131543419821967550873,
		-0.9382745520027327585236490017087214496548,
		0.9382745520027327585236490017087214496548,
		-0.9747285559713094981983919930081690617411,
		0.9747285559713094981983919930081690617411,
		-0.9951872199970213601799974097007368118745,
		0.9951872199970213601799974097007368118745
	};

	public static double[] CTable = new double[24]
	{
		0.1279381953467521569740561652246953718517,
		0.1279381953467521569740561652246953718517,
		0.1258374563468282961213753825111836887264,
		0.1258374563468282961213753825111836887264,
		0.121670472927803391204463153476262425607,
		0.121670472927803391204463153476262425607,
		0.1155056680537256013533444839067835598622,
		0.1155056680537256013533444839067835598622,
		0.1074442701159656347825773424466062227946,
		0.1074442701159656347825773424466062227946,
		0.0976186521041138882698806644642471544279,
		0.0976186521041138882698806644642471544279,
		0.086190161531953275917185202983742667185,
		0.086190161531953275917185202983742667185,
		0.0733464814110803057340336152531165181193,
		0.0733464814110803057340336152531165181193,
		0.0592985849154367807463677585001085845412,
		0.0592985849154367807463677585001085845412,
		0.0442774388174198061686027482113382288593,
		0.0442774388174198061686027482113382288593,
		0.0285313886289336631813078159518782864491,
		0.0285313886289336631813078159518782864491,
		0.0123412297999871995468056670700372915759,
		0.0123412297999871995468056670700372915759
	};
}
