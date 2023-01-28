// Solver.cs
// Konstantin Filipchenko <konstantin.filipchenko@gmail.com>
// 2013-2-28 / MITS

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Messages;
using MiscUtil;

namespace Munkres
{
    enum MState
    {
        NORMAL = 0x0,
        STAR = 0x1,
        PRIME = 0x2
    }
    
    public class Solver<T> where T : struct
    {
        public Solver()
        {
            steps = new Func<Boolean>[] {
                () => false, step_1, step_2, step_3, step_4, step_5
            };

            currentStep = 1;
        }

        public Boolean Solve(Matrix<T> data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            
            Logger.Output(String.Format("data [r={1} x c={0}]", data.Columns(), data.Rows()), GetType().Name);

            matrix = data;
            
            Logger.Output(String.Format("matrix [r={1} x c={0}]", matrix.Columns(), matrix.Rows()), GetType().Name);
            
            /** NOTE: Заменяем бесконечное значение веса, на значение большее на 1, чем наибольшее небесконечное. Проверяем наличие отрицательных весов. В случае их присутствия увеличиваем все веса на значение наибольшего по абсолютному значению отрицательного веса. Иными словами сводим матрицу к приемлемому виду (неотрицательные и небесконечные элементы). */

            var maxValue = Operator.NotEqual(matrix.At(0, 0), Operator.Convert<Int32, T>(Int32.MaxValue)) ? matrix.At(0,0) : Operator.Convert<Int32, T>(0);
            var minValue = Operator.NotEqual(matrix.At(0, 0), Operator.Convert<Int32, T>(Int32.MinValue)) ? matrix.At(0, 0) : Operator.Convert<Int32, T>(0);
            var isNegative = false;

            for (UInt32 row = 0; row < matrix.Rows(); ++row)
                for (UInt32 column = 0; column < matrix.Columns(); ++column)
                {
                    if (
                        (
                            //Operator.NotEqual(matrix.At(row, column), Operator.Convert<Double, T>(Double.PositiveInfinity)) ||
                            Operator.NotEqual(matrix.At(row, column), Operator.Convert<Int32, T>(Int32.MaxValue))
                        )
                        && Operator.GreaterThan(matrix.At(row, column), maxValue)
                        )
                        maxValue = matrix.At(row, column);

                    if (
                        (isNegative |=
                            Operator.LessThan(matrix.At(row, column), Operator.Convert<Int32, T>(default(Int32))))
                        && Operator.LessThan(matrix.At(row, column), minValue)
                        )
                        minValue = matrix.At(row, column);
                }

            maxValue = Operator.Add(maxValue, Operator.Convert<Int32, T>(1));
            minValue = Operator.Multiply(minValue, Operator.Convert<Int32, T>(-1));

            for (UInt32 row = 0; row < matrix.Rows(); ++row)
                for (UInt32 column = 0; column < matrix.Columns(); ++column)
                {
                    if (
                        // Operator.Equal(matrix.At(row, column), Operator.Convert<Double, T>(Double.PositiveInfinity)) ||
                        Operator.Equal(matrix.At(row, column), Operator.Convert<Int32, T>(Int32.MaxValue)))
                        matrix.Set(row, column, maxValue);

                    if (isNegative)
                        matrix.Add(row, column, minValue);
                }

            /** NOTE: Конец замены. Вычитаем из каждого элемента строки наименьший в строке элемент, затем -- из каждого элемента столбца наименьший в столбце. Такие преобразования предположительно способствуют более быстрой сходимости. */

            matrix.Minimize();

            /** Конец предварительных преобразований. Создаём остальные необходимые элементы. */

            mask = new Matrix<MState>(matrix.Rows(), matrix.Columns());

            row_mask = new Boolean[matrix.Rows()];
            for (var i = 0; i < row_mask.Length; ++i)
                row_mask[i] = false;

            col_mask = new Boolean[matrix.Columns()];
            for (var i = 0; i < col_mask.Length; ++i)
                col_mask[i] = false;

            /** Запускаем алгоритм. */

            try
            {
                while (steps[currentStep]())
                {
                    Logger.Output(String.Format("next step = {0}", currentStep), GetType().Name);
                    Console.WriteLine("next step = {0}", currentStep);
                }
            }
            catch (Exception e)
            {
                Logger.Output(String.Format("Error!\n{0}", e), GetType().Name);
                Console.WriteLine("Error!\n{0}", e);
                return false;
            }

            return true;
        }

