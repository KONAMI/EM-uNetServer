EM-uNetServer - Session
======================================================================

概要
----------------------------------------------------------------------

AWSを使用した、シンプルなRedisによるSession管理構成

使用する構成/開発ツールは下記の通り

- Makefile 
- Server Less Framework
- Terraform
- jq
- yq（2.6.0以降）
- php
- .NET Core 2.0

> macOSでの動作を確認している

How To Build
----------------------------------------------------------------------

### 1.初期化

```
make env
```

### 2.ElastiCache 初期化 & 構築

> TF STATE 管理用の S3 Bucket がない場合は、適宜生成しておくこと。

```
make -C TechRedis-Terraform env TFSTATE_BUCKET=${YOUR_TFSTATE_BUCKET} \
AWS_ACCESS_KEY_ID=${YOUR_AWS_ACCESSS_KEY_ID} AWS_SECRET_ACCESS_KEY=${YOUR_AWS_SECRET_ACCESS_KEY}
make -C TechRedis-Terraform tf-init tf-all 
```

### 3.Session API 構築

```
make all package api-all api-info
```

### 4.動作確認

```
make api-test-gwk API_FUNC_ARG=tests/TechRedisSetArg01.json
make api-test-gwk API_FUNC_ARG=tests/TechRedisGetArg01.json
```

API経由で値のSet/Getが正常に行えていれば、OK！

