using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Symbolics;
using UnityEngine;


public static class Sylvester
{

    public static Matrix<double> T(double t)
    {
        return Matrix<double>.Build.DenseOfArray(
            new double[4, 1]
            {
                { 1 },
                { t },
                { t * t },
                { t * t * t }
            });
    }
    
    public static Matrix<double> K(double k)
    {
        return Matrix<double>.Build.DenseOfArray(
            new double[4, 4]
            {
                { 1,0,0,0 },
                { 0,k,0,0 },
                { 0,0,k*k,0 },
                { 0,0,0,k*k*k }
            });
    }
    
    public static Matrix<double> GetSylvesterMatrix(double[] f, double[] g)
    {
        int degree = f.Length + g.Length - 2; // 행렬 크기 설정
        double[,] matrix = new double[degree, degree];

		
        int xSpace = 0;
        for (int y = 0; y < f.Length - 1; y++)
        {
            for (int x = 0; x < f.Length; x++)
            {
                matrix[x + xSpace, y] = f[x];
                matrix[x + xSpace, y+f.Length - 1] = g[x];
				
                // matrix[y, x + xSpace] = f[x];
                // matrix[y+f.Length - 1, x + xSpace] = g[x];
            }
            xSpace++;
        }
        return Matrix<double>.Build.DenseOfArray(matrix);
    }
    
    public static List<double> GetIntersectionPoints(Matrix<double> B1, Matrix<double> B2)
    {
        // c가 b1 d가 b2
        double c0x = B1[0,0];
        double c1x = B1[0,1];
        double c2x = B1[0,2];
        double c3x = B1[0,3];
        double c0y = B1[1,0];
        double c1y = B1[1,1];
        double c2y = B1[1,2];
        double c3y = B1[1,3];
        
        double d0x = B2[0,0];
        double d1x = B2[0,1];
        double d2x = B2[0,2];
        double d3x = B2[0,3];
        double d0y = B2[1,0];
        double d1y = B2[1,1];
        double d2y = B2[1,2];
        double d3y = B2[1,3];

        double a = c3x;
        double b = d3x;
        double c = c2x;
        double d = d2x;
        double e = c1x;
        double f = d1x;
        double m = c3y;
        double n = d3y;
        double p = c2y;
        double q = d2y;
        double r = c1y;
        double s = d1y;
        double dx = c0x - d0x;
        double dy = c0y - d0y;
        
        // 공통 항목 변수로 정리
        var k = SymbolicExpression.Variable("k");
        var kk = k * k;
        var kkk = k * k * k;
        
        // S2K-1 행렬
        // X_{2K-1}(t) 와 Y_{2K-1}(t) 식이 들어갔다.
        SymbolicExpression[,] matrix = {
            { a - b * kkk, c - d * kk, e - f * k, dx, 0, 0 },
            { 0, a - b * kkk, c - d * kk, e - f * k, dx, 0 },
            { 0, 0, a - b * kkk, c - d * kk, e - f * k, dx },
            { m - n * kkk, p - q * kk, r - s * k, dy, 0, 0 },
            { 0, m - n * kkk, p - q * kk, r - s * k, dy, 0 },
            { 0, 0, m - n * kkk, p - q * kk, r - s * k, dy }
        };
        
        SymbolicExpression determinant = ComputeDeterminant(matrix);
        SymbolicExpression sylvesterDetPolynomial = determinant.Expand();
        
        List<double> coefficients = new List<double>();

        for (int i = 0; i <= 9; i++)
        {
            SymbolicExpression coefficient = sylvesterDetPolynomial.Coefficient(k, i);
            coefficients.Add(coefficient.RealNumberValue);
        }
        
        List<double> answerRoots = new List<double>();
        try
        {
            // 다항식의 차수
            int degree = coefficients.Count - 1;  // 계수 배열 크기에서 1 빼기
            
            // 다항식의 복소수 근 찾기
            alglib.complex[] kRoots;
            alglib.polynomialsolverreport report;
            alglib.polynomialsolve(coefficients.ToArray(), degree, out kRoots, out report);
            
            kRoots = kRoots.Where(k => { return k.x >= 0; }).ToArray();

            alglib.complex[] tRoots;
            foreach (var kRoot in kRoots)
            {
                List<double> firstPolytRoots = new List<double>();
                
                double rootK = kRoot.x;
                double rootKK = rootK * rootK;
                double rootKKK = rootKK * rootK;

                double x0 = dx;
                double x1 = e - f * rootK;
                double x2 = c - d * rootKK;
                double x3 = a - b * rootKKK;
                
                coefficients.Clear();
                coefficients.Add(x0);
                coefficients.Add(x1);
                coefficients.Add(x2);
                coefficients.Add(x3);

                alglib.polynomialsolve(coefficients.ToArray(), coefficients.Count-1, out tRoots, out report);
                foreach (var tRoot in tRoots)
                {
                    // x = 실수, y = 허수
                    firstPolytRoots.Add(tRoot.x);
                }
                
                double y0 = dy;
                double y1 = r - s * rootK;
                double y2 = p - q * rootKK;
                double y3 = m - n * rootKKK;
                
                coefficients.Clear();
                coefficients.Add(y0);
                coefficients.Add(y1);
                coefficients.Add(y2);
                coefficients.Add(y3);

                alglib.polynomialsolve(coefficients.ToArray(), coefficients.Count-1, out tRoots, out report);
                foreach (var tRoot in tRoots)
                {
                    foreach (var compareT in firstPolytRoots)
                    {
                        // k가 0~1사이라면
                        if (MyMath.FloatZero((float)compareT - (float)tRoot.x) && 0 <= tRoot.x && tRoot.x <= 1)
                        {
                            if (rootK * tRoot.x <= 1)
                            {
                                answerRoots.Add(tRoot.x);
                            }
                        }
                    }
                }
            }
        }
        catch (alglib.alglibexception ew)
        {
            Debug.Log($"ALGLIB 예외 발생: {ew.Message}");
        }
        return answerRoots;
    }
    
