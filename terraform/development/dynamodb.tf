resource "aws_dynamodb_table" "assetinformationapi_dynamodb_table" {
    name                  = "Assets"
    billing_mode          = "PROVISIONED"
    read_capacity         = 10
    write_capacity        = 10
    hash_key              = "id"

    attribute {
        name              = "id"
        type              = "S"
    }

    attribute {
        name              = "parentAssetIds"
        type              = "S"
    }

    attribute {
        name              = "rootAsset"
        type              = "S"
    }

    global_secondary_index {
        name              = "AssetParentsAndChilds"
        hash_key          = "rootAsset"
        range_key         = "parentAssetIds"
        write_capacity    = 10
        read_capacity     = 10
        projection_type   = "ALL"
    }

    tags = {
        Name              = "asset-information-api-${var.environment_name}"
        Environment       = var.environment_name
        terraform-managed = true
        project_name      = var.project_name
        backup_policy     = "Dev"
    }

    point_in_time_recovery {
        enabled           = true
    }
}
