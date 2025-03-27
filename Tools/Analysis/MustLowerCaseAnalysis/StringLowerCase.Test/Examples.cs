using System;

namespace Analyzers1.Sample;

public class examples
{
    public string Xxx = "sdfsdf";
    public class MyCompanyClass
    {
        private string mXxx = "sdfsdfs";

        public void DoJob([MustLowerAttribute] string aaa)
        {
            
        }
        
        public void DoJob2([MustLower] string aaa)
        {
            
        }

        public void Test()
        {
            //这里的参数应该报告错误.并给出代码修复提示.
            string param1 = "XXXXDDDFFFF";
            DoJob(param1);
            DoJob2(param1);
        }
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class MustLowerAttribute : Attribute
{
    
}