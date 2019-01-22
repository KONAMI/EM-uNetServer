#####################################################################################

MAKEFILE_ENV          := ${shell touch "Makefile.env"}

include Makefile.env
include ../Makefile.env

CURRENT_DIR            = ${shell basename `pwd`}
TERRAFORM             ?= terraform

TFSTATE_BUCKET        ?= 
AWS_ACCESS_KEY_ID     ?= 
AWS_SECRET_ACCESS_KEY ?=

PROJECT_NAME          := `echo ${PROJECT} | tr A-Z a-z`
PROJECT_ENV           ?= release

CLUSTER_ID      ?= ${shell cat ClusterId.txt}
CLUSTER_NODE_ID ?= ${shell cat ClusterNodeId.txt}

#####################################################################################

default:

clean:
	rm tfplan
	rm terraform.tfvars
	rm -rf .terraform

Makefile.env:
	${MAKE} env

env:
	@echo "TFSTATE_BUCKET        ?= ${TFSTATE_BUCKET}" > Makefile.env
	@echo "AWS_ACCESS_KEY_ID     ?= ${AWS_ACCESS_KEY_ID}" >> Makefile.env
	@echo "AWS_SECRET_ACCESS_KEY ?= ${AWS_SECRET_ACCESS_KEY}" >> Makefile.env
	@echo "PROJECT_NAME          ?= ${PROJECT_NAME}" >> Makefile.env
	@echo "PROJECT_ENV           ?= ${PROJECT_ENV}" >> Makefile.env

terraform.tfvars: Makefile.env
	echo "aws_access_key = \"${AWS_ACCESS_KEY_ID}\"" > terraform.tfvars
	echo "aws_secret_key = \"${AWS_SECRET_ACCESS_KEY}\"" >> terraform.tfvars
	echo "tfstate_bucket = \"${TFSTATE_BUCKET}\"" >> terraform.tfvars
	echo "tfstate_object = \"${CURRENT_DIR}\"" >> terraform.tfvars
	echo "title          = \"${PROJECT_NAME}\"" >> terraform.tfvars
	echo "env            = \"${PROJECT_ENV}\"" >> terraform.tfvars

tf-init: terraform.tfvars
	${TERRAFORM} init \
		-backend-config "bucket=${TFSTATE_BUCKET}" \
		-backend-config "key=${CURRENT_DIR}" \
		-backend-config "region=ap-northeast-1" \

tf-test:
	${TERRAFORM} validate 

tf-plan:
	${TERRAFORM} plan -out=tfplan -input=false 

tf-apply:
	${TERRAFORM} apply -input=false tfplan 

tf-all: tf-test tf-plan tf-apply ClusterId ClusterNodeId 

tf-destroy:
	${TERRAFORM} plan --destroy -out=tfplan -input=false 

tf-clean: tf-test tf-destroy tf-apply

tf-status:
	${TERRAFORM} show

tf-reboot:
	aws elasticache reboot-cache-cluster --cache-cluster-id ${CLUSTER_ID} --cache-node-ids-to-reboot ${CLUSTER_NODE_ID} 

setupTfstate:
	${TERRAFORM} remote config \
		-backend=S3 \
		-backend-config="${TFSTATE_BUCKET}" \
		-backend-config="region=ap-northeast-1" \
		-backend-config="key=${CURRENT_DIR}.tfstate" \
		-backend-config="access_key=${AWS_ACCESS_KEY_ID}" \
		-backend-config="secret_key=${AWS_SECRET_ACCESS_KEY}"

ServerlessRedisConfig:
	${TERRAFORM} state show aws_elasticache_cluster.redis | grep "cache_nodes.0.address" | awk '{print "host: " $$3}' > ../serverlessRedisConfig.yml
	${TERRAFORM} state show aws_elasticache_cluster.redis | grep "cache_nodes.0.port" | awk '{print "port: " $$3}' >> ../serverlessRedisConfig.yml

ServerlessVpcConfig:
	${TERRAFORM} state show aws_vpc.VPC | grep "default_security_group_id" | awk '{print "sg: " $$3}' > ../serverlessVpcConfig.yml
	${TERRAFORM} state show aws_elasticache_subnet_group.SubnetGroup | grep " subnet-" | awk '{print "subnetId: "$$3}' >> ../serverlessVpcConfig.yml

ClusterId:
	${TERRAFORM} state show aws_elasticache_cluster.redis | grep "cluster_id" | awk '{print $$3}' > ClusterId.txt

ClusterNodeId:
	${TERRAFORM} state show aws_elasticache_cluster.redis | grep "cache_nodes.0.id" | awk '{print $$3}' > ClusterNodeId.txt

#####################################################################################
