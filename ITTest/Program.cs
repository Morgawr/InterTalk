using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterTalk;
using System.Diagnostics;

namespace ITTest
{
    class TestingClass
    {
        public int value = 0;
    }

    class Program
    {
        public int a = 0;
        public int b = 10000000;

        private void AddToA()
        {
            a++;
        }

        private void TakeFromB()
        {
            b--;
        }


        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            InterManagerCore im = InterManagerCore.Instance;
            Program p = new Program();

            Console.WriteLine("Registering..");

            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                im.Register(0, "a", new Action(p.AddToA), null);
                im.Register(0, "b", new Action(p.TakeFromB), null);
            }
            sw.Stop();
            Console.WriteLine("Registered in " + sw.Elapsed.Seconds + " seconds and " + sw.Elapsed.Milliseconds + " ms.");

            sw = new Stopwatch();
            Console.WriteLine("Invoking..");
            sw.Start();
            im.Invoke(0, "a", null, true);
            sw.Stop();
            Console.WriteLine("With multithreading: " + sw.Elapsed.Seconds + " seconds and "+sw.Elapsed.Milliseconds+" ms.");
            
            sw = new Stopwatch();

            sw.Start();
            im.Invoke(0, "b", null, false);
            sw.Stop();
            Console.WriteLine("With singlethreading: " + sw.Elapsed.Seconds + " seconds and " + sw.Elapsed.Milliseconds + " ms.");

            Console.ReadKey();

        }
    }
}

