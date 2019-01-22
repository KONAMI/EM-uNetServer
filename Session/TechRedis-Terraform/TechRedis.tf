##[ VPC ]##############################################################

## VPC
resource "aws_vpc" "VPC" {
  cidr_block                       = "${var.vpc_cidr_block}"
  assign_generated_ipv6_cidr_block = true
  instance_tenancy                 = "default"
  tags {
    Name = "${var.title}-${var.env}"
  }
}

## Subnet
resource "aws_subnet" "Subnet" {
  vpc_id            = "${aws_vpc.VPC.id}"
  cidr_block        = "${cidrsubnet(aws_vpc.VPC.cidr_block, 8, 0)}"
  ipv6_cidr_block   = "${cidrsubnet(aws_vpc.VPC.ipv6_cidr_block, 8, 0)}"  
  availability_zone = "${var.aws_az}"
  tags {
    Name = "${var.title}-${var.env}"
  }
}

## SubnetGroup for ElastiCache
resource "aws_elasticache_subnet_group" "SubnetGroup" {
  name       = "${var.title}-subnet-group-${var.env}"
  subnet_ids = ["${aws_subnet.Subnet.id}"]
}

## gateway
resource "aws_internet_gateway" "IGW" {
  vpc_id = "${aws_vpc.VPC.id}"
  tags {
    Name = "${var.title}-${var.env}"
  }
}

## security group
resource "aws_security_group" "Security" {
  name     = "${var.title}-Security-${var.env}"
  vpc_id   = "${aws_vpc.VPC.id}"
  ingress {
    from_port = 6379
    to_port   = 6379
    protocol  = "tcp"
#    self = true
    cidr_blocks = ["0.0.0.0/0"]
  }
#  ingress {
#    from_port = 22
#    to_port = 22
#    protocol = "tcp"
#    cidr_blocks = ["0.0.0.0/0"]
#  }
  egress {
    from_port = 0
    to_port = 0
    protocol = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port = 0
    to_port = 0
    protocol = "-1"
    ipv6_cidr_blocks = ["::/0"]
  }
  tags {
    Name = "${var.title}-${var.env}"
  }
}

##[ ElastiCache ]##########################################################

resource "aws_elasticache_cluster" "redis" {
  cluster_id           = "${var.title}-${var.env}"
  engine               = "redis"
  engine_version       = "5.0.0"
  node_type            = "cache.t2.micro"
  port                 = 6379
  num_cache_nodes      = 1
  parameter_group_name = "default.redis5.0"
  security_group_ids   = ["${aws_security_group.Security.id}"]
  subnet_group_name = "${aws_elasticache_subnet_group.SubnetGroup.name}"
  tags {
    Name = "${var.title}-${var.env}"
  }
}

################################################################################
