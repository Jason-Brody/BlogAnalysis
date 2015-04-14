using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary1;
using System.Linq.Expressions;


namespace App1
{
 

    class Program
    {
        static void Main(string[] args)
        {
            var a = AppDomain.CurrentDomain.GetAssemblies();
            AppDomain app2 = AppDomain.CreateDomain("Test");
            AppDomain app3 = AppDomain.CreateDomain("Test");
            app2.ExecuteAssembly("App2.exe");
            //var stu = app2.CreateInstance("App2", "App2.Student", true, System.Reflection.BindingFlags.CreateInstance, null, new object[] { "Zhou" }, null, null);
            Name n = (Name)app2.CreateInstanceAndUnwrap("App2", "App2.Student", true, System.Reflection.BindingFlags.CreateInstance, null, new object[] { "Zhou" }, null, null);
            
            Name n1 = (Name)app3.CreateInstanceAndUnwrap("App2", "App2.Student", true, System.Reflection.BindingFlags.CreateInstance, null, new object[] { "Yang" }, null, null);
            n.MyName();
            n1.MyName();
            AppDomain.Unload(app2);
            n1.MyName();
            AppDomain.Unload(app3);
            Console.ReadLine();
            
        }
    }
}
