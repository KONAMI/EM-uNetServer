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

public class RedisClient
{

    string m_apiUrl;
    string m_apiKey;
    
    public RedisClient(string apiUrl, string apiKey){
	m_apiUrl = apiUrl;
	m_apiKey = apiKey;
    }

#if INCLUDE_AWS_CODE
    ILambdaContext m_ctx;
    public RedisClient(string apiUrl, string apiKey, ILambdaContext ctx){
        m_apiUrl = apiUrl;
	m_apiKey = apiKey;
        m_ctx    = ctx;
    }
    void Log(string msg){
        m_ctx.Log(msg);
    }
#else
    void Log(string msg){
        UnityEngine.Debug.Log(msg);
    }
#endif
    
    public async Task<RedisData.Response> Get(string k){
	var resp = new RedisData.Response(){ status = RedisError.E_CHAOS.ToString() };

	using (var client = new HttpClient()){
	    	    
            try {

		var request = new HttpRequestMessage(HttpMethod.Post, m_apiUrl);
		request.Content = new StringContent
		    (
		     JsonSerializer.ToJsonString(new RedisData.Request(){
			     role      = "server",
			     mode      = "get",
			     k         = k
			 })
		     ,Encoding.UTF8, @"application/json"
		     );
		
		request.Headers.Add(@"x-api-key", m_apiKey);
		
		var response = await client.SendAsync(request);
		
                var responseContent = await response.Content.ReadAsStringAsync();
    
                if(!String.IsNullOrEmpty(responseContent)){
		    Log("Got Resp >> " + responseContent);
                    resp = JsonSerializer.Deserialize<RedisData.Response>(responseContent);
                }
                else {
                    resp.status = RedisError.E_CRITICAL.ToString();
                }
            }
            catch(Exception e){
                Log("Exception : " + e.ToString());
                resp.status = RedisError.E_CRITICAL.ToString();
            }
	}
	return resp;
    }

    public async Task<RedisData.Response> Set(string k, string v, int ttlSec = -1){
	var resp = new RedisData.Response(){ status = RedisError.E_CHAOS.ToString() };

	using (var client = new HttpClient()){
	    	    
            try {
		var request = new HttpRequestMessage(HttpMethod.Post, m_apiUrl);
		request.Content = new StringContent
		    (
		     JsonSerializer.ToJsonString(new RedisData.Request(){
			     role      = "server",
			     mode      = "set",
			     k         = k,
			     v         = v,
			     ttlSec    = ttlSec
			 })
		     ,Encoding.UTF8, @"application/json"
		     );
		
		request.Headers.Add(@"x-api-key", m_apiKey);
		
		var response = await client.SendAsync(request);
		
                var responseContent = await response.Content.ReadAsStringAsync();
    
                if(!String.IsNullOrEmpty(responseContent)){
		    Log("Got Resp >> " + responseContent);
                    resp = JsonSerializer.Deserialize<RedisData.Response>(responseContent);
                }
                else {
                    resp.status = RedisError.E_CRITICAL.ToString();
                }
            }
            catch(Exception e){
                Log("Exception : " + e.ToString());
                resp.status = RedisError.E_CRITICAL.ToString();
            }
	}
	return resp;
    }

    
}
}
