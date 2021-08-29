provider "aws" {
  access_key = var.AWS_ACCESS_KEY
  secret_key = var.AWS_SECRET_KEY
  region     = var.AWS_REGION
}

terraform {
  backend "s3" {
    bucket = "tz-bwf-terraform-backend"
    key    = "tz.bwf.tfstate"
    region = "us-east-2"
  }
}

resource "aws_iam_role" "iam-tz-bwf-api-role" {
  name = "iam-tz-${terraform.workspace}-bwf-api-role"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF

}

resource "aws_iam_role_policy" "iam-tz-bwf-api-policy" {
  name = "iam-tz-${terraform.workspace}-bwf-api-policy"
  role = aws_iam_role.iam-tz-bwf-api-role.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": [
        "arn:aws:logs:*:*:*"
        "arn:aws:dynamodb:*:*:table/${var.BWF_WORDS_TABLE_NAME}",
        "arn:aws:dynamodb:*:*:table/${var.BWF_WORDS_TABLE_NAME}",
        "arn:aws:dynamodb:*:*:table/${var.BWF_WORDS_TABLE_NAME}/index/*",
        "arn:aws:dynamodb:*:*:table/${var.BWF_WORDS_TABLE_NAME}/index/*",
      ],
      "Effect": "Allow"
    }
  ]
}
EOF

}

data "aws_s3_bucket_object" "tz-bwf-api" {
bucket = "tz-bwf-api"
key    = "IPLookup.API.Host.Lambda.zip"
}

resource "aws_lambda_function" "tz-bwf-api" {
function_name                  = "tz-${terraform.workspace}-bwf-api"
s3_bucket                      = data.aws_s3_bucket_object.tz-bwf-api.bucket
s3_key                         = data.aws_s3_bucket_object.tz-bwf-api.key
role                           = aws_iam_role.iam-tz-bwf-api-role.arn
source_code_hash               = data.aws_s3_bucket_object.tz-bwf-api.etag
reserved_concurrent_executions = var.RESERVED_CAPACITY
memory_size                    = var.MEM_SIZE
timeout                        = var.TIMEOUT
handler                        = "BWF.API.Host.Lambda::BWF.API.Host.Lambda.LambdaEntryPoint::FunctionHandlerAsync"
runtime                        = var.RUNTIME

}

resource "aws_api_gateway_resource" "proxy" {
rest_api_id = aws_api_gateway_rest_api.tz-bwf-api.id
parent_id   = aws_api_gateway_rest_api.tz-bwf-api.root_resource_id
path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "proxy" {
rest_api_id   = aws_api_gateway_rest_api.tz-bwf-api.id
resource_id   = aws_api_gateway_resource.proxy.id
http_method   = "ANY"
authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda" {
rest_api_id = aws_api_gateway_rest_api.tz-bwf-api.id
resource_id = aws_api_gateway_method.proxy.resource_id
http_method = aws_api_gateway_method.proxy.http_method

integration_http_method = "POST"
type                    = "AWS_PROXY"
uri                     = aws_lambda_function.tz-bwf-api.invoke_arn
}

resource "aws_api_gateway_rest_api" "tz-bwf-api" {
name = "tz-${terraform.workspace}-bwf-api"
}

resource "aws_api_gateway_method" "proxy_root" {
rest_api_id   = aws_api_gateway_rest_api.tz-bwf-api.id
resource_id   = aws_api_gateway_rest_api.tz-bwf-api.root_resource_id
http_method   = "ANY"
authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_root" {
rest_api_id = aws_api_gateway_rest_api.tz-bwf-api.id
resource_id = aws_api_gateway_method.proxy_root.resource_id
http_method = aws_api_gateway_method.proxy_root.http_method

integration_http_method = "POST"
type                    = "AWS_PROXY"
uri                     = aws_lambda_function.tz-bwf-api.invoke_arn
}

resource "aws_api_gateway_deployment" "tz-bwf-api-gateway" {
depends_on = [
aws_api_gateway_integration.lambda,
aws_api_gateway_integration.lambda_root,
]

rest_api_id = aws_api_gateway_rest_api.tz-bwf-api.id
stage_name  = var.ENVIRONMENT
}

resource "aws_lambda_permission" "apigw" {
statement_id  = "AllowAPIGatewayInvoke"
action        = "lambda:InvokeFunction"
function_name = aws_lambda_function.tz-bwf-api.arn
principal     = "apigateway.amazonaws.com"
# The /*/* portion grants access from any method on any resource
# within the API Gateway "REST API".
source_arn = "${aws_api_gateway_deployment.tz-bwf-api-gateway.execution_arn}/*/*"
}

resource "aws_dynamodb_table" "bwf-dynamodb-table" {
  name           = var.BWF_WORDS_TABLE_NAME
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "Id"
  range_key      = "SK"
  attribute {
    name = "Id"
    type = "S"
  }
  attribute {
    name = "SK"
    type = "S"
  }
  attribute {
    name = "SortData"
    type = "S"
  }
  global_secondary_index {
    name               = "SK_SortData_index"
    hash_key           = "SK"
    range_key          = "SortData"
    projection_type    = "ALL"
  }
}


output "base_url" {
value = aws_api_gateway_deployment.tz-bwf-api-gateway.invoke_url
}