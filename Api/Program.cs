using System;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.Core;

namespace kde.tech
{
    class Program {
	
        static void Main(string[] args){

	    /*==[ Info > Server ]======================================================================*/
	    _TestRun(new LambdaInfo(), new LambdaInfoArg(){ role = "server", mode = "load"});
	    /*==[ Info > Client ]======================================================================*/
	    _TestRun(new LambdaInfo(), new LambdaInfoArg(){ role = "client", mode = "load"});
        }
	
	static void TestRun<T_FUNC, T_ARG>(T_FUNC lambdaFunc, T_ARG lambdaArg)
	    where T_FUNC: LambdaBase<T_ARG>
	    where T_ARG: LambdaBaseArg
	{
	    Console.WriteLine(lambdaFunc.ToString() + " >> " + lambdaArg.role + " : " + lambdaArg.mode + " >> Test.");
	    var ret = lambdaFunc.Handler(lambdaArg, new TestLambdaContext());
	    Console.WriteLine("Ret > " +  ret);
	    Console.WriteLine("======================================================");
	}

        static void _TestRun<T_FUNC, T_ARG>(T_FUNC lambdaFunc, T_ARG lambdaArg)
            where T_FUNC: LambdaBase<T_ARG>
            where T_ARG: LambdaBaseArg
        {
            //Console.WriteLine(lambdaFunc.ToString() + " >> " + lambdaArg.role + " : " + lambdaArg.mode + " >> Bypass.");
            //Console.WriteLine("======================================================");
        }
    }
}
