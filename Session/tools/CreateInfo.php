<?php

$apiInfo = array();

$apiInfo["lastupdate"] = time();
$apiInfo["list"] = array();

$list = file("apiList.txt");

foreach($list as $url){
    $record = array();
    $tmp    = explode("/", trim($url));
    array_splice($tmp, 0, 4);
    $record["name"] = implode("/", $tmp);
    $record["url"]  = trim($url);
    array_push($apiInfo["list"], $record);
}

$apiInfo["key"] = trim(file_get_contents("apiKey.txt"));

//echo json_encode($apiInfo, JSON_PRETTY_PRINT);
echo json_encode($apiInfo);

