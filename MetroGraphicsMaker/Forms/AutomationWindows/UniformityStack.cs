using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Division_numbers.April2015
{

    public class Solver
    {
        /// <summary>
        /// Делимые
        /// </summary>
        Stack<Int32> dividends = new Stack<Int32>();
        
        /// <summary>
        /// Делители
        /// </summary>
        Stack<Int32> dividers = new Stack<Int32>();
        
        /// <summary>
        /// Частные
        /// </summary>
        Stack<Int32> quotients = new Stack<Int32>();
        
        /// <summary>
        /// Остатки от деления
        /// </summary>
        Stack<Int32> remainders = new Stack<Int32>();

        /// <summary>
        /// Отбивка в таблице
        /// </summary>
        String solidLine = new String('\u2550', 10);

        /*public void MakeHeader()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine("\u2554{0}\u2564{0}\u2564{0}\u2564{0}\u2557", solidLine);
            Console.WriteLine("\u2551{0,9} \u2502{1,9} \u2502{2,9} \u2502{3,10}\u2551", "Dividend", "Quotient", "Divider", "Remainder");
            Console.WriteLine("\u2560{0}\u256a{0}\u256a{0}\u256a{0}\u2563", solidLine);
        }*/

        public void MakeBody()
        {
            var localDividends = dividends.Reverse().ToArray();
            var localQuotients = quotients.Reverse().ToArray();
            var localDividers = dividers.Reverse().ToArray();
            var localRemainders = remainders.Reverse().ToArray();

            var rowIndex = 0;
            /*while (rowIndex < localDividends.Length - 1)
            {
                Console.WriteLine("\u2551{0,10}\u2502{1,10}\u2502{2,10}\u2502{3,10}\u2551",
                       localDividends[rowIndex], localQuotients[rowIndex], localDividers[rowIndex], localRemainders[rowIndex]);
                Console.WriteLine("\u2560{0}\u256a{0}\u256a{0}\u256a{0}\u2563", solidLine);
                rowIndex++;
            }
            Console.WriteLine("\u2551{0,10}\u2502{1,10}\u2502{2,10}\u2502{3,10}\u2551",
                       localDividends[rowIndex], localQuotients[rowIndex], localDividers[rowIndex], localRemainders[rowIndex]);
            Console.WriteLine("\u255a{0}\u2567{0}\u2567{0}\u2567{0}\u255d", solidLine);*/
        }

        public void MakeStacks(Int32 firstNumber, Int32 secondNumber)
        {
            dividends.Push(firstNumber);
            dividers.Push(secondNumber);

            while (dividers.Peek() > 0)
            {
                quotients.Push(dividends.Peek() / dividers.Peek());
                remainders.Push(dividends.Peek() % dividers.Peek());
                dividends.Push(dividers.Peek());
                dividers.Push(remainders.Peek());
            };

            // заплатка -- убрать при реальном использовании 
            quotients.Push(0);
            remainders.Push(0);
            // заплатка -- убрать при реальном использовании 
        }
    }

    class Equability
    {
        static void Main()
        {
            // Get start-up data
            Console.WriteLine("Enter your dividend or numetator (N) ..");
            var firstNumber = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Enter your divider or denominator (K) ..");
            var secondNumber = Int32.Parse(Console.ReadLine());

            var solver = new Solver();
            solver.MakeHeader();
            solver.MakeStacks(firstNumber, secondNumber);
            solver.MakeBody();
            
            Console.ReadKey();


            //--Imax;
            //Console.ReadKey();            
            //int[] Result = new int[firstNumber];
            //int[] PrevResult = new int[firstNumber];
            //PrevResult[1] = quotient[Imax];
            //Result[0]=0;
            //for (int k = Imax; k > 1; --k)
            //{
            //    int j = 1;
            //    for (int i = 1; i <= divider[k - 1]; ++i)
            //    {
                    
            //        if (PrevResult[j] == i)
            //        {
            //            Result[i] = Result[i - 1] + quotient[k-1] + 1;
            //            ++j;
            //        }
            //        else
            //        {
            //            Result[i] = Result[i - 1] + quotient[k-1];
            //        };

            //    };
            //    for (int i = 1; i <= divider[k - 1]; ++i)
            //    {
            //        PrevResult[i] = Result[i];
            //    }
            //}
        }
    }
}
