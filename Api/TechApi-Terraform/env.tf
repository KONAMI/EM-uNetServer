variable "aws_access_key" {}
variable "aws_secret_key" {}
variable "aws_region" { default = "ap-northeast-1" }
variable "tfstate_bucket" {}
variable "tfstate_object" {}

variable "title"        {}
variable "env"          {}


provider "aws" {
  access_key = "${var.aws_access_key}"
  secret_key = "${var.aws_secret_key}"
  region     = "${var.aws_region}"
}

terraform {
  backend "s3" {}
}

data "terraform_remote_state" "state" {
  backend = "s3"
  config {
    bucket = "${var.tfstate_bucket}"
    key    = "${var.tfstate_object}"
    region = "${var.aws_region}"
  }
}
