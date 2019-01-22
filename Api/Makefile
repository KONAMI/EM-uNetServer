#######################################################################################

.SUFFIXES : .php .json

#######################################################################################

MAKEFILE_ENV := ${shell touch "Makefile.env"}

include Makefile.env

PROJECT      ?= EMNS-Api
API_FUNC     ?= TechInfo
API_FUNC_ARG ?= tests/TechInfoArg01.json
API_STAGE    ?= release
API_NAME     ?= $(shell cat serverless.yml | yq '.functions.${API_FUNC}.events[0].http.path' | tr -d "\"")
API_URL      ?= $(shell cat ApiInfo-${API_STAGE}.json | jq '.list[] | select (.url | endswith("${API_NAME}")) | .url '  | tr -d "\"")
API_KEY      ?= ${shell cat ApiInfo-${API_STAGE}.json | jq '.key' | tr -d "\""}

S3_BUCKET    ?= `echo ${PROJECT} | tr A-Z a-z`-data-${API_STAGE}

# Session側から構築後の情報をコピーしておく必要あり。
REDIS_API_URL ?= ${shell cat SessionApiInfo-${API_STAGE}.json | jq '.list[0].url' | tr -d "\"" } 
REDIS_API_KEY ?= ${shell cat SessionApiInfo-${API_STAGE}.json | jq '.key' | tr -d "\"" } 

# Measurement側の構築後の情報をコピーしておく必要あり。
DELAY_API_IP      ?= ${shell cat MeasurementInfo-${API_STAGE}.json | jq '.delay.ip' | tr -d "\"" }
DELAY_API_PORT    ?= ${shell cat MeasurementInfo-${API_STAGE}.json | jq '.delay.port' | tr -d "\"" }
BANDWIDTH_API_URL ?= ${shell cat MeasurementInfo-${API_STAGE}.json | jq '.bandwidth.url' | tr -d "\"" }

# LocalLambda実行時の環境変数設定   
LAMBDA_ENV_VAR  = export LAMBDA_SERVICE=${PROJECT}; 
LAMBDA_ENV_VAR += export LAMBDA_ENV=${API_STAGE}; 
LAMBDA_ENV_VAR += export S3_BUCKET=${S3_BUCKET};
LAMBDA_ENV_VAR += export REDIS_API_URL=${REDIS_API_URL};
LAMBDA_ENV_VAR += export REDIS_API_KEY=${REDIS_API_KEY};
LAMBDA_ENV_VAR += export DEBUG_API_URL=${API_URL}; 
LAMBDA_ENV_VAR += export DEBUG_API_KEY=${API_KEY}; 

#######################################################################################

default:

env:
	@echo "PROJECT       ?= ${PROJECT}"       > Makefile.env
	@echo "API_FUNC      ?= ${API_FUNC}"     >> Makefile.env
	@echo "API_FUNC_ARG  ?= ${API_FUNC_ARG}" >> Makefile.env
	@echo "API_STAGE     ?= ${API_STAGE}"    >> Makefile.env

Makefile.env: env

clean:
	-rm -rf arc
	-rm -rf obj
	-rm -rf bin
	-rm -rf .serverless
	-rm publish.zip
	-rm serverlessConfig.yml
	-rm ${PROJECT}Info-*.json
	-rm apiKey.txt
	-rm apiList.txt

##[ Import From Session ]#############################################################

SessionApiInfo-${API_STAGE}.json:
	cp ../Session/EMNS-SessionInfo-${API_STAGE}.json $@

##[ Lambda Build ]####################################################################

all: init build run

build:
	dotnet build --configuration Release

new:
	dotnet new console

init:
	dotnet restore

run:
	${LAMBDA_ENV_VAR} dotnet run

publish:
	dotnet publish --output arc

package: publish
#	dotnet lambda package --configuration Release --framework netcoreapp1.0
	(cd arc && zip ../publish.zip *)

add-library:
	dotnet add package ${PKG}

##[ Serverless Framework ]#############################################################

serverlessConfig.yml: SessionApiInfo-${API_STAGE}.json 
	echo "service: ${PROJECT}" > serverlessConfig.yml
	echo "redisApiUrl: ${REDIS_API_URL}" >> serverlessConfig.yml
	echo "redisApiKey: ${REDIS_API_KEY}" >> serverlessConfig.yml
	echo "s3Bucket: ${S3_BUCKET}" >> serverlessConfig.yml

api-all: serverlessConfig.yml
	serverless deploy -v --stage ${API_STAGE}

api:
	serverless deploy function -f ${API_FUNC} --stage ${API_STAGE}

api-clean:
	serverless remove -v --stage ${API_STAGE}

# Lambda 直接呼
api-test:
	serverless invoke --function ${API_FUNC} -p ${API_FUNC_ARG} --log --stage ${API_STAGE}

# LambdaをAPIGatway経由で呼ぶ場合（APIキーなし）
api-test-gw:
	curl -X POST -H 'Content-Type:application/json' -d @${API_FUNC_ARG} ${API_URL}

# LambdaをAPIGatway経由で呼ぶ場合（APIキーあり）
api-test-gwk:
	curl -X POST -H 'Content-Type:application/json' -d @${API_FUNC_ARG} ${API_URL} --header x-api-key:${API_KEY}

# あとから確認したい用
api-log:
	serverless logs --function ${API_FUNC} --stage ${API_STAGE}

export-api-list:
	serverless info --stage ${API_STAGE} | grep "https://" | awk '{print $$3}' > apiList.txt

export-api-key:
	serverless info --stage ${API_STAGE} | grep -a1 "api keys" | tail -n1 | cut -d":" -f2 | awk '{print $$1}' > apiKey.txt

export-measurement-list:
	@echo "bandwidth,${BANDWIDTH_API_URL}" > measurementList.txt
	@echo "delay,${DELAY_API_IP}:${DELAY_API_PORT}" >> measurementList.txt

api-info: export-api-list export-api-key export-measurement-list
	php tools/CreateApiInfo.php > ApiInfo-${API_STAGE}.json
	cat ApiInfo-${API_STAGE}.json | jq .
	aws s3 cp ApiInfo-${API_STAGE}.json s3://${S3_BUCKET}/InfoData/ApiInfo-${API_STAGE}.json

#######################################################################################

CLIENT_API_DIR ?= ../../EM-uNetMiner/Assets/EM-uNetPi-Client/EMNS-Api

syncClient:
	mkdir -p ${CLIENT_API_DIR}
	rsync -avz TechAuth/ ${CLIENT_API_DIR}/TechAuth
	rsync -avz TechInfo/ ${CLIENT_API_DIR}/TechInfo
	rsync -avz TechStore/ ${CLIENT_API_DIR}/TechStore
	rsync -avz TechUtils.cs ${CLIENT_API_DIR}/TechUtils.cs

#######################################################################################
