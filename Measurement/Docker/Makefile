########################################################################

REPO_NAME ?= em-unetserver-measurement
TARGET    ?= emns-measurement

########################################################################

default:

all: dummyFile build 

dummyFile:
	dd if=/dev/zero of=htdocs/512K.dat bs=1k count=512
	dd if=/dev/zero of=htdocs/1M.dat bs=1k count=1024
	dd if=/dev/zero of=htdocs/2M.dat bs=1k count=2048
	dd if=/dev/zero of=htdocs/4M.dat bs=1k count=4096
	dd if=/dev/zero of=htdocs/8M.dat bs=1k count=8192

build:
	docker build -t ${TARGET} .

ls:
	docker images

run:
	docker run --rm -p 5730:5730/udp -p 4000:80 ${TARGET}

stop:

ps:
	docker ps -a

clear:
	docker rm `docker ps -aq`

rm:
	docker rmi ${IMAGE_ID}

########################################################################

REPO_URI  ?= ${shell aws ecr describe-repositories | jq '.repositories[] | select  (.repositoryName | endswith("${REPO_NAME}")) | .repositoryUri' | tr -d "\"" }

ecr_all: ecr_login ecr_tag ecr_push

ecr_login:
	aws ecr get-login --no-include-email --region ap-northeast-1 > login.sh
	bash login.sh

ecr_repo:
	aws ecr create-repository --repository-name ${REPO_NAME}

ecr_tag:
	docker tag ${TARGET}:latest ${REPO_URI}:latest

ecr_push:
	docker push ${REPO_URI}:latest

ecr_repo_info:
	@echo ${REPO_URI}

########################################################################
