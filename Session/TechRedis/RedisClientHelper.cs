using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;
using System.Net.Http;
using Utf8Json;

#if INCLUDE_AWS_CODE
using Amazon.Lambda.Core;
#else
using UnityEngine;
#endif

namespace kde.tech
{

public static class RedisClientHelper
{

    public static async Task<RedisError> CheckValueEqual(ILambdaContext ctx, string apiUrl, string apiKey,
							 string key, string value){

	if((key == "") || (value == "")){ return RedisError.E_INVALID_ARGS; }
	
	try {
	    var client = new RedisClient(apiUrl, apiKey, ctx);
	    var resp   = await client.Get(key);
	
	    if(resp.status != RedisError.E_OK.ToString()){ return RedisError.E_NOTFOUND; }
	    else if(resp.v != value)                     { return RedisError.E_NOTMATCH; }
	    else                                         { return RedisError.E_OK; }
	}
	catch(Exception e){
	    ctx.Log("Exception : " + e.ToString());
	    return RedisError.E_CRITICAL;
	}
    }

    public static async Task<RedisError> Set(ILambdaContext ctx, string apiUrl, string apiKey,
					     string key, string value, int ttlSec = -1){

	if(key == ""){ return RedisError.E_INVALID_ARGS; }
	
	try {
	    var client = new RedisClient(apiUrl, apiKey, ctx);
	    var resp   = await client.Set(key, value, ttlSec);
	    return (RedisError)Enum.Parse(typeof(RedisError), resp.status, true);
	}
	catch(Exception e){
	    ctx.Log("Exception : " + e.ToString());
	    return RedisError.E_CRITICAL;
	}
    }
    
    public static async Task<string> Get(ILambdaContext ctx, string apiUrl, string apiKey,
					 string key){
	string ret = "";
	
	if(key == ""){ return ret; }
	
	try {
	    var client = new RedisClient(apiUrl, apiKey, ctx);
	    var resp   = await client.Get(key);	    
	    var status = (RedisError)Enum.Parse(typeof(RedisError), resp.status, true);
	    if(status == RedisError.E_OK){ ret = resp.v; }
	}
	catch(Exception e){
	    ctx.Log("Exception : " + e.ToString());
	}
	
	return ret;	
    }
    
}
}
