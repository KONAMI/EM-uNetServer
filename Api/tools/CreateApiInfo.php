<?php

$apiInfo = array();

$apiInfo["lastupdate"] = time();
$apiInfo["status"] = array( 
    "maintStatus"  => 1, // 0 : 正常, 1 > : メンテナンス
    "maintMessage" => "Some services are down."
);
$apiInfo["list"] = array();

$list = file("apiList.txt");

foreach($list as $url){
    $record = array();
    $tmp    = explode("/", trim($url));
    array_splice($tmp, 0, 4);
    $record["name"] = implode("/", $tmp);
    $record["url"]  = trim($url);
    $record["isAvailable"] = 1;
    array_push($apiInfo["list"], $record);
}

$list = file("measurementList.txt");

foreach($list as $url){
    $record = array();
    $tmp    = explode(",", trim($url));
    $record["name"] = $tmp[0];
    $record["url"]  = $tmp[1];
    $record["isAvailable"] = 1;
    array_push($apiInfo["list"], $record);
}

$apiInfo["key"]  = trim(file_get_contents("apiKey.txt"));

//echo json_encode($apiInfo, JSON_PRETTY_PRINT);
echo json_encode($apiInfo);