        public void ShowMask(bool flag = false)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}ShowMask():{0}", Environment.NewLine);
            if (flag)
            {
                for (UInt32 i = 0; i < mask.Rows(); ++i)
                {
                    for (UInt32 j = 0; j < mask.Columns(); ++j)
                    {
                        var value = (Operator.Equal(matrix.At(i, j), Operator.Convert<Int32, T>(Int32.MaxValue)))
                            ? "∞"
                            : matrix.At(i, j).ToString();
                        switch (mask.At(i, j))
                        {
                            case MState.NORMAL:
                                sb.AppendFormat(" {0}, ", value);
                                break;
                            case MState.STAR:
                                sb.AppendFormat("[{0}],", value);
                                break;
                            case MState.PRIME:
                                sb.AppendFormat("({0}),", value);
                                break;
                        }
                    }
                sb.AppendLine();
            }

            sb.AppendFormat("{0}row_mask:{0}\t", Environment.NewLine);
            foreach (var el in row_mask)
                sb.AppendFormat("{0}, ", el);

            sb.AppendFormat("{0}col_mask:{0}\t", Environment.NewLine);
            foreach (var el in col_mask)
                sb.AppendFormat("{0}, ", el);
        }
        else
        {
            for (UInt32 i = 0; i < mask.Rows(); ++i)
            {
                for (UInt32 j = 0; j < mask.Columns(); ++j)
                    sb.AppendFormat("{0}\t", matrix.At(i, j));
                sb.AppendLine();
            }
        }
            