    // 행렬식을 계산하는 함수 (Laplace 전개 방식)
    static SymbolicExpression ComputeDeterminant(SymbolicExpression[,] matrix)
    {
        int size = matrix.GetLength(0);

        // 1x1 행렬이면 바로 반환
        if (size == 1)
            return matrix[0, 0];

        // 2x2 행렬이면 직접 계산
        if (size == 2)
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        SymbolicExpression determinant = SymbolicExpression.Zero;

        // 첫 번째 행을 기준으로 Laplace 전개
        for (int col = 0; col < size; col++)
        {
            SymbolicExpression cofactor = matrix[0, col] * ComputeDeterminant(Minor(matrix, 0, col));
            determinant += (col % 2 == 0 ? cofactor : -cofactor);
        }

        return determinant;
    }

    // 소행렬(minor) 계산 (행과 열을 제외한 행렬 생성)
    static SymbolicExpression[,] Minor(SymbolicExpression[,] matrix, int row, int col)
    {
        int size = matrix.GetLength(0);
        SymbolicExpression[,] minor = new SymbolicExpression[size - 1, size - 1];

        int m = 0, n;
        for (int i = 0; i < size; i++)
        {
            if (i == row) continue;
            n = 0;
            for (int j = 0; j < size; j++)
            {
                if (j == col) continue;
                minor[m, n] = matrix[i, j];
                n++;
            }
            m++;
        }
        return minor;
    }
    
    // 다항식 계산
    static double EvaluatePolynomial(double[] coefficients, double x)
    {
        double result = 0;
        for (int i = 0; i < coefficients.Length; i++)
        {
            result += coefficients[i] * Math.Pow(x, coefficients.Length - i - 1);
        }
        return result;
    }

    // 다항식의 미분 계산
    static double EvaluateDerivative(double[] coefficients, double x)
    {
        double result = 0;
        for (int i = 0; i < coefficients.Length - 1; i++)
        {
            result += coefficients[i] * (coefficients.Length - i - 1) * Math.Pow(x, coefficients.Length - i - 2);
        }
        return result;
    }
    
    public static Matrix<double> GetBezoutMatrix(Matrix<double> B1, Matrix<double> B2)
    {
        double c0x = B1[0,0];
        double c1x = B1[0,1];
        double c2x = B1[0,2];
        double c3x = B1[0,3];
        double c0y = B1[1,0];
        double c1y = B1[1,1];
        double c2y = B1[1,2];
        double c3y = B1[1,3];
        
        double d0x = B2[0,0];
        double d1x = B2[0,1];
        double d2x = B2[0,2];
        double d3x = B2[0,3];
        double d0y = B2[1,0];
        double d1y = B2[1,1];
        double d2y = B2[1,2];
        double d3y = B2[1,3];

        double u0 = c0x + c0y;
        double u1 = c1x + c1y;
        double u2 = c2x + c2y;
        double u3 = c3x + c3y;
        
        double v0 = d0x + d0y;
        double v1 = d1x + d1y;
        double v2 = d2x + d2y;
        double v3 = d3x + d3y;
        
        Matrix<double> Bezout = Matrix<double>.Build.DenseOfArray(new double[3, 3]
        {
            {u1 * v0 - u0 * v1,u2 * v0 - u0 * v2,u3 * v0 - u0 * v3},
            {u2 * v0 - u0 * v2, u2 * v1 - u1 * v2 + u3 * v0 - u0 * v3, u3 * v1 - u1 * v3},
            {u3 * v0 - u0 * v3, u3 * v1 - u1 * v3, u3 * v2 - u2 * v3}
        });
        return Bezout;
    }

}