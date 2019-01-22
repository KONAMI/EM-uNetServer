using System;
using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

namespace kde.tech
{
    public static class AWSExtention {
	public static void Log(this ILambdaContext ctx,
			       string msg,
			       [CallerFilePath] string file = "",
			       [CallerLineNumber] int line = 0,
			       [CallerMemberName] string member = ""
			       ){
	    var s = string.Format("[ {0}:{1} - {2} ] {3}", Path.GetFileName(file), line, member, msg);
	    ctx.Logger.LogLine(s); 
	}

    }
}
