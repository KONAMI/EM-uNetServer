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

namespace kde.tech
{
    
public class LambdaRedis : LambdaBase<LambdaRedisArg>
{

    ILambdaContext m_ctx;
    LambdaRedisArg m_data;
	
    public override string Handler(LambdaRedisArg data, ILambdaContext context){
	m_ctx  = context;
	m_data = data;
	
	m_data.sw.Start();
	
	m_ctx.Log(String.Format("{0:D8} : Start Process", m_data.sw.ElapsedMilliseconds));
	
	string resp;
	
	if(data.role == "client"){
	    Task<RedisData.Response> result = ClientJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else if(data.role == "server"){
	    Task<RedisData.Response> result = ServerJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else {
	    var dummyResp = new RedisData.Response() {
		status = RedisError.E_CHAOS.ToString()
	    };
	    resp = JsonSerializer.ToJsonString(dummyResp);
	}

	m_ctx.Log(String.Format("{0:D8} : Complete Process", m_data.sw.ElapsedMilliseconds));

	return resp;
    }

    async Task<RedisData.Response> ClientJob(LambdaRedisArg data, ILambdaContext ctx){
	RedisData.Response ret = null;

	var env           = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service       = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");
	data.apiUrl       = System.Environment.GetEnvironmentVariable("DEBUG_API_URL");
	data.apiKey       = System.Environment.GetEnvironmentVariable("DEBUG_API_KEY");

	if(env != null){
	    ctx.Log("Environment [env] : " + env);
	}
	else { 
	    ctx.Log("Environment [env] : null");
	}	
	
	if(data.mode == "set")
	{
	    var checkRet = await RedisClientHelper.Set(ctx, data.apiUrl, data.apiKey, data.k, data.v, data.ttlSec);
	    ret = new RedisData.Response(){ status = checkRet.ToString(), k = data.k, v = data.v  };
	}
	else if(data.mode == "get")
	{
	    data.v = await RedisClientHelper.Get(ctx, data.apiUrl, data.apiKey, data.k);
	    ret = new RedisData.Response(){ status = RedisError.E_OK.ToString(), k = data.k, v = data.v };
	}
	else if(data.mode == "checkEqual")
	{
	    var checkRet = await RedisClientHelper.CheckValueEqual(ctx, data.apiUrl, data.apiKey, data.k, data.v);
	    ret = new RedisData.Response(){ status = checkRet.ToString(), k = data.k, v = data.v  };
	}
	else
	{ ret = await Task.Run(() => { return new RedisData.Response(){ status = RedisError.E_CHAOS.ToString() }; });}

	ctx.Log("Ret : " + ret.status + " > [ " + ret.k + " : " + ret.v + " ]"); 
	
	return ret;	    
    }
    
    
    async Task<RedisData.Response> ServerJob(LambdaRedisArg data, ILambdaContext ctx){
	RedisData.Response ret = null;

	var redisServer = new RedisServer(ctx);	

	var env           = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service       = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");
	data.redisURI     = System.Environment.GetEnvironmentVariable("REDIS_URI");
	
	if(env != null){
	    ctx.Log("Environment [env] : " + env);
	}
	else { 
	    ctx.Log("Environment [env] : null");
	}

	if(data.mode == "set")
	{ ret = redisServer.Set(data); }
	else if(data.mode == "get")
	{ ret = redisServer.Get(data); }
	else
	{ ret = await Task.Run(() => { return new RedisData.Response(){ status = RedisError.E_CHAOS.ToString() }; });}

	return ret;	    
    }
    
}

public class LambdaRedisArg : LambdaBaseArg
{
    /*==[ lambda args ]=====================================================================*/
    
    // header
    public string k  { get; set; } = "";
    public string v  { get; set; } = "";
    public int    ttlSec  { get; set; } = -1;
    
    /*==[ env params  ]=====================================================================*/

    public string redisURI { get; set; } = "";

    /*==[ debug params ]====================================================================*/

    public string apiUrl  { get; set; } = "";
    public string apiKey  { get; set; } = "";
    
    /*======================================================================================*/        
}
}
