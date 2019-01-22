EM-uNetServer - Api
======================================================================

概要
----------------------------------------------------------------------

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

### 0.事前準備

- Session, Measurement 側を先に構築しておく
- Measurement 側に関しては生成後に、下記のようなJsonを定義し、Api/MeasurementInfo-${API_STAGE}.json として配置すること。

```
{"bandwidth":{"url":"http:\/\/203.0.113.10"},"delay":{"ip":"203.0.113.11","port":5730}}
```

### 1.初期化

```
make env
```

### 2.S3 初期化 & 構築

> TF STATE 管理用の S3 Bucket がない場合は、適宜生成しておくこと。

```
make -C TechApi-Terraform env TFSTATE_BUCKET=${YOUR_TFSTATE_BUCKET} \
AWS_ACCESS_KEY_ID=${YOUR_AWS_ACCESSS_KEY_ID} AWS_SECRET_ACCESS_KEY=${YOUR_AWS_SECRET_ACCESS_KEY}
make -C TechApi-Terraform tf-init tf-all 
```

### 3.Session API 構築

```
make all package api-all api-info
```

### 4.動作確認

```
make api-test-gwk API_FUNC_ARG=tests/TechInfoArg01.json
```

それぞれのAPIでE_OKが返っていればOK！

