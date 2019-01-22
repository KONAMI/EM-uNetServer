using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.IO;
using Utf8Json;

namespace kde.tech
{

public class RedisData
{

    /*=========================================================*/

    public class Request
    {
	[DataMember(Name = "role")]
	public string role { get; set; } = "server";
	[DataMember(Name = "mode")]
	public string mode { get; set; }
	[DataMember(Name = "k")]
	public string k { get; set; } = "";
	// Save Mode Only
	[DataMember(Name = "v")]
	public string v { get; set; } = "";
	[DataMember(Name = "ttlSec")]
	public int ttlSec { get; set; } = -1;
    }
    
    public class Response
    {
	[DataMember(Name = "status")]
	public string status { get; set; }	
	[DataMember(Name = "meta")]
	public string meta { get; set; } = "";
	[DataMember(Name = "k")]
	public string k { get; set; } = "";
	[DataMember(Name = "v")]
	public string v { get; set; } = "";
	[DataMember(Name = "ttlSec")]
	public int ttlSec { get; set; } = -1;
    }
    
    /*=========================================================*/
    
    public RedisData(){
    }   
    
}
}