        var result = sb.ToString();
            // Console.Write(result);
            Logger.Output(result, GetType().FullName, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        Boolean FindUncoveredElement(T element, ref UInt32 row, ref UInt32 column)
        {
            for (row = 0; row < matrix.Rows(); ++row)
                if (!row_mask[row])
                    for (column = 0; column < matrix.Columns(); ++column)
                        if (!col_mask[column])
                            if (Operator.Equal(matrix.At(row, column), element))
                                return true;

            return false;
        }

        Boolean step_1()
        {
            Console.WriteLine("step_1: M.Rows = {0}, M.Cols = {1}, mask[{2} x {3}]", matrix.Rows(), matrix.Columns(), mask.Rows(), mask.Columns());
            Logger.Output("step_1", GetType().Name);
            for (UInt32 row = 0; row < matrix.Rows(); row++)
                for (UInt32 column = 0; column < matrix.Columns(); column++)
                    if (Operator.Equal(matrix.At(row, column), Operator.Convert<Int32, T>(0)))
                    {
                        var isStarred = false;
                        for (UInt32 nrow = 0; nrow < matrix.Rows(); nrow++)
                            if (isStarred = (mask.At(nrow, column) == MState.STAR))
                                break;

                        if (!isStarred)
                            for (UInt32 ncol = 0; ncol < matrix.Columns(); ncol++)
                                if (isStarred = (mask.At(row, ncol) == MState.STAR))
                                    break;

                        if (!isStarred)
                            mask.Set(row, column, MState.STAR);
                    }
            Console.WriteLine("ShowMask { ");
            ShowMask(true);
            Console.WriteLine(" } ShowMask");
            currentStep = 2;
            return true;
        }

        Boolean step_2()
        {
            Console.WriteLine("step_2");
            Logger.Output("step_2", GetType().FullName);
            UInt32 coverCount = 0;
            for (UInt32 row = 0; row < matrix.Rows(); row++)
                for (UInt32 column = 0; column < matrix.Columns(); column++)
                    if (mask.At(row, column) == MState.STAR)
                    {
                        col_mask[column] = true;
                        coverCount++;
                    }

            if (coverCount >= matrix.MinimalSize())
            {
                ShowMask();
                Console.WriteLine("step_2 return false");
                Logger.Output("step_2 return false", GetType().Name);
                currentStep = 0;
                return false;
            }

            ShowMask();

            currentStep = 3;
            return true;
        }

        Boolean step_3()
        {
            Console.WriteLine("step_3");
            /*
            Main Zero Search

             1. Find an uncovered Zero in the distance matrix and prime it. If no such zero exists, go to Step 5
             2. If No Zero* exists in the row of the Zero', go to Step 4.
             3. If a Zero* exists, cover this row and uncover the column of the Zero*. Return to Step 3.1 to find a new Zero.
            */
            if (FindUncoveredElement(Operator.Convert<Int32, T>(0), ref saveRow, ref saveCol))
            {
                mask.Set(saveRow, saveCol, MState.PRIME); // prime it.
            }
            else
            {
                currentStep = 5;
                return true;
            }

            for (UInt32 ncol = 0; ncol < matrix.Columns(); ncol++)
                if (mask.At(saveRow, ncol) == MState.STAR)
                {
                    row_mask[saveRow] = true; //cover this row and
                    col_mask[ncol] = false; // uncover the column containing the starred zero
                    currentStep = 3; // repeat
                    return true;
                }

            currentStep = 4; // no starred zero in the row containing this primed zero
            return true;
        }

        Boolean step_4()
        {
            Console.WriteLine("step_4");

            var sequence = new List<KeyValuePair<Int32, Int32>>();

            // use saverow, savecol from step 3.
            var zero_0  = new KeyValuePair<Int32, Int32>(Convert.ToInt32(saveRow), Convert.ToInt32(saveCol));
            var zero_1  = new KeyValuePair<Int32, Int32>(-1, -1);
            var zero_2N = new KeyValuePair<Int32, Int32>(-1, -1);

            sequence.Add(zero_0);

            UInt32 row, col = saveCol;
            /*
            Increment Set of Starred Zeros

            1. Construct the ``alternating sequence'' of primed and starred zeros:

                    Zero0 : Unpaired Zero' from Step 4.2 
                    Zero1 : The Zero* in the column of Zero0
                    Zero[2N] : The Zero' in the row of Zero[2N-1], if such a zero exists 
                    Zero[2N+1] : The Zero* in the column of Zero[2N]

                The sequence eventually terminates with an unpaired Zero' = Zero[2N] for some N.
            */
            Boolean madePair;
            do
            {
                madePair = false;
                for (row = 0; row < matrix.Rows(); row++)
                    if (mask.At(row, col) == MState.STAR)
                    {
                        zero_1 = new KeyValuePair<Int32, Int32>(Convert.ToInt32(row), Convert.ToInt32(col));

                        if (sequence.Contains(zero_1))
                            continue;

                        madePair = true;
                        sequence.Add(zero_1);
                        break;
                    }

                if (!madePair)
                    break;

                madePair = false;

                for (col = 0; col < matrix.Columns(); col++)
                    if (mask.At(row, col) == MState.PRIME)
                    {
                        zero_2N = new KeyValuePair<Int32, Int32>(Convert.ToInt32(row), Convert.ToInt32(col));

                        if(sequence.Contains(zero_2N))
                            continue;

                        madePair = true;
                        sequence.Add(zero_2N);
                        break;
                    }
            } while (madePair);

            foreach (var coordinate in sequence)
            {
                // 2. Unstar each starred zero of the sequence.
                if (mask.At(coordinate) == MState.STAR)
                    mask.Set(coordinate, MState.NORMAL);

                // 3. Star each primed zero of the sequence,
                // thus increasing the number of starred zeros by one.
                if (mask.At(coordinate) == MState.PRIME)
                    mask.Set(coordinate, MState.STAR);
            }

            // 4. Erase all primes, uncover all columns and rows, 
            for (UInt32 nrow = 0; nrow < mask.Rows(); nrow++)
                for (UInt32 ncol = 0; ncol < mask.Columns(); ncol++)
                    if (mask.At(nrow, ncol) == MState.PRIME)
                        mask.Set(nrow, ncol, MState.NORMAL);

            for (UInt32 i = 0; i < matrix.Rows(); i++)
                row_mask[i] = false;

            for (UInt32 i = 0; i < matrix.Columns(); i++)
                col_mask[i] = false;

            // and return to Step 2. 
            currentStep = 2;
            return true;
        }

        Boolean step_5()
        {
            Console.WriteLine("step_5");

            /*
            New Zero Manufactures

             1. Let h be the smallest uncovered entry in the (modified) distance matrix.
             2. Add h to all covered rows.
             3. Subtract h from all uncovered columns
             4. Return to Step 3, without altering stars, primes, or covers. 
            */

            T h = Operator.Convert<Int32, T>(0);
            for (UInt32 row = 0; row < matrix.Rows(); row++)
                if (!row_mask[row])
                    for (UInt32 col = 0; col < matrix.Columns(); col++)
                        if ( !col_mask[col] 
                            && ( Operator.GreaterThan(h, matrix.At(row, col)) 
                                && Operator.NotEqual(matrix.At(row, col), Operator.Convert<Int32, T>(0)) 
                                || Operator.Equal(h, Operator.Convert<Int32, T>(0)) 
                                )
                            )
                            h = matrix.At(row, col);

            /* Ни какого отношения к ускорению это не имеет, так как это основа венгерского алгоритма. Читай описание Асанов, Баранский, Расин -- Дискретная математика. Графы, матроиды, алгоритмы (глава 12, особенно параграф 12.4). */
            for (UInt32 row = 0; row < matrix.Rows(); row++)
                if (row_mask[row])
                    for (UInt32 col = 0; col < matrix.Columns(); col++)
                    {
                        matrix.Add(row, col, h);
                        Console.WriteLine("Add: ({0}, {1}) {2}", row, col, h);
                    }

            for (UInt32 col = 0; col < matrix.Columns(); col++)
                if (!col_mask[col])
                    for (UInt32 row = 0; row < matrix.Rows(); row++)
                    {
                        matrix.Sub(row, col, h);
                        Console.WriteLine("Sub: ({0}, {1}) {2}", row, col, h);
                    }

            ShowMask(true);

            currentStep = 3;
            return true;
        }

        public List<Answer<T>> GetAnswer()
        {
            var result = new List<Answer<T>>();
            for (UInt32 i = 0; i < mask.Rows(); ++i)
                for (UInt32 j = 0; j < mask.Columns(); ++j)
                    if (mask.At(i, j) == MState.STAR)
                        result.Add(new Answer<T>
                        {
                            Row = (int) i,
                            Column = (int) j,
                            Value = matrix.At(i, j)
                        });
            return result;
        }

        public Matrix<T> getMatrix()
        {
            return matrix;
        }

        Matrix<MState> mask;
        Matrix<T> matrix;
        readonly Func<Boolean>[] steps;
        Boolean[] row_mask;
        Boolean[] col_mask;
        UInt32 saveCol = 0;
        UInt32 saveRow = 0;
        UInt32 currentStep;
    }

    public struct Answer<T>
    {
        public Int32 Row;

        public Int32 Column;

        public T Value;
    }
}

