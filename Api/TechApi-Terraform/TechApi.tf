##[ S3 定義 ]##############################################################################

resource "aws_s3_bucket" "project-storage" {
  bucket = "${var.title}-data-${var.env}"
  acl    = "private"
}

###########################################################################################
