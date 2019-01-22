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

tf-all: tf-test tf-plan tf-apply 

tf-destroy:
	${TERRAFORM} plan --destroy -out=tfplan -input=false 

tf-clean: tf-test tf-destroy tf-apply

tf-status:
	${TERRAFORM} show

setupTfstate:
	${TERRAFORM} remote config \
		-backend=S3 \
		-backend-config="${TFSTATE_BUCKET}" \
		-backend-config="region=ap-northeast-1" \
		-backend-config="key=${CURRENT_DIR}.tfstate" \
		-backend-config="access_key=${AWS_ACCESS_KEY_ID}" \
		-backend-config="secret_key=${AWS_SECRET_ACCESS_KEY}"

#####################################################################################
