﻿
namespace CSparse.Double
{
    using CSparse.Properties;
    using CSparse.Storage;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Dense matrix stored in column major order.
    /// </summary>
    [DebuggerDisplay("DenseMatrix {RowCount}x{ColumnCount}")]
    [Serializable]
    public class DenseMatrix : DenseColumnMajorStorage<Double>
    {
        /// <summary>
        /// Initializes a new instance of the DenseMatrix class.
        /// </summary>
        public DenseMatrix(int rows, int columns)
            : this(rows, columns, new double[rows * columns])
        {
        }

        /// <summary>
        /// Initializes a new instance of the DenseMatrix class.
        /// </summary>
        public DenseMatrix(int rows, int columns, double[] values)
            : base(rows, columns, values)
        {
        }

        /// <inheritdoc />
        public override double L1Norm()
        {
            double sum, norm = 0.0;

            for (var j = 0; j < columnCount; j++)
            {
                sum = 0.0;
                for (var i = 0; i < rowCount; i++)
                {
                    sum += Math.Abs(Values[(j * rowCount) + i]);
                }
                norm = Math.Max(norm, sum);
            }

            return norm;
        }
        
        /// <inheritdoc />
        public override double InfinityNorm()
        {
            var r = new double[rowCount];

            for (var j = 0; j < columnCount; j++)
            {
                for (var i = 0; i < rowCount; i++)
                {
                    r[i] += Math.Abs(Values[(j * rowCount) + i]);
                }
            }

            double norm = r[0];

            for (int i = 1; i < rowCount; i++)
            {
                if (r[i] > norm)
                {
                    norm = r[i];
                }
            }

            return norm;
        }

        /// <inheritdoc />
        public override double FrobeniusNorm()
        {
            double sum = 0.0, norm = 0.0;

            int length = rowCount * columnCount;

            for (int i = 0; i < length; i++)
            {
                sum = Math.Abs(Values[i]);
                norm += sum * sum;
            }

            return Math.Sqrt(norm);
        }

        /// <inheritdoc />
        public override DenseColumnMajorStorage<double> Clone()
        {
            var values = (double[])this.Values.Clone();

            return new DenseMatrix(rowCount, columnCount, values);
        }

        /// <inheritdoc />
        public override void Multiply(double[] x, double[] y)
        {
            var A = Values;

            int rows = rowCount;
            int cols = columnCount;

            for (int i = 0; i < rows; i++)
            {
                double sum = 0.0;

                for (int j = 0; j < cols; j++)
                {
                    sum += A[(j * rows) + i] * x[j];
                }

                y[i] = sum;
            }
        }

        /// <inheritdoc />
        public override void Multiply(double alpha, double[] x, double beta, double[] y)
        {
            var A = Values;

            int rows = rowCount;
            int cols = columnCount;

            for (int i = 0; i < rows; i++)
            {
                double sum = 0.0;

                for (int j = 0; j < cols; j++)
                {
                    sum += A[(j * rows) + i] * x[j];
                }

                y[i] = beta * y[i] + alpha * sum;
            }
        }

        /// <inheritdoc />
        public override void TransposeMultiply(double[] x, double[] y)
        {
            var A = Values;

            int rows = rowCount;
            int cols = columnCount;
            
            for (int j = 0; j < cols; j++)
            {
                int col = j * rows;

                double sum = 0.0;

                for (int i = 0; i < rows; i++)
                {
                    sum += A[col + i] * x[i];
                }

                y[j] = sum;
            }
        }

        /// <inheritdoc />
        public override void TransposeMultiply(double alpha, double[] x, double beta, double[] y)
        {
            var A = Values;

            int rows = rowCount;
            int cols = columnCount;

            for (int j = 0; j < cols; j++)
            {
                y[j] = beta * y[j];
            }
            
            for (int j = 0; j < cols; j++)
            {
                int col = j * rows;

                double sum = 0.0;

                for (int i = 0; i < rows; i++)
                {
                    sum += A[col + i] * x[i];
                }

                y[j] = beta * y[j] +  alpha * sum;
            }
        }

        /// <inheritdoc />
        public override void Add(DenseColumnMajorStorage<double> other, DenseColumnMajorStorage<double> result)
        {
            int rows = this.rowCount;
            int columns = this.columnCount;

            // check inputs
            if (rows != other.RowCount || columns != other.ColumnCount)
            {
                throw new ArgumentException();
            }

            var target = result.Values;

            var a = this.Values;
            var b = other.Values;

            int length = rows * columns;

            for (int i = 0; i < length; i++)
            {
                target[i] = a[i] + b[i];
            }
        }

        /// <inheritdoc />
        public override void Multiply(DenseColumnMajorStorage<double> other, DenseColumnMajorStorage<double> result)
        {
            var A = Values;
            var B = other.Values;
            var C = result.Values;

            int m = rowCount; // rows of matrix A
            int n = other.ColumnCount;
            int o = columnCount;

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double sum = 0.0;

                    for (int k = 0; k < o; ++k)
                    {
                        sum += A[(k * m) + i] * B[(j * o) + k];
                    }

                    C[(j * m) + i] += sum;
                }
            }
        }

        /// <inheritdoc />
        public override void PointwiseMultiply(DenseColumnMajorStorage<double> other, DenseColumnMajorStorage<double> result)
        {
            if (RowCount != other.RowCount || ColumnCount != other.ColumnCount)
            {
                throw new ArgumentException(Resources.MatrixDimensions);
            }

            if (RowCount != result.RowCount || ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException(Resources.MatrixDimensions);
            }

            var x = Values;
            var y = other.Values;

            var target = result.Values;

            int length = target.Length;

            for (int i = 0; i < length; i++)
            {
                target[i] = x[i] * y[i];
            }
        }

        /// <inheritdoc />
        public override bool Equals(Matrix<double> other, double tolerance)
        {
            if (rowCount != other.RowCount || columnCount != other.ColumnCount)
            {
                return false;
            }

            var dense = other as DenseColumnMajorStorage<double>;

            if (dense == null)
            {
                return false;
            }

            int length = rowCount * columnCount;

            var otherValues = dense.Values;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(Values[i] - otherValues[i]) > tolerance)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
