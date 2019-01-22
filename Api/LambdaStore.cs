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
    
public class LambdaStore : LambdaBase<LambdaStoreArg>
{

    ILambdaContext m_ctx;
    LambdaStoreArg m_data;
	
    public override string Handler(LambdaStoreArg data, ILambdaContext context){
	m_ctx  = context;
	m_data = data;
	
	m_data.sw.Start();
	
	m_ctx.Log(String.Format("{0:D8} : Start Process", m_data.sw.ElapsedMilliseconds));
	m_ctx.Log(data.dataName);
	
	string resp;
	
	if(data.role == "client"){
	    Task<StoreData.Response> result = ClientJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else if(data.role == "server"){
	    Task<StoreData.Response> result = ServerJob(data, context);
	    m_ctx.Log(String.Format("{0:D8} : Wait Async Method", m_data.sw.ElapsedMilliseconds));
	    resp = JsonSerializer.ToJsonString(result.Result);
	}
	else {
	    var dummyResp = new StoreData.Response() {
		status = StoreError.E_CHAOS.ToString()
	    };
	    resp = JsonSerializer.ToJsonString(dummyResp);
	}

	m_ctx.Log(String.Format("{0:D8} : Complete Process", m_data.sw.ElapsedMilliseconds));

	return resp;
    }

    async Task<StoreData.Response> ClientJob(LambdaStoreArg data, ILambdaContext ctx){
	StoreData.Response ret = null;

	var env           = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service       = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");
	data.apiUrl       = System.Environment.GetEnvironmentVariable("DEBUG_STORE_API_URL");
	data.apiKey       = System.Environment.GetEnvironmentVariable("DEBUG_STORE_API_KEY");

	ctx.Log(data.mode + " >> Target Object : " + data.s3Path);	
	
	var storeClient = new StoreClient(data.apiUrl, data.apiKey, ctx);
	
	if(data.mode == "load")
	{
	    ret = await storeClient.Load(data.sessCode, data.dataName, data.dataIndex);
	}
	else if(data.mode == "save")
	{
	    ret = await storeClient.Save(data.sessCode, data.dataBody, data.dataType, data.dataName, data.dataIndex);
	}
	else
	{ ret = await Task.Run(() => { return new StoreData.Response(){ status = StoreError.E_CHAOS.ToString() }; });}

	return ret;	    
    }
    
    
    async Task<StoreData.Response> ServerJob(LambdaStoreArg data, ILambdaContext ctx){
	StoreData.Response ret = null;

	var storeServer = new StoreServer(ctx);	

	// 環境変数に依存する処理
	var env     = System.Environment.GetEnvironmentVariable("LAMBDA_ENV");
	var service = System.Environment.GetEnvironmentVariable("LAMBDA_SERVICE");

	data.s3Bucket    = System.Environment.GetEnvironmentVariable("S3_BUCKET");
	
	ctx.Log("s3Bucket : " + data.s3Bucket);
	ctx.Log(data.mode + " >> Target Object : " + data.s3Path);	
	
	if     (data.mode == "load"){ ret = await storeServer.Load(data); }
	else if(data.mode == "save"){ ret = await storeServer.Save(data); }
	else
	{ ret = await Task.Run(() => { return new StoreData.Response(){ status = StoreError.E_CHAOS.ToString() }; });}

	return ret;	    
    }
    
}

public class LambdaStoreArg : LambdaBaseArg
{
    /*==[ lambda args ]=====================================================================*/
    
    // header
    public string sessCode   { get; set; } = "";
    public string userCode   { get; set; } = "dummy"; 
    public string dataName   { get; set; } = "name";     
    public int    dataIndex  { get; set; } = 1;    
    public StoreData.Type dataType { get; set; } = StoreData.Type.TEXT;

    // body
    // 親クラスの dataBody プロパティを使う。
    
    /*======================================================================================*/

    public string s3Bucket     { get; set; } = "";
    public string s3DirObject  { get; set; } = "UserStore/";
    public string s3Path { get {
	    return s3DirObject + userCode + "/" + dataName + "_" + dataIndex.ToString("D4") + dataType.ToExtention();
	}
    }
    
    /*==[ debug params ]====================================================================*/

    public string apiUrl  { get; set; } = "";
    public string apiKey  { get; set; } = "";

    /*======================================================================================*/
    
}
}
