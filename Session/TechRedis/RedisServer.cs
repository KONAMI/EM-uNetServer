#if INCLUDE_AWS_CODE
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Utf8Json;
using StackExchange.Redis;

namespace kde.tech
{

public class RedisServer
{
    
    public RedisServer(ILambdaContext ctx){
	m_ctx = ctx;
    }
    
    ILambdaContext m_ctx;
    
    public RedisData.Response Get(LambdaRedisArg data){
	
	var resp = new RedisData.Response(){ status = RedisError.E_CHAOS.ToString(), k = data.k };
	try{
	    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(data.redisURI);
	    IDatabase cache = redis.GetDatabase();
	    var redisResp = cache.StringGetWithExpiry(data.k);	    
	    if(redisResp.Value.IsNullOrEmpty){ resp.v = ""; resp.status = RedisError.E_NOTFOUND.ToString(); }
	    else                 {
		resp.v = redisResp.Value.ToString();
		if(redisResp.Expiry != null){
		    TimeSpan tmp = (TimeSpan)redisResp.Expiry; 
		    resp.ttlSec  = (int)tmp.TotalSeconds;
		}
		resp.status  = RedisError.E_OK.ToString();
	    }
	}
	catch (Exception ex){
	    m_ctx.Log(ex.Message);	
	    resp.status = RedisError.E_CRITICAL.ToString();
	    resp.meta   = ex.Message;
	}
	return resp;
    }

    public RedisData.Response Set(LambdaRedisArg data){
	
	var resp    = new RedisData.Response(){ status = RedisError.E_CHAOS.ToString(), k = data.k  };
	try {

	    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(data.redisURI);
	    IDatabase cache = redis.GetDatabase();

	    m_ctx.Log("SetTTL > " + data.ttlSec.ToString());
			    
	    if(data.ttlSec > 0){
		if(cache.StringSet(data.k, data.v, TimeSpan.FromSeconds(data.ttlSec))){
		    resp.status = RedisError.E_OK.ToString();		    
		}
		else {
		    resp.status = RedisError.E_CRITICAL.ToString();		    
		}
	    }
	    else {
		if(cache.StringSet(data.k, data.v)){
		    resp.status = RedisError.E_OK.ToString();
		}
		else {
		    resp.status = RedisError.E_CRITICAL.ToString();
		}
	    }
	}
	catch(Exception ex)
	{
	    m_ctx.Log(ex.ToString());
	    resp.status = RedisError.E_CRITICAL.ToString();
	    resp.meta   = ex.Message;
	}
	return resp;
    }

}
}
#endif
