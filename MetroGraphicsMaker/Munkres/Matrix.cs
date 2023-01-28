// Matrix.cs
// Konstantin Filipchenko <konstantin.filipchenko@gmail.com>
// 2013-2-25 / MITS

using System;
using System.Text;
using MiscUtil;
using Messages;
using System.Collections.Generic;

namespace Munkres
{
    public class Matrix<T> where T : struct
    {

        UInt32 m_rows = 0;
        UInt32 m_columns = 0;
        T[,] m_data = null;


        public Matrix() {}

        public Matrix(UInt32 rows, UInt32 columns)
        {
            Resize(rows, columns);
        }

        // TODO: Необходимо решить вопрос освобождения памяти

        public Matrix(T[,] inputArray)
        {
            FromArray(inputArray);
        }


        public void FromArray(T[,] array)
        {
            UInt32 rows, cols;
            if (
                ((rows = Convert.ToUInt32(array.GetLength(0))) > 0)
                &&
                ((cols = Convert.ToUInt32(array.GetLength(1))) > 0)
                )
            {
                /*
                if (rows == m_rows && cols == m_columns)
                {
                    for (UInt32 i = 0; i < rows; ++i)
                        for (UInt32 j = 0; j < cols; ++j)
                            m_data[i, j] = array[Convert.ToInt32(i), Convert.ToInt32(j)];
                }
                else
                {
                    
                    Messages.Logger.output("Не написано", GetType().FullName);
                    
                    UInt32  minC = cols, 
                            maxC = m_columns, 
                            minR = rows, 
                            maxR = m_rows;
                    
                    if (cols > m_columns)
                    {
                        minC = m_columns; maxC = cols;
                    }

                    if (rows > m_rows)
                    {
                        minR = m_rows; maxR = rows;
                    }

                    // cols и rows -- размеры массива входных данных

                    for (UInt32 i = 0; i < minR; ++i)
                        for (UInt32 j = 0; j < minC; ++j)
                            m_data[i, j] = array[Convert.ToInt32(i), Convert.ToInt32(j)];
                    
                }
                 */

                Logger.Output(String.Format("Переписано! m_data[{0}, {1}]; array[{2}, {3}]", m_rows, m_columns, rows, cols), GetType().FullName);

                if (m_columns != cols || m_rows != rows)
                {
                    Resize(rows, cols);
                }

                for (UInt32 i = 0; i < rows; ++i)
                    for (UInt32 j = 0; j < cols; ++j)
                        m_data[i, j] = array[Convert.ToInt32(i), Convert.ToInt32(j)];
            }

        }

        public void Minimize()
        {
            for (UInt32 i = 0; i < m_rows; ++i)
            {
                var min = m_data[i, 0];
                for (UInt32 j = 1; j < m_columns; ++j)
                    if (Operator.LessThan(m_data[i, j], min))
                        min = m_data[i, j];

                for (UInt32 j = 0; j < m_columns; ++j)
                    m_data[i, j] = Operator.Subtract(m_data[i, j], min);
            }

            for (UInt32 j = 0; j < m_columns; ++j)
            {
                var min = m_data[0, j];
                for (UInt32 i = 1; i < m_rows; ++i)
                    if (Operator.LessThan(m_data[i, j], min))
                        min = m_data[i, j];

                for (UInt32 i = 0; i < m_rows; ++i)
                    m_data[i, j] = Operator.Subtract(m_data[i, j], min);
            }
        }

        public void Resize(UInt32 rows, UInt32 columns)
        {
            if (m_data == null)
            {
                m_data = new T[rows, columns];
                m_rows = rows;
                m_columns = columns;

                Clear();
            }
            else
            {
                var storage = new T[rows, columns];

                var minrows = (rows < m_rows) ? rows : m_rows;
                var mincols = (columns < m_columns) ? columns : m_columns;
                for (UInt32 i = 0; i < minrows; ++i)
                    for (UInt32 j = 0; j < mincols; ++j)
                        storage[i, j] = At(i, j);

                m_data = storage;
                m_rows = rows;
                m_columns = columns;
            }
        }

        /// <summary>
        /// Identy this instance. Throw InvalidOperationException.
        /// </summary>
        public void Identy()
        {
            Clear();

            var min = MinimalSize();
            for (var index = 0; index < min; ++index)
                m_data[index, index] = Operator.Convert<Int32, T>(1);

        }
        
        /// <summary>
        /// Clear this instance. Throw InvalidOperationException.
        /// </summary>
        public void Clear()
        {
            if (m_data == null) return;
            for (var i = 0; i < m_rows; ++i)
                for (var j = 0; j < m_columns; ++j)
                    m_data[i, j] = Operator.Convert<Int32, T>(0);
            // @TODO: Обычно подразумевается, что-то другое
        }

