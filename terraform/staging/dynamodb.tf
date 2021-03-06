resource "aws_dynamodb_table" "assetinformationapi_dynamodb_table" {
  name           = "Assets"
  billing_mode   = "PROVISIONED"
  read_capacity  = 10
  write_capacity = 10
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }

  attribute {
    name = "parentAssetIds"
    type = "S"
  }

  attribute {
    name = "rootAsset"
    type = "S"
  }

  attribute {
    name = "assetId"
    type = "S"
  }

  global_secondary_index {
    name            = "AssetParentsAndChilds"
    hash_key        = "rootAsset"
    range_key       = "parentAssetIds"
    write_capacity  = 10
    read_capacity   = 10
    projection_type = "ALL"
  }

  global_secondary_index {
    name            = "AssetId"
    hash_key        = "assetId"
    write_capacity  = 10
    read_capacity   = 10
    projection_type = "ALL"
  }

  timeouts {
    create = "60m"
    update = "60m"
  }

  tags = merge(
    local.default_tags,
    { BackupPolicy = "Stg" }
  )

  point_in_time_recovery {
    enabled = true
  }
}
