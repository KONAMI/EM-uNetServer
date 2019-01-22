using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Utf8Json;

[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace kde.tech
{
    
public abstract class LambdaBase<T_ARG> where T_ARG : LambdaBaseArg {
    
    public virtual string Handler(T_ARG data, ILambdaContext context){ return ""; }

    protected async Task PostProcess(T_ARG data, ILambdaContext ctx){

	var env     = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");
	if((data.postLambda == "") || (env == null) || (service == null)){ return; } 

	var funcName = service + "-" + env + "-" + data.postLambda;
	
	ctx.Log("PostProcess Lambda  : " + funcName);

	var payload = new PostLambdaArg()
	    {
		role     = "server",
		mode     = data.postLambdaMode,
		dataBody = data.postLambdaArg
	    };
	    
	using (var client = new AmazonLambdaClient(Amazon.RegionEndpoint.APNortheast1))
	{
	    if(data.isWaitPostProcess){
		var request = new InvokeRequest()
		    {
			FunctionName   = funcName,
			InvocationType = InvocationType.RequestResponse,
			Payload        = JsonSerializer.ToJsonString(payload)
		    };
		var response = await client.InvokeAsync(request);
	    }
	    else {
		var request = new InvokeRequest()
		    {
			FunctionName   = funcName,
			InvocationType = InvocationType.Event,
			Payload        = JsonSerializer.ToJsonString(payload)
		    };
		var response = await client.InvokeAsync(request);
	    }	    
	}
    }    
}

public class PostLambdaArg {
    [DataMember(Name = "role")]
    public string role { get; set; } 
    [DataMember(Name = "mode")]
    public string mode { get; set; } 
    [DataMember(Name = "dataBody")]
    public string dataBody { get; set; }     
}
        
public abstract class LambdaBaseArg
{
    /*==[ lambda args ]=====================================================================*/
    
    public string evt  { get; set; } = "default";
    public string role { get; set; } = "server";

    // header
    public string mode         { get; set; } = ""; 
    
    // body
    public string dataBody     { get; set; } = ""; 

    /* 
     * post処理
     * 処理後に呼び出すLambda、func名のみ指定（{service}-{env}- はランタイムで付与する） 
     */
    public string postLambda     { get; set; } = ""; 
    public string postLambdaArg  { get; set; } = ""; 
    public string postLambdaMode { get; set; } = ""; 
    public bool   isWaitPostProcess { get; set; } = false;
    
    /*======================================================================================*/

    public RegionEndpoint region { get { return Amazon.RegionEndpoint.APNortheast1; }}
   
    /*======================================================================================*/
    
    public uint timestamp {
	get {
	    var timespan = m_now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	    return (uint)timespan.TotalSeconds;
	}
    }
    public Stopwatch sw { get { return m_sw; }}

    /*======================================================================================*/
    
    protected DateTime  m_now; 
    protected Stopwatch m_sw;
    
    public LambdaBaseArg(){
	m_now = DateTime.UtcNow;
	m_sw  = new Stopwatch();	
    }
    
    /*======================================================================================*/        
}
}
