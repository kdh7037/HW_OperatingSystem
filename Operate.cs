using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operate
{
    class Program
    {
        private static volatile string _str = "abc";
        private static volatile int _val = 0;
        private static volatile int _valp = 0;

        static void AppendStr(string newstr)
        {
            while (true)
            {
                var original = Interlocked.CompareExchange(ref _str, null, null);
                var newString = original + newstr;
                if (Interlocked.CompareExchange(ref _str, newString, original) == original)
                    break;
            }
        }

        static void AddValue(int value)
        {
            while (true)
            {
                var orgVal = _val;
                var newVal = orgVal + value;
                if (Interlocked.CompareExchange(ref _val, newVal, orgVal) == orgVal)
                    break;
            }
        }

        static void Thread1()
        {
            for (int i = 0; i < 100000; i++)  // Main 스레드와 병행 수행
                AddValue(1);
        }

        static void Thread2()
        {
            for (int i = 0; i < 100000; i++)
                _valp++;
        }

        static void test1()
        {
            Thread t1 = new Thread(Thread1);
            Thread t2 = new Thread(Thread1);
            Thread t3 = new Thread(Thread1);
            Thread t4 = new Thread(Thread1);
            Thread t5 = new Thread(Thread1);

            Thread h1 = new Thread(Thread2);
            Thread h2 = new Thread(Thread2);
            Thread h3 = new Thread(Thread2);
            Thread h4 = new Thread(Thread2);
            Thread h5 = new Thread(Thread2);

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();
            t5.Join();

            h1.Start();
            h2.Start();
            h3.Start();
            h4.Start();
            h5.Start();

            h1.Join();
            h2.Join();
            h3.Join();
            h4.Join();
            h5.Join();
            Console.WriteLine("1번 문제 결과입니다.");
            Console.WriteLine("AddVal()을 활용하여 원자적으로 값을 증가시킨 결과 : " + _val);
            Console.WriteLine("단순히 1씩 증가시킨 결과 : " + _valp);
        }

        public class ListNode
        {
            public ListNode prevNode;
            public ListNode nextNode;

            public ListNode()
            {
                prevNode = null;
                nextNode = null;
            }
        }

        static int mut = 0;

        public class HeadNode
        {
            public ListNode firstNode = null;
            public ListNode backNode = null;

            public void AddFirstNode()
            {
                while (Interlocked.CompareExchange(ref mut, 1, 0) == 1) ;
                ListNode A = new ListNode();
                if (firstNode == null)
                {
                    firstNode = A;
                    backNode = A;
                }
                else
                {
                    ListNode B = firstNode;
                    firstNode = A;
                    A.nextNode = B;
                    B.prevNode = A;
                }
                mut = 0;
            }

            public void AddBackNode()
            {
                while (Interlocked.CompareExchange(ref mut, 1, 0) == 1) ;
                ListNode A = new ListNode();
                if (backNode == null)
                {
                    firstNode = A;
                    backNode = A;
                }
                else
                {
                    ListNode B = backNode;
                    backNode = A;
                    A.prevNode = B;
                    B.nextNode = A;
                }
                mut = 0;
            }

            public void DeleteFirstNode()
            {
                while (Interlocked.CompareExchange(ref mut, 1, 0) == 1) ;
                firstNode.nextNode.prevNode = null;
                firstNode = firstNode.nextNode;
                mut = 0;
            }

            public void DeleteBackNode()
            {
                while (Interlocked.CompareExchange(ref mut, 1, 0) == 1) ;
                backNode.prevNode.nextNode = null;
                backNode = backNode.prevNode;
                mut = 0;
            }

            public void PrintData()
            {
                int a = 0;
                for (ListNode A = firstNode; A != null; A = A.nextNode)
                    a++;
                Console.WriteLine("리스트는 {0}개 입니다.", a);
            }
        }

        static HeadNode head = new HeadNode();

        static void Thread3()
        {
            for (int i = 0; i < 100000; i++)
                head.AddFirstNode();
        }
        static void Thread4()
        {
            for (int i = 0; i < 100000; i++)
                head.AddBackNode();
        }
        static void Thread5()
        {
            for (int i = 0; i < 50000; i++)
                head.DeleteFirstNode();
        }
        static void Thread6()
        {
            for (int i = 0; i < 50000; i++)
                head.DeleteBackNode();
        }

        static void test2()
        {
            Thread add = new Thread(Thread3);
            Thread s1 = new Thread(Thread3);
            Thread s2 = new Thread(Thread4);
            Thread s3 = new Thread(Thread5);
            Thread s4 = new Thread(Thread6);

            add.Start();
            add.Join();

            s1.Start();
            s2.Start();
            s3.Start();
            s4.Start();
            s1.Join();
            s2.Join();
            s3.Join();
            s4.Join();

            head.PrintData();
        }

        static Mutex mMutex = new Mutex();

        public class HeadNodeM
        {
            public ListNode firstNode = null;
            public ListNode backNode = null;

            public void AddFirstNode()
            {
                mMutex.WaitOne();
                ListNode A = new ListNode();
                if (firstNode == null)
                {
                    firstNode = A;
                    backNode = A;
                }
                else
                {
                    ListNode B = firstNode;
                    firstNode = A;
                    A.nextNode = B;
                    B.prevNode = A;
                }
                mMutex.ReleaseMutex();
            }

            public void AddBackNode()
            {
                mMutex.WaitOne();
                ListNode A = new ListNode();
                if (backNode == null)
                {
                    firstNode = A;
                    backNode = A;
                }
                else
                {
                    ListNode B = backNode;
                    backNode = A;
                    A.prevNode = B;
                    B.nextNode = A;
                }
                mMutex.ReleaseMutex();
            }

            public void DeleteFirstNode()
            {
                mMutex.WaitOne();
                firstNode.nextNode.prevNode = null;
                firstNode = firstNode.nextNode;
                mMutex.ReleaseMutex();
            }

            public void DeleteBackNode()
            {
                mMutex.WaitOne();
                backNode.prevNode.nextNode = null;
                backNode = backNode.prevNode;
                mMutex.ReleaseMutex();
            }

            public void PrintData()
            {
                int a = 0;
                for (ListNode A = firstNode; A != null; A = A.nextNode)
                    a++;
                Console.WriteLine("리스트는 {0}개 입니다.", a);
            }
        }

        static HeadNodeM headM = new HeadNodeM();

        static void Thread7()
        {
            for (int i = 0; i < 100000; i++)
                headM.AddFirstNode();
        }
        static void Thread8()
        {
            for (int i = 0; i < 100000; i++)
                headM.AddBackNode();
        }
        static void Thread9()
        {
            for (int i = 0; i < 50000; i++)
                headM.DeleteFirstNode();
        }
        static void Thread10()
        {
            for (int i = 0; i < 50000; i++)
                headM.DeleteBackNode();
        }

        static void test3()
        {
            Thread add = new Thread(Thread7);
            Thread s1 = new Thread(Thread7);
            Thread s2 = new Thread(Thread8);
            Thread s3 = new Thread(Thread9);
            Thread s4 = new Thread(Thread10);

            add.Start();
            add.Join();

            s1.Start();
            s2.Start();
            s3.Start();
            s4.Start();
            s1.Join();
            s2.Join();
            s3.Join();
            s4.Join();

            headM.PrintData();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("2015920005 김대현 운영체제 과제");
            test1();
            test2();
            test3();
        }
    }
}