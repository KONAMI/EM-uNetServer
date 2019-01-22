using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Lambda.Core;
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

//[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace kde.tech
{
    
public class LambdaInfo : LambdaBase<LambdaInfoArg>
{

    ILambdaContext m_ctx;
    LambdaInfoArg m_data;
	
    public override string Handler(LambdaInfoArg data, ILambdaContext context){
	m_ctx  = context;
	m_data = data;
	
	m_data.sw.Start();
	
	m_ctx.Log(String.Format("{0:D8} : Start Process", m_data.sw.ElapsedMilliseconds));
	
	string resp;
	
	if(data.role == "client"){
	    Task<InfoData.Response> result = ClientJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else if(data.role == "server"){
	    Task<InfoData.Response> result = ServerJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else {
	    var dummyResp = new InfoData.Response() {
		status = InfoError.E_CHAOS.ToString()
	    };
	    resp = JsonSerializer.ToJsonString(dummyResp);
	}

	m_ctx.Log(String.Format("{0:D8} : Complete Process", m_data.sw.ElapsedMilliseconds));

	return resp;
    }

    async Task<InfoData.Response> ClientJob(LambdaInfoArg data, ILambdaContext ctx){
	InfoData.Response ret = null;

	var infoClient = new InfoClient("");
	
	if(data.mode == "load")
	{ ret = await infoClient.LoadInfo(); }
	else
	{ ret = await Task.Run(() => { return new InfoData.Response(){ status = InfoError.E_INVALID_ARGS.ToString() }; });}

	return ret;	    
    }

    
    async Task<InfoData.Response> ServerJob(LambdaInfoArg data, ILambdaContext ctx){
	InfoData.Response ret = null;
	
	var infoServer = new InfoServer();

	// 環境変数に依存する処理
	var env     = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");

	data.s3Bucket    = System.Environment.GetEnvironmentVariable("S3_BUCKET");	

	if(env != null){
	    ctx.Log("Environment [env] : " + env);
	    data.s3InfoObject = "InfoData/ApiInfo-" + env + ".json";
	}
	else {
	    ctx.Log("Environment [env] : null");
	    data.s3InfoObject = "InfoData/ApiInfo-dev.json";
	}
	
	if(data.mode == "load")
	{ ret = await infoServer.LoadInfo(data, ctx); }
	else
	{ ret = await Task.Run(() => { return new InfoData.Response(){ status = InfoError.E_INVALID_ARGS.ToString() }; });}
	
	return ret;	    
    }
    
}

public class LambdaInfoArg : LambdaBaseArg
{
    /*==[ lambda args ]=====================================================================*/
    
    public string         s3Bucket     { get; set; } = "";
    public string         s3InfoObject { get; set; }
   
    /*======================================================================================*/        
}
}
