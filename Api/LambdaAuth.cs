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
    
public class LambdaAuth : LambdaBase<LambdaAuthArg>
{

    ILambdaContext m_ctx;
    LambdaAuthArg  m_data;
	
    public override string Handler(LambdaAuthArg data, ILambdaContext context){
	m_ctx  = context;
	m_data = data;
	
	m_data.sw.Start();
	
	m_ctx.Log(String.Format("{0:D8} : Start Process", m_data.sw.ElapsedMilliseconds));
	
	string resp;
	
	if(data.role == "client"){
	    Task<AuthData.Response> result = ClientJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else if(data.role == "server"){
	    Task<AuthData.Response> result = ServerJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else {
	    var dummyResp = new AuthData.Response() {
		status = AuthError.E_CHAOS.ToString()
	    };
	    resp = JsonSerializer.ToJsonString(dummyResp);
	}

	m_ctx.Log(String.Format("{0:D8} : Complete Process", m_data.sw.ElapsedMilliseconds));

	return resp;
    }

    async Task<AuthData.Response> ClientJob(LambdaAuthArg data, ILambdaContext ctx){
	AuthData.Response ret = null;

	var env           = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service       = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");
	data.apiUrl       = System.Environment.GetEnvironmentVariable("DEBUG_API_URL");
	data.apiKey       = System.Environment.GetEnvironmentVariable("DEBUG_API_KEY");
	
	var authClient = new AuthClient(data.apiUrl, data.apiKey, ctx);

	if(data.mode == "regist"){
	    ret = await authClient.Regist();
	}
	else if(data.mode == "login"){
	    ret = await authClient.Login(data.userProfile.guid);
	}
	else if(data.mode == "createTakeoverCode"){
	    ret = await authClient.CreateTakeoverCode(data.userProfile.uid, data.sessCode);
	}
	else if(data.mode == "consumeTakeoverCode"){
	    ret = await authClient.ConsumeTakeoverCode(data.userProfile.uid, data.takeoverCode);
	}
	else
	{ ret = await Task.Run(() => { return new AuthData.Response(){ status = AuthError.E_CHAOS.ToString() }; }); }
	
	return ret;	    
    }

    
    async Task<AuthData.Response> ServerJob(LambdaAuthArg data, ILambdaContext ctx){
	AuthData.Response ret = null;
	
	var authServer = new AuthServer(ctx);

	// 環境変数に依存する処理
	var env      = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service  = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");

	data.s3Bucket    = System.Environment.GetEnvironmentVariable("S3_BUCKET");
	data.redisApiUrl = System.Environment.GetEnvironmentVariable("REDIS_API_URL");
	data.redisApiKey = System.Environment.GetEnvironmentVariable("REDIS_API_KEY");

	ctx.Log("s3Bucket : " + data.s3Bucket);
	ctx.Log("redisApiUrl : " + data.redisApiUrl);
	ctx.Log("redisApiKey : " + data.redisApiKey);
	
	if     (data.mode == "regist"){ ret = await authServer.Regist(data); }
	else if(data.mode == "login"){ ret = await authServer.Login(data); }
	else if(data.mode == "createTakeoverCode"){ ret = await authServer.CreateTakeoverCode(data); }
	else if(data.mode == "consumeTakeoverCode"){ ret = await authServer.ConsumeTakeoverCode(data); }
	else
	{ ret = await Task.Run(() => { return new AuthData.Response(){ status = AuthError.E_CHAOS.ToString() }; }); }

	return ret;	    
    }
    
}

public class LambdaAuthArg : LambdaBaseArg
{
    /*==[ lambda args ]=====================================================================*/
    
    public UserData.Profile userProfile  { get; set; } = new UserData.Profile();
    public string           sessCode     { get; set; } = "";
    public string           takeoverCode { get; set; } = "";
    
    /*==[ env params  ]=====================================================================*/

    public string redisApiUrl  { get; set; } = "";
    public string redisApiKey  { get; set; } = "";
    
    public string s3Bucket     { get; set; } = "";
    public string s3DirObject  { get; set; } = "UserProfile/";
 
    /*==[ debug params ]====================================================================*/

    public string apiUrl  { get; set; } = "";
    public string apiKey  { get; set; } = "";
   
    /*======================================================================================*/        
}
}