        public UInt32 MinimalSize()
        {
            return (m_columns < m_rows) ? m_columns : m_rows;
        }

        public UInt32 Rows()
        {
            return m_rows;
        }

        public UInt32 Columns()
        {
            return m_columns;
        }


        // TODO: ПЕРЕПИСАТЬ!!!
        public T At(UInt32 row, UInt32 column)
        {
            //Console.WriteLine("At({0},{1}) : m_rows = {2}, m_cols = {3}", row, column, m_rows, m_columns);
            //Logger.output(String.Format("At({0},{1}) : m_rows = {2}, m_cols = {3}", row, column, m_rows, m_columns), GetType().Name);

            if (m_data != null && row < m_rows && column < m_columns)
                return m_data[row, column];

            throw new Exception("Null or indexOutOfBounds");
        }

        public T At(KeyValuePair<int, int> coords)
        {
            var row = (uint)coords.Key;
            var column = (uint)coords.Value;

            return At(row, column);
        }

        public void Set(UInt32 row, UInt32 column, T value)
        {
            m_data[row, column] = value;
        }

        public void Set(KeyValuePair<int, int> coords, T value)
        {
            // TODO: try-catch section
            var row = Convert.ToUInt32(coords.Key);
            var column = Convert.ToUInt32(coords.Value);

            //uint row = (uint)coords.Key;
            //uint column = (uint)coords.Value;

            m_data[row, column] = value;
        }

        public void Add(UInt32 row, UInt32 column, T value)
        {
            m_data[row, column] = Operator.Add(m_data[row, column], value);
        }

        public void Sub(UInt32 row, UInt32 column, T value)
        {
            m_data[row, column] = Operator.Subtract(m_data[row, column], value);
        }

        /// <summary>
        /// Trace this instance. Throw InvalidOperationException.
        /// </summary>
        public T Trace()
        {
            var value = Operator.Convert<Int32, T>(0);
            try
            {
                var min = MinimalSize();
                for (var index = 0; index < min; ++index)
                    value = Operator.Add(value, m_data[index, index]);
            }
            catch (NullReferenceException)
            {
            }

            return value;
        }

        public Matrix<T> Transpose()
        {
            var newRows = m_columns;
            var newCols = m_rows;

            if (m_rows != m_columns)
            {
                var max = (m_columns > m_rows) ? m_columns : m_rows;
                Resize(max, max);
            }

            for (var i = 0; i < m_rows; ++i)
            {
                for (var j = i + 1; j < m_columns; ++j)
                {
                    var currentValue = m_data[i, j];
                    m_data[i, j] = m_data[j, i];
                    m_data[j, i] = currentValue;
                }
            }

            if (newRows != newCols)
                Resize(newRows, newCols);

            return this;
        }

        public Matrix<T> Product(Matrix<T> otherMatrix)
        {
            Matrix<T> outMatrix = null;
            if (m_columns == otherMatrix.m_rows)
            {
                outMatrix = new Matrix<T>(m_rows, otherMatrix.m_columns);
                for (UInt32 i = 0; i < outMatrix.m_rows; ++i)
                    for (UInt32 j = 0; j < outMatrix.m_columns; ++j)
                        for (UInt32 k = 0; k < m_columns; ++k)
                            outMatrix.m_data[i, j] = Operator.Add(
                                outMatrix.At(i, j), Operator.Multiply(At(i, k), otherMatrix.At(k, j))
                            );
            }
            return outMatrix;
        }

        public void FillRandomValues(Int32 max, Boolean isFloatingPointNumbers = false)
        {
            var random = new Random();

            for (var i = 0; i < m_rows; ++i)
                for (var j = 0; j < m_columns; ++j)
                    m_data[i, j] = Operator.Convert<Double, T>(
                        Convert.ToDouble(random.Next(max))
                        + (isFloatingPointNumbers ? random.NextDouble() : 0)
                    );
        }

        public String ToString(String delimiter = ";", Boolean delimiterOnEndOfRow = false)
        {
            var sb = new StringBuilder(String.Format("{2}Matrix.m_data[{0}, {1}]:{2}", m_rows, m_columns, Environment.NewLine));
            for (UInt32 i = 0; i < m_rows; ++i)
            {
                sb.Append("\t");
                for (UInt32 j = 0; j < m_columns - 1; ++j)
                    sb.AppendFormat("{0}{1} ", (Operator.Equal(At(i, j), Operator.Convert<Int32, T>(Int32.MaxValue))) ? "∞" : At(i, j).ToString(), delimiter);
                sb.AppendFormat("{0}{1}{2}", (Operator.Equal(At(i, m_columns - 1), Operator.Convert<Int32, T>(Int32.MaxValue))) ? "∞" : At(i, m_columns - 1).ToString(), (delimiterOnEndOfRow ? delimiter : ""), Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}

