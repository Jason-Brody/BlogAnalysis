using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary1;

namespace App2
{


    public class Student : MarshalByRefObject,Name
    {
        private string name;
        public Student(string name)
        {
            this.name = name;
            Console.WriteLine("My Name is {0}", name);
        }

        public void MyName()
        {
            Console.WriteLine(name);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello");
        }
    }
}
